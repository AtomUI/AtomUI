using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class StepsNavigationArrow : Control
{
    #region 公共属性定义

    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<StepsNavigationArrow, IBrush?>(nameof(BorderBrush));

    public static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<StepsNavigationArrow, double>(nameof(BorderThickness), 1d);

    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    #endregion

    static StepsNavigationArrow()
    {
        AffectsRender<StepsNavigationArrow>(
            BorderBrushProperty,
            BorderThicknessProperty,
            UseLayoutRoundingProperty);
    }

    public override void Render(DrawingContext context)
    {
        var thickness = Math.Max(0, BorderThickness);
        if (BorderBrush is null || thickness <= 0)
        {
            return;
        }

        var halfThickness = thickness / 2;
        var pen           = new Pen(BorderBrush, thickness);
        var right         = Math.Max(0, Bounds.Width - halfThickness);
        var top           = halfThickness;
        var bottom        = Math.Max(top, Bounds.Height - halfThickness);

        context.DrawLine(pen, new Point(halfThickness, top), new Point(right, top));
        context.DrawLine(pen, new Point(right, top), new Point(right, bottom));
    }
}
