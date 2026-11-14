using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Themes.Tests;

/// <summary>
/// The four Aura profiles after they moved onto the <c>PrimeNG:Aura</c> / <c>PrimeNG:Aura.Dark</c> algorithms
/// (spec 4.8): the dark profile is the light one with the dark chain, the compact profile is the base one with
/// <c>AtomUI:Compact</c> appended (except the slots the README lists as not equivalent), and the XML no longer
/// spells out what the rules produce.
/// </summary>
public class PrimeNGAuraMigrationTests
{
    private static readonly XNamespace ThemeNamespace = "https://atomui.net/schemas/theme/v1";
    private static readonly ThemeAlgorithm Aura = new("PrimeNG", "Aura");
    private static readonly ThemeAlgorithm AuraDark = new("PrimeNG", "Aura.Dark");

    private static readonly string[] AuraProfiles =
        ["PrimeNGAura", "PrimeNGAura.Dark", "PrimeNGAura.Compact", "PrimeNGAura.DarkCompact"];

    /// <summary>
    /// The README's "不等价项": slots where <c>PrimeNGAura</c> + <c>AtomUI:Compact</c> differs from
    /// <c>PrimeNGAura.Compact</c>, as snapshot keys. Keep in sync with the README.
    /// </summary>
    internal static readonly string[] CompactNonEquivalentSlots = [];

    /// <summary>
    /// Explicit tokens of the old dark XML that never compiled to the value the XML wrote: the old chain
    /// <c>[AtomUI:Dark]</c> re-derived these Seed tokens in its Map stage, so the baseline holds <c>#2FB685</c> and
    /// <c>#D98F8F</c>. The algorithm reaches the author's values (<c>#34D399</c>, <c>#FCA5A5</c>) from the shared
    /// 500 seeds, so for these slots the expected value is the XML text instead of the baseline (Task 18 ruling).
    /// </summary>
    private static readonly (string Profile, string Token)[] XmlTextExceptions =
    [
        ("PrimeNGAura.Dark", "ColorPrimary"),
        ("PrimeNGAura.Dark", "ColorError"),
        ("PrimeNGAura.DarkCompact", "ColorPrimary"),
        ("PrimeNGAura.DarkCompact", "ColorError")
    ];

