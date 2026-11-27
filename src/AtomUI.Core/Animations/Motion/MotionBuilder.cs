using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

/// <summary>
/// Receives the declaration of a motion while <see cref="AbstractMotion.Build" /> runs.
/// </summary>
/// <remarks>
/// A new builder is created for every request, and it rejects every write once the build has ended. Each channel
/// property returns the same channel throughout the build, so a channel is declared once.
/// </remarks>
public sealed class MotionBuilder
{
    /// <summary>
    /// The channel for the opacity of the target, from 0 to 1. Values outside that range are clamped when applied.
    /// </summary>
    public MotionChannel Opacity => Channel(MotionKind.Opacity);
    /// <summary>
    /// The channel for the horizontal offset of the target in DIPs. Positive values move the target to the right.
    /// </summary>
    public MotionChannel TranslateX => Channel(MotionKind.TranslateX);
    /// <summary>
    /// The channel for the vertical offset of the target in DIPs. Positive values move the target down.
    /// </summary>
    public MotionChannel TranslateY => Channel(MotionKind.TranslateY);
    /// <summary>
    /// The channel for scaling the target equally on both axes, where 1 is its original size. It cannot be combined
    /// with <see cref="ScaleX" /> or <see cref="ScaleY" />.
    /// </summary>
    public MotionChannel Scale => Channel(MotionKind.Scale);
    /// <summary>
    /// The channel for the horizontal scale factor of the target, where 1 is its original width.
    /// </summary>
    public MotionChannel ScaleX => Channel(MotionKind.ScaleX);
    /// <summary>
    /// The channel for the vertical scale factor of the target, where 1 is its original height.
    /// </summary>
    public MotionChannel ScaleY => Channel(MotionKind.ScaleY);
    /// <summary>
    /// The channel for the rotation of the target around <see cref="Origin" />, in degrees.
    /// </summary>
    public MotionChannel Rotation => Channel(MotionKind.Rotation);

    /// <summary>
    /// Declares a layout transition that reveals the content of the target: its share of the layout grows to the size
    /// of the content while the motion plays.
    /// </summary>
    /// <param name="direction">
    /// The side of the target where space is added. The default is <see cref="Direction.Bottom" />.
    /// </param>
    /// <remarks>
    /// The target must implement <see cref="IMotionLayout" />. Playing the motion on another target throws a
    /// <see cref="NotSupportedException" />, and a motion declares at most one layout transition.
    /// </remarks>
    public void Expand(Direction direction = Direction.Bottom)
    {
        DeclareLayout(direction, true);
    }

    /// <summary>
    /// Declares a layout transition that hides the content of the target: its share of the layout shrinks to zero while
    /// the motion plays.
    /// </summary>
    /// <param name="direction">
    /// The side of the target where space is removed. The default is <see cref="Direction.Bottom" />.
    /// </param>
    /// <remarks>
    /// The target must implement <see cref="IMotionLayout" />. Playing the motion on another target throws a
    /// <see cref="NotSupportedException" />, and a motion declares at most one layout transition.
    /// </remarks>
    public void Collapse(Direction direction = Direction.Bottom)
    {
        DeclareLayout(direction, false);
    }

