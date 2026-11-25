using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ToolTipToken : AbstractControlDesignToken
{

    /// <summary>Maximum width of the tooltip, which constrains wrapping of its text.</summary>
    public partial double ToolTipMaxWidth { get; set; }

    /// <summary>
    /// 提示文本的字号
    /// Font size of the tooltip text.
    /// </summary>
    public partial double ToolTipFontSize { get; set; }

    /// <summary>
    /// 提示框的最小高度
    /// Minimum height of the tooltip box.
    /// </summary>
    public partial double ToolTipMinHeight { get; set; }

    /// <summary>Tooltip content foreground on <see cref="ToolTipBackground"/>.</summary>
    public partial Color ToolTipColor { get; set; }

    /// <summary>Fill behind the tooltip content. Pair with <see cref="ToolTipColor"/>.</summary>
    public partial Color ToolTipBackground { get; set; }

    /// <summary>Corner radius of the tooltip surface.</summary>
    public partial CornerRadius ToolTipCornerRadius { get; set; }

    /// <summary>Space inside the tooltip frame around its content.</summary>
    public partial Thickness ContentPadding { get; set; }
    
    /// <summary>
    /// 弹出与收起的缩放动效时长（antd Tooltip 使用 zoom-big-fast，即 motionDurationFast）
    /// </summary>
    public partial TimeSpan MotionDuration { get; set; }

    /// <summary>
    /// The shadow of the tooltip popup, independent of the menu popup's. Defaults to what the tooltip popup drew
    /// before (the popup token's <c>BoxShadowsSecondary</c>); a theme with a shadowless tooltip (shadcn) sets it
    /// to empty.
    /// </summary>
    public partial BoxShadows ToolTipShadows { get; set; }

    /// <summary>
    /// The gap between the anchor and the tooltip (the arrow's tip when it shows one), used while neither the host
    /// nor the tooltip sets <c>ToolTip.MarginToAnchor</c>. Defaults to 4, the attached property's default.
    /// </summary>
    public partial double MarginToAnchor { get; set; }

    /// <summary>
    /// Radius of the tooltip arrow's rounded tip; defaults to the arrow box's (<c>BorderRadiusXS</c>).
    /// </summary>
    public partial double ArrowTipRadius { get; set; }

    /// <summary>
    /// Radius of the arcs joining the tooltip arrow to the box edge; defaults to the arrow box's (<c>BorderRadiusOuter</c>).
    /// </summary>
    public partial double ArrowBaseRadius { get; set; }

    /// <summary>
    /// The size of the tooltip's arrow. Defaults to the arrow box's own default (<c>SizePopupArrow</c>); a theme
    /// whose tooltip arrow is smaller than its popover arrow (PrimeNG <c>tooltip.gutter</c>) sets it here.
    /// </summary>
    public partial double ArrowSize { get; set; }

    /// <summary>
    /// The width of the tooltip arrow's outline. Defaults to the arrow box's default of 1; 0 draws a plain filled
    /// arrow (PrimeNG's tooltip arrow is a border triangle without an outline).
    /// </summary>
    public partial double ArrowStrokeThickness { get; set; }
    
}
