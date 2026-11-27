using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of a message box: those of the dialog token it extends and the size of the status icon.
/// </summary>
[ControlDesignToken]
public sealed partial class MessageBoxToken : AbstractDialogToken
{
    /// <summary>
    /// Width and height of the message box status icon.
    /// </summary>
    public partial double StyleIconSize { get; set; }
}
