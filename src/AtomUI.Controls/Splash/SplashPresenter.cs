using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;

namespace AtomUI.Controls;

/// <summary>
/// Creates and shows the <see cref="SplashWindow" /> of the startup splash, forwards startup progress to it,
/// waits in the error state until the user acknowledges it, and hides it before closing it at handoff. Every
/// member is called on the UI thread by the startup orchestration in <c>AtomUI.Core</c>.
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
        // The handoff may have hidden the window already when a failure is reported: the error state must
        // become visible and interactive again.
        if (_window is { } window)
        {
            window.SetCurrentValue(Avalonia.Visual.OpacityProperty, 1d);
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
        // The first frame of the main window has been rendered, so the splash is hidden directly, without an
        // opacity animation. A fade would leave a gap between the splash disappearing and the main window
        // reaching the screen.
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
