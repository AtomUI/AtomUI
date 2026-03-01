using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class FormViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Form";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private FormLayout _formLayout = FormLayout.Horizontal;

    public FormLayout FormLayout
    {
        get => _formLayout;
        set => this.RaiseAndSetIfChanged(ref _formLayout, value);
    }
    
    public FormViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}