using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public enum SearchEditButtonStyle
{
    Default,
    Primary
}

public class SearchEdit : LineEdit
{
    #region 公共属性定义

    public static readonly StyledProperty<SearchEditButtonStyle> SearchButtonStyleProperty =
        AvaloniaProperty.Register<SearchEdit, SearchEditButtonStyle>(nameof(SearchButtonStyle));

    public static readonly StyledProperty<string> SearchButtonTextProperty =
        AvaloniaProperty.Register<SearchEdit, string>(nameof(SearchButtonText));

    public static readonly StyledProperty<bool> IsOperatingProperty =
        AvaloniaProperty.Register<SearchEdit, bool>(nameof(IsOperating));

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

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> SearchButtonClickEvent =
        RoutedEvent.Register<SearchEdit, RoutedEventArgs>(nameof(SearchButtonClick), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? SearchButtonClick
    {
        add => AddHandler(SearchButtonClickEvent, value);
        remove => RemoveHandler(SearchButtonClickEvent, value);
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

    internal void NotifySearchButtonClicked()
    {
        if (IsOperating)
        {
            return;
        }
        var eventArgs = new RoutedEventArgs(SearchButtonClickEvent, this);
        RaiseEvent(eventArgs);
    }
}