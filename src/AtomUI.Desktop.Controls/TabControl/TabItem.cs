using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
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

[PseudoClasses(StdPseudoClass.Pressed, StdPseudoClass.Selected)]
public partial class TabItem : HeaderedContentControl, ISelectable
{
    #region 公共属性定义

    public static readonly DirectProperty<TabItem, Dock?> TabStripPlacementProperty =
        AvaloniaProperty.RegisterDirect<TabItem, Dock?>(nameof(TabStripPlacement),
            o => o.TabStripPlacement);

    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<TabItem>();

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<TabItem, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<TabItem, PathIcon?>(nameof(CloseIcon));

    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<TabItem, bool>(nameof(IsClosable));

    public static readonly StyledProperty<bool> IsAutoHideCloseButtonProperty =
        AvaloniaProperty.Register<TabItem, bool>(nameof(IsAutoHideCloseButton));

    public Dock? TabStripPlacement
    {
        get => _tabStripPlacement;
        internal set => SetAndRaise(TabStripPlacementProperty, ref _tabStripPlacement, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

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

    internal static readonly StyledProperty<double> CloseButtonOpacityProperty =
        AvaloniaProperty.Register<TabItem, double>(nameof(CloseButtonOpacity));

    internal static readonly StyledProperty<bool> HasIconProperty =
        AvaloniaProperty.Register<TabItem, bool>(nameof(HasIcon));

    internal static readonly StyledProperty<bool> IsIconSlotReservedProperty =
        AvaloniaProperty.Register<TabItem, bool>(nameof(IsIconSlotReserved));

    internal static readonly DirectProperty<TabItem, Thickness> LineMaskMarginProperty =
        AvaloniaProperty.RegisterDirect<TabItem, Thickness>(
            nameof(LineMaskMargin),
            o => o.LineMaskMargin,
            (o, v) => o.LineMaskMargin = v);

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

    // Card only
    private Thickness _lineMaskMargin;

    internal Thickness LineMaskMargin
    {
        get => _lineMaskMargin;
        set => SetAndRaise(LineMaskMarginProperty, ref _lineMaskMargin, value);
    }
    #endregion

    private Dock? _tabStripPlacement;
    private IconButton? _closeButton;

    private readonly StateMotionSession _stateMotion;

    static TabItem()
    {
        SelectableMixin.Attach<TabItem>(IsSelectedProperty);
        PressedMixin.Attach<TabItem>();
        FocusableProperty.OverrideDefaultValue(typeof(TabItem), true);
        DataContextProperty.Changed.AddClassHandler<TabItem>((x, e) => x.UpdateHeader(e));
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<TabItem>(AutomationControlType.TabItem);
        AutomationProperties.IsOffscreenBehaviorProperty.OverrideDefaultValue<TabItem>(IsOffscreenBehavior.FromClip);
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
        _stateMotion.CompleteInitialization();
    }

    private void HandleCloseRequest(object? sender, RoutedEventArgs args)
    {
        if (Parent is BaseTabControl tabControl)
        {
            tabControl.CloseTab(this);
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

    private void SetupShapeThemeBindings(bool force)
    {
        if (force || Theme == null)
        {
            string? resourceKey = null;
            if (Shape == TabSharp.Line)
            {
                resourceKey = "TabItemTheme";
            }
            else
            {
                resourceKey = "CardTabItemTheme";
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

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        UpdateSelectionFromEvent(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var tabControl = ItemsControl.ItemsControlFromItemContainer(this) as BaseTabControl;
        tabControl?.NotifyTabActivationPointerPressed(this, e);
        tabControl?.NotifyTabReorderPointerPressed(this, e);

        base.OnPointerPressed(e);
        UpdateSelectionFromEvent(e);

        tabControl?.NotifyTabReorderPointerPressCompleted(this, e);
    }

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

    protected bool UpdateSelectionFromEvent(RoutedEventArgs e) =>
        SelectingItemsControl.ItemsControlFromItemContainer(this)?.UpdateSelectionFromEvent(this, e) ?? false;

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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _stateMotion.CompleteInitialization();
    }
}
