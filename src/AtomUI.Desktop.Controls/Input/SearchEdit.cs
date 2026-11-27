using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The look of the search button at the end of a <see cref="SearchEdit" />.
/// </summary>
public enum SearchButtonType
{
    /// <summary>
    /// The search button has the look of a default button.
    /// </summary>
    Default,
    /// <summary>
    /// The search button has the look of a primary button.
    /// </summary>
    Primary
}

/// <summary>
/// A <see cref="LineEdit" /> with a search button attached to the end of the input. Clicking the button raises
/// <see cref="SearchRequested" />; pressing Enter raises it as well while <see cref="IsSearchOnEnterEnabled" />
/// is <see langword="true" />.
/// </summary>
public partial class SearchEdit : LineEdit
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SearchButtonStyle" /> property.
    /// </summary>
    public static readonly StyledProperty<SearchButtonType> SearchButtonStyleProperty =
        AvaloniaProperty.Register<SearchEdit, SearchButtonType>(nameof(SearchButtonStyle));

    /// <summary>
    /// Identifies the <see cref="SearchButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string> SearchButtonTextProperty =
        AvaloniaProperty.Register<SearchEdit, string>(nameof(SearchButtonText));

    /// <summary>
    /// Identifies the <see cref="IsOperating" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOperatingProperty =
        AvaloniaProperty.Register<SearchEdit, bool>(nameof(IsOperating));

    /// <summary>
    /// Identifies the <see cref="SearchButtonTheme" /> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> SearchButtonThemeProperty =
        AvaloniaProperty.Register<SearchEdit, ControlTheme?>(nameof(SearchButtonTheme));
    
    /// <summary>
    /// Identifies the <see cref="IsSearchOnEnterEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSearchOnEnterEnabledProperty =
        AvaloniaProperty.Register<SearchEdit, bool>(nameof(IsSearchOnEnterEnabled), true);

    /// <summary>
    /// The look of the search button at the end of the input: <see cref="SearchButtonType.Default" /> or
    /// <see cref="SearchButtonType.Primary" />. The value is passed to the inner search box. The default is
    /// <see cref="SearchButtonType.Default" />.
    /// </summary>
    public SearchButtonType SearchButtonStyle
    {
        get => GetValue(SearchButtonStyleProperty);
        set => SetValue(SearchButtonStyleProperty, value);
    }

    /// <summary>
    /// The caption of the search button, shown next to its search icon. The value is passed to the inner search box,
    /// which shows it as the content of the button. When it is <see langword="null" />, the button shows only the icon.
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
    /// The theme of the search button at the end of the input. The value is passed to the inner search box. When the
    /// property is not set, the control theme supplies its own search button theme.
    /// </summary>
    public ControlTheme? SearchButtonTheme
    {
        get => GetValue(SearchButtonThemeProperty);
        set => SetValue(SearchButtonThemeProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> raises <see cref="SearchRequested" /> when the user presses Enter in the input and the
    /// key press is not handled earlier; <see langword="false" /> leaves the search button as the only trigger.
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
    /// Identifies the <see cref="SearchRequested" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<SearchRequestedEventArgs> SearchRequestedEvent =
        RoutedEvent.Register<SearchEdit, SearchRequestedEventArgs>(
            nameof(SearchRequested),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Raised when the user clicks the search button, or presses Enter in the input while
    /// <see cref="IsSearchOnEnterEnabled" /> is <see langword="true" /> and the key press is neither handled nor
    /// suppressed. The event bubbles up the element tree. It is not raised while <see cref="IsOperating" /> is
    /// <see langword="true" />.
    /// </summary>
    /// <remarks>
    /// <see cref="SearchRequestedEventArgs.Query" /> carries the current text, an empty string when the input is empty,
    /// and <see cref="SearchRequestedEventArgs.Trigger" /> tells what started the search.
    /// </remarks>
    public event EventHandler<SearchRequestedEventArgs>? SearchRequested
    {
        add => AddHandler(SearchRequestedEvent, value);
        remove => RemoveHandler(SearchRequestedEvent, value);
    }

    #endregion
    
    /// <summary>
    /// Calls the base implementation, which already applies the same default, then sets
    /// <see cref="AbstractTextInput.ClearIcon" /> to a <see cref="CloseCircleFilled" /> icon when it is still
    /// <see langword="null" />. A value that is already set is kept.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (ClearIcon == null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }
    }

    /// <summary>
    /// Calls the base implementation, then makes this control the owner of the search box of the template, so that a
    /// click on its search button raises <see cref="SearchRequested" /> here. An override must call the base
    /// implementation.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var decoratedBox = e.NameScope.Find<SearchEditDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
        if (decoratedBox != null)
        {
            decoratedBox.OwningSearchEdit = this;
        }
    }

    /// <summary>
    /// Raises <see cref="SearchRequested" /> with the current text as the query. It is called by the inner search box
    /// when its search button is clicked, and by <c>OnKeyUp</c> for the Enter key. The request is dropped while
    /// <see cref="IsOperating" /> is <see langword="true" />.
    /// </summary>
    internal void RaiseSearchRequested(SearchTriggerSource trigger)
    {
        if (IsOperating)
        {
            return;
        }

        RaiseEvent(new SearchRequestedEventArgs(
            SearchRequestedEvent,
            this,
            Text ?? string.Empty,
            trigger));
    }

    /// <summary>
    /// Calls the base implementation, then raises <see cref="SearchRequested" /> for the Enter key when
    /// <see cref="IsSearchOnEnterEnabled" /> is <see langword="true" />, the key release is not already handled and
    /// <c>ShouldSuppressSearchForEnter</c> is <see langword="false" />, that is no input method composition is in
    /// progress and the <c>EnterPressed</c> event was not handled. The key release is then marked as handled, also
    /// when <see cref="IsOperating" /> is <see langword="true" /> and no event is raised.
    /// </summary>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (!IsSearchOnEnterEnabled)
        {
            return;
        }
        
        if (e is not { Key: Key.Enter, Handled: false } || ShouldSuppressSearchForEnter)
        {
            return;
        }

        e.Handled = true;
        RaiseSearchRequested(SearchTriggerSource.EnterKey);
    }
}
