using AtomUI.Reflection;
using Avalonia.Rendering;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests;

// Contract: on Windows AtomUI swaps the sleep timer of the Avalonia DefaultRenderLoop for a vertical
// sync timer through the private fields _timer and _timerLock. Neither field is part of a public contract,
// so an Avalonia upgrade that changes them makes the swap fail silently and fall back to sleep timing
// (animations stop locking to the refresh rate). Neither the compiler, nor ApiCompat, nor the other tests
// notice; this test pins the field resolution and hand-over semantics against the real DefaultRenderLoop.
public class RenderLoopReflectionTests
{
    [Fact]
    public void Replacing_the_timer_of_a_running_render_loop_hands_its_tick_to_the_new_timer()
    {
        var oldTimer = new RecordingTimer();
        var newTimer = new RecordingTimer();
        var loop = RenderLoop.FromTimer(oldTimer);
        Action<TimeSpan> running = static _ => { };
        // While the render loop runs, its Tick hangs on the current timer; replacing only the fields would let
        // the old and the new timer drive it at the same time.
        oldTimer.Tick = running;

        RenderLoopReflection.TryReplaceTimer(loop, newTimer).ShouldBeTrue();

        RenderLoopReflection.TryGetTimer(loop, out var current).ShouldBeTrue();
        current.ShouldBeSameAs(newTimer);
        oldTimer.Tick.ShouldBeNull();
        newTimer.Tick.ShouldBeSameAs(running);
    }

    private sealed class RecordingTimer : IRenderTimer
    {
        public Action<TimeSpan>? Tick { get; set; }

        public bool RunsInBackground => true;
    }
}
