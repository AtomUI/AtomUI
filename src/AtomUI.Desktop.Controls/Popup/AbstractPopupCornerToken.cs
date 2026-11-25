using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractPopupCornerToken : AbstractControlDesignToken
{
    /// <summary>Corner radius of the popup surface.</summary>
    public partial CornerRadius PopupCornerRadius { get; set; }
}
