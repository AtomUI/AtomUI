using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering;

namespace AtomUI.Utils;

/// <summary>
/// Windows 下让渲染循环跟随显示器刷新出帧。
/// </summary>
/// <remarks>
/// RedirectionSurface 合成模式（以及没有可用合成器时的回退）下，Avalonia 用 SleepLoopRenderTimer 驱动渲染循环：
/// 渲染线程以带超时的内核等待睡到下一帧，不对齐垂直同步；Windows 默认计时精度约 15.6 ms，等待还会被拉长到下一个系统时钟
/// 节拍，渲染线程不定期多睡一帧，所有合成动画随之卡顿。其余合成模式由系统合成器按垂直同步驱动，不需要处理。
/// <para>
/// 处理方式分两级：优先把睡眠计时器换成 <see cref="DwmVsyncRenderTimer"/>；Avalonia 内部布局变化导致无法替换时，
/// 退回把进程计时精度提高到 1 ms（保持到进程退出），只消除多睡一帧，节拍仍不锁定刷新率。
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
internal static class WindowsRenderPacing
{
    private const uint TimerResolutionMilliseconds = 1;
    private static int s_timerResolutionRaised;

    /// <summary>
    /// 平台服务就绪后调用：此时渲染循环与它的计时器已按最终的 <see cref="Win32PlatformOptions"/> 创建。
    /// </summary>
    internal static void Apply(AppBuilder appBuilder)
    {
        if (Design.IsDesignMode)
        {
            return;
        }
        bool sleeps;
        if (AvaloniaLocator.Current.GetService<IRenderLoop>() is { } loop &&
            RenderLoopReflection.TryGetTimer(loop, out var timer))
        {
            if (timer is not SleepLoopRenderTimer)
            {
                return;
            }
            if (RenderLoopReflection.TryReplaceTimer(loop, new DwmVsyncRenderTimer()))
            {
                return;
            }
            sleeps = true;
        }
        else
        {
            // 读不到计时器时按选项推断：首选 RedirectionSurface 即睡眠计时。
            sleeps = AvaloniaLocator.Current.GetService<Win32PlatformOptions>()?.CompositionMode is { Count: > 0 } modes &&
                     modes[0] == Win32CompositionMode.RedirectionSurface;
        }
        if (!sleeps)
        {
            return;
        }
        StartupProcess.LogWarning(
            "AtomUI could not replace Avalonia's sleep-based render timer with the DWM vsync timer; " +
            "raising the process timer resolution to 1 ms instead. Animations will not be locked to the display refresh.");
        if (Interlocked.Exchange(ref s_timerResolutionRaised, 1) == 0)
        {
            // 失败（TIMERR_NOCANDO）时保持系统默认精度，渲染行为与未提高时相同，无需额外处理。
            _ = timeBeginPeriod(TimerResolutionMilliseconds);
        }
    }

    [DllImport("winmm.dll")]
    private static extern uint timeBeginPeriod(uint uPeriod);
}
