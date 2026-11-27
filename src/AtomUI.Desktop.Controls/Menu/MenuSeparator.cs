using AtomUI.Media;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

using AvaloniaSeparator = Avalonia.Controls.Separator;

/// <summary>
/// A separator line inside a menu. The host menu creates one for each <see cref="MenuSeparatorData" /> entry.
/// The control has no template: <see cref="Render" /> paints the line itself.
/// </summary>
public class MenuSeparator : AvaloniaSeparator
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<MenuSeparator, Orientation>(nameof(Orientation), Orientation.Horizontal);

    /// <summary>
    /// Identifies the <see cref="LineWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> LineWidthProperty =
        AvaloniaProperty.Register<MenuSeparator, double>(nameof(LineWidth), 1);

    /// <summary>
    /// Thickness of the separator line in device-independent units. With layout rounding enabled the thickness is
    /// adjusted for the layout scale before the pen is created. The MenuSeparator theme sets it from
    /// <c>MenuSeparatorToken.LineWidth</c>; a local value overrides that, and clearing it returns to the token.
    /// </summary>
    public double LineWidth
    {
        get => GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }

    /// <summary>
    /// The orientation assigned by the host that creates the separator. <see cref="Menu" /> and
    /// <see cref="ContextMenu" /> set <c>Vertical</c>; <see cref="MenuItem" />, <see cref="MenuItemGroup" /> and
    /// <see cref="MenuFlyoutPresenter" /> set <c>Horizontal</c>. The value does not affect rendering:
    /// <see cref="Render" /> always draws a horizontal line.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    #endregion

    private IPen? _linePen;

    static MenuSeparator()
    {
        AffectsRender<MenuSeparator>(LineWidthProperty, BorderBrushProperty, UseLayoutRoundingProperty);
    }

    /// <summary>
    /// Draws one horizontal line through the vertical center of the control, from its left edge to its right edge.
    /// The pen is built from <c>BorderBrush</c> and <see cref="LineWidth" />; no line is drawn while no pen can be
    /// created from them. <see cref="Orientation" /> is ignored.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        var lineWidth = BorderUtils.BuildRenderScaleAwareThickness(this, LineWidth);
        PenUtils.TryModifyOrCreate(ref _linePen, (IBrush?)BorderBrush, lineWidth);
        var offsetY = Bounds.Height / 2.0;
        if (_linePen is not null)
        {
            context.DrawLine(_linePen, new Point(0, offsetY), new Point(Bounds.Right, offsetY));
        }
    }
}
