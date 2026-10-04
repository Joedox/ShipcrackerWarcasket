using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Xunit;

namespace ShipcrackerWarcasket.Tests;

// The space preview's per-cell cache: generation packing, the budgeted sweep that resumes
// across frames, re-tests deferred until a sweep completes, and the outline it hands to
// DrawFieldEdges. The world is a FakeTester, so no map is involved.
public class SpacePreviewCacheTests
{
    private const int MapSize = 8;
    private static readonly IntVec3 Wearer = new(0, 0, 0);

    private sealed class FakeTester : ISpacePreviewTester
    {
        public Func<IntVec3, bool> IsWalkable = _ => true;
        public Func<IntVec3, bool> IsReachable = _ => true;

        // Cells tested before OutOfBudget reports true, per frame.
        public int Budget = int.MaxValue;

        public readonly List<IntVec3> Tested = new();
        private int testedThisFrame;

        public bool Walkable(IntVec3 cell)
        {
            Tested.Add(cell);
            return IsWalkable(cell);
        }

        public bool CanReach(IntVec3 cell) => IsReachable(cell);

        public bool OutOfBudget() => ++testedThisFrame >= Budget;

        public void NewFrame()
        {
            Tested.Clear();
            testedThisFrame = 0;
        }
    }

    private static SpacePreviewCache NewCache()
    {
        var cache = new SpacePreviewCache();
        cache.Reset(MapSize, MapSize, Wearer);
        return cache;
    }

    // One frame: what DrawSpaceValidCells does per call, minus the drawing.
    private static List<IntVec3> Frame(SpacePreviewCache cache, CellRect view, FakeTester tester, IntVec3? wearer = null)
    {
        tester.NewFrame();
        cache.Sweep(view, wearer ?? Wearer, tester);
        return tester.Tested.ToList();
    }

    private static (List<IntVec3> landable, HashSet<IntVec3> border) Collect(SpacePreviewCache cache, CellRect view)
    {
        var landable = new List<IntVec3>();
        var border = new HashSet<IntVec3>();
        cache.Collect(view, landable, border);
        return (landable, border);
    }

    private static readonly CellRect View3x3 = new(0, 0, 3, 3);

    [Fact]
    public void Pack_KeepsGenerationAboveTheFlags()
    {
        Assert.Equal((5 << 2) | SpacePreviewCache.Walkable, SpacePreviewCache.Pack(5, true, false));
        Assert.Equal((1 << 2) | SpacePreviewCache.Walkable | SpacePreviewCache.Landable, SpacePreviewCache.Pack(1, true, true));
        // A tested, unwalkable cell must still read as tested.
        Assert.NotEqual(0, SpacePreviewCache.Pack(1, false, false));
        Assert.Equal(7, SpacePreviewCache.Pack(7, false, false) >> 2);
    }

    [Fact]
    public void FirstSweep_TestsEveryCellInViewOnce()
    {
        var cache = NewCache();
        var tester = new FakeTester();

        var tested = Frame(cache, View3x3, tester);

        Assert.Equal(View3x3.Cells.OrderBy(c => c.z).ThenBy(c => c.x), tested);
    }

    [Fact]
    public void Landable_NeedsBothWalkableAndReachable_AndReachIsSkippedForUnwalkableCells()
    {
        var cache = NewCache();
        var reachAsked = new List<IntVec3>();
        var tester = new FakeTester
        {
            IsWalkable = c => c.x != 0,
            IsReachable = c => { reachAsked.Add(c); return c.z != 0; },
        };

        Frame(cache, View3x3, tester);
        var (landable, _) = Collect(cache, View3x3);

        Assert.DoesNotContain(reachAsked, c => c.x == 0);
        Assert.Equal(new HashSet<IntVec3> { new(1, 0, 1), new(2, 0, 1), new(1, 0, 2), new(2, 0, 2) },
            landable.ToHashSet());
    }

    [Fact]
    public void CompletedSweep_StaticScene_TestsNothing()
    {
        var cache = NewCache();
        var tester = new FakeTester();
        Frame(cache, View3x3, tester);

        Assert.Empty(Frame(cache, View3x3, tester));
        Assert.Empty(Frame(cache, View3x3, tester));
    }

    [Fact]
    public void BudgetedSweep_ResumesWhereItStopped()
    {
        var cache = NewCache();
        var tester = new FakeTester { Budget = 4 };

        var first = Frame(cache, View3x3, tester);
        var second = Frame(cache, View3x3, tester);
        var third = Frame(cache, View3x3, tester);
        var fourth = Frame(cache, View3x3, tester);

        Assert.Equal(4, first.Count);
        Assert.Equal(4, second.Count);
        Assert.Single(third);
        Assert.Empty(fourth);
        Assert.Equal(View3x3.Cells.ToHashSet(), first.Concat(second).Concat(third).ToHashSet());
    }

    [Fact]
    public void WearerMove_MidSweep_FinishesTheSweepBeforeRetesting()
    {
        var cache = NewCache();
        var tester = new FakeTester { Budget = 4 };
        var moved = new IntVec3(1, 0, 0);

        Frame(cache, View3x3, tester);
        var afterMove = Frame(cache, View3x3, tester, moved);
        var tail = Frame(cache, View3x3, tester, moved);
        var retest = Frame(cache, View3x3, tester, moved);

        // The sweep in progress carries on from its cursor rather than restarting...
        Assert.Equal(4, afterMove.Count);
        Assert.Single(tail);
        // ...and only then does a new generation re-test from the top of the view.
        Assert.Equal(new IntVec3(0, 0, 0), retest.First());
        Assert.Equal(4, retest.Count);
    }

