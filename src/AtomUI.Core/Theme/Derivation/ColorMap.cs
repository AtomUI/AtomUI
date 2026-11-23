using Avalonia.Media;

namespace AtomUI.Theme.Derivation;

public record ColorMap
{
    public Color Color1 { get; set; }
    public Color Color2 { get; set; }
    public Color Color3 { get; set; }
    public Color Color4 { get; set; }
    public Color Color5 { get; set; }
    public Color Color6 { get; set; }
    public Color Color7 { get; set; }
    public Color Color8 { get; set; }
    public Color Color9 { get; set; }
    public Color Color10 { get; set; }

    public static ColorMap FromColors(IReadOnlyList<Color> colors)
    {
        // TODO 在这里要检查数组的大小，不合适就抛出异常
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
