using AtomUI.Theme;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaTabStripItem = Avalonia.Controls.Primitives.TabStripItem;

/// <summary>
/// The shape of the tabs of a tab strip or tab control.
/// </summary>
public enum TabSharp
{
    /// <summary>
    /// The tab shape of <see cref="TabStrip" /> and <see cref="TabControl" />. A
    /// <see cref="TabStripItem" /> with this shape uses the <c>TabStripItemTheme</c> control theme.
    /// </summary>
    Line,
    /// <summary>
    /// The tab shape of <see cref="CardTabStrip" /> and <see cref="CardTabControl" />. A
    /// <see cref="TabStripItem" /> with this shape uses the <c>CardTabStripItemTheme</c> control theme.
    /// </summary>
    Card
}

/// <summary>
/// One tab of a tab strip or tab control. The strip creates an instance for each item of its items source, or
/// accepts a <c>TabStripItem</c> that is used directly as an item as its own container. For an item that
/// implements <see cref="ITabItemData" />, the strip copies the values of the item onto the tab. The tab is not a
/// token owner: the content it hosts keeps the token owner inherited from its host.
/// </summary>
[TokenOwnerTransparent]
public partial class TabStripItem : AvaloniaTabStripItem
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<TabStripItem, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="CloseIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<TabStripItem, PathIcon?>(nameof(CloseIcon));

    /// <summary>
    /// Identifies the <see cref="IsClosable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<TabStripItem, bool>(nameof(IsClosable));

    /// <summary>
    /// Identifies the <see cref="IsAutoHideCloseButton" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAutoHideCloseButtonProperty =
        AvaloniaProperty.Register<TabStripItem, bool>(nameof(IsAutoHideCloseButton));

    /// <summary>
    /// Identifies the <see cref="TabStripPlacement" /> property.
    /// </summary>
    public static readonly DirectProperty<TabStripItem, Dock?> TabStripPlacementProperty =
        AvaloniaProperty.RegisterDirect<TabStripItem, Dock?>(nameof(TabStripPlacement),
            o => o.TabStripPlacement);

    /// <summary>
    /// Icon shown in front of the tab label, or <see langword="null" /> to show none. For an item that implements
    /// <see cref="ITabItemData" />, the strip copies the icon of the item onto the tab unless a value is set on the
    /// tab. Setting or clearing the icon notifies the owning strip, which then decides whether the tabs without an
    /// icon keep an empty icon slot.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Icon of the close button, shown only while <see cref="IsClosable" /> is <see langword="true" />. While the
    /// tab is closable, <see langword="null" /> falls back to the default <c>CloseOutlined</c> icon. For an item
    /// that implements <see cref="ITabItemData" />, the strip copies the close icon of the item onto the tab unless
    /// a value is set on the tab.
    /// </summary>
    public PathIcon? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    /// <summary>
    /// Whether the tab shows a close button at the end of its label. Clicking the button asks the owning strip to
    /// close the tab. The default is <see langword="false" />. For an item that implements <see cref="ITabItemData" />,
    /// the strip copies the value of the item onto the tab unless a value is set on the tab.
    /// </summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    /// Whether the close button of a closable tab stays transparent until the pointer is over the tab; while
    /// transparent it still takes its place and receives clicks. It has no effect while
    /// <see cref="IsClosable" /> is <see langword="false" />. The default is <see langword="false" />, which paints
    /// the button as soon as the tab is closable. For an item that implements
    /// <see cref="ITabItemData" />, the strip copies the value of the item onto the tab unless a value is set on
    /// the tab.
    /// </summary>
    public bool IsAutoHideCloseButton
    {
        get => GetValue(IsAutoHideCloseButtonProperty);
        set => SetValue(IsAutoHideCloseButtonProperty, value);
    }

    private Dock? _tabStripPlacement;

    /// <summary>
    /// Where the tab strip that owns this tab is placed. The owning strip copies its own
    /// <see cref="BaseTabStrip.TabStripPlacement" /> onto the tab when it prepares the tab, so the setter is
    /// internal. The theme selects horizontal or vertical margins and padding from the value.
    /// </summary>
    public Dock? TabStripPlacement
    {
        get => _tabStripPlacement;
        internal set => SetAndRaise(TabStripPlacementProperty, ref _tabStripPlacement, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<TabStripItem>();

    internal static readonly StyledProperty<bool> IsTabReorderDraggingProperty =
        AvaloniaProperty.Register<TabStripItem, bool>(nameof(IsTabReorderDragging));

    internal static readonly StyledProperty<TabSharp> ShapeProperty =
        AvaloniaProperty.Register<TabStripItem, TabSharp>(nameof(Shape));


    internal static readonly StyledProperty<double> CloseButtonOpacityProperty =
        AvaloniaProperty.Register<TabStripItem, double>(nameof(CloseButtonOpacity));

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TabStripItem>();

    internal static readonly StyledProperty<bool> HasIconProperty =
        AvaloniaProperty.Register<TabStripItem, bool>(nameof(HasIcon));

    internal static readonly StyledProperty<bool> IsIconSlotReservedProperty =
        AvaloniaProperty.Register<TabStripItem, bool>(nameof(IsIconSlotReserved));

    /// <summary>
    /// The preset size of the tab; the theme derives its padding from the value. The owning strip binds the tab to
    /// its own <see cref="BaseTabStrip.SizeType" /> when it prepares the tab, so the size is set on the strip, not
    /// on the tab.
    /// </summary>
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    internal bool IsTabReorderDragging
    {
        get => GetValue(IsTabReorderDraggingProperty);
        set => SetValue(IsTabReorderDraggingProperty, value);
    }

    /// <summary>
    /// The shape of the tab, which decides the control theme of the tab. The owning strip sets it when it creates
    /// the tab. A change of a tab that is attached to a logical tree applies the theme of the new shape.
    /// </summary>
    public TabSharp Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal double CloseButtonOpacity
    {
        get => GetValue(CloseButtonOpacityProperty);
        set => SetValue(CloseButtonOpacityProperty, value);
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
        var tabStrip = ItemsControl.ItemsControlFromItemContainer(this) as BaseTabStrip ??
                       Parent as BaseTabStrip;
        tabStrip?.NotifyTabStripItemIconStateChanged();
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

    private readonly TransitionInitializationSession _transitionInitialization;

    public TabStripItem()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    /// <summary>
    /// Resets the transition initialization session, then calls the base method. An override that does not call the
    /// base method must still reset the session, so that it is completed again by <see cref="OnLoaded" />, when the
    /// tab is attached once more.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Resets the transition initialization session, then lets the base method run. An override that does not call
    /// the base method must still reset the session, so that it is completed again for the new template by
    /// <see cref="OnApplyTemplate" /> or <see cref="OnLoaded" />.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Applies the control theme of the tab's shape when the tab has no theme yet, then updates the icon state that
    /// the owning strip uses to reserve icon slots. The base method runs first.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        SetupShapeThemeBindings(false);
        ConfigureHasIcon();
        NotifyIconSlotOwner();
    }

    /// <summary>
    /// Releases the bindings that the owning strip created on the tab when the strip removes the tab directly from
    /// its logical children, so that the removed tab does not keep the strip alive. A detach that cascades from an
    /// ancestor keeps the bindings, because attaching the tree again relies on them. The base method runs first.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        // A direct detach (e.Source == this) means the tab strip dropped this item from
        // its logical children. Items that are their own container never receive
        // ClearContainerForItemOverride, so the owner-created indexer bindings must be
        // released here or the owner keeps the removed item alive for its lifetime.
        // Cascaded detach (whole tree teardown) carries an ancestor's args and must keep
        // the bindings so re-attaching the tree restores the property sync.
        if (ReferenceEquals(e.Source, this) && e.Parent is BaseTabStrip tabStrip)
        {
            tabStrip.ReleaseTabStripItemOwnerBindings(this);
        }
    }

    /// <summary>
    /// Applies the default close icon before the template is built, then releases the click handler of the previous
    /// close button, subscribes the click of the <c>PART_ItemCloseButton</c> of the new template and completes the
    /// transition initialization. An override that does not call the base method must still complete the
    /// initialization.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        SetupDefaultCloseIcon();

        base.OnApplyTemplate(e);
        ReleaseCloseButton();

        _closeButton = e.NameScope.Find<IconButton>("PART_ItemCloseButton");

        if (_closeButton is not null)
        {
            _closeButton.Click += HandleCloseRequest;
        }
        _transitionInitialization.CompleteInitialization();
    }

    private void HandleCloseRequest(object? sender, RoutedEventArgs args)
    {
        if (Parent is BaseTabStrip tabStrip)
        {
            tabStrip.CloseTab(this);
        }
    }

    /// <summary>
    /// Calls the base method, then keeps the tab in step with its properties: a shape change of a tab that is
    /// attached to a logical tree applies the theme of the new shape, an icon change updates the icon slot state,
    /// and a change of the close icon or of <see cref="IsClosable" /> applies the default close icon when the tab
    /// is closable and has no close icon.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToLogicalTree())
        {
            if (change.Property == ShapeProperty)
            {
                SetupShapeThemeBindings(true);
            }
        }

        if (change.Property == IconProperty)
        {
            ConfigureHasIcon();
        }
        else if (change.Property == CloseIconProperty)
        {
            SetupDefaultCloseIcon();
        }
        else if (change.Property == IsClosableProperty)
        {
            SetupDefaultCloseIcon();
        }
    }

    private void SetupShapeThemeBindings(bool force)
    {
        if (force || Theme == null)
        {
            string? resourceKey = null;
            if (Shape == TabSharp.Line)
            {
                resourceKey = "TabStripItemTheme";
            }
            else
            {
                resourceKey = "CardTabStripItemTheme";
            }

            // Resolve from the item itself so a theme supplied in a local resource scope wins over the application one.
            if (this.TryFindResource(resourceKey, out var resource) && resource is ControlTheme theme)
            {
                Theme = theme;
            }
        }
    }

    /// <summary>
    /// Completes the transition initialization once the tab is loaded. The base method runs first; an override that
    /// does not call it must still complete the initialization.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// Hands the press to the owning strip before the base method runs, so that the strip can prepare the activation
    /// of the tab and start a tab reorder. After the base method ran, the strip captures the pointer when a reorder
    /// started on this tab. An override that does not call the base method must still forward the press to the
    /// strip.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var tabStrip = ItemsControl.ItemsControlFromItemContainer(this) as BaseTabStrip;
        tabStrip?.NotifyTabActivationPointerPressed(this, e);
        tabStrip?.NotifyTabReorderPointerPressed(this, e);

        base.OnPointerPressed(e);

        tabStrip?.NotifyTabReorderPointerPressCompleted(this, e);
    }

    /// <summary>
    /// Lets the base method run, then the owning strip update a tab reorder of this tab. When the strip consumed the
    /// move for the reorder, the tab sets its pressed pseudo-class and marks the event as handled.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (ItemsControl.ItemsControlFromItemContainer(this) is BaseTabStrip tabStrip &&
            tabStrip.NotifyTabReorderPointerMoved(this, e))
        {
            PseudoClasses.Set(StdPseudoClass.Pressed, true);
            e.Handled = true;
        }
    }

    /// <summary>
    /// When the owning strip reports that the release ended a tab reorder of this tab, the event is marked as
    /// handled and the base method is skipped, so that this release does not select the tab. Otherwise the strip
    /// updates its selection from the release, the base method runs and the strip drops the activation pending
    /// from the press.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var tabStrip = ItemsControl.ItemsControlFromItemContainer(this) as BaseTabStrip;
        if (tabStrip is not null && tabStrip.NotifyTabReorderPointerReleased(this, e))
        {
            e.Handled = true;
            return;
        }

        tabStrip?.UpdateSelectionFromEvent(this, e);
        base.OnPointerReleased(e);
        tabStrip?.NotifyTabActivationPointerReleased(this, e);
    }

    internal void SetTabReorderDragging(bool isDragging)
    {
        IsTabReorderDragging = isDragging;
        PseudoClasses.Set(StdPseudoClass.Pressed, isDragging);
    }

}
