using Avalonia;

namespace AtomUI.Desktop.Controls;

internal class SelectCandidateListItem : ListItem
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