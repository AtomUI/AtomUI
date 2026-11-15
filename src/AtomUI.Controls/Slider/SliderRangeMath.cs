using Avalonia;
using Avalonia.Layout;

namespace AtomUI.Controls;

internal static class SliderRangeMath
{
    public static IReadOnlyList<double> NormalizeRangeValues(
        IReadOnlyList<double>? values,
        double minimum,
        double maximum)
    {
        if (values is null || values.Count < 2)
        {
            return [minimum, minimum];
        }

        var needsNormalization = false;
        var previous = double.NegativeInfinity;
        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i];
            if (!double.IsFinite(value))
            {
                return [];
            }

            var clamped = Math.Clamp(value, minimum, maximum);
            if (clamped != value || value < previous)
            {
                needsNormalization = true;
            }

            previous = value;
        }

        if (!needsNormalization)
        {
            return values;
        }

        var normalized = new double[values.Count];
        for (var i = 0; i < values.Count; i++)
        {
            normalized[i] = Math.Clamp(values[i], minimum, maximum);
        }

        Array.Sort(normalized);
        return normalized;
    }

    public static int FindNearestEnabledHandleIndex(
        IReadOnlyList<double> values,
        IReadOnlyList<bool>? disabledHandles,
        double targetValue)
    {
        var nearestIndex = -1;
        var nearestDelta = double.PositiveInfinity;

        for (var i = 0; i < values.Count; i++)
        {
            if (IsHandleDisabled(disabledHandles, i))
            {
                continue;
            }

            var delta = Math.Abs(values[i] - targetValue);
            if (delta < nearestDelta)
            {
                nearestDelta = delta;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    public static (double minimum, double maximum) GetHandleBounds(
        IReadOnlyList<double> values,
        int handleIndex,
        double minimum,
        double maximum)
    {
        if (handleIndex < 0 || handleIndex >= values.Count)
        {
            return (minimum, maximum);
        }

        var lower = handleIndex > 0 ? values[handleIndex - 1] : minimum;
        var upper = handleIndex < values.Count - 1 ? values[handleIndex + 1] : maximum;
        return (lower, upper);
    }

    public static IReadOnlyList<double> MoveHandle(
        IReadOnlyList<double> values,
        IReadOnlyList<bool>? disabledHandles,
        int handleIndex,
        double targetValue,
        double minimum,
        double maximum)
    {
        if (handleIndex < 0 ||
            handleIndex >= values.Count ||
            IsHandleDisabled(disabledHandles, handleIndex))
        {
            return values.ToArray();
        }

        var updated = values.ToArray();
        var bounds  = GetHandleBounds(values, handleIndex, minimum, maximum);
        updated[handleIndex] = Math.Clamp(targetValue, bounds.minimum, bounds.maximum);
        return updated;
    }

    public static IReadOnlyList<double> ApplyTrackOffset(
        IReadOnlyList<double> values,
        IReadOnlyList<bool>? disabledHandles,
        double offset,
        double minimum,
        double maximum)
    {
        if (values.Count == 0 || offset == 0.0 || HasDisabledHandle(values.Count, disabledHandles))
        {
            return values.ToArray();
        }

        var first = values[0];
        var last  = values[^1];
        var effectiveOffset = Math.Clamp(offset, minimum - first, maximum - last);
        var updated = new double[values.Count];
        for (var i = 0; i < values.Count; i++)
        {
            updated[i] = values[i] + effectiveOffset;
        }

        return updated;
    }

    public static bool HasDisabledHandle(int handleCount, IReadOnlyList<bool>? disabledHandles)
    {
        if (disabledHandles is null)
        {
            return false;
        }

        for (var i = 0; i < handleCount; i++)
        {
            if (IsHandleDisabled(disabledHandles, i))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsHandleDisabled(IReadOnlyList<bool>? disabledHandles, int handleIndex)
    {
        return disabledHandles is not null &&
               handleIndex >= 0 &&
               handleIndex < disabledHandles.Count &&
               disabledHandles[handleIndex];
    }

    public static double ValueToRatio(double value, double minimum, double maximum)
    {
        var range = maximum - minimum;
        if (range <= 0 || !double.IsFinite(range))
        {
            return 0;
        }

        return Math.Clamp((value - minimum) / range, 0, 1);
    }

    public static double RatioToValue(double ratio, double minimum, double maximum)
    {
        return minimum + Math.Clamp(ratio, 0, 1) * (maximum - minimum);
    }

    public static Rect CreateSegmentRect(Rect railRect, Orientation orientation, double startRatio, double endRatio)
    {
        var start = Math.Min(startRatio, endRatio);
        var end   = Math.Max(startRatio, endRatio);
        if (orientation == Orientation.Horizontal)
        {
            return new Rect(
                railRect.X + railRect.Width * start,
                railRect.Y,
                railRect.Width * (end - start),
                railRect.Height);
        }

        var top    = railRect.Y + railRect.Height * (1 - end);
        var height = railRect.Height * (end - start);
        return new Rect(railRect.X, top, railRect.Width, height);
    }

    internal static double SnapToTick(double value, double minimum, double maximum, double frequency)
    {
        if (frequency <= 0 || !double.IsFinite(frequency))
        {
            return value - minimum < maximum - value ? minimum : maximum;
        }

        try
        {
            var min = (decimal)minimum;
            var max = (decimal)maximum;
            var step = (decimal)frequency;
            var current = (decimal)value;
            if (step != 0)
            {
                var tick = decimal.Floor((current - min) / step);
                var lower = min + tick * step;
                // Keep the range's maximum available even if the last interval is shorter.
                var upper = Math.Min(max, lower + step);
                return (double)(current - lower < upper - current ? lower : upper);
            }
        }
        catch (OverflowException)
        {
            // Some valid double ranges cannot be represented as decimals.
        }

        var index = Math.Floor((value - minimum) / frequency);
        var previous = minimum + index * frequency;
        if (!double.IsFinite(previous))
        {
            return value;
        }
        var next = Math.Min(maximum, previous + frequency);
        return Math.Clamp(value - previous < next - value ? previous : next, minimum, maximum);
    }

    internal static double AdjacentTick(double value, double minimum, double frequency, double direction)
    {
        try
        {
            var min = (decimal)minimum;
            var step = (decimal)frequency;
            if (step != 0)
            {
                var tick = ((decimal)value - min) / step;
                var next = direction > 0 ? decimal.Floor(tick) + 1 : decimal.Ceiling(tick) - 1;
                return (double)(min + next * step);
            }
        }
        catch (OverflowException)
        {
            // Fall back to double arithmetic outside the decimal range.
        }
        return value + Math.CopySign(frequency, direction);
    }
}
