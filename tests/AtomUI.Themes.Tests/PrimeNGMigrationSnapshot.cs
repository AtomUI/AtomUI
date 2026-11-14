using System.Globalization;
using System.Text;
using System.Xml.Linq;
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
/// One-off tool for rebuilding the PrimeNG preset package on top of algorithms: it dumps the complete compiled
/// result of a theme snapshot (every global slot and every registered control's Own slots) as sorted text lines
/// and diffs two such dumps. The dumps live outside the repository, in the directory named by
/// <c>ATOMUI_PRIMENG_BASELINE_DIR</c>. This file is deleted once the migration is verified.
/// </summary>
/// <remarks>
/// Line formats: <c>G &lt;TokenName&gt; = &lt;value&gt;</c> for global slots and
/// <c>C &lt;Catalog&gt;:&lt;ControlId&gt; &lt;TokenName&gt; = &lt;value&gt;</c> for control Own slots. Values are
/// formatted with the token descriptor's <c>Format</c> (culture-invariant). All global lines come first, then all
/// control lines; within each group lines are ordered by their key (the text before <c> = </c>) ordinally.
/// Lazily compiled controls are forced while dumping. Gradient brushes are the one exception to <c>Format</c>: it
/// yields only their type name, so they are written with their geometry and stops instead.
/// </remarks>
internal static class PrimeNGMigrationSnapshot
{
    internal const string BaselineDirectoryVariable = "ATOMUI_PRIMENG_BASELINE_DIR";
    private const string Separator = " = ";

