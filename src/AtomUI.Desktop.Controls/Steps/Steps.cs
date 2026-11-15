using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal)]
public partial class Steps : AbstractSteps,
                              ISizeTypeAware,
                              IMotionAwareControl
{
    #region 公共属性定义

    public new static readonly StyledProperty<int> CurrentProperty =
        AbstractSteps.CurrentProperty.AddOwner<Steps>();

    public new static readonly StyledProperty<int> InitialProperty =
        AbstractSteps.InitialProperty.AddOwner<Steps>();

    public new static readonly StyledProperty<StepsStatus> StatusProperty =
        AbstractSteps.StatusProperty.AddOwner<Steps>();

    public static readonly StyledProperty<double?> PercentProperty =
        AvaloniaProperty.Register<Steps, double?>(nameof(Percent), coerce: CoercePercent);

    public static readonly StyledProperty<StepsType> TypeProperty =
        AvaloniaProperty.Register<Steps, StepsType>(nameof(Type));

    public static readonly StyledProperty<StepsPanelVariant> PanelVariantProperty =
        AvaloniaProperty.Register<Steps, StepsPanelVariant>(nameof(PanelVariant), StepsPanelVariant.Filled);

    public new static readonly StyledProperty<Orientation> OrientationProperty =
        AbstractSteps.OrientationProperty.AddOwner<Steps>();

    public static readonly StyledProperty<Orientation> TitlePlacementProperty =
        AvaloniaProperty.Register<Steps, Orientation>(nameof(TitlePlacement), Orientation.Horizontal);

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Steps>();

    public static readonly StyledProperty<bool> IsItemClickableProperty =
        AvaloniaProperty.Register<Steps, bool>(nameof(IsItemClickable));

    public static readonly StyledProperty<int> OffsetProperty =
        AvaloniaProperty.Register<Steps, int>(nameof(Offset), coerce: CoerceOffset);

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<Steps, HorizontalAlignment>(
            nameof(HorizontalContentAlignment),
            HorizontalAlignment.Center);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Steps>();

    public static readonly StyledProperty<IBrush?> ItemHeaderForegroundProperty =
        AvaloniaProperty.Register<Steps, IBrush?>(nameof(ItemHeaderForeground));

    public static readonly StyledProperty<IBrush?> ItemSubHeaderForegroundProperty =
        AvaloniaProperty.Register<Steps, IBrush?>(nameof(ItemSubHeaderForeground));

    public static readonly StyledProperty<IBrush?> ItemRailBackgroundProperty =
        AvaloniaProperty.Register<Steps, IBrush?>(nameof(ItemRailBackground));

    public static readonly StyledProperty<IReadOnlyList<double>?> BorderDashArrayProperty =
        AvaloniaProperty.Register<Steps, IReadOnlyList<double>?>(nameof(BorderDashArray));

    public static readonly StyledProperty<double> BorderDashOffsetProperty =
        AvaloniaProperty.Register<Steps, double>(nameof(BorderDashOffset));

    public new int Current
    {
        get => GetValue(CurrentProperty);
        set => SetValue(CurrentProperty, value);
    }

    public new int Initial
    {
        get => GetValue(InitialProperty);
        set => SetValue(InitialProperty, value);
    }

    public new StepsStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public double? Percent
    {
        get => GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    public StepsType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public StepsPanelVariant PanelVariant
    {
        get => GetValue(PanelVariantProperty);
        set => SetValue(PanelVariantProperty, value);
    }

    public new Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public Orientation TitlePlacement
    {
        get => GetValue(TitlePlacementProperty);
        set => SetValue(TitlePlacementProperty, value);
    }

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public bool IsItemClickable
    {
        get => GetValue(IsItemClickableProperty);
        set => SetValue(IsItemClickableProperty, value);
    }

    public int Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public IBrush? ItemHeaderForeground
    {
        get => GetValue(ItemHeaderForegroundProperty);
        set => SetValue(ItemHeaderForegroundProperty, value);
    }

    public IBrush? ItemSubHeaderForeground
    {
        get => GetValue(ItemSubHeaderForegroundProperty);
        set => SetValue(ItemSubHeaderForegroundProperty, value);
    }

    public IBrush? ItemRailBackground
    {
        get => GetValue(ItemRailBackgroundProperty);
        set => SetValue(ItemRailBackgroundProperty, value);
    }

    public IReadOnlyList<double>? BorderDashArray
    {
        get => GetValue(BorderDashArrayProperty);
        set => SetValue(BorderDashArrayProperty, value);
    }

    public double BorderDashOffset
    {
        get => GetValue(BorderDashOffsetProperty);
        set => SetValue(BorderDashOffsetProperty, value);
    }

    #endregion

    #region 公共事件定义

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

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new StepsItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<StepsItem>(item, out recycleKey);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index) =>
        base.PrepareContainerForItemOverride(container, item, index);

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

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex) =>
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);

    protected override void ClearContainerForItemOverride(Control container) => base.ClearContainerForItemOverride(container);

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

    private protected override void OnStepStateChanged(AbstractStepsItem item, int index)
    {
        var connectorStatus = index < ItemCount - 1 ? GetEffectiveStepStatus(index + 1) : StepsStatus.Wait;
        ((StepsItem)item).ApplyDesktopConnectorStatus(connectorStatus);
    }

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
