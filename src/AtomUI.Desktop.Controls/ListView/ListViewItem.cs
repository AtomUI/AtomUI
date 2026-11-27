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
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// Token-owner transparent item container: the item theme reads the hosting list's Tokens (ListView, or the host of a derived
// list such as the Select candidate list), so Controls[ListView] also reaches the items and their transitions.
/// <summary>
/// Container for one item of a <see cref="ListView" />; also the base class of the group header of a grouped list.
/// </summary>
[TokenOwnerTransparent]
public class ListViewItem : ContentControl,
                            IListItemVirtualizingContextAware,
                            ISelectable, ITransitionInitializationOwner
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsSelected" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        ListView.IsSelectedProperty.AddOwner<ListViewItem>();
    
    /// <summary>
    /// Whether the item shown by this container is selected in the owning <see cref="ListView" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The owning list writes the value from its selection and clears it when the container is recycled.
    /// A change that the list did not write itself is applied by the list to its selection, following its selection
    /// mode, when the container is a direct child of the list.
    /// </para>
    /// </remarks>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    #endregion
    
    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="Clicked" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickedEvent =
        RoutedEvent.Register<ListViewItem, RoutedEventArgs>(
            nameof(Clicked),
            RoutingStrategies.Bubble);
    
    /// <summary>
    /// Raised when the item is clicked: on the left button press while the owning list has
    /// <see cref="ListView.ItemClickMode" /> set to <see cref="ClickMode.Press" />, or on the left button release
    /// while it is set to <see cref="ClickMode.Release" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The event bubbles and is not raised when the pointer event is already handled. The handling of a press by
    /// this container to update the selection does not count as handled here; the handling of a release does.
    /// </para>
    /// <para>
    /// The press variant requires the container to belong to a <see cref="ListView" />.
    /// </para>
    /// </remarks>
    public event EventHandler<RoutedEventArgs>? Clicked
    {
        add => AddHandler(ClickedEvent, value);
        remove => RemoveHandler(ClickedEvent, value);
    }

    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ListViewItem>();
    
    // Forwarded by the owning ListView from its terminal theme; see ListView.ItemTheme.cs.
    internal static readonly StyledProperty<IBrush?> ItemColorProperty =
        AvaloniaProperty.Register<ListViewItem, IBrush?>(nameof(ItemColor));

    internal IBrush? ItemColor
    {
        get => GetValue(ItemColorProperty);
        set => SetValue(ItemColorProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> ItemHoverColorProperty =
        AvaloniaProperty.Register<ListViewItem, IBrush?>(nameof(ItemHoverColor));

    internal IBrush? ItemHoverColor
    {
        get => GetValue(ItemHoverColorProperty);
        set => SetValue(ItemHoverColorProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> ItemSelectedColorProperty =
        AvaloniaProperty.Register<ListViewItem, IBrush?>(nameof(ItemSelectedColor));

    internal IBrush? ItemSelectedColor
    {
        get => GetValue(ItemSelectedColorProperty);
        set => SetValue(ItemSelectedColorProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> ItemSelectedIndicatorColorProperty =
        AvaloniaProperty.Register<ListViewItem, IBrush?>(nameof(ItemSelectedIndicatorColor));

    internal IBrush? ItemSelectedIndicatorColor
    {
        get => GetValue(ItemSelectedIndicatorColorProperty);
        set => SetValue(ItemSelectedIndicatorColorProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> ItemDisabledColorProperty =
        AvaloniaProperty.Register<ListViewItem, IBrush?>(nameof(ItemDisabledColor));

    internal IBrush? ItemDisabledColor
    {
        get => GetValue(ItemDisabledColorProperty);
        set => SetValue(ItemDisabledColorProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> ItemBgColorProperty =
        AvaloniaProperty.Register<ListViewItem, IBrush?>(nameof(ItemBgColor));

    internal IBrush? ItemBgColor
    {
        get => GetValue(ItemBgColorProperty);
        set => SetValue(ItemBgColorProperty, value);
    }

    internal static readonly StyledProperty<Thickness> ItemPaddingProperty =
        AvaloniaProperty.Register<ListViewItem, Thickness>(nameof(ItemPadding));

    internal Thickness ItemPadding
    {
        get => GetValue(ItemPaddingProperty);
        set => SetValue(ItemPaddingProperty, value);
    }

    internal static readonly StyledProperty<Thickness> ItemMarginProperty =
        AvaloniaProperty.Register<ListViewItem, Thickness>(nameof(ItemMargin));

    internal Thickness ItemMargin
    {
        get => GetValue(ItemMarginProperty);
        set => SetValue(ItemMarginProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> GroupHeaderColorProperty =
        AvaloniaProperty.Register<ListViewItem, IBrush?>(nameof(GroupHeaderColor));

    internal IBrush? GroupHeaderColor
    {
        get => GetValue(GroupHeaderColorProperty);
        set => SetValue(GroupHeaderColorProperty, value);
    }

    internal static readonly StyledProperty<Thickness> GroupHeaderPaddingProperty =
        AvaloniaProperty.Register<ListViewItem, Thickness>(nameof(GroupHeaderPadding));

    internal Thickness GroupHeaderPadding
    {
        get => GetValue(GroupHeaderPaddingProperty);
        set => SetValue(GroupHeaderPaddingProperty, value);
    }

    internal static readonly StyledProperty<Thickness> SelectedIndicatorMarginProperty =
        AvaloniaProperty.Register<ListViewItem, Thickness>(nameof(SelectedIndicatorMargin));

    internal Thickness SelectedIndicatorMargin
    {
        get => GetValue(SelectedIndicatorMarginProperty);
        set => SetValue(SelectedIndicatorMarginProperty, value);
    }

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

    // The default split line under the item is SplitLineWidth thick (the theme reads the host's LineWidth); a BorderThickness
    // set by the application or a semantic style replaces it as a whole.
    internal static readonly StyledProperty<double> SplitLineWidthProperty =
        AvaloniaProperty.Register<ListViewItem, double>(nameof(SplitLineWidth));

    internal double SplitLineWidth
    {
        get => GetValue(SplitLineWidthProperty);
        set => SetValue(SplitLineWidthProperty, value);
    }

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
    
    private readonly TransitionInitializationSession _transitionInitialization;

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
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    /// <summary>
    /// Completes the transition initialization session once the template is applied. The completion releases the
    /// suppression of the item's transitions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Completion takes effect only when the item is loaded and attached to the visual tree. Otherwise
    /// <see cref="OnLoaded" /> completes the session.
    /// </para>
    /// <para>
    /// An override must call the base method, otherwise the transitions stay suppressed for a template that is
    /// applied while the item is loaded.
    /// </para>
    /// </remarks>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// Resets the transition initialization session when the template is replaced, which suppresses the item's
    /// transitions again until <see cref="OnApplyTemplate" /> or <see cref="OnLoaded" /> completes it.
    /// </summary>
    /// <remarks>
    /// An override must call the base method, otherwise the item's transitions are not suppressed for the new
    /// template.
    /// </remarks>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Resets the transition initialization session when the item leaves the visual tree. This suppresses the
    /// transitions of the item again until <see cref="OnLoaded" /> completes the session after the next attach.
    /// </summary>
    /// <remarks>
    /// An override must call the base method, otherwise the transitions of the item are not suppressed when it is
    /// attached again.
    /// </remarks>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Completes the transition initialization session once the item is loaded. The completion releases the
    /// suppression of the item's transitions.
    /// </summary>
    /// <remarks>
    /// An override must call the base method, otherwise the transitions stay suppressed until the next time the
    /// template is applied.
    /// </remarks>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// Updates the selected indicator and the effective split line of the item when their inputs change.
    /// </summary>
    /// <remarks>
    /// An override must call the base method, otherwise these two states are not kept up to date.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsSelectedProperty ||
            change.Property == IsShowSelectedIndicatorProperty)
        {
            ConfigureSelectedIndicator();
        }

        if (change.Property == BorderThicknessProperty ||
            change.Property == SplitLineWidthProperty ||
            change.Property == IsGroupItemProperty ||
            change.Property == IsSplitLineVisibleProperty)
        {
            ConfigureEffectiveBorderThickness();
        }
    }

    /// <summary>
    /// Computes the selected indicator and the effective split line once the item is initialized.
    /// </summary>
    /// <remarks>
    /// An override must call the base method, otherwise both are only computed when an input changes later.
    /// </remarks>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        ConfigureSelectedIndicator();
        ConfigureEffectiveBorderThickness();

    }




    
    /// <summary>
    /// Forwards a press on the item to the selection of the owning <see cref="ListView" /> and raises
    /// <see cref="Clicked" /> on a left press while <see cref="ListView.ItemClickMode" /> is
    /// <see cref="ClickMode.Press" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A mouse press or a right pen press updates the selection immediately. The press is marked handled when the
    /// list handled that update. A touch or pen tip press only stores the press point, because its selection
    /// update happens in <see cref="OnPointerReleased" />. The press is left unhandled so that the scroll gesture
    /// recognizer works.
    /// </para>
    /// <para>
    /// Nothing happens when the press is already handled or when the container does not belong to a
    /// <see cref="ListView" />. An override must call the base method before it handles the press itself.
    /// </para>
    /// </remarks>
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

    /// <summary>
    /// Forwards a tap on the item to the selection of the owning <see cref="ListView" /> and raises
    /// <see cref="Clicked" /> on a left button release while <see cref="ListView.ItemClickMode" /> is
    /// <see cref="ClickMode.Release" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only a touch or pen tip press stores the point that this method needs, so only such a press can update the
    /// selection on release. The release must lie inside the item and within the platform tap size around the
    /// stored point. When the list handled the update, the release is marked handled and raised again on the
    /// list.
    /// </para>
    /// <para>
    /// <see cref="Clicked" /> is not raised when the release is already handled, including a release handled here
    /// to update the selection. An override must call the base method before it handles the release itself.
    /// </para>
    /// </remarks>
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

    #region 实现 ITransitionInitializationOwner 接口

    TransitionInitializationSession ITransitionInitializationOwner.TransitionInitialization => _transitionInitialization;

    #endregion

    private void ConfigureSelectedIndicator()
    {
        SetCurrentValue(IsSelectedIndicatorVisibleProperty, IsShowSelectedIndicator && IsSelected);
    }

    private void ConfigureEffectiveBorderThickness()
    {
        var thickness     = IsSet(BorderThicknessProperty) ? BorderThickness : new Thickness(0, 0, 0, SplitLineWidth);
        var showSplitLine = IsSplitLineVisible && !IsGroupItem && thickness.Bottom > 0;
        IsSplitLineEffectiveVisible = showSplitLine;
        EffectiveBorderThickness = showSplitLine ? thickness : new Thickness(0);
    }


}
