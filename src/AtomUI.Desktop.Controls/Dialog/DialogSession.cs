using System.Runtime.ExceptionServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls;

internal enum DialogSessionState
{
    Created,
    Opening,
    Open,
    ClosePending,
    Closing,
    Closed
}

internal readonly record struct DialogCloseRequest(
    object? Result,
    DialogCloseReason Reason,
    DialogButton? SourceButton,
    bool IsForced = false,
    bool IsUserInitiated = false);

internal sealed class DialogSession : IAsyncDisposable
{
    private readonly Dialog _dialog;
    private readonly IDialogPresenter _presenter;
    private readonly CancellationToken _cancellationToken;
    private readonly CancellationTokenSource _lifetimeCancellationSource;
    private readonly Control? _placementTarget;
    private readonly AvaloniaWindow? _ownerWindow;
    private readonly TaskCompletionSource<object?> _completionSource =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private CancellationTokenSource? _pendingCloseCancellationSource;
    private IInputElement? _previousFocus;

    internal DialogSessionState State { get; private set; } = DialogSessionState.Created;

    internal Task<object?> Completion => _completionSource.Task;

    internal IDialogPresenter Presenter => _presenter;

    internal DialogSession(
        Dialog dialog,
        IDialogPresenter presenter,
        CancellationToken cancellationToken,
        Control? placementTarget = null,
        AvaloniaWindow? ownerWindow = null)
    {
        _dialog            = dialog;
        _presenter         = presenter;
        _cancellationToken = cancellationToken;
        _placementTarget   = placementTarget;
        _ownerWindow       = ownerWindow;
        _lifetimeCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _presenter.CloseRequested += HandlePresenterCloseRequested;
        if (_placementTarget is not null)
        {
            _placementTarget.DetachedFromVisualTree += HandlePlacementTargetDetached;
        }

        if (_ownerWindow is not null)
        {
            _ownerWindow.Closed += HandleOwnerClosed;
        }
    }

    internal async Task<object?> RunAsync()
    {
        if (State != DialogSessionState.Created)
        {
            throw new InvalidOperationException("A dialog session can only be run once.");
        }

        _cancellationToken.ThrowIfCancellationRequested();
        _previousFocus = _ownerWindow?.FocusManager.GetFocusedElement() ??
                         (_placementTarget is null
                             ? null
                             : TopLevel.GetTopLevel(_placementTarget)?.FocusManager.GetFocusedElement());
        State = DialogSessionState.Opening;
        try
        {
            await _presenter.ShowAsync(_lifetimeCancellationSource.Token);
            if (State == DialogSessionState.Opening)
            {
                _presenter.FocusScope.Focus();
            }
        }
        catch (OperationCanceledException) when (State is DialogSessionState.Closing or DialogSessionState.Closed)
        {
            return await Completion;
        }
        catch (OperationCanceledException) when (_cancellationToken.IsCancellationRequested)
        {
            await CancelAsync();
            return await Completion;
        }
        catch (Exception ex)
        {
            await FailAsync(ex);
            return await Completion;
        }

        if (State != DialogSessionState.Opening)
        {
            return await Completion;
        }

        State = DialogSessionState.Open;
        try
        {
            _dialog.NotifySessionOpened();
        }
        catch (Exception ex)
        {
            await FailAsync(ex);
            return await Completion;
        }

        try
        {
            return await Completion.WaitAsync(_cancellationToken);
        }
        catch (OperationCanceledException) when (_cancellationToken.IsCancellationRequested && !Completion.IsCompleted)
        {
            await CancelAsync();
        }

        return await Completion;
    }

