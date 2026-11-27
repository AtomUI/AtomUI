using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design token of the <see cref="PopupConfirm" /> control: the spacing of the action buttons of the confirmation
/// popup and the margins of its icon, title, description and action buttons.
/// </summary>
[ControlDesignToken]
public sealed partial class PopupConfirmToken : AbstractControlDesignToken
{

    /// <summary>
    /// Gap between the popup's action buttons.
    /// </summary>
    public partial double ButtonSpacing { get; set; }

    /// <summary>
    /// Margin around the confirmation icon.
    /// </summary>
    public partial Thickness IconMargin { get; set; }

    /// <summary>
    /// Margin around the main content container.
    /// </summary>
    public partial Thickness ContentContainerMargin { get; set; }

    /// <summary>
    /// Margin around the action button container. The theme uses 0 instead while ConfirmContent is null.
    /// </summary>
    public partial Thickness ButtonContainerMargin { get; set; }

    /// <summary>
    /// Margin around the popup title.
    /// </summary>
    public partial Thickness TitleMargin { get; set; }
    
}
