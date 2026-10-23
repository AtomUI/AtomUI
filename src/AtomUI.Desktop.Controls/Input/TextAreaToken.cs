using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class TextAreaToken : AbstractControlDesignToken, IControlTokenDefaults
{
    /// <summary>
    /// Resize 指示器颜色
    /// </summary>
    public Color ResizeIndicatorLineColor { get; set; }
    
    /// <summary>
    /// Resize 指示器大小
    /// </summary>
    public double ResizeHandleSize { get; set; }
    
    /// <summary>
    /// 输入框内边距
    /// </summary>
    public Thickness RightAddOnPadding { get; set; }

    /// <summary>
    /// 小号输入框内边距
    /// </summary>
    public Thickness RightAddOnPaddingSM { get; set; }

    /// <summary>
    /// 大号输入框内边距
    /// </summary>
    public Thickness RightAddOnPaddingLG { get; set; }

    /// <summary>
    /// 左侧内部附加内容与输入内容之间的间距。
    /// </summary>
    public Thickness LeftInnerAddOnMargin { get; set; }

    [ControlTokenDefault(nameof(ResizeIndicatorLineColor))]
    private static Color DefaultForResizeIndicatorLineColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDescription;

    [ControlTokenDefault(nameof(ResizeHandleSize))]
    private static double DefaultForResizeHandleSize(in ControlTokenDefaultContext context) =>
        context.Global.SizeXS;

    [ControlTokenDefault(nameof(RightAddOnPadding))]
    private static Thickness DefaultForRightAddOnPadding(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyPaddingSM - context.Global.LineWidth, 0);

    [ControlTokenDefault(nameof(RightAddOnPaddingSM))]
    private static Thickness DefaultForRightAddOnPaddingSM(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.ControlPaddingHorizontalSM - context.Global.LineWidth, 0);

    [ControlTokenDefault(nameof(RightAddOnPaddingLG))]
    private static Thickness DefaultForRightAddOnPaddingLG(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.ControlPaddingHorizontal - context.Global.LineWidth, 0);

    [ControlTokenDefault(nameof(LeftInnerAddOnMargin))]
    private static Thickness DefaultForLeftInnerAddOnMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyMarginXXS, 0);
}
