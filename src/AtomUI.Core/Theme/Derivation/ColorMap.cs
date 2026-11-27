using Avalonia.Media;

namespace AtomUI.Theme.Derivation;

/// <summary>
/// One color palette: ten colors at palette index 1 to 10, as <see cref="PaletteGenerator.GeneratePalette" />
/// returns them. A palette that is not blended for the dark theme variant is ordered from the lightest to the
/// darkest color.
/// </summary>
public record ColorMap
{
    /// <summary>
    /// The first palette color, the lightest one of an unblended palette.
    /// </summary>
    public Color Color1 { get; set; }
    /// <summary>
    /// The second palette color.
    /// </summary>
    public Color Color2 { get; set; }
    /// <summary>
    /// The third palette color.
    /// </summary>
    public Color Color3 { get; set; }
    /// <summary>
    /// The fourth palette color.
    /// </summary>
    public Color Color4 { get; set; }
    /// <summary>
    /// The fifth palette color.
    /// </summary>
    public Color Color5 { get; set; }
    /// <summary>
    /// The sixth palette color, the base color of an unblended palette.
    /// </summary>
    public Color Color6 { get; set; }
    /// <summary>
    /// The seventh palette color.
    /// </summary>
    public Color Color7 { get; set; }
    /// <summary>
    /// The eighth palette color.
    /// </summary>
    public Color Color8 { get; set; }
    /// <summary>
    /// The ninth palette color.
    /// </summary>
    public Color Color9 { get; set; }
    /// <summary>
    /// The tenth palette color, the darkest one of an unblended palette.
    /// </summary>
    public Color Color10 { get; set; }

    /// <summary>
    /// Creates a palette from the first ten colors of <paramref name="colors" />, in index order.
    /// </summary>
    /// <param name="colors">
    /// At least ten colors. A shorter list throws when it is indexed; colors after the tenth are ignored.
    /// </param>
    public static ColorMap FromColors(IReadOnlyList<Color> colors)
    {
        // TODO Validate that the list holds at least ten colors and throw an exception when it does not.
        // Remove this when FromColors checks the count itself.
        var map = new ColorMap
        {
            Color1  = colors[0],
            Color2  = colors[1],
            Color3  = colors[2],
            Color4  = colors[3],
            Color5  = colors[4],
            Color6  = colors[5],
            Color7  = colors[6],
            Color8  = colors[7],
            Color9  = colors[8],
            Color10 = colors[9]
        };
        return map;
    }

    /// <summary>
    /// Creates a palette by parsing every entry of <paramref name="hexColors" /> as a color and taking the first ten.
    /// </summary>
    /// <param name="hexColors">
    /// At least ten color strings, each in a form that <c>Color.Parse</c> accepts. A shorter list throws when it is
    /// indexed.
    /// </param>
    public static ColorMap FromColors(IReadOnlyList<string> hexColors)
    {
        var colors = new Color[hexColors.Count];
        for (var i = 0; i < hexColors.Count; i++)
        {
            colors[i] = Color.Parse(hexColors[i]);
        }
        return FromColors(colors);
    }
}
