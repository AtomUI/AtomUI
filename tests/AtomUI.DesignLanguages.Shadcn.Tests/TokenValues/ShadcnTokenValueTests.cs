using AtomUI.Theme;
using AtomUI.Theme.Testing;
using Avalonia;
using Avalonia.Headless.XUnit;
using Shouldly;

namespace AtomUI.DesignLanguages.Shadcn.Tests.TokenValues;

/// <summary>
/// The token values of every shadcn variant and seed case against the pre-refactor baseline dumps (G-value). The
/// baseline is not in the repository: set <c>ATOMUI_SHADCN_TOKEN_BASELINE</c> to the baseline dump directory
/// (<c>.artifacts/design-language-guards/m0-selfcheck/tokens/baseline</c>); <c>ATOMUI_SHADCN_TOKEN_DUMP_DIR</c>
/// additionally writes the candidate dumps for <c>tools/performances/compare_token_dumps.py</c>.
/// </summary>
public class ShadcnTokenValueTests
{
    [AvaloniaFact]
    public void Every_Variant_And_Seed_Case_Matches_The_Baseline()
    {
        var baseline = Environment.GetEnvironmentVariable("ATOMUI_SHADCN_TOKEN_BASELINE");
        var output = Environment.GetEnvironmentVariable("ATOMUI_SHADCN_TOKEN_DUMP_DIR");
        if (string.IsNullOrEmpty(baseline) && string.IsNullOrEmpty(output))
        {
            return;
        }

        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        var failures = new List<string>();
        foreach (var (fileName, selection) in ShadcnTokenDumps.Cases())
        {
            var dump = ShadcnTokenDumps.Dump(manager, selection);
            if (!string.IsNullOrEmpty(output))
            {
                Directory.CreateDirectory(output);
                File.WriteAllText(Path.Combine(output, fileName), dump);
            }

            if (string.IsNullOrEmpty(baseline))
            {
                continue;
            }

            var controls = dump.Split('\n').Where(static line => line.StartsWith("C ", StringComparison.Ordinal))
                               .Select(static line => line.Split(' ')[1]).ToHashSet(StringComparer.Ordinal);
            // The baseline also holds the Gallery-only controls; tokens added since the baseline are allowed.
            var expected = string.Join('\n', File.ReadAllLines(Path.Combine(baseline, fileName))
                                                .Select(static line => line.TrimStart('﻿'))
                                                .Where(line => !line.StartsWith("G ColorNeutral ", StringComparison.Ordinal))
                                                .Where(line => !line.StartsWith("C ", StringComparison.Ordinal) ||
                                                               controls.Contains(line.Split(' ')[1])));
            failures.AddRange(ThemeSnapshotDump.Diff(expected, dump)
                                               .Where(static difference => !difference.StartsWith("added ", StringComparison.Ordinal))
                                               .Select(difference => $"{fileName}: {difference}"));
        }

        failures.ShouldBeEmpty(string.Join(Environment.NewLine, failures.Take(50)));
    }
}
