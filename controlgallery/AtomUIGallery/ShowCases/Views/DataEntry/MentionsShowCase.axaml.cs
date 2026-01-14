using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class MentionsShowCase : ReactiveUserControl<MentionsViewModel>
{
    public MentionsShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is MentionsViewModel vm)
            {
                InitBasicMentionOptions(vm);
                vm.MentionOptionAsyncLoader = new MentionOptionAsyncLoader();
            }
        });
        InitializeComponent();
    }
    
    private void InitBasicMentionOptions(MentionsViewModel viewModel)
    {
        viewModel.BasicMentionOptions =
        [
            new MentionOption()
            {
                Header = "afc163",
                Value = "afc163"
            },
            new MentionOption()
            {
                Header = "zombieJ",
                Value  = "zombieJ"
            },
            new MentionOption()
            {
                Header = "yesmeck",
                Value  = "yesmeck"
            }
        ];
    }

}