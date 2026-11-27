using System.ComponentModel;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The title, the result, the open and close events and the close requests of a dialog. <see cref="Dialog" />
/// implements it and passes it to <see cref="IDialogAwareDataContext.NotifyAttachedToDialog" />.
/// </summary>
public interface IDialog
{
    /// <summary>
    /// The text shown in the header of the dialog.
    /// </summary>
    string? Title { get; set; }
    /// <summary>
    /// The value the dialog closes with. Calling <c>Done()</c> closes the dialog with the current value, and every
    /// close replaces the value with the result of its close request before <see cref="Finished" /> is raised.
    /// </summary>
    object? Result { get; set; }

    /// <summary>
    /// Raised when the dialog has closed and its host window or overlay has been closed. It is the last event of
    /// a close and follows <see cref="Finished" />.
    /// </summary>
    public event EventHandler? Closed;
    /// <summary>
    /// Raised when the dialog has been shown and has received the input focus.
    /// </summary>
    public event EventHandler? Opened;
    /// <summary>
    /// Raised when a close is requested, before the dialog starts closing. Set <see cref="CancelEventArgs.Cancel" />
    /// to keep the dialog open. A forced close raises the event too but closes the dialog regardless of
    /// <see cref="CancelEventArgs.Cancel" />, as when the dialog is detached from the visual tree while it is open.
    /// </summary>
    public event EventHandler<CancelEventArgs>? Closing;
    /// <summary>
    /// Raised when the dialog closes with the result <see cref="DialogCode.Accepted" />, after <see cref="Accept" />
    /// or after the click of a button whose role requests this result. The close is already committed and cannot
    /// be vetoed at this point; <see cref="Finished" /> is raised next.
    /// </summary>
    public event EventHandler? Accepted;
    /// <summary>
    /// Raised when the dialog closes with the result <see cref="DialogCode.Rejected" />, after <see cref="Reject" />
    /// or after the click of a button whose role requests this result. The close is already committed and cannot
    /// be vetoed at this point; <see cref="Finished" /> is raised next.
    /// </summary>
    public event EventHandler? Rejected;
    /// <summary>
    /// Raised for every close that is committed, after <see cref="Accepted" /> or <see cref="Rejected" /> and
    /// before the host of the dialog is closed. The event arguments carry the result of the close request, which
    /// is <see langword="null" /> when the dialog closed without a result.
    /// </summary>
    public event EventHandler<DialogFinishedEventArgs>? Finished;
    /// <summary>
    /// Raised when a button of the dialog is clicked, before the role of the button acts on the click. Set
    /// <see cref="DialogButtonClickedEventArgs.Handled" /> to keep the role of the button from requesting a close.
    /// </summary>
    public event EventHandler<DialogButtonClickedEventArgs>? ButtonClicked;

    /// <summary>
    /// Requests the dialog to close with the result <see cref="DialogCode.Accepted" />. The request raises
    /// <see cref="Closing" /> and can be vetoed there; it has no effect unless the dialog is open.
    /// </summary>
    void Accept();
    /// <summary>
    /// Requests the dialog to close with the result <see cref="DialogCode.Rejected" />. The request raises
    /// <see cref="Closing" /> and can be vetoed there; it has no effect unless the dialog is open.
    /// </summary>
    void Reject();
    /// <summary>
    /// Requests the dialog to close with <paramref name="result" /> as the result. The request raises
    /// <see cref="Closing" /> and can be vetoed there; it has no effect unless the dialog is open.
    /// </summary>
    /// <param name="result">
    /// The value the dialog closes with.
    /// </param>
    void Done(object? result);
    /// <summary>
    /// Requests the dialog to close with the current value of <see cref="Result" /> as the result. The request
    /// raises <see cref="Closing" /> and can be vetoed there; it has no effect unless the dialog is open.
    /// </summary>
    void Done();
}