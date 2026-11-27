namespace AtomUI.Desktop.Controls;

/// <summary>
/// Describes a pending close of a dialog and is handed to the <see cref="Dialog.BeforeCloseAsync" /> callback,
/// which returns <see langword="true" /> to let the close proceed or <see langword="false" /> to keep the dialog
/// open. The callback is not called for a close that is forced or already canceled by the
/// <see cref="Dialog.Closing" /> handlers.
/// </summary>
public sealed class DialogClosingContext
{
    internal DialogClosingContext(
        Dialog dialog,
        object? result,
        DialogCloseReason reason,
        DialogButton? sourceButton,
        CancellationToken cancellationToken)
    {
        Dialog            = dialog;
        Result            = result;
        DialogCode        = result is DialogCode code ? code : null;
        Reason            = reason;
        SourceButton      = sourceButton;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets the dialog that is about to close.
    /// </summary>
    public Dialog Dialog { get; }
    /// <summary>
    /// Gets the result that the dialog will report when the close proceeds: the
    /// <see cref="AtomUI.Desktop.Controls.DialogCode" /> when the dialog was accepted or rejected, and otherwise
    /// the value that application code supplied, or <see langword="null" /> when there is none.
    /// </summary>
    public object? Result { get; }
    /// <summary>
    /// Gets the <see cref="AtomUI.Desktop.Controls.DialogCode" /> when <see cref="Result" /> is one, otherwise
    /// <see langword="null" />.
    /// </summary>
    public DialogCode? DialogCode { get; }
    /// <summary>
    /// Gets what initiated the close.
    /// </summary>
    public DialogCloseReason Reason { get; }
    /// <summary>
    /// Gets the dialog button that initiated the close, or <see langword="null" /> when the close was not
    /// initiated by a dialog button.
    /// </summary>
    public DialogButton? SourceButton { get; }
    /// <summary>
    /// Gets a token that is canceled when the pending close is dropped or superseded, for example because a
    /// forced close took over. Pass it to the asynchronous work of the callback so that the work stops then.
    /// </summary>
    public CancellationToken CancellationToken { get; }
}
