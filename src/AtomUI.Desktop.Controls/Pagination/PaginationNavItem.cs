using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal enum PaginationItemType
{
    Previous,
    PageIndicator,
    Next,
    JumpPrevious,
    JumpNext
}

internal class PaginationNavItem : ContentControl, ISelectable
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<PaginationNavItem>();
    
    public static readonly StyledProperty<PaginationItemType> PaginationItemTypeProperty =
        AvaloniaProperty.Register<PaginationNavItem, PaginationItemType>(nameof(PaginationItemType));
    
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<PaginationNavItem>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<PaginationNavItem>();
    
    public static readonly DirectProperty<PaginationNavItem, bool> IsPressedProperty =
        AvaloniaProperty.RegisterDirect<PaginationNavItem, bool>(nameof(IsPressed), b => b.IsPressed);
    
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<PaginationNavItem, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<PathIcon?> JumpIconProperty =
        AvaloniaProperty.Register<PaginationNavItem, PathIcon?>(nameof(JumpIcon));

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }
    
    public PaginationItemType PaginationItemType
    {
        get => GetValue(PaginationItemTypeProperty);
        set => SetValue(PaginationItemTypeProperty, value);
    }
    
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private bool _isPressed = false;

    public bool IsPressed
    {
        get => _isPressed;
        private set => SetAndRaise(IsPressedProperty, ref _isPressed, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public PathIcon? JumpIcon
    {
        get => GetValue(JumpIconProperty);
        set => SetValue(JumpIconProperty, value);
    }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<PaginationNavItem, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    #endregion

    #region 内部属性定义

    internal int PageNumber { get; set; } = -1;

    #endregion
    
    private readonly StateMotionSession _stateMotion;

    static PaginationNavItem()
    {
        SelectableMixin.Attach<PaginationNavItem>(IsSelectedProperty);
        PressedMixin.Attach<PaginationNavItem>();
        FocusableProperty.OverrideDefaultValue<PaginationNavItem>(true);
        AffectsMeasure<PaginationNavItem>(BorderThicknessProperty);
        AffectsRender<PaginationNavItem>(BackgroundProperty, BorderBrushProperty);
    }

    protected virtual void OnClick()
    {
        var e = new RoutedEventArgs(ClickEvent);
        RaiseEvent(e);
    }

    public PaginationNavItem()
    {
        _stateMotion = new StateMotionSession(this);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _stateMotion.CompleteInitialization();
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _stateMotion?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _stateMotion.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _stateMotion.CompleteInitialization();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        SyncSemanticItemMarker();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (change.Property == PaginationItemTypeProperty)
        {
            SyncSemanticItemMarker();
        }

        if (change.Property == IsPressedProperty)
        {
            UpdatePseudoClasses();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            IsPressed = true;
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (IsPressed && e.InitialPressMouseButton == MouseButton.Left)
        {
            IsPressed = false;
            e.Handled = true;
            if (this.ContainsSelfOrDescendantAt(e.GetPosition(this)))
            {
                OnClick();
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (IsEnabled && e.Key == Key.Enter)
        {
            OnClick();
            e.Handled = true;
        }
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);

        IsPressed = false;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        IsPressed = false;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Pressed, IsPressed);
    }

    private void SyncSemanticItemMarker()
    {
        if (PaginationItemType is PaginationItemType.JumpPrevious or PaginationItemType.JumpNext)
        {
            Classes.Remove(PaginationSemanticParts.ItemClass);
        }
        else
        {
            Classes.Add(PaginationSemanticParts.ItemClass);
        }
    }
}
