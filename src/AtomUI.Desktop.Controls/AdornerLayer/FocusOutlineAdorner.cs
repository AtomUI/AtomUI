using AtomUI.Controls.Primitives;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Keyboard focus outline matching Ant Design <c>genFocusOutline</c>: a stroke of
/// <see cref="OutlineThickness"/> drawn <see cref="OutlineOffset"/> outside the adorned element and
/// following its corner radius.
/// </summary>
internal sealed class FocusOutlineAdorner : Control
{
    public static readonly StyledProperty<IBrush?> OutlineBrushProperty =
        AvaloniaProperty.Register<FocusOutlineAdorner, IBrush?>(nameof(OutlineBrush));

    public static readonly StyledProperty<double> OutlineThicknessProperty =
        AvaloniaProperty.Register<FocusOutlineAdorner, double>(nameof(OutlineThickness));

    public static readonly StyledProperty<double> OutlineOffsetProperty =
        AvaloniaProperty.Register<FocusOutlineAdorner, double>(nameof(OutlineOffset));

    public IBrush? OutlineBrush
    {
        get => GetValue(OutlineBrushProperty);
        set => SetValue(OutlineBrushProperty, value);
    }

    public double OutlineThickness
    {
        get => GetValue(OutlineThicknessProperty);
        set => SetValue(OutlineThicknessProperty, value);
    }

    public double OutlineOffset
    {
        get => GetValue(OutlineOffsetProperty);
        set => SetValue(OutlineOffsetProperty, value);
    }

    private Pen? _pen;

    static FocusOutlineAdorner()
    {
        AffectsRender<FocusOutlineAdorner>(OutlineBrushProperty, OutlineThicknessProperty, OutlineOffsetProperty,
            AdornerLayer.AdornedElementProperty);
        IsHitTestVisibleProperty.OverrideDefaultValue<FocusOutlineAdorner>(false);
    }

    public FocusOutlineAdorner()
    {
        // The outline sits outside the adorned element; the default adorner clip starts at the element's
        // own bounds (TemplatedControl clips by default) and would remove it entirely.
        AdornerLayer.SetIsClipEnabled(this, false);
    }

    public override void Render(DrawingContext context)
    {
        var thickness = OutlineThickness;
        if (OutlineBrush is not { } brush || thickness <= 0)
        {
            return;
        }

        if (_pen is null || !ReferenceEquals(_pen.Brush, brush) || !MathUtils.AreClose(_pen.Thickness, thickness))
        {
            _pen = new Pen(brush, thickness);
        }

        // The pen is centered on the path, so the path runs half a stroke further out than the offset.
        var inflate      = OutlineOffset + thickness / 2;
        var cornerRadius = ResolveCornerRadius(AdornerLayer.GetAdornedElement(this));
        var rect         = new Rect(Bounds.Size).Inflate(inflate);
        var maxRadius    = Math.Min(rect.Width, rect.Height) / 2;
        context.DrawRectangle(null, _pen, new RoundedRect(rect,
            Grow(cornerRadius.TopLeft, inflate, maxRadius),
            Grow(cornerRadius.TopRight, inflate, maxRadius),
            Grow(cornerRadius.BottomRight, inflate, maxRadius),
            Grow(cornerRadius.BottomLeft, inflate, maxRadius)));
    }

    // Shapes such as circle buttons report a radius larger than half their size; clamp so the outline
    // stays a circle instead of relying on the renderer.
    private static double Grow(double radius, double inflate, double maxRadius) =>
        radius > 0 ? Math.Min(radius + inflate, maxRadius) : 0;

    private static CornerRadius ResolveCornerRadius(Visual? adorned)
    {
        return adorned switch
        {
            IFocusOutlineShapeProvider provider => provider.FocusOutlineCornerRadius,
            TemplatedControl templatedControl => templatedControl.CornerRadius,
            Border border => border.CornerRadius,
            DashedBorder dashedBorder => dashedBorder.CornerRadius,
            _ => default
        };
    }
}
