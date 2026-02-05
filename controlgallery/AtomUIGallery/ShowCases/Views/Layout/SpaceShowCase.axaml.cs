using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class SpaceShowCase : ReactiveUserControl<SpaceViewModel>
{
    public SpaceShowCase()
    {
        this.WhenActivated(_ => { });
    }
}
