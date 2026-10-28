using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

public sealed class MotionBuilder
{
    public MotionChannel Opacity => Channel(MotionKind.Opacity);
    public MotionChannel TranslateX => Channel(MotionKind.TranslateX);
    public MotionChannel TranslateY => Channel(MotionKind.TranslateY);
    public MotionChannel Scale => Channel(MotionKind.Scale);
    public MotionChannel ScaleX => Channel(MotionKind.ScaleX);
    public MotionChannel ScaleY => Channel(MotionKind.ScaleY);
    public MotionChannel Rotation => Channel(MotionKind.Rotation);

    public void Expand(Direction direction = Direction.Bottom)
    {
        DeclareLayout(direction, true);
    }

    public void Collapse(Direction direction = Direction.Bottom)
    {
        DeclareLayout(direction, false);
    }

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

    public void Sequence(IEnumerable<Motion> children)
    {
        Compose(children, false);
    }

    public void Parallel(IEnumerable<Motion> children)
    {
        Compose(children, true);
    }

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

    private void Compose(IEnumerable<Motion> children, bool parallel)
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
