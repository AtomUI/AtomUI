using Avalonia;
using Avalonia.Media;
using AtomUI.Theme.Derivation;

namespace AtomUI.Theme.DesignTokens;

public partial class DesignToken : AbstractDesignToken
{
    private const int PresetColorCount = 14;
    private static readonly IReadOnlyDictionary<PresetColorType, PresetPrimaryColor> s_defaultPresetColors =
        BuildDefaultPresetColors();
    private static readonly Color s_defaultColorPrimary = Color.FromUInt32(0xFF1677ffu);
    private static readonly Color s_defaultColorSuccess = Color.FromUInt32(0xFF52c41au);
    private static readonly Color s_defaultColorWarning = Color.FromUInt32(0xFFfaad14u);
    private static readonly Color s_defaultColorError = Color.FromUInt32(0xFFff4d4fu);
    // FontFamily parsing creates an immutable description; font resolution remains with FontManager.
    private static readonly FontFamily DefaultFontFamily = new FontFamily("fonts:AlibabaSans#Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, $Default");

    /// <summary>
    /// 现在这里的实现是写死的主色，后面是不是可以读取配置
    /// </summary>

    [NotTokenDefinition] public IDictionary<PresetPrimaryColor, ColorMap> ColorPalettes { get; set; }

    public DesignToken() : this(initializeSeed: true)
    {
    }

    internal DesignToken(bool initializeSeed)
    {
        if (initializeSeed)
        {
            InitSeedTokenValues();
        }
        ColorPalettes = new Dictionary<PresetPrimaryColor, ColorMap>(PresetColorCount);
    }

    private void InitSeedTokenValues()
    {
        ColorPrimary     = s_defaultColorPrimary;
        ColorSuccess     = s_defaultColorSuccess;
        ColorWarning     = s_defaultColorWarning;
        ColorError       = s_defaultColorError;
        ColorInfo        = s_defaultColorPrimary;
        FontFamily       = DefaultFontFamily;
        BorderRadius     = new CornerRadius(6);
        ColorTransparent = Colors.Transparent;
    }

    public PresetPrimaryColor GetPresetPrimaryColor(PresetColorType colorType)
    {
        return s_defaultPresetColors[colorType];
    }

    public void SetColorPalette(PresetPrimaryColor primaryColor, ColorMap colorMap)
    {
        ColorPalettes[primaryColor] = colorMap;
    }

    public ColorMap? GetColorPalette(PresetPrimaryColor primaryColor)
    {
        ColorMap? value;
        if (ColorPalettes.TryGetValue(primaryColor, out value))
        {
            return value;
        }

        return null;
    }

    internal void NormalizeFinalBehavior()
    {
        if (!EnableMotion)
        {
            MotionDurationFast     = TimeSpan.Zero;
            MotionDurationMid      = TimeSpan.Zero;
            MotionDurationSlow     = TimeSpan.Zero;
            MotionDurationVerySlow = TimeSpan.Zero;
        }
        if (!EnableWaveSpirit)
        {
            WaveAnimationRange = 0;
            WaveStartOpacity   = 0;
        }
    }

    private static IReadOnlyDictionary<PresetColorType, PresetPrimaryColor> BuildDefaultPresetColors()
    {
        var colors = new Dictionary<PresetColorType, PresetPrimaryColor>(PresetColorCount);
        foreach (var color in PresetPrimaryColor.AllColorTypes())
        {
            colors[color.Type] = color;
        }

        return colors;
    }
}
