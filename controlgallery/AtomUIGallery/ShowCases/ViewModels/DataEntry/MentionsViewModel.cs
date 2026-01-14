using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class MentionsViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Mentions";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
        
    private List<IMentionOption> _basicMentionOptions = [];
    
    public List<IMentionOption> BasicMentionOptions
    {
        get => _basicMentionOptions;
        set => this.RaiseAndSetIfChanged(ref _basicMentionOptions, value);
    }

    public MentionsViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}