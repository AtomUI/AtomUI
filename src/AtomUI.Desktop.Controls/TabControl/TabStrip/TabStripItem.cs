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

public enum TabSharp
{
    Line,
    Card
}

public partial class TabStripItem : AvaloniaTabStripItem
{
    #region 公共属性定义

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<TabStripItem, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<TabStripItem, PathIcon?>(nameof(CloseIcon));

    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<TabStripItem, bool>(nameof(IsClosable));

    public static readonly StyledProperty<bool> IsAutoHideCloseButtonProperty =
        AvaloniaProperty.Register<TabStripItem, bool>(nameof(IsAutoHideCloseButton));

    public static readonly DirectProperty<TabStripItem, Dock?> TabStripPlacementProperty =
        AvaloniaProperty.RegisterDirect<TabStripItem, Dock?>(nameof(TabStripPlacement),
            o => o.TabStripPlacement);

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public PathIcon? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    public bool IsAutoHideCloseButton
    {
        get => GetValue(IsAutoHideCloseButtonProperty);
        set => SetValue(IsAutoHideCloseButtonProperty, value);
    }

    private Dock? _tabStripPlacement;

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

    private readonly StateMotionSession _stateMotion;

    public TabStripItem()
    {
        _stateMotion = new StateMotionSession(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _stateMotion.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _stateMotion?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        SetupShapeThemeBindings(false);
        ConfigureHasIcon();
        NotifyIconSlotOwner();
    }

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
        _stateMotion.CompleteInitialization();
    }

    private void HandleCloseRequest(object? sender, RoutedEventArgs args)
    {
        if (Parent is BaseTabStrip tabStrip)
        {
            tabStrip.CloseTab(this);
        }
    }

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

            if (Application.Current != null)
            {
                if (Application.Current.TryFindResource(resourceKey, out var resource))
                {
                    if (resource is ControlTheme theme)
                    {
                        Theme = theme;
                    }
                }
            }
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _stateMotion.CompleteInitialization();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var tabStrip = ItemsControl.ItemsControlFromItemContainer(this) as BaseTabStrip;
        tabStrip?.NotifyTabActivationPointerPressed(this, e);
        tabStrip?.NotifyTabReorderPointerPressed(this, e);

        base.OnPointerPressed(e);

        tabStrip?.NotifyTabReorderPointerPressCompleted(this, e);
    }

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
