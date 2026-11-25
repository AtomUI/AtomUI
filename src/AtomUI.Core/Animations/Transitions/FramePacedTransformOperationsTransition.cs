using System.Diagnostics;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace AtomUI.Animations;

internal sealed class FramePacedTransformOperationsTransition : InterpolatingTransitionBase<ITransform>
{
    private readonly Dictionary<(TransformOperations From, TransformOperations To, double Progress), TransformOperations> _samples = new();
    private long _sampleFrame;

    protected override ITransform Interpolate(double progress, ITransform from, ITransform to)
    {
        var start = from as TransformOperations ?? TransformOperations.Identity;
        var end = to as TransformOperations ?? TransformOperations.Identity;
        if (MotionFrameTiming.Clock is not { RefreshInterval: > 0 and var refresh } ||
            Stopwatch.GetElapsedTime(0, refresh) is not { Ticks: > 0 } interval ||
            Duration <= interval || !(progress is >= 0 and <= 1))
        {
            return TransformOperations.Interpolate(start, end, progress);
        }

        var steps = Math.Ceiling(Duration / interval);
        var step = Math.Floor(progress * steps);
        if (step == 0)
        {
            return start;
        }
        if (step == steps)
        {
            return end;
        }

        // Returning the same transform for the same sample prevents another RenderTransform invalidation
        // when Avalonia pulses again before the display can show the updated angle.
        var frame = Stopwatch.GetTimestamp() / refresh;
        if (_sampleFrame != frame)
        {
            _samples.Clear();
            _sampleFrame = frame;
        }

        var key = (From: start, To: end, Progress: step / steps);
        if (!_samples.TryGetValue(key, out var transform))
        {
            transform = TransformOperations.Interpolate(start, end, key.Progress);
            _samples.Add(key, transform);
        }
        return transform;
    }
}
