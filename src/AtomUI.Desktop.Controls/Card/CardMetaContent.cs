using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public partial class CardMetaContent : HeaderedContentControl
{
    #region 公共属性定义
    
    public static readonly StyledProperty<Avatar?> AvatarProperty = 
        AvaloniaProperty.Register<CardMetaContent, Avatar?>(nameof (Avatar));
    
    public Avatar? Avatar
    {
        get => GetValue(AvatarProperty);
        set => SetValue(AvatarProperty, value);
    }
    
    #endregion
}
