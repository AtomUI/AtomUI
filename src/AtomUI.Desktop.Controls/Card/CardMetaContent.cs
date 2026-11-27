using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Card content that shows an optional avatar to the left of a title and a description. Use it as the content of a
/// <see cref="Card" />: <see cref="HeaderedContentControl.Header" /> becomes the title and
/// <see cref="Avalonia.Controls.ContentControl.Content" /> the description.
/// </summary>
public partial class CardMetaContent : HeaderedContentControl
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="Avatar" /> property.
    /// </summary>
    public static readonly StyledProperty<Avatar?> AvatarProperty = 
        AvaloniaProperty.Register<CardMetaContent, Avatar?>(nameof (Avatar));
    
    /// <summary>
    /// Avatar shown to the left of the title and description, aligned to their top, or
    /// <see langword="null" /> to show no avatar.
    /// </summary>
    public Avatar? Avatar
    {
        get => GetValue(AvatarProperty);
        set => SetValue(AvatarProperty, value);
    }
    
    #endregion
}