    [AvaloniaTheory(Explicit = true)]
    [InlineData("PrimeNGAura")]
    [InlineData("PrimeNGAura.Dark")]
    [InlineData("PrimeNGAura.Compact")]
    [InlineData("PrimeNGAura.DarkCompact")]
    public async Task Tokens_The_Old_Xml_Set_Explicitly_Keep_Their_Values(string profile)
    {
        // Temporary migration check (removed with the snapshot tool in Task 21). Reads the pre-change XML copy and
        // the baseline dump from ATOMUI_PRIMENG_BASELINE_DIR.
        var directory = PrimeNGMigrationSnapshot.BaselineDirectory;
        var baseline = ParseDump(await File.ReadAllTextAsync(Path.Combine(directory, profile + ".txt"),
            TestContext.Current.CancellationToken));
        var oldXml = XDocument.Load(Path.Combine(directory, "xml", profile + ".theme.xml")).Root!;

        var snapshot = await Apply(new ThemeRequest(profile, null, ThemeTransitionReason.UserRequest));
        var current = ParseDump(PrimeNGMigrationSnapshot.Dump(snapshot));
        var registry = snapshot.Registry;
        var mismatches = new List<string>();
        var checkedCount = 0;

        foreach (var token in oldXml.Element(ThemeNamespace + "Tokens")!.Elements(ThemeNamespace + "Token"))
        {
            var name = token.Attribute("Name")!.Value;
            var key = "G " + name;
            registry.TryGetGlobalToken(name, out var descriptor).ShouldBeTrue(name);
            string expected;
            if (XmlTextExceptions.Contains((profile, name)))
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

        foreach (var control in oldXml.Element(ThemeNamespace + "Controls")?.Elements(ThemeNamespace + "Control") ?? [])
        {
            var identity = new ControlTokenIdentity(control.Attribute("Catalog")!.Value, control.Attribute("Id")!.Value);
            registry.TryGetControl(identity, out var owner).ShouldBeTrue(identity.ToString());
            foreach (var token in control.Element(ThemeNamespace + "Tokens")!.Elements(ThemeNamespace + "Token"))
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
                var expected = global!.Format(global.Parse(token.Attribute("Value")!.Value));
                var actual = global.Format(snapshot.Controls[owner.Slot]
                                                   .GetEffectiveGlobalValue<object?>(snapshot.GlobalTokenValues, global.Slot));
                if (actual != expected)
                {
                    mismatches.Add($"{identity} global {name}: expected {expected}, got {actual}");
                }
            }
        }

        checkedCount.ShouldBeGreaterThan(40);
        mismatches.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task Swapping_The_Chain_To_Aura_Dark_Equals_The_Dark_Profile()
    {
        var swapped = await Dump("PrimeNGAura", AuraDark);
        var dark = await Dump("PrimeNGAura.Dark");
        PrimeNGMigrationSnapshot.Diff(dark, swapped).ShouldBeEmpty();

        var swappedCompact = await Dump("PrimeNGAura.Compact", AuraDark, ThemeAlgorithm.Compact);
        var darkCompact = await Dump("PrimeNGAura.DarkCompact");
        PrimeNGMigrationSnapshot.Diff(darkCompact, swappedCompact).ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task Appending_Compact_Equals_The_Compact_Profile_Except_Listed_Slots()
    {
        foreach (var (baseId, chain) in new[]
                 {
                     ("PrimeNGAura", new[] { Aura, ThemeAlgorithm.Compact }),
                     ("PrimeNGAura.Dark", new[] { AuraDark, ThemeAlgorithm.Compact })
                 })
        {
            var appended = await Dump(baseId, chain);
            var compact = await Dump(baseId.EndsWith(".Dark", StringComparison.Ordinal)
                ? "PrimeNGAura.DarkCompact"
                : "PrimeNGAura.Compact");
            var slots = PrimeNGMigrationSnapshot.Diff(compact, appended)
                                                .Select(SlotOf)
                                                .ToArray();
            slots.ShouldBe(CompactNonEquivalentSlots, ignoreOrder: true, baseId);
        }
    }

    [AvaloniaFact]
    public async Task Primary_Seed_Change_Moves_Focus_Ring_And_Selected_Background()
    {
        var emerald = await Apply(new ThemeRequest("PrimeNGAura", null, ThemeTransitionReason.UserRequest));
        var emeraldSelected = Global<Color>(emerald, "ControlItemBgActive");

        var config = new ThemeConfigBuilder().WithToken("ColorPrimary", "#3B82F6").Build();
        var blue = await Apply(new ThemeRequest("PrimeNGAura", config, ThemeTransitionReason.UserRequest));
        Global<Color>(blue, "ColorPrimary").ShouldBe(Color.Parse("#3B82F6"));
        Global<Color>(blue, "ColorFocusBorder").ShouldBe(Global<Color>(blue, "ColorPrimary"));
        Global<Color>(blue, "ControlItemBgActive").ShouldNotBe(emeraldSelected);
        Global<Color>(blue, "ControlItemBgActive").ShouldBe(Color.Parse("#EFF6FF"));
    }

    [Fact]
    public void Aura_Xml_No_Longer_Contains_Rule_Tokens()
    {
        string[] ruleTokens =
        [
            "ColorPrimaryHover", "ColorPrimaryActive", "ColorPrimaryBg", "ColorPrimaryText", "ColorTextLightSolid",
            "ColorText", "ColorTextSecondary", "ColorTextDescription", "ColorTextPlaceholder", "ColorBgContainer",
            "ColorBgElevated", "ColorBgBase", "ColorTextBase", "ColorFillTertiary", "ColorBgContainerDisabled",
            "ColorBorderSecondary", "ColorBorder", "ColorBgMask", "ColorFocusBorder", "FocusVisualBorderThickness",
            "ControlOutlineWidth", "BorderRadiusXS", "BorderRadiusSM", "BorderRadiusLG", "ControlHeightSM",
            "ControlHeightLG", "PaddingContentHorizontal", "BoxShadows", "BoxShadowsSecondary"
        ];
        var themes = Path.Combine(FindRepositoryRoot(), "src", "AtomUI.Themes.PrimeNG", "Themes");
        foreach (var profile in AuraProfiles)
        {
            var root = XDocument.Load(Path.Combine(themes, profile + ".theme.xml")).Root!;
            var names = root.Element(ThemeNamespace + "Tokens")!
                            .Elements(ThemeNamespace + "Token")
                            .Select(token => token.Attribute("Name")!.Value)
                            .ToArray();
            names.Intersect(ruleTokens).ShouldBeEmpty(profile);
            // Every per-control value is a control stage now.
            root.Element(ThemeNamespace + "Controls").ShouldBeNull(profile);
        }

        // Light and dark differ only in the chain (and the profile metadata on the root element).
        StripMetadata(Path.Combine(themes, "PrimeNGAura.theme.xml"), "Aura")
            .ShouldBe(StripMetadata(Path.Combine(themes, "PrimeNGAura.Dark.theme.xml"), "Aura.Dark"));
        StripMetadata(Path.Combine(themes, "PrimeNGAura.Compact.theme.xml"), "Aura")
            .ShouldBe(StripMetadata(Path.Combine(themes, "PrimeNGAura.DarkCompact.theme.xml"), "Aura.Dark"));
    }

    private static string StripMetadata(string path, string algorithmId)
    {
        var root = XDocument.Load(path).Root!;
        root.Attributes().Remove();
        var algorithm = root.Element(ThemeNamespace + "Algorithms")!
                            .Elements(ThemeNamespace + "Algorithm")
                            .Single(element => element.Attribute("Catalog")?.Value == "PrimeNG");
        algorithm.Attribute("Id")!.Value.ShouldBe(algorithmId);
        algorithm.Remove();
        return root.ToString(SaveOptions.DisableFormatting);
    }

    private static string SlotOf(string difference)
    {
        // "changed G X: a -> b", "added C A:B X = v", "removed G X = v"
        var body = difference[(difference.IndexOf(' ') + 1)..];
        var end = body.IndexOf(difference.StartsWith("changed", StringComparison.Ordinal) ? ": " : " = ",
            StringComparison.Ordinal);
        return body[..end];
    }

    private static async Task<string> Dump(string profile, params ThemeAlgorithm[] chain)
    {
        var config = chain.Length == 0 ? null : new ThemeConfigBuilder().WithAlgorithms(chain).Build();
        var snapshot = await Apply(new ThemeRequest(profile, config, ThemeTransitionReason.UserRequest));
        return PrimeNGMigrationSnapshot.Dump(snapshot);
    }

    private static async Task<ThemeSnapshot> Apply(ThemeRequest request)
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        (await manager.ApplyThemeAsync(request, TestContext.Current.CancellationToken))
            .Status.ShouldBe(ThemeTransitionStatus.Committed);
        return manager.CurrentSnapshot!;
    }

    private static T Global<T>(ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue();
        return snapshot.GlobalTokenValues.Get<T>(token!.Slot);
    }

    private static Dictionary<string, string> ParseDump(string dump)
    {
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in dump.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var index = line.IndexOf(" = ", StringComparison.Ordinal);
            entries.Add(line[..index], line[(index + 3)..]);
        }
        return entries;
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src", "AtomUI.Themes.PrimeNG", "Themes")))
            {
                return directory.FullName;
            }
        }
        throw new DirectoryNotFoundException("The repository root (with src/AtomUI.Themes.PrimeNG/Themes) was not found.");
    }
}
