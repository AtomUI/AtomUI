using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Media;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class ImagePreviewerViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "ImagePreviewer";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private IList<IImage>? _defaultImages;

    public IList<IImage>? DefaultImages
    {
        get => _defaultImages;
        set => this.RaiseAndSetIfChanged(ref _defaultImages, value);
    }

    public ImagePreviewerViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}