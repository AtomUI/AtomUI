using System.Text;
using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme;
using AtomUI.Theme.Testing;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.GalleryPerformance;

/// <summary>
/// Writes the compiled token values (every global token and every control's own tokens) of each design-language
/// variant under each seed case, registered exactly as the Gallery registers its packages. A baseline build and a
/// candidate build each write a directory; <c>tools/performances/compare_token_dumps.py</c> compares them key by key.
/// </summary>
/// <remarks>
/// Output: <c>&lt;dir&gt;/&lt;variant key&gt;[@&lt;seed case&gt;].tokens.txt</c>, 484 files. Lines use the format of
/// <c>ThemeSnapshotDump</c>: <c>G &lt;Token&gt; = &lt;value&gt;</c> and
/// <c>C &lt;Catalog&gt;:&lt;Control&gt; &lt;Token&gt; = &lt;value&gt;</c>.
/// </remarks>
internal static class TokenDumpProbe
{
    private static readonly TimeSpan s_timeout = TimeSpan.FromSeconds(60);

    // Ant Design preset brand colors other than the default Daybreak Blue.
    private static readonly string[] s_antDesignBrands = ["#52C41A", "#FA8C16", "#722ED1", "#EB2F96"];

    // PrimeNG primary palettes (emerald, blue, amber 500) instead of the default noir.
    private static readonly string[] s_primeNGBrands = ["#10B981", "#3B82F6", "#F59E0B"];

    // shadcn Base Colors: the 500 stop of each Tailwind v4.3.5 neutral family.
    private static readonly (string Name, string Color)[] s_shadcnBaseColors =
    [
        ("neutral", "#737373"), ("stone", "#79716B"), ("zinc", "#71717B"), ("mauve", "#79697B"),
        ("olive", "#7C7C67"), ("mist", "#67787C"), ("taupe", "#7C6D67")
    ];

    // shadcn Themes: the 700 stop (400 for yellow) of the Tailwind v4.3.5 family.
    private static readonly (string Name, string Color)[] s_shadcnThemes =
    [
        ("blue", "#1447E6"), ("rose", "#C70036"), ("green", "#008236"), ("violet", "#7008E7"), ("yellow", "#FDC700")
    ];

    private sealed record SeedCase(string Name, Color? Brand, Color? Neutral)
    {
        public string FileName(DesignLanguageVariant variant) =>
            Name.Length == 0 ? $"{variant.Key}.tokens.txt" : $"{variant.Key}@{Name}.tokens.txt";
    }

    public static int Run(string directory)
    {
        Directory.CreateDirectory(directory);
        Program.SetupAvalonia(out _, renderPixels: false);
        if (Application.Current?.GetThemeManager() is not ThemeManager manager)
        {
            Console.Error.WriteLine("dump-tokens: the theme manager is not available.");
            return 1;
        }

        var written  = 0;
        var failures = 0;
        foreach (var variant in DesignLanguageVariants.All)
        {
            foreach (var seeds in SeedCasesFor(variant))
            {
                var fileName = seeds.FileName(variant);
                try
                {
                    var request = DesignLanguageVariants.CreateRequest(manager, variant, seeds.Brand, seeds.Neutral);
                    var result  = DesignLanguageVariants.WaitFor(manager.ApplyThemeAsync(request), s_timeout);
                    if (result.Status is not (ThemeTransitionStatus.Committed or ThemeTransitionStatus.NoOp))
                    {
                        throw new InvalidOperationException(
                            $"request for '{request.ThemeId}' ended as {result.Status}: " +
                            string.Join(" ", result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
                    }
                    var snapshot = manager.CurrentSnapshot
                                   ?? throw new InvalidOperationException("no current snapshot after the request.");
                    // M0 wrote the Shadcn base colour as a global seed. Keep that key in the comparison output
                    // so the guard checks the migrated language seed on every base-colour case. The other two
                    // languages never set the old optional seed.
                    var legacyNeutral = snapshot.Language.Id == "Shadcn" &&
                                        snapshot.Language.TryGetLanguageToken(nameof(ShadcnToken.BaseColor), out var baseColor)
                        ? baseColor.Format(snapshot.LanguageTokenValues.GetValue(baseColor.Slot))
                        : "null";
                    var dump = ThemeSnapshotDump.Dump(snapshot) + $"G ColorNeutral = {legacyNeutral}\n";
                    File.WriteAllText(Path.Combine(directory, fileName), dump, Encoding.UTF8);
                    written++;
                }
                catch (Exception ex)
                {
                    failures++;
                    Console.Error.WriteLine($"{fileName}: FAILED {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        Console.WriteLine($"dump-tokens: {written} files written to {directory}, {failures} failed");
        return failures == 0 ? 0 : 1;
    }

    private static IEnumerable<SeedCase> SeedCasesFor(DesignLanguageVariant variant)
    {
        yield return new SeedCase(string.Empty, null, null);
        if (variant.DesignLanguage == "AntDesign")
        {
            foreach (var color in s_antDesignBrands)
            {
                yield return new SeedCase("brand-" + color[1..], Color.Parse(color), null);
            }
        }
        else if (variant.DesignLanguage.StartsWith("PrimeNG.", StringComparison.Ordinal))
        {
            foreach (var color in s_primeNGBrands)
            {
                yield return new SeedCase("brand-" + color[1..], Color.Parse(color), null);
            }
        }
        else if (variant.DesignLanguage.StartsWith("Shadcn.", StringComparison.Ordinal))
        {
            foreach (var (name, color) in s_shadcnBaseColors)
            {
                yield return new SeedCase("base-" + name, null, Color.Parse(color));
            }
            foreach (var (name, color) in s_shadcnThemes)
            {
                yield return new SeedCase("theme-" + name, Color.Parse(color), null);
            }
        }
        else
        {
            throw new InvalidOperationException($"No seed cases for design language '{variant.DesignLanguage}'.");
        }
    }
}
