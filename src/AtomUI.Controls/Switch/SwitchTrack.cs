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
    /// Fill brush of the pill-shaped switch track.
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

    private IPen? _pen;

    static SwitchTrack()
    {
        AffectsRender<SwitchTrack>(BackgroundProperty, BorderBrushProperty, BorderThicknessProperty);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(Bounds.Size);
        if (BorderThickness <= 0 || BorderBrush is null)
        {
            context.DrawPilledRect(Background, null, bounds);
            return;
        }

        PenUtils.TryModifyOrCreate(ref _pen, BorderBrush, BorderThickness);
        context.DrawPilledRect(Background, _pen, bounds.Deflate(BorderThickness / 2));
    }
}
