using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

public sealed class MotionChannel
{
    public MotionChannel From(double value)
    {
        return KeyFrame(0, value);
    }

    public MotionChannel FromCurrent()
    {
        _fromCurrent = true;
        return From(_owner.InitialValues.Get(_kind));
    }

    public MotionChannel To(double value)
    {
        return KeyFrame(1, value);
    }

    public MotionChannel KeyFrame(double cue, double value, Easing? easing = null)
    {
        _owner.VerifyOpen();
        if (!double.IsFinite(cue) || cue < 0 || cue > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(cue));
        }
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
        if (_frames.Any(frame => frame.Cue == cue))
        {
            throw new InvalidOperationException("A motion channel cannot declare the same keyframe twice.");
        }
        _frames.Add(new MotionFrame(cue, value, easing));
        return this;
    }

    private readonly MotionBuilder _owner;
    private readonly MotionKind _kind;
    private readonly List<MotionFrame> _frames = [];
    private bool _fromCurrent;

    internal MotionChannel(MotionBuilder owner, MotionKind kind)
    {
        _owner = owner;
        _kind = kind;
    }

    internal MotionTrack Freeze(TimeSpan duration, Easing easing)
    {
        var frames = _frames.OrderBy(frame => frame.Cue).ToArray();
        if (frames.Length < 2 || frames[0].Cue != 0 || frames[^1].Cue != 1)
        {
            throw new InvalidOperationException("A motion channel requires both a starting and an ending keyframe.");
        }
        return new MotionTrack(_kind, frames, TimeSpan.Zero, duration, easing, _fromCurrent);
    }
}
