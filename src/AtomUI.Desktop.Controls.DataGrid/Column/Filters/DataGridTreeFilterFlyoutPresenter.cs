using Avalonia.LogicalTree;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class DataGridTreeFilterFlyoutPresenter : TreeViewFlyoutPresenter
{
    private DataGridFilterThemeScope? _themeScope;

    private Button? _resetButton;
    private Button? _okButton;

    static DataGridTreeFilterFlyoutPresenter()
    {
        Button.ClickEvent.AddClassHandler<DataGridTreeFilterFlyoutPresenter>(
            (presenter, args) => presenter.HandleButtonClick(args));
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _themeScope?.Dispose();
        _themeScope = new DataGridFilterThemeScope(this);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _themeScope?.Dispose();
        _themeScope = null;
        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _resetButton = e.NameScope.Find<Button>(DataGridFilterFlyoutPresenterThemeConstants.ResetButtonPart);
        _okButton    = e.NameScope.Find<Button>(DataGridFilterFlyoutPresenterThemeConstants.OkButtonPart);
    }
    private void HandleButtonClick(RoutedEventArgs e)
    {
        if (ReferenceEquals(e.Source, _resetButton))
        {
            ResetFilter();
        }
        else if (ReferenceEquals(e.Source, _okButton))
        {
            ConfirmFilter();
        }
    }
    
    private void ResetFilter()
    {
        ClearCheckStateRecursive(this);
    }
    
    internal List<object> GetFilterValues()
    {
        var selectedValueCount = CountSelectedFilterValueLeaves(this);
        if (selectedValueCount == 0)
        {
            return DataGridFilterValuesSelectedEventArgs.EmptyValues;
        }

        var values = new List<object>(selectedValueCount);
        CollectFilterValues(values, this);
        return values;
    }

    internal void NotifyFilterSelectionChanged()
    {
        if (TreeViewFlyout is DataGridTreeFilterFlyout treeFilterFlyout)
        {
            treeFilterFlyout.NotifyFilterValuesSelected(
                new DataGridFilterValuesSelectedEventArgs(
                    DataGridFilterValuesCommitKind.SelectionChanged,
                    GetFilterValues()));
        }
    }
    
    private void CollectFilterValues(List<object> filterValues, ItemsControl itemsControl)
    {
        for (var i = 0; i < itemsControl.ItemCount; i++)
        {
            var item = GetFilterItem(itemsControl, i);
            if (item is DataGridFilterTreeViewItem filterTreeItem)
            {
                CollectFilterValues(filterValues, filterTreeItem);
            }
        }
        
        if (itemsControl is DataGridFilterTreeViewItem treeItem && itemsControl.ItemCount == 0)
        {
            if (treeItem.IsChecked == true && treeItem.FilterValue != null)
            {
                filterValues.Add(treeItem.FilterValue);
            }
        }
    }

    private int CountSelectedFilterValueLeaves(ItemsControl itemsControl)
    {
        var count = 0;
        for (var i = 0; i < itemsControl.ItemCount; i++)
        {
            var item = GetFilterItem(itemsControl, i);
            if (item is DataGridFilterTreeViewItem filterTreeItem)
            {
                count += CountSelectedFilterValueLeaves(filterTreeItem);
            }
        }

        if (itemsControl is DataGridFilterTreeViewItem treeItem &&
            itemsControl.ItemCount == 0 &&
            treeItem.IsChecked == true &&
            treeItem.FilterValue != null)
        {
            count++;
        }

        return count;
    }

    private Control? GetFilterItem(ItemsControl itemsControl, int index)
    {
        if (itemsControl.ContainerFromIndex(index) is Control container)
        {
            return container;
        }

        return index >= 0 && index < itemsControl.ItemsView.Count
            ? itemsControl.ItemsView[index] as Control
            : null;
    }


    private void ConfirmFilter()
    {
        if (TreeViewFlyout is DataGridTreeFilterFlyout treeFilterFlyout)
        {
            treeFilterFlyout.IsActiveShutdown = true;
        }
        
        TreeViewFlyout?.Hide();
    }
    
    private void ClearCheckStateRecursive(ItemsControl itemsControl)
    {
        for (var i = 0; i < itemsControl.ItemCount; i++)
        {
            var item = GetFilterItem(itemsControl, i);
            if (item is TreeViewItem filterTreeViewItem)
            {
                ClearCheckStateRecursive(filterTreeViewItem);
            }
        }

        if (itemsControl is TreeViewItem treeViewItem && itemsControl.ItemCount == 0)
        {
            treeViewItem.IsChecked = false;
        }
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is DataGridFilterTreeViewItem treeItem)
        {
            treeItem.OwningPresenter = this;
        }
    }
}