    /// <summary>
    /// Declares a channel that animates a styled property of the target.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value of the property.
    /// </typeparam>
    /// <param name="property">
    /// The property to animate. It must be registered on the target or be an attached property, and it can be declared
    /// once. <c>Opacity</c>, <c>RenderTransform</c>, <c>RenderTransformOrigin</c>, <c>IsVisible</c> and
    /// <c>Transitions</c> are rejected: use the built-in channels and <see cref="Motion.IsVisibleProperty" /> for them.
    /// </param>
    /// <param name="interpolate">
    /// Returns the value for a position between two keyframe values. It receives the earlier value, the later value and
    /// the eased progress of the segment, which lies outside 0 to 1 when an easing overshoots.
    /// </param>
    /// <returns>
    /// The channel on which the keyframes of the property are declared.
    /// </returns>
    /// <remarks>
    /// While the motion plays, the property is driven at animation priority, so a binding or style on it stays in
    /// place. The channel cannot be combined with <see cref="MotionBuilder.Sequence" /> or
    /// <see cref="MotionBuilder.Parallel" />.
    /// </remarks>
    public MotionPropertyChannel<T> Property<T>(StyledProperty<T> property, Func<T, T, double, T> interpolate)
    {
        VerifyOpen();
        ArgumentNullException.ThrowIfNull(property);
        ArgumentNullException.ThrowIfNull(interpolate);
        if (_compositeTracks is not null)
        {
            throw new InvalidOperationException("A builder cannot mix direct properties with a composed timeline.");
        }
        if (property == Visual.OpacityProperty || property == Visual.RenderTransformProperty ||
            property == Visual.RenderTransformOriginProperty || property == Visual.IsVisibleProperty ||
            property == Animatable.TransitionsProperty)
        {
            throw new InvalidOperationException("Use the built-in motion channels for visual and lifecycle properties.");
        }
        _properties ??= new List<IMotionPropertyBuilder>();
        if (_properties.Any(channel => channel.Property == property))
        {
            throw new InvalidOperationException("Declare a property channel once and keep its fluent builder.");
        }
        var channel = new MotionPropertyChannel<T>(this, property, interpolate);
        _properties.Add(channel);
        return channel;
    }

    /// <summary>
    /// Plays the child motions one after another. A child starts when the previous child, including its delay, has
    /// ended.
    /// </summary>
    /// <param name="children">
    /// The motions to compose. Each must keep the fill mode <c>Both</c>.
    /// </param>
    /// <remarks>
    /// A child without a <see cref="AbstractMotion.Duration" /> plays with the default duration. An explicit duration
    /// of this motion scales the whole timeline, and its <see cref="AbstractMotion.Delay" /> is applied once, before
    /// the first child. The easing of this motion must be a <see cref="LinearEasing" />, and a composition cannot be
    /// combined with direct channels, property channels or a layout transition. A violation throws an
    /// <see cref="InvalidOperationException" /> while the motion is built.
    /// </remarks>
    public void Sequence(IEnumerable<AbstractMotion> children)
    {
        Compose(children, false);
    }

    /// <summary>
    /// Plays the child motions at the same time. The composition lasts as long as the longest child, counting its
    /// delay.
    /// </summary>
    /// <param name="children">
    /// The motions to compose. Each must keep the fill mode <c>Both</c>. Two children cannot write the same channel
    /// or property, whatever their delays. For consecutive writes use <see cref="MotionBuilder.Sequence" />.
    /// </param>
    /// <remarks>
    /// A child without a <see cref="AbstractMotion.Duration" /> plays with the default duration. An explicit duration
    /// of this motion scales the whole timeline, and its <see cref="AbstractMotion.Delay" /> is applied once, before
    /// the children. The easing of this motion must be a <see cref="LinearEasing" />, and a composition cannot be
    /// combined with direct channels, property channels or a layout transition. A violation throws an
    /// <see cref="InvalidOperationException" /> while the motion is built.
    /// </remarks>
    public void Parallel(IEnumerable<AbstractMotion> children)
    {
        Compose(children, true);
    }

    /// <summary>
    /// The pivot of scale and rotation, as a point in the bounds of the target. The default is the center.
    /// </summary>
    /// <remarks>
    /// <see cref="AbstractMotion.Origin" /> replaces this value when the motion definition sets it. When the children
    /// of a composition scale or rotate around different origins, the origin must be set here or on the motion
    /// definition, or building the motion throws an <see cref="InvalidOperationException" />.
    /// </remarks>
    public RelativePoint Origin
    {
        get => _origin;
        set
        {
            VerifyOpen();
            if (!double.IsFinite(value.Point.X) || !double.IsFinite(value.Point.Y))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _origin = value;
            _originWasSet = true;
        }
    }

    private readonly MotionContext _context;
    private readonly MotionChannel?[] _channels = new MotionChannel?[(int)MotionKind.Count];
    private RelativePoint _origin = RelativePoint.Center;
    private bool _closed;
    private bool _originWasSet;
    private bool _conflictingOrigins;
    private MotionTrack[]? _compositeTracks;
    private TimeSpan _compositeDuration;
    private List<IMotionPropertyBuilder>? _properties;
    private IMotionPropertyTrack[]? _compositeProperties;
    private MotionLayoutSegment[]? _layouts;