    internal async ValueTask RequestCloseAsync(DialogCloseRequest request)
    {
        if (request.IsForced)
        {
            await ForceCloseCoreAsync(request);
            return;
        }

        if (request.IsUserInitiated && _dialog.IsConfirmLoading)
        {
            return;
        }

        if (State != DialogSessionState.Open)
        {
            return;
        }

        using var closeCancellationSource =
            CancellationTokenSource.CreateLinkedTokenSource(_lifetimeCancellationSource.Token);
        _pendingCloseCancellationSource = closeCancellationSource;
        State = DialogSessionState.ClosePending;
        bool shouldClose;
        try
        {
            shouldClose = await _dialog.EvaluateSessionCloseAsync(request, closeCancellationSource.Token);
        }
        catch (OperationCanceledException) when (
            closeCancellationSource.IsCancellationRequested &&
            !ReferenceEquals(_pendingCloseCancellationSource, closeCancellationSource))
        {
            return;
        }
        catch
        {
            if (ReferenceEquals(_pendingCloseCancellationSource, closeCancellationSource))
            {
                _pendingCloseCancellationSource = null;
                State                           = DialogSessionState.Open;
            }

            throw;
        }

        if (!ReferenceEquals(_pendingCloseCancellationSource, closeCancellationSource))
        {
            return;
        }

        _pendingCloseCancellationSource = null;
        if (!shouldClose)
        {
            State = DialogSessionState.Open;
            _dialog.NotifySessionCloseVetoed();
            return;
        }

        await CommitCloseAsync(request);
    }

    private async ValueTask ForceCloseCoreAsync(DialogCloseRequest request)
    {
        if (State == DialogSessionState.Closing)
        {
            await Completion;
            return;
        }

        if (State == DialogSessionState.Closed)
        {
            return;
        }

        if (State is not (DialogSessionState.Created or
            DialogSessionState.Opening or
            DialogSessionState.Open or
            DialogSessionState.ClosePending))
        {
            return;
        }

        var closingException = BeginClosing();
        try
        {
            await _dialog.EvaluateSessionCloseAsync(request, _lifetimeCancellationSource.Token);
        }
        catch (Exception ex)
        {
            closingException ??= ex;
        }

        await CommitCloseAsync(request, closingException);
    }

    private async ValueTask CommitCloseAsync(
        DialogCloseRequest request,
        Exception? lifecycleException = null)
    {
        var beginClosingException = BeginClosing();
        lifecycleException ??= beginClosingException;
        await CompleteCloseAsync(request, lifecycleException);
    }

    private async ValueTask CancelAsync()
    {
        var request = new DialogCloseRequest(
            null,
            DialogCloseReason.Programmatic,
            null,
            IsForced: true);
        var lifecycleException = BeginClosing();
        try
        {
            await _dialog.EvaluateSessionCloseAsync(request, _lifetimeCancellationSource.Token);
        }
        catch (Exception ex)
        {
            lifecycleException ??= ex;
        }

        await CompleteCloseAsync(request, lifecycleException, _cancellationToken);
    }

    private async ValueTask FailAsync(Exception exception)
    {
        var request = new DialogCloseRequest(
            null,
            DialogCloseReason.Programmatic,
            null,
            IsForced: true);
        BeginClosing();
        try
        {
            await _dialog.EvaluateSessionCloseAsync(request, _lifetimeCancellationSource.Token);
        }
        catch
        {
            // The presenter/opened failure happened first and remains the reported failure.
        }

        await CompleteCloseAsync(request, exception);
    }

    private Exception? BeginClosing()
    {
        if (State == DialogSessionState.Closing)
        {
            return null;
        }

        var pendingCloseCancellationSource = _pendingCloseCancellationSource;
        _pendingCloseCancellationSource = null;
        State = DialogSessionState.Closing;
        Exception? firstException = null;
        try
        {
            pendingCloseCancellationSource?.Cancel();
        }
        catch (Exception ex)
        {
            firstException = ex;
        }

        try
        {
            _lifetimeCancellationSource.Cancel();
        }
        catch (Exception ex)
        {
            firstException ??= ex;
        }

        _presenter.CloseRequested -= HandlePresenterCloseRequested;
        if (_placementTarget is not null)
        {
            _placementTarget.DetachedFromVisualTree -= HandlePlacementTargetDetached;
        }

        if (_ownerWindow is not null)
        {
            _ownerWindow.Closed -= HandleOwnerClosed;
        }

        return firstException;
    }

