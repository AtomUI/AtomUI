using Avalonia;

namespace AtomUI.Desktop.Controls;

internal class UploadPictureUploadingContent : AbstractUploadPictureContent
{
    public static readonly StyledProperty<double> ProgressProperty =
        AbstractUploadListItem.ProgressProperty.AddOwner<UploadPictureUploadingContent>();
    
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }
}