using AtomUI.Controls.Commons;
using Avalonia;

namespace AtomUI.Desktop.Controls;

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
