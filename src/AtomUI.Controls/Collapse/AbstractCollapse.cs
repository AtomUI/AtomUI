using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Controls;

public abstract class AbstractCollapse : SelectingItemsControl
{
    public static readonly StyledProperty<bool> IsAccordionProperty =
        AvaloniaProperty.Register<AbstractCollapse, bool>(nameof(IsAccordion));

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

    protected virtual AbstractCollapseItem CreateCollapseItem() => new DefaultCollapseItem();

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => CreateCollapseItem();

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<AbstractCollapseItem>(item, out recycleKey);

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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsAccordionProperty)
        {
            SetupSelectionMode();
        }
    }

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
