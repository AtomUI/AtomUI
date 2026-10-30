using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

internal enum MotionKind
{
    Opacity,
    TranslateX,
    TranslateY,
    Scale,
    ScaleX,
    ScaleY,
    Rotation,
    LayoutProgress,
    Count
}

internal readonly record struct MotionFrame(double Cue, double Value, Easing? Easing);

internal sealed class MotionTrack
{
    internal MotionKind Kind { get; }
    internal MotionFrame[] Frames { get; }
    internal TimeSpan Start { get; }
    internal TimeSpan Duration { get; }
    internal Easing Easing { get; }
    internal bool StartsFromCurrent { get; }

    internal MotionTrack(MotionKind kind, MotionFrame[] frames, TimeSpan start, TimeSpan duration,
        Easing easing, bool startsFromCurrent)
    {
        Kind = kind;
        Frames = frames;
        Start = start;
        Duration = duration;
        Easing = easing;
        StartsFromCurrent = startsFromCurrent;
    }

    internal double Sample(TimeSpan elapsed)
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
        var eased = (to.Easing ?? Easing).Ease(progress);
        if (!double.IsFinite(eased))
        {
            throw new InvalidOperationException("A motion easing returned a non-finite value.");
        }
        return from.Value + (to.Value - from.Value) * eased;
    }

    internal MotionTrack Retarget(double value)
    {
        var frames = (MotionFrame[])Frames.Clone();
        frames[0] = frames[0] with { Value = value };
        return new MotionTrack(Kind, frames, Start, Duration, Easing, StartsFromCurrent);
    }

    internal MotionTrack AsAxis(MotionKind kind, double initial)
    {
        var frames = Frames;
        if (StartsFromCurrent)
        {
            frames = (MotionFrame[])frames.Clone();
            frames[0] = frames[0] with { Value = initial };
        }
        return new MotionTrack(kind, frames, Start, Duration, Easing, StartsFromCurrent);
    }

    internal MotionTrack At(TimeSpan start, TimeSpan duration)
    {
        return new MotionTrack(Kind, Frames, start, duration, Easing, StartsFromCurrent);
    }
}

internal sealed class MotionPlan
{
    internal MotionTrack[] Tracks { get; }
    internal TimeSpan Duration { get; }
    internal TimeSpan Delay { get; }
    internal RelativePoint Origin { get; }
    internal FillMode FillMode { get; }
    internal IMotionPropertyTrack[] PropertyTracks { get; }
    internal IMotionPropertyTimeline[] Properties { get; }
    internal MotionLayoutSegment[] Layouts { get; }
    internal bool HasOpacity => Tracks.Any(track => track.Kind == MotionKind.Opacity);
    internal bool HasTransform => Tracks.Any(track => track.Kind is not (MotionKind.Opacity or MotionKind.LayoutProgress));
    internal bool FillsBefore => FillMode is FillMode.Backward or FillMode.Both;
    internal bool FillsAfter => FillMode is FillMode.Forward or FillMode.Both;

    internal MotionPlan(MotionTrack[] tracks, TimeSpan duration, TimeSpan delay, RelativePoint origin, FillMode fillMode,
        IMotionPropertyTrack[]? propertyTracks = null, MotionLayoutSegment[]? layouts = null)
    {
        Tracks = tracks;
        Duration = duration;
        Delay = delay;
        Origin = origin;
        FillMode = fillMode;
        PropertyTracks = propertyTracks ?? [];
        Properties = PropertyTracks.GroupBy(track => track.Property)
            .Select(group => group.First().Timeline(group)).ToArray();
        Layouts = layouts ?? [];
    }

    internal MotionValues Sample(TimeSpan elapsed, MotionValues baseline)
    {
        var values = baseline;
        var sampled = 0;
        foreach (var track in Tracks)
        {
            var flag = 1 << (int)track.Kind;
            if (elapsed >= track.Start || (sampled & flag) == 0)
            {
                values = values.With(track.Kind, track.Sample(elapsed));
                sampled |= flag;
            }
        }
        return values;
    }
}

internal readonly record struct MotionValues(double Opacity, double X, double Y, double ScaleX, double ScaleY, double Rotation,
    double LayoutProgress = 0)
{
    internal static MotionValues Baseline(double opacity) => new(opacity, 0, 0, 1, 1, 0);

    internal double Get(MotionKind kind)
    {
        return kind switch
        {
            MotionKind.Opacity => Opacity,
            MotionKind.TranslateX => X,
            MotionKind.TranslateY => Y,
            MotionKind.Scale or MotionKind.ScaleX => ScaleX,
            MotionKind.ScaleY => ScaleY,
            MotionKind.Rotation => Rotation,
            MotionKind.LayoutProgress => LayoutProgress,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }

    internal MotionValues With(MotionKind kind, double value)
    {
        return kind switch
        {
            MotionKind.Opacity => this with { Opacity = value },
            MotionKind.TranslateX => this with { X = value },
            MotionKind.TranslateY => this with { Y = value },
            MotionKind.Scale => this with { ScaleX = value, ScaleY = value },
            MotionKind.ScaleX => this with { ScaleX = value },
            MotionKind.ScaleY => this with { ScaleY = value },
            MotionKind.Rotation => this with { Rotation = value },
            MotionKind.LayoutProgress => this with { LayoutProgress = value },
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }
}