    internal MotionValues InitialValues => _context.InitialValues;

    internal T ReadActualProperty<T>(StyledProperty<T> property) => _context.Preparation.ReadProperty(property);

    internal T ReadProperty<T>(StyledProperty<T> property)
    {
        VerifyOpen();
        return _context.InitialProperties is { } values && values.TryGetValue(property, out var value)
            ? (T)value! : ReadActualProperty(property);
    }

    internal MotionBuilder(in MotionContext context)
    {
        _context = context;
    }

    internal void VerifyOpen()
    {
        if (_closed)
        {
            throw new InvalidOperationException("The motion builder is no longer active.");
        }
    }

    internal MotionPlan Freeze(Avalonia.Animation.FillMode fillMode, RelativePoint? origin)
    {
        VerifyOpen();
        _closed = true;
        if (_compositeTracks is not null)
        {
            if (_conflictingOrigins && origin is null && !_originWasSet)
            {
                throw new InvalidOperationException("Composed scale/rotation motions require a common origin. Set the parent Origin explicitly.");
            }
            var duration = _context.HasExplicitDuration ? _context.Duration : _compositeDuration;
            var composed = _compositeTracks;
            var properties = _compositeProperties ?? [];
            var layouts = _layouts ?? [];
            if (duration != _compositeDuration)
            {
                composed = composed.Select(track =>
                {
                    var start = ScaleTime(track.Start, _compositeDuration, duration);
                    var end = ScaleTime(track.Start + track.Duration, _compositeDuration, duration);
                    return track.At(start, end - start);
                }).ToArray();
                properties = properties.Select(track =>
                {
                    var start = ScaleTime(track.Start, _compositeDuration, duration);
                    var end = ScaleTime(track.Start + track.Duration, _compositeDuration, duration);
                    return track.At(start, end - start);
                }).ToArray();
                layouts = layouts.Select(segment =>
                {
                    var start = ScaleTime(segment.Start, _compositeDuration, duration);
                    var end = ScaleTime(segment.Start + segment.Duration, _compositeDuration, duration);
                    return segment.At(start, end - start);
                }).ToArray();
            }
            return new MotionPlan(composed, duration, _context.Delay, origin ?? _origin, fillMode, properties, layouts);
        }
        if (_channels[(int)MotionKind.Scale] is not null &&
            (_channels[(int)MotionKind.ScaleX] is not null || _channels[(int)MotionKind.ScaleY] is not null))
        {
            throw new InvalidOperationException("Uniform scale and axis scale cannot own the same channel.");
        }
        var tracks = new List<MotionTrack>();
        foreach (var channel in _channels)
        {
            if (channel is null)
            {
                continue;
            }
            var track = channel.Freeze(_context.Duration, _context.Easing);
            if (track.Kind == MotionKind.Scale)
            {
                tracks.Add(track.AsAxis(MotionKind.ScaleX, _context.InitialValues.ScaleX));
                tracks.Add(track.AsAxis(MotionKind.ScaleY, _context.InitialValues.ScaleY));
            }
            else
            {
                tracks.Add(track);
            }
        }
        var propertyTracks = _properties?.Select(channel => channel.Freeze(_context.Duration, _context.Easing)).ToArray();
        return new MotionPlan(tracks.ToArray(), _context.Duration, _context.Delay, origin ?? _origin, fillMode, propertyTracks, _layouts);
    }

    internal void Close()
    {
        _closed = true;
    }

    private MotionChannel Channel(MotionKind kind)
    {
        VerifyOpen();
        if (_compositeTracks is not null)
        {
            throw new InvalidOperationException("A builder cannot mix direct channels with a composed timeline.");
        }
        return _channels[(int)kind] ??= new MotionChannel(this, kind);
    }

