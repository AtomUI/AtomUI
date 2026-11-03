using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class TreeViewFlyoutPresenterToken : AbstractControlDesignToken, IControlTokenDefaults
{
    

    
    /// <summary>
    /// 弹出框背景色
    /// </summary>
    public Color PopupBgColor { get; set; }

    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public Thickness PopupContentPadding { get; set; }

    /// <summary>
    /// 菜单 Popup 最小宽度
    /// </summary>
    public double PopupMinWidth { get; set; }

    /// <summary>
    /// 菜单 Popup 最大宽度
    /// </summary>
    public double PopupMaxWidth { get; set; }

    /// <summary>
    /// 菜单 Popup 最小高度
    /// </summary>
    public double PopupMinHeight { get; set; }

    /// <summary>
    /// 菜单 Popup 最大高度
    /// </summary>
    public double PopupMaxHeight { get; set; }

    [ControlTokenDefault(nameof(PopupMinWidth))]
    private static double DefaultForPopupMinWidth(in ControlTokenDefaultContext context) => 120;

    [ControlTokenDefault(nameof(PopupMaxWidth))]
    private static double DefaultForPopupMaxWidth(in ControlTokenDefaultContext context) => 800;

    [ControlTokenDefault(nameof(PopupMinHeight))]
    private static double DefaultForPopupMinHeight(in ControlTokenDefaultContext context) => context.Global.ControlHeightSM * 3;

    [ControlTokenDefault(nameof(PopupMaxHeight))]
    private static double DefaultForPopupMaxHeight(in ControlTokenDefaultContext context) => context.Global.ControlHeightSM * 30;

    [ControlTokenDefault(nameof(PopupContentPadding))]
    private static Thickness DefaultForPopupContentPadding(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyPaddingXXS, context.Global.BorderRadiusLG.TopLeft / 2);

    [ControlTokenDefault(nameof(PopupBgColor))]
    private static Color DefaultForPopupBgColor(in ControlTokenDefaultContext context) => context.Global.ColorBgElevated;
}
