using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

// A Token-owner transparent part: a StepsItem is an item of Steps only and has no Own Tokens, so its theme reads the Steps
// TokenResource and its Token-declared transitions (and those of its indicator) resolve to the Steps through the
// inherited owner. Controls[Steps] therefore reaches the item titles, tails, icons and motion.
[TokenOwnerTransparent]
[PseudoClasses(
    StepsItemPseudoClass.ItemHeaderForegroundOverride,
    StepsItemPseudoClass.ItemSubHeaderForegroundOverride,
    StepsItemPseudoClass.ItemRailBackgroundOverride)]
public class StepsItem : AbstractStepsItem
{
    #region 公共属性定义

    public new static readonly StyledProperty<object?> SubHeaderProperty =
        AbstractStepsItem.SubHeaderProperty.AddOwner<StepsItem>();

    public new static readonly StyledProperty<IDataTemplate?> SubHeaderTemplateProperty =
        AbstractStepsItem.SubHeaderTemplateProperty.AddOwner<StepsItem>();

    public new static readonly StyledProperty<PathIcon?> IconProperty =
        AbstractStepsItem.IconProperty.AddOwner<StepsItem>();

    public new static readonly StyledProperty<StepsStatus?> StatusProperty =
        AbstractStepsItem.StatusProperty.AddOwner<StepsItem>();

    public new object? SubHeader
    {
        get => GetValue(SubHeaderProperty);
        set => SetValue(SubHeaderProperty, value);
    }

    public new IDataTemplate? SubHeaderTemplate
    {
        get => GetValue(SubHeaderTemplateProperty);
        set => SetValue(SubHeaderTemplateProperty, value);
    }

    public new PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public new StepsStatus? Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<StepsType> TypeProperty =
        Steps.TypeProperty.AddOwner<StepsItem>();

    internal static readonly StyledProperty<StepsPanelVariant> PanelVariantProperty =
        Steps.PanelVariantProperty.AddOwner<StepsItem>();

    internal static readonly StyledProperty<Orientation> OrientationProperty =
        Steps.OrientationProperty.AddOwner<StepsItem>();

    internal static readonly StyledProperty<Orientation> TitlePlacementProperty =
        Steps.TitlePlacementProperty.AddOwner<StepsItem>();

    internal static readonly StyledProperty<SizeType> SizeTypeProperty =
        Steps.SizeTypeProperty.AddOwner<StepsItem>();

    internal static readonly StyledProperty<bool> IsClickableProperty =
        AvaloniaProperty.Register<StepsItem, bool>(nameof(IsClickable));

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        Steps.IsMotionEnabledProperty.AddOwner<StepsItem>();

    internal static readonly StyledProperty<double?> PercentProperty =
        Steps.PercentProperty.AddOwner<StepsItem>();

    internal static readonly StyledProperty<IBrush?> ItemHeaderForegroundProperty =
        AvaloniaProperty.Register<StepsItem, IBrush?>(nameof(ItemHeaderForeground));

    internal static readonly StyledProperty<IBrush?> ItemSubHeaderForegroundProperty =
        AvaloniaProperty.Register<StepsItem, IBrush?>(nameof(ItemSubHeaderForeground));

    internal static readonly StyledProperty<IBrush?> ItemRailBackgroundProperty =
        AvaloniaProperty.Register<StepsItem, IBrush?>(nameof(ItemRailBackground));

    internal static readonly DirectProperty<StepsItem, StepsStatus> ConnectorStatusProperty =
        AvaloniaProperty.RegisterDirect<StepsItem, StepsStatus>(
            nameof(ConnectorStatus),
            item => item.ConnectorStatus,
            (item, value) => item.ConnectorStatus = value);

    internal static readonly DirectProperty<StepsItem, bool> CanInvokeProperty =
        AvaloniaProperty.RegisterDirect<StepsItem, bool>(
            nameof(CanInvoke),
            item => item.CanInvoke,
            (item, value) => item.CanInvoke = value);

    internal static readonly DirectProperty<StepsItem, bool> IsProgressVisibleProperty =
        AvaloniaProperty.RegisterDirect<StepsItem, bool>(
            nameof(IsProgressVisible),
            item => item.IsProgressVisible,
            (item, value) => item.IsProgressVisible = value);

    internal static readonly DirectProperty<StepsItem, bool> IsProgressFrameReservedProperty =
        AvaloniaProperty.RegisterDirect<StepsItem, bool>(
            nameof(IsProgressFrameReserved),
            item => item.IsProgressFrameReserved,
            (item, value) => item.IsProgressFrameReserved = value);

    internal StepsType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    internal StepsPanelVariant PanelVariant
    {
        get => GetValue(PanelVariantProperty);
        set => SetValue(PanelVariantProperty, value);
    }

    internal Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    internal Orientation TitlePlacement
    {
        get => GetValue(TitlePlacementProperty);
        set => SetValue(TitlePlacementProperty, value);
    }

