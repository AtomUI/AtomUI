using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class ButtonSpinnerToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 历史输入框宽度资源；内置布局不读取，保留默认值及配置/资源兼容。
    /// </summary>
    public double ControlWidth { get; set; }

    /// <summary>
    /// 操作按钮宽度
    /// </summary>
    public double HandleWidth { get; set; }

    /// <summary>
    /// 操作按钮图标大小
    /// </summary>
    public double HandleIconSize { get; set; }

    /// <summary>
    /// 操作按钮背景色
    /// </summary>
    public Color HandleBg { get; set; }

    /// <summary>
    /// 操作按钮激活背景色
    /// </summary>
    public Color HandleActiveBg { get; set; }

    /// <summary>
    /// 操作按钮悬浮颜色
    /// </summary>
    public Color HandleHoverColor { get; set; }

    /// <summary>
    /// 操作按钮边框颜色
    /// </summary>
    public Color HandleBorderColor { get; set; }

    /// <summary>
    /// 面性变体操作按钮背景色
    /// </summary>
    public Color FilledHandleBg { get; set; }

    [ControlTokenDefault(nameof(ControlWidth))]
    private static double DefaultForControlWidth(in ControlTokenDefaultContext context) => 90;

    [ControlTokenDefault(nameof(HandleWidth))]
    private static double DefaultForHandleWidth(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightSM;

    [ControlTokenDefault(nameof(HandleIconSize))]
    private static double DefaultForHandleIconSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSize / 2;

    [ControlTokenDefault(nameof(HandleBg))]
    private static Color DefaultForHandleBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(HandleActiveBg))]
    private static Color DefaultForHandleActiveBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillAlter;

    [ControlTokenDefault(nameof(HandleHoverColor))]
    private static Color DefaultForHandleHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(HandleBorderColor))]
    private static Color DefaultForHandleBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBorder;

    [ControlTokenDefault(nameof(FilledHandleBg), nameof(HandleBg))]
    private static Color DefaultForFilledHandleBg(in ControlTokenDefaultContext context, Color handleBg) =>
        ColorUtils.OnBackground(context.Global.ColorFillSecondary, handleBg);
}
