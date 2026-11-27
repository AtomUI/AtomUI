using AtomUI.Animations;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Renders the default activation outline through the shared feedback host.
/// </summary>
internal sealed class AntDesignWaveEffect(bool quick) : InteractionEffect
{
    internal static readonly AntDesignWaveEffect Default = new(quick: false);
    internal static readonly AntDesignWaveEffect Quick = new(quick: true);

    public override void OnActivated(in InteractionEffectActivateContext context)
    {
        var host = (InteractionEffectHost)context.Playback;
        var brush = context.Brush ?? host.GetMetric<IBrush?>(SharedTokenKind.ColorPrimary, null);
        if (brush is null || CreateMotion(host) is not { } motion)
        {
            return;
        }

        var radius = context.CornerRadius;
        context.Playback.PlayDecoration(() => WaveOutline.Rent(radius, brush), motion, InteractionEffectClip.OutsideShape);
    }

    internal AbstractMotion? CreateMotion(InteractionEffectHost host)
    {
        var range = Math.Clamp(host.GetMetric(SharedTokenKind.WaveAnimationRange, 0d), 0, 8);
        var opacity = Math.Clamp(host.GetMetric(SharedTokenKind.WaveStartOpacity, 0d), 0, 1);
        var duration = quick
            ? host.GetMetric(SharedTokenKind.MotionDurationSlow, TimeSpan.Zero)
            : host.GetMetric(SharedTokenKind.MotionDurationVerySlow, TimeSpan.Zero);
        if (range <= 0 || opacity <= 0 || duration <= TimeSpan.Zero)
        {
            return null;
        }

        var fade = quick ? duration : host.GetMetric(SharedTokenKind.WaveFadeDuration, TimeSpan.FromSeconds(2));
        var easing = quick
            ? host.GetMetric<Easing>(SharedTokenKind.MotionEaseInOut, MotionCurves.InOut)
            : host.GetMetric<Easing>(SharedTokenKind.WaveEasing, MotionCurves.OutCirc);
        return new WaveMotion(duration, fade, range, opacity, easing);
    }

    internal static IBrush? ResolveBrush(IBrush? border, IBrush? background) =>
        IsVisibleColor(border) ? border : IsVisibleColor(background) ? background : null;

    private static bool IsVisibleColor(IBrush? brush)
    {
        if (brush is not ISolidColorBrush solid || solid.Opacity <= 0 || solid.Color.A == 0)
        {
            return false;
        }
        var color = solid.Color;
        return color.R != 255 || color.G != 255 || color.B != 255 || color.A < 255 || solid.Opacity < 1;
    }

    private sealed class WaveMotion(TimeSpan duration, TimeSpan fade, double range, double opacity, Easing easing) : AbstractMotion
    {
        protected override void Build(MotionBuilder builder, in MotionContext context)
        {
            builder.Parallel([new ExpandOutline(duration, range, easing), new FadeOutline(fade, opacity, easing)]);
        }

        private sealed class ExpandOutline(TimeSpan duration, double range, Easing easing) : AbstractMotion(duration, easing)
        {
            protected override void Build(MotionBuilder builder, in MotionContext context) =>
                builder.Property(WaveOutline.SpreadProperty, static (a, b, progress) => a + (b - a) * progress)
                    .From(0).To(range);
        }

        private sealed class FadeOutline(TimeSpan duration, double opacity, Easing easing) : AbstractMotion(duration, easing)
        {
            protected override void Build(MotionBuilder builder, in MotionContext context) =>
                builder.Opacity.From(opacity).To(0);
        }
    }

    private sealed class WaveOutline : Control, IReusableInteractionVisual
    {
        private const int MaxRetained = 8;
        private static readonly Stack<WaveOutline> s_pool = [];
        internal static readonly StyledProperty<double> SpreadProperty =
            AvaloniaProperty.Register<WaveOutline, double>("Spread");

        private CornerRadius _radius;
        private IBrush? _brush;

        static WaveOutline()
        {
            AffectsRender<WaveOutline>(SpreadProperty);
        }

        internal static WaveOutline Rent(CornerRadius radius, IBrush brush)
        {
            var visual = s_pool.Count == 0 ? new WaveOutline() : s_pool.Pop();
            visual._radius = radius;
            visual._brush = brush;
            return visual;
        }

        void IReusableInteractionVisual.ReturnToPool()
        {
            _radius = default;
            _brush = null;
            ClearValue(SpreadProperty);
            if (s_pool.Count < MaxRetained)
            {
                s_pool.Push(this);
            }
        }

        public override void Render(DrawingContext context)
        {
            var spread = GetValue(SpreadProperty);
            if (spread <= 0 || _brush is null)
            {
                return;
            }
            var bounds = new Rect(Bounds.Size);
            var outer = new CornerRadius(_radius.TopLeft + spread, _radius.TopRight + spread,
                                         _radius.BottomRight + spread, _radius.BottomLeft + spread);
            context.DrawGeometry(_brush, null, new CombinedGeometry(GeometryCombineMode.Exclude,
                InteractionEffectAdorner.CreateOutline(bounds.Inflate(spread), outer),
                InteractionEffectAdorner.CreateOutline(bounds, _radius)));
        }
    }
}
