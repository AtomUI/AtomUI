using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI;

/// <summary>从宿主声明选择启动 surface；延迟建窗与未挂载的 MainView 不视为已经绘制。</summary>
internal sealed class StartupLifetimeReadiness : IDisposable
{
    private IApplicationLifetime? _lifetime;
    private Action<TopLevel?>? _ready;
    private Action? _cancel;
    private IDisposable? _windowOpened;
    private Control? _view;

    internal StartupLifetimeReadiness(IApplicationLifetime? lifetime, Action<TopLevel?> ready, Action cancel)
    {
        _lifetime = lifetime;
        _ready = ready;
        _cancel = cancel;
    }

    internal void Start()
    {
        if (_ready is null)
        {
            return;
        }
        var lifetime = _lifetime;
        if (lifetime is IControlledApplicationLifetime controlled)
        {
            controlled.Exit += HandleExit;
        }
        switch (lifetime)
        {
            case IClassicDesktopStyleApplicationLifetime { MainWindow: { } window }:
                Resolve(window);
                break;
            case IClassicDesktopStyleApplicationLifetime desktop when desktop.ShutdownMode != ShutdownMode.OnExplicitShutdown:
                _windowOpened = Window.WindowOpenedEvent.AddClassHandler<Window>(HandleWindowOpened);
                break;
            case ISingleViewApplicationLifetime { MainView: { } view }:
                if (TopLevel.GetTopLevel(view) is { } topLevel)
                {
                    Resolve(topLevel);
                }
                else
                {
                    _view = view;
                    view.AttachedToVisualTree += HandleViewAttached;
                }
                break;
            default:
                // 无视图宿主/显式关闭的无窗桌面应用采用无 surface 策略，不强迫激活或等待不存在的窗口。
                Resolve(null);
                break;
        }
    }

    private void HandleWindowOpened(Window window, RoutedEventArgs args)
    {
        if (_lifetime is IClassicDesktopStyleApplicationLifetime desktop && ReferenceEquals(desktop.MainWindow, window))
        {
            Resolve(window);
        }
    }

    private void HandleViewAttached(object? sender, VisualTreeAttachmentEventArgs args)
    {
        if (_view is { } view && TopLevel.GetTopLevel(view) is { } topLevel)
        {
            Resolve(topLevel);
        }
    }

    private void Resolve(TopLevel? topLevel)
    {
        _windowOpened?.Dispose();
        _windowOpened = null;
        if (_view is { } view)
        {
            view.AttachedToVisualTree -= HandleViewAttached;
            _view = null;
        }
        var ready = _ready;
        _ready = null;
        ready?.Invoke(topLevel);
    }

    private void HandleExit(object? sender, ControlledApplicationLifetimeExitEventArgs args) => _cancel?.Invoke();

    public void Dispose()
    {
        _windowOpened?.Dispose();
        _windowOpened = null;
        if (_view is { } view)
        {
            view.AttachedToVisualTree -= HandleViewAttached;
            _view = null;
        }
        if (_lifetime is IControlledApplicationLifetime controlled)
        {
            controlled.Exit -= HandleExit;
        }
        _lifetime = null;
        _ready = null;
        _cancel = null;
    }
}
