using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Animations;

/// <summary>
/// Expands from the press location and fades on the original decoration after release.
/// </summary>
public sealed class InkRippleInteractionEffect : InteractionEffect
{
    /// <summary>
    /// Shared default instance of the ink ripple effect.
    /// </summary>
    public static InkRippleInteractionEffect Default { get; } = new();

    /// <inheritdoc />
    public override IInteractionEffectSession OnPressStarted(in InteractionEffectStartContext context)
    {
        var create = CreateVisual(context.Size, context.PointerOrigin, context.Brush);
        return new Session(context.Playback.PlayDecoration(create, RippleMotion.Enter, InteractionEffectClip.InsideShape));
    }

    /// <inheritdoc />
    public override void OnActivated(in InteractionEffectActivateContext context)
    {
        if (!context.HadPress)
        {
            context.Playback.PlayDecoration(CreateVisual(context.Size, context.PointerOrigin, context.Brush),
                RippleMotion.Activate, InteractionEffectClip.InsideShape);
        }
    }

    private static Func<Control> CreateVisual(Size size, Point? pointer, IBrush? brush)
    {
        var origin = pointer ?? new Point(size.Width / 2, size.Height / 2);
        var dx = Math.Max(Math.Abs(origin.X), Math.Abs(size.Width - origin.X));
        var dy = Math.Max(Math.Abs(origin.Y), Math.Abs(size.Height - origin.Y));
        var radius = Math.Sqrt(dx * dx + dy * dy);
        return () => new RippleVisual(origin, radius, brush ?? Brushes.Gray);
    }

    private sealed class Session(IDecorationEffectLease lease) : IInteractionEffectSession
    {
        public void Complete(bool activated) => lease.TryBeginExit(RippleMotion.Exit);
        public void Cancel() => lease.Dispose();
        public void Dispose() => lease.Dispose();
    }

    private sealed class RippleMotion(TimeSpan duration, bool fromCurrent, double expansion, double opacity)
        : AbstractMotion(duration, new CubicEaseOut())
    {
        internal static readonly RippleMotion Enter = new(TimeSpan.FromMilliseconds(180), false, 0.75, 0.18);
        internal static readonly RippleMotion Exit = new(TimeSpan.FromMilliseconds(180), true, 1, 0);
        internal static readonly RippleMotion Activate = new(TimeSpan.FromMilliseconds(300), false, 1, 0);

        protected override void Build(MotionBuilder builder, in MotionContext context)
        {
            var radius = builder.Property(RippleVisual.ExpansionProperty, static (a, b, progress) => a + (b - a) * progress);
            if (fromCurrent)
            {
                radius.FromCurrent().To(expansion);
                builder.Opacity.FromCurrent().To(opacity);
            }
            else
            {
                radius.From(0).To(expansion);
                builder.Opacity.From(0.18).To(opacity);
            }
        }
    }

    private sealed class RippleVisual(Point origin, double radius, IBrush brush) : Control
    {
        internal static readonly StyledProperty<double> ExpansionProperty =
            AvaloniaProperty.Register<RippleVisual, double>("Expansion");

        static RippleVisual()
        {
            AffectsRender<RippleVisual>(ExpansionProperty);
        }

        public override void Render(DrawingContext context)
        {
            var currentRadius = Math.Max(0, radius * GetValue(ExpansionProperty));
            context.DrawEllipse(brush, null, origin, currentRadius, currentRadius);
        }
    }
}
