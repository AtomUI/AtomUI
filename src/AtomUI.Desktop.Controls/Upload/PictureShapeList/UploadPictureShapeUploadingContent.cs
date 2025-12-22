using Avalonia;

namespace AtomUI.Desktop.Controls;

internal class UploadPictureShapeUploadingContent : AbstractUploadPictureContent
{
    public static readonly StyledProperty<double> ProgressProperty =
        AbstractUploadListItem.ProgressProperty.AddOwner<UploadPictureShapeUploadingContent>();
    
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }
}