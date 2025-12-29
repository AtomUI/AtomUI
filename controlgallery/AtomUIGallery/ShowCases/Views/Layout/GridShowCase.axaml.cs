using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class GridShowCase : ReactiveUserControl<GridViewModel>
{
    public GridShowCase()
    {
        this.WhenActivated(_ => { });
        InitializeComponent();
    }
}
