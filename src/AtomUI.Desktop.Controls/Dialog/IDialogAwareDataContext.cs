namespace AtomUI.Desktop.Controls;

/// <summary>
/// Implemented by the data context of a dialog to receive notifications when the dialog starts or stops using
/// it and when the dialog has closed. The image previewer sends the closed notification to its data context
/// too.
/// </summary>
public interface IDialogAwareDataContext
{
    /// <summary>
    /// Notifies the object that a dialog now uses it as its data context. The call follows the change, also
    /// when the data context is assigned before the dialog is opened.
    /// </summary>
    /// <param name="dialog">
    /// The dialog whose data context this object has become.
    /// </param>
    void NotifyAttachedToDialog(IDialog dialog);
    /// <summary>
    /// Notifies the object that the dialog no longer uses it as its data context because the data context was
    /// replaced or cleared. The default implementation does nothing.
    /// </summary>
    public void NotifyDetachedFromDialog() {}
    /// <summary>
    /// Notifies the object that the dialog has closed, before the <see cref="Dialog.Closed" /> event is raised.
    /// The default implementation does nothing.
    /// </summary>
    public void NotifyClosed() {}
}