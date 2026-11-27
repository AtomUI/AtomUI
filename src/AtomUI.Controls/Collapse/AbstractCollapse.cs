using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Controls;

/// <summary>
/// Base class of controls that show a list of collapsible panels, each of which is expanded while it is selected.
/// </summary>
/// <remarks>
/// An item that is itself an <see cref="AbstractCollapseItem" /> is used as its own panel. Any other item gets a
/// panel from <see cref="CreateCollapseItem" />, which <see cref="PrepareContainerForItemOverride" /> fills with
/// the data of the item.
/// </remarks>
public abstract class AbstractCollapse : SelectingItemsControl
{
    /// <summary>
    /// Identifies the <see cref="IsAccordion" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAccordionProperty =
        AvaloniaProperty.Register<AbstractCollapse, bool>(nameof(IsAccordion));

    /// <summary>
    /// Whether the panels work as an accordion, so that at most one panel is expanded at a time. The default is
    /// <see langword="false" />, which allows several panels to be expanded at once.
    /// </summary>
    /// <remarks>
    /// Changing the value switches the control between single and multiple selection. In accordion mode a selection
    /// of several panels is reduced to the first one when containers are prepared and when the control is attached
    /// to the visual tree.
    /// </remarks>
    public bool IsAccordion
    {
        get => GetValue(IsAccordionProperty);
        set => SetValue(IsAccordionProperty, value);
    }

    static AbstractCollapse()
    {
        SelectionModeProperty.OverrideDefaultValue<AbstractCollapse>(SelectionMode.Multiple | SelectionMode.Toggle);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<AbstractCollapse>(false);
    }

    protected AbstractCollapse()
    {
        SetupSelectionMode();
    }

    /// <summary>
    /// Creates a panel for an item that is not an <see cref="AbstractCollapseItem" /> itself.
    /// </summary>
    /// <returns>
    /// A new, empty panel. The default is a bare <see cref="AbstractCollapseItem" />; a derived class overrides
    /// this to return its own panel type.
    /// </returns>
    protected virtual AbstractCollapseItem CreateCollapseItem() => new DefaultCollapseItem();

    /// <summary>
    /// Creates the panel for an item that needs one by calling <see cref="CreateCollapseItem" />.
    /// </summary>
    /// <param name="item">
    /// The item to display.
    /// </param>
    /// <param name="index">
    /// The index of the item.
    /// </param>
    /// <param name="recycleKey">
    /// The recycle key of the item, see <see cref="NeedsContainerOverride" />.
    /// </param>
    /// <returns>
    /// The panel that displays the item.
    /// </returns>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => CreateCollapseItem();

    /// <summary>
    /// Decides whether an item needs a panel. An item that is itself an <see cref="AbstractCollapseItem" /> is used
    /// as its own container; every other item needs one.
    /// </summary>
    /// <param name="item">
    /// The item to check.
    /// </param>
    /// <param name="index">
    /// The index of the item.
    /// </param>
    /// <param name="recycleKey">
    /// <see langword="null" /> when the item is its own container; otherwise
    /// <see cref="ItemsControl.DefaultRecycleKey" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the item needs a container; otherwise <see langword="false" />.
    /// </returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<AbstractCollapseItem>(item, out recycleKey);

    /// <summary>
    /// Fills a panel with the data of its item: the item becomes the content of the panel, and an
    /// <see cref="ICollapseItemData" /> also supplies its header, its selected state and whether it shows the
    /// expand icon.
    /// </summary>
    /// <param name="container">
    /// The panel that displays the item.
    /// </param>
    /// <param name="item">
    /// The item to display.
    /// </param>
    /// <param name="index">
    /// The index of the item.
    /// </param>
    /// <remarks>
    /// Only a panel that is an <see cref="AbstractCollapseItem" /> and an item that is neither
    /// <see langword="null" /> nor a <c>Visual</c> are handled. Each value is applied with <c>SetCurrentValue</c>
    /// and only when the panel does not have that property set.
    /// </remarks>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (container is not AbstractCollapseItem collapseItem)
        {
            return;
        }

        if (item != null && item is not Visual)
        {
            if (!collapseItem.IsSet(AbstractCollapseItem.ContentProperty))
            {
                collapseItem.SetCurrentValue(AbstractCollapseItem.ContentProperty, item);
            }

            if (item is ICollapseItemData itemData)
            {
                if (!collapseItem.IsSet(AbstractCollapseItem.HeaderProperty))
                {
                    collapseItem.SetCurrentValue(AbstractCollapseItem.HeaderProperty, itemData.Header);
                }
                if (!collapseItem.IsSet(AbstractCollapseItem.IsSelectedProperty))
                {
                    collapseItem.SetCurrentValue(AbstractCollapseItem.IsSelectedProperty, itemData.IsSelected);
                }
                if (!collapseItem.IsSet(AbstractCollapseItem.IsShowExpandIconProperty))
                {
                    collapseItem.SetCurrentValue(AbstractCollapseItem.IsShowExpandIconProperty, itemData.IsShowExpandIcon);
                }
            }
        }
    }

    /// <summary>
    /// Calls the base member, then keeps the earliest selected panel when the control is in accordion mode: a panel
    /// that arrives already selected does not take the selection away from an earlier selected panel, and of several
    /// selected panels only the first one stays selected.
    /// </summary>
    /// <param name="container">
    /// The panel that has been prepared.
    /// </param>
    /// <param name="item">
    /// The item being displayed.
    /// </param>
    /// <param name="index">
    /// The index of the item being displayed.
    /// </param>
    /// <remarks>
    /// A derived class must call the base member; otherwise accordion mode is not enforced.
    /// </remarks>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        var containerWasSelected = container.GetValue(AbstractCollapseItem.IsSelectedProperty);
        var previousSelectedIndex = IsAccordion ? Selection.SelectedIndex : -1;

        base.ContainerForItemPreparedOverride(container, item, index);

        if (IsAccordion &&
            containerWasSelected &&
            previousSelectedIndex >= 0 &&
            previousSelectedIndex < index &&
            Selection.SelectedIndex == index)
        {
            Selection.SelectedIndex = previousSelectedIndex;
        }

        NormalizeAccordionSelection();
    }

    /// <summary>
    /// Calls the base member, then switches the control between single and multiple selection when
    /// <see cref="IsAccordion" /> changes.
    /// </summary>
    /// <param name="change">
    /// The property change.
    /// </param>
    /// <remarks>
    /// A derived class must call the base member. Otherwise changing <see cref="IsAccordion" /> leaves the
    /// selection mode unchanged.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsAccordionProperty)
        {
            SetupSelectionMode();
        }
    }

    /// <summary>
    /// Calls the base member, then reduces a selection of several panels to the first one when the control is in
    /// accordion mode.
    /// </summary>
    /// <param name="e">
    /// The arguments of the event.
    /// </param>
    /// <remarks>
    /// A derived class must call the base member; otherwise accordion mode is not enforced on attachment.
    /// </remarks>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        NormalizeAccordionSelection();
    }

    private void NormalizeAccordionSelection()
    {
        if (!IsAccordion || Selection.SelectedIndexes.Count <= 1)
        {
            return;
        }

        Selection.SelectedIndex = Selection.SelectedIndexes.Min();
    }

    private void SetupSelectionMode()
    {
        SelectionMode = IsAccordion
            ? SelectionMode.Single | SelectionMode.Toggle
            : SelectionMode.Multiple | SelectionMode.Toggle;
    }

    private sealed class DefaultCollapseItem : AbstractCollapseItem;
}
