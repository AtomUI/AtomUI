using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;

namespace AtomUI.Controls;

/// <summary>
/// Splash 呈现器：创建并显示 SplashWindow，汇报启动进度，错误态等待用户确认，交接时淡出关闭。
/// 全部成员在 UI 线程调用（由 Core 的启动编排保证）。
/// </summary>
internal sealed class SplashPresenter : IAtomUISplashPresenter
{
    private static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(150);

    private readonly TaskCompletionSource _firstFrame = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _errorAcknowledged = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private SplashWindow? _window;
    private bool _closingByFramework;

    public Task FirstFrameRendered => _firstFrame.Task;

    public Task ErrorAcknowledged => _errorAcknowledged.Task;

    public event EventHandler? ClosedByUser;

    public bool Show(IReadOnlyDictionary<object, object?> globalResources, AtomUISplashContent content)
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
            AtomUIStartupProcess.LogError(exception);
            Close();
            return false;
        }
    }

    public void ReportProgress(AtomUIStartupProgress progress)
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
    }

    public async Task FadeOutAndCloseAsync()
    {
        if (_window is not { } window)
        {
            return;
        }
        if (ElementComposition.GetElementVisual(window) is { } visual)
        {
            var fade = visual.Compositor.CreateScalarKeyFrameAnimation();
            fade.Target = "Opacity";
            fade.InsertKeyFrame(0f, 1f);
            fade.InsertKeyFrame(1f, 0f);
            fade.Duration = FadeDuration;
            visual.StartAnimation("Opacity", fade);
            await Task.Delay(FadeDuration + TimeSpan.FromMilliseconds(10));
        }
        Close();
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
                AtomUIStartupEventSource.Log.SplashFirstFrame();
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
