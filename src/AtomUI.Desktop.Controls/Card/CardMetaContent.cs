using AtomUI.Theme.Resources;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public partial class CardMetaContent : HeaderedContentControl, IThemeInheritanceSource
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
    #region 实现 IThemeInheritanceSource 接口

    // ControlTheme cannot read the protected inheritance parent; its Setters own
    // optional typography while this bridge exposes the actual native source.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

}
