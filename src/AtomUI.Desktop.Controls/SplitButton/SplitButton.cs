using System.Reactive.Disposables;
using System.Windows.Input;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Icons.AntDesign;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public partial class SplitButton : ContentControl,
                           ICommandSource, 
                           ICustomizableSizeTypeAware,
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

    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        Flyout.IsPointAtCenterProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        Popup.PlacementProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<PopupAnchor> PlacementAnchorProperty =
        Popup.PlacementAnchorProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<PopupGravity> PlacementGravityProperty =
        Popup.PlacementGravityProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<double> GutterToFlyoutProperty =
        AvaloniaProperty.Register<SplitButton, double>(nameof(GutterToFlyout));

    public static readonly StyledProperty<int> MouseEnterDelayProperty =
        FlyoutStateHelper.MouseEnterDelayProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<int> MouseLeaveDelayProperty =
        FlyoutStateHelper.MouseLeaveDelayProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<PathIcon?> IconProperty =
        Button.IconProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<PathIcon?> OpenIndicatorProperty =
        AvaloniaProperty.Register<SplitButton, PathIcon?>(nameof(OpenIndicator));

    public static readonly StyledProperty<bool> IsDangerProperty =
        Button.IsDangerProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<bool> IsPrimaryButtonTypeProperty =
        AvaloniaProperty.Register<SplitButton, bool>(nameof(IsPrimaryButtonType));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        Flyout.ShouldUseOverlayPopupProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Flyout.IsPopupPinnedOpenProperty.AddOwner<SplitButton>();

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

    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
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

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public PathIcon? OpenIndicator
    {
        get => GetValue(OpenIndicatorProperty);
        set => SetValue(OpenIndicatorProperty, value);
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

    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the flyout popup stays pinned open. Used by
    /// Gallery semantic part previews to keep the cross-visual-root popup materialized.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
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

    /// <summary>
    /// primary 形态接缝分隔线当前是否以其对比线色呈现。对齐上游 solid 组合规则：线色 =
    /// 次按钮 hover 背景 token（colorPrimaryHover / colorErrorHover），因此持线的次按钮被指向时
    /// 接缝条带与按钮 hover 背景同色，分隔线视觉上隐藏——隐藏完全由颜色涌现，
    /// 不允许改动次按钮几何（曾用左移覆盖实现，导致 hover 位移与边缘闪烁的体验回归）。
    /// </summary>
    internal bool IsSeparatorVisible =>
        IsPrimaryButtonType && _secondaryButton is not null && !_isPointerOverSecondaryButton;

    #endregion
    
    private Button? _primaryButton;
    private Button? _secondaryButton;
    private KeyGesture? _hotkey;

    private bool _commandCanExecute = true;
    private bool _isFlyoutOpen;
    private bool _isKeyboardPressed;
    private bool _isPointerOverSecondaryButton;
    private bool _isPinnedOpenRetryArmed;
    private readonly FlyoutStateHelper _flyoutStateHelper;
    
    private CompositeDisposable? _flyoutBindingDisposables;
    private int _pinnedOpenGeneration;
    private Flyout? _registeredFlyout;

    static SplitButton()
    {
        PlacementProperty.OverrideDefaultValue<SplitButton>(PlacementMode.BottomEdgeAlignedRight);
        IsArrowVisibleProperty.OverrideDefaultValue<SplitButton>(false);
        HorizontalAlignmentProperty.OverrideDefaultValue<SplitButton>(HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<SplitButton>(VerticalAlignment.Top);
        AffectsMeasure<SplitButton>(SizeTypeProperty);
        AffectsArrange<SplitButton>(IsPrimaryButtonTypeProperty, BorderThicknessProperty, UseLayoutRoundingProperty);
        AffectsRender<SplitButton>(IsPrimaryButtonTypeProperty, IsDangerProperty, SplitSeparatorBrushProperty);
    }

    public SplitButton()
    {
        _flyoutStateHelper = new FlyoutStateHelper();
        _flyoutStateHelper[!FlyoutStateHelper.FlyoutProperty]          = this[!FlyoutProperty];
        _flyoutStateHelper[!FlyoutStateHelper.MouseEnterDelayProperty] = this[!MouseEnterDelayProperty];
        _flyoutStateHelper[!FlyoutStateHelper.MouseLeaveDelayProperty] = this[!MouseLeaveDelayProperty];
        _flyoutStateHelper[!FlyoutStateHelper.TriggerTypeProperty]     = this[!TriggerTypeProperty];
    }

    internal virtual bool InternalIsChecked => false;
    protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (OpenIndicator == null)
        {
            SetCurrentValue(OpenIndicatorProperty, new EllipsisOutlined());
        }
    }

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
        if (flyout == null || ReferenceEquals(_registeredFlyout, flyout))
        {
            return;
        }

        UnregisterFlyoutEvents(_registeredFlyout);

        _registeredFlyout = flyout;
        flyout.Opened += HandleFlyoutOpened;
        flyout.Closed += HandleFlyoutClosed;

        _flyoutBindingDisposables?.Dispose();
        _flyoutBindingDisposables = new CompositeDisposable(10);

        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, PlacementProperty, flyout, Flyout.RequestedPlacementProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, PlacementAnchorProperty, flyout));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, PlacementGravityProperty, flyout));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsArrowVisibleProperty, flyout));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsPointAtCenterProperty, flyout));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, GutterToFlyoutProperty, flyout, MenuFlyout.MarginToAnchorProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, flyout));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, ShouldUseOverlayPopupProperty, flyout));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsPopupPinnedOpenProperty, flyout, Flyout.IsPopupPinnedOpenProperty));

        _flyoutBindingDisposables.Add(flyout.GetPropertyChangedObservable(Popup
                                                .RequestedPlacementProperty)
                                            .Subscribe(HandleFlyoutPlacementPropertyChanged));

        if (IsPopupPinnedOpen && this.IsAttachedToVisualTree())
        {
            QueuePinnedOpen();
        }
    }

    /// <summary>
    /// Explicitly unregisters all flyout events.
    /// </summary>
    /// <param name="flyout">The flyout to disconnect events from.</param>
    private void UnregisterFlyoutEvents(Flyout? flyout)
    {
        if (flyout == null || !ReferenceEquals(_registeredFlyout, flyout))
        {
            return;
        }

        flyout.Opened -= HandleFlyoutOpened;
        flyout.Closed -= HandleFlyoutClosed;

        ++_pinnedOpenGeneration;
        flyout.CloseForLifecycle();
        _registeredFlyout = null;
        _isFlyoutOpen     = false;
        _flyoutBindingDisposables?.Dispose();
        _flyoutBindingDisposables = null;
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (_primaryButton != null)
        {
            _primaryButton.Click -= HandlePrimaryButtonClick;
        }
        if (_secondaryButton != null)
        {
            _secondaryButton.PointerEntered -= HandleSecondaryButtonPointerEntered;
            _secondaryButton.PointerExited  -= HandleSecondaryButtonPointerExited;
        }
        _primaryButton                  = e.NameScope.Find<Button>("PART_PrimaryButton");
        _secondaryButton                = e.NameScope.Find<Button>("PART_SecondaryButton");
        _flyoutStateHelper.AnchorTarget = _secondaryButton;
        ConfigureButtonCornerRadius();
        ConfigureButtonCustomSizeOverrides();
        if (_primaryButton != null)
        {
            _primaryButton.Click += HandlePrimaryButtonClick;
        }
        if (_secondaryButton != null)
        {
            _secondaryButton.PointerEntered += HandleSecondaryButtonPointerEntered;
            _secondaryButton.PointerExited  += HandleSecondaryButtonPointerExited;
        }

        if (IsPopupPinnedOpen && this.IsAttachedToVisualTree())
        {
            QueuePinnedOpen();
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _flyoutStateHelper.NotifyAttachedToVisualTree();
        UpdatePseudoClasses();
        RegisterFlyoutEvents(Flyout);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        UnregisterFlyoutEvents(_registeredFlyout);
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

            // Must unregister events here while a reference to the old flyout still exists
            UnregisterFlyoutEvents(oldFlyout);
            if (this.IsAttachedToVisualTree())
            {
                RegisterFlyoutEvents(newFlyout);
            }
            UpdatePseudoClasses();
        }
        else if (change.Property == IsPopupPinnedOpenProperty && this.IsAttachedToVisualTree())
        {
            if (change.GetNewValue<bool>())
            {
                QueuePinnedOpen();
            }
            else
            {
                ++_pinnedOpenGeneration;
            }
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

        if (ShouldConfigureCustomSizeOverrides(change.Property))
        {
            ConfigureButtonCustomSizeOverrides();
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

    private void ConfigureButtonCustomSizeOverrides()
    {
        var useCustomOverrides = SizeType == CustomizableSizeType.Custom;

        ConfigureButtonCustomSizeOverride(_primaryButton, HeightProperty, Height, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_secondaryButton, HeightProperty, Height, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_primaryButton, MinHeightProperty, MinHeight, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_secondaryButton, MinHeightProperty, MinHeight, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_primaryButton, WidthProperty, Width, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_secondaryButton, WidthProperty, Width, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_primaryButton, MinWidthProperty, MinWidth, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_secondaryButton, MinWidthProperty, MinWidth, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_primaryButton, PaddingProperty, Padding, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_secondaryButton, PaddingProperty, Padding, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_primaryButton, FontSizeProperty, FontSize, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_secondaryButton, FontSizeProperty, FontSize, useCustomOverrides);
    }

    private void ConfigureButtonCustomSizeOverride<T>(
        Button? button,
        StyledProperty<T> property,
        T value,
        bool useCustomOverrides)
    {
        if (button is null)
        {
            return;
        }

        if (useCustomOverrides && IsLocalValue(property))
        {
            button.SetValue(property, value);
        }
        else
        {
            button.ClearValue(property);
        }
    }

    private bool IsLocalValue(AvaloniaProperty property)
    {
        return IsSet(property);
    }

    private static bool ShouldConfigureCustomSizeOverrides(AvaloniaProperty property)
    {
        return property == SizeTypeProperty ||
               property == HeightProperty ||
               property == MinHeightProperty ||
               property == WidthProperty ||
               property == MinWidthProperty ||
               property == PaddingProperty ||
               property == FontSizeProperty;
    }

    private void QueuePinnedOpen()
    {
        var generation = ++_pinnedOpenGeneration;
        var flyout     = _registeredFlyout;
        var anchor     = _secondaryButton;
        Dispatcher.Post(() =>
        {
            if (generation == _pinnedOpenGeneration &&
                IsPopupPinnedOpen &&
                this.IsAttachedToVisualTree() &&
                ReferenceEquals(flyout, _registeredFlyout) &&
                ReferenceEquals(anchor, _secondaryButton))
            {
                _flyoutStateHelper.ShowFlyout(immediately: true);
            }
        });
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
        else if (key == Key.Down && e.KeyModifiers.HasAllFlags(KeyModifiers.Alt) && IsEffectivelyEnabled)
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
        // To fix this, only continue here if the Flyout target belongs to this SplitButton.
        if (IsOwnFlyoutTarget(flyout))
        {
            _isFlyoutOpen = true;
            UpdatePseudoClasses();
            OnFlyoutOpened();
        }
    }

    /// <summary>
    /// Event handler for when the split button's flyout is closed.
    /// </summary>
    private void HandleFlyoutClosed(object? sender, EventArgs e)
    {
        var flyout = sender as Flyout;

        // See comments in HandleFlyoutOpened
        if (IsOwnFlyoutTarget(flyout))
        {
            _isFlyoutOpen = false;
            UpdatePseudoClasses();
            OnFlyoutClosed();
            if (IsPopupPinnedOpen && this.IsAttachedToVisualTree())
            {
                QueuePinnedOpenRetry();
            }
        }
    }

    // Flyout 关闭（上游 HideCore）会清掉 Target 与 popup 的 PlacementTarget / 逻辑父子，
    // Popup 侧的钉住恢复监控随之拆除。钉住弹层的关闭只能来自放置目标生命周期失效
    //（页签切走 / 滚出视口），owner 持有钉住意图：锚点重新可见后重新 ShowAt 重建整条链。
    private void QueuePinnedOpenRetry()
    {
        if (_isPinnedOpenRetryArmed)
        {
            return;
        }

        _isPinnedOpenRetryArmed = true;
        LayoutUpdated += HandlePinnedOpenRetryLayoutUpdated;
    }

    private void HandlePinnedOpenRetryLayoutUpdated(object? sender, EventArgs e)
    {
        if (_isFlyoutOpen ||
            !IsPopupPinnedOpen ||
            !this.IsAttachedToVisualTree())
        {
            DisarmPinnedOpenRetry();
            return;
        }

        if (IsEffectivelyVisible && _secondaryButton is { IsEffectivelyVisible: true })
        {
            DisarmPinnedOpenRetry();
            QueuePinnedOpen();
        }
    }

    private void DisarmPinnedOpenRetry()
    {
        if (!_isPinnedOpenRetryArmed)
        {
            return;
        }

        _isPinnedOpenRetryArmed = false;
        LayoutUpdated -= HandlePinnedOpenRetryLayoutUpdated;
    }

    private bool IsOwnFlyoutTarget(Flyout? flyout)
    {
        var target = flyout?.Target;
        return ReferenceEquals(target, this) ||
               ReferenceEquals(target, _secondaryButton);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);

        if (_primaryButton is not null && _secondaryButton is not null)
        {
            var originRect = _secondaryButton.Bounds;
            var secondaryLeft = _primaryButton.Bounds.Right;
            if (!IsPrimaryButtonType)
            {
                secondaryLeft -= _secondaryButton.BorderThickness.Left;
            }
            else
            {
                secondaryLeft += BorderUtils.BuildRenderScaleAwareThickness(this, BorderThickness.Left);
            }

            _secondaryButton.Arrange(new Rect(
                secondaryLeft,
                originRect.Y,
                Math.Max(0, originRect.Right - secondaryLeft),
                originRect.Height));
        }

        return size;
    }

    private void HandleSecondaryButtonPointerEntered(object? sender, PointerEventArgs e)
    {
        SetSeparatorVisibility(false);
    }

    private void HandleSecondaryButtonPointerExited(object? sender, PointerEventArgs e)
    {
        SetSeparatorVisibility(true);
    }

    private void SetSeparatorVisibility(bool visible)
    {
        // 分隔线的 hover 隐藏由颜色涌现实现（线色 = 次按钮 hover 背景 token），
        // 状态翻转不驱动布局或重绘——改动次按钮几何会造成可见位移与边缘闪烁。
        _isPointerOverSecondaryButton = !visible;
    }

    public override void Render(DrawingContext context)
    {
        if (IsPrimaryButtonType && _secondaryButton is not null)
        {
            var cornerRadius = (float)CornerRadius.TopLeft;
            context.FillRectangle(SplitSeparatorBrush ?? Brushes.White, new Rect(0, 0, Bounds.Width, Bounds.Height),
                cornerRadius);
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
