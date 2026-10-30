using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

public enum SearchButtonType
{
    Default,
    Primary
}

public enum SearchTriggerSource
{
    Button,
    EnterKey
}

public sealed class SearchRequestedEventArgs : RoutedEventArgs
{
    public SearchRequestedEventArgs(
        RoutedEvent routedEvent,
        object? source,
        string query,
        SearchTriggerSource trigger)
        : base(routedEvent, source)
    {
        Query   = query;
        Trigger = trigger;
    }

    public string Query { get; }

    public SearchTriggerSource Trigger { get; }
}

public partial class SearchEdit : LineEdit
{
    #region 公共属性定义

    public static readonly StyledProperty<SearchButtonType> SearchButtonStyleProperty =
        AvaloniaProperty.Register<SearchEdit, SearchButtonType>(nameof(SearchButtonStyle));

    public static readonly StyledProperty<string> SearchButtonTextProperty =
        AvaloniaProperty.Register<SearchEdit, string>(nameof(SearchButtonText));

    public static readonly StyledProperty<bool> IsOperatingProperty =
        AvaloniaProperty.Register<SearchEdit, bool>(nameof(IsOperating));

    public static readonly StyledProperty<ControlTheme?> SearchButtonThemeProperty =
        AvaloniaProperty.Register<SearchEdit, ControlTheme?>(nameof(SearchButtonTheme));
    
    public static readonly StyledProperty<bool> IsSearchOnEnterEnabledProperty =
        AvaloniaProperty.Register<SearchEdit, bool>(nameof(IsSearchOnEnterEnabled), true);

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

    public bool IsOperating
    {
        get => GetValue(IsOperatingProperty);
        set => SetValue(IsOperatingProperty, value);
    }

    public ControlTheme? SearchButtonTheme
    {
        get => GetValue(SearchButtonThemeProperty);
        set => SetValue(SearchButtonThemeProperty, value);
    }

    public bool IsSearchOnEnterEnabled
    {
        get => GetValue(IsSearchOnEnterEnabledProperty);
        set => SetValue(IsSearchOnEnterEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<SearchRequestedEventArgs> SearchRequestedEvent =
        RoutedEvent.Register<SearchEdit, SearchRequestedEventArgs>(
            nameof(SearchRequested),
            RoutingStrategies.Bubble);

    public event EventHandler<SearchRequestedEventArgs>? SearchRequested
    {
        add => AddHandler(SearchRequestedEvent, value);
        remove => RemoveHandler(SearchRequestedEvent, value);
    }

    #endregion
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (ClearIcon == null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var decoratedBox = e.NameScope.Find<SearchEditDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
        if (decoratedBox != null)
        {
            decoratedBox.OwningSearchEdit = this;
        }
    }

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

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (!IsSearchOnEnterEnabled)
        {
            return;
        }
        
        if (e is not { Key: Key.Enter, Handled: false })
        {
            return;
        }

        e.Handled = true;
        RaiseSearchRequested(SearchTriggerSource.EnterKey);
    }
}