    private void Compose(IEnumerable<AbstractMotion> children, bool parallel)
    {
        VerifyOpen();
        ArgumentNullException.ThrowIfNull(children);
        if (_compositeTracks is not null || _channels.Any(channel => channel is not null) || _properties is { Count: > 0 })
        {
            throw new InvalidOperationException("A builder can declare either direct channels or one composed timeline.");
        }
        if (_context.Easing is not LinearEasing)
        {
            throw new InvalidOperationException("A composition cannot apply a second non-linear easing over its child timelines.");
        }
        var tracks = new List<MotionTrack>();
        var properties = new List<IMotionPropertyTrack>();
        var layouts = new List<MotionLayoutSegment>();
        var ownedProperties = new HashSet<AvaloniaProperty>();
        var initialProperties = new Dictionary<AvaloniaProperty, object?>();
        if (_context.InitialProperties is { } inherited)
        {
            foreach (var pair in inherited)
            {
                initialProperties.Add(pair.Key, pair.Value);
            }
        }
        var total = TimeSpan.Zero;
        var initial = _context.InitialValues;
        var owned = 0;
        RelativePoint? pivotOrigin = null;
        var hasTransformOrigin = false;
        foreach (var child in children.ToArray())
        {
            ArgumentNullException.ThrowIfNull(child);
            if (child.FillMode != FillMode.Both)
            {
                throw new InvalidOperationException("A composed child must use Both; the parent owns completion fill.");
            }
            var plan = child.PrepareChild(_context, parallel ? _context.InitialValues : initial,
                parallel ? _context.InitialProperties : initialProperties);
            var childMask = 0;
            foreach (var track in plan.Tracks)
            {
                childMask |= 1 << (int)track.Kind;
            }
            if (parallel && (owned & childMask) != 0)
            {
                throw new InvalidOperationException("Parallel motions cannot write the same channel. Use a sequence for consecutive writes.");
            }
            owned |= childMask;
            var offset = parallel ? TimeSpan.Zero : total;
            foreach (var track in plan.Tracks)
            {
                tracks.Add(track.At(offset + plan.Delay + track.Start, track.Duration));
            }
            foreach (var timeline in plan.Properties)
            {
                if (parallel && !ownedProperties.Add(timeline.Property))
                {
                    throw new InvalidOperationException("Parallel motions cannot write the same custom property.");
                }
            }
            foreach (var track in plan.PropertyTracks)
            {
                properties.Add(track.At(offset + plan.Delay + track.Start, track.Duration));
            }
            foreach (var segment in plan.Layouts)
            {
                layouts.Add(segment.At(offset + plan.Delay + segment.Start, segment.Duration));
            }
            var end = plan.Delay + plan.Duration;
            total = parallel ? (end > total ? end : total) : total + end;
            if (!parallel)
            {
                initial = plan.Sample(plan.Duration, initial);
                foreach (var timeline in plan.Properties)
                {
                    initialProperties[timeline.Property] = timeline.SampleBoxed(plan.Duration);
                }
            }
            if (plan.HasTransform && !hasTransformOrigin && !_originWasSet)
            {
                _origin = plan.Origin;
                hasTransformOrigin = true;
            }
            if (plan.Tracks.Any(track => track.Kind is MotionKind.ScaleX or MotionKind.ScaleY or MotionKind.Rotation))
            {
                if (pivotOrigin is { } previousOrigin && previousOrigin != plan.Origin)
                {
                    _conflictingOrigins = true;
                }
                pivotOrigin ??= plan.Origin;
                if (!_originWasSet)
                {
                    _origin = pivotOrigin.Value;
                }
            }
        }
        _compositeTracks = tracks.OrderBy(track => track.Start).ToArray();
        _compositeProperties = properties.OrderBy(track => track.Start).ToArray();
        _layouts = layouts.OrderBy(segment => segment.Start).ToArray();
        _compositeDuration = total;
    }

    private void DeclareLayout(Direction direction, bool expanded)
    {
        VerifyOpen();
        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }
        if (_layouts is not null || _compositeTracks is not null)
        {
            throw new InvalidOperationException("Declare only one direct layout transition per motion.");
        }
        _layouts = [new MotionLayoutSegment(direction, expanded, TimeSpan.Zero, _context.Duration)];
        Channel(MotionKind.LayoutProgress).From(0).To(1);
    }

    private static TimeSpan ScaleTime(TimeSpan value, TimeSpan original, TimeSpan requested)
    {
        if (original == TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }
        var fraction = (decimal)value.Ticks / original.Ticks;
        return TimeSpan.FromTicks((long)decimal.Round(fraction * requested.Ticks));
    }
}
