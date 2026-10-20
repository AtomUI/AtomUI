using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUIGallery.ShowCases.Skeleton;

public partial class SkeletonShowCase : GalleryReactiveUserControl<SkeletonViewModel>
{
    public const string LanguageId = nameof(SkeletonShowCase);

    public SkeletonShowCase()
    {
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

    private void HandleSkeletonElementChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not AtomUI.Desktop.Controls.Segmented segmented || segmented.SelectedIndex < 0)
        {
            return;
        }

        var preview = segmented.GetVisualAncestors().OfType<SemanticPartPreview>().FirstOrDefault();
        if (preview is null)
        {
            return;
        }

        var previewHost = preview.PreviewContent?
                               .GetVisualDescendants()
                               .OfType<Avalonia.Controls.Grid>()
                               .FirstOrDefault(static control => control.Name == "SkeletonElementPreviewHost");
        if (previewHost is null)
        {
            return;
        }

        Control? owner = segmented.SelectedIndex switch
        {
            0 => new SkeletonAvatar
            {
                Width = double.NaN,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                SizeType = CustomizableSizeType.Middle
            },
            1 => new SkeletonButton
            {
                Width = double.NaN,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                SizeType = CustomizableSizeType.Middle
            },
            2 => new SkeletonInput
            {
                Width = double.NaN,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                SizeType = CustomizableSizeType.Middle
            },
            3 => new SkeletonImage
            {
                Width = double.NaN,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            },
            4 => new SkeletonNode
            {
                Width = double.NaN,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            },
            _ => null
        };
        if (owner is null)
        {
            return;
        }

        owner.Classes.Add("semantic-preview-owner");
        previewHost.Children.Clear();
        previewHost.Children.Add(owner);
        preview.SemanticOwner = owner;
        preview.SemanticOwnerType = owner.GetType();
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
