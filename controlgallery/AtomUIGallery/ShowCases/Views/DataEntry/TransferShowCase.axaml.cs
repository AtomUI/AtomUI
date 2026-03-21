using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia.Interactivity;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class TransferShowCase : ReactiveUserControl<TransferViewModel>
{
    public TransferShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is TransferViewModel vm)
            {
                InitBasicTransferItems(vm);
                InitOneWayTransferItems(vm);
                InitSearchTransferItems(vm);
                InitPaginationTransferItems(vm);
                InitDataGridTransferItems(vm);
                vm.TransferFilterValueSelector = record =>
                {
                    if (record is ListItemData listItemData)
                    {
                        return listItemData.Content;
                    }
                    return record.ToString();
                };
                InitAdvanceTransferItems(vm);
                InitTreeViewTransferItems(vm);
            }
        });
        InitializeComponent();
    }

    private void InitBasicTransferItems(TransferViewModel vm)
    {
        var items = new List<IListItemData>();
        for (var i = 0; i < 20; i++)
        {
            items.Add(new ListItemData()
            {
                ItemKey = $"{i}",
                Content = $"content{i + 1}"
            });
        }

        vm.BasicTransferItems = items;
    }
    
    private void InitOneWayTransferItems(TransferViewModel vm)
    {
        var items = new List<IListItemData>();
        for (var i = 0; i < 20; i++)
        {
            items.Add(new ListItemData()
            {
                ItemKey = $"{i}",
                Content = $"content{i + 1}",
                IsEnabled = !(i % 3 < 1)
            });
        }

        vm.OneWayTransferItems = items;
    }
    
    private void InitSearchTransferItems(TransferViewModel vm)
    {
        var items = new List<IListItemData>();
        for (var i = 0; i < 20; i++)
        {
            items.Add(new SearchCaseItemData()
            {
                ItemKey   = $"{i}",
                Content   = $"content{i + 1}",
                Description = $"description of content{i + 1}"
            });
        }

        vm.SearchTransferItems = items;
    }
    
    private void InitAdvanceTransferItems(TransferViewModel vm)
    {
        var items      = new List<IListItemData>();
        var targetKeys = new List<EntityKey>();
        for (var i = 0; i < 20; i++)
        {
            var item = new SearchCaseItemData()
            {
                ItemKey     = $"{i}",
                Content     = $"content{i + 1}",
                Description = $"description of content{i + 1}"
            };
            items.Add(item);
            if (i % 2 == 0)
            {
                targetKeys.Add(item.ItemKey!.Value);
            }
        }
        vm.AdvanceTransferItems             = items;
        vm.AdvanceTransferDefaultTargetKeys = targetKeys;
    }
    
    private void ReloadAdvancedTransferItems(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TransferViewModel vm)
        {
            AdvanceTransfer.TargetKeys = vm.AdvanceTransferDefaultTargetKeys;
        }
    }
    
    private void InitPaginationTransferItems(TransferViewModel vm)
    {
        var items      = new List<IListItemData>();
        var targetKeys = new List<EntityKey>();
        for (var i = 0; i < 2000; i++)
        {
            var item = new SearchCaseItemData()
            {
                ItemKey     = $"{i}",
                Content     = $"content{i + 1}",
                Description = $"description of content{i + 1}"
            };
            items.Add(item);
            if (i % 2 == 0)
            {
                targetKeys.Add(item.ItemKey!.Value);
            }
        }
        vm.PaginationTransferItems             = items;
        vm.PaginationTransferDefaultTargetKeys = targetKeys;
    }

    private void InitDataGridTransferItems(TransferViewModel vm)
    {
        var          items = new List<DataGridTransferData>();
        List<string> tags  = ["cat", "dog", "bird"];
        for (var i = 0; i < 20; i++)
        {
            items.Add(new DataGridTransferData()
            {
                ItemKey     = $"{i}",
                Title       = $"content{i + 1}",
                Description = $"description of content{i + 1}",
                Tag         = tags[i % 3]
            });
        }
        vm.GridDataTransformItems = items;
    }

    private void InitTreeViewTransferItems(TransferViewModel vm)
    {
        vm.TransferTreeNodes = [
            new TreeItemNode()
            {
                ItemKey = "0-0",
                Header = "0-0"
            },
            new TreeItemNode()
            {
                ItemKey = "0-1",
                Header = "0-1",
                Children = [
                    new TreeItemNode()
                    {
                        ItemKey = "0-1-0",
                        Header = "0-1-0",
                    },
                    new TreeItemNode()
                    {
                        ItemKey = "0-1-1",
                        Header = "0-1-1",
                    }
                ]
            },
            new TreeItemNode()
            {
                ItemKey = "0-2",
                Header = "0-2"
            },
            new TreeItemNode()
            {
                ItemKey = "0-3",
                Header = "0-3"
            },
            new TreeItemNode()
            {
                ItemKey = "0-4",
                Header = "0-4"
            }
        ];
    }
    
}