using AtomUI.Reflection;
using Avalonia.Rendering;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests;

// 契约：Windows 下 AtomUI 通过 Avalonia DefaultRenderLoop 的私有字段 _timer / _timerLock 把睡眠计时器换成垂直同步计时器。
// 这两个字段不在任何公开契约里，Avalonia 升级改动它们时替换会静默失败并退回睡眠计时（动画不再锁定刷新率），
// 编译器、ApiCompat 和其他测试都发现不了；这里用真实的 DefaultRenderLoop 守住字段解析与交接语义。
public class RenderLoopReflectionTests
{
    [Fact]
    public void Replacing_the_timer_of_a_running_render_loop_hands_its_tick_to_the_new_timer()
    {
        var oldTimer = new RecordingTimer();
        var newTimer = new RecordingTimer();
        var loop = RenderLoop.FromTimer(oldTimer);
        Action<TimeSpan> running = static _ => { };
        // 渲染循环运行时，它的 Tick 挂在当前计时器上；只换字段会让新旧计时器同时驱动。
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
