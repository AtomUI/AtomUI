using AtomUI.Utils;
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
                    AssetsBitmapLoader.Load("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/1.png")
                ];
            }
        });
        InitializeComponent();
    }
}