using System.Collections.Specialized;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace AtomUI.Controls;

/// <summary>
/// Base class of an items control that shows its items as a sequence of steps.
/// It gives every item a step container, which is the item itself when it is an
/// <see cref="AbstractStepsItem" /> and otherwise created by this control, and assigns each realized step the state
/// that follows from <see cref="Current" />, <see cref="Initial" /> and <see cref="Status" />: the step number,
/// whether it is the current step and the status it displays.
/// A derived control supplies its item type through <see cref="CreateStepsItem" /> and reacts to the state of a
/// step through <see cref="PrepareStepContainer" />, <see cref="OnStepStateChanged" /> and
/// <see cref="OnStepDetached" />.
/// </summary>
[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal)]
public abstract class AbstractSteps : ItemsControl
{
    /// <summary>
    /// Identifies the <see cref="Current" /> property.
    /// </summary>
    public static readonly StyledProperty<int> CurrentProperty =
        AvaloniaProperty.Register<AbstractSteps, int>(nameof(Current));

    /// <summary>
    /// Identifies the <see cref="Initial" /> property.
    /// </summary>
    public static readonly StyledProperty<int> InitialProperty =
        AvaloniaProperty.Register<AbstractSteps, int>(nameof(Initial));

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<StepsStatus> StatusProperty =
        AvaloniaProperty.Register<AbstractSteps, StepsStatus>(nameof(Status), StepsStatus.Process);

    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        ScrollBar.OrientationProperty.AddOwner<AbstractSteps>();

    /// <summary>
    /// Number of the step that is in progress. A step is current when <see cref="Initial" /> plus its zero-based index
    /// equals this value, so the value uses the same numbering as <see cref="Initial" />.
    /// The current step displays <see cref="Status" />, the steps before it are <see cref="StepsStatus.Finish" /> and
    /// the steps after it are <see cref="StepsStatus.Wait" />, unless an item overrides this with its own
    /// <see cref="AbstractStepsItem.Status" />.
    /// The default is 0. A value that matches no step makes no step current.
    /// </summary>
    public int Current
    {
        get => GetValue(CurrentProperty);
        set => SetValue(CurrentProperty, value);
    }

    /// <summary>
    /// Number of the first item. The step number of an item is this value plus its zero-based index, and
    /// <see cref="Current" /> is compared with that number.
    /// The default is 0.
    /// </summary>
    public int Initial
    {
        get => GetValue(InitialProperty);
        set => SetValue(InitialProperty, value);
    }

    /// <summary>
    /// Status displayed by the current step when the item does not set its own
    /// <see cref="AbstractStepsItem.Status" />. It applies to the current step only; the other steps are finished or
    /// waiting by their position.
    /// The default is <see cref="StepsStatus.Process" />.
    /// </summary>
    public StepsStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Direction in which the steps are laid out, <see cref="Avalonia.Layout.Orientation.Horizontal" /> by default.
    /// Changing the value sets the <c>vertical</c> or <c>horizontal</c> pseudo-class of this control and invalidates
    /// its measure.
    /// </summary>
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

    /// <summary>
    /// Creates the container for one item, which <see cref="CreateContainerForItemOverride" /> returns whenever the
    /// items control needs a new container.
    /// The default returns a plain <see cref="AbstractStepsItem" />; override it to return the item type of the
    /// derived control.
    /// </summary>
    protected virtual AbstractStepsItem CreateStepsItem() => new DefaultStepsItem();

    /// <summary>
    /// Returns the container for an item. The default is <see cref="CreateStepsItem" />, so a derived control changes
    /// the container type by overriding that method, or by replacing this method without calling it.
    /// Return an <see cref="AbstractStepsItem" />: the step state is only applied to such containers, and
    /// <see cref="NeedsContainerOverride" /> must accept the same type.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => CreateStepsItem();

