using AtomUI.Controls;
using AtomUI.Controls.Commons;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public partial class Skeleton : AbstractCompositeSkeleton
{
    #region 公共属性定义

    public static readonly StyledProperty<CustomizableSizeType> AvatarSizeTypeProperty =
        AvaloniaProperty.Register<Skeleton, CustomizableSizeType>(nameof(AvatarSizeType), CustomizableSizeType.Middle);

    public CustomizableSizeType AvatarSizeType
    {
        get => GetValue(AvatarSizeTypeProperty);
        set => SetValue(AvatarSizeTypeProperty, value);
    }

    #endregion
}
