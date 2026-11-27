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

/// <summary>
/// Desktop tab strip and base class of <c>TabStrip</c> and <c>CardTabStrip</c>. On top of
/// <see cref="AbstractTabStrip" /> it adds tabs that can be closed and dragged to another
/// position, extra content before and after the tabs, a preset tab size, and an overflow list for
/// the tabs that do not fit in the viewport.
/// </summary>
public abstract class BaseTabStrip : AbstractTabStrip,
                                     ISizeTypeAware,
                                     IMotionAwareControl,
                                     ITabOverflowOwner
{
    /// <summary>
    /// Always <see langword="true" />: the base class binds no selection indicator when the template is
    /// applied. A strip that shows an indicator binds its own template parts itself through
    /// <c>SetCustomIndicatorParts</c>.
    /// </summary>
    private protected override bool UsesCustomIndicatorTransition => true;

    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel());

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<BaseTabStrip>();

    /// <summary>
    /// Identifies the <see cref="TabStripPlacement" /> property.
    /// </summary>
    public new static readonly StyledProperty<Dock> TabStripPlacementProperty =
        AbstractTabStrip.TabStripPlacementProperty.AddOwner<BaseTabStrip>();

    /// <summary>
    /// Identifies the <see cref="TabAlignmentCenter" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> TabAlignmentCenterProperty =
        AvaloniaProperty.Register<BaseTabStrip, bool>(nameof(TabAlignmentCenter));

    /// <summary>
    /// Identifies the <see cref="IsTabReorderEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTabReorderEnabledProperty =
        AvaloniaProperty.Register<BaseTabStrip, bool>(nameof(IsTabReorderEnabled));

    /// <summary>
    /// Identifies the <see cref="TabActivationTrigger" /> property.
    /// </summary>
    public static readonly StyledProperty<TabActivationTrigger> TabActivationTriggerProperty =
        AvaloniaProperty.Register<BaseTabStrip, TabActivationTrigger>(
            nameof(TabActivationTrigger),
            TabActivationTrigger.PointerReleased);

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsMotionEnabledProperty =
        AbstractTabStrip.IsMotionEnabledProperty.AddOwner<BaseTabStrip>();
    
    /// <summary>
    /// Identifies the <see cref="HeaderStartEdgePadding" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HeaderStartEdgePaddingProperty = 
        AvaloniaProperty.Register<BaseTabStrip, double>(nameof (HeaderStartEdgePadding));
    
    /// <summary>
    /// Identifies the <see cref="HeaderEndEdgePadding" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HeaderEndEdgePaddingProperty = 
        AvaloniaProperty.Register<BaseTabStrip, double>(nameof (HeaderEndEdgePadding));
    
    /// <summary>
    /// Identifies the <see cref="HeaderStartExtraContent" /> property.
    /// </summary>
    public new static readonly StyledProperty<object?> HeaderStartExtraContentProperty =
        AbstractTabStrip.HeaderStartExtraContentProperty.AddOwner<BaseTabStrip>();
    
    /// <summary>
    /// Identifies the <see cref="HeaderStartExtraContentTemplate" /> property.
    /// </summary>
    public new static readonly StyledProperty<IDataTemplate?> HeaderStartExtraContentTemplateProperty =
        AbstractTabStrip.HeaderStartExtraContentTemplateProperty.AddOwner<BaseTabStrip>();
    
    /// <summary>
    /// Identifies the <see cref="HeaderEndExtraContent" /> property.
    /// </summary>
    public new static readonly StyledProperty<object?> HeaderEndExtraContentProperty =
        AbstractTabStrip.HeaderEndExtraContentProperty.AddOwner<BaseTabStrip>();
    
    /// <summary>
    /// Identifies the <see cref="HeaderEndExtraContentTemplate" /> property.
    /// </summary>
    public new static readonly StyledProperty<IDataTemplate?> HeaderEndExtraContentTemplateProperty =
        AbstractTabStrip.HeaderEndExtraContentTemplateProperty.AddOwner<BaseTabStrip>();

    /// <summary>
    /// Identifies the <see cref="OverflowPopupTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> OverflowPopupTemplateProperty =
        BaseTabControl.OverflowPopupTemplateProperty.AddOwner<BaseTabStrip>();
    
    /// <summary>
    /// Identifies the <see cref="IsTabClosable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTabClosableProperty =
        AvaloniaProperty.Register<BaseTabStrip, bool>(nameof(IsTabClosable));
    
    /// <summary>
    /// Identifies the <see cref="IsTabAutoHideCloseButton" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTabAutoHideCloseButtonProperty =
        AvaloniaProperty.Register<BaseTabStrip, bool>(nameof(IsTabAutoHideCloseButton));
    
    /// <summary>
    /// Preset size of the tabs. The strip binds the size of every tab to this value, and the tab theme
    /// selects the padding, font size and icon size of the tab by it. Set it on the strip, not on a tab.
    /// </summary>
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Placement of the strip; <see cref="AbstractTabStrip.TabStripPlacement" /> describes how the
    /// selection indicator follows it. The desktop strip also copies the value onto every tab, which
    /// picks horizontal or vertical margins and padding from it, updates the placement pseudo-classes
    /// of the strip, draws its baseline along the edge that faces the content, and maps
    /// <see cref="HeaderStartEdgePadding" /> and <see cref="HeaderEndEdgePadding" /> to the left and
    /// right edges or to the top and bottom edges.
    /// </summary>
    public new Dock TabStripPlacement
    {
        get => GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
    }

    /// <summary>
    /// When <see langword="true" />, the theme centers the tabs along the strip direction:
    /// horizontally for a strip placed at the top or bottom, vertically for one placed at the left or
    /// right. The <c>TabStrip</c> theme centers its whole header layout, extra content included; the
    /// <c>CardTabStrip</c> theme centers the tab list only. The default is <see langword="false" />.
    /// </summary>
    public bool TabAlignmentCenter
    {
        get => GetValue(TabAlignmentCenterProperty);
        set => SetValue(TabAlignmentCenterProperty, value);
    }

    /// <summary>
    /// When <see langword="true" />, the user can drag a tab with the left mouse button to another
    /// position of the strip. A drag starts only while the strip and the pressed tab are enabled, the
    /// strip has at least two items and the press is not on a button inside the tab. While dragging,
    /// the other tabs preview the new position and the viewport scrolls when the pointer reaches one
    /// of its edges. On release the item is moved in the items list, which must be one the strip can
    /// modify: <see cref="TabReordering" /> is raised before the move and <see cref="TabReordered" />
    /// after it. The default is <see langword="false" />.
    /// </summary>
    public bool IsTabReorderEnabled
    {
        get => GetValue(IsTabReorderEnabledProperty);
        set => SetValue(IsTabReorderEnabledProperty, value);
    }

    /// <summary>
    /// Pointer event that selects a tab. The default is <c>PointerReleased</c>: a tab is selected when
    /// the left mouse button is released over the same tab on which it was pressed, while strip and
    /// tab are enabled. <c>PointerPressed</c> selects a tab when the button is pressed on it. With
    /// <c>PointerReleased</c>, the pending selection is dropped when the press comes from a button
    /// inside the tab, such as its close button, or when the tab is dragged to reorder it.
    /// </summary>
    public TabActivationTrigger TabActivationTrigger
    {
        get => GetValue(TabActivationTriggerProperty);
        set => SetValue(TabActivationTriggerProperty, value);
    }
    
    /// <summary>
    /// Motion switch of the strip and of its tabs. The strip binds the value onto every tab and onto
    /// the tabs container of its template; <see cref="AbstractTabStrip.IsMotionEnabled" /> describes
    /// how the value controls the selection indicator and the centering scroll.
    /// </summary>
    /// <remarks>
    /// The strip theme binds the value to the <c>EnableMotion</c> token; clearing a local value falls
    /// back to that binding.
    /// </remarks>
    public new bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Space between the start edge of the strip and its header layout, which holds the extra content
    /// and the tabs, in device-independent pixels. The start edge is the left edge for a strip placed
    /// at the top or bottom and the top edge for one placed at the left or right. The default is
    /// <c>0</c>.
    /// </summary>
    public double HeaderStartEdgePadding
    {
        get => GetValue(HeaderStartEdgePaddingProperty);
        set => SetValue(HeaderStartEdgePaddingProperty, value);
    }
    
    /// <summary>
    /// Space between the end edge of the strip and its header layout, which holds the extra content
    /// and the tabs, in device-independent pixels. The end edge is the right edge for a strip placed
    /// at the top or bottom and the bottom edge for one placed at the left or right. The default is
    /// <c>0</c>.
    /// </summary>
    public double HeaderEndEdgePadding
    {
        get => GetValue(HeaderEndEdgePaddingProperty);
        set => SetValue(HeaderEndEdgePaddingProperty, value);
    }
    
    /// <summary>
    /// Extra content shown at the start of the strip, in front of the tabs and inside the header edge
    /// paddings. The default is <see langword="null" />, which shows nothing.
    /// </summary>
    [DependsOn(nameof(HeaderStartExtraContentTemplate))]
    public new object? HeaderStartExtraContent
    {
        get => GetValue(HeaderStartExtraContentProperty);
        set => SetValue(HeaderStartExtraContentProperty, value);
    }
    
    /// <summary>
    /// Template used to build <see cref="HeaderStartExtraContent" />. The default is
    /// <see langword="null" />, which leaves the choice of a template to the content presenter.
    /// </summary>
    public new IDataTemplate? HeaderStartExtraContentTemplate
    {
        get => GetValue(HeaderStartExtraContentTemplateProperty);
        set => SetValue(HeaderStartExtraContentTemplateProperty, value);
    }
    
    /// <summary>
    /// Extra content shown at the end of the strip, after the tabs and inside the header edge
    /// paddings. The default is <see langword="null" />, which shows nothing.
    /// </summary>
    [DependsOn(nameof(HeaderEndExtraContentTemplate))]
    public new object? HeaderEndExtraContent
    {
        get => GetValue(HeaderEndExtraContentProperty);
        set => SetValue(HeaderEndExtraContentProperty, value);
    }
    
    /// <summary>
    /// Template used to build <see cref="HeaderEndExtraContent" />. The default is
    /// <see langword="null" />, which leaves the choice of a template to the content presenter.
    /// </summary>
    public new IDataTemplate? HeaderEndExtraContentTemplate
    {
        get => GetValue(HeaderEndExtraContentTemplateProperty);
        set => SetValue(HeaderEndExtraContentTemplateProperty, value);
    }

    /// <summary>
    /// Template used to present tabs that are not fully visible in the tab viewport. The template data item is a
    /// <see cref="TabOverflowPopupContext" />.
    /// </summary>
    public IDataTemplate? OverflowPopupTemplate
    {
        get => GetValue(OverflowPopupTemplateProperty);
        set => SetValue(OverflowPopupTemplateProperty, value);
    }
    
    /// <summary>
    /// When <see langword="true" />, every tab of the strip is closable and shows its close button;
    /// clicking the button calls <see cref="CloseTab" />. The strip writes the value onto each tab at
    /// template priority, so a value set on the tab itself wins. The default is <see langword="false" />.
    /// </summary>
    public bool IsTabClosable
    {
        get => GetValue(IsTabClosableProperty);
        set => SetValue(IsTabClosableProperty, value);
    }
    
    /// <summary>
    /// When <see langword="true" />, the close button of a closable tab stays transparent until the
    /// pointer is over the tab, while still taking its place and receiving clicks. It changes nothing
    /// on a tab that is not closable. The strip writes the value onto each tab at template priority,
    /// so a value set on the tab itself wins. The default is <see langword="false" />.
    /// </summary>
    public bool IsTabAutoHideCloseButton
    {
        get => GetValue(IsTabAutoHideCloseButtonProperty);
        set => SetValue(IsTabAutoHideCloseButtonProperty, value);
    }

    #endregion
    
    #region 公共事件定义
    
    /// <summary>
    /// Identifies the <see cref="Closing" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<TabStripClosingEventArgs> ClosingEvent =
        RoutedEvent.Register<BaseTabStrip, TabStripClosingEventArgs>(nameof(Closing), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="Closed" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<TabStripClosedEventArgs> ClosedEvent =
        RoutedEvent.Register<BaseTabStrip, TabStripClosedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    internal static readonly RoutedEvent<TabReorderingEventArgs> TabReorderingEvent =
        RoutedEvent.Register<BaseTabStrip, TabReorderingEventArgs>(nameof(TabReordering), RoutingStrategies.Bubble);

    internal static readonly RoutedEvent<TabReorderedEventArgs> TabReorderedEvent =
        RoutedEvent.Register<BaseTabStrip, TabReorderedEventArgs>(nameof(TabReordered), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised by <see cref="CloseTab" /> before a tab is closed, while its item is still in the strip.
    /// This happens when the user clicks the close button of the tab or closes the tab from the
    /// overflow list. A handler sets <see cref="TabStripClosingEventArgs.Cancel" /> to keep the tab
    /// open; the item then stays in the strip and <see cref="Closed" /> is not raised.
    /// </summary>
    public event EventHandler<TabStripClosingEventArgs>? Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    /// <summary>
    /// Raised after a tab was closed by <see cref="CloseTab" /> and its item was removed from the items
    /// of the strip. The event cannot be canceled.
    /// </summary>
    public event EventHandler<TabStripClosedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    /// <summary>
    /// Raised before a tab that was dragged to another position is moved in the items list, while the
    /// old order is still in place. Cancelling the event keeps the original order: the item then stays
    /// where it was and <see cref="TabReordered" /> is not raised.
    /// </summary>
    public event EventHandler<TabReorderingEventArgs>? TabReordering
    {
        add => AddHandler(TabReorderingEvent, value);
        remove => RemoveHandler(TabReorderingEvent, value);
    }

    /// <summary>
    /// Raised after a tab was moved to another position in the items list by dragging, with the
    /// selection restored to the item that was selected before the drag.
    /// </summary>
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
    
    /// <summary>
    /// Closes a tab: raises <see cref="Closing" />, moves the selection off the tab when it is
    /// selected, removes its item from the items of the strip and raises <see cref="Closed" />.
    /// </summary>
    /// <param name="tabStripItem">
    /// The tab to close. It must be a closable container of this strip.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the item was removed. <see langword="false" /> when the tab is not
    /// closable, is not a container of this strip, the items of the strip are not in a list the strip
    /// can modify, or a handler of <see cref="Closing" /> set
    /// <see cref="TabStripClosingEventArgs.Cancel" />. The item then stays in the strip.
    /// </returns>
    /// <remarks>
    /// When the closed tab is selected, the selection moves to the previous tab, or to the next tab
    /// when the closed tab is the first one, and is cleared when the tab was the only item.
    /// </remarks>
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
    

    /// <summary>
    /// Prepares a tab container: copies the placement of the strip onto it, fills it from a data item,
    /// binds it to the item template, size and motion flag of the strip, and applies
    /// <see cref="IsTabClosable" /> and <see cref="IsTabAutoHideCloseButton" /> to it.
    /// </summary>
    /// <remarks>
    /// The container must be a <see cref="TabStripItem" />; any other type causes an
    /// <c>ArgumentOutOfRangeException</c>. A data item that is not a visual becomes the content of the
    /// tab. An item that implements <see cref="ITabItemData" /> also provides the header, icon, close
    /// icon, enabled state, closable state and close button auto-hide setting of the tab; a property
    /// that already has a value on the tab is left alone. The base member is called first and
    /// <see cref="PrepareTabStripItem" /> is called last. A derived override calls the base member,
    /// otherwise the tab is neither filled nor bound to the strip.
    /// </remarks>
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

    /// <summary>
    /// Releases what the strip set on a tab container: its icon slot reservation and the bindings to
    /// the item template, size and motion flag of the strip. The base member is called first.
    /// </summary>
    /// <remarks>
    /// Releasing the bindings lets a removed tab be collected. A derived override calls the base
    /// member, otherwise every removed tab stays alive for the lifetime of the strip.
    /// </remarks>
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

    /// <summary>
    /// Refreshes the icon slot reservation of all tabs once a tab container has been prepared. The
    /// base member is called first; a derived override calls it to keep the reservation in step with
    /// the tabs.
    /// </summary>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is TabStripItem)
        {
            UpdateIconSlotReservation();
        }
    }

    /// <summary>
    /// Lets a derived strip prepare a tab further. It is called from
    /// <see cref="PrepareContainerForItemOverride" /> after the tab was filled from its item and bound
    /// to the strip, and before <see cref="IsTabClosable" /> and <see cref="IsTabAutoHideCloseButton" />
    /// are applied to it. The default does nothing.
    /// </summary>
    protected virtual void PrepareTabStripItem(TabStripItem tabStripItem, object? item, int index)
    {
    }

    /// <summary>
    /// Keeps the tabs, the header padding and the overflow list in step with property changes of the
    /// strip. The base member is called first.
    /// </summary>
    /// <remarks>
    /// A change of <see cref="TabStripPlacement" /> updates the placement pseudo-classes, the placement
    /// of every tab, the header padding and the icon slot reservation. A change of
    /// <see cref="HeaderStartEdgePadding" /> or <see cref="HeaderEndEdgePadding" /> updates the header
    /// padding. A change of <see cref="IsTabClosable" /> or <see cref="IsTabAutoHideCloseButton" />
    /// writes the new value onto every tab. A change of the selected index notifies the overflow list.
    /// </remarks>
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

    /// <summary>
    /// Draws the baseline of the strip along the edge that faces the content: the bottom edge for a
    /// strip placed at the top, the top edge for one placed at the bottom, the right edge for one placed
    /// at the left and the left edge for one placed at the right. The line uses <c>BorderBrush</c> and
    /// the left value of <c>BorderThickness</c>, made render-scale aware, and is drawn only while the
    /// strip has items. A derived override calls the base member to keep the line.
    /// </summary>
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

    /// <summary>
    /// Drops a pending tab activation and a running tab reorder, detaches the strip from the tabs
    /// container of the old template and attaches it to the tabs container of the new template as its
    /// overflow owner, then refreshes the header padding and the icon slot reservation.
    /// </summary>
    /// <remarks>
    /// The base member is called between the detach and the attach step. A derived override calls it
    /// to keep both.
    /// </remarks>
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

    /// <summary>
    /// Attaches the strip as the overflow owner of its tabs container, so that the overflow list follows
    /// the strip again after it was detached from a visual tree. The base member is called first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _tabReorderScrollViewer?.AttachOverflowOwner(this);
    }

    /// <summary>
    /// Detaches the strip from its tabs container, so that the overflow list no longer follows it, and
    /// drops a pending tab activation and a running tab reorder. The base member is called last.
    /// </summary>
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

    /// <summary>
    /// Decides whether a pointer event on a tab selects the tab. It follows
    /// <see cref="TabActivationTrigger" />: with <c>PointerReleased</c>, a tab is selected by the release of
    /// the left mouse button over the same tab on which it was pressed, while strip and tab are enabled; with
    /// <c>PointerPressed</c>, the press of the left mouse button selects the tab as the base member decides.
    /// A derived override calls the base member to keep this decision.
    /// </summary>
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

