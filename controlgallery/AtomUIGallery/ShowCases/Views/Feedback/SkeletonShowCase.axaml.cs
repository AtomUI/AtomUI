using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class SkeletonShowCase : ReactiveUserControl<SkeletonViewModel>
{
    public SkeletonShowCase()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
    }

    private void HandleSizeTypeChanged(object? sender, OptionCheckedChangedEventArgs e)
    {
        if (DataContext is SkeletonViewModel viewModel)
        {
            if (e.Index == 0)
            {
                viewModel.SkeletonButtonAndInputSizeType = CustomizableSizeType.Middle;
            }
            else if (e.Index == 1)
            {
                viewModel.SkeletonButtonAndInputSizeType = CustomizableSizeType.Large;
            }
            else if (e.Index == 2)
            {
                viewModel.SkeletonButtonAndInputSizeType = CustomizableSizeType.Small;
            }
        }
    }
    
    private void HandleButtonShapeChanged(object? sender, OptionCheckedChangedEventArgs e)
    {
        if (DataContext is SkeletonViewModel viewModel)
        {
            if (e.Index == 0)
            {
                viewModel.SkeletonButtonShape = SkeletonButtonShape.Square;
            }
            else if (e.Index == 1)
            {
                viewModel.SkeletonButtonShape = SkeletonButtonShape.Round;
            }
            else if (e.Index == 2)
            {
                viewModel.SkeletonButtonShape = SkeletonButtonShape.Circle;
            }
        }
    }
    
    private void HandleButtonAvatarChanged(object? sender, OptionCheckedChangedEventArgs e)
    {
        if (DataContext is SkeletonViewModel viewModel)
        {
            if (e.Index == 0)
            {
                viewModel.SkeletonAvatarShape = AvatarShape.Square;
            }
            else if (e.Index == 1)
            {
                viewModel.SkeletonAvatarShape = AvatarShape.Circle;
            }
        }
    }

    private void HandleLoadingButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SkeletonViewModel viewModel)
        {
            viewModel.SkeletonLoading = !viewModel.SkeletonLoading;
            DispatcherTimer.RunOnce(() =>
            {
                viewModel.SkeletonLoading = !viewModel.SkeletonLoading;
            }, TimeSpan.FromSeconds(3));
        }
    }
}