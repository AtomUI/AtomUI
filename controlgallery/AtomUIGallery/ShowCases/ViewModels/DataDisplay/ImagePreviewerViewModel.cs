using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class ImagePreviewerViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "ImagePreviewer";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private IList<string>? _defaultImages;

    public IList<string>? DefaultImages
    {
        get => _defaultImages;
        set => this.RaiseAndSetIfChanged(ref _defaultImages, value);
    }
    
    private IList<string>? _threeImages;

    public IList<string>? ThreeImages
    {
        get => _threeImages;
        set => this.RaiseAndSetIfChanged(ref _threeImages, value);
    }
    
    private string? _fallbackImage;
    
    public string? FallbackImage
    {
        get => _fallbackImage;
        set => this.RaiseAndSetIfChanged(ref _fallbackImage, value);
    }
    
    private string? _blurImage;
    
    public string? BlurImage
    {
        get => _blurImage;
        set => this.RaiseAndSetIfChanged(ref _blurImage, value);
    }

    public ImagePreviewerViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}