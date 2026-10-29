using System.Diagnostics;
using NWayland.Protocols.PresentationTime;
using NWayland.Protocols.Wayland;

namespace AtomUI.Native;

/// <summary>
/// Tracks wp_presentation feedback and predicts when a frame produced now will reach the display.
/// </summary>
/// <remarks>
/// Feedback is requested on the Wayland worker immediately before each prepared-frame commit. The prediction
/// is read from the UI thread, so the published values are written and read atomically.
/// Prediction is only offered for a fixed refresh on CLOCK_MONOTONIC, which is the Stopwatch clock on Linux.
/// </remarks>
internal sealed class WaylandPresentationTiming
{
    private const uint ClockMonotonic = 1;
    private const long NanosecondsPerSecond = 1_000_000_000;
    private const int LatencyWindow = 32;
    private const int MaxLatencyFrames = 3;
    private const int CorrectionWindow = 16;
    private const int MaxCorrectionFrames = 2;

    private readonly WpPresentation _presentation;
    private long _lastPresented;
    private long _refresh;
    // Expected presentation of the most recently prepared frame; written on the worker, read on the UI thread.
    private long _scheduledPresent;
    private int _latencyFrames = 1;
    // Worker-thread only: recent submit-to-present latencies, in refreshes.
    private readonly int[] _latencies = new int[LatencyWindow];
    private readonly int[] _latencyCounts = new int[MaxLatencyFrames + 1];
    private int _latencyCursor;
    private int _latencySamples;
    private bool _monotonic;
    // Frames whose draw has started, and frames drawn or held but not yet submitted (any surface).
    private long _framesStarted;
    private int _framesInFlight;
    // Worker-thread only: residual error of scheduled presentations, integrated into a whole-refresh correction.
    private readonly int[] _residualCounts = new int[2 * MaxCorrectionFrames + 1];
    private int _residualSamples;
    private int _correctionFrames;
    private long _presentedFrames;
    private long _predictedFrames;
    private long _predictionHits;

    internal WaylandPresentationTiming(Func<WpPresentation.Listener, WpPresentation> bind)
    {
        _presentation = bind(new PresentationListener(this));
    }

    /// <summary>Frames reported as presented.</summary>
    internal long PresentedFrames => Interlocked.Read(ref _presentedFrames);

    /// <summary>Presented frames that carried a prediction.</summary>
    internal long PredictedFrames => Interlocked.Read(ref _predictedFrames);

    /// <summary>Predicted frames whose prediction landed within a quarter refresh of the actual presentation.</summary>
    internal long PredictionHits => Interlocked.Read(ref _predictionHits);

    /// <summary>Frames whose draw has started; content committed before a later start is in that frame.</summary>
    internal long FramesStarted => Interlocked.Read(ref _framesStarted);

    /// <summary>Frames drawn or held for a callback and not yet submitted.</summary>
    internal int FramesInFlight => Volatile.Read(ref _framesInFlight);

    /// <summary>The current refresh interval in Stopwatch ticks, or zero when unknown or variable.</summary>
    internal long RefreshInterval => Interlocked.Read(ref _refresh);

    internal void OnFrameStarted()
    {
        Interlocked.Increment(ref _framesInFlight);
        Interlocked.Increment(ref _framesStarted);
    }

    internal void OnFrameReleased()
    {
        Interlocked.Decrement(ref _framesInFlight);
    }

    /// <summary>
    /// The Stopwatch timestamp at which content sampled at <paramref name="now"/> is expected to be shown,
    /// or null while no fixed-refresh feedback is available.
    /// </summary>
    /// <remarks>
    /// Content produced while a prepared frame waits for its callback lands in the frame after it, so the target
    /// is never earlier than one refresh after the latest prepared frame. Sampling that frame ahead of time lets the
    /// worker draw fresh content as soon as the prepared slot frees, instead of waiting for the UI to cross a vblank.
    /// </remarks>
    internal long? PredictPresentation(long now)
    {
        if (NextPresentation(now, out var refresh) is not { } presentation)
        {
            return null;
        }
        var scheduled = Interlocked.Read(ref _scheduledPresent);
        return scheduled > 0 ? Math.Max(presentation, scheduled + refresh) : presentation;
    }

    /// <summary>Records a prepared frame on the worker and returns its expected presentation.</summary>
    internal long? OnFramePrepared(long now)
    {
        if (NextPresentation(now, out var refresh) is not { } presentation)
        {
            return null;
        }
        // At most one frame is prepared per callback, so consecutive prepared frames are a refresh apart.
        var scheduled = Interlocked.Read(ref _scheduledPresent);
        var expected = scheduled > 0 ? Math.Max(presentation, scheduled + refresh) : presentation;
        Interlocked.Exchange(ref _scheduledPresent, expected);
        return expected;
    }

