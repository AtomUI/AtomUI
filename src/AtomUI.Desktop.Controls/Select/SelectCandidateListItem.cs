using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class SelectCandidateListItem : ListViewItem
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsCandidateSelectedProperty =
        AvaloniaProperty.Register<SelectCandidateListItem, bool>(nameof(IsCandidateSelected));

    public bool IsCandidateSelected
    {
        get => GetValue(IsCandidateSelectedProperty);
        set => SetValue(IsCandidateSelectedProperty, value);
    }
    #endregion

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BoundsProperty &&
            change.GetOldValue<Rect>().Height != change.GetNewValue<Rect>().Height &&
            ItemsControl.ItemsControlFromItemContainer(this) is SelectCandidateList layoutOwner)
        {
            layoutOwner.NotifyCandidateRowSizeChanged(this);
        }
        if (change.Property == IsEnabledProperty ||
            change.Property == IsVisibleProperty ||
            change.Property == IsGroupItemProperty ||
            change.Property == IsSelectedProperty)
        {
            if (ItemsControl.ItemsControlFromItemContainer(this) is SelectCandidateList owner)
            {
                owner.NotifyCandidateContainerAvailabilityChanged(this);
            }
        }
    }

    #region 内部属性定义

    internal static readonly DirectProperty<SelectCandidateListItem, bool> IsHideSelectedOptionsProperty =
        AvaloniaProperty.RegisterDirect<SelectCandidateListItem, bool>(
            nameof(IsHideSelectedOptions),
            o => o.IsHideSelectedOptions,
            (o, v) => o.IsHideSelectedOptions = v);

    private bool _isHideSelectedOptions;

    internal bool IsHideSelectedOptions
    {
        get => _isHideSelectedOptions;
        set => SetAndRaise(IsHideSelectedOptionsProperty, ref _isHideSelectedOptions, value);
    }
    #endregion
}
