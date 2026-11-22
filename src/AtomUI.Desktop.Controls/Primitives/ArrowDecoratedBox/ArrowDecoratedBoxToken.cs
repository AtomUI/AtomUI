using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ArrowDecoratedBoxToken : AbstractControlDesignToken
{

    /// <summary>
    /// 箭头三角形大小
    /// </summary>
    public partial double ArrowSize { get; set; }

    /// <summary>
    /// 箭头尖角圆角半径
    /// Radius of the arrow's rounded tip (antd <c>borderRadiusXS</c>)
    /// </summary>
    public partial double ArrowTipRadius { get; set; }

    /// <summary>
    /// 箭头与内容框衔接处的圆弧半径
    /// Radius of the arcs joining the arrow's sides to the box edge (antd <c>borderRadiusOuter</c>)
    /// </summary>
    public partial double ArrowBaseRadius { get; set; }

    /// <summary>
    /// 默认的内边距
    /// </summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>
    /// 箭头描边颜色
    /// </summary>
    public partial Color ArrowStrokeColor { get; set; }

    /// <summary>
    /// 箭头描边粗细
    /// </summary>
    public partial double ArrowStrokeThickness { get; set; }

    /// <summary>
    /// Border colour of the content box; with a non-zero <see cref="ContentBorderThickness"/> it outlines the box and
    /// the arrow as one shape.
    /// </summary>
    public partial Color ContentBorderColor { get; set; }

    /// <summary>
    /// Border width of the content box (0 by default: no border). A non-zero width draws one outline around the box
    /// and the arrow.
    /// </summary>
    public partial Thickness ContentBorderThickness { get; set; }

    /// <summary>
    /// Minimum height of the box.
    /// </summary>
    public partial double ContentMinHeight { get; set; }

    public ArrowDecoratedBoxToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        // antd genRoundedArrow: sizePopupArrow wide, half as high, borderRadiusXS at the tip and borderRadiusOuter
        // where the sides meet the box.
        ArrowSize            = EffectiveGlobalToken.SizePopupArrow;
        ArrowTipRadius       = EffectiveGlobalToken.BorderRadiusXS.TopLeft;
        ArrowBaseRadius      = EffectiveGlobalToken.BorderRadiusOuter.TopLeft;
        ContentPadding       = EffectiveGlobalToken.PaddingXS;
        ArrowStrokeColor     = ColorUtils.FromRgbF(0.07, 0, 0, 0);
        ArrowStrokeThickness = 1;
        ContentBorderColor     = EffectiveGlobalToken.ColorBorderSecondary;
        ContentBorderThickness = default;
        ContentMinHeight       = EffectiveGlobalToken.ControlHeight;
    }

}
