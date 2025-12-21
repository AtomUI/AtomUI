using Avalonia;

namespace AtomUI.Desktop.Controls;

internal class UploadPictureShapeListItem : AbstractUploadListItem
{
    public static readonly StyledProperty<bool> IsCircleProperty =
        AvaloniaProperty.Register<UploadPictureShapeListItem, bool>(nameof(IsCircle));
    
    public bool IsCircle
    {
        get => GetValue(IsCircleProperty);
        set => SetValue(IsCircleProperty, value);
    }
}