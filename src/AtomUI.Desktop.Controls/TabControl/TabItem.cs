using System.Reactive.Linq;
using AtomUI.Theme;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// One tab of a <see cref="TabControl" /> or a <see cref="CardTabControl" />. It shows the header, an
/// optional leading icon and an optional close button, and it reports focus and pointer input to the
/// owning tab control, which updates the selection and drives tab activation and drag reordering.
/// </summary>
[PseudoClasses(StdPseudoClass.Pressed, StdPseudoClass.Selected)]
[TokenOwnerTransparent]
public partial class TabItem : AbstractTabItem, ISelectable
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="TabStripPlacement" /> property.
    /// </summary>
    public new static readonly DirectProperty<TabItem, Dock?> TabStripPlacementProperty =
        AbstractTabItem.TabStripPlacementProperty.AddOwner<TabItem>(owner => owner.TabStripPlacement);

    /// <summary>
    /// Identifies the <see cref="IsSelected" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsSelectedProperty =
        AbstractTabItem.IsSelectedProperty.AddOwner<TabItem>();

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<TabItem, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="CloseIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<TabItem, PathIcon?>(nameof(CloseIcon));

    /// <summary>
    /// Identifies the <see cref="IsClosable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<TabItem, bool>(nameof(IsClosable));

    /// <summary>
    /// Identifies the <see cref="IsAutoHideCloseButton" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAutoHideCloseButtonProperty =
        AvaloniaProperty.Register<TabItem, bool>(nameof(IsAutoHideCloseButton));

    /// <summary>
    /// Edge of the tab strip the tab belongs to, with the contract of
    /// <see cref="AbstractTabItem.TabStripPlacement" />. The owning tab control copies its own placement here when
    /// it prepares the tab and when that placement changes; the value is <see langword="null" /> until then. The
    /// tab theme uses it to lay the tab out for a horizontal or vertical strip. The setter is internal.
    /// </summary>
    public new Dock? TabStripPlacement
    {
        get => base.TabStripPlacement;
        internal set => base.TabStripPlacement = value;
    }

    /// <summary>
    /// Whether the tab is the selected one, with the contract of <see cref="AbstractTabItem.IsSelected" />. The
    /// owning tab control maintains the value together with its own selection; <see langword="true" /> also sets
    /// the <c>:selected</c> pseudo-class.
    /// </summary>
    public new bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Icon shown at the start of the tab, before the header; <see langword="null" /> shows no icon. Setting or
    /// clearing the icon notifies the owning tab control, which then decides whether the tabs without an icon
    /// keep an empty icon slot.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Icon of the close button, which exists only while <see cref="IsClosable" /> is <see langword="true" />.
    /// While the tab is closable, <see langword="null" /> falls back to the default <c>CloseOutlined</c> icon.
    /// </summary>
    public PathIcon? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    /// <summary>
    /// Whether the tab shows a close button at the end of its header. Clicking the button asks the owning
    /// <see cref="BaseTabControl" /> to close the tab. The default is <see langword="false" />.
    /// </summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    /// Whether the close button of a closable tab stays transparent until the pointer is over the tab; while
    /// transparent it still takes its place and receives clicks. It has no effect while
    /// <see cref="IsClosable" /> is <see langword="false" />. The default is <see langword="false" />, which
    /// paints the button as soon as the tab is closable.
    /// </summary>
    public bool IsAutoHideCloseButton
    {
        get => GetValue(IsAutoHideCloseButtonProperty);
        set => SetValue(IsAutoHideCloseButtonProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="CloseButtonOpacity" /> property.
    /// </summary>
    public static readonly StyledProperty<double> CloseButtonOpacityProperty =
        AvaloniaProperty.Register<TabItem, double>(nameof(CloseButtonOpacity));

    /// <summary>
    /// Identifies the <see cref="LineMaskMargin" /> property.
    /// </summary>
    public static readonly DirectProperty<TabItem, Thickness> LineMaskMarginProperty =
        AvaloniaProperty.RegisterDirect<TabItem, Thickness>(
            nameof(LineMaskMargin),
            o => o.LineMaskMargin,
            (o, v) => o.LineMaskMargin = v);

    /// <summary>
    /// Opacity applied to the tab close button in the template, driven by <see cref="IsAutoHideCloseButton" /> and
    /// pointer-over state.
    /// </summary>
    public double CloseButtonOpacity
    {
        get => GetValue(CloseButtonOpacityProperty);
        set => SetValue(CloseButtonOpacityProperty, value);
    }

    // Card only
    private Thickness _lineMaskMargin;

    /// <summary>
    /// Margin of the border-masking strip in a card tab template that covers the shared separator line along the
    /// active tab edge.
    /// </summary>
    public Thickness LineMaskMargin
    {
        get => _lineMaskMargin;
        internal set => SetAndRaise(LineMaskMarginProperty, ref _lineMaskMargin, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<TabItem>();

    internal static readonly StyledProperty<bool> IsTabReorderDraggingProperty =
        AvaloniaProperty.Register<TabItem, bool>(nameof(IsTabReorderDragging));

    internal static readonly StyledProperty<TabSharp> ShapeProperty =
        AvaloniaProperty.Register<TabItem, TabSharp>(nameof(Shape));

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TabItem>();

    internal static readonly StyledProperty<bool> HasIconProperty =
        AvaloniaProperty.Register<TabItem, bool>(nameof(HasIcon));

    internal static readonly StyledProperty<bool> IsIconSlotReservedProperty =
        AvaloniaProperty.Register<TabItem, bool>(nameof(IsIconSlotReserved));

    internal SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    internal bool IsTabReorderDragging
    {
        get => GetValue(IsTabReorderDraggingProperty);
        set => SetValue(IsTabReorderDraggingProperty, value);
    }

    internal TabSharp Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal bool HasIcon
    {
        get => GetValue(HasIconProperty);
        set => SetValue(HasIconProperty, value);
    }

    internal bool IsIconSlotReserved
    {
        get => GetValue(IsIconSlotReservedProperty);
        set => SetValue(IsIconSlotReservedProperty, value);
    }
    #endregion

    private IconButton? _closeButton;

    private readonly TransitionInitializationSession _transitionInitialization;

    static TabItem()
    {
        PressedMixin.Attach<TabItem>();
        DataContextProperty.Changed.AddClassHandler<TabItem>((x, e) => x.UpdateHeader(e));
    }

    private void ConfigureHasIcon()
    {
        var hasIcon = Icon is not null;
        if (HasIcon == hasIcon)
        {
            return;
        }

        HasIcon = hasIcon;
        NotifyIconSlotOwner();
    }

    private void NotifyIconSlotOwner()
    {
        var tabControl = ItemsControl.ItemsControlFromItemContainer(this) as BaseTabControl ??
                         Parent as BaseTabControl;
        tabControl?.NotifyTabItemIconStateChanged();
    }

    private void SetupDefaultCloseIcon()
    {
        if (IsClosable && CloseIcon is null)
        {
            ClearValue(CloseIconProperty);
            SetValue(CloseIconProperty, new CloseOutlined(), BindingPriority.Template);
        }
    }

    private void ReleaseCloseButton()
    {
        if (_closeButton is null)
        {
            return;
        }

        _closeButton.Click -= HandleCloseRequest;
        if (_closeButton.GetVisualParent() is Panel parentPanel)
        {
            parentPanel.Children.Remove(_closeButton);
        }
        _closeButton = null;
    }


    public TabItem()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    /// <summary>
    /// Resets the transition initialization session when the tab leaves the visual tree;
    /// <see cref="OnApplyTemplate" /> and <see cref="OnLoaded" /> complete it again after a re-attach. Calls
    /// the base member after the reset; an override must call it.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Resets the transition initialization session when the template of the tab is replaced;
    /// <see cref="OnApplyTemplate" /> completes it again for the new template. Calls the base member after the
    /// reset; an override must call it.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Calls the base member, then applies the theme that matches the <c>Shape</c> of the tab unless a theme is
    /// already set, brings <c>HasIcon</c> in step with <see cref="Icon" /> and notifies the owning tab control
    /// of the icon state. An override must call the base member.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        UpdateShapeThemeBinding();
        ConfigureHasIcon();
        NotifyIconSlotOwner();
    }

    /// <summary>
    /// Calls the base member, then releases the bindings that the owning tab control created for this tab when
    /// the tab control drops the tab from its logical children. A cascaded detach of a whole tree keeps the
    /// bindings, so that re-attaching the tree restores the property sync. An override must call the base member.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _shapeThemeBinding?.Dispose();
        _shapeThemeBinding = null;
        base.OnDetachedFromLogicalTree(e);
        // A direct detach (e.Source == this) means the tab control dropped this item from
        // its logical children. Items that are their own container never receive
        // ClearContainerForItemOverride, so the owner-created indexer bindings must be
        // released here or the owner keeps the removed item alive for its lifetime.
        // Cascaded detach (whole tree teardown) carries an ancestor's args and must keep
        // the bindings so re-attaching the tree restores the property sync.
        if (ReferenceEquals(e.Source, this) && e.Parent is BaseTabControl tabControl)
        {
            tabControl.ReleaseTabItemOwnerBindings(this);
        }
    }

    /// <summary>
    /// Calls the base member, releases the close button of the previous template, subscribes to the click of the
    /// <c>PART_ItemCloseButton</c> of the new template, installs the default close icon where needed and
    /// completes the transition initialization. An override must call the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ReleaseCloseButton();

        _closeButton = e.NameScope.Find<IconButton>("PART_ItemCloseButton");

        if (_closeButton is not null)
        {
            _closeButton.Click += HandleCloseRequest;
        }

        SetupDefaultCloseIcon();
        _transitionInitialization.CompleteInitialization();
    }

    private void HandleCloseRequest(object? sender, RoutedEventArgs args)
    {
        if (Parent is BaseTabControl tabControl)
        {
            tabControl.CloseTab(this);
        }
    }

    /// <summary>
    /// Besides the base behavior, keeps the tab in step with its own state: a changed <c>Shape</c> re-applies the
    /// shape theme while the tab is attached to the logical tree, a changed <see cref="CloseIcon" /> or
    /// <see cref="IsClosable" /> re-evaluates the default close icon, a changed <see cref="Icon" /> updates
    /// <c>HasIcon</c>, and a changed <see cref="TabStripPlacement" /> updates the margin of the line mask.
    /// An override must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToLogicalTree())
        {
            if (change.Property == ShapeProperty)
            {
                UpdateShapeThemeBinding();
            }
        }

        if (change.Property == CloseIconProperty)
        {
            SetupDefaultCloseIcon();
        }
        else if (change.Property == IconProperty)
        {
            ConfigureHasIcon();
        }
        else if (change.Property == IsClosableProperty)
        {
            SetupDefaultCloseIcon();
        }
        else if (change.Property == TabStripPlacementProperty)
        {
            HandleTabStripPlacementChanged();
        }
    }

    private IDisposable? _shapeThemeBinding;

    private void UpdateShapeThemeBinding()
    {
        _shapeThemeBinding?.Dispose();
        // A ControlTheme cannot choose its own Theme. Keep this named shape fallback at Style priority,
        // scoped by Avalonia's resource observable so that an application Theme or binding remains effective.
        var key = Shape == TabSharp.Line ? "TabItemTheme" : "CardTabItemTheme";
        _shapeThemeBinding = this.Bind(ThemeProperty,
            this.GetResourceObservable(key).Select(static value => value is ControlTheme theme
                ? new BindingValue<ControlTheme?>(theme)
                : BindingValue<ControlTheme?>.Unset), BindingPriority.Style);
    }

    /// <summary>
    /// Calls the base member, then lets the owning tab control update its selection from the focus event. An
    /// override must call the base member.
    /// </summary>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        UpdateSelectionFromEvent(e);
    }

    /// <summary>
    /// Tells the owning tab control that a tab activation and a tab reorder press started on this tab, calls the
    /// base member, lets the tab control update its selection from the event and finally reports that the reorder
    /// press completed. An override must call the base member.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var tabControl = ItemsControl.ItemsControlFromItemContainer(this) as BaseTabControl;
        tabControl?.NotifyTabActivationPointerPressed(this, e);
        tabControl?.NotifyTabReorderPointerPressed(this, e);

        base.OnPointerPressed(e);
        UpdateSelectionFromEvent(e);

        tabControl?.NotifyTabReorderPointerPressCompleted(this, e);
    }

    /// <summary>
    /// Calls the base member, then hands the move to the reorder tracking of the owning tab control. When that
    /// tracking reports that it consumed the move for reordering, sets the <c>:pressed</c> pseudo-class and
    /// marks the event handled. An override must call the base member.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (ItemsControl.ItemsControlFromItemContainer(this) is BaseTabControl tabControl &&
            tabControl.NotifyTabReorderPointerMoved(this, e))
        {
            PseudoClasses.Set(StdPseudoClass.Pressed, true);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Hands the release to the reorder tracking of the owning tab control first and, when that tracking reports
    /// that it consumed the release for reordering, marks the event handled and stops there. Otherwise lets the
    /// tab control update its selection from the event, calls the base member and reports the end of the tab
    /// activation. An override must call the base member.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var tabControl = ItemsControl.ItemsControlFromItemContainer(this) as BaseTabControl;
        if (tabControl is not null && tabControl.NotifyTabReorderPointerReleased(this, e))
        {
            e.Handled = true;
            return;
        }

        UpdateSelectionFromEvent(e);
        base.OnPointerReleased(e);
        tabControl?.NotifyTabActivationPointerReleased(this, e);
    }

    /// <summary>
    /// Forwards an input event that should select this tab to the owning selecting items control.
    /// </summary>
    /// <param name="e">
    /// The routed event that targets this tab.
    /// </param>
    /// <returns>
    /// The value returned by the owning control, or <see langword="false" /> when the tab has no owning selecting
    /// items control.
    /// </returns>
    protected bool UpdateSelectionFromEvent(RoutedEventArgs e) =>
        SelectingItemsControl.ItemsControlFromItemContainer(this)?.UpdateSelectionFromEvent(this, e) ?? false;

    /// <summary>
    /// Called when the access key of the tab is invoked. Focuses the tab, selects it with <c>SetCurrentValue</c>
    /// so that a binding on <see cref="IsSelected" /> is kept, and marks the event handled. This member does not
    /// call the base member.
    /// </summary>
    protected override void OnAccessKey(RoutedEventArgs e)
    {
        Focus();
        SetCurrentValue(IsSelectedProperty, true);
        e.Handled = true;
    }

    internal void SetTabReorderDragging(bool isDragging)
    {
        IsTabReorderDragging = isDragging;
        PseudoClasses.Set(StdPseudoClass.Pressed, isDragging);
    }

    private void UpdateHeader(AvaloniaPropertyChangedEventArgs obj)
    {
        if (Header == null)
        {
            if (obj.NewValue is IHeadered headered)
            {
                if (Header != headered.Header)
                {
                    SetCurrentValue(HeaderProperty, headered.Header);
                }
            }
            else
            {
                if (!(obj.NewValue is Control))
                {
                    SetCurrentValue(HeaderProperty, obj.NewValue);
                }
            }
        }
        else
        {
            if (Header == obj.OldValue)
            {
                SetCurrentValue(HeaderProperty, obj.NewValue);
            }
        }
    }

    private void HandleTabStripPlacementChanged()
    {
        if (TabStripPlacement == Dock.Top || TabStripPlacement == Dock.Bottom)
        {
            SetCurrentValue(LineMaskMarginProperty, new Thickness(1, 0, 1, 0));
        }
        else
        {
            SetCurrentValue(LineMaskMarginProperty, new Thickness(0, 1, 0, 1));
        }
    }

    /// <summary>
    /// Calls the base member, then completes the transition initialization of the tab. An override must call the
    /// base member.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }
}
