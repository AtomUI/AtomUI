using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Rendering.Composition;

namespace AtomUI.Controls;

/// <summary>
/// Render-thread side of the skeleton shimmer. One instance per shimmer host draws a single band that sweeps across
/// every block of the host: each block is clipped to its rounded rectangle and filled with the band brush shifted to
/// the band's current position. Outside the band the brush pads with the base color, so a block reads as its normal
/// background except where the band passes.
/// The handler reads nothing but the messages it receives; per frame it only computes value types and pushes struct
/// states, so frames allocate no managed memory and never touch layout.
/// </summary>
internal sealed class SkeletonShimmerVisualHandler : CompositionCustomVisualHandler
{
    /// <summary>
    /// Width of the band brush, as a multiple of the travel width W.
    /// </summary>
    internal const double BandWidthFactor = 1.52;

    /// <summary>
    /// Where the highlight sits inside the band brush, as a fraction of the band width.
    /// </summary>
    internal const double PeakOffset = 0.316;

    private RoundedRect[] _blocks = [];
    private ImmutableLinearGradientBrush? _brush;
    private Color _from;
    private Color _to;
    private double _width;
    private TimeSpan _duration;
    private Easing? _easing;
    private bool _isRunning;
    private bool _isFrameRequested;
    private TimeSpan _startTime;

    internal bool IsRunning => _isRunning;

    internal RoundedRect[] Blocks => _blocks;

    internal double BandWidth => _width;

    /// <summary>
    /// True while a next-frame callback is pending. Frames are requested only while the band is running with blocks to
    /// draw; a request that is already pending stays pending when the band stops or a scene without blocks arrives,
    /// until the next animation frame update.
    /// </summary>
    internal bool IsFrameRequested => _isFrameRequested;

    internal int FrameRequestCount { get; private set; }

    /// <summary>
    /// Left edge of the band <paramref name="seconds" /> after the start: <c>t = (seconds mod D) / D</c>, left =
    /// <c>-2W + 3W · easing(t)</c>. With the 1.52W band and its highlight at 0.316, the highlight travels from
    /// -1.52W to +1.48W each cycle.
    /// </summary>
    internal static double SampleBandLeft(double seconds, double width, TimeSpan duration, Easing easing)
    {
        var period = duration.TotalSeconds;
        if (!(period > 0) || !double.IsFinite(seconds))
        {
            return -2 * width;
        }
        var t = seconds % period / period;
        if (t < 0)
        {
            t += 1;
        }
        return -2 * width + 3 * width * easing.Ease(t);
    }

    /// <summary>
    /// The band brush for a travel width: base color at 0, highlight at <see cref="PeakOffset" />, base color
    /// at 1, over <c>1.52W</c> in absolute units and padded beyond.
    /// </summary>
    internal static ImmutableLinearGradientBrush CreateBandBrush(Color from, Color to, double width)
    {
        var stops = new[]
        {
            new ImmutableGradientStop(0, from),
            new ImmutableGradientStop(PeakOffset, to),
            new ImmutableGradientStop(1, from)
        };
        return new ImmutableLinearGradientBrush(
            stops,
            spreadMethod: GradientSpreadMethod.Pad,
            startPoint: new RelativePoint(0, 0, RelativeUnit.Absolute),
            endPoint: new RelativePoint(Math.Max(width * BandWidthFactor, 1), 0, RelativeUnit.Absolute));
    }

    /// <summary>
    /// Draws one frame: every block clipped to its rounded rectangle and filled with the brush shifted by
    /// <paramref name="left" />.
    /// </summary>
    internal static void DrawFrame(ImmediateDrawingContext context, RoundedRect[] blocks, IImmutableBrush brush,
                                   double left)
    {
        var shift = Matrix.CreateTranslation(left, 0);
        var back  = new Vector(-left, 0);
        for (var i = 0; i < blocks.Length; i++)
        {
            var block = blocks[i];
            using (context.PushClip(block))
            using (context.PushPreTransform(shift))
            {
                context.FillRectangle(brush, block.Rect.Translate(back));
            }
        }
    }

    public override void OnMessage(object message)
    {
        switch (message)
        {
            case SkeletonShimmerScene scene:
            {
                if (_brush is null || scene.From != _from || scene.To != _to || !scene.Width.Equals(_width))
                {
                    _brush = CreateBandBrush(scene.From, scene.To, scene.Width);
                }
                _blocks   = scene.Blocks;
                _from     = scene.From;
                _to       = scene.To;
                _width    = scene.Width;
                _duration = scene.Duration;
                _easing   = scene.Easing;
                Invalidate();
                // A scene with blocks re-arms the frames a running band stopped requesting while it had none.
                RequestFrameIfDrawing();
                break;
            }
            case SkeletonShimmerStart:
            {
                _isRunning = true;
                _startTime = CompositionNow;
                Invalidate();
                RequestFrameIfDrawing();
                break;
            }
            case SkeletonShimmerStop:
            {
                _isRunning = false;
                Invalidate();
                break;
            }
        }
    }

    public override void OnAnimationFrameUpdate()
    {
        _isFrameRequested = false;
        if (!_isRunning || _blocks.Length == 0)
        {
            return;
        }
        Invalidate();
        RequestFrameIfDrawing();
    }

    // Frames are requested only while running with something to draw; at most one request is pending.
    private void RequestFrameIfDrawing()
    {
        if (!_isRunning || _blocks.Length == 0 || _isFrameRequested)
        {
            return;
        }
        _isFrameRequested = true;
        FrameRequestCount++;
        RegisterForNextAnimationFrameUpdate();
    }

    public override void OnRender(ImmediateDrawingContext context)
    {
        if (!_isRunning || _brush is null || _easing is null || _blocks.Length == 0)
        {
            return;
        }
        var left = SampleBandLeft((CompositionNow - _startTime).TotalSeconds, _width, _duration, _easing);
        DrawFrame(context, _blocks, _brush, left);
    }
}
