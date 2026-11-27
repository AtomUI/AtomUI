namespace AtomUI.Animations;

/// <summary>
/// How a motion request ended.
/// </summary>
public enum MotionOutcome
{
    /// <summary>
    /// The motion reached its end, or applied its end state at once because its duration was zero or animation was
    /// disabled.
    /// </summary>
    Completed,
    /// <summary>
    /// A newer request replaced this one on the same target before it ended.
    /// </summary>
    Superseded,
    /// <summary>
    /// The cancellation token was canceled before the motion ended. The target returns to the values it has without
    /// the motion.
    /// </summary>
    Cancelled,
    /// <summary>
    /// The target left the visual tree before the motion ended.
    /// </summary>
    Detached,
    /// <summary>
    /// Nothing was started, because the target was not attached to the visual tree or had no valid arrangement.
    /// </summary>
    NotReady
}

/// <summary>
/// The result of one motion request.
/// </summary>
/// <param name="Outcome">
/// How the request ended.
/// </param>
/// <param name="RequestId">
/// The number that identifies the playback on its target. It is 0 for a result that did not start a playback.
/// </param>
/// <param name="WasAnimated">
/// Whether frames were played for this playback. It is <see langword="false" /> when the end state was applied at
/// once, when no keyframe changes a value, or when nothing was started.
/// </param>
public readonly record struct MotionResult(MotionOutcome Outcome, long RequestId, bool WasAnimated)
{
    /// <summary>
    /// Tells whether the motion reached its end state. A newer request can still have followed.
    /// <see cref="Motion.IsCurrent" /> tells whether the result is still the latest one for the target.
    /// </summary>
    public bool IsCompleted => Outcome == MotionOutcome.Completed;
}
