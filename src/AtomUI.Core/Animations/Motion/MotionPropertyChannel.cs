using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;

namespace AtomUI.Animations;

public sealed class MotionPropertyChannel<T> : IMotionPropertyBuilder
{
    public MotionPropertyChannel<T> From(T value) => KeyFrame(0, value);

    public MotionPropertyChannel<T> FromCurrent() => From(_owner.ReadProperty(_property));

    public MotionPropertyChannel<T> To(T value) => KeyFrame(1, value);

    public MotionPropertyChannel<T> KeyFrame(double cue, T value, Easing? easing = null)
    {
        _owner.VerifyOpen();
        // T already guarantees the value's type; avoid Avalonia's object conversion
        // path, which discovers implicit conversion operators through reflection.
        if (!double.IsFinite(cue) || cue < 0 || cue > 1 || _property.ValidateValue?.Invoke(value) == false)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
        if (_frames.Any(frame => frame.Cue == cue))
        {
            throw new InvalidOperationException("A property channel cannot declare the same keyframe twice.");
        }
        _frames.Add(new MotionPropertyFrame<T>(cue, value, easing));
        return this;
    }

    private readonly MotionBuilder _owner;
    private readonly StyledProperty<T> _property;
    private readonly Func<T, T, double, T> _interpolate;
    private readonly T _initial;
    private readonly List<MotionPropertyFrame<T>> _frames = [];

    AvaloniaProperty IMotionPropertyBuilder.Property => _property;

    internal MotionPropertyChannel(MotionBuilder owner, StyledProperty<T> property, Func<T, T, double, T> interpolate)
    {
        _owner = owner;
        _property = property;
        _interpolate = interpolate;
        _initial = owner.ReadActualProperty(property);
    }

    IMotionPropertyTrack IMotionPropertyBuilder.Freeze(TimeSpan duration, Easing easing)
    {
        var frames = _frames.OrderBy(frame => frame.Cue).ToArray();
        if (frames.Length < 2 || frames[0].Cue != 0 || frames[^1].Cue != 1)
        {
            throw new InvalidOperationException("A property channel requires starting and ending keyframes.");
        }
        return new MotionPropertyTrack<T>(_property, frames, TimeSpan.Zero, duration, easing, _interpolate, _initial);
    }
}

internal interface IMotionPropertyBuilder
{
    AvaloniaProperty Property { get; }
    IMotionPropertyTrack Freeze(TimeSpan duration, Easing easing);
}

internal interface IMotionPropertyTrack
{
    AvaloniaProperty Property { get; }
    TimeSpan Start { get; }
    TimeSpan Duration { get; }
    IMotionPropertyTrack At(TimeSpan start, TimeSpan duration);
    IMotionPropertyTimeline Timeline(IEnumerable<IMotionPropertyTrack> segments);
}

internal interface IMotionPropertyTimeline
{
    AvaloniaProperty Property { get; }
    bool HasChanges { get; }
    object? SampleBoxed(TimeSpan elapsed);
    void Retarget();
    IMotionPropertyPresentation Create(Control target);
}

internal interface IMotionPropertyPresentation : IDisposable
{
    AvaloniaProperty Property { get; }
    void Attach();
    void Apply(TimeSpan elapsed);
    void Seal();
}

internal readonly record struct MotionPropertyFrame<T>(double Cue, T Value, Easing? Easing);

internal sealed class MotionPropertyTrack<T> : IMotionPropertyTrack
{
    internal StyledProperty<T> TypedProperty { get; }
    internal MotionPropertyFrame<T>[] Frames { get; }
    internal T Initial { get; }
    public AvaloniaProperty Property => TypedProperty;
    public TimeSpan Start { get; }
    public TimeSpan Duration { get; }

    private readonly Easing _easing;
    private readonly Func<T, T, double, T> _interpolate;

    internal MotionPropertyTrack(StyledProperty<T> property, MotionPropertyFrame<T>[] frames, TimeSpan start,
        TimeSpan duration, Easing easing, Func<T, T, double, T> interpolate, T initial)
    {
        TypedProperty = property;
        Frames = frames;
        Start = start;
        Duration = duration;
        _easing = easing;
        _interpolate = interpolate;
        Initial = initial;
    }

    public IMotionPropertyTrack At(TimeSpan start, TimeSpan duration)
    {
        return new MotionPropertyTrack<T>(TypedProperty, Frames, start, duration, _easing, _interpolate, Initial);
    }

