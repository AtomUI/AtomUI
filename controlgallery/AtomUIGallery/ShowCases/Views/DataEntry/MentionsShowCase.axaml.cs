using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class MentionsShowCase : ReactiveUserControl<MentionsViewModel>
{
    public MentionsShowCase()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
    }
}