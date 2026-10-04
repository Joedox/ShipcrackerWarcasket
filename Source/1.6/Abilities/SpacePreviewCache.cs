using System.Collections.Generic;
using Verse;

namespace ShipcrackerWarcasket;

// What the space preview asks of the world: the two halves of the landing rule, and whether
// this frame's testing budget is spent. Ability_BreachJump answers from the wearer's live map.
internal interface ISpacePreviewTester
{
    bool Walkable(IntVec3 cell);

    // The range or sight half of the landing rule, for a cell already known to be walkable.
    bool CanReach(IntVec3 cell);

    bool OutOfBudget();
}

// Per-cell landing cache behind the Breach Jump's space preview, for one map at a time. Owns
// no map: the ability resets it on a map change and feeds it the map's change events.
//
// Each cell holds 0 when never tested, otherwise the generation it was tested in shifted over
// two flag bits (see Pack). Walkable is kept apart from landable so a path-cost event can tell
// a real walkability flip from filth landing in a cell that was already out of sight. Stale
// cells keep their answer until the sweep re-tests them; the generation bumps only between
// sweeps and only when the wearer moved or MarkDirty was called, so a static scene costs
// nothing. Never cleared on a wearer move, which would wipe the outline and regrow it every
// step.
internal sealed class SpacePreviewCache
{
    public const int Walkable = 1;
    public const int Landable = 2;

    private int[] state;
    private int sizeX;
    private int sizeZ;
    private IntVec3 origin;
    private int generation;
    private bool dirty;

    // Sweep cursor into the row-major enumeration of rect, carried across frames so a re-test
    // covers the whole view before another begins.
    private CellRect rect;
    private int cursor;

    public bool HasGrid => state != null;

    public bool Dirty => dirty;

    public static int Pack(int generation, bool walkable, bool landable) =>
        (generation << 2) | (walkable ? Walkable : 0) | (landable ? Landable : 0);

    public int StateAt(IntVec3 cell) => state[Index(cell)];

    // Forgets every cell and starts over on a map of the given size, with the wearer at origin.
    public void Reset(int mapSizeX, int mapSizeZ, IntVec3 origin)
    {
        var cellCount = mapSizeX * mapSizeZ;
        if (state == null || state.Length != cellCount)
            state = new int[cellCount];
        else
            System.Array.Clear(state, 0, cellCount);
        sizeX = mapSizeX;
        sizeZ = mapSizeZ;
        this.origin = origin;
        generation = 1;
        dirty = false;
        rect = default;
        cursor = 0;
    }

    // Schedules a re-test of every cell once the sweep in progress has covered the view.
    public void MarkDirty() => dirty = true;

    // The walkability cached for cell, or false when it was never tested.
    public bool TryGetCachedWalkable(IntVec3 cell, out bool walkable)
    {
        var s = state[Index(cell)];
        walkable = (s & Walkable) != 0;
        return s != 0;
    }

    // Advances the sweep through view, testing cells that are not of the current generation
    // until the tester's budget is spent; fresh cells are skipped for the cost of an array read.
    // A new generation starts only once the previous sweep has covered the whole view, so a
    // burst of map events coalesces into one re-test and no part of the view is starved. The
    // tester reads the wearer's live cell, so cells tested after a mid-sweep step already use
    // the new origin; the mismatch recorded here just schedules the pass that makes the rest
    // agree.
    public void Sweep(CellRect view, IntVec3 wearerCell, ISpacePreviewTester tester)
    {
        if (view != rect)
        {
            rect = view;
            cursor = 0;
        }
        var area = rect.Area;

        if (cursor >= area && (dirty || wearerCell != origin))
        {
            generation++;
            origin = wearerCell;
            dirty = false;
            cursor = 0;
        }

        var width = rect.Width;
        for (; cursor < area; cursor++)
        {
            var cell = new IntVec3(rect.minX + cursor % width, 0, rect.minZ + cursor / width);
            var i = Index(cell);
            if (state[i] >> 2 == generation)
                continue;
            var walkable = tester.Walkable(cell);
            state[i] = Pack(generation, walkable, walkable && tester.CanReach(cell));
            if (tester.OutOfBudget())
            {
                cursor++;
                break;
            }
        }
    }

    // Everything in view known to be landable, at whatever age, plus the never-tested cells
    // next to them: DrawFieldEdges skips edges that face a cell in ignoreBorderCells, so the
    // fill carves shadows into view rather than sweeping a visible frontier line up the map.
    public void Collect(CellRect view, List<IntVec3> landable, HashSet<IntVec3> untestedBorder)
    {
        landable.Clear();
        untestedBorder.Clear();
        foreach (var cell in view)
        {
            if ((state[Index(cell)] & Landable) == 0)
                continue;
            landable.Add(cell);
            for (var d = 0; d < 4; d++)
            {
                var n = cell + GenAdj.CardinalDirections[d];
                if (n.x >= 0 && n.z >= 0 && n.x < sizeX && n.z < sizeZ && state[Index(n)] == 0)
                    untestedBorder.Add(n);
            }
        }
    }

    // Row-major, the same layout as CellIndices.
    private int Index(IntVec3 cell) => cell.z * sizeX + cell.x;
}
