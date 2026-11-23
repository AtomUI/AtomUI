using System.Diagnostics;
using AtomUI.Theme;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Resources;
using Avalonia.Media;
using Avalonia.Threading;

namespace AtomUI.GalleryPerformance;

/// <summary>
/// One design-language variant the design-language refactor must keep pixel-identical: a design-language id of the
/// pre-refactor theme catalog with an appearance and a density. The key keeps the pre-refactor spelling
/// (<c>Normal</c>/<c>Compact</c>) so the artifacts line up with the baseline's.
/// </summary>
internal readonly record struct DesignLanguageVariant(
    string DesignLanguage,
    ThemeAppearance Appearance,
    string Density)
{
    /// <summary>Gets whether the variant is the compact density.</summary>
    public bool IsCompact => Density == "Compact";

    /// <summary>The language identity: the part of the key before the style (<c>Shadcn</c> for <c>Shadcn.Maia</c>).</summary>
    public string Language => DesignLanguage.Split('.')[0];

    /// <summary>The style named by the key, or <see langword="null"/> for a single-style language.</summary>
    public DesignStyle? Style => DesignLanguage.IndexOf('.') is var dot and > 0 ? new DesignStyle(DesignLanguage[(dot + 1)..]) : null;

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

    private static readonly (ThemeAppearance Appearance, string Density)[] s_forms =
    [
        (ThemeAppearance.Light, "Normal"),
        (ThemeAppearance.Light, "Compact"),
        (ThemeAppearance.Dark, "Normal"),
        (ThemeAppearance.Dark, "Compact")
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
    /// density (<c>GalleryWorkspaceViewModel</c>): the selection of the language default in the variant's style, with
    /// the brand seed as <c>ColorPrimary</c> and a neutral seed as the language seed <c>BaseColor</c> when the language accepts them.
    /// <paramref name="themeId"/> selects a named theme of the language instead of its default entry.
    /// </summary>
    public static ThemeRequest CreateRequest(IThemeManager manager, DesignLanguageVariant variant, Color? brand, Color? neutral,
                                             string? themeId = null)
    {
        var language = manager.Languages.FirstOrDefault(info => info.Id == variant.Language);
        // The Gallery always names the style; a single-style language's key omits it, so the default style stands in.
        var selection = ThemeSelection.Of(variant.Language, variant.Style ?? language?.DefaultStyle)
                                      .WithAppearance(variant.Appearance);
        // As the Gallery: the compact toggle writes the density, the normal form leaves it to the theme.
        if (variant.IsCompact)
        {
            selection = selection.WithDensity(DesignDensity.Compact);
        }
        if (themeId is not null)
        {
            selection = selection.WithTheme(themeId);
        }
        if (brand is { } brandSeed && language?.AdjustableSeeds.Contains(nameof(SharedTokenKind.ColorPrimary)) == true)
        {
            selection = selection.WithSeed(nameof(SharedTokenKind.ColorPrimary), FormatColor(brandSeed));
        }
        if (neutral is { } neutralSeed && language?.AdjustableSeeds.Contains(nameof(AtomUI.DesignLanguages.Shadcn.ShadcnToken.BaseColor)) == true)
        {
            selection = selection.WithLanguageSeed(nameof(AtomUI.DesignLanguages.Shadcn.ShadcnToken.BaseColor), FormatColor(neutralSeed));
        }
        return ThemeSelectionResolver.Resolve(manager.Languages, manager.Themes, selection).Request;
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
