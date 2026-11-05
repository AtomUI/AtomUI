using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class RateToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// Typeface for character glyph content; retains the platform glyph default.
    /// </summary>
    public FontFamily CharacterFontFamily { get; set; } = FontFamily.Default;

    /// <summary>
    /// 星星颜色
    /// Star color
    /// </summary>
    public Color StarColor { get; set; }
    
    /// <summary>
    /// 星星尺寸
    /// Star size
    /// </summary>
    public double StarSize { get; set; }
    
    /// <summary>
    /// 小星星尺寸
    /// Small star size
    /// </summary>
    public double StarSizeSM { get; set; }
    
    /// <summary>
    /// 大星星尺寸
    /// Large star size
    /// </summary>
    public double StarSizeLG { get; set; }
    
    /// <summary>
    /// 星星悬浮时的缩放
    /// Scale of star when hover
    /// </summary>
    public double StarHoverScale { get; set; }
    
    /// <summary>
    /// 星星背景色
    /// Star background color
    /// </summary>
    public Color StarBg { get; set; }

    [ControlTokenDefault(nameof(CharacterFontFamily))]
    private static FontFamily DefaultForCharacterFontFamily(in ControlTokenDefaultContext context) =>
        FontFamily.Default;

    [ControlTokenDefault(nameof(StarColor))]
    private static Color DefaultForStarColor(in ControlTokenDefaultContext context) =>
        PresetPalettes.GetPresetPalette(PresetPrimaryColor.Yellow, context.Appearance == ThemeAppearance.Dark).Primary;

    [ControlTokenDefault(nameof(StarSize))]
    private static double DefaultForStarSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight * 0.625;

    [ControlTokenDefault(nameof(StarSizeSM))]
    private static double DefaultForStarSizeSM(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightSM * 0.625;

    [ControlTokenDefault(nameof(StarSizeLG))]
    private static double DefaultForStarSizeLG(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 0.625;

    [ControlTokenDefault(nameof(StarHoverScale))]
    private static double DefaultForStarHoverScale(in ControlTokenDefaultContext context) =>
        1.2;

    [ControlTokenDefault(nameof(StarBg))]
    private static Color DefaultForStarBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillContent;

}
