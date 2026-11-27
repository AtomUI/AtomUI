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

/// <summary>
/// One step of a <see cref="Steps" /> control: the container that the control creates for each of its items.
/// It adds the layout state that the owner assigns to the header, sub-header, icon and status of
/// <see cref="AbstractStepsItem" />, and forwards a click or an Enter or Space key press on an invocable step
/// to the owner as a request to make the step current.
/// </summary>
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

    /// <summary>
    /// Identifies the <see cref="SubHeader" /> property.
    /// </summary>
    public new static readonly StyledProperty<object?> SubHeaderProperty =
        AbstractStepsItem.SubHeaderProperty.AddOwner<StepsItem>();

    /// <summary>
    /// Identifies the <see cref="SubHeaderTemplate" /> property.
    /// </summary>
    public new static readonly StyledProperty<IDataTemplate?> SubHeaderTemplateProperty =
        AbstractStepsItem.SubHeaderTemplateProperty.AddOwner<StepsItem>();

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public new static readonly StyledProperty<PathIcon?> IconProperty =
        AbstractStepsItem.IconProperty.AddOwner<StepsItem>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public new static readonly StyledProperty<StepsStatus?> StatusProperty =
        AbstractStepsItem.StatusProperty.AddOwner<StepsItem>();

    /// <summary>
    /// Secondary text of the step, shown in addition to the title only while this value is not
    /// <see langword="null" />, the default. A string is shown as a wrapping text block; any other object is
    /// presented with <see cref="SubHeaderTemplate" />.
    /// This is the same property as <see cref="AbstractStepsItem.SubHeader" />: both declarations read and write
    /// one value.
    /// </summary>
    public new object? SubHeader
    {
        get => GetValue(SubHeaderProperty);
        set => SetValue(SubHeaderProperty, value);
    }

    /// <summary>
    /// Template that presents <see cref="SubHeader" /> when it is not a string; a string is shown as a text block
    /// without it. When this is <see langword="null" />, the default, the presenter uses the data template it finds
    /// for the value.
    /// This is the same property as <see cref="AbstractStepsItem.SubHeaderTemplate" />: both declarations read and
    /// write one value.
    /// </summary>
    public new IDataTemplate? SubHeaderTemplate
    {
        get => GetValue(SubHeaderTemplateProperty);
        set => SetValue(SubHeaderTemplateProperty, value);
    }

    /// <summary>
    /// Custom icon shown in the step indicator in place of the step number, the finish mark and the error mark.
    /// The default is <see langword="null" />, which keeps those built-in marks. While an icon is set, the
    /// current step in progress shows no circular progress. A step of the <see cref="StepsType.Panel" /> form
    /// shows no indicator and therefore no icon.
    /// This is the same property as <see cref="AbstractStepsItem.Icon" />: both declarations read and write one
    /// value.
    /// </summary>
    public new PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Status displayed by this step when it must differ from the status that follows from its position. When it
    /// is <see langword="null" />, the default, the owning steps control derives the status: the current step shows
    /// the status of the control, the steps before it are finished and the steps after it are waiting. A change
    /// makes the owner apply the state of this step and of the step before it again.
    /// This is the same property as <see cref="AbstractStepsItem.Status" />: both declarations read and write one
    /// value.
    /// </summary>
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

    /// <summary>
    /// Resets the state that depends on the owner: the connector status becomes
    /// <see cref="StepsStatus.Wait" />, the step is no longer invocable, the circular progress of the
    /// indicator is not visible and its frame is not reserved. The base method is empty.
    /// </summary>
    private protected override void OnOwnerDetached()
    {
        ConnectorStatus = StepsStatus.Wait;
        CanInvoke = false;
        IsProgressVisible = false;
        IsProgressFrameReserved = false;
    }

    /// <summary>
    /// Calls the base member and then looks up the indicator of the new template; the indicator of the previous
    /// template is dropped first. While no template with an indicator is applied, the step shows no hover state
    /// on the indicator and plays no activation wave.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _indicator = null;
        base.OnApplyTemplate(e);
        _indicator = e.NameScope.Find<StepsItemIndicator>("PART_Indicator");
    }

    /// <summary>
    /// Calls the base member, which notifies the owner of a change of <see cref="Status" />, and then updates the
    /// state that depends on the changed property: whether the step can be invoked, the circular progress state
    /// of the indicator, the hover state of a step that becomes current, and the pseudo-classes of the item
    /// brush overrides.
    /// An override must call the base member; otherwise the owner is not notified.
    /// </summary>
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

    /// <summary>
    /// Calls the base member, then shows the hover state on the indicator when the step can be invoked and is not
    /// the current step.
    /// </summary>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (_indicator is not null && CanInvoke && !IsCurrent)
        {
            _indicator.IsItemHover = true;
        }
    }

    /// <summary>
    /// Calls the base member, then captures the pointer, stops gesture recognition and marks the press handled
    /// when the step can be invoked and the left button is pressed, so that the matching release reaches
    /// <see cref="OnPointerReleased" />.
    /// </summary>
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

    /// <summary>
    /// Calls the base member, then ends a press that captured the pointer: it releases the capture and marks the
    /// release handled. When the press started with the left button, the pointer is still inside the step and
    /// the step can still be invoked, it asks the owner to make this step current. A release of a pointer that
    /// the step did not capture is ignored.
    /// </summary>
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

    /// <summary>
    /// Clears the hover state of the indicator before the base member is called.
    /// </summary>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_indicator is not null)
        {
            _indicator.IsItemHover = false;
        }

        base.OnPointerCaptureLost(e);
    }

    /// <summary>
    /// Marks Enter and Space as handled before the base member is called when the step can be invoked. The request
    /// to change the current step is made in <see cref="OnKeyUp" />, not here.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (CanInvoke && e.Key is Key.Enter or Key.Space)
        {
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    /// <summary>
    /// Asks the owner to make this step current when Enter or Space is released and the step can be invoked, and
    /// marks the key as handled before the base member is called.
    /// </summary>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (CanInvoke && e.Key is Key.Enter or Key.Space)
        {
            InvokeFromKeyboard();
            e.Handled = true;
        }

        base.OnKeyUp(e);
    }

    /// <summary>
    /// Calls the base member, then clears the hover state of the indicator.
    /// </summary>
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
