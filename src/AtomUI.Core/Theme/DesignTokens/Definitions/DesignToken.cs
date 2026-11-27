using Avalonia;
using Avalonia.Media;
using AtomUI.Theme.Derivation;

namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// The global design tokens shared by every control: the Seed values a theme is built from, the Map values a
/// design language derives from them, and the Alias values derived for a purpose of use. Control token rules
/// read these values. The tokens are declared in partial files, one per group.
/// </summary>
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
    /// The color palette of each preset primary color, stored by <see cref="SetColorPalette" />. It is not a
    /// token, so the token source generator leaves it out.
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

    /// <summary>
    /// Returns the built-in preset primary color of the given type. The result is the same for every instance
    /// and does not depend on <see cref="ColorPrimary" />.
    /// </summary>
    /// <param name="colorType">
    /// The type of the preset to return.
    /// </param>
    public PresetPrimaryColor GetPresetPrimaryColor(PresetColorType colorType)
    {
        return s_defaultPresetColors[colorType];
    }

    /// <summary>
    /// Stores the palette of one preset primary color, replacing the palette stored earlier for it.
    /// </summary>
    /// <param name="primaryColor">
    /// The preset the palette belongs to.
    /// </param>
    /// <param name="colorMap">
    /// The palette generated from the color of that preset.
    /// </param>
    public void SetColorPalette(PresetPrimaryColor primaryColor, ColorMap colorMap)
    {
        ColorPalettes[primaryColor] = colorMap;
    }

    /// <summary>
    /// Returns the palette stored for the given preset primary color, or <see langword="null" /> when no palette
    /// was stored for it.
    /// </summary>
    /// <param name="primaryColor">
    /// The preset whose palette to return.
    /// </param>
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
            MotionDurationEnter    = TimeSpan.Zero;
            MotionDurationLeave    = TimeSpan.Zero;
            StateTransitionDuration     = TimeSpan.Zero;
            StateTransitionDurationFast = TimeSpan.Zero;
            StateTransitionDurationSlow = TimeSpan.Zero;
            StateTransitionDurationPress = TimeSpan.Zero;
            WaveFadeDuration = TimeSpan.Zero;
        }
        if (!EnableInteractionEffect)
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
