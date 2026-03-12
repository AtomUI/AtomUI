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
}