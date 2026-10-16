using AtomUI.Media;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

using AvaloniaSeparator = Avalonia.Controls.Separator;

public class MenuSeparator : AvaloniaSeparator
{
    #region 公共属性定义
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<MenuSeparator, Orientation>(nameof(Orientation), Orientation.Horizontal);

    public static readonly StyledProperty<double> LineWidthProperty =
        AvaloniaProperty.Register<MenuSeparator, double>(nameof(LineWidth), 1);

    public double LineWidth
    {
        get => GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }

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
