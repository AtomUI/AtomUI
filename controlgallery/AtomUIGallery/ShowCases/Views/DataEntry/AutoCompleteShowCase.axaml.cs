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
            if (DataContext is CascaderViewModel vm)
            {
            }
        });
        InitializeComponent();
    }
}