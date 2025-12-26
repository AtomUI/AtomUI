using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class SelectHandle : TemplatedControl
{
    public static readonly StyledProperty<bool> IsInputHoverProperty =
        AvaloniaProperty.Register<SelectHandle, bool>(nameof(IsInputHover));
    
    public static readonly StyledProperty<bool> IsInputPressedProperty =
        AvaloniaProperty.Register<SelectHandle, bool>(nameof(IsInputPressed));
    
    public static readonly StyledProperty<bool> IsSearchEnabledProperty =
        AvaloniaProperty.Register<SelectHandle, bool>(nameof(IsSearchEnabled));
        
    public static readonly StyledProperty<bool> IsClearableProperty =
        AvaloniaProperty.Register<SelectHandle, bool>(nameof(IsClearable));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<SelectHandle>();
    
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<SelectHandle, bool>(nameof(IsLoading));
    
    public static readonly StyledProperty<bool> IsSelectionEmptyProperty =
        AvaloniaProperty.Register<SelectHandle, bool>(nameof(IsSelectionEmpty), true);
    
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<SelectHandle, bool>(nameof(IsDropDownOpen), false);
    
    public bool IsInputHover
    {
        get => GetValue(IsInputHoverProperty);
        set => SetValue(IsInputHoverProperty, value);
    }
    
    public bool IsInputPressed
    {
        get => GetValue(IsInputPressedProperty);
        set => SetValue(IsInputPressedProperty, value);
    }
    
    public bool IsSearchEnabled
    {
        get => GetValue(IsSearchEnabledProperty);
        set => SetValue(IsSearchEnabledProperty, value);
    }
    
    public bool IsClearable
    {
        get => GetValue(IsClearableProperty);
        set => SetValue(IsClearableProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool IsSelectionEmpty
    {
        get => GetValue(IsSelectionEmptyProperty);
        set => SetValue(IsSelectionEmptyProperty, value);
    }
    
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    
    public static readonly RoutedEvent<RoutedEventArgs> ClearRequestedEvent = 
        RoutedEvent.Register<Button, RoutedEventArgs>(nameof(ClearRequested), RoutingStrategies.Bubble);
    
    public event EventHandler<RoutedEventArgs>? ClearRequested
    {
        add => AddHandler(ClearRequestedEvent, value);
        remove => RemoveHandler(ClearRequestedEvent, value);
    }

    private IconButton? _clearButton;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_clearButton != null)
        {
            _clearButton.Click -= HandleClearButtonClicked;
        }
        _clearButton = e.NameScope.Find<IconButton>(SelectHandleThemeConstants.ClearButtonPart);
        if (_clearButton != null)
        {
            _clearButton.Click += HandleClearButtonClicked;
        }
    }

    private void HandleClearButtonClicked(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ClearRequestedEvent, this));
    }
}