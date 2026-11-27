using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia.Rendering;

namespace AtomUI.Reflection;

/// <summary>
/// Reads and replaces the timer of the Avalonia <c>DefaultRenderLoop</c>. Avalonia has no public entry point
/// for replacing the timer of the render loop: during platform initialization the render loop is created
/// together with its timer and handed to the compositor, which keeps it.
/// </summary>
/// <remarks>
/// Every member reports a missing target through its return value, <see langword="false" /> or
/// <see langword="null" />, instead of throwing: callers treat the replacement as an optional improvement
/// and must fall back to the original timer when the internal layout of Avalonia changes.
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

    /// <summary>
    /// Gets the timer that currently drives the render loop. Returns <see langword="false" /> with
    /// <paramref name="timer" /> set to <see langword="null" /> when the loop is not a
    /// <c>DefaultRenderLoop</c>, its internal layout has changed, or it holds no timer.
    /// </summary>
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
    /// Replaces the timer of the render loop with <paramref name="replacement" /> and moves the <c>Tick</c>
    /// handler that the old timer is using over to it. The render loop is already running when the platform
    /// services are ready, so replacing only the field would let the old and the new timer drive it at the
    /// same time. When the call fails, the render loop is left unchanged.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the timer was replaced. <see langword="false" /> when the loop is not a
    /// <c>DefaultRenderLoop</c>, its internal layout has changed, it holds no timer or no timer lock, or the
    /// runtime rejected the write.
    /// </returns>
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
                // The field is written first: when the runtime rejects the write (a read-only field, metadata
                // removed by trimming), the old timer has not been touched yet.
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
    /// <returns>
    /// The installed timer, or <see langword="null" /> when the render-thread proxy or its inner timer is
    /// missing, when <paramref name="create" /> returns <see langword="null" />, or when the runtime rejects
    /// the write. In that last case the replacement is disposed.
    /// </returns>
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
