using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class DataGridTreeFilterFlyoutPresenter : TreeViewFlyoutPresenter
{
    private Button? _resetButton;
    private Button? _okButton;
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _resetButton = e.NameScope.Find<Button>(DataGridFilterFlyoutPresenterThemeConstants.ResetButtonPart);
        _okButton    = e.NameScope.Find<Button>(DataGridFilterFlyoutPresenterThemeConstants.OkButtonPart);
        
        if (_resetButton != null)
        {
            _resetButton.Click += HandleResetButtonClick;
        }

        if (_okButton != null)
        {
            _okButton.Click += HandleOkButtonClick;
        }
    }
    
    private void HandleResetButtonClick(object? sender, RoutedEventArgs e)
    {
        ClearCheckStateRecursive(this);
    }
    
    internal List<String> GetFilterValues()
    {
        var values =  new List<String>();
        CollectFilterValues(values, this);
        return values;
    }
    
    private void CollectFilterValues(List<string> filterValues, ItemsControl itemsControl)
    {
        for (var i = 0; i < itemsControl.ItemCount; i++)
        {
            var item = itemsControl.ContainerFromIndex(i);
            if (item is DataGridFilterTreeItem filterTreeItem)
            {
                CollectFilterValues(filterValues, filterTreeItem);
            }
        }
        
        if (itemsControl is DataGridFilterTreeItem treeItem && itemsControl.ItemCount == 0)
        {
            if (treeItem.IsChecked == true && treeItem.FilterValue != null)
            {
                filterValues.Add(treeItem.FilterValue);
            }
        }
    }


    private void HandleOkButtonClick(object? sender, RoutedEventArgs e)
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
            var item = itemsControl.ContainerFromIndex(i);
            if (item is TreeItem filterTreeViewItem)
            {
                ClearCheckStateRecursive(filterTreeViewItem);
            }
        }

        if (itemsControl is TreeItem treeViewItem && itemsControl.ItemCount == 0)
        {
            treeViewItem.IsChecked = false;
        }
    }
}