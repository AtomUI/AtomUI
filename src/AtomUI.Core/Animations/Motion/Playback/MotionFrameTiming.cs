namespace AtomUI.Animations;

/// <summary>Presentation timing reported by the platform renderer, in Stopwatch ticks.</summary>
internal interface IMotionPresentationClock
{
    /// <summary>The earliest presentation that content sampled at <paramref name="now"/> can reach, or null.</summary>
    long? PredictPresentation(long now);

    /// <summary>The fixed refresh interval, or zero when unknown or variable.</summary>
    long RefreshInterval { get; }

    /// <summary>Frames whose draw has started; content committed before a later start is in that frame.</summary>
    long FramesStarted { get; }

    /// <summary>Frames drawn or waiting for the compositor and not yet submitted.</summary>
    int FramesInFlight { get; }
}

/// <summary>Optional platform presentation timing installed by the desktop host.</summary>
internal static class MotionFrameTiming
{
    /// <summary>Read on the UI thread; null when the platform offers no reliable presentation timing.</summary>
    internal static IMotionPresentationClock? Clock { get; set; }
}
