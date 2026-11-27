using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

/// <summary>
/// Declares the keyframes of one built-in channel of a motion, such as opacity or translation.
/// </summary>
/// <remarks>
/// A channel comes from a property of <see cref="MotionBuilder" /> and accepts writes only while the motion is
/// built. It needs a keyframe at 0 and one at 1, or building the motion throws an
/// <see cref="InvalidOperationException" />.
/// </remarks>
public sealed class MotionChannel
{
    /// <summary>
    /// Sets the value the channel has at the start of the motion.
    /// </summary>
    public MotionChannel From(double value)
    {
        return KeyFrame(0, value);
    }

    /// <summary>
    /// Starts the channel from the value it has when the motion is prepared, instead of a fixed start value. For
    /// opacity that is the current opacity of the target. The other channels start from the values of a motion that is
    /// playing or held on the target, or from the unchanged state when there is none.
    /// </summary>
    public MotionChannel FromCurrent()
    {
        _fromCurrent = true;
        return From(_owner.InitialValues.Get(_kind));
    }

    /// <summary>
    /// Sets the value the channel has at the end of the motion.
    /// </summary>
    public MotionChannel To(double value)
    {
        return KeyFrame(1, value);
    }

    /// <summary>
    /// Adds a keyframe to the channel.
    /// </summary>
    /// <param name="cue">
    /// The position of the keyframe within the motion, from 0 at the start to 1 at the end.
    /// </param>
    /// <param name="value">
    /// The value of the channel at that position.
    /// </param>
    /// <param name="easing">
    /// The easing of the segment that leads to this keyframe. It replaces the easing of the motion for that segment.
    /// <see langword="null" /> keeps the easing of the motion.
    /// </param>
    /// <returns>
    /// This channel, to add further keyframes.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="cue" /> is not between 0 and 1, or <paramref name="value" /> is not a finite number.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The channel already has a keyframe at <paramref name="cue" />, or the motion has finished building.
    /// </exception>
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
