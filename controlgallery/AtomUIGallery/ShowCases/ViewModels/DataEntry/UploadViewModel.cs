using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class UploadViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Upload";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private List<UploadTaskInfo>? _defaultTaskList;

    public List<UploadTaskInfo>? DefaultTaskList
    {
        get => _defaultTaskList;
        set => this.RaiseAndSetIfChanged(ref _defaultTaskList, value);
    }

    public UploadViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}