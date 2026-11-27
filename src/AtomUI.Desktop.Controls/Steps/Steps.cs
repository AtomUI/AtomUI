using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Desktop steps control: shows its items as a sequence of steps and marks the progress through them with
/// <see cref="Current" />. The form of the steps is chosen with <see cref="Type" /> and their direction with
/// <see cref="Orientation" />.
/// Each item is presented by a <see cref="StepsItem" /> container, and the appearance values set on the
/// control are forwarded to every step.
/// Activating a step does not change <see cref="Current" />: while <see cref="IsItemClickable" /> is
/// <see langword="true" />, the control raises <see cref="CurrentChangeRequested" /> and the application
/// decides whether to move <see cref="Current" />.
/// </summary>
[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal)]
public partial class Steps : AbstractSteps,
                              ISizeTypeAware,
                              IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Current" /> property.
    /// </summary>
    public new static readonly StyledProperty<int> CurrentProperty =
        AbstractSteps.CurrentProperty.AddOwner<Steps>();

    /// <summary>
    /// Identifies the <see cref="Initial" /> property.
    /// </summary>
    public new static readonly StyledProperty<int> InitialProperty =
        AbstractSteps.InitialProperty.AddOwner<Steps>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public new static readonly StyledProperty<StepsStatus> StatusProperty =
        AbstractSteps.StatusProperty.AddOwner<Steps>();

    /// <summary>
    /// Identifies the <see cref="Percent" /> property.
    /// </summary>
    public static readonly StyledProperty<double?> PercentProperty =
        AvaloniaProperty.Register<Steps, double?>(nameof(Percent), coerce: CoercePercent);

    /// <summary>
    /// Identifies the <see cref="Type" /> property.
    /// </summary>
    public static readonly StyledProperty<StepsType> TypeProperty =
        AvaloniaProperty.Register<Steps, StepsType>(nameof(Type));

    /// <summary>
    /// Identifies the <see cref="PanelVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<StepsPanelVariant> PanelVariantProperty =
        AvaloniaProperty.Register<Steps, StepsPanelVariant>(nameof(PanelVariant), StepsPanelVariant.Filled);

    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public new static readonly StyledProperty<Orientation> OrientationProperty =
        AbstractSteps.OrientationProperty.AddOwner<Steps>();

    /// <summary>
    /// Identifies the <see cref="TitlePlacement" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> TitlePlacementProperty =
        AvaloniaProperty.Register<Steps, Orientation>(nameof(TitlePlacement), Orientation.Horizontal);

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Steps>();

    /// <summary>
    /// Identifies the <see cref="IsItemClickable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsItemClickableProperty =
        AvaloniaProperty.Register<Steps, bool>(nameof(IsItemClickable));

    /// <summary>
    /// Identifies the <see cref="Offset" /> property.
    /// </summary>
    public static readonly StyledProperty<int> OffsetProperty =
        AvaloniaProperty.Register<Steps, int>(nameof(Offset), coerce: CoerceOffset);

    /// <summary>
    /// Identifies the <see cref="HorizontalContentAlignment" /> property.
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<Steps, HorizontalAlignment>(
            nameof(HorizontalContentAlignment),
            HorizontalAlignment.Center);

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Steps>();

    /// <summary>
    /// Identifies the <see cref="ItemHeaderForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ItemHeaderForegroundProperty =
        AvaloniaProperty.Register<Steps, IBrush?>(nameof(ItemHeaderForeground));

    /// <summary>
    /// Identifies the <see cref="ItemSubHeaderForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ItemSubHeaderForegroundProperty =
        AvaloniaProperty.Register<Steps, IBrush?>(nameof(ItemSubHeaderForeground));

    /// <summary>
    /// Identifies the <see cref="ItemRailBackground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ItemRailBackgroundProperty =
        AvaloniaProperty.Register<Steps, IBrush?>(nameof(ItemRailBackground));

    /// <summary>
    /// Identifies the <see cref="BorderDashArray" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<double>?> BorderDashArrayProperty =
        AvaloniaProperty.Register<Steps, IReadOnlyList<double>?>(nameof(BorderDashArray));

    /// <summary>
    /// Identifies the <see cref="BorderDashOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<double> BorderDashOffsetProperty =
        AvaloniaProperty.Register<Steps, double>(nameof(BorderDashOffset));

    /// <summary>
    /// Number of the step that is in progress. A step is current when <see cref="Initial" /> plus its zero-based
    /// index equals this value, so the value uses the same numbering as <see cref="Initial" />.
    /// The current step displays <see cref="Status" />, the steps before it are <see cref="StepsStatus.Finish" />
    /// and the steps after it are <see cref="StepsStatus.Wait" />, unless an item overrides this with its own
    /// status.
    /// The default is 0. A value that matches no step makes no step current.
    /// This is the same property as <see cref="AbstractSteps.Current" />: both declarations read and write one
    /// value.
    /// </summary>
    public new int Current
    {
        get => GetValue(CurrentProperty);
        set => SetValue(CurrentProperty, value);
    }

    /// <summary>
    /// Number of the first step. The step number of an item is this value plus its zero-based index, and
    /// <see cref="Current" /> is compared with that number.
    /// The default is 0. This is the same property as <see cref="AbstractSteps.Initial" />: both declarations read
    /// and write one value.
    /// </summary>
    public new int Initial
    {
        get => GetValue(InitialProperty);
        set => SetValue(InitialProperty, value);
    }

    /// <summary>
    /// Status displayed by the current step when the item does not set its own
    /// <see cref="AbstractStepsItem.Status" />. It applies to the current step only; the other steps are finished
    /// or waiting by their position.
    /// The default is <see cref="StepsStatus.Process" />. This is the same property as
    /// <see cref="AbstractSteps.Status" />: both declarations read and write one value.
    /// </summary>
    public new StepsStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Progress of the current step in percent, from 0 to 100, or <see langword="null" /> to show no progress.
    /// The value is forwarded to every step, but only the current step displays it, as an arc in the indicator
    /// while its status is <see cref="StepsStatus.Process" /> and the item has no icon, in the
    /// <see cref="StepsType.Default" /> and <see cref="StepsType.Navigation" /> forms.
    /// A value outside 0 to 100 is clamped to that range; a value that is NaN or infinite becomes
    /// <see langword="null" />.
    /// </summary>
    public double? Percent
    {
        get => GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    /// <summary>
    /// Form of the steps, for example a numbered indicator with a connector, a dot or arrow-shaped panels.
    /// The value is forwarded to every step and to the panel that arranges them. A change refreshes the realized
    /// items, so that their step containers are prepared again for the new form.
    /// The default is <see cref="StepsType.Default" />.
    /// </summary>
    public StepsType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>
    /// Fill style of the step panels: filled with the color of their status, or outlined. It only applies in the
    /// <see cref="StepsType.Panel" /> form; the other forms ignore it.
    /// The value is forwarded to every step, and a change refreshes the realized items, so that their step
    /// containers are prepared again. The default is <see cref="StepsPanelVariant.Filled" />.
    /// </summary>
    public StepsPanelVariant PanelVariant
    {
        get => GetValue(PanelVariantProperty);
        set => SetValue(PanelVariantProperty, value);
    }

    /// <summary>
    /// Direction in which the steps are laid out, <see cref="Avalonia.Layout.Orientation.Horizontal" /> by default.
    /// The value is forwarded to every step and to the panel that arranges them. A change sets the
    /// <c>vertical</c> or <c>horizontal</c> pseudo-class and measures the control again.
    /// This is the same property as <see cref="AbstractSteps.Orientation" />: both declarations read and write one
    /// value.
    /// </summary>
    public new Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Placement of the title and the description of a step in relation to its indicator: beside the indicator, or
    /// below it, so that the indicator stands above the title.
    /// The value is forwarded to every step and to the panel that arranges them. The default is
    /// <see cref="Avalonia.Layout.Orientation.Horizontal" />.
    /// </summary>
    public Orientation TitlePlacement
    {
        get => GetValue(TitlePlacementProperty);
        set => SetValue(TitlePlacementProperty, value);
    }

    /// <summary>
    /// Preset size of the steps: <c>Large</c>, <c>Middle</c> or <c>Small</c>. The theme derives the size of the
    /// indicator and the layout of a step from it.
    /// The value is forwarded to every step. It does not inherit and the default is <c>Middle</c>.
    /// </summary>
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Lets the user activate a step with the pointer or the keyboard while <see langword="true" />.
    /// Activating a step raises <see cref="CurrentChangeRequested" />; the control does not change
    /// <see cref="Current" /> itself, so the application sets it in the event handler.
    /// The value is forwarded to every step. The default is <see langword="false" />.
    /// </summary>
    public bool IsItemClickable
    {
        get => GetValue(IsItemClickableProperty);
        set => SetValue(IsItemClickableProperty, value);
    }

    /// <summary>
    /// Number of step widths of empty space to leave before the first step. It only applies in the
    /// <see cref="StepsType.Inline" /> form; the other forms ignore it.
    /// A negative value is treated as 0, which is also the default.
    /// </summary>
    public int Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    /// <summary>
    /// Horizontal placement of the steps inside the control when they do not fill its width: <c>Left</c>,
    /// <c>Center</c>, <c>Right</c>, or <c>Stretch</c> to make each step as wide as the control. The default is
    /// <c>Center</c>.
    /// </summary>
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Lets the steps animate their state changes while <see langword="true" />; while it is
    /// <see langword="false" />, a change takes effect at once, without a transition.
    /// The value is forwarded to every step. The theme takes it from the <c>EnableMotion</c> token; a value set on
    /// the control replaces that, and without either it is <see langword="false" />.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Foreground of the title of every step. The value is forwarded to each step and replaces the foreground of
    /// its theme. <see langword="null" />, the default, leaves the foreground to the theme of the step.
    /// </summary>
    public IBrush? ItemHeaderForeground
    {
        get => GetValue(ItemHeaderForegroundProperty);
        set => SetValue(ItemHeaderForegroundProperty, value);
    }

    /// <summary>
    /// Foreground of the description (the sub-header) of every step. The value is forwarded to each step and
    /// replaces the foreground of its theme. <see langword="null" />, the default, leaves the foreground to the
    /// theme of the step.
    /// </summary>
    public IBrush? ItemSubHeaderForeground
    {
        get => GetValue(ItemSubHeaderForegroundProperty);
        set => SetValue(ItemSubHeaderForegroundProperty, value);
    }

    /// <summary>
    /// Background of the connector line of every step. The value is forwarded to each step and replaces the
    /// background of its theme. <see langword="null" />, the default, leaves the background to the theme of the
    /// step.
    /// </summary>
    public IBrush? ItemRailBackground
    {
        get => GetValue(ItemRailBackgroundProperty);
        set => SetValue(ItemRailBackgroundProperty, value);
    }

    /// <summary>
    /// Dash pattern of the border that the template draws around the control, as the alternating lengths of dashes
    /// and gaps. <see langword="null" />, the default, draws the border without dashes.
    /// </summary>
    public IReadOnlyList<double>? BorderDashArray
    {
        get => GetValue(BorderDashArrayProperty);
        set => SetValue(BorderDashArrayProperty, value);
    }

    /// <summary>
    /// Position within the dash pattern of <see cref="BorderDashArray" /> at which the border of the control
    /// starts. The default is 0, the start of the pattern.
    /// </summary>
    public double BorderDashOffset
    {
        get => GetValue(BorderDashOffsetProperty);
        set => SetValue(BorderDashOffsetProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when the user activates a step with the pointer or the keyboard while <see cref="IsItemClickable" />
    /// is <see langword="true" />. The event data carries the number of the activated step.
    /// The control does not change <see cref="Current" />: the handler decides whether to assign that number to
    /// <see cref="Current" />.
    /// No event is raised for the current step, or while the control or the step is disabled.
    /// </summary>
    public event EventHandler<StepsCurrentChangeRequestedEventArgs>? CurrentChangeRequested;

    #endregion

    static Steps()
    {
        AffectsMeasure<Steps>(
            TypeProperty,
            PanelVariantProperty,
            OrientationProperty,
            TitlePlacementProperty,
            SizeTypeProperty,
            OffsetProperty);
    }

    /// <summary>
    /// Creates the container for an item that is not a <see cref="StepsItem" /> itself: a new
    /// <see cref="StepsItem" /> every time. It replaces the base implementation without calling it.
    /// An override must return a <see cref="StepsItem" />, because preparing any other container type throws, and
    /// <see cref="NeedsContainerOverride" /> must accept the same type.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new StepsItem();
    }

    /// <summary>
    /// Returns whether an item needs a container from this control. An item that is itself a
    /// <see cref="StepsItem" /> is used as its own container; any other item gets one from
    /// <see cref="CreateContainerForItemOverride" />.
    /// An override must accept exactly the type that <see cref="CreateContainerForItemOverride" /> creates.
    /// </summary>
    /// <returns>
    /// <c>true</c> when <paramref name="item" /> is not a <see cref="StepsItem" />; otherwise <c>false</c>.
    /// </returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<StepsItem>(item, out recycleKey);
    }

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index) =>
        base.PrepareContainerForItemOverride(container, item, index);

    /// <summary>
    /// Adds the semantic item class to a step container and binds the type, the panel variant, the
    /// orientation, the title placement, the size, the clickability, the motion flag, the percent value and
    /// the three item brushes of the step to the properties of this control. For a generated container it
    /// also clears the header and the header template and binds the content template to <c>ItemTemplate</c>.
    /// <see cref="OnStepDetached" /> removes the bindings again.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="container" /> is not a <see cref="StepsItem" />.
    /// </exception>
    private protected override void PrepareStepContainer(AbstractStepsItem container, object? item, int index)
    {
        if (container is not StepsItem stepsItem)
        {
            throw new ArgumentOutOfRangeException(
                nameof(container),
                "The container type is incorrect, it must be type StepsItem.");
        }

        stepsItem.Classes.Add(StepsSemanticParts.ItemClass);

        var isGenerated = !ReferenceEquals(container, item);
        if (isGenerated)
        {
            stepsItem.ClearValue(HeaderedContentControl.HeaderProperty);
            stepsItem.ClearValue(HeaderedContentControl.HeaderTemplateProperty);
            stepsItem[!ContentControl.ContentTemplateProperty] = this[!ItemTemplateProperty];
        }

        stepsItem[!StepsItem.TypeProperty] = this[!TypeProperty];
        stepsItem[!StepsItem.PanelVariantProperty] = this[!PanelVariantProperty];
        stepsItem[!StepsItem.OrientationProperty] = this[!OrientationProperty];
        stepsItem[!StepsItem.TitlePlacementProperty] = this[!TitlePlacementProperty];
        stepsItem[!StepsItem.SizeTypeProperty] = this[!SizeTypeProperty];
        stepsItem[!StepsItem.IsClickableProperty] = this[!IsItemClickableProperty];
        stepsItem[!StepsItem.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        stepsItem[!StepsItem.PercentProperty] = this[!PercentProperty];
        stepsItem[!StepsItem.ItemHeaderForegroundProperty] = this[!ItemHeaderForegroundProperty];
        stepsItem[!StepsItem.ItemSubHeaderForegroundProperty] = this[!ItemSubHeaderForegroundProperty];
        stepsItem[!StepsItem.ItemRailBackgroundProperty] = this[!ItemRailBackgroundProperty];
    }

    /// <inheritdoc />
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex) =>
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);

    /// <inheritdoc />
    protected override void ClearContainerForItemOverride(Control container) => base.ClearContainerForItemOverride(container);

    /// <summary>
    /// Calls the base method first, then applies the state of every realized step again after
    /// <see cref="Type" /> or <see cref="PanelVariant" /> changed.
    /// An override must call the base method.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TypeProperty || change.Property == PanelVariantProperty)
        {
            RefreshRealizedItems();
        }
    }

    internal void RequestCurrentChange(StepsItem item)
    {
        if (!ReferenceEquals(item.Owner, this) ||
            !IsItemClickable ||
            !IsEffectivelyEnabled ||
            !item.CanInvoke ||
            item.StepNumber == Current)
        {
            return;
        }

        CurrentChangeRequested?.Invoke(
            this,
            new StepsCurrentChangeRequestedEventArgs(item.StepNumber));
    }

    private static double? CoercePercent(AvaloniaObject sender, double? value)
    {
        if (!value.HasValue || !double.IsFinite(value.Value))
        {
            return null;
        }

        return Math.Clamp(value.Value, 0d, 100d);
    }

    private static int CoerceOffset(AvaloniaObject sender, int value)
    {
        return Math.Max(0, value);
    }

    /// <summary>
    /// Applies to the step the status of the connector after it: the effective status of the next step, or
    /// <see cref="StepsStatus.Wait" /> behind the last step. The step then updates whether it can be invoked
    /// and the circular progress of its indicator.
    /// </summary>
    private protected override void OnStepStateChanged(AbstractStepsItem item, int index)
    {
        var connectorStatus = index < ItemCount - 1 ? GetEffectiveStepStatus(index + 1) : StepsStatus.Wait;
        ((StepsItem)item).ApplyDesktopConnectorStatus(connectorStatus);
    }

    /// <summary>
    /// Removes the bindings that <see cref="PrepareStepContainer" /> added to the step and, for a generated
    /// container, the binding of the content template to the item template as well.
    /// </summary>
    private protected override void OnStepDetached(AbstractStepsItem item, bool generatedContainer)
    {
        ClearOwnerBindings((StepsItem)item);
        if (generatedContainer)
        {
            item.ClearValue(ContentControl.ContentTemplateProperty);
        }
    }

    private static void ClearOwnerBindings(StepsItem item)
    {
        item.ClearValue(StepsItem.TypeProperty);
        item.ClearValue(StepsItem.PanelVariantProperty);
        item.ClearValue(StepsItem.OrientationProperty);
        item.ClearValue(StepsItem.TitlePlacementProperty);
        item.ClearValue(StepsItem.SizeTypeProperty);
        item.ClearValue(StepsItem.IsClickableProperty);
        item.ClearValue(StepsItem.IsMotionEnabledProperty);
        item.ClearValue(StepsItem.PercentProperty);
        item.ClearValue(StepsItem.ItemHeaderForegroundProperty);
        item.ClearValue(StepsItem.ItemSubHeaderForegroundProperty);
        item.ClearValue(StepsItem.ItemRailBackgroundProperty);
    }
}