    // A frame finished now is submitted at the next frame callback, which follows the next vblank,
    // and is shown the measured number of refreshes after that submission.
    private long? NextPresentation(long now, out long refresh)
    {
        refresh = Interlocked.Read(ref _refresh);
        if (!Volatile.Read(ref _monotonic) || Stopwatch.Frequency != NanosecondsPerSecond)
        {
            return null;
        }
        var last = Interlocked.Read(ref _lastPresented);
        if (refresh <= 0 || last <= 0)
        {
            return null;
        }
        var elapsed = now - last;
        var nextVblank = elapsed <= 0 ? last : last + (elapsed + refresh - 1) / refresh * refresh;
        return nextVblank + (Volatile.Read(ref _latencyFrames) + Volatile.Read(ref _correctionFrames)) * refresh;
    }

    /// <summary>Requests feedback for the commit that follows; called on the Wayland worker before committing.</summary>
    internal void RequestFeedback(WlSurface surface, long? predicted)
    {
        _presentation.Feedback(surface, new FeedbackListener(this, Stopwatch.GetTimestamp(), predicted));
    }

    private void OnPresented(long submitted, long? predicted, long presented, long refresh)
    {
        Interlocked.Increment(ref _presentedFrames);
        if (refresh > 0)
        {
            var frames = (int)Math.Clamp(Math.Round((double)(presented - submitted) / refresh), 0, MaxLatencyFrames);
            Volatile.Write(ref _latencyFrames, RecordLatency(frames));
            if (predicted is { } expected)
            {
                RecordResidual((int)Math.Clamp(Math.Round((double)(presented - expected) / refresh),
                    -MaxCorrectionFrames, MaxCorrectionFrames));
                Interlocked.Increment(ref _predictedFrames);
                if (Math.Abs(presented - expected) <= refresh / 4)
                {
                    Interlocked.Increment(ref _predictionHits);
                }
            }
        }
        Interlocked.Exchange(ref _refresh, refresh);
        Interlocked.Exchange(ref _lastPresented, presented);
    }

    // A single slow frame must not shift every following prediction; publish the most common recent latency.
    private int RecordLatency(int frames)
    {
        if (_latencySamples == LatencyWindow)
        {
            _latencyCounts[_latencies[_latencyCursor]]--;
        }
        else
        {
            _latencySamples++;
        }
        _latencies[_latencyCursor] = frames;
        _latencyCounts[frames]++;
        _latencyCursor = (_latencyCursor + 1) % LatencyWindow;
        var mode = 0;
        for (var index = 1; index <= MaxLatencyFrames; index++)
        {
            if (_latencyCounts[index] > _latencyCounts[mode])
            {
                mode = index;
            }
        }
        return mode;
    }

    // Pipeline stages the latency model does not see (for example when the compositor callback trails its vblank)
    // show up as a constant whole-refresh residual. Integrate the dominant residual of each full window; a
    // window without a dominant non-zero residual leaves the correction unchanged, so it cannot oscillate.
    private void RecordResidual(int frames)
    {
        _residualCounts[frames + MaxCorrectionFrames]++;
        if (++_residualSamples < CorrectionWindow)
        {
            return;
        }
        var dominant = 0;
        for (var index = 0; index < _residualCounts.Length; index++)
        {
            if (_residualCounts[index] * 2 > CorrectionWindow)
            {
                dominant = index - MaxCorrectionFrames;
            }
        }
        Array.Clear(_residualCounts);
        _residualSamples = 0;
        if (dominant != 0)
        {
            Volatile.Write(ref _correctionFrames,
                Math.Clamp(Volatile.Read(ref _correctionFrames) + dominant, -MaxCorrectionFrames, MaxCorrectionFrames));
        }
    }

    private sealed class PresentationListener(WaylandPresentationTiming timing) : WpPresentation.Listener
    {
        protected override void ClockId(WpPresentation eventSender, uint clkId)
        {
            Volatile.Write(ref timing._monotonic, clkId == ClockMonotonic);
        }
    }

    private sealed class FeedbackListener(WaylandPresentationTiming timing, long submitted, long? predicted)
        : WpPresentationFeedback.Listener
    {
        protected override void SyncOutput(WpPresentationFeedback eventSender, WlOutput? output)
        {
        }

        protected override void Presented(WpPresentationFeedback eventSender, uint tvSecHi, uint tvSecLo,
            uint tvNsec, uint refresh, uint seqHi, uint seqLo, WpPresentationFeedback.KindEnum flags)
        {
            var seconds = ((long)tvSecHi << 32) | tvSecLo;
            timing.OnPresented(submitted, predicted, seconds * NanosecondsPerSecond + tvNsec, refresh);
            eventSender.Dispose();
        }

        protected override void Discarded(WpPresentationFeedback eventSender)
        {
            eventSender.Dispose();
        }
    }
}
