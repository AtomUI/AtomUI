using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="Descriptions" />: the colors of the title, the extra area and the item labels
/// and content, the margin of the header and the padding of the bordered item cells by size.
/// </summary>
[ControlDesignToken]
public sealed partial class DescriptionsToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Background of an item label in the bordered layout.
    /// </summary>
    public partial Color LabelBg { get; set; }

    /// <summary>
    /// Text color of an item label and of the colon that follows it.
    /// </summary>
    public partial Color LabelColor { get; set; }

    /// <summary>
    /// Text color of the header title.
    /// </summary>
    public partial Color TitleColor { get; set; }

    /// <summary>
    /// Margin of the header bar that holds the title and the extra area.
    /// </summary>
    public partial Thickness HeaderMargin { get; set; }

    /// <summary>
    /// Padding of an item cell in the bordered layout, for the large size.
    /// </summary>
    public partial Thickness ItemPaddingLG { get; set; }
    
    /// <summary>
    /// Padding of an item cell in the bordered layout, for the medium size.
    /// </summary>
    public partial Thickness ItemPadding { get; set; }
    
    /// <summary>
    /// Padding of an item cell in the bordered layout, for the small size.
    /// </summary>
    public partial Thickness ItemPaddingSM { get; set; }

    /// <summary>
    /// Margin of the colon that follows a label.
    /// </summary>
    public partial Thickness ColonMargin { get; set; }

    /// <summary>
    /// Text color of the content of an item.
    /// </summary>
    public partial Color ContentColor { get; set; }

    /// <summary>
    /// Text color of the extra area in the header.
    /// </summary>
    public partial Color ExtraColor { get; set; }
    
}
