using System.Runtime.CompilerServices;
using Avalonia;

namespace AtomUI;

/// <summary>AtomUI 启动挂接入口。生成器为 partial Application 生成调用；手写时在 base.RegisterServices() 之后调用。</summary>
public static class AtomUIStartup
{
    private static readonly ConditionalWeakTable<Application, object> s_attached = new();
    private static readonly object s_marker = new();

    public static void Attach(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        if (AvaloniaLocator.Current.GetService<AtomUIStartupConfiguration>() is not { } configuration)
        {
            return;
        }
        if (!s_attached.TryAdd(application, s_marker))
        {
            return;
        }
        new AtomUIStartupRun(application, configuration).Run();
    }

    /// <summary>按显式配置直接提交（不提前执行、不显示 Splash）。供测试与内部工具使用。</summary>
    internal static void AttachWith(Application application, Action<IAtomUIBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(configure);
        if (!s_attached.TryAdd(application, s_marker))
        {
            throw new InvalidOperationException(
                $"AtomUI is already initialized for Application '{application.GetType().FullName}'.");
        }
        var builder = new AtomUIBuilder(application.GetType());
        configure(builder);
        var process = new AtomUIStartupProcess();
        AtomUIStartupCommit.Commit(application, builder, process, null, null);
        process.MoveTo(AtomUIStartupState.Committed);
        process.CompleteReady();
    }

    internal static bool IsAttached(Application application) => s_attached.TryGetValue(application, out _);
}