    public IMotionPropertyTimeline Timeline(IEnumerable<IMotionPropertyTrack> segments)
    {
        return new MotionPropertyTimeline<T>(segments.Cast<MotionPropertyTrack<T>>().OrderBy(track => track.Start).ToArray());
    }

    internal MotionPropertyTrack<T> Retarget(T value)
    {
        var frames = (MotionPropertyFrame<T>[])Frames.Clone();
        frames[0] = frames[0] with { Value = value };
        return new MotionPropertyTrack<T>(TypedProperty, frames, Start, Duration, _easing, _interpolate, Initial);
    }

    internal T Sample(TimeSpan elapsed)
    {
        if (elapsed < Start)
        {
            return Frames[0].Value;
        }
        if (Duration <= TimeSpan.Zero || elapsed >= Start + Duration)
        {
            return Frames[^1].Value;
        }
        var cue = (elapsed - Start).TotalSeconds / Duration.TotalSeconds;
        var right = 1;
        while (right < Frames.Length - 1 && Frames[right].Cue < cue)
        {
            right++;
        }
        var from = Frames[right - 1];
        var to = Frames[right];
        var progress = (cue - from.Cue) / (to.Cue - from.Cue);
        var eased = (to.Easing ?? _easing).Ease(progress);
        if (!double.IsFinite(eased))
        {
            throw new InvalidOperationException("A property easing returned a non-finite value.");
        }
        return _interpolate(from.Value, to.Value, eased);
    }
}

internal sealed class MotionPropertyTimeline<T> : IMotionPropertyTimeline
{
    public AvaloniaProperty Property => _segments[0].TypedProperty;
    public bool HasChanges => _segments.Any(segment => segment.Frames.Any(frame =>
        !EqualityComparer<T>.Default.Equals(frame.Value, _segments[0].Initial)));

    internal T Initial => _segments[0].Initial;
    internal StyledProperty<T> TypedProperty => _segments[0].TypedProperty;

    private readonly MotionPropertyTrack<T>[] _segments;

    internal MotionPropertyTimeline(MotionPropertyTrack<T>[] segments)
    {
        _segments = segments;
    }

    internal T Sample(TimeSpan elapsed)
    {
        var selected = 0;
        var low = 0;
        var high = _segments.Length - 1;
        while (low <= high)
        {
            var middle = (low + high) / 2;
            if (_segments[middle].Start <= elapsed)
            {
                selected = middle;
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }
        return _segments[selected].Sample(elapsed);
    }

    public object? SampleBoxed(TimeSpan elapsed) => Sample(elapsed);

    public void Retarget()
    {
        _segments[0] = _segments[0].Retarget(Initial);
    }

    public IMotionPropertyPresentation Create(Control target) => new MotionPropertyPresentation<T>(target, this);
}

internal sealed class MotionPropertyPresentation<T> : IMotionPropertyPresentation
{
    public AvaloniaProperty Property => _property;

    private readonly Control _target;
    private readonly StyledProperty<T> _property;
    private MotionPropertyTimeline<T>? _timeline;
    private BehaviorSubject<T>? _source;
    private IDisposable? _binding;
    private IDisposable? _constant;
    private T _value;
    private bool _attached;
    private bool _disposed;

    internal MotionPropertyPresentation(Control target, MotionPropertyTimeline<T> timeline)
    {
        _target = target;
        _property = timeline.TypedProperty;
        _timeline = timeline;
        _value = timeline.Initial;
        _source = new BehaviorSubject<T>(_value);
    }

    public void Attach()
    {
        if (_attached || _disposed || _source is null)
        {
            return;
        }
        _attached = true;
        var binding = _target.Bind(_property, _source, BindingPriority.Animation);
        if (_disposed)
        {
            binding.Dispose();
        }
        else
        {
            _binding = binding;
        }
    }

    public void Apply(TimeSpan elapsed)
    {
        if (_disposed || _source is null || _timeline is null)
        {
            return;
        }
        _value = _timeline.Sample(elapsed);
        _source.OnNext(_value);
    }

    public void Seal()
    {
        if (_disposed || _source is null)
        {
            return;
        }
        var constant = _target.SetValue(_property, _value, BindingPriority.Animation);
        if (_disposed)
        {
            constant?.Dispose();
        }
        else
        {
            _constant = constant;
        }
        _binding?.Dispose();
        _binding = null;
        _source?.Dispose();
        _source = null;
        _timeline = null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        try
        {
            _binding?.Dispose();
        }
        finally
        {
            _binding = null;
            try
            {
                _constant?.Dispose();
            }
            finally
            {
                _constant = null;
                _source?.Dispose();
                _source = null;
                _timeline = null;
            }
        }
    }
}
