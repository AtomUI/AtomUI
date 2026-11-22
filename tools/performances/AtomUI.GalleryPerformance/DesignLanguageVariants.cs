using System.Diagnostics;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia.Media;
using Avalonia.Threading;

namespace AtomUI.GalleryPerformance;

/// <summary>
/// One design-language variant the design-language refactor must keep pixel-identical: a design-language id of the
/// pre-refactor theme catalog with an appearance and a density.
/// </summary>
internal readonly record struct DesignLanguageVariant(
    string DesignLanguage,
    ThemeAppearance Appearance,
    ThemeDensity Density)
{
    /// <summary>The variant key and artifact directory name, for example <c>Shadcn.Maia.Dark.Compact</c>.</summary>
    public string Key => $"{DesignLanguage}.{Appearance}.{Density}";

    /// <summary>The command-line form, for example <c>Shadcn.Maia Dark Compact</c>.</summary>
    public override string ToString() => $"{DesignLanguage} {Appearance} {Density}";
}

/// <summary>
/// The 48 variants of the three design languages and the theme request the Gallery sends for each of them.
/// </summary>
internal static class DesignLanguageVariants
{
    private static readonly string[] s_designLanguages =
    [
        "AntDesign",
        "PrimeNG.Aura", "PrimeNG.Lara", "PrimeNG.Nora",
        "Shadcn.Vega", "Shadcn.Nova", "Shadcn.Maia", "Shadcn.Lyra",
        "Shadcn.Mira", "Shadcn.Luma", "Shadcn.Sera", "Shadcn.Rhea"
    ];

    private static readonly (ThemeAppearance Appearance, ThemeDensity Density)[] s_forms =
    [
        (ThemeAppearance.Light, ThemeDensity.Normal),
        (ThemeAppearance.Light, ThemeDensity.Compact),
        (ThemeAppearance.Dark, ThemeDensity.Normal),
        (ThemeAppearance.Dark, ThemeDensity.Compact)
    ];

    /// <summary>
    /// The variants in a fixed order: AntDesign, the three PrimeNG presets, the eight shadcn styles; within each,
    /// Light/Normal, Light/Compact, Dark/Normal, Dark/Compact.
    /// </summary>
    public static IReadOnlyList<DesignLanguageVariant> All { get; } =
        s_designLanguages.SelectMany(language => s_forms.Select(form =>
                             new DesignLanguageVariant(language, form.Appearance, form.Density)))
                         .ToArray();

    /// <summary>Finds a variant by its exact command-line parts (case-sensitive names, no numeric enum values).</summary>
    public static bool TryParse(string designLanguage, string appearance, string density, out DesignLanguageVariant variant)
    {
        var text = $"{designLanguage} {appearance} {density}";
        foreach (var candidate in All)
        {
            if (string.Equals(candidate.ToString(), text, StringComparison.Ordinal))
            {
                variant = candidate;
                return true;
            }
        }
        variant = default;
        return false;
    }

    /// <summary>
    /// The request the Gallery sends when the user picks the variant's design language with its appearance and
    /// density (<c>GalleryWorkspaceViewModel.ApplyThemeSettingsAsync</c>, design-language branch): the resolver's theme
    /// and algorithm chain, the brand seed as <c>ColorPrimary</c> and a neutral seed the language accepts as
    /// <c>ColorNeutral</c>.
    /// </summary>
    public static ThemeRequest CreateRequest(IReadOnlyList<ThemeInfo> catalog, DesignLanguageVariant variant,
                                             Color? brand, Color? neutral)
    {
        var resolved = ThemeSelectionResolver.Resolve(catalog, new ThemeSelection(
            variant.DesignLanguage, variant.Appearance, variant.Density, brand, neutral));
        var builder = new ThemeConfigBuilder();
        if (resolved.Algorithms is not null)
        {
            builder.WithAlgorithms(resolved.Algorithms.ToArray());
        }
        if (resolved.BrandSeed is { } brandSeed)
        {
            builder.WithToken(nameof(SharedTokenKind.ColorPrimary), FormatColor(brandSeed));
        }
        if (!resolved.NeutralColorIgnored && resolved.NeutralSeed is { } neutralSeed)
        {
            builder.WithToken(nameof(SharedTokenKind.ColorNeutral), FormatColor(neutralSeed));
        }
        return new ThemeRequest(resolved.ThemeId, builder.Build(), ThemeTransitionReason.UserRequest);
    }

    /// <summary>Pumps the UI dispatcher until <paramref name="task"/> completes, then returns its result.</summary>
    /// <exception cref="TimeoutException">The task did not complete within <paramref name="timeout"/>.</exception>
    public static T WaitFor<T>(Task<T> task, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!task.IsCompleted)
        {
            if (stopwatch.Elapsed > timeout)
            {
                throw new TimeoutException($"Timed out after {timeout.TotalSeconds:0}s.");
            }
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }
        return task.GetAwaiter().GetResult();
    }

    // The text GalleryWorkspaceViewModel.FormatColor writes.
    private static string FormatColor(Color color) =>
        color.A == 255
            ? $"#{color.R:X2}{color.G:X2}{color.B:X2}"
            : $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
}
