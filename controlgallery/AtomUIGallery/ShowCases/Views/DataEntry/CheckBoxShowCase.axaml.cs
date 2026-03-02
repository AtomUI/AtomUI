using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class CheckBoxShowCase : ReactiveUserControl<CheckBoxViewModel>
{
    public CheckBoxShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is CheckBoxViewModel vm)
            {
                ConfigureCheckBoxOptions(vm);
            }
        });
        InitializeComponent();
    }
    
    private void ConfigureCheckBoxOptions(CheckBoxViewModel viewModel)
    {
        var apple = new CheckBoxOption() { Content = "Apple" };
        var pear = new CheckBoxOption() { Content = "Pear" };
        viewModel.CheckBoxOptions = new List<CheckBoxOption>
        {
            apple,
            pear,
            new () { Content = "Orange", IsEnabled = false},
        };
        viewModel.DefaultCheckBoxOptions = new List<CheckBoxOption>
        {
            pear,
        };
    }
}