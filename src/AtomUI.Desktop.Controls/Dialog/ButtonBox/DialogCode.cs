namespace AtomUI.Desktop.Controls;

/// <summary>
/// The outcome a dialog reports when it closes. A close that is neither accepted nor rejected carries no code.
/// </summary>
public enum DialogCode
{
    /// <summary>
    /// The dialog was accepted: <see cref="Dialog.Accept" /> was called, or a button whose
    /// <see cref="DialogButton.Role" /> is AcceptRole, YesRole, ApplyRole or ResetRole was clicked.
    /// </summary>
    Accepted,
    /// <summary>
    /// The dialog was rejected: <see cref="Dialog.Reject" /> was called, or a button whose
    /// <see cref="DialogButton.Role" /> is RejectRole or NoRole was clicked.
    /// </summary>
    Rejected
}