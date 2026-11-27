using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The <see cref="AbstractAutoComplete" /> whose text box is a <see cref="SearchEdit" /> with a search button.
/// The user can pick a suggestion or ask for a search with the entered text.
/// </summary>
public partial class AutoCompleteSearchEdit : CompactSpaceAwareAutoComplete
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SearchButtonStyle" /> property.
    /// </summary>
    public static readonly StyledProperty<SearchButtonType> SearchButtonStyleProperty =
        SearchEdit.SearchButtonStyleProperty.AddOwner<AutoCompleteSearchEdit>();

    /// <summary>
    /// Identifies the <see cref="SearchButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string> SearchButtonTextProperty =
        SearchEdit.SearchButtonTextProperty.AddOwner<AutoCompleteSearchEdit>();

    /// <summary>
    /// Identifies the <see cref="IsOperating" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOperatingProperty =
        SearchEdit.IsOperatingProperty.AddOwner<AutoCompleteSearchEdit>();

    /// <summary>
    /// Identifies the <see cref="IsSearchOnEnterEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSearchOnEnterEnabledProperty =
        SearchEdit.IsSearchOnEnterEnabledProperty.AddOwner<AutoCompleteSearchEdit>();
    
    /// <summary>
    /// The look of the search button at the end of the input: <see cref="SearchButtonType.Default" /> or
    /// <see cref="SearchButtonType.Primary" />. The value is passed to the inner search box.
    /// </summary>
    public SearchButtonType SearchButtonStyle
    {
        get => GetValue(SearchButtonStyleProperty);
        set => SetValue(SearchButtonStyleProperty, value);
    }

    /// <summary>
    /// The caption of the search button. The value is passed to the inner search box, which shows it as the
    /// content of the button.
    /// </summary>
    public object? SearchButtonText
    {
        get => GetValue(SearchButtonTextProperty);
        set => SetValue(SearchButtonTextProperty, value);
    }

    /// <summary>
    /// Set to <see langword="true" /> while a search runs: the search button then shows its loading state and
    /// <see cref="SearchRequested" /> is not raised. The default is <see langword="false" />.
    /// </summary>
    public bool IsOperating
    {
        get => GetValue(IsOperatingProperty);
        set => SetValue(IsOperatingProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> raises <see cref="SearchRequested" /> when the user presses Enter in the input and
    /// the key press is not handled earlier; <see langword="false" /> leaves the search button as the only trigger.
    /// The default is <see langword="true" />.
    /// </summary>
    public bool IsSearchOnEnterEnabled
    {
        get => GetValue(IsSearchOnEnterEnabledProperty);
        set => SetValue(IsSearchOnEnterEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when the user clicks the search button, or presses Enter in the input while
    /// <see cref="IsSearchOnEnterEnabled" /> is true and the key press is not handled earlier. It is not raised
    /// while <see cref="IsOperating" /> is true. The event bubbles up from the inner search box and carries the
    /// entered text.
    /// </summary>
    public event EventHandler<SearchRequestedEventArgs>? SearchRequested
    {
        add => AddHandler(SearchEdit.SearchRequestedEvent, value);
        remove => RemoveHandler(SearchEdit.SearchRequestedEvent, value);
    }

    #endregion
    
    private AutoCompleteSearchEditBox? _searchEditBox;

    /// <summary>
    /// Finds the search box of the template after the base implementation has run, so that
    /// <see cref="GetBorderThicknessForCompactSpace" /> can read its border width.
    /// An override must call the base implementation, which binds the text box and the suggestion list parts.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _searchEditBox = e.NameScope.Find<AutoCompleteSearchEditBox>(AutoCompleteThemeConstants.TextBoxPart);
    }
    
    /// <summary>
    /// Returns the border width of the search box, or 0 when the template has no search box that provides one.
    /// A compact <c>Space</c> reads this value from the control.
    /// </summary>
    protected override double GetBorderThicknessForCompactSpace()
    {
        if (_searchEditBox is ICompactSpaceAware compactSpaceAware)
        {
            return compactSpaceAware.GetBorderThickness();
        }
        return 0.0d;
    }
}
