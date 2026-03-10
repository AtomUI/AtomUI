using AtomUI;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class DescriptionsViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Descriptions";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private SizeType _descriptionsSizeType;

    public SizeType DescriptionsSizeType
    {
        get => _descriptionsSizeType;
        set => this.RaiseAndSetIfChanged(ref _descriptionsSizeType, value);
    }
    
    public DescriptionsViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}