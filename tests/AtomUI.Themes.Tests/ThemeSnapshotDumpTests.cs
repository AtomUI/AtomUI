using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using Avalonia;
using Avalonia.Headless.XUnit;
using Shouldly;
using Xunit;

namespace AtomUI.Themes.Tests;

public class ThemeSnapshotDumpTests
{
    [AvaloniaFact]
    public async Task Dump_Writes_Every_Global_And_Own_Slot_Once()
    {
        // The equivalence tests compare dumps; a slot the dump left out would never be compared.
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        (await manager.ApplyThemeAsync(new ThemeRequest("PrimeNGAura", null, ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken)).Status.ShouldBe(ThemeTransitionStatus.Committed);
        var snapshot = manager.CurrentSnapshot!;

        var dump = ThemeSnapshotDump.Dump(snapshot);
        var lines = dump.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var registry = snapshot.Registry;
        lines.Count(line => line.StartsWith("G ", StringComparison.Ordinal)).ShouldBe(registry.GlobalTokens.Count);
        lines.Count(line => line.StartsWith("C ", StringComparison.Ordinal))
             .ShouldBe(registry.Controls.Sum(control => control.OwnTokens.Count));
        // Parsing rejects duplicate keys.
        ThemeSnapshotDump.Diff(dump, dump).ShouldBeEmpty();
    }

    [Fact]
    public void Diff_Of_Identical_Dumps_Is_Empty()
    {
        const string dump = "G ColorPrimary = #ff10b981\nC AtomUI:Button FontWeight = Medium\n";
        ThemeSnapshotDump.Diff(dump, dump).ShouldBeEmpty();
    }

    [Fact]
    public void Diff_Reports_Changed_Added_And_Removed_Lines_By_Token_Name()
    {
        const string expected = "G ColorPrimary = #ff10b981\nG ColorText = #ff334155\nC AtomUI:Button FontWeight = Medium\n";
        const string actual = "G ColorPrimary = #ff059669\nC AtomUI:Button FontWeight = Medium\nC AtomUI:Card BodyPadding = 20,20,20,20\n";
        var differences = ThemeSnapshotDump.Diff(expected, actual);
        differences.ShouldBe(new[]
        {
            "added C AtomUI:Card BodyPadding = 20,20,20,20",
            "changed G ColorPrimary: #ff10b981 -> #ff059669",
            "removed G ColorText = #ff334155"
        });
        differences.Select(ThemeSnapshotDump.SlotOf)
                   .ShouldBe(new[] { "C AtomUI:Card BodyPadding", "G ColorPrimary", "G ColorText" });
    }
}
