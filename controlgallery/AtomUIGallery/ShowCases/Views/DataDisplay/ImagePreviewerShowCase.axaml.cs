using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class ImagePreviewerShowCase : ReactiveUserControl<ImagePreviewerViewModel>
{
    public ImagePreviewerShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is ImagePreviewerViewModel viewModel)
            {
                viewModel.DefaultImages = [
                    "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/1.png"
                ];
                viewModel.ThreeImages = [
                    "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/4.webp",
                    "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/5.webp",
                    "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/6.webp"
                ];
                viewModel.TwoImages = [
                    "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/2.svg",
                    "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/3.svg",
                ];
                viewModel.FallbackImage = "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/Fallback.png";
                viewModel.BlurImage = "avares://AtomUIGallery/Assets/ImagePreviewerShowCase/Blur.png";
            }
        });
        InitializeComponent();
    }
}