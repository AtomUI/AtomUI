using System.Collections;
using System.Collections.Specialized;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaTabStrip = Avalonia.Controls.Primitives.TabStrip;

public abstract class BaseTabStrip : AvaloniaTabStrip, 
                                     ISizeTypeAware,
                                     IMotionAwareControl,
                                     ITabOverflowOwner
{
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel());

    #region 公共属性定义

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<BaseTabStrip>();

    public static readonly StyledProperty<Dock> TabStripPlacementProperty =
        AvaloniaProperty.Register<BaseTabStrip, Dock>(nameof(TabStripPlacement), Dock.Top);

    public static readonly StyledProperty<bool> TabAlignmentCenterProperty =
        AvaloniaProperty.Register<BaseTabStrip, bool>(nameof(TabAlignmentCenter));

    public static readonly StyledProperty<bool> IsTabReorderEnabledProperty =
        AvaloniaProperty.Register<BaseTabStrip, bool>(nameof(IsTabReorderEnabled));

    public static readonly StyledProperty<TabActivationTrigger> TabActivationTriggerProperty =
        AvaloniaProperty.Register<BaseTabStrip, TabActivationTrigger>(
            nameof(TabActivationTrigger),
            TabActivationTrigger.PointerReleased);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<BaseTabStrip>();
    
    public static readonly StyledProperty<double> HeaderStartEdgePaddingProperty = 
        AvaloniaProperty.Register<BaseTabStrip, double>(nameof (HeaderStartEdgePadding));
    
    public static readonly StyledProperty<double> HeaderEndEdgePaddingProperty = 
        AvaloniaProperty.Register<BaseTabStrip, double>(nameof (HeaderEndEdgePadding));
    
    public static readonly StyledProperty<object?> HeaderStartExtraContentProperty = 
        AvaloniaProperty.Register<BaseTabStrip, object?>(nameof (HeaderStartExtraContent));
    
    public static readonly StyledProperty<IDataTemplate?> HeaderStartExtraContentTemplateProperty =
        AvaloniaProperty.Register<BaseTabStrip, IDataTemplate?>(nameof(HeaderStartExtraContentTemplate));
    
    public static readonly StyledProperty<object?> HeaderEndExtraContentProperty = 
        AvaloniaProperty.Register<BaseTabStrip, object?>(nameof (HeaderEndExtraContent));
    
    public static readonly StyledProperty<IDataTemplate?> HeaderEndExtraContentTemplateProperty =
        AvaloniaProperty.Register<BaseTabStrip, IDataTemplate?>(nameof(HeaderEndExtraContentTemplate));

    public static readonly StyledProperty<IDataTemplate?> OverflowPopupTemplateProperty =
        BaseTabControl.OverflowPopupTemplateProperty.AddOwner<BaseTabStrip>();
    
    public static readonly StyledProperty<bool> IsTabClosableProperty =
        AvaloniaProperty.Register<BaseTabStrip, bool>(nameof(IsTabClosable));
    
    public static readonly StyledProperty<bool> IsTabAutoHideCloseButtonProperty =
        AvaloniaProperty.Register<BaseTabStrip, bool>(nameof(IsTabAutoHideCloseButton));
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public Dock TabStripPlacement
    {
        get => GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
    }

    public bool TabAlignmentCenter
    {
        get => GetValue(TabAlignmentCenterProperty);
        set => SetValue(TabAlignmentCenterProperty, value);
    }

    public bool IsTabReorderEnabled
    {
        get => GetValue(IsTabReorderEnabledProperty);
        set => SetValue(IsTabReorderEnabledProperty, value);
    }

    public TabActivationTrigger TabActivationTrigger
    {
        get => GetValue(TabActivationTriggerProperty);
        set => SetValue(TabActivationTriggerProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public double HeaderStartEdgePadding
    {
        get => GetValue(HeaderStartEdgePaddingProperty);
        set => SetValue(HeaderStartEdgePaddingProperty, value);
    }
    
    public double HeaderEndEdgePadding
    {
        get => GetValue(HeaderEndEdgePaddingProperty);
        set => SetValue(HeaderEndEdgePaddingProperty, value);
    }
    
    [DependsOn(nameof(HeaderStartExtraContentTemplate))]
    public object? HeaderStartExtraContent
    {
        get => GetValue(HeaderStartExtraContentProperty);
        set => SetValue(HeaderStartExtraContentProperty, value);
    }
    
    public IDataTemplate? HeaderStartExtraContentTemplate
    {
        get => GetValue(HeaderStartExtraContentTemplateProperty);
        set => SetValue(HeaderStartExtraContentTemplateProperty, value);
    }
    
    [DependsOn(nameof(HeaderEndExtraContentTemplate))]
    public object? HeaderEndExtraContent
    {
        get => GetValue(HeaderEndExtraContentProperty);
        set => SetValue(HeaderEndExtraContentProperty, value);
    }
    
    public IDataTemplate? HeaderEndExtraContentTemplate
    {
        get => GetValue(HeaderEndExtraContentTemplateProperty);
        set => SetValue(HeaderEndExtraContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to present tabs that are not fully visible in the tab viewport.
    /// The template data item is a <see cref="TabOverflowPopupContext"/>.
    /// </summary>
    public IDataTemplate? OverflowPopupTemplate
    {
        get => GetValue(OverflowPopupTemplateProperty);
        set => SetValue(OverflowPopupTemplateProperty, value);
    }
    
    public bool IsTabClosable
    {
        get => GetValue(IsTabClosableProperty);
        set => SetValue(IsTabClosableProperty, value);
    }
    
    public bool IsTabAutoHideCloseButton
    {
        get => GetValue(IsTabAutoHideCloseButtonProperty);
        set => SetValue(IsTabAutoHideCloseButtonProperty, value);
    }

    #endregion
    
    #region 公共事件定义
    
    public static readonly RoutedEvent<TabStripClosingEventArgs> ClosingEvent =
        RoutedEvent.Register<BaseTabStrip, TabStripClosingEventArgs>(nameof(Closing), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<TabStripClosedEventArgs> ClosedEvent =
        RoutedEvent.Register<BaseTabStrip, TabStripClosedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    internal static readonly RoutedEvent<TabReorderingEventArgs> TabReorderingEvent =
        RoutedEvent.Register<BaseTabStrip, TabReorderingEventArgs>(nameof(TabReordering), RoutingStrategies.Bubble);

    internal static readonly RoutedEvent<TabReorderedEventArgs> TabReorderedEvent =
        RoutedEvent.Register<BaseTabStrip, TabReorderedEventArgs>(nameof(TabReordered), RoutingStrategies.Bubble);

    public event EventHandler<TabStripClosingEventArgs>? Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    public event EventHandler<TabStripClosedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public event EventHandler<TabReorderingEventArgs>? TabReordering
    {
        add => AddHandler(TabReorderingEvent, value);
        remove => RemoveHandler(TabReorderingEvent, value);
    }

    public event EventHandler<TabReorderedEventArgs>? TabReordered
    {
        add => AddHandler(TabReorderedEvent, value);
        remove => RemoveHandler(TabReorderedEvent, value);
    }
    
    #endregion
    
    #region 内部属性定义
    internal static readonly DirectProperty<BaseTabStrip, Thickness> EffectiveHeaderPaddingProperty =
        AvaloniaProperty.RegisterDirect<BaseTabStrip, Thickness>(nameof(EffectiveHeaderPadding),
            o => o.EffectiveHeaderPadding,
            (o, v) => o.EffectiveHeaderPadding = v);

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Flyout.IsPopupPinnedOpenProperty.AddOwner<BaseTabStrip>();
        
    private Thickness _effectiveHeaderPadding;

    internal Thickness EffectiveHeaderPadding
    {
        get => _effectiveHeaderPadding;
        set => SetAndRaise(EffectiveHeaderPaddingProperty, ref _effectiveHeaderPadding, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    #endregion

    private Pen? _tabStripBorderPen;
    private IBrush? _tabStripBorderPenBrush;
    private double _tabStripBorderPenThickness;
    private TabScrollViewer? _tabReorderScrollViewer;
    private EventHandler? _overflowSelectionChanged;
    private DispatcherTimer? _tabReorderAutoScrollTimer;
    private TabStripItem? _pendingTabActivationContainer;
    private IPointer? _pendingTabActivationPointer;
    private TabStripItem? _tabReorderContainer;
    private IPointer? _tabReorderPointer;
    private object? _tabReorderItem;
    private object? _tabReorderSelectedItem;
    private int _tabReorderOldIndex = TabReorderHelper.InvalidIndex;
    private int _tabReorderTargetIndex = TabReorderHelper.InvalidIndex;
    private Point _tabReorderStartPointerRootPosition;
    private double _tabReorderPointerAnchorPrimary;
    private bool _isTabReorderDragging;
    private TransitionSuppressionScope? _tabReorderIndicatorSuppression;
    private bool _isTabReorderIndicatorRefreshDeferred;
    private bool _isTabReorderIndicatorRefreshPending;
    private readonly Dictionary<Control, ITransform?> _tabReorderOriginalTransforms = new();
    private readonly Dictionary<Control, int> _tabReorderOriginalZIndexes = new();
    private readonly Dictionary<Control, Vector> _tabReorderPreviewOffsets = new();
    private int _tabReorderAutoScrollDirection;
    private Point _tabReorderLastPointerRootPosition;
    private const double TabReorderAutoScrollEdgeThickness = 24;
    private const double TabReorderAutoScrollStep = 16;
    private static readonly TimeSpan TabReorderAutoScrollInterval = TimeSpan.FromMilliseconds(16);

    static BaseTabStrip()
    {
        ItemsPanelProperty.OverrideDefaultValue<BaseTabStrip>(DefaultPanel);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<BaseTabStrip>(false);
        AffectsRender<BaseTabStrip>(TabStripPlacementProperty, BorderBrushProperty, BorderThicknessProperty, UseLayoutRoundingProperty);
        AffectsMeasure<BaseTabStrip>(TabStripPlacementProperty);
    }

    public BaseTabStrip()
    {
        AddHandler(PointerMovedEvent, HandleTabReorderPointerMoved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, HandleTabReorderPointerReleased, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(PointerCaptureLostEvent, HandleTabReorderPointerCaptureLost, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
    }
    
    public bool CloseTab(TabStripItem tabStripItem)
    {
        if (!tabStripItem.IsClosable)
        {
            return false;
        }

        if (!TabReorderHelper.TryResolveItemsList(this, out var list) ||
            !TabReorderHelper.CanMoveItems(list))
        {
            return false;
        }

        var index = TabReorderHelper.FindContainerIndex(this, list, tabStripItem);
        if (!TabReorderHelper.IsValidIndex(index, list.Count))
        {
            return false;
        }

        var item = list[index];
        var closingArgs = new TabStripClosingEventArgs(ClosingEvent, tabStripItem);
        RaiseEvent(closingArgs);

        if (closingArgs.Cancel || !tabStripItem.IsClosable ||
            !TabReorderHelper.TryGetCurrentItemIndex(this, list, tabStripItem, item, out index))
        { 
            return false;
        }

        var nextSelectionIndex = SelectedIndex;
        if (nextSelectionIndex == index)
        {
            if (index > 0)
            {
                nextSelectionIndex = index - 1;
            }
            else if (list.Count > 1)
            {
                nextSelectionIndex = 1;
            }
            else
            {
                nextSelectionIndex = -1;
            }
        }
        var nextSelectedItem = TabReorderHelper.IsValidIndex(nextSelectionIndex, list.Count)
            ? list[nextSelectionIndex]
            : null;
        SelectedIndex = nextSelectionIndex;
        
        if (!tabStripItem.IsClosable ||
            !TabReorderHelper.TryGetCurrentItemIndex(this, list, tabStripItem, item, out index))
        {
            return false;
        }
        var itemAfterCallback = SelectedItem;
        // A collection change during SelectionChanged can recommit the original numeric index.
        var selectionDrifted = SelectedIndex == nextSelectionIndex &&
                               !TabReorderHelper.IsSameItem(itemAfterCallback, nextSelectedItem);
        list.RemoveAt(index);
        if (selectionDrifted)
        {
            TabReorderHelper.RestoreSelectionAfterClose(this, list, nextSelectedItem, itemAfterCallback);
        }
        
        var closedArgs = new TabStripClosedEventArgs(ClosedEvent, tabStripItem);
        RaiseEvent(closedArgs);
        
        return true;
    }

    internal bool NotifyTabReorderPointerPressed(TabStripItem tabStripItem, PointerPressedEventArgs args)
    {
        if (!CanStartTabReorder(tabStripItem, args))
        {
            return false;
        }

        if (!TabReorderHelper.TryResolveItemsList(this, out var list))
        {
            return false;
        }

        var oldIndex = IndexFromContainer(tabStripItem);
        if (!TabReorderHelper.IsValidIndex(oldIndex, list.Count))
        {
            return false;
        }

        CancelTabReorder();

        var pointerRootPosition = TabReorderHelper.GetPointerRootPosition(this, args);
        if (!TabReorderHelper.TryGetPointerAnchorPrimary(
                this,
                TabStripPlacement,
                tabStripItem,
                pointerRootPosition,
                out var pointerAnchorPrimary))
        {
            return false;
        }

        _tabReorderContainer   = tabStripItem;
        _tabReorderPointer     = args.Pointer;
        _tabReorderItem        = list[oldIndex];
        _tabReorderSelectedItem = SelectedItem;
        _tabReorderOldIndex    = oldIndex;
        _tabReorderTargetIndex = oldIndex;
        _tabReorderStartPointerRootPosition = pointerRootPosition;
        _tabReorderLastPointerRootPosition  = pointerRootPosition;
        _tabReorderPointerAnchorPrimary     = pointerAnchorPrimary;

        return true;
    }

    internal void NotifyTabActivationPointerPressed(TabStripItem tabStripItem, PointerPressedEventArgs args)
    {
        if (TabActivationTrigger != TabActivationTrigger.PointerReleased ||
            args.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed ||
            !IsEnabled ||
            !tabStripItem.IsEnabled ||
            IsPointerFromEmbeddedButton(tabStripItem, args.Source as Visual))
        {
            ClearPendingTabActivation();
            return;
        }

        _pendingTabActivationContainer = tabStripItem;
        _pendingTabActivationPointer   = args.Pointer;
    }

    internal void NotifyTabActivationPointerReleased(TabStripItem tabStripItem, PointerReleasedEventArgs args)
    {
        if (_pendingTabActivationPointer == args.Pointer)
        {
            ClearPendingTabActivation();
        }
    }

    internal void NotifyTabReorderPointerPressCompleted(TabStripItem tabStripItem, PointerPressedEventArgs args)
    {
        if (_tabReorderContainer != tabStripItem || _tabReorderPointer != args.Pointer)
        {
            return;
        }

        args.Pointer.Capture(tabStripItem);
    }

    internal bool NotifyTabReorderPointerMoved(TabStripItem tabStripItem, PointerEventArgs args)
    {
        if (_tabReorderContainer != tabStripItem || _tabReorderPointer != args.Pointer)
        {
            return false;
        }

        return UpdateActiveTabReorder(args);
    }

    internal bool NotifyTabReorderPointerReleased(TabStripItem tabStripItem, PointerReleasedEventArgs args)
    {
        if (_tabReorderContainer != tabStripItem || _tabReorderPointer != args.Pointer)
        {
            return false;
        }

        ClearPendingTabActivation();
        return CompleteActiveTabReorder(args);
    }

    internal void NotifyTabReorderPointerCaptureLost(TabStripItem tabStripItem)
    {
        if (_tabReorderContainer == tabStripItem)
        {
            ClearTabReorder(releasePointer: false);
        }
    }

    internal void NotifyTabStripItemIconStateChanged()
    {
        UpdateIconSlotReservation();
    }
    

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TabStripItem tabStripItem)
        {
            tabStripItem.TabStripPlacement = TabStripPlacement;

            if (item != null && item is not Visual)
            {
                if (!tabStripItem.IsSet(TabStripItem.ContentProperty))
                {
                    tabStripItem.SetCurrentValue(TabStripItem.ContentProperty, item);
                }

                if (item is ITabItemData tabItemData)
                {
                    if (!tabStripItem.IsSet(TabStripItem.IconProperty))
                    {
                        tabStripItem.SetCurrentValue(TabStripItem.IconProperty, tabItemData.Icon);
                    }
                    if (!tabStripItem.IsSet(TabStripItem.CloseIconProperty))
                    {
                        tabStripItem.SetCurrentValue(TabStripItem.CloseIconProperty, tabItemData.CloseIcon);
                    }
                    if (!tabStripItem.IsSet(TabStripItem.ContentProperty))
                    {
                        tabStripItem.SetCurrentValue(TabStripItem.ContentProperty, tabItemData.Header);
                    }
                    if (!tabStripItem.IsSet(TabStripItem.IsEnabledProperty))
                    {
                        tabStripItem.SetCurrentValue(TabStripItem.IsEnabledProperty, tabItemData.IsEnabled);
                    }
                    if (!tabStripItem.IsSet(TabStripItem.IsClosableProperty))
                    {
                        tabStripItem.SetCurrentValue(TabStripItem.IsClosableProperty, tabItemData.IsClosable);
                    }
                    if (!tabStripItem.IsSet(TabStripItem.IsAutoHideCloseButtonProperty))
                    {
                        tabStripItem.SetCurrentValue(TabStripItem.IsAutoHideCloseButtonProperty, tabItemData.IsAutoHideCloseButton);
                    }
                }
            }

            if (ItemTemplate != null)
            {
                tabStripItem[!TabItem.ContentTemplateProperty] = this[!ItemTemplateProperty];
            }

            tabStripItem[!TabStripItem.SizeTypeProperty] = this[!SizeTypeProperty];
            tabStripItem[!TabStripItem.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];

            PrepareTabStripItem(tabStripItem, item, index);
            ConfigureTabStripItem(tabStripItem);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type TabStripItem.");
        }
    }

    protected override void ClearContainerForItemOverride(Control element)
    {
        base.ClearContainerForItemOverride(element);
        if (element is TabStripItem tabStripItem)
        {
            tabStripItem.IsIconSlotReserved = false;
            ReleaseTabStripItemOwnerBindings(tabStripItem);
        }
        UpdateIconSlotReservation();
    }

    // The indexer bindings created in PrepareContainerForItemOverride subscribe to this
    // owner's PropertyChanged, and Avalonia never disposes them on its own: without this
    // release every removed item stays alive for the owner's lifetime. Avalonia skips
    // ClearContainerForItemOverride for items that are their own container, so TabStripItem
    // also routes its direct logical detach into this method.
    internal virtual void ReleaseTabStripItemOwnerBindings(TabStripItem tabStripItem)
    {
        BindingOperations.GetBindingExpressionBase(tabStripItem, TabStripItem.SizeTypeProperty)?.Dispose();
        BindingOperations.GetBindingExpressionBase(tabStripItem, TabStripItem.IsMotionEnabledProperty)?.Dispose();
        BindingOperations.GetBindingExpressionBase(tabStripItem, TabItem.ContentTemplateProperty)?.Dispose();
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is TabStripItem)
        {
            UpdateIconSlotReservation();
        }
    }

    protected virtual void PrepareTabStripItem(TabStripItem tabStripItem, object? item, int index)
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedIndexProperty)
        {
            _overflowSelectionChanged?.Invoke(this, EventArgs.Empty);
        }
        if (change.Property == TabStripPlacementProperty)
        {
            UpdatePseudoClasses();
            for (var i = 0; i < ItemCount; ++i)
            {
                var itemContainer = ContainerFromIndex(i);
                if (itemContainer is TabStripItem tabStripItem)
                {
                    tabStripItem.TabStripPlacement = TabStripPlacement;
                }
            }

            ConfigureEffectiveHeaderPadding();
            UpdateIconSlotReservation();
        }
        else if (change.Property == HeaderStartEdgePaddingProperty || change.Property == HeaderEndEdgePaddingProperty)
        {
            ConfigureEffectiveHeaderPadding();
        }
        if (change.Property == IsTabAutoHideCloseButtonProperty ||
            change.Property == IsTabClosableProperty)
        {
            if (Items.Count > 0)
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    if ((ContainerFromIndex(i) ?? Items[i]) is TabStripItem tabStripItem)
                    {
                        ConfigureTabStripItem(tabStripItem);
                    }
                }
            }
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(TabPseudoClass.Top, TabStripPlacement == Dock.Top);
        PseudoClasses.Set(TabPseudoClass.Right, TabStripPlacement == Dock.Right);
        PseudoClasses.Set(TabPseudoClass.Bottom, TabStripPlacement == Dock.Bottom);
        PseudoClasses.Set(TabPseudoClass.Left, TabStripPlacement == Dock.Left);
    }

    public override void Render(DrawingContext context)
    {
        if (Items.Count > 0)
        {
            Point startPoint      = default;
            Point endPoint        = default;
            var   borderThickness = BorderUtils.BuildRenderScaleAwareThickness(this, BorderThickness.Left);
            var   offsetDelta     = borderThickness / 2;
            if (TabStripPlacement == Dock.Top)
            {
                startPoint = new Point(0, Bounds.Height - offsetDelta);
                endPoint   = new Point(Bounds.Width, Bounds.Height - offsetDelta);
            }
            else if (TabStripPlacement == Dock.Right)
            {
                startPoint = new Point(offsetDelta, 0);
                endPoint   = new Point(offsetDelta, Bounds.Height);
            }
            else if (TabStripPlacement == Dock.Bottom)
            {
                startPoint = new Point(0, offsetDelta);
                endPoint   = new Point(Bounds.Width, offsetDelta);
            }
            else
            {
                startPoint = new Point(Bounds.Width - offsetDelta, 0);
                endPoint   = new Point(Bounds.Width - offsetDelta, Bounds.Height);
            }

            using var optionState = context.PushRenderOptions(new RenderOptions
            {
                EdgeMode = EdgeMode.Aliased
            });
            context.DrawLine(GetTabStripBorderPen(borderThickness), startPoint, endPoint);
        }
    }

    private Pen GetTabStripBorderPen(double borderThickness)
    {
        if (_tabStripBorderPen is null ||
            !ReferenceEquals(_tabStripBorderPenBrush, BorderBrush) ||
            !double.Equals(_tabStripBorderPenThickness, borderThickness))
        {
            _tabStripBorderPenBrush     = BorderBrush;
            _tabStripBorderPenThickness = borderThickness;
            _tabStripBorderPen          = new Pen(BorderBrush, borderThickness);
        }

        return _tabStripBorderPen!;
    }
    
    private void ConfigureEffectiveHeaderPadding()
    {
        if (TabStripPlacement == Dock.Top)
        {
            SetCurrentValue(EffectiveHeaderPaddingProperty, new  Thickness(HeaderStartEdgePadding, 0, HeaderEndEdgePadding, 0));
        }
        else if (TabStripPlacement == Dock.Right)
        {
            SetCurrentValue(EffectiveHeaderPaddingProperty, new Thickness(0, HeaderStartEdgePadding, 0, HeaderEndEdgePadding));
        }
        else if (TabStripPlacement == Dock.Bottom)
        {
            SetCurrentValue(EffectiveHeaderPaddingProperty, new  Thickness(HeaderStartEdgePadding, 0, HeaderEndEdgePadding, 0));
        }
        else
        {
            SetCurrentValue(EffectiveHeaderPaddingProperty, new Thickness(0, HeaderStartEdgePadding, 0, HeaderEndEdgePadding));
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ClearPendingTabActivation();
        CancelTabReorder();
        DetachTabScrollViewer();
        base.OnApplyTemplate(e);
        _tabReorderScrollViewer = TabReorderHelper.FindTabScrollViewer(e.NameScope);
        _tabReorderScrollViewer?.AttachOverflowOwner(this);
        ConfigureEffectiveHeaderPadding();
        UpdateIconSlotReservation();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _tabReorderScrollViewer?.AttachOverflowOwner(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _tabReorderScrollViewer?.DetachOverflowOwner(this);
        ClearPendingTabActivation();
        CancelTabReorder();
        base.OnDetachedFromVisualTree(e);
    }

    private void DetachTabScrollViewer()
    {
        if (_tabReorderScrollViewer is { } scrollViewer)
        {
            scrollViewer.DetachOverflowOwner(this);
        }
        _tabReorderScrollViewer = null;
    }

    protected override bool ShouldTriggerSelection(Visual selectable, PointerEventArgs eventArgs)
    {
        if (eventArgs.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
        {
            return TabActivationTrigger == TabActivationTrigger.PointerReleased &&
                   IsEnabled &&
                   selectable is Control { IsEnabled: true } &&
                   IsPendingTabActivationRelease(selectable, eventArgs);
        }

        return eventArgs.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed &&
               TabActivationTrigger == TabActivationTrigger.PointerPressed &&
               base.ShouldTriggerSelection(selectable, eventArgs);
    }
    
    private void ConfigureTabStripItem(TabStripItem tabItem)
    {
        tabItem.SetValue(TabStripItem.IsClosableProperty, IsTabClosable, BindingPriority.Template);
        tabItem.SetValue(TabStripItem.IsAutoHideCloseButtonProperty, IsTabAutoHideCloseButton, BindingPriority.Template);
    }

    private void UpdateIconSlotReservation()
    {
        var shouldReserve = IsVerticalTabStripPlacement() && HasAnyTabStripItemIcon();

        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is TabStripItem tabStripItem)
            {
                tabStripItem.IsIconSlotReserved = shouldReserve;
            }
        }
    }

    private bool HasAnyTabStripItemIcon()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is TabStripItem { HasIcon: true })
            {
                return true;
            }

            var item = Items[i];
            if (item is TabStripItem { Icon: not null } ||
                item is ITabItemData { Icon: not null })
            {
                return true;
            }
        }

        return false;
    }

    private bool IsVerticalTabStripPlacement()
    {
        return TabStripPlacement is Dock.Left or Dock.Right;
    }

    private bool CanStartTabReorder(TabStripItem tabStripItem, PointerPressedEventArgs args)
    {
        return IsTabReorderEnabled &&
               IsEnabled &&
               tabStripItem.IsEnabled &&
               ItemCount > 1 &&
               args.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed &&
               !IsPointerFromEmbeddedButton(tabStripItem, args.Source as Visual);
    }

    private static bool IsPointerFromEmbeddedButton(TabStripItem tabStripItem, Visual? source)
    {
        for (var current = source; current is not null && current != tabStripItem; current = current.GetVisualParent())
        {
            if (current is Avalonia.Controls.Button)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPendingTabActivationRelease(Visual selectable, PointerEventArgs args)
    {
        if (selectable is not TabStripItem tabStripItem ||
            _pendingTabActivationContainer != tabStripItem ||
            _pendingTabActivationPointer != args.Pointer)
        {
            return false;
        }

        var position = args.GetPosition(tabStripItem);
        return new Rect(default, tabStripItem.Bounds.Size).Contains(position);
    }

    private void ClearPendingTabActivation()
    {
        _pendingTabActivationContainer = null;
        _pendingTabActivationPointer   = null;
    }

    private void UpdateTabReorderTarget(Point pointerRootPosition)
    {
        _tabReorderLastPointerRootPosition = pointerRootPosition;
        ResolveTabReorderTarget(pointerRootPosition);
        UpdateTabReorderAutoScroll(pointerRootPosition);
    }

    private void ResolveTabReorderTarget(Point pointerRootPosition)
    {
        if (_tabReorderContainer is null)
        {
            return;
        }

        _tabReorderTargetIndex = TabReorderHelper.GetInsertionIndex(
            this,
            TabStripPlacement,
            _tabReorderContainer,
            pointerRootPosition,
            _tabReorderPointerAnchorPrimary,
            _tabReorderOldIndex);
        UpdateTabReorderIndicatorTransitionSuppression();
        TabReorderHelper.ApplyLivePreview(
            this,
            TabStripPlacement,
            _tabReorderContainer,
            pointerRootPosition,
            _tabReorderPointerAnchorPrimary,
            _tabReorderOldIndex,
            _tabReorderTargetIndex,
            _tabReorderOriginalTransforms,
            _tabReorderOriginalZIndexes,
            _tabReorderPreviewOffsets);
        NotifyTabReorderPreviewChanged();
    }

    private void ClearTabReorderLivePreview()
    {
        TabReorderHelper.ClearLivePreview(
            _tabReorderOriginalTransforms,
            _tabReorderOriginalZIndexes,
            _tabReorderPreviewOffsets);
    }

    internal double GetTabReorderPreviewPrimaryOffset(Control container)
    {
        if (!_tabReorderPreviewOffsets.TryGetValue(container, out var offset))
        {
            return 0;
        }

        return TabReorderHelper.IsHorizontal(TabStripPlacement) ? offset.X : offset.Y;
    }

    internal bool IsTabReorderIndicatorRefreshDeferred => _isTabReorderIndicatorRefreshDeferred;

    private void NotifyTabReorderPreviewChanged()
    {
        if (this is TabStrip tabStrip)
        {
            tabStrip.NotifyTabReorderPreviewChanged();
        }
    }

    private void UpdateTabReorderAutoScroll(Point pointerRootPosition)
    {
        var direction = GetTabReorderAutoScrollDirection(pointerRootPosition);
        if (direction == 0)
        {
            StopTabReorderAutoScroll();
            return;
        }

        _tabReorderAutoScrollDirection = direction;
        if (ScrollTabReorderViewport())
        {
            ResolveTabReorderTarget(pointerRootPosition);
        }
        StartTabReorderAutoScroll();
    }

    private int GetTabReorderAutoScrollDirection(Point pointerRootPosition)
    {
        if (_tabReorderScrollViewer is null)
        {
            return 0;
        }

        var scrollViewerOffset = TabReorderHelper.TranslateToRoot(this, _tabReorderScrollViewer, default);
        if (scrollViewerOffset is null)
        {
            return 0;
        }

        var scrollViewerBounds = new Rect(scrollViewerOffset.Value, _tabReorderScrollViewer.Bounds.Size);
        if (!scrollViewerBounds.Contains(pointerRootPosition))
        {
            return 0;
        }

        var isHorizontal = TabReorderHelper.IsHorizontal(TabStripPlacement);
        var offset       = isHorizontal ? _tabReorderScrollViewer.Offset.X : _tabReorderScrollViewer.Offset.Y;
        var extent       = isHorizontal ? _tabReorderScrollViewer.Extent.Width : _tabReorderScrollViewer.Extent.Height;
        var viewport     = isHorizontal ? _tabReorderScrollViewer.Viewport.Width : _tabReorderScrollViewer.Viewport.Height;
        var maxOffset    = Math.Max(0, extent - viewport);
        if (maxOffset <= 0)
        {
            return 0;
        }

        if (isHorizontal)
        {
            if (pointerRootPosition.X <= scrollViewerBounds.Left + TabReorderAutoScrollEdgeThickness && offset > 0)
            {
                return -1;
            }

            if (pointerRootPosition.X >= scrollViewerBounds.Right - TabReorderAutoScrollEdgeThickness && offset < maxOffset)
            {
                return 1;
            }
        }
        else
        {
            if (pointerRootPosition.Y <= scrollViewerBounds.Top + TabReorderAutoScrollEdgeThickness && offset > 0)
            {
                return -1;
            }

            if (pointerRootPosition.Y >= scrollViewerBounds.Bottom - TabReorderAutoScrollEdgeThickness && offset < maxOffset)
            {
                return 1;
            }
        }

        return 0;
    }

    private void StartTabReorderAutoScroll()
    {
        _tabReorderAutoScrollTimer ??= new DispatcherTimer
        {
            Interval = TabReorderAutoScrollInterval
        };
        _tabReorderAutoScrollTimer.Tick -= HandleTabReorderAutoScrollTimerTick;
        _tabReorderAutoScrollTimer.Tick += HandleTabReorderAutoScrollTimerTick;
        if (!_tabReorderAutoScrollTimer.IsEnabled)
        {
            _tabReorderAutoScrollTimer.Start();
        }
    }

    private void StopTabReorderAutoScroll()
    {
        _tabReorderAutoScrollDirection = 0;
        if (_tabReorderAutoScrollTimer is null)
        {
            return;
        }

        _tabReorderAutoScrollTimer.Stop();
        _tabReorderAutoScrollTimer.Tick -= HandleTabReorderAutoScrollTimerTick;
        _tabReorderAutoScrollTimer = null;
    }

    private void HandleTabReorderAutoScrollTimerTick(object? sender, EventArgs args)
    {
        if (!_isTabReorderDragging || _tabReorderPointer is null)
        {
            StopTabReorderAutoScroll();
            return;
        }

        if (ScrollTabReorderViewport())
        {
            ResolveTabReorderTarget(_tabReorderLastPointerRootPosition);
        }
        else
        {
            StopTabReorderAutoScroll();
        }
    }

    private bool ScrollTabReorderViewport()
    {
        if (_tabReorderScrollViewer is null || _tabReorderAutoScrollDirection == 0)
        {
            return false;
        }

        var isHorizontal = TabReorderHelper.IsHorizontal(TabStripPlacement);
        var offset       = _tabReorderScrollViewer.Offset;
        var maxOffset = isHorizontal
            ? Math.Max(0, _tabReorderScrollViewer.Extent.Width - _tabReorderScrollViewer.Viewport.Width)
            : Math.Max(0, _tabReorderScrollViewer.Extent.Height - _tabReorderScrollViewer.Viewport.Height);
        var currentOffset = isHorizontal ? offset.X : offset.Y;
        var nextOffset = Math.Clamp(
            currentOffset + _tabReorderAutoScrollDirection * TabReorderAutoScrollStep,
            0,
            maxOffset);
        if (Math.Abs(nextOffset - currentOffset) < 0.5)
        {
            return false;
        }

        _tabReorderScrollViewer.Offset = isHorizontal
            ? new Vector(nextOffset, offset.Y)
            : new Vector(offset.X, nextOffset);
        return true;
    }

    private bool UpdateTabReorderDrag(Point pointerRootPosition)
    {
        if (!_isTabReorderDragging)
        {
            var delta             = pointerRootPosition - _tabReorderStartPointerRootPosition;
            var manhattanDistance = Math.Abs(delta.X) + Math.Abs(delta.Y);
            if (manhattanDistance <= Constants.DragThreshold)
            {
                return false;
            }

            _isTabReorderDragging = true;
            SetTabReorderContainerDragging(true);
            LayoutUpdated -= HandleTabReorderLayoutUpdated;
            LayoutUpdated += HandleTabReorderLayoutUpdated;
            ClearPendingTabActivation();
        }

        UpdateTabReorderTarget(pointerRootPosition);
        return true;
    }

    private void HandleTabReorderLayoutUpdated(object? sender, EventArgs args)
    {
        if (!_isTabReorderDragging || _tabReorderContainer is null)
        {
            return;
        }

        ResolveTabReorderTarget(_tabReorderLastPointerRootPosition);
    }

    private bool UpdateActiveTabReorder(PointerEventArgs args)
    {
        if (_tabReorderContainer is null || _tabReorderPointer != args.Pointer)
        {
            return false;
        }

        return UpdateTabReorderDrag(TabReorderHelper.GetPointerRootPosition(this, args));
    }

    private void HandleTabReorderPointerMoved(object? sender, PointerEventArgs args)
    {
        if (UpdateActiveTabReorder(args))
        {
            args.Handled = true;
        }
    }

    private void HandleTabReorderPointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        if (CompleteActiveTabReorder(args))
        {
            args.Handled = true;
        }
    }

    private void HandleTabReorderPointerCaptureLost(object? sender, PointerCaptureLostEventArgs args)
    {
        if (_tabReorderPointer == args.Pointer &&
            (ReferenceEquals(args.Source, this) || ReferenceEquals(args.Source, _tabReorderContainer)))
        {
            ClearTabReorder(releasePointer: false);
        }

        if (_pendingTabActivationPointer == args.Pointer)
        {
            ClearPendingTabActivation();
        }
    }

    private bool CompleteActiveTabReorder(PointerReleasedEventArgs args)
    {
        if (_tabReorderContainer is null || _tabReorderPointer != args.Pointer)
        {
            return false;
        }

        UpdateTabReorderDrag(TabReorderHelper.GetPointerRootPosition(this, args));

        var shouldHandle = _isTabReorderDragging;
        var reorderCommitted = false;
        try
        {
            if (_isTabReorderDragging)
            {
                BeginTabReorderIndicatorRefreshDeferral();
                reorderCommitted = CommitTabReorder();
                if (!reorderCommitted)
                {
                    RestoreSelectionAfterCanceledReorder();
                }
            }
        }
        finally
        {
            ClearTabReorder(releasePointer: true, deferIndicatorRefresh: reorderCommitted);
        }
        return shouldHandle;
    }

    private bool CommitTabReorder()
    {
        if (!TabReorderHelper.TryResolveItemsList(this, out var list) ||
            !TabReorderHelper.CanMoveItems(list))
        {
            return false;
        }

        var oldIndex = TabReorderHelper.FindItemIndex(list, _tabReorderItem, _tabReorderOldIndex);
        if (!TabReorderHelper.IsValidIndex(oldIndex, list.Count))
        {
            return false;
        }

        var newIndex = TabReorderHelper.NormalizeTargetIndex(oldIndex, _tabReorderTargetIndex, list.Count);
        if (!TabReorderHelper.IsValidIndex(newIndex, list.Count) || newIndex == oldIndex)
        {
            return false;
        }

        var draggedContainer = _tabReorderContainer;
        var item = _tabReorderItem;
        var selectedItem = _tabReorderSelectedItem;
        var reorderingArgs = new TabReorderingEventArgs(TabReorderingEvent, item, oldIndex, newIndex);
        if (!TabReorderHelper.RaiseReorderingEvent(this, list, reorderingArgs) ||
            !ReferenceEquals(_tabReorderContainer, draggedContainer))
        {
            return false;
        }

        TabReorderHelper.MoveItem(list, oldIndex, newIndex);
        RestoreSelectionAfterReorder(list, selectedItem);

        var reorderedArgs = new TabReorderedEventArgs(TabReorderedEvent, item, oldIndex, newIndex);
        RaiseEvent(reorderedArgs);
        return true;
    }

    private void RestoreSelectionAfterReorder(IList list, object? selectedItem)
    {
        if (selectedItem is null)
        {
            return;
        }

        var selectedIndex = TabReorderHelper.FindItemIndex(list, selectedItem, SelectedIndex);
        if (TabReorderHelper.IsValidIndex(selectedIndex, list.Count))
        {
            SelectedIndex = selectedIndex;
            SelectedItem  = selectedItem;
        }
    }

    private void RestoreSelectionAfterCanceledReorder()
    {
        if (!TabReorderHelper.TryResolveItemsList(this, out var list))
        {
            return;
        }

        RestoreSelectionAfterReorder(list, _tabReorderSelectedItem);
    }

    private void CancelTabReorder()
    {
        ClearTabReorder(releasePointer: true);
    }

    private void ClearTabReorder(bool releasePointer, bool deferIndicatorRefresh = false)
    {
        var pointer       = _tabReorderPointer;
        var captureTarget = _tabReorderContainer;

        if (deferIndicatorRefresh)
        {
            BeginTabReorderIndicatorRefreshDeferral();
        }
        else
        {
            CancelPendingTabReorderIndicatorRefresh();
        }

        SetTabReorderContainerDragging(false);

        _tabReorderContainer    = null;
        _tabReorderPointer      = null;
        _tabReorderItem         = null;
        _tabReorderSelectedItem = null;
        _tabReorderOldIndex     = TabReorderHelper.InvalidIndex;
        _tabReorderTargetIndex  = TabReorderHelper.InvalidIndex;
        _isTabReorderDragging   = false;
        _tabReorderStartPointerRootPosition = default;
        _tabReorderLastPointerRootPosition  = default;
        _tabReorderPointerAnchorPrimary     = 0;
        LayoutUpdated -= HandleTabReorderLayoutUpdated;
        ClearTabReorderLivePreview();
        if (deferIndicatorRefresh)
        {
            ScheduleTabReorderIndicatorRefreshAfterLayout();
        }
        else
        {
            CompleteTabReorderIndicatorRefresh();
        }
        StopTabReorderAutoScroll();

        if (releasePointer &&
            pointer is not null &&
            (ReferenceEquals(pointer.Captured, this) || ReferenceEquals(pointer.Captured, captureTarget)))
        {
            pointer.Capture(null);
        }

    }

    private void BeginTabReorderIndicatorRefreshDeferral()
    {
        _isTabReorderIndicatorRefreshDeferred = true;
        SetTabReorderIndicatorTransitionSuppressed(true);
    }

    private void ScheduleTabReorderIndicatorRefreshAfterLayout()
    {
        if (_isTabReorderIndicatorRefreshPending)
        {
            return;
        }

        _isTabReorderIndicatorRefreshPending = true;
        LayoutUpdated += HandleTabReorderIndicatorRefreshLayoutUpdated;
    }

    private void HandleTabReorderIndicatorRefreshLayoutUpdated(object? sender, EventArgs args)
    {
        CompletePendingTabReorderIndicatorRefresh();
    }

    private void CompletePendingTabReorderIndicatorRefresh()
    {
        if (!_isTabReorderIndicatorRefreshPending)
        {
            return;
        }

        _isTabReorderIndicatorRefreshPending = false;
        LayoutUpdated -= HandleTabReorderIndicatorRefreshLayoutUpdated;
        CompleteTabReorderIndicatorRefresh();
    }

    private void CompleteTabReorderIndicatorRefresh()
    {
        _isTabReorderIndicatorRefreshDeferred = false;
        try
        {
            NotifyTabReorderPreviewChanged();
        }
        finally
        {
            SetTabReorderIndicatorTransitionSuppressed(false);
        }
    }

    private void CancelPendingTabReorderIndicatorRefresh()
    {
        if (_isTabReorderIndicatorRefreshPending)
        {
            LayoutUpdated -= HandleTabReorderIndicatorRefreshLayoutUpdated;
        }

        _isTabReorderIndicatorRefreshPending = false;
        _isTabReorderIndicatorRefreshDeferred = false;
    }

    private void SetTabReorderContainerDragging(bool isDragging)
    {
        _tabReorderContainer?.SetTabReorderDragging(isDragging);
    }

    private void UpdateTabReorderIndicatorTransitionSuppression()
    {
        SetTabReorderIndicatorTransitionSuppressed(IsSelectedTabReorderContainerDragging());
    }

    private bool IsSelectedTabReorderContainerDragging()
    {
        return _isTabReorderDragging &&
               _tabReorderContainer is not null &&
               SelectedItem is not null &&
               ReferenceEquals(ContainerFromItem(SelectedItem), _tabReorderContainer);
    }

    private void SetTabReorderIndicatorTransitionSuppressed(bool isSuppressed)
    {
        if (isSuppressed)
        {
            _tabReorderIndicatorSuppression ??= new TransitionSuppressionScope(this);
        }
        else
        {
            var suppression = _tabReorderIndicatorSuppression;
            _tabReorderIndicatorSuppression = null;
            suppression?.Dispose();
        }
    }

    int ITabOverflowOwner.OverflowItemCount => ItemCount;

    int ITabOverflowOwner.OverflowSelectedIndex => SelectedIndex;

    INotifyCollectionChanged ITabOverflowOwner.OverflowItems => ItemsView;

    event EventHandler? ITabOverflowOwner.OverflowSelectionChanged
    {
        add => _overflowSelectionChanged += value;
        remove => _overflowSelectionChanged -= value;
    }

    Control? ITabOverflowOwner.GetOverflowContainer(int index) => ContainerFromIndex(index);

    object? ITabOverflowOwner.GetOverflowLogicalItem(int index) =>
        index >= 0 && index < ItemsView.Count ? ItemsView[index] : null;

    TabOverflowItem ITabOverflowOwner.CreateOverflowItem(int index, object? logicalItem, Control container)
    {
        if (container is not TabStripItem tabStripItem)
        {
            throw new ArgumentException("TabStrip overflow containers must be TabStripItem instances.", nameof(container));
        }

        return new TabOverflowItem(
            logicalItem,
            tabStripItem.Content,
            tabStripItem.ContentTemplate,
            tabStripItem.IsEffectivelyEnabled,
            index == SelectedIndex,
            tabStripItem.IsClosable);
    }

    bool ITabOverflowOwner.TryActivateOverflowItem(int index, object? logicalItem, Control container)
    {
        if (!IsCurrentOverflowEntry(index, logicalItem, container) || !container.IsEffectivelyEnabled)
        {
            return false;
        }

        container.BringIntoView();
        SelectedIndex = index;
        return true;
    }

    bool ITabOverflowOwner.TryCloseOverflowItem(int index, object? logicalItem, Control container)
    {
        return IsCurrentOverflowEntry(index, logicalItem, container) &&
               container is TabStripItem tabStripItem &&
               CloseTab(tabStripItem);
    }

    private bool IsCurrentOverflowEntry(int index, object? logicalItem, Control container)
    {
        return index >= 0 &&
               index < ItemsView.Count &&
               ReferenceEquals(ContainerFromIndex(index), container) &&
               Equals(ItemsView[index], logicalItem);
    }
}

public class TabStripClosingEventArgs : RoutedEventArgs
{
    public TabStripClosingEventArgs(RoutedEvent routedEvent, TabStripItem tabStripItem)
        : base(routedEvent)
    {
        TabStripItem = tabStripItem;
    }
    
    public TabStripItem TabStripItem { get; }
    public bool Cancel { get; set; }
}

public class TabStripClosedEventArgs : RoutedEventArgs
{
    public TabStripClosedEventArgs(RoutedEvent routedEvent, TabStripItem tabStripItem)
        : base(routedEvent)
    {
        TabStripItem = tabStripItem;
    }
    
    public TabStripItem TabStripItem { get; }
}
