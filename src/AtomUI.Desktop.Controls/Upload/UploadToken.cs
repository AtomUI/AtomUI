using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The design tokens of the upload control: the sizes, margins and paddings of the drop area, the trigger and
/// the list items, and the color of the action icons in a text-list item.
/// </summary>
[ControlDesignToken]
public sealed partial class UploadToken : AbstractControlDesignToken
{
    /// <summary>
    /// Color of the icons of the action buttons in the header of a text-list item, such as the remove button.
    /// A failed item shows them in the error color instead.
    /// </summary>
    public partial Color ActionsColor { get; set; }
    
    /// <summary>
    /// Width and height of picture-card and picture-circle upload items and their trigger.
    /// </summary>
    public partial double PictureCardSize { get; set; }
    
    /// <summary>
    /// Margin around the file header in a text-list item.
    /// </summary>
    public partial Thickness TextListItemMargin { get; set; }
    
    /// <summary>
    /// Padding inside the file name element of a text-list item.
    /// </summary>
    public partial Thickness TextListNamePadding { get; set; }

    /// <summary>
    /// Padding inside the file header of a text-list item.
    /// </summary>
    public partial Thickness TextListItemPadding { get; set; }

    /// <summary>
    /// Height of the upload progress line in a text-list item.
    /// </summary>
    public partial double TextListProgressLineHeight { get; set; }

    /// <summary>
    /// Padding around the upload progress line in a text-list item.
    /// </summary>
    public partial Thickness TextListProgressPadding { get; set; }
    
    /// <summary>
    /// Width and height of the file-type icon in an upload item.
    /// </summary>
    public partial double UploadThumbnailSize { get; set; }
    
    /// <summary>
    /// Size of the icon in the upload drop area.
    /// </summary>
    public partial double DragIconSize { get; set; }
    
    /// <summary>
    /// Margin around the icon in the upload drop area.
    /// </summary>
    public partial Thickness DragIconMargin { get; set; }
    
    /// <summary>
    /// Margin around the main heading in the upload drop area.
    /// </summary>
    public partial Thickness DragHeaderMargin { get; set; }
    
    /// <summary>
    /// Margin around an item in the picture list.
    /// </summary>
    public partial Thickness PictureListItemMargin { get; set; }
    
    /// <summary>
    /// Size reserved for an image preview in a picture-list item.
    /// </summary>
    public partial double PictureListPreviewerSize { get; set; }
    
}
