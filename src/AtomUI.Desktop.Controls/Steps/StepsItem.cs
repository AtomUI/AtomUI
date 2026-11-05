using AtomUI.Theme.Resources;
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

[PseudoClasses(
    StepsItemPseudoClass.ItemHeaderForegroundOverride,
    StepsItemPseudoClass.ItemSubHeaderForegroundOverride,
    StepsItemPseudoClass.ItemRailBackgroundOverride)]
public class StepsItem : HeaderedContentControl, IControlThemeGlobalOwnerSource
{
    #region 公共属性定义

    public static readonly StyledProperty<object?> SubHeaderProperty =
        AvaloniaProperty.Register<StepsItem, object?>(nameof(SubHeader));

    public static readonly StyledProperty<IDataTemplate?> SubHeaderTemplateProperty =
        AvaloniaProperty.Register<StepsItem, IDataTemplate?>(nameof(SubHeaderTemplate));

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<StepsItem, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<StepsStatus?> StatusProperty =
        AvaloniaProperty.Register<StepsItem, StepsStatus?>(nameof(Status));

    public object? SubHeader
    {
        get => GetValue(SubHeaderProperty);
        set => SetValue(SubHeaderProperty, value);
    }

    public IDataTemplate? SubHeaderTemplate
    {
        get => GetValue(SubHeaderTemplateProperty);
        set => SetValue(SubHeaderTemplateProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public StepsStatus? Status
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

    internal static readonly DirectProperty<StepsItem, int> StepNumberProperty =
        AvaloniaProperty.RegisterDirect<StepsItem, int>(
            nameof(StepNumber),
            item => item.StepNumber,
            (item, value) => item.StepNumber = value);

    internal static readonly DirectProperty<StepsItem, bool> IsCurrentProperty =
        AvaloniaProperty.RegisterDirect<StepsItem, bool>(
            nameof(IsCurrent),
            item => item.IsCurrent,
            (item, value) => item.IsCurrent = value);

    internal static readonly DirectProperty<StepsItem, StepsStatus> AutomaticStatusProperty =
        AvaloniaProperty.RegisterDirect<StepsItem, StepsStatus>(
            nameof(AutomaticStatus),
            item => item.AutomaticStatus,
            (item, value) => item.AutomaticStatus = value);

    internal static readonly DirectProperty<StepsItem, StepsStatus> EffectiveStatusProperty =
        AvaloniaProperty.RegisterDirect<StepsItem, StepsStatus>(
            nameof(EffectiveStatus),
            item => item.EffectiveStatus,
            (item, value) => item.EffectiveStatus = value);

    internal static readonly DirectProperty<StepsItem, bool> IsFirstProperty =
        AvaloniaProperty.RegisterDirect<StepsItem, bool>(
            nameof(IsFirst),
            item => item.IsFirst,
            (item, value) => item.IsFirst = value);

    internal static readonly DirectProperty<StepsItem, bool> IsLastProperty =
        AvaloniaProperty.RegisterDirect<StepsItem, bool>(
            nameof(IsLast),
            item => item.IsLast,
            (item, value) => item.IsLast = value);

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

    private int _stepNumber;

    internal int StepNumber
    {
        get => _stepNumber;
        private set => SetAndRaise(StepNumberProperty, ref _stepNumber, value);
    }

    private bool _isCurrent;

    internal bool IsCurrent
    {
        get => _isCurrent;
        private set => SetAndRaise(IsCurrentProperty, ref _isCurrent, value);
    }

    private StepsStatus _automaticStatus = StepsStatus.Wait;

    internal StepsStatus AutomaticStatus
    {
        get => _automaticStatus;
        private set => SetAndRaise(AutomaticStatusProperty, ref _automaticStatus, value);
    }

    private StepsStatus _effectiveStatus = StepsStatus.Wait;

    internal StepsStatus EffectiveStatus
    {
        get => _effectiveStatus;
        private set => SetAndRaise(EffectiveStatusProperty, ref _effectiveStatus, value);
    }

    private bool _isFirst;

    internal bool IsFirst
    {
        get => _isFirst;
        private set => SetAndRaise(IsFirstProperty, ref _isFirst, value);
    }

    private bool _isLast;

    internal bool IsLast
    {
        get => _isLast;
        private set => SetAndRaise(IsLastProperty, ref _isLast, value);
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

    internal Steps? Owner { get; private set; }

    internal int ItemIndex { get; private set; } = -1;

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

    #region 内部协作 API

    internal void AttachToOwner(Steps owner, int index)
    {
        Owner = owner;
        ItemIndex = index;
        UpdateCanInvoke();
        UpdateProgressState();
    }

    #endregion

    #region 内部协作 API

    internal void ApplyOwnerState(
        int stepNumber,
        bool isCurrent,
        StepsStatus automaticStatus,
        bool isFirst,
        bool isLast,
        StepsStatus connectorStatus)
    {
        StepNumber = stepNumber;
        IsCurrent = isCurrent;
        AutomaticStatus = automaticStatus;
        EffectiveStatus = Status ?? automaticStatus;
        IsFirst = isFirst;
        IsLast = isLast;
        ConnectorStatus = connectorStatus;
        UpdateProgressState();
    }

    #endregion

    #region 内部协作 API

    internal void DetachFromOwner()
    {
        Owner = null;
        ItemIndex = -1;
        StepNumber = 0;
        IsCurrent = false;
        AutomaticStatus = StepsStatus.Wait;
        EffectiveStatus = StepsStatus.Wait;
        IsFirst = false;
        IsLast = false;
        ConnectorStatus = StepsStatus.Wait;
        CanInvoke = false;
        IsProgressVisible = false;
        IsProgressFrameReserved = false;
    }

    #endregion

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _indicator = null;
        base.OnApplyTemplate(e);
        _indicator = e.NameScope.Find<StepsItemIndicator>("PART_Indicator");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == StatusProperty)
        {
            Owner?.HandleItemStatusChanged(this);
        }
        else if (change.Property == IsClickableProperty ||
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

    #region 实现 IControlThemeGlobalOwnerSource 接口

    // The owning Steps supplies composed item globals. Standalone items retain
    // their own identity; template ancestor bindings cannot express this owner.
    Control? IControlThemeGlobalOwnerSource.ThemeGlobalOwner => Owner;

    #endregion

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
