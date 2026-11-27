using System.ComponentModel;
using System.Runtime.ExceptionServices;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class Dialog
{
    private DialogSession? _session;
    private Task<object?>? _sessionTask;
    private Task? _sessionObservationTask;
    private long _intentGeneration;

    static Dialog()
    {
        IsHitTestVisibleProperty.OverrideDefaultValue<Dialog>(false);
        IsOpenProperty.Changed.AddClassHandler<Dialog>(
            static (dialog, e) => dialog.HandleIsOpenChanged((AvaloniaPropertyChangedEventArgs<bool>)e));
    }

    public Dialog()
    {
        SetCurrentValue(EffectiveMinimizableProperty, !IsModal && IsMinimizable);
    }

    internal async ValueTask<bool> EvaluateSessionCloseAsync(
        DialogCloseRequest request,
        CancellationToken cancellationToken)
    {
        var closingArgs = new CancelEventArgs();
        Closing?.Invoke(this, closingArgs);
        if (request.IsForced)
        {
            return true;
        }

        if (closingArgs.Cancel)
        {
            return false;
        }

        if (BeforeCloseAsync is null)
        {
            return true;
        }

        var context = new DialogClosingContext(
            this,
            request.Result,
            request.Reason,
            request.SourceButton,
            cancellationToken);
        return await BeforeCloseAsync(context);
    }

    /// <summary>
    /// Opens the dialog and completes when the dialog has closed, after the <see cref="Closed" /> event. The
    /// result of the close is then in <see cref="Result" /> and the dialog can be opened again. The method can be
    /// called from any thread. An exception thrown by an <see cref="Opened" />, <see cref="Accepted" />,
    /// <see cref="Rejected" />, <see cref="Finished" /> or <see cref="Closed" /> handler is thrown by the returned
    /// task, after the dialog has closed.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancels the session: the dialog is closed as a forced close with a <see langword="null" /> result, and the
    /// returned task ends canceled.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The dialog already has an active session, or no session could start because neither the placement target nor
    /// the dialog control is attached. In the latter case <see cref="IsOpen" /> is reset to <see langword="false" />.
    /// </exception>
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (!Dispatcher.CheckAccess())
        {
            await Dispatcher.InvokeAsync(async () => await OpenAsync(cancellationToken));
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var session = _session;
        if (session is not null)
        {
            if (session.State == DialogSessionState.Closed)
            {
                var closedObservationTask = _sessionObservationTask;
                if (closedObservationTask is not null)
                {
                    await closedObservationTask;
                }

                if (_session is not null)
                {
                    throw CreateActiveSessionException();
                }
            }
            else
            {
                throw CreateActiveSessionException();
            }
        }

        if (!IsOpen)
        {
            SetCurrentValue(IsOpenProperty, true);
        }

        await ReconcileAwaitedIntentAsync(cancellationToken);
        var sessionTask = _sessionTask;
        if (sessionTask is null)
        {
            if (IsOpen)
            {
                SetCurrentValue(IsOpenProperty, false);
            }

            throw new InvalidOperationException(
                "Dialog.OpenAsync requires an attached placement target or an attached Dialog control.");
        }

        var sessionObservationTask = _sessionObservationTask;
        try
        {
            await sessionTask;
        }
        finally
        {
            if (sessionObservationTask is not null)
            {
                await sessionObservationTask;
            }
        }
    }

    private static InvalidOperationException CreateActiveSessionException()
    {
        return new InvalidOperationException(
            "Dialog.OpenAsync cannot be called while the dialog already has an active session.");
    }

    /// <summary>
    /// Requests the dialog to close with <see cref="DialogCode.Accepted" /> as the result. The request raises
    /// <see cref="Closing" /> and then consults <see cref="BeforeCloseAsync" />; either can keep the dialog open.
    /// The request has no effect unless the dialog is open.
    /// </summary>
    public void Accept()
    {
        RequestClose(new DialogCloseRequest(DialogCode.Accepted, DialogCloseReason.Accepted, null));
    }

    /// <summary>
    /// Requests the dialog to close with <see cref="DialogCode.Rejected" /> as the result. The request raises
    /// <see cref="Closing" /> and then consults <see cref="BeforeCloseAsync" />; either can keep the dialog open.
    /// The request has no effect unless the dialog is open.
    /// </summary>
    public void Reject()
    {
        RequestClose(new DialogCloseRequest(DialogCode.Rejected, DialogCloseReason.Rejected, null));
    }

    /// <summary>
    /// Requests the dialog to close with <paramref name="dialogResult" /> as the result. The request raises
    /// <see cref="Closing" /> and then consults <see cref="BeforeCloseAsync" />; either can keep the dialog open.
    /// The request has no effect unless the dialog is open.
    /// </summary>
    /// <param name="dialogResult">
    /// The value the dialog closes with.
    /// </param>
    public void Done(object? dialogResult)
    {
        RequestClose(new DialogCloseRequest(dialogResult, DialogCloseReason.Programmatic, null));
    }

    /// <summary>
    /// Requests the dialog to close with the current value of <see cref="Result" /> as the result. The request
    /// raises <see cref="Closing" /> and then consults <see cref="BeforeCloseAsync" />; either can keep the dialog
    /// open. The request has no effect unless the dialog is open.
    /// </summary>
    public void Done()
    {
        Done(Result);
    }

    private void HandleIsOpenChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        _intentGeneration++;
        Dispatcher.Post(ReconcilePostedIntent, DispatcherPriority.Normal);
    }

    private async void ReconcilePostedIntent()
    {
        try
        {
            await ReconcileIntentAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            ReportUnhandledException(new InvalidOperationException("Dialog intent reconciliation failed.", ex));
        }
    }

    private ValueTask ReconcileIntentAsync(CancellationToken cancellationToken)
    {
        return ReconcileIntentCoreAsync(cancellationToken, ReportUnhandledException);
    }

    private ValueTask ReconcileAwaitedIntentAsync(CancellationToken cancellationToken)
    {
        return ReconcileIntentCoreAsync(cancellationToken, null);
    }

    private async ValueTask ReconcileIntentCoreAsync(
        CancellationToken cancellationToken,
        Action<Exception>? sessionFailureReporter)
    {
        if (IsOpen)
        {
            if (_session is null)
            {
                TryStartSession(cancellationToken, _intentGeneration, sessionFailureReporter);
            }
            else if (_session.State == DialogSessionState.ClosePending)
            {
                _session.CancelPendingClose();
            }

            return;
        }

        if (_session is null)
        {
            return;
        }

        if (_session.State is DialogSessionState.Created or DialogSessionState.Opening)
        {
            await _session.ForceCloseAsync(DialogCloseReason.Programmatic);
        }
        else if (_session.State == DialogSessionState.Open)
        {
            await _session.RequestCloseAsync(
                new DialogCloseRequest(Result, DialogCloseReason.Programmatic, null));
        }
    }

    private bool TryStartSession(
        CancellationToken cancellationToken,
        long generation,
        Action<Exception>? sessionFailureReporter)
    {
        var placementTarget = OverlayLayerResolver.TryResolvePlacementTarget(this, PlacementTarget);
        var topLevel = TopLevel.GetTopLevel(placementTarget);
        if (placementTarget is null || topLevel is null)
        {
            return false;
        }

        var hostType = ResolveDialogHostType(DialogHostType);
        IDialogPresenter presenter = hostType == DialogHostType.Window
            ? new WindowDialogPresenter(this, topLevel)
            : new OverlayDialogPresenter(this, placementTarget);
        var session = new DialogSession(
            this,
            presenter,
            cancellationToken,
            placementTarget,
            topLevel as Avalonia.Controls.Window);
        _session     = session;
        _sessionTask = session.RunAsync();
        _sessionObservationTask = ObserveSessionAsync(
            session,
            _sessionTask,
            generation,
            sessionFailureReporter);
        return true;
    }

    private async Task ObserveSessionAsync(
        DialogSession session,
        Task<object?> sessionTask,
        long generation,
        Action<Exception>? sessionFailureReporter)
    {
        await Task.Yield();
        try
        {
            await sessionTask;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            sessionFailureReporter?.Invoke(ex);
        }
        finally
        {
            if (!Dispatcher.CheckAccess())
            {
                await Dispatcher.InvokeAsync(
                    async () => await FinalizeObservedSessionAsync(session, sessionTask, generation));
            }
            else
            {
                await FinalizeObservedSessionAsync(session, sessionTask, generation);
            }
        }
    }

    private async Task FinalizeObservedSessionAsync(
        DialogSession session,
        Task<object?> sessionTask,
        long generation)
    {
        if (!ReferenceEquals(_session, session))
        {
            return;
        }

        _session                = null;
        _sessionTask            = null;
        _sessionObservationTask = null;
        if (sessionTask.IsCanceled || sessionTask.IsFaulted)
        {
            if (_intentGeneration == generation && IsOpen)
            {
                SetCurrentValue(IsOpenProperty, false);
            }
        }

        if (IsOpen)
        {
            await ReconcileIntentAsync(CancellationToken.None);
        }
    }

    private static DialogHostType ResolveDialogHostType(DialogHostType requestedHostType)
    {
        return requestedHostType == DialogHostType.Window && !RuntimePlatform.Features.SupportsNativeWindow
            ? DialogHostType.Overlay
            : requestedHostType;
    }

    private void RequestClose(DialogCloseRequest request)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Post(() => RequestClose(request), DispatcherPriority.Normal);
            return;
        }

        if (_session is null)
        {
            return;
        }

        _ = ObserveCloseRequestAsync(_session, request);
    }

    private static async Task ObserveCloseRequestAsync(DialogSession session, DialogCloseRequest request)
    {
        try
        {
            await session.RequestCloseAsync(request);
        }
        catch (Exception ex)
        {
            if (!session.Completion.IsCompleted)
            {
                ReportUnhandledException(ex);
            }
        }
    }

    internal static void ReportUnhandledException(Exception exception)
    {
        Dispatcher.UIThread.Post(() => ExceptionDispatchInfo.Capture(exception).Throw());
    }

    internal void NotifySessionOpened()
    {
        Opened?.Invoke(this, EventArgs.Empty);
    }

    internal void NotifySessionCloseVetoed()
    {
        if (!IsOpen)
        {
            SetCurrentValue(IsOpenProperty, true);
        }
    }

    internal void NotifySessionCloseCommitted(DialogCloseRequest request)
    {
        Exception? firstException = null;
        try
        {
            if (IsOpen)
            {
                SetCurrentValue(IsOpenProperty, false);
            }
        }
        catch (Exception ex)
        {
            firstException = ex;
        }

        try
        {
            if (request.Result is DialogCode.Accepted)
            {
                Accepted?.Invoke(this, EventArgs.Empty);
            }
            else if (request.Result is DialogCode.Rejected)
            {
                Rejected?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            firstException ??= ex;
        }

        try
        {
            Finished?.Invoke(this, new DialogFinishedEventArgs(request.Result));
        }
        catch (Exception ex)
        {
            firstException ??= ex;
        }

        if (firstException is not null)
        {
            ExceptionDispatchInfo.Capture(firstException).Throw();
        }
    }

    internal void NotifySessionClosed()
    {
        Exception? firstException = null;
        try
        {
            if (DataContext is IDialogAwareDataContext dialogAwareDataContext)
            {
                dialogAwareDataContext.NotifyClosed();
            }
        }
        catch (Exception ex)
        {
            firstException = ex;
        }

        try
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            firstException ??= ex;
        }

        if (firstException is not null)
        {
            ExceptionDispatchInfo.Capture(firstException).Throw();
        }
    }

    /// <summary>
    /// Called when a property of the dialog changes. Recomputes <see cref="EffectiveMinimizable" /> when
    /// <see cref="IsModal" /> or <see cref="IsMinimizable" /> changes, and notifies the previous and the new data
    /// context, when they implement <see cref="IDialogAwareDataContext" />, when <c>DataContext</c> changes.
    /// Call the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsModalProperty || change.Property == IsMinimizableProperty)
        {
            SetCurrentValue(EffectiveMinimizableProperty, !IsModal && IsMinimizable);
        }
        else if (change.Property == DataContextProperty)
        {
            if (change.OldValue is IDialogAwareDataContext oldDataContext)
            {
                oldDataContext.NotifyDetachedFromDialog();
            }

            if (change.NewValue is IDialogAwareDataContext newDataContext)
            {
                newDataContext.NotifyAttachedToDialog(this);
            }
        }
    }

    /// <summary>
    /// Called when the dialog is attached to a visual tree. When <see cref="IsOpen" /> is set and no session
    /// exists, it posts the reconciliation that starts the session, so that an open request made before the dialog
    /// was attached is carried out once the dialog is attached. Call the base member first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (IsOpen && _session is null)
        {
            Dispatcher.Post(ReconcilePostedIntent, DispatcherPriority.Normal);
        }
    }

    /// <summary>
    /// Called when the dialog is detached from a visual tree. When a session exists, it requests a forced close
    /// with a <see langword="null" /> result and the reason
    /// <see cref="DialogCloseReason.PlacementTargetDetached" />, which <see cref="Closing" /> handlers cannot veto.
    /// Call the base member first.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_session is not null)
        {
            _ = ObserveCloseRequestAsync(
                _session,
                new DialogCloseRequest(
                    null,
                    DialogCloseReason.PlacementTargetDetached,
                    null,
                    IsForced: true));
        }
    }

    internal Point CalculatePlacementOffset(Size hostSize, Size ownerSize)
    {
        var x = CalculateHorizontalPlacementOffset(hostSize, ownerSize) + OffsetX;
        var y = CalculateVerticalPlacementOffset(hostSize, ownerSize) + OffsetY;
        return new Point(x, y);
    }

    private double CalculateHorizontalPlacementOffset(Size hostSize, Size ownerSize)
    {
        return HorizontalStartupLocation switch
        {
            DialogHorizontalAnchor.Left   => 0,
            DialogHorizontalAnchor.Center => Math.Max((ownerSize.Width - hostSize.Width) / 2, 0),
            DialogHorizontalAnchor.Right  => Math.Max(ownerSize.Width - hostSize.Width, 0),
            _                             => HorizontalOffset?.Resolve(ownerSize.Width) ?? 0
        };
    }

    private double CalculateVerticalPlacementOffset(Size hostSize, Size ownerSize)
    {
        return VerticalStartupLocation switch
        {
            DialogVerticalAnchor.Top    => 0,
            DialogVerticalAnchor.Center => Math.Max((ownerSize.Height - hostSize.Height) / 2, 0),
            DialogVerticalAnchor.Bottom => Math.Max(ownerSize.Height - hostSize.Height, 0),
            _                           => VerticalOffset?.Resolve(ownerSize.Height) ?? 0
        };
    }

    internal bool NotifySurfaceButtonClicked(DialogButton button)
    {
        var buttonClickedArgs = new DialogButtonClickedEventArgs(button);
        ButtonClicked?.Invoke(this, buttonClickedArgs);
        return !buttonClickedArgs.Handled;
    }

}
