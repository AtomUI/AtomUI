using Avalonia.Media;

namespace AtomUIGallery.Workspace;

/// <summary>
/// A named brand color offered by the Gallery theme menu.
/// </summary>
/// <param name="Name">The display name of the swatch.</param>
/// <param name="Color">The brand color.</param>
internal sealed record GalleryBrandSwatch(string Name, Color Color);

/// <summary>
/// The single source of the brand swatches shown by the Gallery theme menu.
/// </summary>
internal static class GalleryBrandSwatches
{
    /// <summary>
    /// The design language that additionally offers the named Ant Design colors.
    /// </summary>
    public const string AntDesignLanguage = "AntDesign";

    /// <summary>
    /// The named Ant Design colors, carried over from the former Gallery color presets.
    /// </summary>
    public static IReadOnlyList<GalleryBrandSwatch> AntNamed { get; } =
    [
        new("Daybreak Blue", Color.Parse("#1677FF")),
        new("Polar Green", Color.Parse("#52C41A")),
        new("Sunset Orange", Color.Parse("#FA8C16")),
        new("Golden Purple", Color.Parse("#722ED1")),
        new("Magenta", Color.Parse("#EB2F96"))
    ];

    /// <summary>
    /// The prefix of the PrimeNG design languages (<c>PrimeNG.Aura</c>, <c>PrimeNG.Lara</c>, <c>PrimeNG.Nora</c>).
    /// </summary>
    public const string PrimeNGLanguagePrefix = "PrimeNG.";

    /// <summary>
    /// The primary colors of the primeng.dev configurator (primeng 22.1.5), in site order: noir (slate-950, the site's
    /// default) and the sixteen Tailwind 500 stops it offers (it has no red).
    /// </summary>
    public static IReadOnlyList<GalleryBrandSwatch> PrimeNG { get; } =
    [
        new("Noir", Color.Parse("#020617")),
        new("Emerald", Color.Parse("#10B981")),
        new("Green", Color.Parse("#22C55E")),
        new("Lime", Color.Parse("#84CC16")),
        new("Orange", Color.Parse("#F97316")),
        new("Amber", Color.Parse("#F59E0B")),
        new("Yellow", Color.Parse("#EAB308")),
        new("Teal", Color.Parse("#14B8A6")),
        new("Cyan", Color.Parse("#06B6D4")),
        new("Sky", Color.Parse("#0EA5E9")),
        new("Blue", Color.Parse("#3B82F6")),
        new("Indigo", Color.Parse("#6366F1")),
        new("Violet", Color.Parse("#8B5CF6")),
        new("Purple", Color.Parse("#A855F7")),
        new("Fuchsia", Color.Parse("#D946EF")),
        new("Pink", Color.Parse("#EC4899")),
        new("Rose", Color.Parse("#F43F5E"))
    ];

    /// <summary>
    /// The 500 stops of the Tailwind palette, offered for every other design language that accepts a brand color.
    /// </summary>
    public static IReadOnlyList<GalleryBrandSwatch> Tailwind500 { get; } =
    [
        new("Emerald", Color.Parse("#10B981")),
        new("Green", Color.Parse("#22C55E")),
        new("Lime", Color.Parse("#84CC16")),
        new("Orange", Color.Parse("#F97316")),
        new("Amber", Color.Parse("#F59E0B")),
        new("Yellow", Color.Parse("#EAB308")),
        new("Teal", Color.Parse("#14B8A6")),
        new("Cyan", Color.Parse("#06B6D4")),
        new("Sky", Color.Parse("#0EA5E9")),
        new("Blue", Color.Parse("#3B82F6")),
        new("Indigo", Color.Parse("#6366F1")),
        new("Violet", Color.Parse("#8B5CF6")),
        new("Purple", Color.Parse("#A855F7")),
        new("Fuchsia", Color.Parse("#D946EF")),
        new("Pink", Color.Parse("#EC4899")),
        new("Rose", Color.Parse("#F43F5E")),
        new("Red", Color.Parse("#EF4444"))
    ];

    /// <summary>
    /// Lists the swatches for a design language. A PrimeNG language lists the primeng.dev primaries, led by the
    /// language's own base color when the site does not offer it. Any other language lists its own base color, the
    /// named Ant colors when the language is Ant Design, then the Tailwind palette. A color is listed once.
    /// </summary>
    public static IReadOnlyList<GalleryBrandSwatch> For(string designLanguage,
                                                        string baseName,
                                                        Color? baseColor)
    {
        var result = new List<GalleryBrandSwatch>();
        if (designLanguage.StartsWith(PrimeNGLanguagePrefix, StringComparison.Ordinal))
        {
            if (baseColor is { } own && !PrimeNG.Any(swatch => swatch.Color == own))
            {
                result.Add(new GalleryBrandSwatch(baseName, own));
            }

            AddDistinct(result, PrimeNG);
            return result;
        }

        if (baseColor is { } accent)
        {
            result.Add(new GalleryBrandSwatch(baseName, accent));
        }

        if (string.Equals(designLanguage, AntDesignLanguage, StringComparison.Ordinal))
        {
            AddDistinct(result, AntNamed);
        }

        AddDistinct(result, Tailwind500);
        return result;
    }

    private static void AddDistinct(List<GalleryBrandSwatch> target, IReadOnlyList<GalleryBrandSwatch> source)
    {
        foreach (var swatch in source)
        {
            if (!target.Any(existing => existing.Color == swatch.Color))
            {
                target.Add(swatch);
            }
        }
    }
}
