using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class FormShowCase : ReactiveUserControl<FormViewModel>
{
    public FormShowCase()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
    }
}