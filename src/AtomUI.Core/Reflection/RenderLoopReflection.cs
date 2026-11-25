using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia.Rendering;

namespace AtomUI.Reflection;

/// <summary>
/// 读取并替换 Avalonia <c>DefaultRenderLoop</c> 的计时器。Avalonia 没有公开替换渲染循环计时器的入口，
/// 平台初始化时渲染循环连同计时器一起创建并交给合成器持有。
/// </summary>
/// <remarks>
/// 所有成员在目标缺失时返回 false 而不是抛出：调用方把替换当作可选增强，Avalonia 内部布局变化时必须退回原有计时器。
/// </remarks>
internal static class RenderLoopReflection
{
    #region 反射信息定义

    // DefaultRenderLoop is internal. It writes _timer.Tick only while holding _timerLock (Wakeup, Remove and the end of
    // TimerTick), with the invariant "_running == (_timer.Tick is not null)". Replacing the timer under the same lock
    // keeps that invariant. The constant assembly-qualified name keeps the lookup analyzable by the trimmer.
    [DynamicDependency("_timer", "Avalonia.Rendering.DefaultRenderLoop", "Avalonia.Base")]
    [DynamicDependency("_timerLock", "Avalonia.Rendering.DefaultRenderLoop", "Avalonia.Base")]
    private static readonly Lazy<Fields?> DefaultRenderLoopFields = new(static () =>
    {
        var type = Type.GetType("Avalonia.Rendering.DefaultRenderLoop, Avalonia.Base");
        var timer = type?.GetField("_timer", BindingFlags.Instance | BindingFlags.NonPublic);
        var timerLock = type?.GetField("_timerLock", BindingFlags.Instance | BindingFlags.NonPublic);
        // The lock must be a plain monitor object: a System.Threading.Lock taken through Monitor would not exclude
        // the render loop.
        if (type is null || timer?.FieldType != typeof(IRenderTimer) || timerLock?.FieldType != typeof(object))
        {
            return null;
        }
        return new Fields(type, timer, timerLock);
    });

    private sealed record Fields(Type LoopType, FieldInfo Timer, FieldInfo TimerLock);

    [DynamicDependency("_inner", typeof(ThreadProxyRenderTimer))]
    [DynamicDependency("_lock", typeof(ThreadProxyRenderTimer))]
    private static readonly Lazy<ProxyFields?> ThreadProxyFields = new(static () =>
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var type = typeof(ThreadProxyRenderTimer);
        var inner = type.GetField("_inner", flags);
        var gate = type.GetField("_lock", flags);
        return inner?.FieldType == typeof(IRenderTimer) && gate?.FieldType == typeof(object)
            ? new ProxyFields(inner, gate) : null;
    });

    private sealed record ProxyFields(FieldInfo Inner, FieldInfo Gate);

    #endregion

    /// <summary>当前驱动渲染循环的计时器；渲染循环不是 <c>DefaultRenderLoop</c> 或其内部布局已变化时返回 false。</summary>
    internal static bool TryGetTimer(IRenderLoop loop, [NotNullWhen(true)] out IRenderTimer? timer)
    {
        timer = null;
        if (DefaultRenderLoopFields.Value is not { } fields || loop.GetType() != fields.LoopType)
        {
            return false;
        }
        timer = fields.Timer.GetValue(loop) as IRenderTimer;
        return timer is not null;
    }

    /// <summary>
    /// 把渲染循环的计时器换成 <paramref name="replacement"/>，并把旧计时器上正在使用的 <c>Tick</c> 摘下交给它。
    /// 渲染循环在平台服务就绪时已在运行，只换字段会让新旧计时器同时驱动。失败时渲染循环保持原状。
    /// </summary>
    internal static bool TryReplaceTimer(IRenderLoop loop, IRenderTimer replacement)
    {
        if (DefaultRenderLoopFields.Value is not { } fields || loop.GetType() != fields.LoopType ||
            fields.TimerLock.GetValue(loop) is not { } timerLock)
        {
            return false;
        }
        lock (timerLock)
        {
            if (fields.Timer.GetValue(loop) is not IRenderTimer current)
            {
                return false;
            }
            try
            {
                // 先写字段：运行时拒绝写入时（字段只读保护、裁剪后缺少元数据）旧计时器还没有被动过。
                fields.Timer.SetValue(loop, replacement);
            }
            catch (Exception)
            {
                return false;
            }
            var running = current.Tick;
            current.Tick = null;
            replacement.Tick = running;
            return true;
        }
    }

    /// <summary>
    /// Installs a clock behind the render-thread proxy without replacing the rendering thread.
    /// The handover runs under the proxy's subscription lock and must not wait for the old clock to stop.
    /// </summary>
    internal static IRenderTimer? TryInstallProxyClock(IRenderLoop loop, Func<IRenderTimer, IRenderTimer?> create,
                                                     Action<IRenderTimer> handover)
    {
        if (ThreadProxyFields.Value is not { } fields || !TryGetTimer(loop, out var timer) ||
            timer is not ThreadProxyRenderTimer proxy || fields.Gate.GetValue(proxy) is not { } gate)
        {
            return null;
        }
        lock (gate)
        {
            if (fields.Inner.GetValue(proxy) is not IRenderTimer original || create(original) is not { } replacement)
            {
                return null;
            }
            try
            {
                fields.Inner.SetValue(proxy, replacement);
            }
            catch (Exception)
            {
                (replacement as IDisposable)?.Dispose();
                return null;
            }
            handover(replacement);
            return replacement;
        }
    }
}
