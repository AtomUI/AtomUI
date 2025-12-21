using Avalonia;

namespace AtomUI.Desktop.Controls;

internal class UploadShapeUploadingContent : AbstractUploadPictureContent
{
    public static readonly StyledProperty<double> ProgressProperty =
        AbstractUploadListItem.ProgressProperty.AddOwner<UploadShapeUploadingContent>();
    
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }
}