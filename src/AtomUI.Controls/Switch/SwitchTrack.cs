using AtomUI.Controls.Primitives;
using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Draws the default switch track without owning any toggle state or layout.
/// </summary>
public sealed class SwitchTrack : Control
{
    /// <summary>
    /// Identifies the <see cref="Background" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<SwitchTrack, IBrush?>(nameof(Background));

    /// <summary>
    /// Identifies the <see cref="BorderBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<SwitchTrack, IBrush?>(nameof(BorderBrush));

    /// <summary>
    /// Identifies the <see cref="BorderThickness" /> property.
    /// </summary>
    public static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<SwitchTrack, double>(nameof(BorderThickness));

    /// <summary>
    /// Identifies the <see cref="CornerRadius" /> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius?> CornerRadiusProperty =
        AvaloniaProperty.Register<SwitchTrack, CornerRadius?>(nameof(CornerRadius));

    /// <summary>
    /// Fill brush of the switch track.
    /// </summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Stroke brush drawn along the inner edge of the switch track when <see cref="BorderThickness" /> is positive.
    /// </summary>
    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Stroke width of the track border in device-independent pixels.
    /// </summary>
    public double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Corner radius of the switch track, or <see langword="null" /> to draw a full capsule pill.
    /// </summary>
    public CornerRadius? CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    private IPen? _pen;

    static SwitchTrack()
    {
        AffectsRender<SwitchTrack>(BackgroundProperty, BorderBrushProperty, BorderThicknessProperty, CornerRadiusProperty);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(Bounds.Size);
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var hasBorder = BorderThickness > 0 && BorderBrush is not null;
        if (CornerRadius is { } cornerRadius)
        {
            if (!hasBorder)
            {
                var clampedRadius = CornerRadiusUtils.ClampToSize(cornerRadius, bounds.Width, bounds.Height);
                context.DrawRectangle(Background, null, new RoundedRect(bounds, clampedRadius));
                return;
            }

            var halfStroke = BorderThickness / 2;
            var strokeRect = bounds.Deflate(halfStroke);
            if (strokeRect.Width <= 0 || strokeRect.Height <= 0)
            {
                return;
            }

            var clampedOuterRadius = CornerRadiusUtils.ClampToSize(cornerRadius, bounds.Width, bounds.Height);
            var innerRadius = new CornerRadius(
                Math.Max(0, clampedOuterRadius.TopLeft - halfStroke),
                Math.Max(0, clampedOuterRadius.TopRight - halfStroke),
                Math.Max(0, clampedOuterRadius.BottomRight - halfStroke),
                Math.Max(0, clampedOuterRadius.BottomLeft - halfStroke));
            var clampedStrokeRadius = CornerRadiusUtils.ClampToSize(innerRadius, strokeRect.Width, strokeRect.Height);
            PenUtils.TryModifyOrCreate(ref _pen, BorderBrush, BorderThickness);
            context.DrawRectangle(Background, _pen, new RoundedRect(strokeRect, clampedStrokeRadius));
            return;
        }

        if (!hasBorder)
        {
            context.DrawPilledRect(Background, null, bounds);
            return;
        }

        PenUtils.TryModifyOrCreate(ref _pen, BorderBrush, BorderThickness);
        context.DrawPilledRect(Background, _pen, bounds.Deflate(BorderThickness / 2));
    }
}
