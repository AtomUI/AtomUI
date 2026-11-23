using AtomUI.Media;
using AtomUI.Utils;
using Avalonia.Media;
using Avalonia.Styling;

namespace AtomUI.Theme.Derivation;

public static class PaletteGenerator
{
   /// <summary>
   /// 色相阶梯
   /// </summary>
   public const int HUE_STEP = 2;

   /// <summary>
   /// 饱和度阶梯，浅色部分
   /// </summary>
   public const float SATURATION_STEP1 = 0.16f;

   /// <summary>
   /// 饱和度阶梯，深色部分
   /// </summary>
   public const float SATURATION_STEP2 = 0.05f;

   /// <summary>
   /// 亮度阶梯，浅色部分
   /// </summary>
   public const float BRIGHTNESS_STEP1 = 0.05f;

   /// <summary>
   /// 亮度阶梯，深色部分
   /// </summary>
   public const float BRIGHTNESS_STEP2 = 0.15f;

   /// <summary>
   /// 浅色数量，主色上
   /// </summary>
   public const int LIGHT_COLOR_COUNT = 5;

   /// <summary>
   /// 深色数量，主色下
   /// </summary>
   public const int DARK_COLOR_COUNT = 4;

    private static readonly DarkColorMapItem[] sm_darkColorMap;
    private static readonly Color s_defaultDarkBackgroundColor;

    static PaletteGenerator()
    {
        s_defaultDarkBackgroundColor = Color.Parse("#141414");
        sm_darkColorMap =
        [
            new() { Index = 7, Opacity = 0.15f },
            new() { Index = 6, Opacity = 0.25f },
            new() { Index = 5, Opacity = 0.3f },
            new() { Index = 5, Opacity = 0.45f },
            new() { Index = 5, Opacity = 0.65f },
            new() { Index = 5, Opacity = 0.85f },
            new() { Index = 4, Opacity = 0.9f },
            new() { Index = 3, Opacity = 0.95f },
            new() { Index = 2, Opacity = 0.97f },
            new() { Index = 1, Opacity = 0.98f }
        ];
    }

    public static IReadOnlyList<Color> GeneratePalette(Color color, PaletteGenerateOption? option = null)
    {
        var patterns = new List<Color>(LIGHT_COLOR_COUNT + 1 + DARK_COLOR_COUNT);
        var hsvColor = color.ToHsv();
        for (var i = LIGHT_COLOR_COUNT; i > 0; --i)
        {
            var rgbColor = HsvColor.ToRgb(GetHsvHue(hsvColor, i, true),
                GetHsvSaturation(hsvColor, i, true),
                GetHsvValue(hsvColor, i, true));
            patterns.Add(rgbColor);
        }

        patterns.Add(color);
        for (var i = 1; i <= DARK_COLOR_COUNT; ++i)
        {
            var rgbColor = HsvColor.ToRgb(GetHsvHue(hsvColor, i, false),
                GetHsvSaturation(hsvColor, i, false),
                GetHsvValue(hsvColor, i, false));
            patterns.Add(rgbColor);
        }

        // dark theme patterns
        if (option?.ThemeVariant == ThemeVariant.Dark)
        {
            var darkPatterns = new List<Color>(sm_darkColorMap.Length);
            var color1       = option.BackgroundColor ?? s_defaultDarkBackgroundColor;
            foreach (var entry in sm_darkColorMap)
            {
                var color2 = patterns[entry.Index];
                var darkColorRgb = MixRgbF(new RgbF(color1.GetRedF(), color1.GetGreenF(), color1.GetBlueF()),
                    new RgbF(color2.GetRedF(), color2.GetGreenF(), color2.GetBlueF()),
                    entry.Opacity * 100);
                darkPatterns.Add(ColorUtils.FromRgbF(1.0, darkColorRgb.R, darkColorRgb.G, darkColorRgb.B));
            }

            return darkPatterns;
        }

        return patterns;
    }

    private static RgbF MixRgbF(RgbF rgb1, RgbF rgb2, double amount)
    {
        var p = amount / 100;
        var r = (rgb2.R - rgb1.R) * p + rgb1.R;
        var g = (rgb2.G - rgb1.G) * p + rgb1.G;
        var b = (rgb2.B - rgb1.B) * p + rgb1.B;
        return new RgbF(r, g, b);
    }

    private static double GetHsvHue(HsvColor hsvColor, int index, bool isLight)
    {
        double hue;
        // 根据色相不同，色相转向不同
        if (Math.Round(hsvColor.H) >= 60d && Math.Round(hsvColor.H) <= 240d)
        {
            hue = isLight ? Math.Round(hsvColor.H) - HUE_STEP * index : Math.Round(hsvColor.H) + HUE_STEP * index;
        }
        else
        {
            hue = isLight ? Math.Round(hsvColor.H) + HUE_STEP * index : Math.Round(hsvColor.H) - HUE_STEP * index;
        }

        if (hue < 0)
        {
            hue += 360d;
        }
        else if (MathUtils.GreaterThanOrClose(hue, 360d))
        {
            hue -= 360d;
        }

        return hue;
    }

    private static double GetHsvSaturation(HsvColor hsvColor, int index, bool isLight)
    {
        // grey color don't change saturation
        if (MathUtils.IsZero(hsvColor.H) && MathUtils.IsZero(hsvColor.S))
        {
            return hsvColor.S;
        }

        double saturation = 0;
        if (isLight)
        {
            saturation = hsvColor.S - SATURATION_STEP1 * index;
        }
        else if (index == DARK_COLOR_COUNT)
        {
            saturation = hsvColor.S + SATURATION_STEP1;
        }
        else
        {
            saturation = hsvColor.S + SATURATION_STEP2 * index;
        }

        // 边界值修正
        saturation = Math.Min(saturation, 1d);
        // 第一格的 s 限制在 0.06-0.1 之间
        if (isLight && index == LIGHT_COLOR_COUNT && saturation > 0.1d)
        {
            saturation = 0.1d;
        }

        saturation = Math.Max(saturation, 0.06d);
        return MathUtils.RoundToFixedPoint(saturation, 2);
    }

    private static double GetHsvValue(HsvColor hsvColor, int index, bool isLight)
    {
        double value;
        if (isLight)
        {
            value = hsvColor.V + BRIGHTNESS_STEP1 * index;
        }
        else
        {
            value = hsvColor.V - BRIGHTNESS_STEP2 * index;
        }

        value = Math.Min(value, 1d);
        return MathUtils.RoundToFixedPoint(value, 2);
        ;
    }

    internal struct DarkColorMapItem
    {
        public int Index { get; set; }
        public double Opacity { get; set; }
    }
}

internal struct RgbF
{
    public RgbF(double r, double g, double b)
    {
        R = r;
        G = g;
        B = b;
    }

    public double R { get; set; }
    public double G { get; set; }
    public double B { get; set; }
}

public class PaletteGenerateOption
{
    public ThemeVariant ThemeVariant { get; set; } = ThemeVariant.Light;
    public Color? BackgroundColor { get; set; }
}
