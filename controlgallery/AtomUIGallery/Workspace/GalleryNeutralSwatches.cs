using Avalonia.Media;

namespace AtomUIGallery.Workspace;

/// <summary>
/// A named neutral color offered by the Gallery theme menu.
/// </summary>
/// <param name="Name">The display name of the swatch.</param>
/// <param name="Color">The neutral seed: the base color's light <c>--muted-foreground</c>.</param>
internal sealed record GalleryNeutralSwatch(string Name, Color Color);

/// <summary>
/// The neutral (base color) swatches of the Gallery theme menu: the seven shadcn base colors, offered for the
/// design languages that accept a replaceable neutral family.
/// </summary>
internal static class GalleryNeutralSwatches
{
    /// <summary>
    /// The seven shadcn base colors (ui.shadcn.com/create), in site order. A swatch writes the base color's light
    /// <c>--muted-foreground</c> — its neutral family's 500 stop — which is exactly the shadcn language seed <c>BaseColor</c>.
    /// </summary>
    public static IReadOnlyList<GalleryNeutralSwatch> Shadcn { get; } =
    [
        new("Neutral", Color.Parse("#737373")),
        new("Stone", Color.Parse("#79716B")),
        new("Zinc", Color.Parse("#71717B")),
        new("Mauve", Color.Parse("#79697B")),
        new("Olive", Color.Parse("#7C7C67")),
        new("Mist", Color.Parse("#67787C")),
        new("Taupe", Color.Parse("#7C6D67"))
    ];

    /// <summary>
    /// Lists the neutral swatches of a design language. A Shadcn style lists the base colors, led by the
    /// definition's own seed when it is not among them; any other language lists none.
    /// </summary>
    public static IReadOnlyList<GalleryNeutralSwatch> For(string designLanguage, Color? baseColor)
    {
        if (!designLanguage.StartsWith(GalleryBrandSwatches.ShadcnLanguagePrefix, StringComparison.Ordinal))
        {
            return [];
        }

        var result = new List<GalleryNeutralSwatch>();
        if (baseColor is { } own && !Shadcn.Any(swatch => swatch.Color == own))
        {
            result.Add(new GalleryNeutralSwatch(string.Empty, own));
        }

        foreach (var swatch in Shadcn)
        {
            if (!result.Any(existing => existing.Color == swatch.Color))
            {
                result.Add(swatch);
            }
        }

        return result;
    }
}