    [Fact]
    public void WearerMove_AfterSweep_RetestsTheWholeViewOnce()
    {
        var cache = NewCache();
        var tester = new FakeTester();
        var moved = new IntVec3(2, 0, 2);
        Frame(cache, View3x3, tester);

        Assert.Equal(9, Frame(cache, View3x3, tester, moved).Count);
        Assert.Empty(Frame(cache, View3x3, tester, moved));
    }

    [Fact]
    public void MarkDirty_Repeatedly_CoalescesIntoOneRetest()
    {
        var cache = NewCache();
        var tester = new FakeTester { Budget = 5 };

        Frame(cache, View3x3, tester);
        cache.MarkDirty();
        // The sweep in progress finishes first: the last four cells, not a restart.
        Assert.Equal(4, Frame(cache, View3x3, tester).Count);
        cache.MarkDirty();
        cache.MarkDirty();

        var retest = Frame(cache, View3x3, tester).Concat(Frame(cache, View3x3, tester)).ToList();

        Assert.Equal(View3x3.Cells.ToHashSet(), retest.ToHashSet());
        Assert.Equal(9, retest.Count);
        Assert.Empty(Frame(cache, View3x3, tester));
        Assert.False(cache.Dirty);
    }

    [Fact]
    public void ViewChange_TestsOnlyCellsNotAlreadyCurrent()
    {
        var cache = NewCache();
        var tester = new FakeTester();
        Frame(cache, View3x3, tester);

        var panned = new CellRect(1, 1, 3, 3);
        var tested = Frame(cache, panned, tester);

        Assert.Equal(panned.Cells.Except(View3x3.Cells).ToHashSet(), tested.ToHashSet());
        Assert.Equal(5, tested.Count);
    }

    [Fact]
    public void StaleAnswers_KeepDrawingUntilRetested()
    {
        var cache = NewCache();
        var tester = new FakeTester();
        Frame(cache, View3x3, tester);

        tester.IsReachable = _ => false;
        cache.MarkDirty();
        Assert.Equal(9, Collect(cache, View3x3).landable.Count);

        Frame(cache, View3x3, tester);
        Assert.Empty(Collect(cache, View3x3).landable);
    }

    [Fact]
    public void TryGetCachedWalkable_DistinguishesUntestedFromUnwalkable()
    {
        var cache = NewCache();
        var tester = new FakeTester { IsWalkable = c => c.x != 1 };
        Frame(cache, new CellRect(0, 0, 2, 1), tester);

        Assert.True(cache.TryGetCachedWalkable(new IntVec3(0, 0, 0), out var walkable));
        Assert.True(walkable);
        Assert.True(cache.TryGetCachedWalkable(new IntVec3(1, 0, 0), out walkable));
        Assert.False(walkable);
        Assert.False(cache.TryGetCachedWalkable(new IntVec3(5, 0, 5), out _));
    }

    [Fact]
    public void Collect_BordersOnlyNeverTestedNeighborsInsideTheMap()
    {
        var cache = NewCache();
        var tester = new FakeTester();
        var corner = new CellRect(0, 0, 2, 2);
        // A tested, unlandable cell beside the corner view.
        Frame(cache, new CellRect(2, 0, 1, 1), new FakeTester { IsWalkable = _ => false });
        Frame(cache, corner, tester);

        var (landable, border) = Collect(cache, corner);

        Assert.Equal(corner.Cells.ToHashSet(), landable.ToHashSet());
        // (2,0) is tested, so its edge draws; nothing off the map's low edges is listed.
        Assert.Equal(new HashSet<IntVec3> { new(2, 0, 1), new(0, 0, 2), new(1, 0, 2) }, border);
    }

    [Fact]
    public void Collect_StaysInsideTheMapAtTheHighEdge()
    {
        var cache = NewCache();
        var tester = new FakeTester();
        var corner = new CellRect(MapSize - 1, MapSize - 1, 1, 1);
        Frame(cache, corner, tester);

        var (_, border) = Collect(cache, corner);

        Assert.Equal(new HashSet<IntVec3> { new(MapSize - 2, 0, MapSize - 1), new(MapSize - 1, 0, MapSize - 2) }, border);
    }

    [Fact]
    public void Reset_ForgetsEveryCell()
    {
        var cache = NewCache();
        var tester = new FakeTester();
        Frame(cache, View3x3, tester);

        cache.Reset(MapSize, MapSize, Wearer);

        Assert.False(cache.TryGetCachedWalkable(new IntVec3(1, 0, 1), out _));
        Assert.Equal(9, Frame(cache, View3x3, tester).Count);
    }

    [Fact]
    public void Reset_ToAnotherMapSize_IndexesByTheNewWidth()
    {
        var cache = NewCache();
        cache.Reset(4, 2, Wearer);
        var tester = new FakeTester { IsWalkable = c => c == new IntVec3(3, 0, 1) };

        Frame(cache, new CellRect(0, 0, 4, 2), tester);

        Assert.Equal(SpacePreviewCache.Pack(1, true, true), cache.StateAt(new IntVec3(3, 0, 1)));
        Assert.Equal(SpacePreviewCache.Pack(1, false, false), cache.StateAt(new IntVec3(0, 0, 1)));
    }
}