/// <summary>
/// Carries the data of the <see cref="BaseTabStrip.Closing" /> event.
/// </summary>
public class TabStripClosingEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Creates the arguments of a <see cref="BaseTabStrip.Closing" /> event for the tab that is about
    /// to be closed.
    /// </summary>
    public TabStripClosingEventArgs(RoutedEvent routedEvent, TabStripItem tabStripItem)
        : base(routedEvent)
    {
        TabStripItem = tabStripItem;
    }
    
    /// <summary>
    /// The tab that is about to be closed.
    /// </summary>
    public TabStripItem TabStripItem { get; }
    /// <summary>
    /// Set to <see langword="true" /> to keep the tab open: the strip then leaves its item in place and
    /// does not raise <see cref="BaseTabStrip.Closed" />.
    /// </summary>
    public bool Cancel { get; set; }
}

/// <summary>
/// Carries the data of the <see cref="BaseTabStrip.Closed" /> event.
/// </summary>
public class TabStripClosedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Creates the arguments of a <see cref="BaseTabStrip.Closed" /> event for the tab that was closed.
    /// </summary>
    public TabStripClosedEventArgs(RoutedEvent routedEvent, TabStripItem tabStripItem)
        : base(routedEvent)
    {
        TabStripItem = tabStripItem;
    }
    
    /// <summary>
    /// The tab that was closed.
    /// </summary>
    public TabStripItem TabStripItem { get; }
}
