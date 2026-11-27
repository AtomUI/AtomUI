namespace AtomUI.Desktop.Controls;

/// <summary>
/// Describes the click of a dialog button. It is raised with <see cref="Dialog.ButtonClicked" /> and with the
/// <c>Clicked</c> event of the button box.
/// </summary>
public class DialogButtonClickedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the button that was clicked.
    /// </summary>
    public DialogButton SourceButton { get; }
    
    /// <summary>
    /// Gets or sets whether a handler consumed the click. <see cref="Dialog.ButtonClicked" /> is raised before
    /// the dialog acts on the <see cref="DialogButton.Role" /> of the button. A handler that sets this to
    /// <see langword="true" /> stops the dialog from requesting a close for the click.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Initializes the arguments for a click of the given button.
    /// </summary>
    /// <param name="sourceButton">
    /// The button that was clicked.
    /// </param>
    public DialogButtonClickedEventArgs(DialogButton sourceButton)
    {
        SourceButton = sourceButton;
    }
}