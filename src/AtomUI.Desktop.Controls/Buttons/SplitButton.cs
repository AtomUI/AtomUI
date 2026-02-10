using System.Reactive.Disposables;
using System.Windows.Input;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Icons.AntDesign;
using AtomUI.Input;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public class SplitButton : ContentControl, 
                           ICommandSource, 
                           ISizeTypeAware,
                           IControlSharedTokenResourcesHost,
                           IWaveSpiritAwareControl,
                           ICompactSpaceAware
{
    #region 公共属性定义

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<SplitButton, RoutedEventArgs>(
            nameof(Click),
            RoutingStrategies.Bubble);

    public static readonly StyledProperty<ICommand?> CommandProperty =
        Avalonia.Controls.Button.CommandProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<object?> CommandParameterProperty =
        Avalonia.Controls.Button.CommandParameterProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<Flyout?> FlyoutProperty =
        AvaloniaProperty.Register<SplitButton, Flyout?>(nameof(Flyout));

    public static readonly StyledProperty<KeyGesture?> HotKeyProperty =
        Avalonia.Controls.Button.HotKeyProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<FlyoutTriggerType> TriggerTypeProperty =
        FlyoutStateHelper.TriggerTypeProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<bool> IsShowArrowProperty =
        ArrowDecoratedBox.IsShowArrowProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        Flyout.IsPointAtCenterProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        Avalonia.Controls.Primitives.Popup.PlacementProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<PopupAnchor> PlacementAnchorProperty =
        Avalonia.Controls.Primitives.Popup.PlacementAnchorProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<PopupGravity> PlacementGravityProperty =
        Avalonia.Controls.Primitives.Popup.PlacementGravityProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<double> GutterToFlyoutProperty =
        AvaloniaProperty.Register<SplitButton, double>(nameof(GutterToFlyout));

    public static readonly StyledProperty<int> MouseEnterDelayProperty =
        FlyoutStateHelper.MouseEnterDelayProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<int> MouseLeaveDelayProperty =
        FlyoutStateHelper.MouseLeaveDelayProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<bool> IsShowIndicatorProperty =
        AvaloniaProperty.Register<SplitButton, bool>(nameof(IsShowIndicator), true);

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<PathIcon?> IconProperty =
        Button.IconProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<PathIcon?> FlyoutButtonIconProperty =
        AvaloniaProperty.Register<SplitButton, PathIcon?>(nameof(FlyoutButtonIcon));

    public static readonly StyledProperty<bool> IsDangerProperty =
        Button.IsDangerProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<bool> IsPrimaryButtonTypeProperty =
        AvaloniaProperty.Register<SplitButton, bool>(nameof(IsPrimaryButtonType));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<SplitButton>();
    
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a parameter to be passed to the <see cref="Command" />.
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="Flyout" /> that is shown when the secondary part is pressed.
    /// </summary>
    public Flyout? Flyout
    {
        get => GetValue(FlyoutProperty);
        set => SetValue(FlyoutProperty, value);
    }

    /// <summary>
    /// Gets or sets an <see cref="KeyGesture" /> associated with this control
    /// </summary>
    public KeyGesture? HotKey
    {
        get => GetValue(HotKeyProperty);
        set => SetValue(HotKeyProperty, value);
    }

    public FlyoutTriggerType TriggerType
    {
        get => GetValue(TriggerTypeProperty);
        set => SetValue(TriggerTypeProperty, value);
    }

    public bool IsShowArrow
    {
        get => GetValue(IsShowArrowProperty);
        set => SetValue(IsShowArrowProperty, value);
    }

    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    public PlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public PopupGravity PlacementGravity
    {
        get => GetValue(PlacementGravityProperty);
        set => SetValue(PlacementGravityProperty, value);
    }

    public PopupAnchor PlacementAnchor
    {
        get => GetValue(PlacementAnchorProperty);
        set => SetValue(PlacementAnchorProperty, value);
    }

    public double GutterToFlyout
    {
        get => GetValue(GutterToFlyoutProperty);
        set => SetValue(GutterToFlyoutProperty, value);
    }

    public int MouseEnterDelay
    {
        get => GetValue(MouseEnterDelayProperty);
        set => SetValue(MouseEnterDelayProperty, value);
    }

    public int MouseLeaveDelay
    {
        get => GetValue(MouseLeaveDelayProperty);
        set => SetValue(MouseLeaveDelayProperty, value);
    }

    public bool IsShowIndicator
    {
        get => GetValue(IsShowIndicatorProperty);
        set => SetValue(IsShowIndicatorProperty, value);
    }

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public PathIcon? FlyoutButtonIcon
    {
        get => GetValue(FlyoutButtonIconProperty);
        set => SetValue(FlyoutButtonIconProperty, value);
    }

    public bool IsDanger
    {
        get => GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }

    public bool IsPrimaryButtonType
    {
        get => GetValue(IsPrimaryButtonTypeProperty);
        set => SetValue(IsPrimaryButtonTypeProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }

    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<IBrush?> SplitSeparatorBrushProperty = 
        AvaloniaProperty.Register<SplitButton, IBrush?>(nameof (SplitSeparatorBrush));

    internal static readonly DirectProperty<SplitButton, ButtonType> EffectiveButtonTypeProperty =
        AvaloniaProperty.RegisterDirect<SplitButton, ButtonType>(nameof(EffectiveButtonType),
            o => o.EffectiveButtonType,
            (o, v) => o.EffectiveButtonType = v);
    
    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<SplitButton>();
    
    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<SplitButton>();
    
    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty = 
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<SplitButton>();
    
    internal IBrush? SplitSeparatorBrush
    {
        get => GetValue(SplitSeparatorBrushProperty);
        set => SetValue(SplitSeparatorBrushProperty, value);
    }

    private ButtonType _effectiveButtonType;

    internal ButtonType EffectiveButtonType
    {
        get => _effectiveButtonType;
        set => SetAndRaise(EffectiveButtonTypeProperty, ref _effectiveButtonType, value);
    }
    
    internal SpaceItemPosition? CompactSpaceItemPosition
    {
        get => GetValue(CompactSpaceItemPositionProperty);
        set => SetValue(CompactSpaceItemPositionProperty, value);
    }
    
    internal Orientation CompactSpaceOrientation
    {
        get => GetValue(CompactSpaceOrientationProperty);
        set => SetValue(CompactSpaceOrientationProperty, value);
    }
    
    internal bool IsUsedInCompactSpace
    {
        get => GetValue(IsUsedInCompactSpaceProperty);
        set => SetValue(IsUsedInCompactSpaceProperty, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ButtonToken.ID;
    
    #endregion
    
    private Button? _primaryButton;
    private Button? _secondaryButton;
    private KeyGesture? _hotkey;

    private bool _commandCanExecute = true;
    private bool _isFlyoutOpen;
    private bool _isKeyboardPressed;
    private readonly FlyoutStateHelper _flyoutStateHelper;
    
    private CompositeDisposable? _flyoutBindingDisposables;
    private CompositeDisposable? _flyoutHelperBindingDisposables;

    static SplitButton()
    {
        PlacementProperty.OverrideDefaultValue<SplitButton>(PlacementMode.BottomEdgeAlignedRight);
        IsShowArrowProperty.OverrideDefaultValue<SplitButton>(false);
        HorizontalAlignmentProperty.OverrideDefaultValue<SplitButton>(HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<SplitButton>(VerticalAlignment.Top);
        AffectsRender<SplitButton>(IsPrimaryButtonTypeProperty, IsDangerProperty, SplitSeparatorBrushProperty);
    }

    public SplitButton()
    {
        _flyoutStateHelper = new FlyoutStateHelper();
        this.RegisterResources();
    }

    internal virtual bool InternalIsChecked => false;
    protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;
    
    void ICommandSource.CanExecuteChanged(object sender, EventArgs e)
    {
        CanExecuteChanged(sender, e);
    }
    
    private void CanExecuteChanged(object? sender, EventArgs e)
    {
        var (command, parameter) = (Command, CommandParameter);
        CanExecuteChanged(command, parameter);
    }

    private void CanExecuteChanged(ICommand? command, object? parameter)
    {
        if (!this.IsAttachedToLogicalTree())
        {
            return;
        }

        var canExecute = command is null || command.CanExecute(parameter);

        if (canExecute != _commandCanExecute)
        {
            _commandCanExecute = canExecute;
            UpdateIsEffectivelyEnabled();
        }
    }

    /// <summary>
    /// Updates the visual state of the control by applying latest PseudoClasses.
    /// </summary>
    protected void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.FlyoutOpen, _isFlyoutOpen);
        PseudoClasses.Set(StdPseudoClass.Pressed, _isKeyboardPressed);
        PseudoClasses.Set(StdPseudoClass.Checked, InternalIsChecked);
    }

    protected void OpenFlyout()
    {
        _flyoutStateHelper.ShowFlyout();
    }

    /// <summary>
    /// Closes the secondary button's flyout.
    /// </summary>
    protected void CloseFlyout()
    {
        _flyoutStateHelper.HideFlyout();
    }

    /// <summary>
    /// Registers all flyout events.
    /// </summary>
    /// <param name="flyout">The flyout to connect events to.</param>
    private void RegisterFlyoutEvents(Flyout? flyout)
    {
        if (flyout != null)
        {
            flyout.Opened += HandleFlyoutOpened;
            flyout.Closed += HandleFlyoutClosed;
            if (flyout is MenuFlyout menuFlyout)
            {
                menuFlyout.IsDetectMouseClickEnabled = false;
                menuFlyout.ClickHideFlyoutPredicate  = ClickHideFlyoutPredicate;
            }
            
            _flyoutBindingDisposables?.Dispose();
            _flyoutBindingDisposables = new CompositeDisposable(8);
            
            _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, PlacementProperty, flyout));
            _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, PlacementAnchorProperty, flyout));
            _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, PlacementGravityProperty, flyout));
            _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsShowArrowProperty, flyout));
            _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsPointAtCenterProperty, flyout));
            _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, GutterToFlyoutProperty, flyout, MenuFlyout.MarginToAnchorProperty));
            _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, flyout));

            _flyoutBindingDisposables.Add(flyout.GetPropertyChangedObservable(Avalonia.Controls.Primitives.Popup
                                                    .PlacementProperty)
                                                .Subscribe(HandleFlyoutPlacementPropertyChanged));
        }
    }

    /// <summary>
    /// Explicitly unregisters all flyout events.
    /// </summary>
    /// <param name="flyout">The flyout to disconnect events from.</param>
    private void UnregisterFlyoutEvents(Flyout? flyout)
    {
        if (flyout != null)
        {
            flyout.Opened -= HandleFlyoutOpened;
            flyout.Closed -= HandleFlyoutClosed;

            if (flyout is MenuFlyout menuFlyout)
            {
                menuFlyout.ClickHideFlyoutPredicate = null;
            }

            _flyoutBindingDisposables?.Dispose();
            _flyoutBindingDisposables = null;
        }
    }
    
    private void SetupDefaultFlyoutButtonIcon()
    {
        if (FlyoutButtonIcon == null)
        {
            ClearValue(FlyoutButtonIconProperty);
            SetValue(FlyoutButtonIconProperty, new EllipsisOutlined(), BindingPriority.Template);
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (_primaryButton != null)
        {
            _primaryButton.Click -= HandlePrimaryButtonClick;
        }
        _primaryButton                  = e.NameScope.Find<Button>(SplitButtonThemeConstants.PrimaryButtonPart);
        _secondaryButton                = e.NameScope.Find<Button>(SplitButtonThemeConstants.SecondaryButtonPart);
        _flyoutStateHelper.AnchorTarget = _secondaryButton;
        ConfigureButtonCornerRadius();
        if (_primaryButton != null)
        {
            _primaryButton.Click += HandlePrimaryButtonClick;
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _flyoutStateHelper.NotifyAttachedToVisualTree();
        UpdatePseudoClasses();
        RegisterFlyoutEvents(Flyout);
        SetupDefaultFlyoutButtonIcon();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        UnregisterFlyoutEvents(Flyout);
        _flyoutStateHelper.NotifyDetachedFromVisualTree();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        
        // Control attached again, set Hotkey to create a hotkey manager for this control
        SetCurrentValue(HotKeyProperty, _hotkey);

        if (Command != null)
        {
            Command.CanExecuteChanged += CanExecuteChanged;
            CanExecuteChanged(this, EventArgs.Empty);
        }

        _flyoutHelperBindingDisposables?.Dispose();
        _flyoutHelperBindingDisposables = new CompositeDisposable();
        _flyoutHelperBindingDisposables.Add(BindUtils.RelayBind(this, FlyoutProperty, _flyoutStateHelper, FlyoutStateHelper.FlyoutProperty));
        _flyoutHelperBindingDisposables.Add(BindUtils.RelayBind(this, MouseEnterDelayProperty, _flyoutStateHelper,
            FlyoutStateHelper.MouseEnterDelayProperty));
        _flyoutHelperBindingDisposables.Add(BindUtils.RelayBind(this, MouseLeaveDelayProperty, _flyoutStateHelper,
            FlyoutStateHelper.MouseLeaveDelayProperty));
        _flyoutHelperBindingDisposables.Add(BindUtils.RelayBind(this, TriggerTypeProperty, _flyoutStateHelper, FlyoutStateHelper.TriggerTypeProperty));
    }
    
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);

        // This will cause the hotkey manager to dispose the observer and the reference to this control
        _hotkey = HotKey;
        SetCurrentValue(HotKeyProperty, null);

        if (Command != null)
        {
            Command.CanExecuteChanged -= CanExecuteChanged;
        }
        _flyoutHelperBindingDisposables?.Dispose();
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == CommandProperty)
        {
            // Must unregister events here while a reference to the old command still exists
            var (oldValue, newValue) = change.GetOldAndNewValue<ICommand?>();

            if (this.IsAttachedToLogicalTree())
            {
                if (oldValue is not null)
                {
                    oldValue.CanExecuteChanged -= CanExecuteChanged;
                }

                if (newValue is not null)
                {
                    newValue.CanExecuteChanged += CanExecuteChanged;
                }
            }

            CanExecuteChanged(newValue, CommandParameter);
        }
        else if (change.Property == CommandParameterProperty && IsLoaded)
        {
            CanExecuteChanged(Command, change.NewValue);
        }
        else if (change.Property == FlyoutProperty)
        {
            var (oldFlyout, newFlyout) = change.GetOldAndNewValue<Flyout?>();

            // If flyout is changed while one is already open, make sure we 
            // close the old one first
            // This is the same behavior as Button
            if (oldFlyout != null && oldFlyout.IsOpen)
            {
                oldFlyout.Hide();
            }

            // Must unregister events here while a reference to the old flyout still exists
            UnregisterFlyoutEvents(oldFlyout);
            RegisterFlyoutEvents(newFlyout);
            UpdatePseudoClasses();
        }
        else if (change.Property == IsPrimaryButtonTypeProperty)
        {
            SetupEffectiveButtonType();
        }

        if (change.Property == CornerRadiusProperty ||
            change.Property == CompactSpaceItemPositionProperty ||
            change.Property == CompactSpaceOrientationProperty)
        {
            ConfigureButtonCornerRadius();
        }

        base.OnPropertyChanged(change);
    }

    private void SetupEffectiveButtonType()
    {
        if (IsPrimaryButtonType)
        {
            EffectiveButtonType = ButtonType.Primary;
        }
        else
        {
            EffectiveButtonType = ButtonType.Default;
        }
    }

    private void ConfigureButtonCornerRadius()
    {
        var effectiveCornerRadius = CompactSpace.CalculateEffectiveCornerRadius(
            CornerRadius, 
            IsUsedInCompactSpace, 
            CompactSpaceItemPosition,
            CompactSpaceOrientation);
        
        var primaryButtonCornerRadius = new CornerRadius(effectiveCornerRadius.TopLeft,
            0,
            0,
            effectiveCornerRadius.BottomLeft);
        
        var secondaryButtonCornerRadius = new CornerRadius(0,
            effectiveCornerRadius.TopRight,
            effectiveCornerRadius.BottomRight,
            0);
        
        if (_primaryButton is not null)
        {
            _primaryButton.CornerRadius = primaryButtonCornerRadius;
        }

        if (_secondaryButton is not null)
        {
            _secondaryButton.CornerRadius = secondaryButtonCornerRadius;
        }
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        var key = e.Key;

        if (key == Key.Space || key == Key.Enter)
        {
            _isKeyboardPressed = true;
            UpdatePseudoClasses();
        }

        base.OnKeyDown(e);
    }
    
    protected override void OnKeyUp(KeyEventArgs e)
    {
        var key = e.Key;

        if (key == Key.Space || key == Key.Enter)
        {
            _isKeyboardPressed = false;
            UpdatePseudoClasses();

            // Consider this a click on the primary button
            if (IsEffectivelyEnabled)
            {
                OnClickPrimary(null);
                e.Handled = true;
            }
        }
        else if (key == Key.Down && e.KeyModifiers.HasAllFlags(KeyModifiers.Alt) && IsEffectivelyEnabled
                 && !this.IsAllowedXYNavigationMode(e.KeyDeviceType))
        {
            OpenFlyout();
            e.Handled = true;
        }
        else if (key == Key.F4 && IsEffectivelyEnabled)
        {
            OpenFlyout();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && _isFlyoutOpen)
        {
            // If Flyout doesn't have focusable content, close the flyout here
            // This is the same behavior as Button
            CloseFlyout();
            e.Handled = true;
        }

        base.OnKeyUp(e);
    }

    /// <summary>
    /// Invokes the <see cref="Click" /> event when the primary button part is clicked.
    /// </summary>
    /// <param name="e">The event args from the internal Click event.</param>
    protected virtual void OnClickPrimary(RoutedEventArgs? e)
    {
        var (command, parameter) = (Command, CommandParameter);
        // Note: It is not currently required to check enabled status; however, this is a failsafe
        if (IsEffectivelyEnabled)
        {
            var eventArgs = new RoutedEventArgs(ClickEvent);
            RaiseEvent(eventArgs);

            if (!eventArgs.Handled && command?.CanExecute(parameter) == true)
            {
                command.Execute(parameter);
                eventArgs.Handled = true;
            }
        }
    }

    /// <summary>
    /// Invoked when the split button's flyout is opened.
    /// </summary>
    protected virtual void OnFlyoutOpened()
    {
        // Available for derived types
    }

    /// <summary>
    /// Invoked when the split button's flyout is closed.
    /// </summary>
    protected virtual void OnFlyoutClosed()
    {
        // Available for derived types
    }

    /// <summary>
    /// Event handler for when the internal primary button part is clicked.
    /// </summary>
    private void HandlePrimaryButtonClick(object? sender, RoutedEventArgs e)
    {
        // Handle internal button click, so it won't bubble outside together with SplitButton.ClickEvent.
        e.Handled = true;
        OnClickPrimary(e);
    }

    /// <summary>
    /// Called when the <see cref="PopupFlyoutBase.Placement" /> property changes.
    /// </summary>
    private void HandleFlyoutPlacementPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        UpdatePseudoClasses();
    }

    /// <summary>
    /// Event handler for when the split button's flyout is opened.
    /// </summary>
    private void HandleFlyoutOpened(object? sender, EventArgs e)
    {
        var flyout = sender as Flyout;

        // It is possible to share flyouts among multiple controls including SplitButton.
        // This can cause a problem here since all controls that share a flyout receive
        // the same Opened/Closed events at the same time.
        // For SplitButton that means they all would be updating their pseudoclasses accordingly.
        // In other words, all SplitButtons with a shared Flyout would have the backgrounds changed together.
        // To fix this, only continue here if the Flyout target matches this SplitButton instance.
        if (ReferenceEquals(flyout?.Target, this))
        {
            _isFlyoutOpen = true;
            UpdatePseudoClasses();
            OnFlyoutOpened();
        }
    }

    private bool ClickHideFlyoutPredicate(IPopupHostProvider hostProvider, RawPointerEventArgs args)
    {
        if (hostProvider.PopupHost != args.Root)
        {
            if (_secondaryButton is not null)
            {
                if (TriggerType == FlyoutTriggerType.Click)
                {
                    return true;
                }

                // 只有 TriggerType 为 Hover 的时候会判断
                var secondaryButtonOrigin =
                    _secondaryButton.TranslatePoint(new Point(0, 0), TopLevel.GetTopLevel(_secondaryButton)!);
                var secondaryBounds = secondaryButtonOrigin.HasValue
                    ? new Rect(secondaryButtonOrigin.Value, _secondaryButton.Bounds.Size)
                    : new Rect();
                if (!secondaryBounds.Contains(args.Position))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Event handler for when the split button's flyout is closed.
    /// </summary>
    private void HandleFlyoutClosed(object? sender, EventArgs e)
    {
        var flyout = sender as Flyout;

        // See comments in HandleFlyoutOpened
        if (ReferenceEquals(flyout?.Target, this))
        {
            _isFlyoutOpen = false;
            UpdatePseudoClasses();
            OnFlyoutClosed();
        }
    }

    private Rect? _originRect;

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (_originRect is null)
        {
            _originRect = _secondaryButton?.Bounds;
        }
        
        
        if (!IsPrimaryButtonType)
        {
            if (_secondaryButton is not null && _originRect.HasValue)
            {
                _secondaryButton.Arrange(
                    _originRect.Value.Inflate(new Thickness(_secondaryButton.BorderThickness.Left * 2, 0, 0, 0)));
            }
        }
        else
        {
            if (_secondaryButton is not null && _originRect.HasValue)
            {
                _secondaryButton.Arrange(
                    _originRect.Value.Deflate(new Thickness(_secondaryButton.BorderThickness.Left, 0, 0, 0)));
            }
        }

        return size;
    }

    public override void Render(DrawingContext context)
    {
        if (IsPrimaryButtonType)
        {
            if (_secondaryButton is not null)
            {
                var cornerRadius = (float)CornerRadius.TopLeft;
                context.FillRectangle(SplitSeparatorBrush ?? Brushes.White, new Rect(0, 0, Bounds.Width, Bounds.Height),
                    cornerRadius);
            }
        }
    }
    
    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        IsUsedInCompactSpace     = position != null;
        CompactSpaceItemPosition = position;
    }

    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        CompactSpaceOrientation = orientation;
    }
    
    double ICompactSpaceAware.GetBorderThickness() => GetBorderThicknessForCompactSpace();

    protected virtual double GetBorderThicknessForCompactSpace()
    {
        if (!IsUsedInCompactSpace)
        {
            return 0.0;
        }

        return CompactSpaceOrientation == Orientation.Horizontal ? BorderThickness.Left : BorderThickness.Top;
    }
}