using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;

namespace AtomUI.Controls;

/// <summary>
/// Base class of the painters that draw the click wave effect of a control; <c>WaveSpiritDecorator</c> in
/// AtomUI.Controls owns the painter.
/// </summary>
/// <remarks>
/// The decorator copies its wave type, range, brush and motion settings into the painter, asks
/// <see cref="NotifyBuildSizeAnimation" /> and <see cref="NotifyBuildOpacityAnimation" /> for the key frames of a
/// wave, and calls <see cref="Paint" /> with the size and opacity of each frame. A derived painter provides the
/// wave geometry by implementing <see cref="Paint" />, <see cref="Clone" /> and
/// <see cref="NotifyBuildSizeAnimation" />. The opacity animation is shared: it is built from
/// <c>OpacityMotionDuration</c>, <c>OpacityEasingCurve</c> (linear when <see langword="null" />) and
/// <c>OriginOpacity</c>.
/// </remarks>
internal abstract class AbstractWavePainter : IWaveSpiritPainter
{
    protected static readonly Easing DefaultLinearEasing = new LinearEasing();

    public Point OriginPoint { get; set; }
    public IBrush? WaveBrush { get; set; }
    public WaveSpiritType WaveType { get; protected set; }
    public double WaveRange { get; set; }
    public TimeSpan SizeMotionDuration { get; set; }
    public TimeSpan OpacityMotionDuration { get; set; }
    public Easing? SizeEasingCurve { get; set; }
    public Easing? OpacityEasingCurve { get; set; }
    public double OriginOpacity { get; set; }

    public AbstractWavePainter(Point originPoint)
    {
        WaveType  = WaveSpiritType.RoundRectWave;
        WaveRange = 3;
        OriginPoint = originPoint;
    }

    public abstract void Paint(DrawingContext context, object newSize, double newOpacity);
    public abstract AbstractWavePainter Clone();
    public abstract void NotifyBuildSizeAnimation(Animation animation, AvaloniaProperty targetProperty);

    public virtual void NotifyBuildOpacityAnimation(Animation animation, AvaloniaProperty targetProperty)
    {
        animation.Duration = OpacityMotionDuration;
        animation.Easing   = OpacityEasingCurve ?? DefaultLinearEasing;
        animation.Children.Add(new KeyFrame
        {
            Setters =
            {
                new Setter
                {
                    Property = targetProperty,
                    Value    = OriginOpacity
                }
            },
            KeyTime = TimeSpan.FromMilliseconds(0)
        });

        animation.Children.Add(new KeyFrame
        {
            Setters =
            {
                new Setter
                {
                    Property = targetProperty,
                    Value    = 0d
                }
            },
            KeyTime = OpacityMotionDuration
        });
    }
}
