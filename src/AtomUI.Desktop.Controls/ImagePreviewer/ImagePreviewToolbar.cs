using Avalonia;

namespace AtomUI.Desktop.Controls;

internal class ImagePreviewToolbar : ImagePreviewBaseToolbar
{
    #region 内部属性定义

    internal static readonly DirectProperty<ImagePreviewToolbar, bool> IsLastImageProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewToolbar, bool>(
            nameof(IsLastImage),
            o => o.IsLastImage,
            (o, v) => o.IsLastImage = v);
    
    internal static readonly DirectProperty<ImagePreviewToolbar, bool> IsFirstImageProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewToolbar, bool>(
            nameof(IsFirstImage),
            o => o.IsFirstImage,
            (o, v) => o.IsFirstImage = v);
    
    private bool _isLastImage;

    internal bool IsLastImage
    {
        get => _isLastImage;
        set => SetAndRaise(IsLastImageProperty, ref _isLastImage, value);
    }
    
    private bool _isFirstImage;

    internal bool IsFirstImage
    {
        get => _isFirstImage;
        set => SetAndRaise(IsFirstImageProperty, ref _isFirstImage, value);
    }

    #endregion
}