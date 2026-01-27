using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class AutoCompleteShowCase : ReactiveUserControl<AutoCompleteViewModel>
{
    public AutoCompleteShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is AutoCompleteViewModel vm)
            {
                vm.BasicOptionsAsyncLoader       = new BasicOptionsAsyncLoader();
                vm.CustomLabelOptionsAsyncLoader = new CustomLabelOptionsAsyncLoader();
                vm.SearchEditOptionsAsyncLoader  = new SearchEditOptionsAsyncLoader();
                InitFilterCaseOptions(vm);
            }
        });
        InitializeComponent();
    }

    private void InitFilterCaseOptions(AutoCompleteViewModel vm)
    {
        vm.FilterCaseOptions = [
            new AutoCompleteOption()
            {
                Header = "Burns Bay Road",
                Value = "Burns Bay Road",
            },
            new AutoCompleteOption()
            {
                Header = "Downing Street",
                Value  = "Downing Street",
            },
            new AutoCompleteOption()
            {
                Header = "Wall Street",
                Value  = "Wall Street",
            },
        ];
    }
}