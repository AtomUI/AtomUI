namespace AtomUI.Desktop.Controls;

/// <summary>
/// A button that carries the information a dialog needs to act on its click: its <see cref="Role" /> and the
/// standard button it stands for. It is styled by the theme of the regular <see cref="Button" />.
/// </summary>
public class DialogButton : Button
{
    /// <summary>
    /// Is always <see cref="Button" />: the dialog button is styled by the theme of the regular button, and no
    /// theme of its own is looked up.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(Button);
    /// <summary>
    /// Gets or sets the standard button this button stands for, or <see langword="null" /> for a button that stands
    /// for none. The button box sets it on the buttons it creates from <see cref="DialogStandardButtons" />, and
    /// the dialog uses it to find the button that the Enter or Escape key triggers.
    /// </summary>
    public DialogStandardButton? StandardButtonType { get; set; }
    /// <summary>
    /// Gets or sets what the click of this button does to the dialog. The default is
    /// <see cref="DialogButtonRole.CustomRole" />, which requests no close.
    /// </summary>
    public DialogButtonRole Role { get; set; } = DialogButtonRole.CustomRole;
    /// <summary>
    /// Gets or sets whether the button box treats this button as its default confirm button. The box sets it, and
    /// the Primary button type, on the standard button whose <see cref="StandardButtonType" /> equals its
    /// <c>DefaultStandardButton</c>. The dialog finds the button that the Enter key triggers by
    /// <see cref="StandardButtonType" />, so setting this flag alone changes no behavior.
    /// </summary>
    public bool IsDefaultConfirmButton { get; set; }
    /// <summary>
    /// Gets or sets whether the button box treats this button as its escape button. The box sets it on the standard
    /// button whose <see cref="StandardButtonType" /> equals its <c>EscapeStandardButton</c>. The dialog finds the
    /// button that the Escape key triggers by <see cref="StandardButtonType" />, so setting this flag alone changes
    /// no behavior.
    /// </summary>
    public bool IsDefaultEscapeButton { get; set; }
}