using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the arrow-decorated box: the geometry and outline of its arrow, and the padding, border and
/// minimum height of its content box.
/// </summary>
[ControlDesignToken]
public sealed partial class ArrowDecoratedBoxToken : AbstractControlDesignToken
{

    /// <summary>
    /// Size of the triangular arrow.
    /// </summary>
    public partial double ArrowSize { get; set; }

    /// <summary>
    /// Corner radius of the arrow's tip (antd <c>borderRadiusXS</c>).
    /// </summary>
    public partial double ArrowTipRadius { get; set; }

    /// <summary>
    /// Radius of the arcs joining the arrow's sides to the box edge (antd <c>borderRadiusOuter</c>).
    /// </summary>
    public partial double ArrowBaseRadius { get; set; }

    /// <summary>
    /// Padding inside the decorated box.
    /// </summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>
    /// Color of the arrow outline.
    /// </summary>
    public partial Color ArrowStrokeColor { get; set; }

    /// <summary>
    /// Thickness of the arrow outline.
    /// </summary>
    public partial double ArrowStrokeThickness { get; set; }

    /// <summary>
    /// Border color of the content box; with a non-zero <see cref="ContentBorderThickness" /> it outlines the box and
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

}
