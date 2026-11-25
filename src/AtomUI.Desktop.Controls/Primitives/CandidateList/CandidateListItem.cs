using Avalonia;
using AtomUI.Theme;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Primitives;

// Token-owner transparent item container (as ListBoxItem): the items read the hosting list's owner globals.
[TokenOwnerTransparent]
public class CandidateListItem : ListBoxItem
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsCandidateSelectedProperty =
        AvaloniaProperty.Register<CandidateListItem, bool>(nameof(IsCandidateSelected));

    public bool IsCandidateSelected
    {
        get => GetValue(IsCandidateSelectedProperty);
        set => SetValue(IsCandidateSelectedProperty, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<Thickness> CandidateIndicatorMarginProperty =
        AvaloniaProperty.Register<CandidateListItem, Thickness>(nameof(CandidateIndicatorMargin));
    internal static readonly StyledProperty<Thickness> CandidatePaddingProperty =
        AvaloniaProperty.Register<CandidateListItem, Thickness>(nameof(CandidatePadding));
    internal static readonly StyledProperty<IBrush?> ItemColorProperty =
        CandidateList.ItemColorProperty.AddOwner<CandidateListItem>();
    internal static readonly StyledProperty<IBrush?> ItemSelectedColorProperty =
        CandidateList.ItemSelectedColorProperty.AddOwner<CandidateListItem>();

    internal static readonly StyledProperty<Thickness> ItemMarginProperty =
        CandidateList.ItemMarginProperty.AddOwner<CandidateListItem>();
    internal static readonly StyledProperty<CornerRadius> ItemCornerRadiusProperty =
        CandidateList.ItemCornerRadiusProperty.AddOwner<CandidateListItem>();
    internal static readonly StyledProperty<double> ItemMinHeightProperty =
        CandidateList.ItemMinHeightProperty.AddOwner<CandidateListItem>();

    internal double ItemMinHeight
    {
        get => GetValue(ItemMinHeightProperty);
        set => SetValue(ItemMinHeightProperty, value);
    }

    internal Thickness ItemMargin
    {
        get => GetValue(ItemMarginProperty);
        set => SetValue(ItemMarginProperty, value);
    }

    internal CornerRadius ItemCornerRadius
    {
        get => GetValue(ItemCornerRadiusProperty);
        set => SetValue(ItemCornerRadiusProperty, value);
    }

    internal IBrush? ItemColor
    {
        get => GetValue(ItemColorProperty);
        set => SetValue(ItemColorProperty, value);
    }

    internal IBrush? ItemSelectedColor
    {
        get => GetValue(ItemSelectedColorProperty);
        set => SetValue(ItemSelectedColorProperty, value);
    }

    internal Thickness CandidateIndicatorMargin
    {
        get => GetValue(CandidateIndicatorMarginProperty);
        set => SetValue(CandidateIndicatorMarginProperty, value);
    }


    internal Thickness CandidatePadding
    {
        get => GetValue(CandidatePaddingProperty);
        set => SetValue(CandidatePaddingProperty, value);
    }

    #endregion
}