    private async ValueTask CompleteCloseAsync(
        DialogCloseRequest request,
        Exception? lifecycleException,
        CancellationToken? canceledToken = null)
    {
        try
        {
            _dialog.SetCurrentValue(Dialog.ResultProperty, request.Result);
        }
        catch (Exception ex)
        {
            lifecycleException ??= ex;
        }

        try
        {
            _dialog.NotifySessionCloseCommitted(request);
        }
        catch (Exception ex)
        {
            lifecycleException ??= ex;
        }

        var closeException = await ClosePresenterAsync();
        lifecycleException ??= closeException;
        try
        {
            _dialog.NotifySessionClosed();
        }
        catch (Exception) when (lifecycleException is not null)
        {
        }
        catch (Exception ex)
        {
            lifecycleException = ex;
        }

        if (lifecycleException is not null)
        {
            _completionSource.TrySetException(lifecycleException);
            ExceptionDispatchInfo.Capture(lifecycleException).Throw();
        }

        if (canceledToken is { } cancellationToken)
        {
            _completionSource.TrySetCanceled(cancellationToken);
        }
        else
        {
            _completionSource.TrySetResult(request.Result);
        }
    }

    private async ValueTask<Exception?> ClosePresenterAsync()
    {

        Exception? closeException = null;
        try
        {
            await _presenter.CloseAsync();
        }
        catch (Exception ex)
        {
            closeException = ex;
        }

        try
        {
            await _presenter.DisposeAsync();
        }
        catch (Exception) when (closeException is not null)
        {
            // Preserve the first failure after completing all teardown.
        }
        catch (Exception ex)
        {
            closeException = ex;
        }

        try
        {
            RestoreFocus();
        }
        catch (Exception) when (closeException is not null)
        {
            // Preserve the first failure after completing all teardown.
        }
        catch (Exception ex)
        {
            closeException = ex;
        }

        State = DialogSessionState.Closed;
        _lifetimeCancellationSource.Dispose();
        return closeException;
    }

    private void RestoreFocus()
    {
        var previousFocus = _previousFocus;
        _previousFocus = null;
        if (CanRestoreFocus(previousFocus) && previousFocus!.Focus())
        {
            return;
        }

        if (CanRestoreFocus(_placementTarget))
        {
            _placementTarget!.Focus();
        }
    }

    private static bool CanRestoreFocus(IInputElement? element)
    {
        return element is { IsEffectivelyEnabled: true, IsEffectivelyVisible: true } &&
               (element is not Visual visual || visual.IsAttachedToVisualTree());
    }

    private async void HandlePresenterCloseRequested(
        object? sender,
        DialogPresenterCloseRequestedEventArgs e)
    {
        try
        {
            await RequestCloseAsync(new DialogCloseRequest(
                e.Result,
                e.Reason,
                e.SourceButton,
                IsUserInitiated: true));
        }
        catch (Exception ex)
        {
            if (!Completion.IsCompleted)
            {
                Dialog.ReportUnhandledException(ex);
            }
        }
    }

    private async void HandlePlacementTargetDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        await ForceCloseFromLifetimeEventAsync(DialogCloseReason.PlacementTargetDetached);
    }

    private async void HandleOwnerClosed(object? sender, EventArgs e)
    {
        await ForceCloseFromLifetimeEventAsync(DialogCloseReason.OwnerClosed);
    }

    private async ValueTask ForceCloseFromLifetimeEventAsync(DialogCloseReason reason)
    {
        try
        {
            await ForceCloseAsync(reason);
        }
        catch when (Completion.IsCompleted)
        {
            // The caller awaiting the session observes teardown failures.
        }
    }

    internal ValueTask ForceCloseAsync(DialogCloseReason reason)
    {
        return RequestCloseAsync(new DialogCloseRequest(null, reason, null, true));
    }

    internal void CancelPendingClose()
    {
        if (State == DialogSessionState.ClosePending)
        {
            var pendingCloseCancellationSource = _pendingCloseCancellationSource;
            _pendingCloseCancellationSource = null;
            State                           = DialogSessionState.Open;
            pendingCloseCancellationSource?.Cancel();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (State != DialogSessionState.Closed)
        {
            await ForceCloseAsync(DialogCloseReason.OwnerClosed);
        }
    }
}
