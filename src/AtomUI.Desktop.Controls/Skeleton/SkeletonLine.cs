using AtomUI.Controls.Commons;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A single placeholder bar of a skeleton, for example one row of a <see cref="SkeletonParagraph" />.
/// </summary>
public class SkeletonLine : AbstractSkeletonLine
{
    #region 内部属性定义

    // SkeletonTitle shares the SkeletonLine theme (StyleKey SkeletonLine), where type selectors cannot tell it apart; the
    // theme sizes title lines through this flag.
    internal static readonly StyledProperty<bool> IsTitleProperty =
        AvaloniaProperty.Register<SkeletonLine, bool>(nameof(IsTitle));

    internal bool IsTitle
    {
        get => GetValue(IsTitleProperty);
        set => SetValue(IsTitleProperty, value);
    }

    #endregion
}
