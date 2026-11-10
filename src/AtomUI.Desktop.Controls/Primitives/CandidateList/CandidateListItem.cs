using Avalonia;
using AtomUI.Theme;

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
}