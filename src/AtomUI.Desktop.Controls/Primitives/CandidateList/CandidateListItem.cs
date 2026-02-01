using Avalonia;

namespace AtomUI.Desktop.Controls.Primitives;

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
}