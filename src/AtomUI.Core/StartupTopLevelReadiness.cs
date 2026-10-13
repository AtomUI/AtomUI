using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace AtomUI;

/// <summary>
/// 等待目标窗口的绘制批次及其请求的激活。Rendered 是 compositor 证据，不等同于显示器 presentation。
/// 所有事件和 continuation 在就绪、关闭或 scope 释放时解除对窗口/应用的引用。
/// </summary>
internal sealed class StartupTopLevelReadiness : IDisposable
{
    private TopLevel? _topLevel;
    private Action? _ready;
    private Action? _cancel;
    private bool _requestedFrame;
    private bool _rendered;
    private bool _activated;
    private bool _requiresActivation;
    private int _frameGeneration;

    internal StartupTopLevelReadiness(TopLevel topLevel, Action ready, Action cancel)
    {
        _topLevel = topLevel;
        _ready = ready;
        _cancel = cancel;
    }

    internal void Start()
    {
        if (_topLevel is not { } topLevel)
        {
            return;
        }
        _activated = topLevel is Window { IsActive: true };
        topLevel.PropertyChanged += HandlePropertyChanged;
        topLevel.Opened += HandleOpened;
        topLevel.Closed += HandleClosed;
        if (topLevel is Window target)
        {
            target.Activated += HandleActivated;
            if (target.PlatformImpl is null)
            {
                HandleClosed(target, EventArgs.Empty);
                return;
            }
        }
        RequestFrame();
    }

    private void HandleOpened(object? sender, EventArgs args) => RequestFrame();

    private void HandlePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property != Visual.IsVisibleProperty)
        {
            return;
        }
        if (_topLevel is { IsVisible: true })
        {
            RequestFrame();
        }
        else
        {
            // 失去可见性后，已请求的 RAF / compositor 批次均不再证明当前 surface 就绪。
            _frameGeneration++;
            _requestedFrame = false;
            _rendered = false;
        }
    }

    private void RequestFrame()
    {
        if (_requestedFrame || _topLevel is not { IsVisible: true } topLevel ||
            ElementComposition.GetElementVisual(topLevel) is null)
        {
            return;
        }
        // 宿主可在声明 MainWindow 后、Show 前选择不激活；按实际显示时的策略等待。
        _requiresActivation = topLevel is Window { ShowActivated: true };
        _requestedFrame = true;
        topLevel.InvalidateVisual();
        var generation = ++_frameGeneration;
        topLevel.RequestAnimationFrame(elapsed => HandleAnimationFrame(generation));
    }

    private void HandleAnimationFrame(int generation)
    {
        if (generation != _frameGeneration)
        {
            return;
        }
        if (_topLevel is not { IsVisible: true } topLevel ||
            ElementComposition.GetElementVisual(topLevel) is not { } visual)
        {
            // RAF 使用全局队列，Hide 不会撤销已排队回调；下次 Show 必须能重新请求。
            _requestedFrame = false;
            return;
        }
        var rendered = visual.Compositor.RequestCompositionBatchCommitAsync().Rendered;
        _ = rendered.ContinueWith(_ =>
            Dispatcher.UIThread.Post(() => HandleRendered(generation), DispatcherPriority.Background),
            CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    private void HandleActivated(object? sender, EventArgs args)
    {
        _activated = true;
        TryComplete();
    }

    private void HandleRendered(int generation)
    {
        if (generation != _frameGeneration || _topLevel is not { IsVisible: true })
        {
            return;
        }
        _rendered = true;
        TryComplete();
    }

    private void TryComplete()
    {
        if (_topLevel is not { IsVisible: true } || !_rendered || (_requiresActivation && !_activated))
        {
            return;
        }
        var generation = _frameGeneration;
        Dispatcher.UIThread.Post(() =>
        {
            if (generation != _frameGeneration || _topLevel is not { IsVisible: true } topLevel ||
                _ready is not { } ready)
            {
                return;
            }
            _ready = null;
            // 保持关闭订阅到 Stage 2 终态；排队后隐藏则等待新的可见帧。
            topLevel.PropertyChanged -= HandlePropertyChanged;
            topLevel.Opened -= HandleOpened;
            if (topLevel is Window window)
            {
                window.Activated -= HandleActivated;
            }
            ready();
        }, DispatcherPriority.Background);
    }

    private void HandleClosed(object? sender, EventArgs args)
    {
        var cancel = _cancel;
        Dispose();
        cancel?.Invoke();
    }

    public void Dispose()
    {
        if (_topLevel is { } topLevel)
        {
            topLevel.PropertyChanged -= HandlePropertyChanged;
            topLevel.Opened -= HandleOpened;
            topLevel.Closed -= HandleClosed;
            if (topLevel is Window window)
            {
                window.Activated -= HandleActivated;
            }
        }
        _topLevel = null;
        _ready = null;
        _cancel = null;
    }
}