    internal SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    internal bool IsClickable
    {
        get => GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal double? Percent
    {
        get => GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    internal IBrush? ItemHeaderForeground
    {
        get => GetValue(ItemHeaderForegroundProperty);
        set => SetValue(ItemHeaderForegroundProperty, value);
    }

    internal IBrush? ItemSubHeaderForeground
    {
        get => GetValue(ItemSubHeaderForegroundProperty);
        set => SetValue(ItemSubHeaderForegroundProperty, value);
    }

    internal IBrush? ItemRailBackground
    {
        get => GetValue(ItemRailBackgroundProperty);
        set => SetValue(ItemRailBackgroundProperty, value);
    }

    private StepsStatus _connectorStatus = StepsStatus.Wait;

    internal StepsStatus ConnectorStatus
    {
        get => _connectorStatus;
        private set => SetAndRaise(ConnectorStatusProperty, ref _connectorStatus, value);
    }

    private bool _canInvoke;

    internal bool CanInvoke
    {
        get => _canInvoke;
        private set => SetAndRaise(CanInvokeProperty, ref _canInvoke, value);
    }

    private bool _isProgressVisible;

    internal bool IsProgressVisible
    {
        get => _isProgressVisible;
        private set => SetAndRaise(IsProgressVisibleProperty, ref _isProgressVisible, value);
    }

    private bool _isProgressFrameReserved;

    internal bool IsProgressFrameReserved
    {
        get => _isProgressFrameReserved;
        private set => SetAndRaise(IsProgressFrameReservedProperty, ref _isProgressFrameReserved, value);
    }

    #endregion

    internal new Steps? Owner => base.Owner as Steps;

    private StepsItemIndicator? _indicator;

    static StepsItem()
    {
        PressedMixin.Attach<StepsItem>();
        FocusableProperty.OverrideDefaultValue<StepsItem>(false);
        AffectsMeasure<StepsItem>(
            TypeProperty,
            PanelVariantProperty,
            OrientationProperty,
            TitlePlacementProperty,
            SizeTypeProperty);
    }

    internal void ApplyOwnerState(int stepNumber, bool isCurrent, StepsStatus automaticStatus,
        bool isFirst, bool isLast, StepsStatus connectorStatus)
    {
        SetStepState(stepNumber, isCurrent, automaticStatus, isFirst, isLast);
        ApplyDesktopConnectorStatus(connectorStatus);
    }

    internal void ApplyDesktopConnectorStatus(StepsStatus status)
    {
        ConnectorStatus = status;
        UpdateCanInvoke();
        UpdateProgressState();
    }

    private protected override void OnOwnerDetached()
    {
        ConnectorStatus = StepsStatus.Wait;
        CanInvoke = false;
        IsProgressVisible = false;
        IsProgressFrameReserved = false;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _indicator = null;
        base.OnApplyTemplate(e);
        _indicator = e.NameScope.Find<StepsItemIndicator>("PART_Indicator");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsClickableProperty ||
                 change.Property == IsEffectivelyEnabledProperty)
        {
            UpdateCanInvoke();
        }

        if (change.Property == PercentProperty ||
            change.Property == TypeProperty ||
            change.Property == PanelVariantProperty ||
            change.Property == IconProperty ||
            change.Property == IsCurrentProperty ||
            change.Property == EffectiveStatusProperty)
        {
            UpdateProgressState();
        }

        if (change.Property == IsCurrentProperty && IsCurrent && _indicator is not null)
        {
            _indicator.IsItemHover = false;
        }

        if (change.Property == ItemHeaderForegroundProperty)
        {
            PseudoClasses.Set(
                StepsItemPseudoClass.ItemHeaderForegroundOverride,
                ItemHeaderForeground is not null);
        }
        else if (change.Property == ItemSubHeaderForegroundProperty)
        {
            PseudoClasses.Set(
                StepsItemPseudoClass.ItemSubHeaderForegroundOverride,
                ItemSubHeaderForeground is not null);
        }
        else if (change.Property == ItemRailBackgroundProperty)
        {
            PseudoClasses.Set(
                StepsItemPseudoClass.ItemRailBackgroundOverride,
                ItemRailBackground is not null);
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (_indicator is not null && CanInvoke && !IsCurrent)
        {
            _indicator.IsItemHover = true;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (CanInvoke && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            e.Pointer.Capture(this);
            e.PreventGestureRecognition();
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!ReferenceEquals(e.Pointer.Captured, this))
        {
            return;
        }

        var invoke = e.InitialPressMouseButton == MouseButton.Left &&
                     new Rect(Bounds.Size).Contains(e.GetPosition(this));
        e.Pointer.Capture(null);
        e.Handled = true;

        if (invoke)
        {
            InvokeFromPointer();
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_indicator is not null)
        {
            _indicator.IsItemHover = false;
        }

        base.OnPointerCaptureLost(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (CanInvoke && e.Key is Key.Enter or Key.Space)
        {
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (CanInvoke && e.Key is Key.Enter or Key.Space)
        {
            InvokeFromKeyboard();
            e.Handled = true;
        }

        base.OnKeyUp(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (_indicator is not null)
        {
            _indicator.IsItemHover = false;
        }
    }

    private void UpdateCanInvoke()
    {
        CanInvoke = Owner is not null && IsClickable && IsEffectivelyEnabled;
        SetCurrentValue(FocusableProperty, CanInvoke);
    }

    private void InvokeFromPointer()
    {
        if (!CanInvoke)
        {
            return;
        }

        PlayActivationWave();
        Owner?.RequestCurrentChange(this);
    }

    private void InvokeFromKeyboard()
    {
        if (CanInvoke)
        {
            PlayActivationWave();
            Owner?.RequestCurrentChange(this);
        }
    }

    private void PlayActivationWave()
    {
        var indicator = _indicator;
        if (indicator is null)
        {
            return;
        }

        Dispatcher.UIThread.Post(indicator.PlayWave);
    }

    private void UpdateProgressState()
    {
        IsProgressFrameReserved = Percent.HasValue &&
                                  Type is StepsType.Default or StepsType.Navigation;
        IsProgressVisible = IsProgressFrameReserved &&
                            IsCurrent &&
                            EffectiveStatus == StepsStatus.Process &&
                            Icon is null;
    }
}
