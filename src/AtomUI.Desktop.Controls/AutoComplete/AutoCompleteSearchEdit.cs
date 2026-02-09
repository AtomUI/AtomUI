using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public class AutoCompleteSearchEdit : CompactSpaceAwareAutoComplete
{
    #region 公共属性定义

    public static readonly StyledProperty<SearchEditButtonStyle> SearchButtonStyleProperty =
        SearchEdit.SearchButtonStyleProperty.AddOwner<AutoCompleteSearchEdit>();

    public static readonly StyledProperty<string> SearchButtonTextProperty =
        SearchEdit.SearchButtonTextProperty.AddOwner<AutoCompleteSearchEdit>();

    public static readonly StyledProperty<bool> IsOperatingProperty =
        SearchEdit.IsOperatingProperty.AddOwner<AutoCompleteSearchEdit>();
    
    public SearchEditButtonStyle SearchButtonStyle
    {
        get => GetValue(SearchButtonStyleProperty);
        set => SetValue(SearchButtonStyleProperty, value);
    }

    public object? SearchButtonText
    {
        get => GetValue(SearchButtonTextProperty);
        set => SetValue(SearchButtonTextProperty, value);
    }

    public bool IsOperating
    {
        get => GetValue(IsOperatingProperty);
        set => SetValue(IsOperatingProperty, value);
    }

    #endregion
    
    private AutoCompleteSearchEditBox? _searchEditBox;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _searchEditBox = e.NameScope.Find<AutoCompleteSearchEditBox>(AutoCompleteThemeConstants.TextBoxPart);
    }
    
    protected override double GetBorderThicknessForCompactSpace()
    {
        if (_searchEditBox is ICompactSpaceAware compactSpaceAware)
        {
            return compactSpaceAware.GetBorderThickness();
        }
        return 0.0d;
    }
}