using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace ShipcrackerWarcasket.Tests;

// Every VFEP warcasket base carries its own VEF ApparelExtension, and VEF merges duplicates at
// resolve time, keeping the highest priority and dropping fields its Merge does not copy (the
// armor def's header has the detail). So each of our defs carries exactly one entry at
// priority 1, and no patch from any load root adds a second entry: new fields go on that one.
// Reads the repo's XML from disk; no game involved.
public class ApparelExtensionPriorityTests
{
    private const string ApparelExtension = "VEF.Apparels.ApparelExtension";

    private static readonly string RepoRoot = FindRepoRoot();

    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "ShipcrackerWarcasket.sln")))
                return dir.FullName;
        }
        throw new InvalidOperationException("ShipcrackerWarcasket.sln not found above the test output folder");
    }

    // Version folders (1.6/, and any later one) hold every load root that carries XML, the
    // gated compat roots under <version>/Mods/ included.
    private static IEnumerable<(string path, XElement root)> ModXml(string rootTag) =>
        Directory.EnumerateDirectories(RepoRoot)
            .Where(d => Regex.IsMatch(Path.GetFileName(d), @"^\d+\.\d+$"))
            .SelectMany(d => Directory.EnumerateFiles(d, "*.xml", SearchOption.AllDirectories))
            .Select(path => (path, root: XDocument.Load(path).Root))
            .Where(f => f.root?.Name.LocalName == rootTag);

    private static bool IsApparelExtension(XElement e) =>
        e.Name.LocalName == "li" && (string)e.Attribute("Class") == ApparelExtension;

    private static string Relative(string path) => path.Substring(RepoRoot.Length + 1);

    [Fact]
    public void EveryDefEntry_HasPriorityOne_AndThereIsOnePerDef()
    {
        var defs = ModXml("Defs")
            .SelectMany(f => f.root.Elements().Select(def => (f.path, def)))
            .Select(x => (x.path, name: (string)x.def.Element("defName"),
                entries: x.def.Element("modExtensions")?.Elements().Where(IsApparelExtension).ToList()))
            .Where(x => x.entries is { Count: > 0 })
            .ToList();

        // The armor, shoulders and helmet at least; guards against the scan silently finding nothing.
        Assert.True(defs.Count >= 3, $"found ApparelExtension entries on only {defs.Count} defs");
        foreach (var (path, name, entries) in defs)
        {
            Assert.True(entries.Count == 1, $"{name} ({Relative(path)}) has {entries.Count} ApparelExtension entries");
            Assert.True((string)entries[0].Element("priority") == "1",
                $"{name} ({Relative(path)}) ApparelExtension priority is not 1");
        }
    }

    [Fact]
    public void NoPatch_AddsAnotherEntry()
    {
        var offenders = ModXml("Patch")
            .Where(f => f.root.Descendants().Any(IsApparelExtension))
            .Select(f => Relative(f.path))
            .ToList();

        Assert.True(offenders.Count == 0,
            "patches adding an ApparelExtension entry (append to the def's existing entry instead): "
            + string.Join(", ", offenders));
    }
}
