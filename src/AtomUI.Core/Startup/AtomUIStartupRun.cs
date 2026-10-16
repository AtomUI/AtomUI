using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace AtomUI;

internal sealed class AtomUIStartupRun
{
    private static readonly TimeSpan PrepareTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan MainWindowFirstFrameTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan SplashFirstFrameTimeout = TimeSpan.FromSeconds(5);

    private readonly Application _application;
    private readonly AtomUIStartupConfiguration _configuration;
    private readonly AtomUIStartupProcess _process = new();
    private readonly CancellationTokenSource _cancellation = new();
    private IAtomUISplashPresenter? _splash;

    internal AtomUIStartupRun(Application application, AtomUIStartupConfiguration configuration)
    {
        _application = application;
        _configuration = configuration;
    }

    internal void Run()
    {
        var early = _configuration.TakeEarly();
        AtomUIBuilder builder;
        try
        {
            builder = early?.Builder ?? _configuration.CreateBuilder(_application.GetType());
            ValidateHost(builder);
        }
        catch (Exception exception)
        {
            _process.Fail(exception);
            throw;
        }

        var appearance = ResolveAppearance(builder);
        var prepared = ResolvePrepared(early, builder);
        _splash = TryShowSplash(builder, prepared, appearance);
        if (_cancellation.IsCancellationRequested)
        {
            if (_splash is { } canceledSplash)
            {
                canceledSplash.ClosedByUser -= HandleSplashClosedByUser;
                canceledSplash.Close();
                _splash = null;
            }
            return;
        }

        try
        {
            AtomUIStartupCommit.Commit(_application, builder, _process, appearance, prepared?.Select(appearance));
        }
        catch (Exception exception)
        {
            _process.Fail(exception);
            ShowErrorAndWait(exception);
            throw;
        }

        _process.MoveTo(AtomUIStartupState.Committed);
        SchedulePresentation(builder);
    }

    private void ValidateHost(AtomUIBuilder builder)
    {
        if (builder.MainWindowFactory is not null &&
            _application.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime)
        {
            throw new InvalidOperationException("UseMainWindow requires IClassicDesktopStyleApplicationLifetime.");
        }
    }

    private ThemeAppearance ResolveAppearance(AtomUIBuilder builder) =>
        builder.ThemeManagerBuilder.FollowSystemLightRequest is null
            ? ThemeAppearance.Light
            : ThemeManager.ResolveSystemAppearance(_application);

    private PreparedThemeStartup? ResolvePrepared(AtomUIEarlyStartup? early, AtomUIBuilder builder)
    {
        if (early?.Prepare is { } prepare)
        {
            AtomUIStartupEventSource.Log.PhaseStart("PrepareWait");
            try
            {
                if (!prepare.Wait(PrepareTimeout))
                {
                    AtomUIStartupProcess.LogWarning(
                        "AtomUI startup Prepare did not finish within 30 s (background work may be waiting on the UI thread); " +
                        "the Splash is skipped and startup continues.");
                    return null;
                }
                return prepare.Result;
            }
            catch (AggregateException exception)
            {
                AtomUIStartupProcess.LogWarning(exception.GetBaseException());
                return null;
            }
            finally
            {
                AtomUIStartupEventSource.Log.PhaseStop("PrepareWait");
            }
        }
        if (!IsSplashEligible(builder) || !builder.ThemeManagerBuilder.CanPrepareBeforeApplication)
        {
            return null;
        }
        try
        {
            return ThemeStartupPreparation.Prepare(builder.ThemeManagerBuilder.CaptureStartupInputs());
        }
        catch (Exception exception)
        {
            AtomUIStartupProcess.LogWarning(exception);
            return null;
        }
    }

    private bool IsSplashEligible(AtomUIBuilder builder)
    {
        if (builder.Splash is not { } splash || Design.IsDesignMode ||
            _application.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime)
        {
            return false;
        }
        return splash.Mode switch
        {
            SplashMode.Always => true,
            SplashMode.Auto => RuntimeFeature.IsDynamicCodeCompiled,
            _ => false
        };
    }

    private IAtomUISplashPresenter? TryShowSplash(AtomUIBuilder builder, PreparedThemeStartup? prepared,
        ThemeAppearance appearance)
    {
        if (prepared is null || builder.Splash is null || !IsSplashEligible(builder))
        {
            return null;
        }
        var presenter = builder.Splash.CreatePresenter();
        AtomUIStartupEventSource.Log.PhaseStart("Splash");
        var shown = false;
        presenter.ClosedByUser += HandleSplashClosedByUser;
        try
        {
            var content = builder.Splash?.Content ?? new AtomUISplashContent(null, null, null, null, null);
            var resources = prepared?.GlobalResources(appearance) ?? new Dictionary<object, object?>();
            if (!presenter.Show(resources, content))
            {
                return null;
            }
            if (!AtomUIDispatcherPump.PumpUntil(presenter.FirstFrameRendered, SplashFirstFrameTimeout))
            {
                AtomUIStartupProcess.LogWarning("Splash first frame did not render within 5 s; startup continues.");
            }
            shown = true;
            return presenter;
        }
        catch (PlatformNotSupportedException exception)
        {
            AtomUIStartupProcess.LogWarning(exception);
            return null;
        }
        finally
        {
            if (!shown)
            {
                presenter.ClosedByUser -= HandleSplashClosedByUser;
                presenter.Close();
            }
            AtomUIStartupEventSource.Log.PhaseStop("Splash");
        }
    }