    internal static string BaselineDirectory
    {
        get
        {
            var directory = Environment.GetEnvironmentVariable(BaselineDirectoryVariable);
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new InvalidOperationException(
                    $"Set the environment variable {BaselineDirectoryVariable} to the directory that holds (or receives) " +
                    "the PrimeNG compile baseline dumps. The directory must be outside the repository.");
            }
            return directory;
        }
    }

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

    internal static IReadOnlyList<string> Diff(string baseline, string current)
    {
        var before = Parse(baseline);
        var after = Parse(current);
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

    /// <summary>
    /// The acceptance check of a migrated profile: every token the pre-change XML of <paramref name="profile"/> set
    /// explicitly (global and per control) must compile to its baseline value. For the tokens in
    /// <paramref name="xmlTextExceptions"/> the expected value is the XML text instead, and each of them must really
    /// differ from the baseline, so the list cannot grow stale. Returns the mismatches; a missing baseline key throws.
    /// </summary>
    internal static IReadOnlyList<string> CompareExplicitXmlTokens(
        ThemeSnapshot snapshot, string profile, IReadOnlyCollection<string> xmlTextExceptions, out int checkedCount)
    {
        var directory = BaselineDirectory;
        var baseline = Parse(File.ReadAllText(Path.Combine(directory, profile + ".txt")));
        var oldXml = XDocument.Load(Path.Combine(directory, "xml", profile + ".theme.xml")).Root!;
        XNamespace ns = "https://atomui.net/schemas/theme/v1";

        var current = Parse(Dump(snapshot));
        var registry = snapshot.Registry;
        var mismatches = new List<string>();
        checkedCount = 0;

        foreach (var token in oldXml.Element(ns + "Tokens")!.Elements(ns + "Token"))
        {
            var name = token.Attribute("Name")!.Value;
            var key = "G " + name;
            registry.TryGetGlobalToken(name, out var descriptor).ShouldBeTrue(name);
            string expected;
            if (xmlTextExceptions.Contains(name))
            {
                expected = descriptor!.Format(descriptor.Parse(token.Attribute("Value")!.Value));
                // The exception list must stay minimal: each entry really differs from the baseline.
                baseline[key].ShouldNotBe(expected, $"{key} no longer needs an exception");
            }
            else
            {
                expected = baseline[key];
            }
            checkedCount++;
            if (current[key] != expected)
            {
                mismatches.Add($"{key}: expected {expected}, got {current[key]}");
            }
        }

        foreach (var control in oldXml.Element(ns + "Controls")?.Elements(ns + "Control") ?? [])
        {
            var identity = new ControlTokenIdentity(control.Attribute("Catalog")!.Value, control.Attribute("Id")!.Value);
            registry.TryGetControl(identity, out var owner).ShouldBeTrue(identity.ToString());
            foreach (var token in control.Element(ns + "Tokens")!.Elements(ns + "Token"))
            {
                var name = token.Attribute("Name")!.Value;
                checkedCount++;
                if (owner!.OwnTokens.Any(own => own.Name == name))
                {
                    var key = $"C {identity.Catalog}:{identity.Id} {name}";
                    if (current[key] != baseline[key])
                    {
                        mismatches.Add($"{key}: expected {baseline[key]}, got {current[key]}");
                    }
                    continue;
                }

                // A control-scoped global override. The dump does not hold effective control globals; the old
                // compiler applied the override value verbatim, so the XML value is the baseline here.
                registry.TryGetGlobalToken(name, out var global).ShouldBeTrue(name);
                var expectedValue = global!.Format(global.Parse(token.Attribute("Value")!.Value));
                var actual = global.Format(snapshot.Controls[owner.Slot]
                                                   .GetEffectiveGlobalValue<object?>(snapshot.GlobalTokenValues, global.Slot));
                if (actual != expectedValue)
                {
                    mismatches.Add($"{identity} global {name}: expected {expectedValue}, got {actual}");
                }
            }
        }

        return mismatches;
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

public class PrimeNGMigrationSnapshotTests
{
    private static readonly string[] Profiles = new[] { "PrimeNGAura", "PrimeNGLara", "PrimeNGNora" }
        .SelectMany(id => new[] { id, id + ".Dark", id + ".Compact", id + ".DarkCompact" })
        .ToArray();

    [AvaloniaFact(Explicit = true)]
    public async Task Dump_Baseline_For_All_Twelve_Profiles()
    {
        var directory = PrimeNGMigrationSnapshot.BaselineDirectory;
        Directory.CreateDirectory(directory);
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        foreach (var profile in Profiles)
        {
            var result = await manager.ApplyThemeAsync(
                new ThemeRequest(profile, null, ThemeTransitionReason.UserRequest), TestContext.Current.CancellationToken);
            result.Status.ShouldBe(ThemeTransitionStatus.Committed);
            var snapshot = manager.CurrentSnapshot!;
            snapshot.ThemeId.ShouldBe(profile);

            var dump = PrimeNGMigrationSnapshot.Dump(snapshot);
            var lines = dump.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var registry = snapshot.Registry;
            lines.Count(line => line.StartsWith("G ", StringComparison.Ordinal)).ShouldBe(registry.GlobalTokens.Count);
            lines.Count(line => line.StartsWith("C ", StringComparison.Ordinal))
                 .ShouldBe(registry.Controls.Sum(control => control.OwnTokens.Count));
            // Parsing rejects duplicate keys.
            PrimeNGMigrationSnapshot.Diff(dump, dump).ShouldBeEmpty();

            await File.WriteAllTextAsync(Path.Combine(directory, profile + ".txt"), dump,
                new UTF8Encoding(false), TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public void Diff_Of_Identical_Dumps_Is_Empty()
    {
        const string dump = "G ColorPrimary = #ff10b981\nC AtomUI:Button FontWeight = Medium\n";
        PrimeNGMigrationSnapshot.Diff(dump, dump).ShouldBeEmpty();
    }

    [Fact]
    public void Diff_Reports_Changed_Added_And_Removed_Lines_By_Token_Name()
    {
        const string baseline = "G ColorPrimary = #ff10b981\nG ColorText = #ff334155\nC AtomUI:Button FontWeight = Medium\n";
        const string current = "G ColorPrimary = #ff059669\nC AtomUI:Button FontWeight = Medium\nC AtomUI:Card BodyPadding = 20,20,20,20\n";
        PrimeNGMigrationSnapshot.Diff(baseline, current).ShouldBe(new[]
        {
            "added C AtomUI:Card BodyPadding = 20,20,20,20",
            "changed G ColorPrimary: #ff10b981 -> #ff059669",
            "removed G ColorText = #ff334155"
        });
    }
}
