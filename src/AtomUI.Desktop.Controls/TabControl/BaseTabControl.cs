using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class BaseTabControl : SelectingItemsControl, IMotionAwareControl, ITabOverflowOwner
{
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel());

    #region 公共属性定义
    
    public static readonly StyledProperty<Dock> TabStripPlacementProperty =
        AvaloniaProperty.Register<BaseTabControl, Dock>(nameof(TabStripPlacement), defaultValue: Dock.Top);
    
    public static readonly StyledProperty<IReadOnlyList<double>?> BorderDashArrayProperty =
        AvaloniaProperty.Register<BaseTabControl, IReadOnlyList<double>?>(nameof(BorderDashArray));

    public static readonly StyledProperty<double> BorderDashOffsetProperty =
        AvaloniaProperty.Register<BaseTabControl, double>(nameof(BorderDashOffset));

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<BaseTabControl>();
    
    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<BaseTabControl>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<BaseTabControl>();

    public static readonly StyledProperty<IDataTemplate?> OverflowPopupTemplateProperty =
        AvaloniaProperty.Register<BaseTabControl, IDataTemplate?>(nameof(OverflowPopupTemplate));
    
    public static readonly DirectProperty<BaseTabControl, object?> SelectedContentProperty =
        AvaloniaProperty.RegisterDirect<BaseTabControl, object?>(nameof(SelectedContent), o => o.SelectedContent);

    public static readonly DirectProperty<BaseTabControl, IDataTemplate?> SelectedContentTemplateProperty =
        AvaloniaProperty.RegisterDirect<BaseTabControl, IDataTemplate?>(nameof(SelectedContentTemplate), o => o.SelectedContentTemplate);
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<BaseTabControl>();

    public static readonly StyledProperty<bool> TabAlignmentCenterProperty =
        AvaloniaProperty.Register<BaseTabControl, bool>(nameof(TabAlignmentCenter));

    public static readonly StyledProperty<bool> IsTabReorderEnabledProperty =
        AvaloniaProperty.Register<BaseTabControl, bool>(nameof(IsTabReorderEnabled));

    public static readonly StyledProperty<TabActivationTrigger> TabActivationTriggerProperty =
        AvaloniaProperty.Register<BaseTabControl, TabActivationTrigger>(
            nameof(TabActivationTrigger),
            TabActivationTrigger.PointerReleased);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<BaseTabControl>();
    
    public static readonly StyledProperty<double> HeaderStartEdgePaddingProperty = 
        AvaloniaProperty.Register<BaseTabControl, double>(nameof (HeaderStartEdgePadding));
    
    public static readonly StyledProperty<double> HeaderEndEdgePaddingProperty = 
        AvaloniaProperty.Register<BaseTabControl, double>(nameof (HeaderEndEdgePadding));
    
    public static readonly StyledProperty<Thickness> ContentPaddingProperty = 
        AvaloniaProperty.Register<BaseTabControl, Thickness>(nameof (ContentPadding));
    
    public static readonly StyledProperty<double> TabAndContentGutterProperty =
        AvaloniaProperty.Register<BaseTabControl, double>(nameof(TabAndContentGutter));
    
    public static readonly StyledProperty<object?> HeaderStartExtraContentProperty = 
        AvaloniaProperty.Register<BaseTabControl, object?>(nameof (HeaderStartExtraContent));
    
    public static readonly StyledProperty<IDataTemplate?> HeaderStartExtraContentTemplateProperty =
        AvaloniaProperty.Register<BaseTabControl, IDataTemplate?>(nameof(HeaderStartExtraContentTemplate));
    
    public static readonly StyledProperty<object?> HeaderEndExtraContentProperty = 
        AvaloniaProperty.Register<BaseTabControl, object?>(nameof (HeaderEndExtraContent));
    
    public static readonly StyledProperty<IDataTemplate?> HeaderEndExtraContentTemplateProperty =
        AvaloniaProperty.Register<BaseTabControl, IDataTemplate?>(nameof(HeaderEndExtraContentTemplate));
    
    public static readonly StyledProperty<bool> IsTabClosableProperty =
        AvaloniaProperty.Register<BaseTabControl, bool>(nameof(IsTabClosable));
    
    public static readonly StyledProperty<bool> IsTabAutoHideCloseButtonProperty =
        AvaloniaProperty.Register<BaseTabControl, bool>(nameof(IsTabAutoHideCloseButton));

    #region 公共事件定义
    
    public static readonly RoutedEvent<TabClosingEventArgs> ClosingEvent =
        RoutedEvent.Register<BaseTabControl, TabClosingEventArgs>(nameof(Closing), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<TabClosedEventArgs> ClosedEvent =
        RoutedEvent.Register<BaseTabControl, TabClosedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    internal static readonly RoutedEvent<TabReorderingEventArgs> TabReorderingEvent =
        RoutedEvent.Register<BaseTabControl, TabReorderingEventArgs>(nameof(TabReordering), RoutingStrategies.Bubble);

    internal static readonly RoutedEvent<TabReorderedEventArgs> TabReorderedEvent =
        RoutedEvent.Register<BaseTabControl, TabReorderedEventArgs>(nameof(TabReordered), RoutingStrategies.Bubble);

    public event EventHandler<TabClosingEventArgs>? Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    public event EventHandler<TabClosedEventArgs>? Closed
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

    public Dock TabStripPlacement
    {
        get => GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
    }
    
    public IReadOnlyList<double>? BorderDashArray
    {
        get => GetValue(BorderDashArrayProperty);
        set => SetValue(BorderDashArrayProperty, value);
    }

    public double BorderDashOffset
    {
        get => GetValue(BorderDashOffsetProperty);
        set => SetValue(BorderDashOffsetProperty, value);
    }

    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }
    
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }
    
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
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
    
    public object? SelectedContent
    {
        get => _selectedContent;
        internal set => SetAndRaise(SelectedContentProperty, ref _selectedContent, value);
    }
    
    public IDataTemplate? SelectedContentTemplate
    {
        get => _selectedContentTemplate;
        internal set => SetAndRaise(SelectedContentTemplateProperty, ref _selectedContentTemplate, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
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
    
    public Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }
    
    public double TabAndContentGutter
    {
        get => GetValue(TabAndContentGutterProperty);
        set => SetValue(TabAndContentGutterProperty, value);
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

    #region 内部属性实现
    
    internal static readonly StyledProperty<IBrush?> SeparatorBorderBrushProperty =
        AvaloniaProperty.Register<BaseTabControl, IBrush?>(nameof(SeparatorBorderBrush));

    internal static readonly StyledProperty<Thickness> SeparatorBorderThicknessProperty =
        AvaloniaProperty.Register<BaseTabControl, Thickness>(nameof(SeparatorBorderThickness));

    internal IBrush? SeparatorBorderBrush
    {
        get => GetValue(SeparatorBorderBrushProperty);
        set => SetValue(SeparatorBorderBrushProperty, value);
    }

    internal Thickness SeparatorBorderThickness
    {
        get => GetValue(SeparatorBorderThicknessProperty);
        set => SetValue(SeparatorBorderThicknessProperty, value);
    }

    internal static readonly DirectProperty<BaseTabControl, Thickness> TabStripMarginProperty =
        AvaloniaProperty.RegisterDirect<BaseTabControl, Thickness>(nameof(TabStripMargin),
            o => o.TabStripMargin,
            (o, v) => o.TabStripMargin = v);
    
    internal static readonly DirectProperty<BaseTabControl, Thickness> EffectiveHeaderPaddingProperty =
        AvaloniaProperty.RegisterDirect<BaseTabControl, Thickness>(nameof(EffectiveHeaderPadding),
            o => o.EffectiveHeaderPadding,
            (o, v) => o.EffectiveHeaderPadding = v);

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Flyout.IsPopupPinnedOpenProperty.AddOwner<BaseTabControl>();

    private Thickness _tabStripMargin;

    internal Thickness TabStripMargin
    {
        get => _tabStripMargin;
        set => SetAndRaise(TabStripMarginProperty, ref _tabStripMargin, value);
    }
    
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

    internal ItemsPresenter? ItemsPresenterPart { get; private set; }

    internal ContentPresenter? ContentPart { get; private set; }
    #endregion
    
    private object? _selectedContent;
    private IDataTemplate? _selectedContentTemplate;
    private CompositeDisposable? _selectedItemSubscriptions;
    private Panel? _alignWrapper;
    private Point _tabStripBorderStartPoint;
    private Point _tabStripBorderEndPoint;
    private Pen? _tabStripBorderPen;
    private IBrush? _tabStripBorderPenBrush;
    private double _tabStripBorderPenThickness;
    private TabScrollViewer? _tabReorderScrollViewer;
    private EventHandler? _overflowSelectionChanged;
    private DispatcherTimer? _tabReorderAutoScrollTimer;
    private TabItem? _pendingTabActivationContainer;
    private IPointer? _pendingTabActivationPointer;
    private TabItem? _tabReorderContainer;
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
    private const int UnselectedIndex = -1;
    private const int FirstItemIndex  = 0;
    private const int SingleItemCount = 1;

    static BaseTabControl()
    {
        SelectionModeProperty.OverrideDefaultValue<BaseTabControl>(SelectionMode.AlwaysSelected);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<BaseTabControl>(false);
        ItemsPanelProperty.OverrideDefaultValue<BaseTabControl>(DefaultPanel);
        AffectsRender<BaseTabControl>(SeparatorBorderBrushProperty, SeparatorBorderThicknessProperty, UseLayoutRoundingProperty);
        AffectsMeasure<BaseTabControl>(TabStripMarginProperty, TabAndContentGutterProperty, TabStripPlacementProperty);
        SelectedItemProperty.Changed.AddClassHandler<BaseTabControl>((x, e) => x.UpdateSelectedContent());
    }

    public BaseTabControl()
    {
        AddHandler(PointerMovedEvent, HandleTabReorderPointerMoved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, HandleTabReorderPointerReleased, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(PointerCaptureLostEvent, HandleTabReorderPointerCaptureLost, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
    }
    
    public bool CloseTab(TabItem tabItem)
    {
        if (!tabItem.IsClosable)
        {
            return false;
        }

        if (!TabReorderHelper.TryResolveItemsList(this, out var list) ||
            !TabReorderHelper.CanMoveItems(list))
        {
            return false;
        }

        var index = TabReorderHelper.FindContainerIndex(this, list, tabItem);
        if (!TabReorderHelper.IsValidIndex(index, list.Count))
        {
            return false;
        }

        var item = list[index];
        var closingArgs = new TabClosingEventArgs(ClosingEvent, tabItem);
        RaiseEvent(closingArgs);

        if (closingArgs.Cancel || !tabItem.IsClosable ||
            !TabReorderHelper.TryGetCurrentItemIndex(this, list, tabItem, item, out index))
        { 
            return false;
        }

        var nextSelectionIndex = GetNextSelectionIndex(index);
        var nextSelectedItem = TabReorderHelper.IsValidIndex(nextSelectionIndex, list.Count)
            ? list[nextSelectionIndex]
            : null;
        SelectedIndex = nextSelectionIndex;
        if (!tabItem.IsClosable ||
            !TabReorderHelper.TryGetCurrentItemIndex(this, list, tabItem, item, out index))
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
        
        var closedArgs = new TabClosedEventArgs(ClosedEvent, tabItem);
        RaiseEvent(closedArgs);
        
        return true;
    }

    private int GetNextSelectionIndex(int closingIndex)
    {
        if (SelectedIndex != closingIndex)
        {
            return SelectedIndex;
        }

        var isClosingLastItem = Items.Count <= SingleItemCount;
        if (isClosingLastItem)
        {
            return UnselectedIndex;
        }

        var previousIndex = closingIndex - SingleItemCount;
        return previousIndex >= FirstItemIndex ? previousIndex : FirstItemIndex;
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ClearPendingTabActivation();
        CancelTabReorder();
        DetachTabScrollViewer();
        base.OnApplyTemplate(e);
        
        ItemsPresenterPart = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        ItemsPresenterPart?.ApplyTemplate();
        _tabReorderScrollViewer = TabReorderHelper.FindTabScrollViewer(e.NameScope);
        _tabReorderScrollViewer?.AttachOverflowOwner(this);

        UpdateTabStripPlacement();

        // Set TabNavigation to Once on the panel if not already set and
        // forward the TabOnceActiveElement to the panel.
        if (ItemsPresenterPart?.Panel is { } panel)
        {
            if (!panel.IsSet(KeyboardNavigation.TabNavigationProperty))
            {
                panel.SetCurrentValue(
                    KeyboardNavigation.TabNavigationProperty,
                    KeyboardNavigationMode.Once);
            }
            KeyboardNavigation.SetTabOnceActiveElement(
                panel,
                KeyboardNavigation.GetTabOnceActiveElement(this));
        }

        _alignWrapper = e.NameScope.Find<Panel>("PART_AlignWrapper");
        HandlePlacementChanged();
        ConfigureEffectiveHeaderPadding();
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

    public override bool UpdateSelectionFromEvent(Control container, RoutedEventArgs eventArgs)
    {
        if (eventArgs is FocusChangedEventArgs { NavigationMethod: not NavigationMethod.Directional })
        {
            return false;
        }

        return base.UpdateSelectionFromEvent(container, eventArgs);
    }

    internal void NotifyTabActivationPointerPressed(TabItem tabItem, PointerPressedEventArgs args)
    {
        if (TabActivationTrigger != TabActivationTrigger.PointerReleased ||
            args.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed ||
            !IsEnabled ||
            !tabItem.IsEnabled ||
            IsPointerFromEmbeddedButton(tabItem, args.Source as Visual))
        {
            ClearPendingTabActivation();
            return;
        }

        _pendingTabActivationContainer = tabItem;
        _pendingTabActivationPointer   = args.Pointer;
    }

    internal void NotifyTabActivationPointerReleased(TabItem tabItem, PointerReleasedEventArgs args)
    {
        if (_pendingTabActivationPointer == args.Pointer)
        {
            ClearPendingTabActivation();
        }
    }

    internal bool NotifyTabReorderPointerPressed(TabItem tabItem, PointerPressedEventArgs args)
    {
        if (!CanStartTabReorder(tabItem, args))
        {
            return false;
        }

        if (!TabReorderHelper.TryResolveItemsList(this, out var list))
        {
            return false;
        }

        var oldIndex = IndexFromContainer(tabItem);
        if (!TabReorderHelper.IsValidIndex(oldIndex, list.Count))
        {
            return false;
        }

        CancelTabReorder();

        var pointerRootPosition = TabReorderHelper.GetPointerRootPosition(this, args);
        if (!TabReorderHelper.TryGetPointerAnchorPrimary(
                this,
                TabStripPlacement,
                tabItem,
                pointerRootPosition,
                out var pointerAnchorPrimary))
        {
            return false;
        }

        _tabReorderContainer   = tabItem;
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

    internal void NotifyTabReorderPointerPressCompleted(TabItem tabItem, PointerPressedEventArgs args)
    {
        if (_tabReorderContainer != tabItem || _tabReorderPointer != args.Pointer)
        {
            return;
        }

        args.Pointer.Capture(tabItem);
    }

    internal bool NotifyTabReorderPointerMoved(TabItem tabItem, PointerEventArgs args)
    {
        if (_tabReorderContainer != tabItem || _tabReorderPointer != args.Pointer)
        {
            return false;
        }

        return UpdateActiveTabReorder(args);
    }

    internal bool NotifyTabReorderPointerReleased(TabItem tabItem, PointerReleasedEventArgs args)
    {
        if (_tabReorderContainer != tabItem || _tabReorderPointer != args.Pointer)
        {
            return false;
        }

        ClearPendingTabActivation();
        return CompleteActiveTabReorder(args);
    }

    internal void NotifyTabReorderPointerCaptureLost(TabItem tabItem)
    {
        if (_tabReorderContainer == tabItem)
        {
            ClearTabReorder(releasePointer: false);
        }
    }

    internal void NotifyTabItemIconStateChanged()
    {
        UpdateIconSlotReservation();
    }
    
    private void UpdateTabStripPlacement()
    {
        var controls = ItemsPresenterPart?.Panel?.Children;
        if (controls is null)
        {
            return;
        }

        foreach (var control in controls)
        {
            if (control is TabItem tabItem)
            {
                tabItem.TabStripPlacement = TabStripPlacement;
            }
        }

        UpdateIconSlotReservation();
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TabItem();
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TabItem>(item, out recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (index == SelectedIndex)
        {
            UpdateSelectedContent(container);
        }
        if (container is TabItem tabItem)
        {
            tabItem.TabStripPlacement = TabStripPlacement;
            
            if (item != null && item is not Visual)
            {
                if (!tabItem.IsSet(TabItem.ContentProperty))
                {
                    tabItem.SetCurrentValue(TabItem.ContentProperty, item);
                }

                if (item is ITabItemData tabItemData)
                {
                    if (!tabItem.IsSet(TabItem.IconProperty))
                    {
                        tabItem.SetCurrentValue(TabItem.IconProperty, tabItemData.Icon);
                    }
                    if (!tabItem.IsSet(TabItem.CloseIconProperty))
                    {
                        tabItem.SetCurrentValue(TabItem.CloseIconProperty, tabItemData.CloseIcon);
                    }
                    if (!tabItem.IsSet(TabItem.HeaderProperty))
                    {
                        tabItem.SetCurrentValue(TabItem.HeaderProperty, tabItemData.Header);
                    }
                    if (!tabItem.IsSet(TabItem.IsEnabledProperty))
                    {
                        tabItem.SetCurrentValue(TabItem.IsEnabledProperty, tabItemData.IsEnabled);
                    }
                    if (!tabItem.IsSet(TabItem.IsClosableProperty))
                    {
                        tabItem.SetCurrentValue(TabItem.IsClosableProperty, tabItemData.IsClosable);
                    }
                    if (!tabItem.IsSet(TabItem.IsAutoHideCloseButtonProperty))
                    {
                        tabItem.SetCurrentValue(TabItem.IsAutoHideCloseButtonProperty, tabItemData.IsAutoHideCloseButton);
                    }
                }
            }

            if (ItemTemplate != null)
            {
                tabItem[!TabItem.ContentTemplateProperty] = this[!ItemTemplateProperty];
            }

            tabItem[!TabItem.SizeTypeProperty] = this[!SizeTypeProperty];
            tabItem[!TabItem.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];

            PrepareTabItem(tabItem, item, index);
            ConfigureTabItem(tabItem);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type TabItem.");
        }
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is TabItem)
        {
            UpdateIconSlotReservation();
        }
    }

    protected virtual void PrepareTabItem(TabItem tabItem, object? item, int index)
    {
    }
    
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);

        var selectedIndex = SelectedIndex;

        if (selectedIndex == oldIndex || selectedIndex == newIndex)
        {
            UpdateSelectedContent();
        }
    }
    
    protected override void ClearContainerForItemOverride(Control element)
    {
        base.ClearContainerForItemOverride(element);
        if (element is TabItem tabItem)
        {
            tabItem.IsIconSlotReserved = false;
            ReleaseTabItemOwnerBindings(tabItem);
        }
        UpdateIconSlotReservation();
        UpdateSelectedContent();
    }

    // The indexer bindings created in PrepareContainerForItemOverride subscribe to this
    // owner's PropertyChanged, and Avalonia never disposes them on its own: without this
    // release every removed item stays alive for the owner's lifetime. Avalonia skips
    // ClearContainerForItemOverride for items that are their own container, so TabItem
    // also routes its direct logical detach into this method.
    internal virtual void ReleaseTabItemOwnerBindings(TabItem tabItem)
    {
        BindingOperations.GetBindingExpressionBase(tabItem, TabItem.SizeTypeProperty)?.Dispose();
        BindingOperations.GetBindingExpressionBase(tabItem, TabItem.IsMotionEnabledProperty)?.Dispose();
        BindingOperations.GetBindingExpressionBase(tabItem, TabItem.ContentTemplateProperty)?.Dispose();
    }
    
    private void UpdateSelectedContent(Control? container = null)
    {
        _selectedItemSubscriptions?.Dispose();
        _selectedItemSubscriptions = null;

        if (SelectedIndex == -1)
        {
            SelectedContent = SelectedContentTemplate = null;
        }
        else
        {
            container ??= ContainerFromIndex(SelectedIndex);
            if (container != null)
            {
                if (SelectedContentTemplate != SelectContentTemplate(container.GetValue(ContentTemplateProperty)))
                {
                    // If the value of SelectedContentTemplate is about to change, clear it first. This ensures
                    // that the template is not reused as soon as SelectedContent changes in the statement below
                    // this block, and also that controls generated from it are unloaded before SelectedContent
                    // (which is typically their DataContext) changes.
                    SelectedContentTemplate = null;
                }

                _selectedItemSubscriptions = new CompositeDisposable(
                    container.GetObservable(ContentControl.ContentProperty).Subscribe(v => SelectedContent = v),
                    container.GetObservable(ContentControl.ContentTemplateProperty).Subscribe(v => SelectedContentTemplate = SelectContentTemplate(v)));

                // Note how we fall back to our own ContentTemplate if the container doesn't specify one
                IDataTemplate? SelectContentTemplate(IDataTemplate? containerTemplate) => containerTemplate ?? ContentTemplate;
            }
        }
    }
    
    protected virtual bool RegisterContentPresenter(ContentPresenter presenter)
    {
        if (presenter.Name == "PART_SelectedContentHost")
        {
            ContentPart = presenter;
            return true;
        }

        return false;
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
            UpdateTabStripPlacement();
        }
        else if (change.Property == ContentTemplateProperty)
        {
            var newTemplate = change.GetNewValue<IDataTemplate?>();
            if (SelectedContentTemplate != newTemplate &&
                ContainerFromIndex(SelectedIndex) is { } container && 
                container.GetValue(ContentControl.ContentTemplateProperty) == null)
            {
                SelectedContentTemplate = newTemplate; // See also UpdateSelectedContent
            }
        }
        else if (change.Property == KeyboardNavigation.TabOnceActiveElementProperty &&
                 ItemsPresenterPart?.Panel is { } panel)
        {
            // Forward TabOnceActiveElement to the panel.
            KeyboardNavigation.SetTabOnceActiveElement(
                panel,
                change.GetNewValue<IInputElement?>());
        }
        if (change.Property == TabStripPlacementProperty || change.Property == TabAndContentGutterProperty)
        {
            UpdatePseudoClasses();
            HandlePlacementChanged();
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
                    if ((ContainerFromIndex(i) ?? Items[i]) is TabItem tabItem)
                    {
                        ConfigureTabItem(tabItem);
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

    private void HandlePlacementChanged()
    {
        if (TabStripPlacement == Dock.Top)
        {
            SetCurrentValue(TabStripMarginProperty, new Thickness(0, 0, 0, TabAndContentGutter));
        }
        else if (TabStripPlacement == Dock.Right)
        {
            SetCurrentValue(TabStripMarginProperty, new Thickness(TabAndContentGutter, 0, 0, 0));
        }
        else if (TabStripPlacement == Dock.Bottom)
        {
            SetCurrentValue(TabStripMarginProperty, new Thickness(0, TabAndContentGutter, 0, 0));
        }
        else
        {
            SetCurrentValue(TabStripMarginProperty, new Thickness(0, 0, TabAndContentGutter, 0));
        }

        ConfigureEffectiveHeaderPadding();
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

    private void SetupTabStripBorderPoints()
    {
        if (_alignWrapper is not null)
        {
            var offset          = _alignWrapper.TranslatePoint(new Point(0, 0), this) ?? default;
            var size            = _alignWrapper.Bounds.Size;
            var borderThickness = BorderUtils.BuildRenderScaleAwareThickness(this, SeparatorBorderThickness.Left);
            var offsetDelta     = borderThickness / 2;
            if (TabStripPlacement == Dock.Top)
            {
                _tabStripBorderStartPoint = new Point(0, size.Height - offsetDelta);
                _tabStripBorderEndPoint   = new Point(size.Width, size.Height - offsetDelta);
            }
            else if (TabStripPlacement == Dock.Right)
            {
                _tabStripBorderStartPoint = new Point(offsetDelta, 0);
                _tabStripBorderEndPoint   = new Point(offsetDelta, size.Height);
            }
            else if (TabStripPlacement == Dock.Bottom)
            {
                _tabStripBorderStartPoint = new Point(0, offsetDelta);
                _tabStripBorderEndPoint   = new Point(size.Width, offsetDelta);
            }
            else
            {
                _tabStripBorderStartPoint = new Point(size.Width - offsetDelta, 0);
                _tabStripBorderEndPoint   = new Point(size.Width - offsetDelta, size.Height);
            }

            _tabStripBorderStartPoint += offset;
            _tabStripBorderEndPoint   += offset;
        }
    }

    public override void Render(DrawingContext context)
    {
        if (Items.Count > 0)
        {
            SetupTabStripBorderPoints();
            var borderThickness = SeparatorBorderThickness.Left;
            using var optionState = context.PushRenderOptions(new RenderOptions
            {
                EdgeMode = EdgeMode.Aliased
            });
            context.DrawLine(GetTabStripBorderPen(borderThickness), _tabStripBorderStartPoint, _tabStripBorderEndPoint);
        }
    }

    private Pen GetTabStripBorderPen(double borderThickness)
    {
        if (_tabStripBorderPen is null ||
            !ReferenceEquals(_tabStripBorderPenBrush, SeparatorBorderBrush) ||
            !double.Equals(_tabStripBorderPenThickness, borderThickness))
        {
            _tabStripBorderPenBrush     = SeparatorBorderBrush;
            _tabStripBorderPenThickness = borderThickness;
            _tabStripBorderPen          = new Pen(SeparatorBorderBrush, borderThickness);
        }

        return _tabStripBorderPen!;
    }
    
    private void ConfigureTabItem(TabItem tabItem)
    {
        tabItem.SetValue(TabItem.IsClosableProperty, IsTabClosable, BindingPriority.Template);
        tabItem.SetValue(TabItem.IsAutoHideCloseButtonProperty, IsTabAutoHideCloseButton, BindingPriority.Template);
    }

    private void UpdateIconSlotReservation()
    {
        var shouldReserve = IsVerticalTabStripPlacement() && HasAnyTabItemIcon();

        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is TabItem tabItem)
            {
                tabItem.IsIconSlotReserved = shouldReserve;
            }
        }
    }

    private bool HasAnyTabItemIcon()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is TabItem { HasIcon: true })
            {
                return true;
            }

            var item = Items[i];
            if (item is TabItem { Icon: not null } ||
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

    private bool CanStartTabReorder(TabItem tabItem, PointerPressedEventArgs args)
    {
        return IsTabReorderEnabled &&
               IsEnabled &&
               tabItem.IsEnabled &&
               ItemCount > SingleItemCount &&
               args.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed &&
               !IsPointerFromEmbeddedButton(tabItem, args.Source as Visual);
    }

    private static bool IsPointerFromEmbeddedButton(TabItem tabItem, Visual? source)
    {
        for (var current = source; current is not null && current != tabItem; current = current.GetVisualParent())
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
        if (selectable is not TabItem tabItem ||
            _pendingTabActivationContainer != tabItem ||
            _pendingTabActivationPointer != args.Pointer)
        {
            return false;
        }

        var position = args.GetPosition(tabItem);
        return new Rect(default, tabItem.Bounds.Size).Contains(position);
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
        if (this is TabControl tabControl)
        {
            tabControl.NotifyTabReorderPreviewChanged();
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
        UpdateSelectedContent();

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

        _tabReorderContainer   = null;
        _tabReorderPointer     = null;
        _tabReorderItem        = null;
        _tabReorderSelectedItem = null;
        _tabReorderOldIndex    = TabReorderHelper.InvalidIndex;
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
        if (container is not TabItem tabItem)
        {
            throw new ArgumentException("TabControl overflow containers must be TabItem instances.", nameof(container));
        }

        return new TabOverflowItem(
            logicalItem,
            tabItem.Header,
            tabItem.HeaderTemplate,
            tabItem.IsEffectivelyEnabled,
            index == SelectedIndex,
            tabItem.IsClosable);
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
               container is TabItem tabItem &&
               CloseTab(tabItem);
    }

    private bool IsCurrentOverflowEntry(int index, object? logicalItem, Control container)
    {
        return index >= 0 &&
               index < ItemsView.Count &&
               ReferenceEquals(ContainerFromIndex(index), container) &&
               Equals(ItemsView[index], logicalItem);
    }
}


public class TabClosingEventArgs : RoutedEventArgs
{
    public TabClosingEventArgs(RoutedEvent routedEvent, TabItem tabItem)
        : base(routedEvent)
    {
        TabItem = tabItem;
    }
    
    public TabItem TabItem { get; }
    public bool Cancel { get; set; }
}

public class TabClosedEventArgs : RoutedEventArgs
{
    public TabClosedEventArgs(RoutedEvent routedEvent, TabItem tabItem)
        : base(routedEvent)
    {
        TabItem = tabItem;
    }
    
    public TabItem TabItem { get; }
}
