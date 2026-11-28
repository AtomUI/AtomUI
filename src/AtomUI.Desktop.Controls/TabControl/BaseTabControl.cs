using System.Collections;
using System.Collections.Specialized;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Theme.Resources;
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

/// <summary>
/// Base class of the desktop tab controls <see cref="TabControl" /> and <see cref="CardTabControl" />. It
/// presents the items as tabs in a tab strip above, below or beside the content of the selected tab, draws
/// the separator line along the tab strip, and provides tab closing, drag reordering and the overflow list
/// of the tabs that do not fit in the tab strip.
/// </summary>
/// <remarks>
/// One tab stays selected as long as there are items, and every item is presented by a <see cref="TabItem" />.
/// </remarks>
public class BaseTabControl : AbstractTabControl, IMotionAwareControl, ITabOverflowOwner
{
    /// <summary>
    /// Always <see langword="true" />: the base class binds no selection indicator when the template is
    /// applied and selects no tab on a left button release. A control that shows an indicator binds its own
    /// template parts itself through <c>SetCustomIndicatorParts</c>, and <see cref="TabItem" /> updates the
    /// selection from the release.
    /// </summary>
    private protected override bool UsesCustomIndicatorTransition => true;

    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel());

    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="TabStripPlacement" /> property.
    /// </summary>
    public new static readonly StyledProperty<Dock> TabStripPlacementProperty =
        AbstractTabControl.TabStripPlacementProperty.AddOwner<BaseTabControl>();
    
    /// <summary>
    /// Identifies the <see cref="BorderDashArray" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<double>?> BorderDashArrayProperty =
        AvaloniaProperty.Register<BaseTabControl, IReadOnlyList<double>?>(nameof(BorderDashArray));

    /// <summary>
    /// Identifies the <see cref="BorderDashOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<double> BorderDashOffsetProperty =
        AvaloniaProperty.Register<BaseTabControl, double>(nameof(BorderDashOffset));

    /// <summary>
    /// Identifies the <see cref="HorizontalContentAlignment" /> property.
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<BaseTabControl>();
    
    /// <summary>
    /// Identifies the <see cref="VerticalContentAlignment" /> property.
    /// </summary>
    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<BaseTabControl>();
    
    /// <summary>
    /// Identifies the <see cref="ContentTemplate" /> property.
    /// </summary>
    public new static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AbstractTabControl.ContentTemplateProperty.AddOwner<BaseTabControl>();

    /// <summary>
    /// Identifies the <see cref="OverflowPopupTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> OverflowPopupTemplateProperty =
        AvaloniaProperty.Register<BaseTabControl, IDataTemplate?>(nameof(OverflowPopupTemplate));
    
    /// <summary>
    /// Identifies the <see cref="SelectedContent" /> property.
    /// </summary>
    public new static readonly DirectProperty<BaseTabControl, object?> SelectedContentProperty =
        AbstractTabControl.SelectedContentProperty.AddOwner<BaseTabControl>(owner => owner.SelectedContent);

    /// <summary>
    /// Identifies the <see cref="SelectedContentTemplate" /> property.
    /// </summary>
    public new static readonly DirectProperty<BaseTabControl, IDataTemplate?> SelectedContentTemplateProperty =
        AbstractTabControl.SelectedContentTemplateProperty.AddOwner<BaseTabControl>(owner => owner.SelectedContentTemplate);
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<BaseTabControl>();

    /// <summary>
    /// Identifies the <see cref="TabAlignmentCenter" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> TabAlignmentCenterProperty =
        AvaloniaProperty.Register<BaseTabControl, bool>(nameof(TabAlignmentCenter));

    /// <summary>
    /// Identifies the <see cref="IsTabReorderEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTabReorderEnabledProperty =
        AvaloniaProperty.Register<BaseTabControl, bool>(nameof(IsTabReorderEnabled));

    /// <summary>
    /// Identifies the <see cref="TabActivationTrigger" /> property.
    /// </summary>
    public static readonly StyledProperty<TabActivationTrigger> TabActivationTriggerProperty =
        AvaloniaProperty.Register<BaseTabControl, TabActivationTrigger>(
            nameof(TabActivationTrigger),
            TabActivationTrigger.PointerReleased);

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsMotionEnabledProperty =
        AbstractTabControl.IsMotionEnabledProperty.AddOwner<BaseTabControl>();
    
    /// <summary>
    /// Identifies the <see cref="HeaderStartEdgePadding" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HeaderStartEdgePaddingProperty = 
        AvaloniaProperty.Register<BaseTabControl, double>(nameof (HeaderStartEdgePadding));
    
    /// <summary>
    /// Identifies the <see cref="HeaderEndEdgePadding" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HeaderEndEdgePaddingProperty = 
        AvaloniaProperty.Register<BaseTabControl, double>(nameof (HeaderEndEdgePadding));
    
    /// <summary>
    /// Identifies the <see cref="ContentPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> ContentPaddingProperty = 
        AvaloniaProperty.Register<BaseTabControl, Thickness>(nameof (ContentPadding));
    
    /// <summary>
    /// Identifies the <see cref="TabAndContentGutter" /> property.
    /// </summary>
    public static readonly StyledProperty<double> TabAndContentGutterProperty =
        AvaloniaProperty.Register<BaseTabControl, double>(nameof(TabAndContentGutter));
    
    /// <summary>
    /// Identifies the <see cref="HeaderStartExtraContent" /> property.
    /// </summary>
    public new static readonly StyledProperty<object?> HeaderStartExtraContentProperty =
        AbstractTabControl.HeaderStartExtraContentProperty.AddOwner<BaseTabControl>();
    
    /// <summary>
    /// Identifies the <see cref="HeaderStartExtraContentTemplate" /> property.
    /// </summary>
    public new static readonly StyledProperty<IDataTemplate?> HeaderStartExtraContentTemplateProperty =
        AbstractTabControl.HeaderStartExtraContentTemplateProperty.AddOwner<BaseTabControl>();
    
    /// <summary>
    /// Identifies the <see cref="HeaderEndExtraContent" /> property.
    /// </summary>
    public new static readonly StyledProperty<object?> HeaderEndExtraContentProperty =
        AbstractTabControl.HeaderEndExtraContentProperty.AddOwner<BaseTabControl>();
    
    /// <summary>
    /// Identifies the <see cref="HeaderEndExtraContentTemplate" /> property.
    /// </summary>
    public new static readonly StyledProperty<IDataTemplate?> HeaderEndExtraContentTemplateProperty =
        AbstractTabControl.HeaderEndExtraContentTemplateProperty.AddOwner<BaseTabControl>();
    
    /// <summary>
    /// Identifies the <see cref="IsTabClosable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTabClosableProperty =
        AvaloniaProperty.Register<BaseTabControl, bool>(nameof(IsTabClosable));
    
    /// <summary>
    /// Identifies the <see cref="IsTabAutoHideCloseButton" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTabAutoHideCloseButtonProperty =
        AvaloniaProperty.Register<BaseTabControl, bool>(nameof(IsTabAutoHideCloseButton));

    #region 公共事件定义
    
    /// <summary>
    /// Identifies the <see cref="Closing" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<TabClosingEventArgs> ClosingEvent =
        RoutedEvent.Register<BaseTabControl, TabClosingEventArgs>(nameof(Closing), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="Closed" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<TabClosedEventArgs> ClosedEvent =
        RoutedEvent.Register<BaseTabControl, TabClosedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    internal static readonly RoutedEvent<TabReorderingEventArgs> TabReorderingEvent =
        RoutedEvent.Register<BaseTabControl, TabReorderingEventArgs>(nameof(TabReordering), RoutingStrategies.Bubble);

    internal static readonly RoutedEvent<TabReorderedEventArgs> TabReorderedEvent =
        RoutedEvent.Register<BaseTabControl, TabReorderedEventArgs>(nameof(TabReordered), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised before a tab is closed by <see cref="CloseTab" />, while its item is still in the items list.
    /// </summary>
    /// <remarks>
    /// A handler sets <see cref="TabClosingEventArgs.Cancel" /> to <see langword="true" /> to keep the tab: the
    /// item then stays in the list and <see cref="Closed" /> is not raised.
    /// </remarks>
    public event EventHandler<TabClosingEventArgs>? Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    /// <summary>
    /// Raised after a tab was closed by <see cref="CloseTab" /> and its item was removed from the items list,
    /// after the selection was updated.
    /// </summary>
    public event EventHandler<TabClosedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    /// <summary>
    /// Raised before a drag reorder moves a tab to another index, at the end of the drag.
    /// </summary>
    /// <remarks>
    /// A handler sets <see cref="TabReorderingEventArgs.Cancel" /> to <see langword="true" /> to keep the original
    /// order: the item is not moved and <see cref="TabReordered" /> is not raised.
    /// </remarks>
    public event EventHandler<TabReorderingEventArgs>? TabReordering
    {
        add => AddHandler(TabReorderingEvent, value);
        remove => RemoveHandler(TabReorderingEvent, value);
    }

    /// <summary>
    /// Raised after a drag reorder moved a tab to another index and the selection was updated.
    /// </summary>
    /// <remarks>
    /// Not raised when the drop leaves the tab at its index or when <see cref="TabReordering" /> was canceled.
    /// </remarks>
    public event EventHandler<TabReorderedEventArgs>? TabReordered
    {
        add => AddHandler(TabReorderedEvent, value);
        remove => RemoveHandler(TabReorderedEvent, value);
    }
    
    #endregion

    /// <summary>
    /// Edge of the control that holds the tab strip; the content of the selected tab takes the remaining space.
    /// The default is <see cref="Dock.Top" />.
    /// </summary>
    /// <remarks>
    /// The value is copied to every <see cref="TabItem" />, it places <see cref="TabAndContentGutter" /> between
    /// the tab strip and the content, and it decides which edges <see cref="HeaderStartEdgePadding" /> and
    /// <see cref="HeaderEndEdgePadding" /> pad. The separator line runs along the side of the tab strip that
    /// faces the content.
    /// </remarks>
    public new Dock TabStripPlacement
    {
        get => GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
    }
    
    /// <summary>
    /// Dash pattern of the frame border around the whole control, given as alternating dash and gap lengths of
    /// the <c>BorderBrush</c> stroke. <see langword="null" />, the default, draws a solid border.
    /// </summary>
    /// <remarks>
    /// The separator line along the tab strip is drawn by <see cref="Render" /> and stays solid.
    /// </remarks>
    public IReadOnlyList<double>? BorderDashArray
    {
        get => GetValue(BorderDashArrayProperty);
        set => SetValue(BorderDashArrayProperty, value);
    }

    /// <summary>
    /// Position within the dash pattern of <see cref="BorderDashArray" /> where the frame border stroke starts.
    /// The default is 0, the start of the first dash.
    /// </summary>
    public double BorderDashOffset
    {
        get => GetValue(BorderDashOffsetProperty);
        set => SetValue(BorderDashOffsetProperty, value);
    }

    /// <summary>
    /// Horizontal alignment of the content of the selected tab inside the content area, which is inset by
    /// <see cref="ContentPadding" />.
    /// </summary>
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }
    
    /// <summary>
    /// Vertical alignment of the content of the selected tab inside the content area, which is inset by
    /// <see cref="ContentPadding" />.
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }
    
    /// <summary>
    /// Template used to present the content of the selected tab when the tab item has no
    /// <c>ContentTemplate</c> of its own. The default is <see langword="null" />, which leaves the choice to the
    /// content presenter.
    /// </summary>
    public new IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to present tabs that are not fully visible in the tab viewport.
    /// The template data item is a <see cref="TabOverflowPopupContext" />.
    /// </summary>
    public IDataTemplate? OverflowPopupTemplate
    {
        get => GetValue(OverflowPopupTemplateProperty);
        set => SetValue(OverflowPopupTemplateProperty, value);
    }
    
    /// <summary>
    /// Content of the selected tab, presented in the content area with <see cref="SelectedContentTemplate" />.
    /// The control updates the value when the selection or the items change; it is <see langword="null" /> when no
    /// tab is selected.
    /// </summary>
    public new object? SelectedContent
    {
        get => base.SelectedContent;
        internal set => base.SelectedContent = value;
    }
    
    /// <summary>
    /// Template used to present <see cref="SelectedContent" />: the <c>ContentTemplate</c> of the selected tab
    /// item, or <see cref="ContentTemplate" /> when the tab item has none. It is <see langword="null" /> when no
    /// tab is selected.
    /// </summary>
    public new IDataTemplate? SelectedContentTemplate
    {
        get => base.SelectedContentTemplate;
        internal set => base.SelectedContentTemplate = value;
    }
    
    /// <summary>
    /// Size grade of the tabs (<c>Large</c>, <c>Middle</c> or <c>Small</c>). The value is copied to every
    /// <see cref="TabItem" />.
    /// </summary>
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Centers the tab strip header, which holds the extra contents and the tabs, along the edge of the control
    /// instead of stretching it across the whole control. The default is <see langword="false" />.
    /// </summary>
    public bool TabAlignmentCenter
    {
        get => GetValue(TabAlignmentCenterProperty);
        set => SetValue(TabAlignmentCenterProperty, value);
    }

    /// <summary>
    /// Enables reordering the tabs by dragging them with the left mouse button. The default is
    /// <see langword="false" />.
    /// </summary>
    /// <remarks>
    /// A drag starts only while the control and the pressed tab are enabled, the control has more than one tab
    /// and the press did not start on a button inside the tab, such as its close button. The move is committed
    /// when the button is released and reports <see cref="TabReordering" /> and <see cref="TabReordered" />.
    /// </remarks>
    public bool IsTabReorderEnabled
    {
        get => GetValue(IsTabReorderEnabledProperty);
        set => SetValue(IsTabReorderEnabledProperty, value);
    }

    /// <summary>
    /// Pointer event that selects a tab: pressing the left button on it (<c>PointerPressed</c>) or releasing it
    /// over the tab (<c>PointerReleased</c>, the default).
    /// </summary>
    /// <remarks>
    /// With <c>PointerReleased</c>, the release must belong to the press that started on the same tab, must end
    /// inside the tab, and neither the control nor the tab may be disabled.
    /// </remarks>
    public TabActivationTrigger TabActivationTrigger
    {
        get => GetValue(TabActivationTriggerProperty);
        set => SetValue(TabActivationTriggerProperty, value);
    }

    /// <summary>
    /// Controls whether the selection indicator and the scroll that centers the selected tab
    /// (<c>IsSelectedItemCentered</c>) animate when the selection changes. When <see langword="false" />, both
    /// take their new position at once.
    /// </summary>
    /// <remarks>
    /// The control theme sets the value from the <c>EnableMotion</c> token, so clearing a local value falls back
    /// to that token. The value is also passed to every <see cref="TabItem" /> and to the tab scroll viewer.
    /// </remarks>
    public new bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Empty space between the start of the control and the tab strip header, which holds the extra contents and
    /// the tabs. It pads the left edge for <see cref="Dock.Top" /> and <see cref="Dock.Bottom" />, and the top
    /// edge for <see cref="Dock.Left" /> and <see cref="Dock.Right" />. The default is 0.
    /// </summary>
    public double HeaderStartEdgePadding
    {
        get => GetValue(HeaderStartEdgePaddingProperty);
        set => SetValue(HeaderStartEdgePaddingProperty, value);
    }
    
    /// <summary>
    /// Empty space between the tab strip header, which holds the extra contents and the tabs, and the end of the
    /// control. It pads the right edge for <see cref="Dock.Top" /> and <see cref="Dock.Bottom" />, and the bottom
    /// edge for <see cref="Dock.Left" /> and <see cref="Dock.Right" />. The default is 0.
    /// </summary>
    public double HeaderEndEdgePadding
    {
        get => GetValue(HeaderEndEdgePaddingProperty);
        set => SetValue(HeaderEndEdgePaddingProperty, value);
    }
    
    /// <summary>
    /// Empty space between the frame of the control and the content of the selected tab. It does not pad the tab
    /// strip. The default is 0.
    /// </summary>
    public Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }
    
    /// <summary>
    /// Gap between the tab strip and the content of the selected tab, placed on the side of the tab strip that
    /// faces the content. The default is 0; the <see cref="TabControl" /> and <see cref="CardTabControl" /> themes
    /// set it from their token of the same name.
    /// </summary>
    public double TabAndContentGutter
    {
        get => GetValue(TabAndContentGutterProperty);
        set => SetValue(TabAndContentGutterProperty, value);
    }
    
    /// <summary>
    /// Extra content placed at the start of the tab strip header, in front of the tab items: on the left of a
    /// horizontal tab strip and at the top of a vertical one. The default is <see langword="null" />, which shows
    /// nothing.
    /// </summary>
    [DependsOn(nameof(HeaderStartExtraContentTemplate))]
    public new object? HeaderStartExtraContent
    {
        get => GetValue(HeaderStartExtraContentProperty);
        set => SetValue(HeaderStartExtraContentProperty, value);
    }
    
    /// <summary>
    /// Template used to build <see cref="HeaderStartExtraContent" />. The default is <see langword="null" />,
    /// which leaves the choice of a template to the content presenter.
    /// </summary>
    public new IDataTemplate? HeaderStartExtraContentTemplate
    {
        get => GetValue(HeaderStartExtraContentTemplateProperty);
        set => SetValue(HeaderStartExtraContentTemplateProperty, value);
    }
    
    /// <summary>
    /// Extra content placed at the end of the tab strip header, behind the tab items: on the right of a
    /// horizontal tab strip and at the bottom of a vertical one. The default is <see langword="null" />, which
    /// shows nothing.
    /// </summary>
    [DependsOn(nameof(HeaderEndExtraContentTemplate))]
    public new object? HeaderEndExtraContent
    {
        get => GetValue(HeaderEndExtraContentProperty);
        set => SetValue(HeaderEndExtraContentProperty, value);
    }
    
    /// <summary>
    /// Template used to build <see cref="HeaderEndExtraContent" />. The default is <see langword="null" />,
    /// which leaves the choice of a template to the content presenter.
    /// </summary>
    public new IDataTemplate? HeaderEndExtraContentTemplate
    {
        get => GetValue(HeaderEndExtraContentTemplateProperty);
        set => SetValue(HeaderEndExtraContentTemplateProperty, value);
    }
    
    /// <summary>
    /// Writes <see cref="TabItem.IsClosable" /> onto every tab, so that each tab shows a close button whose click
    /// calls <see cref="CloseTab" />. The default is <see langword="false" />.
    /// </summary>
    /// <remarks>
    /// The value is written at template priority, so a value set on the tab or supplied by an
    /// <see cref="ITabItemData" /> item wins. It is written when a tab is prepared and whenever this property
    /// changes.
    /// </remarks>
    public bool IsTabClosable
    {
        get => GetValue(IsTabClosableProperty);
        set => SetValue(IsTabClosableProperty, value);
    }
    
    /// <summary>
    /// Writes <see cref="TabItem.IsAutoHideCloseButton" /> onto every tab: while <see langword="true" />, the
    /// close button of a closable tab stays transparent until the pointer is over the tab. The default is
    /// <see langword="false" />.
    /// </summary>
    /// <remarks>
    /// The value is written at template priority, so a value set on the tab or supplied by an
    /// <see cref="ITabItemData" /> item wins. It has no effect on tabs that are not closable.
    /// </remarks>
    public bool IsTabAutoHideCloseButton
    {
        get => GetValue(IsTabAutoHideCloseButtonProperty);
        set => SetValue(IsTabAutoHideCloseButtonProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="TabStripMargin" /> property.
    /// </summary>
    public static readonly DirectProperty<BaseTabControl, Thickness> TabStripMarginProperty =
        AvaloniaProperty.RegisterDirect<BaseTabControl, Thickness>(nameof(TabStripMargin),
            o => o.TabStripMargin,
            (o, v) => o.TabStripMargin = v);

    private Thickness _tabStripMargin;

    /// <summary>
    /// Margin applied around the tab strip header container based on <see cref="TabControl.TabStripPlacement" />.
    /// </summary>
    public Thickness TabStripMargin
    {
        get => _tabStripMargin;
        internal set => SetAndRaise(TabStripMarginProperty, ref _tabStripMargin, value);
    }
    #endregion

    #region 内部属性实现
    
    internal static readonly StyledProperty<bool> UseSeparatorTokenResourcesProperty =
        AvaloniaProperty.Register<BaseTabControl, bool>(nameof(UseSeparatorTokenResources));

    internal static readonly StyledProperty<bool> UseCardRadiusTokenResourcesProperty =
        AvaloniaProperty.Register<BaseTabControl, bool>(nameof(UseCardRadiusTokenResources));

    internal bool UseSeparatorTokenResources
    {
        get => GetValue(UseSeparatorTokenResourcesProperty);
        set => SetValue(UseSeparatorTokenResourcesProperty, value);
    }

    internal bool UseCardRadiusTokenResources
    {
        get => GetValue(UseCardRadiusTokenResourcesProperty);
        set => SetValue(UseCardRadiusTokenResourcesProperty, value);
    }

    internal IBrush? SeparatorBorderBrush { get; private set; }
    internal Thickness SeparatorBorderThickness { get; private set; }

    private const int SeparatorResourceCount = 2;
    private OwnerTokenSet? _chromeTokens;
    /// <summary>
    /// The corner radius of the control, read from the token resource that <c>CardRadiusKeys</c> gives for
    /// the current <c>SizeType</c>, or the default while <c>UseCardRadiusTokenResources</c> is
    /// <see langword="false" /> or no keys are given. It changes only through the chrome style refresh, which
    /// then calls <c>OnCardRadiusChanged</c>.
    /// </summary>
    private protected CornerRadius CardRadius { get; private set; }
    /// <summary>
    /// The resource keys of the border-radius tokens watched while <c>UseCardRadiusTokenResources</c> is
    /// <see langword="true" />, one for each size type in the order small, middle and large. The base property
    /// returns <see langword="null" />, which keeps <c>CardRadius</c> at its default.
    /// </summary>
    private protected virtual object[]? CardRadiusKeys => null;

    internal static readonly DirectProperty<BaseTabControl, Thickness> EffectiveHeaderPaddingProperty =
        AvaloniaProperty.RegisterDirect<BaseTabControl, Thickness>(nameof(EffectiveHeaderPadding),
            o => o.EffectiveHeaderPadding,
            (o, v) => o.EffectiveHeaderPadding = v);

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Flyout.IsPopupPinnedOpenProperty.AddOwner<BaseTabControl>();

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

    internal new ContentPresenter? ContentPart => base.ContentPart;
    #endregion
    
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
        AffectsRender<BaseTabControl>(UseLayoutRoundingProperty);
        AffectsMeasure<BaseTabControl>(TabStripMarginProperty, TabAndContentGutterProperty, TabStripPlacementProperty);
    }

    public BaseTabControl()
    {
        AddHandler(PointerMovedEvent, HandleTabReorderPointerMoved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, HandleTabReorderPointerReleased, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(PointerCaptureLostEvent, HandleTabReorderPointerCaptureLost, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
    }
    
    /// <summary>
    /// Closes a tab by removing its item from the items list. <see cref="Closing" /> is raised first, and a
    /// handler cancels the close by setting <see cref="TabClosingEventArgs.Cancel" />; <see cref="Closed" /> is
    /// raised after the item was removed.
    /// </summary>
    /// <param name="tabItem">
    /// The tab container to close.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the item was removed; <see langword="false" /> when the tab is not closable,
    /// its item is not found in the items list, the list cannot be changed or the close was canceled.
    /// </returns>
    /// <remarks>
    /// When the closed tab was selected, the tab before it is selected instead, or the tab after it when the
    /// first tab is closed. Nothing is selected when the last tab is closed.
    /// </remarks>
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
    
    /// <summary>
    /// Resolves the template parts when the template is applied: the items presenter, the tab scroll viewer,
    /// which then takes this control as its overflow owner, and the alignment wrapper that positions the
    /// separator line. It then copies <see cref="TabStripPlacement" /> to the tabs and updates the tab strip
    /// margin and the header padding. A pending tab activation and a running drag reorder are canceled first.
    /// </summary>
    /// <remarks>
    /// An override must call the base method, and should do so before it looks up its own parts.
    /// </remarks>
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

    /// <summary>
    /// Refreshes the subscription to the separator and card radius tokens and registers this control as the
    /// overflow owner of its tab scroll viewer. The base method runs first.
    /// </summary>
    /// <remarks>
    /// An override must call the base method.
    /// </remarks>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateChromeTokenSubscription();
        _tabReorderScrollViewer?.AttachOverflowOwner(this);
    }

    /// <summary>
    /// Disposes the token subscription, unregisters this control from its tab scroll viewer, and cancels a
    /// pending tab activation and a running drag reorder without committing it. The base method runs last.
    /// </summary>
    /// <remarks>
    /// An override must call the base method.
    /// </remarks>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _chromeTokens?.Dispose();
        _chromeTokens = null;
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
    /// Decides whether a left button press or release on a tab selects the tab, following
    /// <see cref="TabActivationTrigger" />.
    /// </summary>
    /// <param name="selectable">
    /// The tab that received the pointer event.
    /// </param>
    /// <param name="eventArgs">
    /// The pointer event.
    /// </param>
    /// <returns>
    /// For a release: <see langword="true" /> only when the trigger is <c>PointerReleased</c>, the control and the
    /// tab are enabled, and the release ends the press that started on the same tab inside its bounds. For a
    /// press: the result of the base method when the trigger is <c>PointerPressed</c>, otherwise
    /// <see langword="false" />.
    /// </returns>
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

    /// <summary>
    /// Updates the selection for an input event on a tab container. A focus change that was not caused by
    /// directional keys never selects the tab.
    /// </summary>
    /// <param name="container">
    /// The tab container that raised the event.
    /// </param>
    /// <param name="eventArgs">
    /// The input event.
    /// </param>
    /// <returns>
    /// <see langword="false" /> for a <see cref="FocusChangedEventArgs" /> whose navigation method is not
    /// <see cref="NavigationMethod.Directional" />; otherwise the result of the base method.
    /// </returns>
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
    
    /// <summary>
    /// Creates the container for an item: always a new <see cref="TabItem" />.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TabItem();
    }
    
    /// <summary>
    /// Decides whether an item needs a container: an item that is not a <see cref="TabItem" /> does, an item that
    /// already is one is its own container.
    /// </summary>
    /// <param name="item">
    /// The item to present.
    /// </param>
    /// <param name="index">
    /// The index of the item.
    /// </param>
    /// <param name="recycleKey">
    /// Receives the key that a recycled container must match for this item.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the item needs a container, otherwise <see langword="false" />.
    /// </returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TabItem>(item, out recycleKey);
    }
    
    /// <summary>
    /// Prepares a tab container for its item. It copies <see cref="TabStripPlacement" /> to the tab, binds
    /// <see cref="SizeType" /> and <see cref="IsMotionEnabled" /> from this control, fills the tab from a
    /// non-visual item and its <see cref="ITabItemData" /> values where the tab has no value of its own, and
    /// updates the selected content when the tab is the selected one.
    /// </summary>
    /// <param name="container">
    /// The <see cref="TabItem" /> container.
    /// </param>
    /// <param name="item">
    /// The item the container presents.
    /// </param>
    /// <param name="index">
    /// The index of the item.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="container" /> is not a <see cref="TabItem" />.
    /// </exception>
    /// <remarks>
    /// <see cref="PrepareTabItem" /> runs before <see cref="IsTabClosable" /> and
    /// <see cref="IsTabAutoHideCloseButton" /> are written onto the tab at template priority.
    /// </remarks>
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

    /// <summary>
    /// Updates the reserved icon slot of the tabs once a container was prepared. On a vertical tab strip, every
    /// tab reserves room for an icon as soon as one tab shows an icon.
    /// </summary>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is TabItem)
        {
            UpdateIconSlotReservation();
        }
    }

    /// <summary>
    /// Called for a tab container after its values were filled from the item and before
    /// <see cref="IsTabClosable" /> and <see cref="IsTabAutoHideCloseButton" /> are written onto it. The default
    /// implementation does nothing.
    /// </summary>
    /// <param name="tabItem">
    /// The prepared tab container.
    /// </param>
    /// <param name="item">
    /// The item the container presents.
    /// </param>
    /// <param name="index">
    /// The index of the item.
    /// </param>
    /// <remarks>
    /// A value set here on the tab wins over those two control values, because they are written at template
    /// priority. The base method need not be called.
    /// </remarks>
    protected virtual void PrepareTabItem(TabItem tabItem, object? item, int index)
    {
    }
    
    /// <inheritdoc />
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
    }
    
    /// <summary>
    /// Clears a tab container that is no longer used for its item: it resets the reserved icon slot, disposes the
    /// bindings this control created on the tab through <see cref="ReleaseTabItemOwnerBindings" />, and then
    /// updates the reserved icon slot of all tabs and the selected content.
    /// </summary>
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
    
    /// <inheritdoc />
    protected override bool RegisterContentPresenter(ContentPresenter presenter) => base.RegisterContentPresenter(presenter);

    /// <summary>
    /// Reacts to property changes after the base method: it refreshes the token subscription, copies
    /// <see cref="TabStripPlacement" /> to the tabs, updates the tab strip margin and the header padding,
    /// forwards <c>TabOnceActiveElement</c> to the items panel, rewrites <see cref="IsTabClosable" /> and
    /// <see cref="IsTabAutoHideCloseButton" /> on the tabs, and tells the overflow popup about selection changes.
    /// </summary>
    /// <remarks>
    /// An override must call the base method.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if ((change.Property == UseSeparatorTokenResourcesProperty || change.Property == UseCardRadiusTokenResourcesProperty) &&
            this.IsAttachedToVisualTree())
        {
            UpdateChromeTokenSubscription();
        }
        else if (change.Property == SizeTypeProperty && _chromeTokens is not null)
        {
            RefreshChromeStyle();
        }
        if (change.Property == SelectedIndexProperty)
        {
            _overflowSelectionChanged?.Invoke(this, EventArgs.Empty);
        }
        if (change.Property == TabStripPlacementProperty)
        {
            UpdateTabStripPlacement();
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

    private void UpdateChromeTokenSubscription()
    {
        _chromeTokens?.Dispose();
        _chromeTokens = null;
        var radiusKeys = UseCardRadiusTokenResources ? CardRadiusKeys : null;
        if (UseSeparatorTokenResources || radiusKeys is not null)
        {
            _chromeTokens = new OwnerTokenSet(this, SeparatorResourceCount + (radiusKeys?.Length ?? 0), RefreshChromeStyle);
            if (UseSeparatorTokenResources)
            {
                _chromeTokens.SetKind(0, SharedTokenKind.ColorBorderSecondary);
                _chromeTokens.SetKind(1, SharedTokenKind.BorderThickness);
            }
            if (radiusKeys is not null)
            {
                for (var index = 0; index < radiusKeys.Length; ++index)
                {
                    _chromeTokens.SetResourceKey(SeparatorResourceCount + index, radiusKeys[index]);
                }
            }
        }
        RefreshChromeStyle();
    }

    private void RefreshChromeStyle()
    {
        var rawBrush = _chromeTokens?.GetValue(0);
        var rawThickness = _chromeTokens?.GetValue(1);
        var brush = ReferenceEquals(rawBrush, BindingOperations.DoNothing) ? SeparatorBorderBrush : rawBrush as IBrush;
        var thickness = ReferenceEquals(rawThickness, BindingOperations.DoNothing)
            ? SeparatorBorderThickness
            : rawThickness is Thickness value ? value : default;
        if (!Equals(SeparatorBorderBrush, brush) || SeparatorBorderThickness != thickness)
        {
            SeparatorBorderBrush = brush;
            SeparatorBorderThickness = thickness;
            InvalidateVisual();
        }

        var radius = UseCardRadiusTokenResources && CardRadiusKeys is not null
            ? TabCardRadiusResources.Read(_chromeTokens, SeparatorResourceCount, SizeType, CardRadius)
            : default;
        if (CardRadius != radius)
        {
            CardRadius = radius;
            OnCardRadiusChanged();
        }
    }

    /// <summary>
    /// Called right after <c>CardRadius</c> received a different corner radius. That happens when the chrome
    /// style refreshes while <c>UseCardRadiusTokenResources</c> is <see langword="true" /> and
    /// <c>CardRadiusKeys</c> is not <see langword="null" />: on a change of <c>SizeType</c>, when the token
    /// subscription is rebuilt, and when a watched radius resource changes. The base method is empty.
    /// </summary>
    private protected virtual void OnCardRadiusChanged()
    {
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

    /// <summary>
    /// Draws the separator line along the side of the tab strip that faces the content, with the separator brush
    /// and thickness read from the theme tokens. Nothing is drawn while the control has no items.
    /// </summary>
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


/// <summary>
/// Carries the data of the <see cref="BaseTabControl.Closing" /> event.
/// </summary>
public class TabClosingEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Creates the event data for a tab that is about to be closed.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event to raise.
    /// </param>
    /// <param name="tabItem">
    /// The tab that is about to be closed.
    /// </param>
    public TabClosingEventArgs(RoutedEvent routedEvent, TabItem tabItem)
        : base(routedEvent)
    {
        TabItem = tabItem;
    }
    
    /// <summary>
    /// The tab that is about to be closed.
    /// </summary>
    public TabItem TabItem { get; }
    /// <summary>
    /// Set to <see langword="true" /> to cancel the close and keep the tab. The default is
    /// <see langword="false" />.
    /// </summary>
    public bool Cancel { get; set; }
}

/// <summary>
/// Carries the data of the <see cref="BaseTabControl.Closed" /> event.
/// </summary>
public class TabClosedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Creates the event data for a tab that was closed.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event to raise.
    /// </param>
    /// <param name="tabItem">
    /// The tab that was closed.
    /// </param>
    public TabClosedEventArgs(RoutedEvent routedEvent, TabItem tabItem)
        : base(routedEvent)
    {
        TabItem = tabItem;
    }
    
    /// <summary>
    /// The tab that was closed.
    /// </summary>
    public TabItem TabItem { get; }
}
