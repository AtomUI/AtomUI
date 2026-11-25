using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class UploadToken : AbstractControlDesignToken
{
    /// <summary>
    /// 操作按扭颜色
    /// Action button color
    /// </summary>
    public partial Color ActionsColor { get; set; }
    
    /// <summary>Width and height of picture-card and picture-circle upload items and their trigger.</summary>
    public partial double PictureCardSize { get; set; }
    
    /// <summary>Margin around the file header in a text-list item.</summary>
    public partial Thickness TextListItemMargin { get; set; }
    
    /// <summary>Padding inside the file name element of a text-list item.</summary>
    public partial Thickness TextListNamePadding { get; set; }

    /// <summary>Padding inside the file header of a text-list item.</summary>
    public partial Thickness TextListItemPadding { get; set; }

    /// <summary>Height of the upload progress line in a text-list item.</summary>
    public partial double TextListProgressLineHeight { get; set; }

    /// <summary>Padding around the upload progress line in a text-list item.</summary>
    public partial Thickness TextListProgressPadding { get; set; }
    
    /// <summary>Width and height of the file-type icon in an upload item.</summary>
    public partial double UploadThumbnailSize { get; set; }
    
    /// <summary>Size of the icon in the upload drop area.</summary>
    public partial double DragIconSize { get; set; }
    
    /// <summary>Margin around the icon in the upload drop area.</summary>
    public partial Thickness DragIconMargin { get; set; }
    
    /// <summary>Margin around the main heading in the upload drop area.</summary>
    public partial Thickness DragHeaderMargin { get; set; }
    
    /// <summary>Margin around an item in the picture list.</summary>
    public partial Thickness PictureListItemMargin { get; set; }
    
    /// <summary>Size reserved for an image preview in a picture-list item.</summary>
    public partial double PictureListPreviewerSize { get; set; }
    
}
