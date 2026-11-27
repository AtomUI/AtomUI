namespace AtomUI.Desktop.Controls;

/// <summary>
/// The behavior a button contributes to a dialog when it is clicked, stored in <see cref="DialogButton.Role" />.
/// Unless a <see cref="Dialog.ButtonClicked" /> handler marks the click handled, AcceptRole, YesRole, ApplyRole
/// and ResetRole make the dialog request a close with <see cref="DialogCode.Accepted" />, and RejectRole and
/// NoRole make it request a close with <see cref="DialogCode.Rejected" />. The other roles request no close.
/// </summary>
public enum DialogButtonRole
{
    /// <summary>
    /// Clicking the button causes the dialog to be accepted (e.g. OK).
    /// </summary>
    AcceptRole,
    /// <summary>
    /// Clicking the button causes the dialog to be rejected (e.g. Cancel).
    /// </summary>
    RejectRole,
    /// <summary>
    /// Clicking the button causes a destructive change (e.g. for Discarding Changes). The click does not request
    /// the dialog to close.
    /// </summary>
    DestructiveRole,
    /// <summary>
    /// Clicking the button causes changes to the elements within the dialog.
    /// </summary>
    ActionRole,
    /// <summary>
    /// The button can be clicked to request help.
    /// </summary>
    HelpRole,
    /// <summary>
    /// The button is a "Yes"-like button. Clicking it requests the dialog to close with the result
    /// <see cref="DialogCode.Accepted" />.
    /// </summary>
    YesRole,
    /// <summary>
    /// The button is a "No"-like button. Clicking it requests the dialog to close with the result
    /// <see cref="DialogCode.Rejected" />.
    /// </summary>
    NoRole,
    /// <summary>
    /// The button applies current changes. The click also requests the dialog to close with the result
    /// <see cref="DialogCode.Accepted" />.
    /// </summary>
    ApplyRole,
    /// <summary>
    /// The button resets the dialog's fields to default values. The click also requests the dialog to close with the
    /// result <see cref="DialogCode.Accepted" />.
    /// </summary>
    ResetRole,
    /// <summary>
    /// User-defined button roles
    /// </summary>
    CustomRole
}