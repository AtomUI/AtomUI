namespace AtomUI.Animations;

/// <summary>
/// Presentation timing reported by the platform renderer, in Stopwatch ticks.
/// </summary>
internal interface IMotionPresentationClock
{
    /// <summary>
    /// Raised on the render thread after the frame and its expected presentation are recorded.
    /// </summary>
    event Action? FrameStarted;

    /// <summary>
    /// The earliest presentation that content sampled at <paramref name="now" /> can reach, or null.
    /// </summary>
    long? PredictPresentation(long now);

    /// <summary>
    /// Whether a new animation must wake the renderer even if its initial sample changes no pixels.
    /// </summary>
    bool RequiresInitialCompositorPulse => false;

    /// <summary>
    /// The fixed refresh interval, or zero when unknown or variable.
    /// </summary>
    long RefreshInterval { get; }

    /// <summary>
    /// The quantization interval for Avalonia transitions, or zero to preserve their samples.
    /// </summary>
    long TransitionSamplingInterval => RefreshInterval;

    /// <summary>
    /// Frames whose draw has started; content committed before a later start is in that frame.
    /// </summary>
    long FramesStarted { get; }

    /// <summary>
    /// Frames drawn or waiting for the compositor and not yet submitted.
    /// </summary>
    int FramesInFlight { get; }
}

/// <summary>
/// Optional platform presentation timing installed by the desktop host.
/// </summary>
internal static class MotionFrameTiming
{
    private static IMotionPresentationClock? s_clock;

    /// <summary>
    /// Read on the UI thread; null when the platform offers no reliable presentation timing.
    /// </summary>
    internal static IMotionPresentationClock? Clock
    {
        get => Volatile.Read(ref s_clock);
        set => Volatile.Write(ref s_clock, value);
    }
}
