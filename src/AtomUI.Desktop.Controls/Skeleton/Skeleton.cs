using AtomUI.Controls.Commons;
using AtomUI.Theme;
using Avalonia;

namespace AtomUI.Desktop.Controls;

// Token-owner transparent: Skeleton wraps loaded content (Card and Dialog bodies, Statistic values), which keeps the owner
// of its host. Every Skeleton visual reads SkeletonTokenResource explicitly; the placeholder parts are registered controls.
/// <summary>
/// The composite skeleton: an optional avatar, a title and a paragraph of placeholder blocks that stand in for
/// <see cref="AbstractCompositeSkeleton.Content" /> while <see cref="AbstractCompositeSkeleton.IsLoading" /> is
/// true.
/// </summary>
[TokenOwnerTransparent]
public partial class Skeleton : AbstractCompositeSkeleton
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="AvatarSizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> AvatarSizeTypeProperty =
        AvaloniaProperty.Register<Skeleton, CustomizableSizeType>(nameof(AvatarSizeType), CustomizableSizeType.Middle);

    /// <summary>
    /// Size step of the avatar placeholder. The theme sets <c>Large</c>; a value set on the control replaces it. It
    /// has no effect while <see cref="AbstractCompositeSkeleton.IsShowAvatar" /> is false.
    /// </summary>
    public CustomizableSizeType AvatarSizeType
    {
        get => GetValue(AvatarSizeTypeProperty);
        set => SetValue(AvatarSizeTypeProperty, value);
    }

    #endregion
}
