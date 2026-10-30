using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using AtomUI.Generated.AtomUIDesktopControls;

namespace AtomUI.Desktop.Controls;

internal class SearchEditDecoratedBox : AddOnDecoratedBox
{
    #region 公共属性定义

    public static readonly StyledProperty<SearchButtonType> SearchButtonStyleProperty =
        SearchEdit.SearchButtonStyleProperty.AddOwner<SearchEditDecoratedBox>();

    public static readonly StyledProperty<string> SearchButtonTextProperty =
        SearchEdit.SearchButtonTextProperty.AddOwner<SearchEditDecoratedBox>();

    public static readonly StyledProperty<bool> IsSearchButtonLoadingProperty = AvaloniaProperty.Register<SearchEditDecoratedBox, bool>(
        nameof(IsSearchButtonLoading));

    public static readonly StyledProperty<ControlTheme?> SearchButtonThemeProperty =
        SearchEdit.SearchButtonThemeProperty.AddOwner<SearchEditDecoratedBox>();
    
    public SearchButtonType SearchButtonStyle
    {
        get => GetValue(SearchButtonStyleProperty);
        set => SetValue(SearchButtonStyleProperty, value);
    }

    public object? SearchButtonText
    {
        get => GetValue(SearchButtonTextProperty);
        set => SetValue(SearchButtonTextProperty, value);
    }
    
    public bool IsSearchButtonLoading
    {
        get => GetValue(IsSearchButtonLoadingProperty);
        set => SetValue(IsSearchButtonLoadingProperty, value);
    }

    public ControlTheme? SearchButtonTheme
    {
        get => GetValue(SearchButtonThemeProperty);
        set => SetValue(SearchButtonThemeProperty, value);
    }

    #endregion
    
    private Button? _searchButton;
    internal SearchEdit? OwningSearchEdit { get; set; }

    protected override void NotifyAddOnBorderInfoCalculated()
    {
        RightAddOnBorderThickness = BorderThickness;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_searchButton != null)
        {
            _searchButton.Click -= HandleSearchButtonClick;
        }
        
        _searchButton = e.NameScope.Find<Button>("PART_RightAddOn");

        if (_searchButton != null)
        {
            _searchButton.Click += HandleSearchButtonClick;
            _searchButton.Classes.Add(SearchEditSemanticParts.ButtonClass);
        }
    }

    private void HandleSearchButtonClick(object? sender, RoutedEventArgs e)
    {
        OwningSearchEdit?.RaiseSearchRequested(SearchTriggerSource.Button);
    }
}
