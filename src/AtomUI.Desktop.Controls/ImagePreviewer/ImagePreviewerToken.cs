using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class ImagePreviewerToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 预览操作图标大小
    /// Size of preview operation icon
    /// </summary>
    public double PreviewOperationSize { get; set; }
    
    /// <summary>
    /// 预览操作图标颜色
    /// Color of preview operation icon
    /// </summary>
    public Color PreviewOperationColor { get; set; }
    
    /// <summary>
    /// 预览操作图标悬浮颜色
    /// Color of hovered preview operation icon
    /// </summary>
    public Color PreviewOperationHoverColor { get; set; }
    
    /// <summary>
    /// 预览操作图标禁用颜色
    /// Disabled color of preview operation icon
    /// </summary>
    public Color PreviewOperationColorDisabled { get; set; }
    
    /// <summary>
    /// 预览切换按钮尺寸
    /// Size of preview switch button
    /// </summary>
    public double ImagePreviewSwitchSize { get; set; }
    
    /// <summary>
    /// 封面的背景颜色
    /// </summary>
    public Color MaskBgColor { get; set; }
    
    /// <summary>
    /// 预览窗口最小宽度
    /// </summary>
    public double DialogMinWidth { get; set; }
    
    /// <summary>
    /// 预览窗口最小高度
    /// </summary>
    public double DialogMinHeight { get; set; }
    
    /// <summary>
    /// 预览窗口标题栏背景色
    /// </summary>
    public Color TitleBarBackgroundColor { get; set; }
    
    /// <summary>
    /// 浮动导航按钮背景颜色
    /// </summary>
    public Color NavButtonBgColor { get; set; }
    
    /// <summary>
    /// 浮动导航按钮 Hover 背景颜色
    /// </summary>
    public Color NavButtonBgHoverColor { get; set; }
    
    /// <summary>
    /// 浮动工具栏内间距
    /// </summary>
    public Thickness FloatToolbarPadding { get; set; }
    
    /// <summary>
    /// 浮动工具栏索引指示器内间距
    /// </summary>
    public Thickness FloatToolbarIndicatorPadding { get; set; }
    
    /// <summary>
    /// 封面默认的宽度
    /// </summary>
    public double CoverImageWidth { get; set; }
    
    [ControlTokenDefault(nameof(PreviewOperationColor))]
    private static Color DefaultForPreviewOperationColor(in ControlTokenDefaultContext context) => context.Global.ColorTextLightSolid.SetAlphaF(0.65);

    [ControlTokenDefault(nameof(PreviewOperationHoverColor))]
    private static Color DefaultForPreviewOperationHoverColor(in ControlTokenDefaultContext context) => context.Global.ColorTextLightSolid.SetAlphaF(0.85);

    [ControlTokenDefault(nameof(PreviewOperationColorDisabled))]
    private static Color DefaultForPreviewOperationColorDisabled(in ControlTokenDefaultContext context) => context.Global.ColorTextLightSolid.SetAlphaF(0.25);

    [ControlTokenDefault(nameof(PreviewOperationSize))]
    private static double DefaultForPreviewOperationSize(in ControlTokenDefaultContext context) => context.Global.FontSizeIcon * 1.5;

    [ControlTokenDefault(nameof(ImagePreviewSwitchSize))]
    private static double DefaultForImagePreviewSwitchSize(in ControlTokenDefaultContext context) => context.Global.ControlHeightLG;

    [ControlTokenDefault(nameof(MaskBgColor))]
    private static Color DefaultForMaskBgColor(in ControlTokenDefaultContext context) => ColorUtils.FromRgbF(0.3, 0, 0, 0);

    [ControlTokenDefault(nameof(DialogMinWidth))]
    private static double DefaultForDialogMinWidth(in ControlTokenDefaultContext context) => 710;

    [ControlTokenDefault(nameof(DialogMinHeight))]
    private static double DefaultForDialogMinHeight(in ControlTokenDefaultContext context) => 240;

    [ControlTokenDefault(nameof(CoverImageWidth))]
    private static double DefaultForCoverImageWidth(in ControlTokenDefaultContext context) => 200;

    [ControlTokenDefault(nameof(TitleBarBackgroundColor))]
    private static Color DefaultForTitleBarBackgroundColor(in ControlTokenDefaultContext context) => context.Appearance == ThemeAppearance.Dark
            ? context.Global.ColorBorderSecondary
            : context.Global.ColorBorderSecondary.Darken(1);

    [ControlTokenDefault(nameof(NavButtonBgColor))]
    private static Color DefaultForNavButtonBgColor(in ControlTokenDefaultContext context) => context.Global.ColorBgMask.SetAlphaF(0.1);

    [ControlTokenDefault(nameof(NavButtonBgHoverColor))]
    private static Color DefaultForNavButtonBgHoverColor(in ControlTokenDefaultContext context) => context.Global.ColorBgMask.SetAlphaF(0.2);

    [ControlTokenDefault(nameof(FloatToolbarPadding))]
    private static Thickness DefaultForFloatToolbarPadding(in ControlTokenDefaultContext context) => new(context.Global.UniformlyPaddingLG, 0);

    [ControlTokenDefault(nameof(FloatToolbarIndicatorPadding))]
    private static Thickness DefaultForFloatToolbarIndicatorPadding(in ControlTokenDefaultContext context) => new(context.Global.UniformlyPaddingXS, 0);
}
