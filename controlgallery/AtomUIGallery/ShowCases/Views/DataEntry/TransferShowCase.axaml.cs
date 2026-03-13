using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
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
                vm.TransferFilterValueSelector = record =>
                {
                    if (record is ListItemData listItemData)
                    {
                        return listItemData.Content;
                    }
                    return record.ToString();
                };
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
    
    
}