    private void ShowErrorAndWait(Exception exception)
    {
        if (_splash is not { } splash)
        {
            return;
        }
        splash.ShowError(exception);
        AtomUIDispatcherPump.PumpUntil(splash.ErrorAcknowledged, Timeout.InfiniteTimeSpan);
    }

    private void HandleSplashClosedByUser(object? sender, EventArgs e)
    {
        _cancellation.Cancel();
        _process.Cancel();
        if (_application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private void SchedulePresentation(AtomUIBuilder builder)
    {
        if (_application.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime)
        {
            _process.CompleteReady();
            return;
        }
        // 主循环开始后执行：此时 OnFrameworkInitializationCompleted 已返回、生命周期已启动。
        Dispatcher.UIThread.Post(() => _ = PresentAsync(builder), DispatcherPriority.Normal);
    }

    private async Task PresentAsync(AtomUIBuilder builder)
    {
        var desktop = (IClassicDesktopStyleApplicationLifetime)_application.ApplicationLifetime!;
        try
        {
            _cancellation.Token.ThrowIfCancellationRequested();
            Window? mainWindow;
            if (builder.MainWindowFactory is { } factory)
            {
                if (desktop.MainWindow is not null)
                {
                    throw new InvalidOperationException("UseMainWindow cannot be combined with assigning MainWindow manually.");
                }
                var progress = new Progress<AtomUIStartupProgress>(value => _splash?.ReportProgress(value));
                mainWindow = await factory(new AtomUIMainWindowContext(progress, _cancellation.Token));
                _cancellation.Token.ThrowIfCancellationRequested();
                desktop.MainWindow = mainWindow;
            }
            else
            {
                mainWindow = desktop.MainWindow;
            }

            if (mainWindow is null)
            {
                await CloseSplashAsync();
                _process.CompleteReady();
                return;
            }

            _process.MoveTo(AtomUIStartupState.Presenting);
            // 主窗体在首帧前被关闭视为用户取消：不再等待首帧，也不再走正常交接。
            var cancelledDuringPresentation = false;
            void HandleMainWindowClosed(object? sender, EventArgs args)
            {
                if (_process.State == AtomUIStartupState.Presenting)
                {
                    cancelledDuringPresentation = true;
                    _process.Cancel();
                }
            }

            mainWindow.Closed += HandleMainWindowClosed;
            try
            {
                if (builder.MainWindowFactory is not null)
                {
                    mainWindow.Show();
                }
                if (!cancelledDuringPresentation)
                {
                    await WaitForFirstFrameAsync(mainWindow, _cancellation.Token);
                }
            }
            finally
            {
                mainWindow.Closed -= HandleMainWindowClosed;
            }
            if (cancelledDuringPresentation)
            {
                await CloseSplashAsync();
                return;
            }
            AtomUIStartupEventSource.Log.MainWindowFirstFrame();
            await CloseSplashAsync();
            _process.CompleteReady();
        }
        catch (OperationCanceledException) when (_cancellation.IsCancellationRequested ||
                                                 _process.State == AtomUIStartupState.Canceled)
        {
            _process.Cancel();
            await CloseSplashAsync();
        }
        catch (Exception exception)
        {
            _process.Fail(exception);
            if (_splash is { } splash)
            {
                splash.ShowError(exception);
                await splash.ErrorAcknowledged;
            }
            var captured = ExceptionDispatchInfo.Capture(exception);
            Dispatcher.UIThread.Post(captured.Throw);
        }
    }

    private static async Task WaitForFirstFrameAsync(Window window, CancellationToken cancellationToken)
    {
        using var closedOrCanceled = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        void HandleClosed(object? sender, EventArgs args) => closedOrCanceled.Cancel();
        window.Closed += HandleClosed;
        try
        {
            if (!window.IsVisible)
            {
                var opened = new TaskCompletionSource();
                void HandleOpened(object? sender, EventArgs args) => opened.TrySetResult();
                window.Opened += HandleOpened;
                try
                {
                    if (window.IsVisible)
                    {
                        opened.TrySetResult();
                    }
                    await opened.Task.WaitAsync(MainWindowFirstFrameTimeout, closedOrCanceled.Token);
                }
                catch (TimeoutException)
                {
                    AtomUIStartupProcess.LogWarning("Main window did not open within 3 s; handing off without waiting.");
                    return;
                }
                finally
                {
                    window.Opened -= HandleOpened;
                }
            }
            closedOrCanceled.Token.ThrowIfCancellationRequested();
            if (ElementComposition.GetElementVisual(window) is not { } visual)
            {
                return;
            }
            try
            {
                await visual.Compositor.RequestCompositionBatchCommitAsync().Rendered
                    .WaitAsync(MainWindowFirstFrameTimeout, closedOrCanceled.Token);
            }
            catch (TimeoutException)
            {
                AtomUIStartupProcess.LogWarning("Main window first frame did not render within 3 s; handing off without waiting.");
            }
        }
        finally
        {
            window.Closed -= HandleClosed;
        }
    }

    private Task CloseSplashAsync()
    {
        if (_splash is not { } splash)
        {
            return Task.CompletedTask;
        }
        splash.ClosedByUser -= HandleSplashClosedByUser;
        _splash = null;
        return splash.FadeOutAndCloseAsync();
    }
}
