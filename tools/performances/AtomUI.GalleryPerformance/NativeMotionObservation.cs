using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Transport;

namespace AtomUI.GalleryPerformance;

/// <summary>Tool-only hooks for existing work. No commit, animation frame or layout is requested here.</summary>
internal sealed class NativeMotionObservation : IDisposable
{
    private readonly TopLevel _root;
    private Control _target;
    private readonly string _propertyName;
    private readonly Compositor _compositor;
    private readonly EventInfo _afterCommit;
    private readonly FieldInfo _pendingBatch;
    private readonly Action _committed;
    private readonly object _layoutManager;
    private readonly PropertyInfo _layoutTimed;
    private readonly PropertyInfo _layoutElapsed;
    private readonly Delegate _layoutCallback;
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly Process _process = Process.GetCurrentProcess();
    private NativeMotionSample? _current;
    internal string TargetType => _target.GetType().FullName!;

    internal NativeMotionObservation(TopLevel root, Control target, string propertyName)
    {
        _root = root;
        _target = target;
        _propertyName = propertyName;
        _compositor = ElementComposition.GetElementVisual(target)?.Compositor
            ?? throw new InvalidOperationException("Scenario has no real composition visual.");
        // These are read-only diagnostics for the currently resolved Avalonia, never product runtime APIs.
        _afterCommit = typeof(Compositor).GetEvent("AfterCommit", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new NotSupportedException("Avalonia compositor diagnostic hook changed.");
        _pendingBatch = typeof(Compositor).GetField("_pendingBatch", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new NotSupportedException("Avalonia pending batch diagnostic hook changed.");
        _layoutManager = typeof(TopLevel).GetProperty("LayoutManager", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(root)
            ?? throw new NotSupportedException("Avalonia layout manager diagnostic hook changed.");
        _layoutTimed = _layoutManager.GetType().GetProperty("LayoutPassTimed", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new NotSupportedException("Avalonia layout timing diagnostic hook changed.");
        var timingType = _layoutTimed.PropertyType.GenericTypeArguments[0];
        _layoutElapsed = timingType.GetProperty("Elapsed")!;
        var originalLayoutTimed = (Delegate?)_layoutTimed.GetValue(_layoutManager);
        var callback = GetType().GetMethod(nameof(LayoutTimed), BindingFlags.Instance | BindingFlags.NonPublic)!.MakeGenericMethod(timingType);
        _layoutCallback = callback.CreateDelegate(_layoutTimed.PropertyType, this);
        _committed = Committed;
        _layoutTimed.SetValue(_layoutManager, Delegate.Combine(originalLayoutTimed, _layoutCallback));
        try
        {
            _afterCommit.GetAddMethod(true)!.Invoke(_compositor, [_committed]);
            _target.PropertyChanged += PropertyChanged;
            _root.LayoutUpdated += LayoutUpdated;
        }
        catch
        {
            _layoutTimed.SetValue(_layoutManager, originalLayoutTimed);
            _process.Dispose();
            throw;
        }
    }

    internal NativeMotionSample Begin(string scenario, string direction, int iteration)
    {
        var sample = new NativeMotionSample(scenario, direction, iteration, _clock.Elapsed.TotalMilliseconds)
        {
            AllocatedBytes = GC.GetTotalAllocatedBytes(precise: false),
            UiAllocatedBytes = GC.GetAllocatedBytesForCurrentThread(),
            CpuMilliseconds = _process.TotalProcessorTime.TotalMilliseconds,
            Collections = [GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2)]
        };
        _current = sample;
        return sample;
    }

    internal void ObserveTarget(Control target)
    {
        if (ReferenceEquals(target, _target))
        {
            return;
        }
        if (!ReferenceEquals(ElementComposition.GetElementVisual(target)?.Compositor, _compositor))
        {
            throw new NotSupportedException("This popup uses a different compositor; root-batch measurements would be incomplete.");
        }
        _target.PropertyChanged -= PropertyChanged;
        _target = target;
        _target.PropertyChanged += PropertyChanged;
    }

    internal async Task EndAsync(NativeMotionSample sample)
    {
        _current = null;
        await Task.WhenAll(sample.PendingBatches).WaitAsync(TimeSpan.FromSeconds(5));
        sample.PendingBatches.Clear();
        sample.ElapsedMilliseconds = _clock.Elapsed.TotalMilliseconds - sample.StartMilliseconds;
        sample.AllocatedBytes = GC.GetTotalAllocatedBytes(precise: false) - sample.AllocatedBytes;
        sample.UiAllocatedBytes = GC.GetAllocatedBytesForCurrentThread() - sample.UiAllocatedBytes;
        sample.CpuMilliseconds = _process.TotalProcessorTime.TotalMilliseconds - sample.CpuMilliseconds;
        for (var generation = 0; generation < 3; generation++)
        {
            sample.Collections[generation] = GC.CollectionCount(generation) - sample.Collections[generation];
        }
    }

    private void PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (_current is not { } sample)
        {
            return;
        }
        if (args.Property.Name == _propertyName && args.NewValue is double value)
        {
            sample.Values.Add(new NativeMotionValue(_clock.Elapsed.TotalMilliseconds, value));
        }
        if (args.Property == Visual.BoundsProperty)
        {
            var bounds = _target.Bounds;
            sample.Bounds.Add(new NativeMotionBounds(_clock.Elapsed.TotalMilliseconds, bounds.X, bounds.Y, bounds.Width, bounds.Height));
        }
    }

    private void LayoutUpdated(object? sender, EventArgs args) => _current?.Layouts.Add(_clock.Elapsed.TotalMilliseconds);

    private void LayoutTimed<T>(T timing)
    {
        if (_current is { } sample)
        {
            sample.LayoutMilliseconds.Add(((TimeSpan)_layoutElapsed.GetValue(timing)!).TotalMilliseconds);
        }
    }

    private void Committed()
    {
        if (_current is not { } sample)
        {
            return;
        }
        sample.Commits.Add(_clock.Elapsed.TotalMilliseconds);
        if (_pendingBatch.GetValue(_compositor) is not CompositionBatch batch)
        {
            sample.MissedBatches++;
            return;
        }
        sample.PendingBatches.Add(Observe(batch.Processed, sample.Processed));
        sample.PendingBatches.Add(Observe(batch.Rendered, sample.Rendered));
    }

    private Task Observe(Task task, List<double> values) => task.ContinueWith(completed =>
    {
        completed.GetAwaiter().GetResult();
        var timestamp = _clock.Elapsed.TotalMilliseconds;
        lock (values)
        {
            values.Add(timestamp);
        }
    }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

    public void Dispose()
    {
        _current = null;
        _root.LayoutUpdated -= LayoutUpdated;
        _target.PropertyChanged -= PropertyChanged;
        _afterCommit.GetRemoveMethod(true)!.Invoke(_compositor, [_committed]);
        var current = (Delegate?)_layoutTimed.GetValue(_layoutManager);
        _layoutTimed.SetValue(_layoutManager, Delegate.Remove(current, _layoutCallback));
        _process.Dispose();
    }
}

internal readonly record struct NativeMotionValue(double Time, double Value);
internal readonly record struct NativeMotionBounds(double Time, double X, double Y, double Width, double Height);

internal sealed class NativeMotionSample(string scenario, string direction, int iteration, double start)
{
    public string Scenario { get; } = scenario;
    public string Direction { get; } = direction;
    public int Iteration { get; } = iteration;
    public double StartMilliseconds { get; } = start;
    public double ElapsedMilliseconds { get; set; }
    public int SampleMilliseconds { get; set; }
    public double Final { get; set; }
    public long AllocatedBytes { get; set; }
    public long UiAllocatedBytes { get; set; }
    public double CpuMilliseconds { get; set; }
    public int[] Collections { get; set; } = [];
    public int MissedBatches { get; set; }
    public string AnimationContract { get; set; } = "";
    public string TargetType { get; set; } = "";
    public List<NativeMotionValue> Values { get; } = new(256);
    public List<NativeMotionBounds> Bounds { get; } = new(256);
    public List<double> Layouts { get; } = new(256);
    public List<double> LayoutMilliseconds { get; } = new(256);
    public List<double> Commits { get; } = new(256);
    public List<double> Processed { get; } = new(256);
    public List<double> Rendered { get; } = new(256);
    [System.Text.Json.Serialization.JsonIgnore]
    internal List<Task> PendingBatches { get; } = new(512);

    internal bool HasIntermediateValues()
    {
        if (Values.Count < 3 || Values[^1].Time - Values[0].Time < 1 || Rendered.Count < 2)
        {
            return false;
        }
        var minimum = Values.Min(value => value.Value);
        var maximum = Values.Max(value => value.Value);
        return Values.Any(value => value.Value > minimum && value.Value < maximum);
    }
}
