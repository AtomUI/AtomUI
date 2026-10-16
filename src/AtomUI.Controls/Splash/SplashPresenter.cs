using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;

namespace AtomUI.Controls;

/// <summary>
/// Splash 呈现器：创建并显示 SplashWindow，汇报启动进度，错误态等待用户确认，交接时先隐藏、后关闭。
/// 全部成员在 UI 线程调用（由 Core 的启动编排保证）。
/// </summary>
internal sealed class SplashPresenter : ISplashPresenter
{
    private readonly TaskCompletionSource _firstFrame = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _errorAcknowledged = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private SplashWindow? _window;
    private bool _closingByFramework;

    public Task FirstFrameRendered => _firstFrame.Task;

    public Task ErrorAcknowledged => _errorAcknowledged.Task;

    public event EventHandler? ClosedByUser;

    public bool Show(IReadOnlyDictionary<object, object?> globalResources, SplashContent content)
    {
        try
        {
            var window = new SplashWindow();
            _window = window;
            window.Initialize(globalResources, content);
            window.Opened += HandleOpened;
            window.Closed += HandleClosed;
            window.Splash!.CloseRequested += HandleCloseRequested;
            window.Show();
            return true;
        }
        catch (Exception exception)
        {
            StartupProcess.LogError(exception);
            Close();
            return false;
        }
    }

    public void ReportProgress(StartupProgress progress)
    {
        if (_window?.Splash is not { } splash)
        {
            return;
        }
        if (progress.Message is { } message)
        {
            splash.Message = message;
        }
        if (progress.Fraction is { } fraction)
        {
            splash.IsIndeterminate = false;
            splash.Progress = fraction;
        }
    }

    public void ShowError(Exception exception)
    {
        if (_window?.Splash is not { } splash)
        {
            _errorAcknowledged.TrySetResult();
            return;
        }
        splash.Status = SplashStatus.Error;
        splash.Message = exception.GetBaseException().Message;
        // 交接隐藏失败时，错误态必须重新可见、可操作。
        if (_window is { } window)
        {
            window.Opacity = 1;
            window.IsHitTestVisible = true;
            window.Topmost = true;
            if (ElementComposition.GetElementVisual(window) is { } visual)
            {
                visual.StopAnimation("Opacity");
                visual.Opacity = 1;
            }
            if (!window.IsVisible)
            {
                window.Show();
            }
        }
    }

    public Task HideAsync()
    {
        if (_window is not { } window)
        {
            return Task.CompletedTask;
        }
        // 主窗体首帧已经完成，直接隐藏 Splash，避免透明度合成和主窗体上屏之间形成空档。
        window.IsHitTestVisible = false;
        window.Topmost = false;
        window.Hide();
        return Task.CompletedTask;
    }

    public void Close()
    {
        if (_window is not { } window)
        {
            return;
        }
        _closingByFramework = true;
        window.Topmost = false;
        window.Close();
        _window = null;
    }

    private void HandleOpened(object? sender, EventArgs e)
    {
        if (_window is not { } window || ElementComposition.GetElementVisual(window) is not { } visual)
        {
            _firstFrame.TrySetResult();
            return;
        }
        visual.Compositor.RequestCompositionBatchCommitAsync().Rendered.ContinueWith(
            static (_, state) =>
            {
                StartupEventSource.Log.SplashFirstFrame();
                ((TaskCompletionSource)state!).TrySetResult();
            },
            _firstFrame, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    private void HandleCloseRequested(object? sender, RoutedEventArgs e) => _window?.Close();

    private void HandleClosed(object? sender, EventArgs e)
    {
        var errorState = _window?.Splash?.Status == SplashStatus.Error;
        if (_window is { } window)
        {
            window.Opened -= HandleOpened;
            window.Closed -= HandleClosed;
            if (window.Splash is { } splash)
            {
                splash.CloseRequested -= HandleCloseRequested;
            }
        }
        _window = null;
        _firstFrame.TrySetResult();
        if (errorState)
        {
            _errorAcknowledged.TrySetResult();
            return;
        }
        if (!_closingByFramework)
        {
            ClosedByUser?.Invoke(this, EventArgs.Empty);
        }
    }
}
