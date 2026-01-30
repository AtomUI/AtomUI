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
}