    /// <summary>
    /// Returns whether an item needs a container created by this control. An item that is itself an
    /// <see cref="AbstractStepsItem" /> is used as its own container; any other item gets one from
    /// <see cref="CreateContainerForItemOverride" />.
    /// An override must accept the same type that <see cref="CreateContainerForItemOverride" /> creates.
    /// </summary>
    /// <returns>
    /// <c>true</c> when <paramref name="item" /> is not an <see cref="AbstractStepsItem" />; otherwise <c>false</c>.
    /// </returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<AbstractStepsItem>(item, out recycleKey);

    /// <summary>
    /// Prepares a container for its item and applies the step state to it.
    /// For a container that is an <see cref="AbstractStepsItem" />, it calls <see cref="PrepareStepContainer" />,
    /// applies the state of the item's index, and then refreshes the step before it, because a derived
    /// <see cref="OnStepStateChanged" /> may derive the look of a step from its successor.
    /// An override must call the base implementation; otherwise the step gets no state.
    /// </summary>
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

    /// <summary>
    /// Lets a derived control prepare a step container before its step state is applied.
    /// It is called each time the items control prepares a container for an item, both for an item that is its own
    /// container and for a generated container. The default does nothing.
    /// </summary>
    /// <param name="step">
    /// The step container.
    /// </param>
    /// <param name="item">
    /// The item the container is prepared for. It is <paramref name="step" /> itself when the item is its own
    /// container, and otherwise the data item.
    /// </param>
    /// <param name="index">
    /// The zero-based index of the item.
    /// </param>
    private protected virtual void PrepareStepContainer(AbstractStepsItem step, object? item, int index)
    {
    }

    /// <summary>
    /// Updates a step whose index changed. It applies the state of the new index to the container, then refreshes the
    /// steps that are now at the old index and before the old and the new index.
    /// An override must call the base implementation; otherwise the step keeps the state of its old index.
    /// </summary>
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

    /// <summary>
    /// Undoes the preparation of a container when the items control releases it.
    /// For a step container it detaches the step from this control, which resets the state of the step and raises
    /// <see cref="OnStepDetached" /> with <c>generatedContainer</c> set to <c>true</c>, and then refreshes the step
    /// before the index the container had.
    /// An override must call the base implementation; otherwise the step keeps its state.
    /// </summary>
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

    /// <summary>
    /// Updates the pseudo-classes when <see cref="Orientation" /> changes, and re-applies the state of every realized
    /// step when <see cref="Current" />, <see cref="Initial" /> or <see cref="Status" /> changes.
    /// An override must call the base implementation; otherwise the steps keep their stale state.
    /// </summary>
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

    /// <summary>
    /// Called after the state that the owner assigns to a step has been applied: its step number, current flag,
    /// automatic status, effective status and first and last flags.
    /// It runs every time that state is applied, also when no value changed. A derived control uses it to derive
    /// further state of the step from its position and from the state of the other steps. The default does nothing.
    /// </summary>
    /// <param name="item">
    /// The step whose state was applied.
    /// </param>
    /// <param name="index">
    /// The zero-based index of the step.
    /// </param>
    private protected virtual void OnStepStateChanged(AbstractStepsItem item, int index)
    {
    }

    /// <summary>
    /// Called when a step is detached from this control, after the state it received from the owner has been reset.
    /// A derived control removes here what it set up in <see cref="PrepareStepContainer" />. The default does nothing.
    /// </summary>
    /// <param name="item">
    /// The step that is detached.
    /// </param>
    /// <param name="generatedContainer">
    /// <c>true</c> when the items control releases a container, as in
    /// <see cref="ClearContainerForItemOverride" />; <c>false</c> when the items collection is reset, or when an item
    /// is removed from it or replaced in it.
    /// </param>
    private protected virtual void OnStepDetached(AbstractStepsItem item, bool generatedContainer)
    {
    }

    /// <summary>
    /// Re-applies the state that the owner assigns to every realized step container. A derived control calls it when
    /// one of its own properties changes the state that it derives in <see cref="OnStepStateChanged" />.
    /// </summary>
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

    /// <summary>
    /// Returns the status displayed by the step at an index: the step's own <see cref="AbstractStepsItem.Status" />
    /// when that is set, and otherwise the status that follows from <see cref="Current" /> and <see cref="Status" />.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the step.
    /// </param>
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
