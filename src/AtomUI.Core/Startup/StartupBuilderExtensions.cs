using Avalonia.Controls;

namespace AtomUI;

public static class StartupBuilderExtensions
{
    /// <summary>
    /// 主循环开始后在 UI 线程调用工厂创建主窗体（可 await 加载数据并汇报进度），返回的窗口设为
    /// <c>IClassicDesktopStyleApplicationLifetime.MainWindow</c> 并显示。只适用于经典桌面生命周期；
    /// 不能同时在应用代码中自行设置 MainWindow。
    /// </summary>
    public static IAtomUIBuilder UseMainWindow(this IAtomUIBuilder builder,
                                              Func<MainWindowContext, Task<Window>> factory)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ((AtomUIBuilder)builder).SetMainWindowFactory(factory);
        return builder;
    }
}
