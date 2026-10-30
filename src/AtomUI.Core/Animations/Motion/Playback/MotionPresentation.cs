using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace AtomUI.Animations;

internal sealed class MotionPresentation : IDisposable
{
    internal MotionValues Values { get; private set; }
    internal bool HasOpacity { get; }
    internal bool HasTransform { get; }

    private readonly Control _target;
    private readonly List<IDisposable> _overrides = [];
    private readonly MotionCompositionTransform? _transform;
    private readonly int _channels;
    private readonly IMotionPropertyPresentation[] _properties;
    private readonly MotionLayoutSegment[] _layouts;
    private IMotionLayoutLease? _layout;
    private int _layoutIndex = -1;
    private TimeSpan _elapsed;
    private BehaviorSubject<double>? _opacity;
    private IDisposable? _opacityBinding;
    private bool _disposed;
    private bool _attached;

    internal MotionPresentation(Control target, MotionPlan plan, MotionValues initial)
    {
        _target = target;
        Values = initial;
        HasOpacity = plan.HasOpacity;
        HasTransform = plan.HasTransform;
        _layouts = plan.Layouts;
        _properties = plan.Properties.Length == 0 ? [] : new IMotionPropertyPresentation[plan.Properties.Length];
        for (var index = 0; index < _properties.Length; index++)
        {
            _properties[index] = plan.Properties[index].Create(target);
        }
        if (plan.FillsBefore || plan.Delay == TimeSpan.Zero || plan.Duration == TimeSpan.Zero)
        {
            foreach (var property in _properties)
            {
                property.Apply(TimeSpan.Zero);
            }
        }
        foreach (var track in plan.Tracks)
        {
            _channels |= 1 << (int)track.Kind;
        }
        if (HasTransform)
        {
            _transform = new MotionCompositionTransform(target, plan);
        }
        if (HasOpacity)
        {
            _opacity = new BehaviorSubject<double>(initial.Opacity);
        }
    }

    internal void Attach(MotionPlayback run)
    {
        if (_attached || _disposed || !run.CanWrite)
        {
            return;
        }
        _attached = true;
        ApplyLayout(Values.LayoutProgress, _elapsed, run);
        if (!run.CanWrite || _disposed)
        {
            return;
        }
        _transform?.Attach(Values);
        if (!run.CanWrite || _disposed)
        {
            return;
        }
        if (_opacity is not null)
        {
            var binding = _target.Bind(Visual.OpacityProperty, _opacity, BindingPriority.Animation);
            if (_disposed)
            {
                binding.Dispose();
            }
            else
            {
                _opacityBinding = binding;
            }
        }
        if (!run.CanWrite || _disposed)
        {
            return;
        }
        foreach (var property in _properties)
        {
            if (!run.CanWrite || _disposed)
            {
                return;
            }
            property.Attach();
        }
    }

    internal void Apply(MotionValues values, TimeSpan elapsed, MotionPlayback run)
    {
        if (_disposed || !run.CanWrite)
        {
            return;
        }
        _elapsed = elapsed;
        if (!_attached)
        {
            Values = values with { Opacity = Math.Clamp(values.Opacity, 0, 1) };
            _opacity?.OnNext(Values.Opacity);
            foreach (var property in _properties)
            {
                property.Apply(elapsed);
            }
            Attach(run);
            return;
        }
        if (_transform is not null)
        {
            Values = Values with
            {
                X = values.X, Y = values.Y, ScaleX = values.ScaleX,
                ScaleY = values.ScaleY, Rotation = values.Rotation
            };
            _transform.Apply(Values);
        }
        Values = Values with { LayoutProgress = values.LayoutProgress };
        ApplyLayout(values.LayoutProgress, elapsed, run);
        if (_disposed || !run.CanWrite)
        {
            return;
        }
        if (_opacity is not null)
        {
            var opacity = Math.Clamp(values.Opacity, 0, 1);
            Values = Values with { Opacity = opacity };
            _opacity.OnNext(opacity);
        }
        foreach (var property in _properties)
        {
            if (_disposed || !run.CanWrite)
            {
                return;
            }
            property.Apply(elapsed);
        }
    }

    internal bool HasChannel(MotionKind kind) => (_channels & (1 << (int)kind)) != 0;
    internal bool HasProperty(AvaloniaProperty property) => _properties.Any(value => value.Property == property);

    internal void Seal(MotionPlayback run)
    {
        if (_disposed || !run.CanWrite)
        {
            return;
        }
        // Transfer the end value to a constant owned frame before removing the live binding.
        if (_opacity is not null)
        {
            Own(_target.SetValue(Visual.OpacityProperty, Values.Opacity, BindingPriority.Animation));
            _opacityBinding?.Dispose();
            _opacityBinding = null;
            _opacity?.Dispose();
            _opacity = null;
        }
        foreach (var property in _properties)
        {
            if (_disposed || !run.CanWrite)
            {
                return;
            }
            property.Seal();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        List<Exception>? failures = null;
        Release(_opacityBinding);
        _opacityBinding = null;
        _opacity?.Dispose();
        _opacity = null;
        var layout = _layout;
        _layout = null;
        Release(layout);
        Release(_transform);
        foreach (var property in _properties)
        {
            Release(property);
        }
        for (var index = _overrides.Count - 1; index >= 0; index--)
        {
            Release(_overrides[index]);
        }
        _overrides.Clear();
        if (failures is not null)
        {
            throw new AggregateException("Motion presentation cleanup failed.", failures);
        }

        void Release(IDisposable? value)
        {
            try
            {
                value?.Dispose();
            }
            catch (Exception exception)
            {
                (failures ??= []).Add(exception);
            }
        }
    }

    private void Own(IDisposable? value)
    {
        if (value is null)
        {
            return;
        }
        if (_disposed)
        {
            value.Dispose();
        }
        else
        {
            _overrides.Add(value);
        }
    }

    private void ApplyLayout(double progress, TimeSpan elapsed, MotionPlayback run)
    {
        if (_layouts.Length == 0 || _disposed || !run.CanWrite)
        {
            return;
        }
        var selected = 0;
        for (var index = 1; index < _layouts.Length && _layouts[index].Start <= elapsed; index++)
        {
            selected = index;
        }
        if (_layoutIndex != selected)
        {
            var previous = _layout;
            _layout = null;
            _layoutIndex = selected;
            try
            {
                var segment = _layouts[selected];
                var layout = ((IMotionLayout)_target).BeginLayoutMotion(segment.Direction, segment.Expanded);
                if (_disposed || !run.CanWrite)
                {
                    layout.Dispose();
                    return;
                }
                _layout = layout;
            }
            finally
            {
                previous?.Dispose();
            }
        }
        if (!_disposed && run.CanWrite)
        {
            _layout?.SetProgress(progress);
        }
    }
}
