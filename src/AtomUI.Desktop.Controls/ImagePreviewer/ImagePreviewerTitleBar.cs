using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace AtomUI.Desktop.Controls;

[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
[TemplatePart("PART_IconPresenter", typeof(IconPresenter))]
internal class ImagePreviewerTitleBar : WindowTitleBar
{
    #region 公共属性定义

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<ImagePreviewerTitleBar, PathIcon?>(nameof(Icon));

    internal static readonly DirectProperty<ImagePreviewerTitleBar, int> CurrentIndexProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerTitleBar, int>(
            nameof(CurrentIndex),
            o => o.CurrentIndex,
            (o, v) => o.CurrentIndex = v);

    internal static readonly DirectProperty<ImagePreviewerTitleBar, int> CountProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerTitleBar, int>(
            nameof(Count),
            o => o.Count,
            (o, v) => o.Count = v);

    internal static readonly DirectProperty<ImagePreviewerTitleBar, bool> IsScaleDownEnabledProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerTitleBar, bool>(
            nameof(IsScaleDownEnabled),
            o => o.IsScaleDownEnabled,
            (o, v) => o.IsScaleDownEnabled = v);

    internal static readonly DirectProperty<ImagePreviewerTitleBar, bool> IsScaleUpEnabledProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerTitleBar, bool>(
            nameof(IsScaleUpEnabled),
            o => o.IsScaleUpEnabled,
            (o, v) => o.IsScaleUpEnabled = v);

    internal static readonly DirectProperty<ImagePreviewerTitleBar, bool> IsImageFitToWindowProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerTitleBar, bool>(
            nameof(IsImageFitToWindow),
            o => o.IsImageFitToWindow,
            (o, v) => o.IsImageFitToWindow = v);

    internal static readonly DirectProperty<ImagePreviewerTitleBar, bool> IsFirstImageProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerTitleBar, bool>(
            nameof(IsFirstImage),
            o => o.IsFirstImage,
            (o, v) => o.IsFirstImage = v);

    internal static readonly DirectProperty<ImagePreviewerTitleBar, bool> IsLastImageProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerTitleBar, bool>(
            nameof(IsLastImage),
            o => o.IsLastImage,
            (o, v) => o.IsLastImage = v);

    private int _currentIndex;

    internal int CurrentIndex
    {
        get => _currentIndex;
        set => SetAndRaise(CurrentIndexProperty, ref _currentIndex, value);
    }

    private int _count;

    internal int Count
    {
        get => _count;
        set => SetAndRaise(CountProperty, ref _count, value);
    }

    private bool _isScaleDownEnabled;

    internal bool IsScaleDownEnabled
    {
        get => _isScaleDownEnabled;
        set => SetAndRaise(IsScaleDownEnabledProperty, ref _isScaleDownEnabled, value);
    }

    private bool _isScaleUpEnabled;

    internal bool IsScaleUpEnabled
    {
        get => _isScaleUpEnabled;
        set => SetAndRaise(IsScaleUpEnabledProperty, ref _isScaleUpEnabled, value);
    }

    private bool _isImageFitToWindow;

    internal bool IsImageFitToWindow
    {
        get => _isImageFitToWindow;
        set => SetAndRaise(IsImageFitToWindowProperty, ref _isImageFitToWindow, value);
    }

    private bool _isFirstImage;

    internal bool IsFirstImage
    {
        get => _isFirstImage;
        set => SetAndRaise(IsFirstImageProperty, ref _isFirstImage, value);
    }

    private bool _isLastImage;

    internal bool IsLastImage
    {
        get => _isLastImage;
        set => SetAndRaise(IsLastImageProperty, ref _isLastImage, value);
    }

    #endregion

    public ImagePreviewerTitleBar()
    {
        var toolbar = new ImagePreviewToolbar();
        toolbar.Bind(ImagePreviewBaseToolbar.CurrentIndexProperty,
            this.GetObservable(CurrentIndexProperty));
        toolbar.Bind(ImagePreviewBaseToolbar.CountProperty,
            this.GetObservable(CountProperty));
        toolbar.Bind(ImagePreviewBaseToolbar.IsScaleDownEnabledProperty,
            this.GetObservable(IsScaleDownEnabledProperty));
        toolbar.Bind(ImagePreviewBaseToolbar.IsScaleUpEnabledProperty,
            this.GetObservable(IsScaleUpEnabledProperty));
        toolbar.Bind(ImagePreviewBaseToolbar.IsImageFitToWindowProperty,
            this.GetObservable(IsImageFitToWindowProperty));
        toolbar.Bind(ImagePreviewToolbar.IsFirstImageProperty,
            this.GetObservable(IsFirstImageProperty));
        toolbar.Bind(ImagePreviewToolbar.IsLastImageProperty,
            this.GetObservable(IsLastImageProperty));
        LeftAddOn = toolbar;
    }
}
