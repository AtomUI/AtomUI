using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design token layer that holds the corner radius of a popup surface.
/// <see cref="PopupToken" /> and <c>TreeViewFlyoutPresenterToken</c> derive from it, and each design language
/// fills every derived token through its popup corner calculator.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractPopupCornerToken : AbstractControlDesignToken
{
    /// <summary>
    /// Corner radius of the popup surface.
    /// </summary>
    public partial CornerRadius PopupCornerRadius { get; set; }
}
