using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="GroupBox" />: the inset of the content and the spacing and title
/// font weight of the header.
/// </summary>
[ControlDesignToken]
public sealed partial class GroupBoxToken : AbstractControlDesignToken
{

    /// <summary>
    /// Inset of the group's content inside its frame.
    /// </summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>
    /// Space around the group header container.
    /// </summary>
    public partial Thickness HeaderContainerMargin { get; set; }

    /// <summary>
    /// Space around the optional header icon beside the label.
    /// </summary>
    public partial Thickness HeaderIconMargin { get; set; }

    /// <summary>
    /// Padding around the content inside the group header.
    /// </summary>
    public partial Thickness HeaderContentPadding { get; set; }

    /// <summary>
    /// Font weight of the title in the group header.
    /// </summary>
    public partial FontWeight HeaderFontWeight { get; set; }
    
}
