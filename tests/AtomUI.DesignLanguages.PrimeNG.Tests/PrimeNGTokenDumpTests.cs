using System.Text;
using AtomUI.Theme;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Testing;
using Avalonia;
using Avalonia.Headless.XUnit;
using Shouldly;
using Xunit;

namespace AtomUI.DesignLanguages.PrimeNG.Tests;

/// <summary>
/// The token values of every PrimeNG variant and seed case the pre-refactor Gallery produced (the 48 baseline dumps of
/// the design-language guards): the request the Gallery sends for a variant is compiled and dumped in the baseline's
/// format. With <c>ATOMUI_PRIMENG_TOKEN_DUMP_DIR</c> set the dumps are written there for
/// <c>tools/performances/compare_token_dumps.py</c>; with <c>ATOMUI_PRIMENG_TOKEN_BASELINE_DIR</c> set every dump is
/// also compared with its baseline file (Gallery-only controls and tokens new since the baseline are left out).
/// </summary>
public class PrimeNGTokenDumpTests
{
    private static readonly string[] s_styles = ["Aura", "Lara", "Nora"];
    private static readonly string[] s_brands = ["#10B981", "#3B82F6", "#F59E0B"];

    [AvaloniaFact]
    public async Task Every_Variant_And_Seed_Case_Matches_The_Baseline()
    {
        var output = Environment.GetEnvironmentVariable("ATOMUI_PRIMENG_TOKEN_DUMP_DIR");
        var baseline = Environment.GetEnvironmentVariable("ATOMUI_PRIMENG_TOKEN_BASELINE_DIR");
        if (output is null && baseline is null)
        {
            Assert.Skip("Set ATOMUI_PRIMENG_TOKEN_DUMP_DIR and/or ATOMUI_PRIMENG_TOKEN_BASELINE_DIR to run the dump.");
        }

        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        var failures = new StringBuilder();
        foreach (var style in s_styles)
        {
            foreach (var appearance in new[] { ThemeAppearance.Light, ThemeAppearance.Dark })
            {
                foreach (var compact in new[] { false, true })
                {
                    foreach (var brand in new string?[] { null }.Concat(s_brands))
                    {
                        var name = $"PrimeNG.{style}.{appearance}.{(compact ? "Compact" : "Normal")}" +
                                   (brand is null ? string.Empty : "@brand-" + brand[1..]) + ".tokens.txt";
                        var dump = await Dump(manager, style, appearance, compact, brand);
                        if (output is not null)
                        {
                            Directory.CreateDirectory(output);
                            await File.WriteAllTextAsync(Path.Combine(output, name), dump, new UTF8Encoding(true),
                                TestContext.Current.CancellationToken);
                        }
                        if (baseline is not null)
                        {
                            var expected = await File.ReadAllTextAsync(Path.Combine(baseline, name),
                                TestContext.Current.CancellationToken);
                            var differences = ThemeSnapshotDump.Diff(Filter(expected), Filter(dump))
                                                               .Where(static d => !d.StartsWith("added ", StringComparison.Ordinal))
                                                               .ToList();
                            if (differences.Count > 0)
                            {
                                failures.Append(name).Append(": ").Append(differences.Count).Append('\n');
                                foreach (var difference in differences.Take(20))
                                {
                                    failures.Append("  ").Append(difference).Append('\n');
                                }
                            }
                        }
                    }
                }
            }
        }

        failures.ToString().ShouldBeEmpty();
    }

    private static async Task<string> Dump(ThemeManager manager, string style, ThemeAppearance appearance, bool compact,
                                           string? brand)
    {
        var selection = ThemeSelection.Of("PrimeNG", new DesignStyle(style)).WithAppearance(appearance);
        if (compact)
        {
            selection = selection.WithDensity(DesignDensity.Compact);
        }
        if (brand is not null)
        {
            selection = selection.WithSeed(nameof(SharedTokenKind.ColorPrimary), brand);
        }
        var request = ThemeSelectionResolver.Resolve(manager.Languages, manager.Themes, selection).Request;
        var result = await manager.ApplyThemeAsync(request, TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
        return ThemeSnapshotDump.Dump(manager.CurrentSnapshot!);
    }

    // The baseline also holds the Gallery's own controls, which this registry does not load.
    private static readonly string[] s_galleryControls =
        ["C AtomUI:Gallery", "C AtomUI:ShowCase", "C AtomUI:ColorItemControl ", "C AtomUI:IconInfoItem ",
         // The global ColorNeutral seed left core (it is the shadcn language seed BaseColor now).
         "G ColorNeutral "];

    private static string Filter(string dump) =>
        string.Join('\n', dump.TrimStart('\uFEFF').Split('\n')
                              .Where(static line => line.Length > 0 &&
                                                    !s_galleryControls.Any(prefix => line.StartsWith(prefix, StringComparison.Ordinal)))) + "\n";
}
