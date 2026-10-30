using System.Collections.Specialized;
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
public partial class Steps : ItemsControl,
                              ISizeTypeAware,
                              IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<int> CurrentProperty =
        AvaloniaProperty.Register<Steps, int>(nameof(Current));

    public static readonly StyledProperty<int> InitialProperty =
        AvaloniaProperty.Register<Steps, int>(nameof(Initial));

    public static readonly StyledProperty<StepsStatus> StatusProperty =
        AvaloniaProperty.Register<Steps, StepsStatus>(nameof(Status), StepsStatus.Process);

    public static readonly StyledProperty<double?> PercentProperty =
        AvaloniaProperty.Register<Steps, double?>(nameof(Percent), coerce: CoercePercent);

    public static readonly StyledProperty<StepsType> TypeProperty =
        AvaloniaProperty.Register<Steps, StepsType>(nameof(Type));

    public static readonly StyledProperty<StepsPanelVariant> PanelVariantProperty =
        AvaloniaProperty.Register<Steps, StepsPanelVariant>(nameof(PanelVariant), StepsPanelVariant.Filled);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        ScrollBar.OrientationProperty.AddOwner<Steps>();

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

    public int Current
    {
        get => GetValue(CurrentProperty);
        set => SetValue(CurrentProperty, value);
    }

    public int Initial
    {
        get => GetValue(InitialProperty);
        set => SetValue(InitialProperty, value);
    }

    public StepsStatus Status
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

    public Orientation Orientation
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
        OrientationProperty.OverrideDefaultValue<Steps>(Orientation.Horizontal);
        AffectsMeasure<Steps>(
            TypeProperty,
            PanelVariantProperty,
            OrientationProperty,
            TitlePlacementProperty,
            SizeTypeProperty,
            OffsetProperty);
    }

    public Steps()
    {
        ItemsView.CollectionChanged += HandleItemsCollectionChanged;
        UpdatePseudoClasses();
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new StepsItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<StepsItem>(item, out recycleKey);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

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

        stepsItem.AttachToOwner(this, index);
        ApplyItemState(stepsItem, index);
        RefreshItemState(index - 1);
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);

        if (container is StepsItem stepsItem)
        {
            stepsItem.AttachToOwner(this, newIndex);
            ApplyItemState(stepsItem, newIndex);
            RefreshItemState(oldIndex - 1);
            RefreshItemState(oldIndex);
            RefreshItemState(newIndex - 1);
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        var oldIndex = -1;
        if (container is StepsItem stepsItem)
        {
            oldIndex = stepsItem.ItemIndex;
            stepsItem.DetachFromOwner();
            ClearGeneratedContainerBindings(stepsItem);
        }

        base.ClearContainerForItemOverride(container);
        RefreshItemState(oldIndex - 1);
    }

    private void HandleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var container in GetRealizedContainers())
            {
                DetachDirectItem(container as StepsItem);
            }
        }
        else if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace &&
                 e.OldItems is not null)
        {
            foreach (var oldItem in e.OldItems)
            {
                DetachDirectItem(oldItem as StepsItem);
            }
        }

    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty)
        {
            UpdatePseudoClasses();
        }

        if (change.Property == TypeProperty || change.Property == PanelVariantProperty)
        {
            RefreshRealizedItems();
        }

        if (change.Property == CurrentProperty ||
            change.Property == InitialProperty ||
            change.Property == StatusProperty)
        {
            RefreshRealizedItems();
        }
    }

    internal void HandleItemStatusChanged(StepsItem item)
    {
        var index = item.ItemIndex;
        if (!ReferenceEquals(item.Owner, this) ||
            index < 0 ||
            !ReferenceEquals(ContainerFromIndex(index), item))
        {
            return;
        }

        ApplyItemState(item, index);
        RefreshItemState(index - 1);
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

    private StepsStatus GetAutomaticStatus(int stepNumber)
    {
        if (stepNumber == Current)
        {
            return Status;
        }

        return stepNumber < Current ? StepsStatus.Finish : StepsStatus.Wait;
    }

    private StepsStatus GetEffectiveStatus(int index)
    {
        var stepNumber      = Initial + index;
        var automaticStatus = GetAutomaticStatus(stepNumber);
        return GetItemStatus(index) ?? automaticStatus;
    }

    private StepsStatus? GetItemStatus(int index)
    {
        if (ContainerFromIndex(index) is StepsItem container)
        {
            return container.Status;
        }

        return ItemsView[index] is StepsItem item ? item.Status : null;
    }

    private void ApplyItemState(StepsItem item, int index)
    {
        var stepNumber = Initial + index;
        var automaticStatus = GetAutomaticStatus(stepNumber);
        var effectiveStatus = item.Status ?? automaticStatus;
        var connectorStatus = index < ItemCount - 1
            ? GetEffectiveStatus(index + 1)
            : StepsStatus.Wait;

        item.ApplyOwnerState(
            stepNumber,
            stepNumber == Current,
            automaticStatus,
            index == 0,
            index == ItemCount - 1,
            connectorStatus);
    }

    private void RefreshRealizedItems()
    {
        for (var index = 0; index < ItemCount; index++)
        {
            RefreshItemState(index);
        }
    }

    private void RefreshItemState(int index)
    {
        if (index >= 0 && index < ItemCount && ContainerFromIndex(index) is StepsItem item)
        {
            item.AttachToOwner(this, index);
            ApplyItemState(item, index);
        }
    }

    private void DetachDirectItem(StepsItem? item)
    {
        if (item is null || !ReferenceEquals(item.Owner, this))
        {
            return;
        }

        item.DetachFromOwner();
        ClearOwnerBindings(item);
    }

    private static void ClearGeneratedContainerBindings(StepsItem item)
    {
        ClearOwnerBindings(item);
        item.ClearValue(ContentControl.ContentTemplateProperty);
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

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Vertical, Orientation == Orientation.Vertical);
        PseudoClasses.Set(StdPseudoClass.Horizontal, Orientation == Orientation.Horizontal);
    }
}
