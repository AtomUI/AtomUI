using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class MessageBoxToken : AbstractDialogToken
{
    /// <summary>Width and height of the message box status icon.</summary>
    public partial double StyleIconSize { get; set; }
}
