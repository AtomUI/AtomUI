namespace AtomUI.Desktop.Controls;

/// <summary>
/// States what initiated the close of a dialog. The value is available to the
/// <see cref="Dialog.BeforeCloseAsync" /> callback through <see cref="DialogClosingContext.Reason" />.
/// </summary>
public enum DialogCloseReason
{
    /// <summary>
    /// The dialog was closed through <see cref="Dialog.Accept" />, with <see cref="DialogCode.Accepted" /> as
    /// the result.
    /// </summary>
    Accepted,
    /// <summary>
    /// The dialog was closed through <see cref="Dialog.Reject" />, with <see cref="DialogCode.Rejected" /> as
    /// the result.
    /// </summary>
    Rejected,
    /// <summary>
    /// The host of the dialog requested the close on behalf of the user: the overlay surface raised its host
    /// close request, or the window host reported a close request.
    /// </summary>
    HostCloseRequest,
    /// <summary>
    /// Application code ended the dialog: through <c>Dialog.Done</c>, by clearing <c>IsOpen</c>, or by
    /// cancelling the token passed to the show method. The last one force-closes the dialog: the
    /// <see cref="Dialog.Closing" /> handlers cannot cancel it and <see cref="Dialog.BeforeCloseAsync" /> is not
    /// called.
    /// </summary>
    Programmatic,
    /// <summary>
    /// The window that owns the dialog closed, which closes the dialog as well.
    /// </summary>
    OwnerClosed,
    /// <summary>
    /// The placement target of the dialog was detached from the visual tree, which closes the dialog as well.
    /// </summary>
    PlacementTargetDetached
}
