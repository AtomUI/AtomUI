using Avalonia.Controls.Primitives;
using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;

namespace AtomUI.Desktop.Controls;

public class ListViewItem : ContentControl,
                            IListItemVirtualizingContextAware,
                            ISelectable, IStateMotionOwner
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsSelectedProperty =
        ListView.IsSelectedProperty.AddOwner<ListViewItem>();
    
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    #endregion
    
    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> ClickedEvent =
        RoutedEvent.Register<ListViewItem, RoutedEventArgs>(
            nameof(Clicked),
            RoutingStrategies.Bubble);
    
    public event EventHandler<RoutedEventArgs>? Clicked
    {
        add => AddHandler(ClickedEvent, value);
        remove => RemoveHandler(ClickedEvent, value);
    }

    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ListViewItem>();
    
    internal static readonly StyledProperty<IBrush?> ItemHoverBgProperty =
        ListBox.ItemHoverBgProperty.AddOwner<ListViewItem>();
    
    internal static readonly StyledProperty<IBrush?> ItemSelectedBgProperty =
        ListBox.ItemSelectedBgProperty.AddOwner<ListViewItem>();
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ListViewItem>();
    
    internal static readonly DirectProperty<ListViewItem, bool> IsShowSelectedIndicatorProperty =
        AvaloniaProperty.RegisterDirect<ListViewItem, bool>(nameof(IsShowSelectedIndicator),
            o => o.IsShowSelectedIndicator,
            (o, v) => o.IsShowSelectedIndicator = v);
    
    internal static readonly DirectProperty<ListViewItem, bool> IsSelectedIndicatorVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListViewItem, bool>(nameof(IsSelectedIndicatorVisible),
            o => o.IsSelectedIndicatorVisible,
            (o, v) => o.IsSelectedIndicatorVisible = v);
    
    internal static readonly DirectProperty<ListViewItem, IconTemplate?> SelectedIndicatorProperty =
        AvaloniaProperty.RegisterDirect<ListViewItem, IconTemplate?>(nameof(SelectedIndicator),
            o => o.SelectedIndicator,
            (o, v) => o.SelectedIndicator = v);
    
    internal static readonly StyledProperty<bool> IsGroupItemProperty =
        AvaloniaProperty.Register<ListViewItem, bool>(nameof(IsGroupItem));

    internal static readonly DirectProperty<ListViewItem, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<ListViewItem, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);

    internal static readonly DirectProperty<ListViewItem, bool> IsSplitLineVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListViewItem, bool>(nameof(IsSplitLineVisible),
            o => o.IsSplitLineVisible,
            (o, v) => o.IsSplitLineVisible = v);

    internal static readonly DirectProperty<ListViewItem, bool> IsSplitLineEffectiveVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListViewItem, bool>(nameof(IsSplitLineEffectiveVisible),
            o => o.IsSplitLineEffectiveVisible,
            (o, v) => o.IsSplitLineEffectiveVisible = v);

    internal static readonly StyledProperty<ClickMode> ItemClickModeProperty =
        ListView.ItemClickModeProperty.AddOwner<ListViewItem>();
    
    internal CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    internal IBrush? ItemHoverBg
    {
        get => GetValue(ItemHoverBgProperty);
        set => SetValue(ItemHoverBgProperty, value);
    }
    
    internal IBrush? ItemSelectedBg
    {
        get => GetValue(ItemSelectedBgProperty);
        set => SetValue(ItemSelectedBgProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private bool _isShowSelectedIndicator;

    internal bool IsShowSelectedIndicator
    {
        get => _isShowSelectedIndicator;
        set => SetAndRaise(IsShowSelectedIndicatorProperty, ref _isShowSelectedIndicator, value);
    }
    
    private bool _isSelectedIndicatorVisible;

    internal bool IsSelectedIndicatorVisible
    {
        get => _isSelectedIndicatorVisible;
        set => SetAndRaise(IsSelectedIndicatorVisibleProperty, ref _isSelectedIndicatorVisible, value);
    }
    
    private IconTemplate? _selectedIndicator;

    internal IconTemplate? SelectedIndicator
    {
        get => _selectedIndicator;
        set => SetAndRaise(SelectedIndicatorProperty, ref _selectedIndicator, value);
    }
    
    internal bool IsGroupItem
    {
        get => GetValue(IsGroupItemProperty);
        set => SetValue(IsGroupItemProperty, value);
    }
    
    private Thickness _effectiveBorderThickness;

    internal Thickness EffectiveBorderThickness
    {
        get => _effectiveBorderThickness;
        private set => SetAndRaise(EffectiveBorderThicknessProperty, ref _effectiveBorderThickness, value);
    }

    private bool _isSplitLineVisible;

    internal bool IsSplitLineVisible
    {
        get => _isSplitLineVisible;
        set => SetAndRaise(IsSplitLineVisibleProperty, ref _isSplitLineVisible, value);
    }

    private bool _isSplitLineEffectiveVisible;

    internal bool IsSplitLineEffectiveVisible
    {
        get => _isSplitLineEffectiveVisible;
        private set => SetAndRaise(IsSplitLineEffectiveVisibleProperty, ref _isSplitLineEffectiveVisible, value);
    }

    internal ClickMode ItemClickMode
    {
        get => GetValue(ItemClickModeProperty);
        set => SetValue(ItemClickModeProperty, value);
    }

    /// <summary>
    /// Identity of the source occurrence currently projected into this recycled container.
    /// </summary>
    internal long? EntryId { get; set; }

    #endregion
    
    private static readonly Point s_invalidPoint = new(double.NaN, double.NaN);
    private Point _pointerDownPoint = s_invalidPoint;
    
    private readonly StateMotionSession _stateMotion;

    static ListViewItem()
    {
        // ListViewItem may be initialized first by a deferred theme. Register only after
        // both types' static fields are ready to avoid a ListView/ListViewItem cctor cycle.
        ListView.RegisterItemClickedEvent(ClickedEvent);
        SelectableMixin.Attach<ListViewItem>(IsSelectedProperty);
        PressedMixin.Attach<ListViewItem>();
        FocusableProperty.OverrideDefaultValue<ListViewItem>(true);
        AutomationProperties.IsOffscreenBehaviorProperty.OverrideDefaultValue<ListViewItem>(IsOffscreenBehavior.FromClip);
    }

    public ListViewItem()
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsSelectedProperty ||
            change.Property == IsShowSelectedIndicatorProperty)
        {
            ConfigureSelectedIndicator();
        }

        if (change.Property == BorderThicknessProperty ||
            change.Property == IsGroupItemProperty ||
            change.Property == IsSplitLineVisibleProperty)
        {
            ConfigureEffectiveBorderThickness();
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ConfigureSelectedIndicator();
        ConfigureEffectiveBorderThickness();

    }




    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        _pointerDownPoint = s_invalidPoint;

        if (e.Handled)
        {
            return;
        }

        if (!e.Handled && ItemsControl.ItemsControlFromItemContainer(this) is ListView owner)
        {
            var p = e.GetCurrentPoint(this);

            if (p.Properties.PointerUpdateKind is PointerUpdateKind.LeftButtonPressed or 
                PointerUpdateKind.RightButtonPressed)
            {
                if (p.Pointer.Type == PointerType.Mouse
                    || (p.Pointer.Type == PointerType.Pen && p.Properties.IsRightButtonPressed))
                {
                    // If the pressed point comes from a mouse or right-click pen, perform the selection immediately.
                    // In case of pen, only right-click is accepted, as left click (a tip touch) is used for scrolling. 
                    e.Handled = owner.UpdateSelectionFromPointerEvent(this, e);
                }
                else
                {
                    // Otherwise perform the selection when the pointer is released as to not
                    // interfere with gestures.
                    _pointerDownPoint = p.Position;

                    // Ideally we'd set handled here, but that would prevent the scroll gesture
                    // recognizer from working.
                    ////e.Handled = true;
                }
            }

            if (p.Properties.PointerUpdateKind is PointerUpdateKind.LeftButtonPressed)
            {
                if (ItemClickMode == ClickMode.Press)
                {
                    RaiseEvent(new RoutedEventArgs(ClickedEvent));
                }
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!e.Handled && 
            !double.IsNaN(_pointerDownPoint.X) &&
            e.InitialPressMouseButton is MouseButton.Left or MouseButton.Right)
        {
            var point    = e.GetCurrentPoint(this);
            var settings = AvaloniaLocator.Current.GetService<IPlatformSettings>();
            var tapSize  = settings?.GetTapSize(point.Pointer.Type) ?? new Size(4, 4);
            var tapRect = new Rect(_pointerDownPoint, new Size())
                .Inflate(new Thickness(tapSize.Width, tapSize.Height));

            if (new Rect(Bounds.Size).ContainsExclusive(point.Position) &&
                tapRect.ContainsExclusive(point.Position) &&
                ItemsControl.ItemsControlFromItemContainer(this) is ListView owner)
            {
                if (owner.UpdateSelectionFromPointerEvent(this, e))
                {
                    // As we only update selection from touch/pen on pointer release, we need to raise
                    // the pointer event on the owner to trigger a commit.
                    if (e.Pointer.Type != PointerType.Mouse)
                    {
                        var sourceBackup = e.Source;
                        owner.RaiseEvent(e);
                        e.Source = sourceBackup;
                    }

                    e.Handled = true;
                }
            }

            
        }
        if (!e.Handled && e.InitialPressMouseButton == MouseButton.Left)
        {
            if (ItemClickMode == ClickMode.Release)
            {
                RaiseEvent(new RoutedEventArgs(ClickedEvent));
            }
        }

        _pointerDownPoint = s_invalidPoint;
    }
    #region 实现 IListItemVirtualizingContextAware 接口

    int IListItemVirtualizingContextAware.VirtualIndex { get; set; } = -1;
    bool IListItemVirtualizingContextAware.VirtualContextOperating { get; set; }

    #endregion

    #region 实现 IStateMotionOwner 接口

    StateMotionSession IStateMotionOwner.StateMotionSession => _stateMotion;

    #endregion

    private void ConfigureSelectedIndicator()
    {
        SetCurrentValue(IsSelectedIndicatorVisibleProperty, IsShowSelectedIndicator && IsSelected);
    }

    private void ConfigureEffectiveBorderThickness()
    {
        var showSplitLine = IsSplitLineVisible && !IsGroupItem && BorderThickness.Bottom > 0;
        IsSplitLineEffectiveVisible = showSplitLine;
        EffectiveBorderThickness = showSplitLine ? BorderThickness : new Thickness(0);
    }


}
