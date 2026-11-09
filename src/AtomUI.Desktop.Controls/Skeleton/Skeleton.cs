using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Theme;
using Avalonia;

namespace AtomUI.Desktop.Controls;

// Token-owner transparent: Skeleton wraps loaded content (Card and Dialog bodies, Statistic values), which keeps the owner
// of its host. Every Skeleton visual reads SkeletonTokenResource explicitly; the placeholder parts are registered controls.
[TokenOwnerTransparent]
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
