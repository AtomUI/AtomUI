using System.Collections.Specialized;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace AtomUI.Controls;

[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal)]
public abstract class AbstractSteps : ItemsControl
{
    public static readonly StyledProperty<int> CurrentProperty =
        AvaloniaProperty.Register<AbstractSteps, int>(nameof(Current));

    public static readonly StyledProperty<int> InitialProperty =
        AvaloniaProperty.Register<AbstractSteps, int>(nameof(Initial));

    public static readonly StyledProperty<StepsStatus> StatusProperty =
        AvaloniaProperty.Register<AbstractSteps, StepsStatus>(nameof(Status), StepsStatus.Process);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        ScrollBar.OrientationProperty.AddOwner<AbstractSteps>();

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

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    static AbstractSteps()
    {
        OrientationProperty.OverrideDefaultValue<AbstractSteps>(Orientation.Horizontal);
        AffectsMeasure<AbstractSteps>(OrientationProperty);
    }

    protected AbstractSteps()
    {
        ItemsView.CollectionChanged += HandleItemsCollectionChanged;
        UpdatePseudoClasses();
    }

    protected virtual AbstractStepsItem CreateStepsItem() => new DefaultStepsItem();

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => CreateStepsItem();

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<AbstractStepsItem>(item, out recycleKey);

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is AbstractStepsItem step)
        {
            PrepareStepContainer(step, item, index);
            ApplyItemState(step, index);
            RefreshItemState(index - 1);
        }
    }

    private protected virtual void PrepareStepContainer(AbstractStepsItem step, object? item, int index)
    {
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        if (container is AbstractStepsItem step)
        {
            ApplyItemState(step, newIndex);
            RefreshItemState(oldIndex - 1);
            RefreshItemState(oldIndex);
            RefreshItemState(newIndex - 1);
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        var oldIndex = -1;
        if (container is AbstractStepsItem step)
        {
            oldIndex = step.ItemIndex;
            step.DetachFromOwner();
            OnStepDetached(step, generatedContainer: true);
        }

        base.ClearContainerForItemOverride(container);
        RefreshItemState(oldIndex - 1);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OrientationProperty)
        {
            UpdatePseudoClasses();
        }

        if (change.Property == CurrentProperty || change.Property == InitialProperty || change.Property == StatusProperty)
        {
            RefreshRealizedItems();
        }
    }

    private protected virtual void OnStepStateChanged(AbstractStepsItem item, int index)
    {
    }

    private protected virtual void OnStepDetached(AbstractStepsItem item, bool generatedContainer)
    {
    }

    private protected void RefreshRealizedItems()
    {
        for (var index = 0; index < ItemCount; index++)
        {
            RefreshItemState(index);
        }
    }

    internal void HandleItemStatusChanged(AbstractStepsItem item)
    {
        var index = item.ItemIndex;
        if (index >= 0 && ReferenceEquals(ContainerFromIndex(index), item))
        {
            ApplyItemState(item, index);
            RefreshItemState(index - 1);
        }
    }

    private StepsStatus GetAutomaticStatus(int number) => number == Current ? Status :
        number < Current ? StepsStatus.Finish : StepsStatus.Wait;

    private protected StepsStatus GetEffectiveStepStatus(int index)
    {
        var item = ContainerFromIndex(index) as AbstractStepsItem ?? ItemsView[index] as AbstractStepsItem;
        return item?.Status ?? GetAutomaticStatus(Initial + index);
    }

    private void ApplyItemState(AbstractStepsItem step, int index)
    {
        var number = Initial + index;
        step.ApplyOwnerState(this, index, number, GetAutomaticStatus(number));
        OnStepStateChanged(step, index);
    }

    private void RefreshItemState(int index)
    {
        if (index >= 0 && index < ItemCount && ContainerFromIndex(index) is AbstractStepsItem item)
        {
            ApplyItemState(item, index);
        }
    }

    private void HandleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var item in GetRealizedContainers())
            {
                DetachDirectItem(item as AbstractStepsItem);
            }
        }
        else if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace &&
                 e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                DetachDirectItem(item as AbstractStepsItem);
            }
        }
    }

    private void DetachDirectItem(AbstractStepsItem? item)
    {
        if (item is not null && ReferenceEquals(item.Owner, this))
        {
            item.DetachFromOwner();
            OnStepDetached(item, generatedContainer: false);
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Vertical, Orientation == Orientation.Vertical);
        PseudoClasses.Set(StdPseudoClass.Horizontal, Orientation == Orientation.Horizontal);
    }

    private sealed class DefaultStepsItem : AbstractStepsItem;
}
