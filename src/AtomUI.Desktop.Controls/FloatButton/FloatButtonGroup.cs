using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Controls.Primitives;
using AtomUI.Controls.Utils;
using AtomUI.Icons.AntDesign;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using ControlList = Avalonia.Controls.Controls;

/// <summary>
/// The side of the trigger button of a float button group at which the menu of items opens.
/// </summary>
public enum FloatButtonGroupMenuPlacement
{
    /// <summary>
    /// The menu opens above the trigger button, which stacks the items vertically. This is the value in effect when
    /// the menu placement is unset.
    /// </summary>
    Top,
    /// <summary>
    /// The menu opens below the trigger button, which stacks the items vertically.
    /// </summary>
    Bottom,
    /// <summary>
    /// The menu opens to the left of the trigger button, which lays the items out horizontally.
    /// </summary>
    Left,
    /// <summary>
    /// The menu opens to the right of the trigger button, which lays the items out horizontally.
    /// </summary>
    Right
}

/// <summary>
/// How a float button group presents its items: as a plain list, or as a menu that a trigger button opens.
/// </summary>
public enum FloatButtonGroupTrigger
{
    /// <summary>
    /// The group is a plain list of its items and has neither a trigger button nor a menu. This is the value in
    /// effect when the trigger is unset.
    /// </summary>
    Default,
    /// <summary>
    /// A trigger button toggles the menu when it is clicked or activated with Enter or Space, and a click outside
    /// the group closes the menu.
    /// </summary>
    Click,
    /// <summary>
    /// The menu is shown while the pointer is over the group, trigger and menu included.
    /// </summary>
    Hover
}

/// <summary>
/// A group of float buttons. With <see cref="Trigger" /> set to <see cref="FloatButtonGroupTrigger.Default" />,
/// the group is a plain list of the buttons in <see cref="Children" />; otherwise a trigger button opens and
/// closes them as a menu. When its parent resolves to a scoped overlay layer, the group positions itself in that
/// layer from <see cref="Placement" /> and the float offsets.
/// </summary>
public partial class FloatButtonGroup : TemplatedControl, IMotionAwareControl
{
    private static readonly CubicEaseOut DefaultShowMotionEasing = new();
    private static readonly CubicEaseIn DefaultHideMotionEasing = new();

    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonPlacement> PlacementProperty =
        AbstractFloatButton.PlacementProperty.AddOwner<FloatButtonGroup>();
    
    /// <summary>
    /// Identifies the <see cref="FloatOffsetX" /> property.
    /// </summary>
    public static readonly StyledProperty<double> FloatOffsetXProperty =
        AbstractFloatButton.FloatOffsetXProperty.AddOwner<FloatButtonGroup>();
    
    /// <summary>
    /// Identifies the <see cref="FloatOffsetY" /> property.
    /// </summary>
    public static readonly StyledProperty<double> FloatOffsetYProperty =
        AbstractFloatButton.FloatOffsetYProperty.AddOwner<FloatButtonGroup>();
    
    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<FloatButtonGroup, PathIcon?>(nameof(Icon));
    
    /// <summary>
    /// Identifies the <see cref="CloseIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<FloatButtonGroup, PathIcon?>(nameof(CloseIcon));
    
    /// <summary>
    /// Identifies the <see cref="ButtonType" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonType> ButtonTypeProperty =
        AbstractFloatButton.ButtonTypeProperty.AddOwner<FloatButtonGroup>();

    /// <summary>
    /// Identifies the <see cref="Shape" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonShape> ShapeProperty =
        AbstractFloatButton.ShapeProperty.AddOwner<FloatButtonGroup>();
    
    /// <summary>
    /// Identifies the <see cref="BoxShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<FloatButtonGroup>();
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FloatButtonGroup>();
    
    /// <summary>
    /// Identifies the <see cref="MenuPlacement" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonGroupMenuPlacement> MenuPlacementProperty =
        AvaloniaProperty.Register<FloatButtonGroup, FloatButtonGroupMenuPlacement>(nameof(MenuPlacement), FloatButtonGroupMenuPlacement.Top);
    
