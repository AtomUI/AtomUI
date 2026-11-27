using System.Text.RegularExpressions;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.Shadcn.Tests;

/// <summary>
/// Parses the generated shadcn alignment appendix so tests assert against the official data instead of hand-copied
/// tables.
/// </summary>
internal static partial class ShadcnAppendix
{
    private static readonly Lazy<string> s_text = new(ReadAppendix);

    private static string ReadAppendix()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null &&
               !File.Exists(Path.Combine(directory.FullName, "docs/developments/specs/2026-11-15-shadcn-latest-alignment-data.md")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new InvalidOperationException("The shadcn alignment appendix was not found above the test binaries.");
        }

        return File.ReadAllText(Path.Combine(
            directory.FullName, "docs/developments/specs/2026-11-15-shadcn-latest-alignment-data.md"));
    }

    /// <summary>
    /// (family, dark, variable without dashes) → hex, from appendix A.1/A.2.
    /// </summary>
    internal static IReadOnlyDictionary<(string Family, bool Dark, string Variable), Color> BaseColors()
    {
        var result = new Dictionary<(string, bool, string), Color>();
        var dark = false;
        string[] families = [];
        foreach (var line in s_text.Value.Split('\n'))
        {
            if (line.StartsWith("### A.1", StringComparison.Ordinal))
            {
                dark = false;
                continue;
            }

            if (line.StartsWith("### A.2", StringComparison.Ordinal))
            {
                dark = true;
                continue;
            }

            if (line.StartsWith("## B.", StringComparison.Ordinal))
            {
                break;
            }

            if (line.StartsWith("| 变量 |", StringComparison.Ordinal))
            {
                families = line.Split('|').Skip(2).SkipLast(1)
                               .Select(static cell => cell.Trim().ToLowerInvariant())
                               .ToArray();
                continue;
            }

            if (!line.StartsWith("| `--", StringComparison.Ordinal))
            {
                continue;
            }

            var cells = line.Split('|');
            var variable = cells[1].Replace("`", "").Replace("-", "").Trim();
            for (var i = 0; i < families.Length; i++)
            {
                var hex = HexCell(cells[i + 2]);
                if (hex is not null)
                {
                    result[(families[i], dark, variable)] = Color.Parse(hex);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// (theme, dark) → (primary, foreground), from appendix B.
    /// </summary>
    internal static IReadOnlyDictionary<(string Theme, bool Dark), (Color Primary, Color Foreground)> Themes()
    {
        var result = new Dictionary<(string, bool), (Color, Color)>();
        var inSectionB = false;
        foreach (var line in s_text.Value.Split('\n'))
        {
            if (line.StartsWith("## B.", StringComparison.Ordinal))
            {
                inSectionB = true;
                continue;
            }

            if (!inSectionB || !line.StartsWith('|') || line.Contains("Theme |") || line.StartsWith("|---"))
            {
                continue;
            }

            var cells = line.Split('|');
            if (cells.Length < 6)
            {
                continue;
            }

            var theme = cells[1].Trim();
            result[(theme, false)] = (Color.Parse(HexCell(cells[2])!), Color.Parse(HexCell(cells[3])!));
            result[(theme, true)] = (Color.Parse(HexCell(cells[4])!), Color.Parse(HexCell(cells[5])!));
        }

        return result;
    }

    /// <summary>
    /// The seven official neutral seeds: the light --muted-foreground of each base color.
    /// </summary>
    internal static IReadOnlyList<(string Family, Color Seed)> NeutralSeeds()
    {
        var table = BaseColors();
        return
        [
            ("neutral", table[("neutral", false, "mutedforeground")]),
            ("stone", table[("stone", false, "mutedforeground")]),
            ("zinc", table[("zinc", false, "mutedforeground")]),
            ("mauve", table[("mauve", false, "mutedforeground")]),
            ("olive", table[("olive", false, "mutedforeground")]),
            ("mist", table[("mist", false, "mutedforeground")]),
            ("taupe", table[("taupe", false, "mutedforeground")])
        ];
    }

    [GeneratedRegex(@"#[0-9A-Fa-f]{6,8}")]
    private static partial Regex HexRegex();

    private static string? HexCell(string cell)
    {
        var match = HexRegex().Match(cell);
        return match.Success ? match.Value : null;
    }

    /// <summary>
    /// WCAG relative-luminance contrast ratio of two opaque colors.
    /// </summary>
    internal static double Contrast(Color foreground, Color background)
    {
        double Channel(byte value)
        {
            var linear = value / 255d;
            return linear <= 0.03928 ? linear / 12.92 : Math.Pow((linear + 0.055) / 1.055, 2.4);
        }

        double Luminance(Color color) =>
            0.2126 * Channel(color.R) + 0.7152 * Channel(color.G) + 0.0722 * Channel(color.B);

        var first = Luminance(foreground);
        var second = Luminance(background);
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }
}
