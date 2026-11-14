using System.Globalization;
using System.Text;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Themes.Tests;

/// <summary>
/// Dumps the complete compiled result of a theme snapshot (every global slot and every registered control's Own
/// slots) as sorted text lines and diffs two such dumps, so a theme test can assert that two ways of reaching a theme
/// compile to the same values slot by slot.
/// </summary>
/// <remarks>
/// Line formats: <c>G &lt;TokenName&gt; = &lt;value&gt;</c> for global slots and
/// <c>C &lt;Catalog&gt;:&lt;ControlId&gt; &lt;TokenName&gt; = &lt;value&gt;</c> for control Own slots. Values are
/// formatted with the token descriptor's <c>Format</c> (culture-invariant). All global lines come first, then all
/// control lines; within each group lines are ordered by their key (the text before <c> = </c>) ordinally.
/// Lazily compiled controls are forced while dumping. Gradient brushes are the one exception to <c>Format</c>: it
/// yields only their type name, so they are written with their geometry and stops instead.
/// </remarks>
internal static class ThemeSnapshotDump
{
    private const string Separator = " = ";

    internal static string Dump(ThemeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var registry = snapshot.Registry;

        var globals = new List<string>(registry.GlobalTokens.Count);
        foreach (var token in registry.GlobalTokens)
        {
            globals.Add($"G {token.Name}{Separator}{FormatValue(token, snapshot.GlobalTokenValues.GetValue(token.Slot))}");
        }

        var controls = new List<string>();
        foreach (var control in registry.Controls)
        {
            if (control.OwnTokens.Count == 0)
            {
                continue;
            }
            // Indexing the lazy control table compiles the control if it has not been compiled yet.
            var values = snapshot.Controls[control.Slot].ControlTokenValues;
            var prefix = $"C {control.Identity.Catalog}:{control.Identity.Id} ";
            foreach (var token in control.OwnTokens)
            {
                controls.Add($"{prefix}{token.Name}{Separator}{FormatValue(token, values.GetValue(token.Slot))}");
            }
        }

        globals.Sort(CompareByKey);
        controls.Sort(CompareByKey);
        var builder = new StringBuilder();
        foreach (var line in globals.Concat(controls))
        {
            builder.Append(line).Append('\n');
        }
        return builder.ToString();
    }

    /// <summary>
    /// The differences between two dumps, ordered by key: <c>changed &lt;key&gt;: &lt;old&gt; -&gt; &lt;new&gt;</c>,
    /// <c>added &lt;key&gt; = &lt;value&gt;</c> or <c>removed &lt;key&gt; = &lt;value&gt;</c>. A malformed line or a
    /// duplicate key in either dump throws.
    /// </summary>
    internal static IReadOnlyList<string> Diff(string expected, string actual)
    {
        var before = Parse(expected);
        var after = Parse(actual);
        var differences = new List<string>();
        foreach (var key in before.Keys.Union(after.Keys).OrderBy(key => key, StringComparer.Ordinal))
        {
            var hadBefore = before.TryGetValue(key, out var oldValue);
            var hasAfter = after.TryGetValue(key, out var newValue);
            if (hadBefore && hasAfter)
            {
                if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
                {
                    differences.Add($"changed {key}: {oldValue} -> {newValue}");
                }
            }
            else if (hadBefore)
            {
                differences.Add($"removed {key}{Separator}{oldValue}");
            }
            else
            {
                differences.Add($"added {key}{Separator}{newValue}");
            }
        }
        return differences;
    }

    /// <summary>The slot key of one <see cref="Diff"/> line, e.g. <c>C AtomUI:Select PopupContentPadding</c>.</summary>
    internal static string SlotOf(string difference)
    {
        // "changed G X: a -> b", "added C A:B X = v", "removed G X = v"
        var body = difference[(difference.IndexOf(' ') + 1)..];
        var end = body.IndexOf(difference.StartsWith("changed", StringComparison.Ordinal) ? ": " : Separator,
            StringComparison.Ordinal);
        return body[..end];
    }

    private static string FormatValue(TokenDescriptor token, object? value)
    {
        if (value is not IGradientBrush gradient)
        {
            return token.Format(value);
        }
        var builder = new StringBuilder(gradient.GetType().Name);
        builder.Append(CultureInfo.InvariantCulture, $"(opacity={gradient.Opacity:R}; spread={gradient.SpreadMethod}");
        if (gradient is ILinearGradientBrush linear)
        {
            builder.Append("; start=").Append(FormatPoint(linear.StartPoint))
                   .Append("; end=").Append(FormatPoint(linear.EndPoint));
        }
        if (gradient.Transform is { } transform)
        {
            var m = transform.Value;
            builder.Append(CultureInfo.InvariantCulture,
                $"; transform={m.M11:R},{m.M12:R},{m.M13:R},{m.M21:R},{m.M22:R},{m.M23:R},{m.M31:R},{m.M32:R},{m.M33:R}");
        }
        else
        {
            builder.Append("; transform=none");
        }
        builder.Append("; stops=");
        for (var i = 0; i < gradient.GradientStops.Count; i++)
        {
            var stop = gradient.GradientStops[i];
            builder.Append(i == 0 ? "" : " ")
                   .Append(CultureInfo.InvariantCulture, $"{stop.Color}@{stop.Offset:R}");
        }
        return builder.Append(')').ToString();
    }

    private static string FormatPoint(RelativePoint point) => string.Create(CultureInfo.InvariantCulture,
        $"{point.Point.X:R},{point.Point.Y:R}{(point.Unit == RelativeUnit.Relative ? "%" : "px")}");

    private static int CompareByKey(string left, string right) =>
        string.CompareOrdinal(KeyOf(left), KeyOf(right));

    private static string KeyOf(string line)
    {
        var index = line.IndexOf(Separator, StringComparison.Ordinal);
        return index < 0 ? line : line[..index];
    }

    private static Dictionary<string, string> Parse(string dump)
    {
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in dump.Split('\n'))
        {
            if (line.Length == 0)
            {
                continue;
            }
            var index = line.IndexOf(Separator, StringComparison.Ordinal);
            if (index < 0)
            {
                throw new FormatException($"Not a snapshot line: '{line}'.");
            }
            if (!entries.TryAdd(line[..index], line[(index + Separator.Length)..]))
            {
                throw new FormatException($"Duplicate snapshot key: '{line[..index]}'.");
            }
        }
        return entries;
    }
}

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