    /// <summary>
    /// Identifies the <see cref="Trigger" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonGroupTrigger> TriggerProperty =
        AvaloniaProperty.Register<FloatButtonGroup, FloatButtonGroupTrigger>(nameof(Trigger), FloatButtonGroupTrigger.Default);
    
    /// <summary>
    /// Identifies the <see cref="IsOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<FloatButtonGroup, bool>(
            nameof(IsOpen),
            defaultBindingMode: BindingMode.TwoWay);
    
    /// <summary>
    /// Identifies the <see cref="MenuMotionDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> MenuMotionDurationProperty =
        AvaloniaProperty.Register<FloatButtonGroup, TimeSpan>(nameof(MenuMotionDuration));
    
    /// <summary>
    /// Gets or sets the edge or corner of the scoped overlay layer that the group is anchored to. It applies when
    /// the parent of the group resolves to such a layer, as inside a floating surface host. Unset means
    /// <see cref="FloatButtonPlacement.Top" />.
    /// </summary>
    public FloatButtonPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the horizontal offset, in device-independent pixels, of the group from the position chosen by
    /// <see cref="Placement" />. A positive value moves the group right, or left when the placement is <c>Right</c>,
    /// <c>TopRight</c> or <c>BottomRight</c>; a negative value moves it the other way. Unset means <c>0</c>,
    /// which is no offset.
    /// </summary>
    public double FloatOffsetX
    {
        get => GetValue(FloatOffsetXProperty);
        set => SetValue(FloatOffsetXProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical offset, in device-independent pixels, of the group from the position chosen by
    /// <see cref="Placement" />. A positive value moves the group down, or up when the placement is <c>Bottom</c>,
    /// <c>BottomLeft</c> or <c>BottomRight</c>; a negative value moves it the other way. Unset means <c>0</c>,
    /// which is no offset.
    /// </summary>
    public double FloatOffsetY
    {
        get => GetValue(FloatOffsetYProperty);
        set => SetValue(FloatOffsetYProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the icon of the trigger button. It applies when <see cref="Trigger" /> is <c>Click</c> or
    /// <c>Hover</c>; while the menu is open the trigger shows <see cref="CloseIcon" /> instead. When no icon is set
    /// when the group is initialized, a default file icon is substituted. Setting it to <see langword="null" />
    /// afterwards leaves the trigger without an icon.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the icon that the trigger button shows while the menu is open. When no icon is set when the
    /// group is initialized, a default close icon is substituted.
    /// </summary>
    public PathIcon? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the visual type of the trigger button, which selects its icon color and background in the
    /// normal, hovered and pressed states. Unset means <see cref="FloatButtonType.Default" />.
    /// </summary>
    public FloatButtonType ButtonType
    {
        get => GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the shape of the group. The trigger button and the list of items take this value, and each
    /// float button added to <see cref="Children" /> is bound to it. Unset means
    /// <see cref="FloatButtonShape.Circle" />.
    /// </summary>
    public FloatButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the shadow drawn under the list of items. Unset means no shadow, except for a square group,
    /// whose theme sets it from the <c>FloatButtonShadows</c> Token of the floating button.
    /// </summary>
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the menu of the group animates when it opens and closes. Each float button added to
    /// <see cref="Children" /> is bound to this value as well. When unset, the theme sets it from the
    /// <c>EnableMotion</c> Token of the float button group.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the side of the trigger button at which the menu of the group opens. It applies when
    /// <see cref="Trigger" /> is <c>Click</c> or <c>Hover</c>. <c>Top</c> and <c>Bottom</c> stack the items
    /// vertically; <c>Left</c> and <c>Right</c> lay them out horizontally. Unset means
    /// <see cref="FloatButtonGroupMenuPlacement.Top" />.
    /// </summary>
    public FloatButtonGroupMenuPlacement MenuPlacement
    {
        get => GetValue(MenuPlacementProperty);
        set => SetValue(MenuPlacementProperty, value);
    }
    
    /// <summary>
    /// Gets or sets how the items of the group are shown. With <see cref="FloatButtonGroupTrigger.Default" /> the
    /// group is a plain list of its items. With <c>Click</c> a trigger button toggles the menu when it is clicked
    /// or activated with Enter or Space, and a click outside the group closes the menu; with <c>Hover</c> the menu
    /// is shown while the pointer is over the group. Unset means <see cref="FloatButtonGroupTrigger.Default" />.
    /// </summary>
    public FloatButtonGroupTrigger Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the menu of the group is open. It applies when <see cref="Trigger" /> is <c>Click</c>
    /// or <c>Hover</c>: <see langword="true" /> shows the menu and <see langword="false" /> hides it. The property
    /// binds two-way by default, and interactions change the value without replacing a binding; a disabled group
    /// ignores their requests. Unset means <see langword="false" />.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the duration of the animation of the menu of the group when it opens and closes. It applies
    /// when <see cref="Trigger" /> is <c>Click</c> or <c>Hover</c> and <see cref="IsMotionEnabled" /> is
    /// <see langword="true" />. When unset, the theme sets it from the <c>MotionDurationMid</c> Token of the float
    /// button group.
    /// </summary>
    public TimeSpan MenuMotionDuration
    {
        get => GetValue(MenuMotionDurationProperty);
        set => SetValue(MenuMotionDurationProperty, value);
    }

    /// <summary>
    /// Gets the items of the group. Once the template is applied, changes of this collection are forwarded to the
    /// list of items in the template. Each float button added to the collection follows the <see cref="Shape" />
    /// and <see cref="IsMotionEnabled" /> of the group. A reset of the collection is not supported once the template
    /// is applied and throws <see cref="NotSupportedException" />.
    /// </summary>
    [Content] 
    public ControlList Children { get; } = new ();
    
    #endregion

    #region 公共事件定义
    /// <summary>
    /// Identifies the <see cref="Clicked" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickedEvent =
        RoutedEvent.Register<FloatButtonGroup, RoutedEventArgs>(nameof(Clicked), RoutingStrategies.Bubble);
    
    /// <summary>
    /// Identifies the <see cref="Closed" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<FloatButtonGroup, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);
    
    /// <summary>
    /// Identifies the <see cref="Opened" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> OpenedEvent =
        RoutedEvent.Register<FloatButtonGroup, RoutedEventArgs>(nameof(Opened), RoutingStrategies.Bubble);
    
    /// <summary>
    /// Raised when the trigger button of the group is clicked, so only when <see cref="Trigger" /> is <c>Click</c>
    /// or <c>Hover</c>. The menu is toggled by the pointer input itself rather than by this event, so handling the
    /// event does not keep the menu from opening or closing.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Clicked
    {
        add => AddHandler(ClickedEvent, value);
        remove => RemoveHandler(ClickedEvent, value);
    }
    
    /// <summary>
    /// Raised after <see cref="IsOpen" /> changes to <see langword="false" /> and the menu has been updated to match.
    /// The change is already effective when the event is raised, and it cannot be canceled.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }
    
    /// <summary>
    /// Raised after <see cref="IsOpen" /> changes to <see langword="true" /> and the menu has been updated to match.
    /// The change is already effective when the event is raised, and it cannot be canceled.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    #endregion

    private FloatButtonItemsControl? _itemsControl;
    private bool _initPressed;
    private bool _wasOpenOnPointerPressed;
    private IDisposable? _clickTriggerDisposable;
    private FloatButton? _triggerButton;
    private FloatButton? _clickTriggerKeyboardButton;
    ScopeAwareOverlayLayer? _overlayLayer;
    private Border? _menuSurface;
    private bool _isAttachedToVisualTree;
    
    static FloatButtonGroup()
    {
        AffectsMeasure<FloatButtonGroup>(IsOpenProperty);
    }

    public FloatButtonGroup()
    {
        Children.CollectionChanged += NotifyChildrenChanged;
    }

    /// <summary>
    /// Called when the group is initialized. It substitutes a default file icon for <see cref="Icon" /> and a default
    /// close icon for <see cref="CloseIcon" /> when they are still <see langword="null" />. Call the base member first.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Icon == null)
        {
            SetCurrentValue(IconProperty, new FileTextOutlined());
        }

        if (CloseIcon == null)
        {
            SetCurrentValue(CloseIconProperty, new CloseOutlined());
        }
    }

    /// <summary>
    /// Called when the value of a property of the group changes. It sets up the input handling when
    /// <see cref="Trigger" /> changes, recalculates the position in the scoped overlay layer when
    /// <see cref="Placement" />, a float offset or the parent changes, and recalculates the position of the menu
    /// when <see cref="Trigger" /> or <see cref="MenuPlacement" /> changes. A change of <see cref="IsOpen" /> shows
    /// or hides the menu and then raises <see cref="Opened" /> or <see cref="Closed" />. Call the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TriggerProperty)
        {
            ConfigureTriggerType();
        }
        else if (change.Property == ParentProperty)
        {
            SetupParentLayer(Parent);
            if (_overlayLayer != null)
            {
                AbstractFloatButton.CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
            }
        }
        else if (change.Property == FloatOffsetXProperty ||
                 change.Property == FloatOffsetYProperty ||
                 change.Property == PlacementProperty)
        {
            if (_overlayLayer != null)
            {
                AbstractFloatButton.CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
            }
        }
        else if (change.Property == IsOpenProperty)
        {
            CalculateItemsControlPosition();
            UpdateMenuMotion();
            RaiseEvent(new RoutedEventArgs(change.GetNewValue<bool>() ? OpenedEvent : ClosedEvent));
        }
        if (change.Property == TriggerProperty ||
            change.Property == MenuPlacementProperty)
        {
            CalculateItemsControlPosition();
        }
    }

    /// <summary>
    /// Called when the template of the group is applied. It looks up the list of items, the trigger button and the
    /// menu surface in the new template, empties the list of the previous template, moves <see cref="Children" />
    /// into the new list and sets up the trigger input. Call the base member first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        DetachItemsControlChildren();
        if (_triggerButton != null)
        {
            _triggerButton.Click -= HandleTriggerButtonClick;
        }
        _itemsControl  = e.NameScope.Find<FloatButtonItemsControl>("ItemsControl");
        _triggerButton = e.NameScope.Find<FloatButton>("Trigger");
        _menuSurface   = e.NameScope.Find<Border>("PART_MenuSurface");
        if (_triggerButton != null)
        {
            _triggerButton.Click += HandleTriggerButtonClick;
        }

        if (_menuSurface != null)
        {
            UpdateMenuMotion(initialize: true);
        }
        
        if (_itemsControl != null)
        {
            _itemsControl.IsTriggerMode = Trigger != FloatButtonGroupTrigger.Default;
        }
        
        _itemsControl?.Children.AddRange(Children);
        ConfigureTriggerType();
        foreach (var item in Children)
        {
            if (item is AbstractFloatButton floatButton)
            {
                NotifyAddItem(floatButton);
            }
        }
    }

    /// <summary>
    /// Called when <see cref="Children" /> changes. Once the template has supplied the list of items, it mirrors
    /// the change on that list: a float button that is added follows the <see cref="Shape" /> and
    /// <see cref="IsMotionEnabled" /> of the group, one that is removed leaves embedded mode, and a reset of the
    /// collection throws <see cref="NotSupportedException" />. An override must call the base member to keep both
    /// lists in step.
    /// </summary>
    protected virtual void NotifyChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_itemsControl != null)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newChildren = ControlCollectionChangedUtils.CollectControls(e.NewItems!);
                    if (newChildren is not null)
                    {
                        foreach (var child in newChildren)
                        {
                            if (child is AbstractFloatButton floatButton)
                            {
                                NotifyAddItem(floatButton);
                            }
                        }

                        _itemsControl.Children.InsertRange(e.NewStartingIndex, newChildren);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    _itemsControl.Children.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems!.Count; ++i)
                    {
                        if (e.OldItems[i] is AbstractFloatButton floatButton)
                        {
                            floatButton.SetCurrentValue(AbstractFloatButton.IsEmbedModeProperty, false);
                        }
                    }
                    _itemsControl.Children.RemoveRange(e.OldStartingIndex, e.OldItems!.Count);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (var i = 0; i < e.OldItems!.Count; ++i)
                    {
                        var index = i + e.OldStartingIndex;
                        var child = (Control)e.NewItems![i]!;
                        _itemsControl.Children[index] = child;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }
        }
    }

    /// <summary>
    /// Makes a float button an item of the group: its shape and motion setting are bound to <see cref="Shape" />
    /// and <see cref="IsMotionEnabled" />, and it is switched to embedded mode, so that it is laid out by the list
    /// of items instead of positioning itself in an overlay layer.
    /// </summary>
    protected void NotifyAddItem(AbstractFloatButton floatButton)
    {
        floatButton[!AbstractFloatButton.ShapeProperty]           = this[!ShapeProperty];
        floatButton[!AbstractFloatButton.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        floatButton.SetCurrentValue(AbstractFloatButton.IsEmbedModeProperty, true);
    }

    /// <summary>
    /// Called when the pointer enters the group, menu included. With <see cref="Trigger" /> set to <c>Hover</c> it
    /// requests the menu to open. Call the base member first.
    /// </summary>
    // Hover is tracked on the whole group rather than the trigger button: the menu
    // surface lives in the group's visual tree, so moving from the trigger onto the
    // menu keeps the group hovered and the menu stays reachable.
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (Trigger == FloatButtonGroupTrigger.Hover)
        {
            RequestOpenStateChange(true);
        }
    }
    
    /// <summary>
    /// Called when the pointer leaves the group. With <see cref="Trigger" /> set to <c>Hover</c> it requests the menu
    /// to close. Call the base member first.
    /// </summary>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (Trigger == FloatButtonGroupTrigger.Hover)
        {
            RequestOpenStateChange(false);
        }
    }

    // Interactions write IsOpen with current-value semantics so a TwoWay binding (the
    // equivalent of Ant Design's open + onOpenChange) receives the new state, while a
    // disabled group ignores open requests like Ant Design's triggerOpen guard. The
    // raw click-trigger input subscription bypasses Avalonia's disabled routing, so
    // the guard must live here rather than rely on the trigger button being disabled.
    private void RequestOpenStateChange(bool isOpen)
    {
        if (!IsEffectivelyEnabled || IsOpen == isOpen)
        {
            return;
        }

        SetCurrentValue(IsOpenProperty, isOpen);
    }

    private void HandleTriggerButtonClick(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ClickedEvent));
    }

    /// <summary>
    /// Called when the group is attached to the visual tree. It marks the group as attached and sets up the input
    /// handling that belongs to <see cref="Trigger" />. Call the base member first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttachedToVisualTree = true;
        ConfigureTriggerType();
    }

    /// <summary>
    /// Called when the group is detached from the visual tree. It disposes the input handling of
    /// <see cref="Trigger" />, which is set up again when the group is attached again. Call the base member first.
    /// which is set up again when the group is attached again. Call the base member first.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isAttachedToVisualTree = false;
        DisposeClickTrigger();

    }

    private void ConfigureTriggerType()
    {
        DisposeClickTrigger();
        if (_isAttachedToVisualTree && Trigger == FloatButtonGroupTrigger.Click)
        {
            var inputManager = AvaloniaLocator.Current.GetService(typeof(IInputManager)) as IInputManager;
            _clickTriggerDisposable = inputManager?.Process.Subscribe(HandleMouseClick);
            AttachClickTriggerKeyboardHandler();
        }

        if (_itemsControl != null)
        {
            _itemsControl.IsTriggerMode = Trigger != FloatButtonGroupTrigger.Default;
        }
    }

    private void DisposeClickTrigger()
    {
        _clickTriggerDisposable?.Dispose();
        _clickTriggerDisposable = null;
        DetachClickTriggerKeyboardHandler();
    }

    private void AttachClickTriggerKeyboardHandler()
    {
        if (_triggerButton is null || ReferenceEquals(_clickTriggerKeyboardButton, _triggerButton))
        {
            return;
        }

        DetachClickTriggerKeyboardHandler();
        _triggerButton.AddHandler(KeyDownEvent, HandleClickTriggerKeyDown, handledEventsToo: true);
        _clickTriggerKeyboardButton = _triggerButton;
    }

    private void DetachClickTriggerKeyboardHandler()
    {
        if (_clickTriggerKeyboardButton is null)
        {
            return;
        }

        _clickTriggerKeyboardButton.RemoveHandler(KeyDownEvent, HandleClickTriggerKeyDown);
        _clickTriggerKeyboardButton = null;
    }

    private void DetachItemsControlChildren()
    {
        if (_itemsControl is { Children.Count: > 0 } itemsControl)
        {
            itemsControl.Children.RemoveRange(0, itemsControl.Children.Count);
        }
    }

    private void HandleMouseClick(RawInputEventArgs args)
    {
        if (args is RawPointerEventArgs pointerEventArgs)
        {
            if (pointerEventArgs.Type == RawPointerEventType.LeftButtonDown)
            {
                _wasOpenOnPointerPressed = IsOpen;
                _initPressed             = pointerEventArgs.IsPointLogicalIn(this);
            }
            else if (pointerEventArgs.Type == RawPointerEventType.LeftButtonUp)
            {
                if (pointerEventArgs.IsPointLogicalIn(this) || pointerEventArgs.IsPointLogicalIn(_itemsControl))
                {
                    if (_initPressed && pointerEventArgs.IsPointLogicalIn(_triggerButton))
                    {
                        RequestOpenStateChange(!IsOpen);
                    }
                }
                else
                {
                    if (_wasOpenOnPointerPressed)
                    {
                        RequestOpenStateChange(false);
                    }
                }
                _initPressed = false;
                _wasOpenOnPointerPressed = false;
            }
        }
    }

    private void HandleClickTriggerKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Space) ||
            Trigger != FloatButtonGroupTrigger.Click ||
            !IsTriggerButtonKeyboardFocused())
        {
            return;
        }

        RequestOpenStateChange(!IsOpen);
        e.Handled = true;
    }

    private bool IsTriggerButtonKeyboardFocused()
    {
        if (_triggerButton is null)
        {
            return false;
        }

        var topLevel = TopLevel.GetTopLevel(_triggerButton);
        return ReferenceEquals(topLevel?.FocusManager?.GetFocusedElement(), _triggerButton);
    }
    
    /// <summary>
    /// Called when the size of the group changes. It sets the corner radius of a circular group to half of its new
    /// height and recalculates the position in the scoped overlay layer. Call the base member first.
    /// </summary>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (Shape == FloatButtonShape.Circle)
        {
            SetValue(CornerRadiusProperty, new CornerRadius(e.NewSize.Height / 2), BindingPriority.Template);
        }

        if (_overlayLayer != null)
        {
            AbstractFloatButton.CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
        }
    }
    
    private void SetupParentLayer(StyledElement? parent)
    {
        if (_overlayLayer != null)
        {
            _overlayLayer.SizeChanged -= HandleLayerSizeChanged;
        }
        if (FloatingSurfaceHost.ResolveLayer(parent) is { } scopeAwareOverlayLayer)
        {
            _overlayLayer                      =  scopeAwareOverlayLayer;
            scopeAwareOverlayLayer.SizeChanged += HandleLayerSizeChanged;
        }
        else
        {
            _overlayLayer = null;
        }
    }

    private void HandleLayerSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        AbstractFloatButton.CalculatePosition(this, e.NewSize, Placement, FloatOffsetX, FloatOffsetY);
    }

    /// <summary>
    /// Measures the group so that the menu overlays the content that follows it instead of taking up layout space.
    /// Call the base member first.
    /// </summary>
    /// <returns>
    /// With <see cref="Trigger" /> set to <c>Click</c> or <c>Hover</c>, the desired size of the trigger button, or the
    /// measured size of the whole group while the trigger button has not been found; otherwise the measured size.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Trigger == FloatButtonGroupTrigger.Default)
        {
            return base.MeasureOverride(availableSize);
        }
        var size = base.MeasureOverride(availableSize);
        return _triggerButton?.DesiredSize ?? size; 
    }

    private void CalculateItemsControlPosition()
    {
        if (Trigger == FloatButtonGroupTrigger.Default)
        {
            return;
        }
        if (!IsOpen)
        {
            return;
        }
        if (_menuSurface == null || _triggerButton == null)
        {
            return;
        }

        var  offsetX         = 0.0d;
        var  offsetY         = 0.0d;
        var  width           = DesiredSize.Width;
        var  height          = DesiredSize.Height;
        var  originVisible   = _menuSurface.IsVisible;
        Size motionActorSize = default;
        try
        {
            _menuSurface.SetCurrentValue(IsVisibleProperty, true);
            using var preparation = _menuSurface.SetValue(OpacityProperty, 0.0d, BindingPriority.Animation);
            LayoutHelper.MeasureChild(_menuSurface, new Size(double.PositiveInfinity, double.PositiveInfinity),
                new Thickness(0));
            motionActorSize = _menuSurface.DesiredSize;
        }
        finally
        {
            _menuSurface.SetCurrentValue(IsVisibleProperty, originVisible);
        }
     
        if (MenuPlacement == FloatButtonGroupMenuPlacement.Top)
        {
            offsetY = -motionActorSize.Height;
        }
        else if (MenuPlacement == FloatButtonGroupMenuPlacement.Bottom)
        {
            offsetY = height;
        }
        else if (MenuPlacement == FloatButtonGroupMenuPlacement.Left)
        {
            offsetX = -motionActorSize.Width;
        }
        else if (MenuPlacement == FloatButtonGroupMenuPlacement.Right)
        {
            offsetX = width;
        }
        Canvas.SetLeft(_menuSurface, offsetX);
        Canvas.SetTop(_menuSurface, offsetY);
    }

    private void UpdateMenuMotion(bool initialize = false)
    {
        if (_menuSurface is not { } surface || Trigger == FloatButtonGroupTrigger.Default)
        {
            return;
        }

        // The offset is the trigger's current DIP size, rather than the animated
        // menu's size. Motion definitions are plain snapshot inputs, so this
        // runtime value cannot be expressed as a binding inside the definition.
        var vertical = MenuPlacement is FloatButtonGroupMenuPlacement.Top or FloatButtonGroupMenuPlacement.Bottom;
        var offset = vertical ? DesiredSize.Height : DesiredSize.Width;
        AbstractMotion enter = MenuPlacement switch
        {
            FloatButtonGroupMenuPlacement.Top => new MoveDownInMotion(offset, easing: DefaultShowMotionEasing),
            FloatButtonGroupMenuPlacement.Bottom => new MoveUpInMotion(offset, easing: DefaultShowMotionEasing),
            FloatButtonGroupMenuPlacement.Left => new MoveRightInMotion(offset, easing: DefaultShowMotionEasing),
            _ => new MoveLeftInMotion(offset, easing: DefaultShowMotionEasing)
        };
        AbstractMotion leave = MenuPlacement switch
        {
            FloatButtonGroupMenuPlacement.Top => new MoveDownOutMotion(offset, easing: DefaultHideMotionEasing),
            FloatButtonGroupMenuPlacement.Bottom => new MoveUpOutMotion(offset, easing: DefaultHideMotionEasing),
            FloatButtonGroupMenuPlacement.Left => new MoveRightOutMotion(offset, easing: DefaultHideMotionEasing),
            _ => new MoveLeftOutMotion(offset, easing: DefaultHideMotionEasing)
        };
        Motion.SetEnter(surface, enter);
        Motion.SetLeave(surface, leave);
        Motion.SetIsVisible(surface, IsOpen);
        if (initialize)
        {
            Motion.SnapVisibility(surface);
        }
    }

    /// <summary>
    /// Called when the group is loaded. It positions the menu for the current <see cref="MenuPlacement" /> and
    /// initializes the motion of the menu. Call the base member first.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        CalculateItemsControlPosition();
        UpdateMenuMotion(initialize: true);
    }
}
