using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

internal class ImagePreviewerDialog : Window,
                                      IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<IList<ImagePreviewEntry>?> ItemsSourceProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, IList<ImagePreviewEntry>?>(nameof(ItemsSource));

    public static readonly StyledProperty<bool> IsImageMovableProperty =
        ImagePreviewer.IsImageMovableProperty.AddOwner<ImagePreviewerDialog>();

    public static readonly StyledProperty<double> ScaleStepProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, double>(nameof(ScaleStep), 0.5);

    public static readonly StyledProperty<double> MinScaleProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, double>(nameof(MinScale), 1.0);

    public static readonly StyledProperty<double> MaxScaleProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, double>(nameof(MaxScale), 50.0);

    public static readonly StyledProperty<bool> IsWheelZoomEnabledProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, bool>(nameof(IsWheelZoomEnabled), true);

    public static readonly StyledProperty<bool> IsModalProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, bool>(nameof(IsModal));

    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, object?>(nameof(LoadingContent));

    public static readonly StyledProperty<IDataTemplate?> LoadingContentTemplateProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, IDataTemplate?>(nameof(LoadingContentTemplate));

    public static readonly StyledProperty<object?> ErrorContentProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, object?>(nameof(ErrorContent));

    public static readonly StyledProperty<IDataTemplate?> ErrorContentTemplateProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, IDataTemplate?>(nameof(ErrorContentTemplate));

    public static readonly StyledProperty<int> CurrentIndexProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, int>(nameof(CurrentIndex), 0);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ImagePreviewerDialog>();

    public static readonly StyledProperty<IImagePreviewTitleResolver?> PreviewTitleResolverProperty =
        AbstractImagePreviewer.PreviewTitleResolverProperty.AddOwner<ImagePreviewerDialog>();

    public static readonly StyledProperty<PathIcon?> TitleIconProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, PathIcon?>(nameof(TitleIcon));

    public static readonly DirectProperty<ImagePreviewerDialog, int> CountProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, int>(
            nameof(Count),
            o => o.Count);

    public static readonly StyledProperty<Transform?> TransformProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, Transform?>(nameof(Transform));

    public IList<ImagePreviewEntry>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public bool IsImageMovable
    {
        get => GetValue(IsImageMovableProperty);
        set => SetValue(IsImageMovableProperty, value);
    }

    public double ScaleStep
    {
        get => GetValue(ScaleStepProperty);
        set => SetValue(ScaleStepProperty, value);
    }

    public double MinScale
    {
        get => GetValue(MinScaleProperty);
        set => SetValue(MinScaleProperty, value);
    }

    public double MaxScale
    {
        get => GetValue(MaxScaleProperty);
        set => SetValue(MaxScaleProperty, value);
    }

    public bool IsWheelZoomEnabled
    {
        get => GetValue(IsWheelZoomEnabledProperty);
        set => SetValue(IsWheelZoomEnabledProperty, value);
    }

    public bool IsModal
    {
        get => GetValue(IsModalProperty);
        set => SetValue(IsModalProperty, value);
    }

    public object? LoadingContent
    {
        get => GetValue(LoadingContentProperty);
        set => SetValue(LoadingContentProperty, value);
    }

    public IDataTemplate? LoadingContentTemplate
    {
        get => GetValue(LoadingContentTemplateProperty);
        set => SetValue(LoadingContentTemplateProperty, value);
    }

    public object? ErrorContent
    {
        get => GetValue(ErrorContentProperty);
        set => SetValue(ErrorContentProperty, value);
    }

    public IDataTemplate? ErrorContentTemplate
    {
        get => GetValue(ErrorContentTemplateProperty);
        set => SetValue(ErrorContentTemplateProperty, value);
    }

    public int CurrentIndex
    {
        get => GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public IImagePreviewTitleResolver? PreviewTitleResolver
    {
        get => GetValue(PreviewTitleResolverProperty);
        set => SetValue(PreviewTitleResolverProperty, value);
    }

    public PathIcon? TitleIcon
    {
        get => GetValue(TitleIconProperty);
        set => SetValue(TitleIconProperty, value);
    }

    private int _count;

    public int Count
    {
        get => _count;
        internal set => SetAndRaise(CountProperty, ref _count, value);
    }

    public Transform? Transform
    {
        get => GetValue(TransformProperty);
        set => SetValue(TransformProperty, value);
    }

    public TopLevel ParentTopLevel { get; }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<ImagePreviewerDialog, IImage?> CurrentImageProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, IImage?>(
            nameof(CurrentImage),
            o => o.CurrentImage,
            (o, v) => o.CurrentImage = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, bool> IsMultiImagesProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, bool>(
            nameof(IsMultiImages),
            o => o.IsMultiImages,
            (o, v) => o.IsMultiImages = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, bool> IsLastImageProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, bool>(
            nameof(IsLastImage),
            o => o.IsLastImage,
            (o, v) => o.IsLastImage = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, bool> IsFirstImageProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, bool>(
            nameof(IsFirstImage),
            o => o.IsFirstImage,
            (o, v) => o.IsFirstImage = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, bool> IsCurrentImageLoadingProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, bool>(
            nameof(IsCurrentImageLoading),
            o => o.IsCurrentImageLoading,
            (o, v) => o.IsCurrentImageLoading = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, bool> IsCurrentImageFailedProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, bool>(
            nameof(IsCurrentImageFailed),
            o => o.IsCurrentImageFailed,
            (o, v) => o.IsCurrentImageFailed = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, bool> IsScaleDownEnabledProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, bool>(
            nameof(IsScaleDownEnabled),
            o => o.IsScaleDownEnabled,
            (o, v) => o.IsScaleDownEnabled = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, bool> IsScaleUpEnabledProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, bool>(
            nameof(IsScaleUpEnabled),
            o => o.IsScaleUpEnabled,
            (o, v) => o.IsScaleUpEnabled = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, double> ImageScaleXProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, double>(
            nameof(ImageScaleX),
            o => o.ImageScaleX,
            (o, v) => o.ImageScaleX = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, double> ImageScaleYProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, double>(
            nameof(ImageScaleY),
            o => o.ImageScaleY,
            (o, v) => o.ImageScaleY = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, bool> ImageScaleChangedProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, bool>(
            nameof(ImageScaleChanged),
            o => o.ImageScaleChanged,
            (o, v) => o.ImageScaleChanged = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, double> ImageRotateProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, double>(
            nameof(ImageRotate),
            o => o.ImageRotate,
            (o, v) => o.ImageRotate = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, bool> IsImageFitToWindowProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, bool>(
            nameof(IsImageFitToWindow),
            o => o.IsImageFitToWindow,
            (o, v) => o.IsImageFitToWindow = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, bool> SuppressTransformAnimationProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, bool>(
            nameof(SuppressTransformAnimation),
            o => o.SuppressTransformAnimation,
            (o, v) => o.SuppressTransformAnimation = v);

    internal static readonly DirectProperty<ImagePreviewerDialog, string?> EffectivePreviewTitleProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, string?>(
            nameof(EffectivePreviewTitle),
            o => o.EffectivePreviewTitle,
            (o, v) => o.EffectivePreviewTitle = v);

    private IImage? _currentImage;

    internal IImage? CurrentImage
    {
        get => _currentImage;
        set => SetAndRaise(CurrentImageProperty, ref _currentImage, value);
    }

    private bool _isMultiImages;

    internal bool IsMultiImages
    {
        get => _isMultiImages;
        set => SetAndRaise(IsMultiImagesProperty, ref _isMultiImages, value);
    }

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

    private bool _isCurrentImageLoading;

    internal bool IsCurrentImageLoading
    {
        get => _isCurrentImageLoading;
        set => SetAndRaise(IsCurrentImageLoadingProperty, ref _isCurrentImageLoading, value);
    }

    private bool _isCurrentImageFailed;

    internal bool IsCurrentImageFailed
    {
        get => _isCurrentImageFailed;
        set => SetAndRaise(IsCurrentImageFailedProperty, ref _isCurrentImageFailed, value);
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

    private double _imageScaleX;

    internal double ImageScaleX
    {
        get => _imageScaleX;
        set => SetAndRaise(ImageScaleXProperty, ref _imageScaleX, value);
    }

    private double _imageScaleY;

    internal double ImageScaleY
    {
        get => _imageScaleY;
        set => SetAndRaise(ImageScaleYProperty, ref _imageScaleY, value);
    }

    private bool _imageScaleChanged;

    internal bool ImageScaleChanged
    {
        get => _imageScaleChanged;
        set => SetAndRaise(ImageScaleChangedProperty, ref _imageScaleChanged, value);
    }

    private double _imageRotate;

    internal double ImageRotate
    {
        get => _imageRotate;
        set => SetAndRaise(ImageRotateProperty, ref _imageRotate, value);
    }

    private bool _isImageFitToWindow = true;

    internal bool IsImageFitToWindow
    {
        get => _isImageFitToWindow;
        set => SetAndRaise(IsImageFitToWindowProperty, ref _isImageFitToWindow, value);
    }

    private bool _suppressTransformAnimation;

    internal bool SuppressTransformAnimation
    {
        get => _suppressTransformAnimation;
        set => SetAndRaise(SuppressTransformAnimationProperty, ref _suppressTransformAnimation, value);
    }

    private string? _effectivePreviewTitle;

    internal string? EffectivePreviewTitle
    {
        get => _effectivePreviewTitle;
        set => SetAndRaise(EffectivePreviewTitleProperty, ref _effectivePreviewTitle, value);
    }

    #endregion

    #region 内部协作 API

    internal void RefreshImageSwitchMode()
    {
        _displayTracker.Refresh();
    }

    #endregion

    private readonly AbstractImagePreviewer _imagePreviewer;

    private readonly ImageViewer _imageViewer;

    private readonly ImagePreviewDisplayTracker _displayTracker;

    private INotifyCollectionChanged? _observedItemsSource;

    private bool _firstSizeCalculated;

    private int _windowResizeTransformSuppressionVersion;

    static ImagePreviewerDialog()
    {
        TitleAlignmentProperty.OverrideDefaultValue<ImagePreviewerDialog>(WindowTitleBarTitleAlignment.WindowCenter);
        AffectsRender<ImagePreviewerDialog>(CurrentImageProperty);
        ImagePreviewBaseToolbar.HorizontalFlipRequestEvent.AddClassHandler<ImagePreviewerDialog>((dialog, args) =>
        {
            dialog.HandleHorizontalFlipRequest();
            args.Handled = true;
        });
        ImagePreviewBaseToolbar.VerticalFlipRequestEvent.AddClassHandler<ImagePreviewerDialog>((dialog, args) =>
        {
            dialog.HandleVerticalFlipRequest();
            args.Handled = true;
        });
        ImagePreviewBaseToolbar.RotateLeftRequestEvent.AddClassHandler<ImagePreviewerDialog>((dialog, args) =>
        {
            dialog.HandleRotateLeftRequest();
            args.Handled = true;
        });
        ImagePreviewBaseToolbar.RotateRightRequestEvent.AddClassHandler<ImagePreviewerDialog>((dialog, args) =>
        {
            dialog.HandleRotateRightRequest();
            args.Handled = true;
        });
        ImagePreviewBaseToolbar.ScaleDownRequestEvent.AddClassHandler<ImagePreviewerDialog>((dialog, args) =>
        {
            dialog.HandleScaleDownRequest();
            args.Handled = true;
        });
        ImagePreviewBaseToolbar.ScaleUpRequestEvent.AddClassHandler<ImagePreviewerDialog>((dialog, args) =>
        {
            dialog.HandleScaleUpRequest();
            args.Handled = true;
        });
        ImagePreviewBaseToolbar.FitToWindowRequestEvent.AddClassHandler<ImagePreviewerDialog>((dialog, args) =>
        {
            dialog.HandleFitToWindowRequest();
            args.Handled = true;
        });
        ImagePreviewBaseToolbar.PreviousRequestEvent.AddClassHandler<ImagePreviewerDialog>((dialog, args) =>
        {
            dialog.HandlePreviousRequest();
            args.Handled = true;
        });
        ImagePreviewBaseToolbar.NextRequestEvent.AddClassHandler<ImagePreviewerDialog>((dialog, args) =>
        {
            dialog.HandleNextImageRequest();
            args.Handled = true;
        });
        ImageViewer.PreviousRequestEvent.AddClassHandler<ImagePreviewerDialog>((dialog, args) =>
        {
            dialog.HandlePreviousRequest();
            args.Handled = true;
        });
        ImageViewer.NextRequestEvent.AddClassHandler<ImagePreviewerDialog>((dialog, args) =>
        {
            dialog.HandleNextImageRequest();
            args.Handled = true;
        });
        ImageViewer.ScaleDownRequestEvent.AddClassHandler<ImagePreviewerDialog>((dialog, args) =>
        {
            var viewer = args.Source as ImageViewer;
            if (viewer is not null)
            {
                dialog.SyncTransformFromViewer();
            }
            args.Handled = true;
        });
        ImageViewer.ScaleUpRequestEvent.AddClassHandler<ImagePreviewerDialog>((dialog, args) =>
        {
            var viewer = args.Source as ImageViewer;
            if (viewer is not null)
            {
                dialog.SyncTransformFromViewer();
            }
            args.Handled = true;
        });
    }

    public ImagePreviewerDialog(TopLevel parent, AbstractImagePreviewer imagePreviewer)
    {
        ParentTopLevel  = parent;
        _imagePreviewer = imagePreviewer;
        _displayTracker = new ImagePreviewDisplayTracker(
            () => _imagePreviewer.ImageSwitchMode,
            UpdateCurrentImageState,
            () => _imagePreviewer.LatestLoadedFullEntry);
        _imageViewer    = CreateImageViewer();
        _imageViewer.PreviewChanged += HandleViewerPreviewChanged;
        // popup.root：native dialog 的内容根包裹 Panel，用生成的 SemanticPart 常量注入
        // marker（overlay 宿主在模板根静态声明同一 marker）。owner 作用域 Semantic Style
        // 不跨 Window/TopLevel 级联，仅 overlay 宿主可命中，见 semantic-part.md。
        Content = new Panel
        {
            Children = { _imageViewer },
            Classes  = { ImagePreviewerSemanticParts.PopupRootClass }
        };
        SetCurrentValue(TitleProperty, null);

        AddHandler(KeyDownEvent, HandleDialogKeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
    }

    protected override Type StyleKeyOverride { get; } = typeof(ImagePreviewerDialog);

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        HandleCurrentIndexChanged();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var calculatedSize = CalculateWindowSize();
        base.MeasureOverride(calculatedSize);
        return calculatedSize;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!e.IsProgrammatic || CloseByClickCloseCaptionButton)
        {
            e.Cancel = true;
            Dispatcher.Post(() =>
            {
                CloseByClickCloseCaptionButton = false;
                _imagePreviewer.NotifyDialogHostCloseRequest();
            });
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CurrentIndexProperty)
        {
            HandleCurrentIndexChanged();
        }
        else if (change.Property == ItemsSourceProperty)
        {
            ObserveItemsSource();
            SetCurrentValue(IsMultiImagesProperty, ItemsSource?.Count > 1);
            Count = ItemsSource?.Count ?? 0;
            HandleCurrentIndexChanged();
        }
        else if (change.Property == TitleProperty ||
                 change.Property == PreviewTitleResolverProperty)
        {
            UpdateEffectivePreviewTitle();
        }
        else if (change.Property == WindowStateProperty)
        {
            SuppressResizeMotion();
        }

        if (change.Property == ImageScaleXProperty ||
            change.Property == ImageScaleYProperty ||
            change.Property == MinScaleProperty ||
            change.Property == MaxScaleProperty)
        {
            UpdateScaleCapability();
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        SuppressResizeMotion();
    }

    public void Close(Action? callback = null)
    {
        UnobserveItemsSource();
        _imageViewer.PreviewChanged -= HandleViewerPreviewChanged;
        _displayTracker.Clear();
        base.Close();
        callback?.Invoke();
    }

    protected override WindowTitleBar? NotifyCreateTitleBar(WindowTitleBar? oldTitleBar)
    {
        return new ImagePreviewerTitleBar
        {
            Name = "PART_TitleBar"
        };
    }

    protected override void NotifyConfigureTitleBar(WindowTitleBar titleBar)
    {
        base.NotifyConfigureTitleBar(titleBar);

        // 使用预览标题算法结果，而不是直接展示 Window.Title。
        titleBar[!WindowTitleBar.TitleProperty] = this[!EffectivePreviewTitleProperty];

        if (titleBar is ImagePreviewerTitleBar previewerTitleBar)
        {
            previewerTitleBar[!ImagePreviewerTitleBar.IconProperty]               = this[!TitleIconProperty];
            previewerTitleBar[!ImagePreviewerTitleBar.CurrentIndexProperty]       = this[!CurrentIndexProperty];
            previewerTitleBar[!ImagePreviewerTitleBar.CountProperty]              = this[!CountProperty];
            previewerTitleBar[!ImagePreviewerTitleBar.IsScaleDownEnabledProperty] = this[!IsScaleDownEnabledProperty];
            previewerTitleBar[!ImagePreviewerTitleBar.IsScaleUpEnabledProperty]   = this[!IsScaleUpEnabledProperty];
            previewerTitleBar[!ImagePreviewerTitleBar.IsImageFitToWindowProperty] = this[!IsImageFitToWindowProperty];
            previewerTitleBar[!ImagePreviewerTitleBar.IsFirstImageProperty]       = this[!IsFirstImageProperty];
            previewerTitleBar[!ImagePreviewerTitleBar.IsLastImageProperty]        = this[!IsLastImageProperty];
        }
    }

    private ImageViewer CreateImageViewer()
    {
        // The viewer is code-created Content, outside this host's ControlTemplate.
        // ControlTheme cannot reach it through /template/; these bindings live with the host.
        var viewer = new ImageViewer
        {
            Name = "PART_ImageViewer"
        };
        viewer[!ImageViewer.BackgroundProperty]                 = this[!BackgroundProperty];
        viewer[!ImageViewer.CountProperty]                      = this[!CountProperty];
        viewer[!ImageViewer.CurrentIndexProperty]               = this[!CurrentIndexProperty];
        viewer[!ImageViewer.CurrentImageProperty]               = this[!CurrentImageProperty];
        viewer[!ImageViewer.IsCurrentImageLoadingProperty]      = this[!IsCurrentImageLoadingProperty];
        viewer[!ImageViewer.IsCurrentImageFailedProperty]       = this[!IsCurrentImageFailedProperty];
        viewer[!ImageViewer.LoadingContentProperty]             = this[!LoadingContentProperty];
        viewer[!ImageViewer.LoadingContentTemplateProperty]     = this[!LoadingContentTemplateProperty];
        viewer[!ImageViewer.ErrorContentProperty]               = this[!ErrorContentProperty];
        viewer[!ImageViewer.ErrorContentTemplateProperty]       = this[!ErrorContentTemplateProperty];
        viewer[!ImageViewer.IsFirstImageProperty]               = this[!IsFirstImageProperty];
        viewer[!ImageViewer.IsLastImageProperty]                = this[!IsLastImageProperty];
        viewer[!ImageViewer.IsMultiImagesProperty]              = this[!IsMultiImagesProperty];
        viewer[!ImageViewer.IsMotionEnabledProperty]            = this[!IsMotionEnabledProperty];
        viewer[!ImageViewer.ImageScaleXProperty]                = this[!ImageScaleXProperty];
        viewer[!ImageViewer.ImageScaleYProperty]                = this[!ImageScaleYProperty];
        viewer[!ImageViewer.ImageRotateProperty]                = this[!ImageRotateProperty];
        viewer[!ImageViewer.IsImageMovableProperty]             = this[!IsImageMovableProperty];
        viewer[!ImageViewer.MinScaleProperty]                   = this[!MinScaleProperty];
        viewer[!ImageViewer.MaxScaleProperty]                   = this[!MaxScaleProperty];
        viewer[!ImageViewer.ScaleStepProperty]                  = this[!ScaleStepProperty];
        viewer[!ImageViewer.IsWheelZoomEnabledProperty]         = this[!IsWheelZoomEnabledProperty];
        viewer[!ImageViewer.IsImageFitToWindowProperty]         = this[!IsImageFitToWindowProperty];
        viewer[!ImageViewer.SuppressTransformAnimationProperty] = this[!SuppressTransformAnimationProperty];
        viewer[!ImageViewer.ImageScaleChangedProperty]          = this[!ImageScaleChangedProperty];
        return viewer;
    }

    private Size CalculateWindowSize()
    {
        if (!_firstSizeCalculated)
        {
            var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
            if (screen is null)
            {
                _firstSizeCalculated = true;
                return ClientSize;
            }

            const double ratio     = 0.70d;
            var workingArea        = screen.WorkingArea;
            var scale              = DesktopScaling;
            var logicalWorkingSize = new Size(workingArea.Width / scale, workingArea.Height / scale);
            var width              = logicalWorkingSize.Width * ratio;
            var height             = logicalWorkingSize.Height * ratio;
            ClientSize             = new Size(width, height);
            _firstSizeCalculated   = true;
            return new Size(width, height);
        }
        return ClientSize;
    }

    private void SuppressResizeMotion()
    {
        SetCurrentValue(SuppressTransformAnimationProperty, true);
        var version = ++_windowResizeTransformSuppressionVersion;
        Dispatcher.Post(() =>
        {
            if (version == _windowResizeTransformSuppressionVersion)
            {
                SetCurrentValue(SuppressTransformAnimationProperty, false);
            }
        }, DispatcherPriority.Background);
    }

    private void HandleCurrentIndexChanged()
    {
        if (ItemsSource is { Count: > 0 } items)
        {
            var currentIndex = ResolveDisplayCurrentIndex(items.Count);
            var entry = items[currentIndex];
            if (!ReferenceEquals(_displayTracker.CurrentItem, entry))
            {
                _imageViewer.ApplyReset();
            }
            _displayTracker.SetCurrentItem(entry);
            SetCurrentValue(IsFirstImageProperty, currentIndex == 0);
            SetCurrentValue(IsLastImageProperty, currentIndex == items.Count - 1);
        }
        else if (ItemsSource == null || ItemsSource?.Count == 0)
        {
            _displayTracker.SetCurrentItem(null);
            SetCurrentValue(IsLastImageProperty, false);
            SetCurrentValue(IsFirstImageProperty, false);
        }

        UpdateEffectivePreviewTitle();
    }

    private void UpdateCurrentImageState()
    {
        _imageViewer.SetImageSize(_displayTracker.EffectiveImageSize);
        SetCurrentValue(CurrentImageProperty, _displayTracker.EffectiveImage);
        SetCurrentValue(IsCurrentImageLoadingProperty, _displayTracker.IsCurrentLoading);
        SetCurrentValue(IsCurrentImageFailedProperty, _displayTracker.IsCurrentFailed);
        HandleViewerPreviewChanged(_imageViewer, EventArgs.Empty);
    }

    private void UpdateEffectivePreviewTitle()
    {
        if (!string.IsNullOrWhiteSpace(Title))
        {
            SetCurrentValue(EffectivePreviewTitleProperty, Title);
            return;
        }

        SetCurrentValue(EffectivePreviewTitleProperty, ResolvePreviewTitle());
    }

    private string? ResolvePreviewTitle()
    {
        if (ItemsSource is not { Count: > 0 } items)
        {
            return null;
        }

        var currentIndex = ResolveDisplayCurrentIndex(items.Count);
        var item = items[currentIndex].Item;
        if (!string.IsNullOrWhiteSpace(item.Title))
        {
            return item.Title;
        }
        if (PreviewTitleResolver is null)
        {
            return null;
        }
        var context = new ImagePreviewTitleResolveContext(item, currentIndex, items.Count);
        var title = PreviewTitleResolver.ResolveTitle(in context);
        return string.IsNullOrWhiteSpace(title) ? null : title;
    }

    private int ResolveDisplayCurrentIndex(int count)
    {
        if (CurrentIndex < 0)
        {
            return 0;
        }

        if (CurrentIndex >= count)
        {
            return count - 1;
        }

        return CurrentIndex;
    }

    private void UpdateScaleCapability()
    {
        SetCurrentValue(IsScaleDownEnabledProperty, _imageViewer.IsScaleDownEnabled);
        SetCurrentValue(IsScaleUpEnabledProperty, _imageViewer.IsScaleUpEnabled);
    }

    private void ObserveItemsSource()
    {
        var source = ItemsSource as INotifyCollectionChanged;
        if (ReferenceEquals(_observedItemsSource, source))
        {
            return;
        }
        if (_observedItemsSource is not null)
        {
            _observedItemsSource.CollectionChanged -= HandleItemsSourceCollectionChanged;
        }
        _observedItemsSource = source;
        if (_observedItemsSource is not null)
        {
            _observedItemsSource.CollectionChanged += HandleItemsSourceCollectionChanged;
        }
    }

    private void UnobserveItemsSource()
    {
        if (_observedItemsSource is not null)
        {
            _observedItemsSource.CollectionChanged -= HandleItemsSourceCollectionChanged;
            _observedItemsSource = null;
        }
    }

    private void HandleItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        // 有效集合增量变更时 CurrentIndex 属性值可能不变（如封顶裁剪后索引稳定），
        // 但索引处的 entry 已更换：宿主必须与 previewer 的 ConfigureCurrentEntry 同步重配
        // 当前项，否则显示跟踪会持有过期（可能已 Dispose）的 entry，表现为预览窗口白屏。
        SetCurrentValue(IsMultiImagesProperty, ItemsSource?.Count > 1);
        Count = ItemsSource?.Count ?? 0;
        HandleCurrentIndexChanged();
    }

    private void HandleHorizontalFlipRequest()
    {
        if (CurrentImage == null)
        {
            return;
        }

        _imageViewer.ApplyFlip(horizontal: true);
    }

    private void HandleVerticalFlipRequest()
    {
        if (CurrentImage == null)
        {
            return;
        }

        _imageViewer.ApplyFlip(horizontal: false);
    }

    private void HandleRotateLeftRequest()
    {
        if (CurrentImage == null)
        {
            return;
        }

        _imageViewer.ApplyRotate(-90);
    }

    private void HandleRotateRightRequest()
    {
        if (CurrentImage == null)
        {
            return;
        }

        _imageViewer.ApplyRotate(90);
    }

    private void HandleScaleDownRequest(double? scaleStep = null)
    {
        _imageViewer.ApplyScaleDown(scaleStep);
    }

    private void HandleScaleUpRequest(double? scaleStep = null)
    {
        _imageViewer.ApplyScaleUp(scaleStep);
    }

    private void HandleViewerPreviewChanged(object? sender, EventArgs e)
    {
        SyncTransformFromViewer();
        _imagePreviewer.NotifyPreviewDecodeSize(
            _displayTracker.CurrentItem,
            _imageViewer.DecodeImageSize,
            _imageViewer.DecodeSize);
    }

    private void SyncTransformFromViewer()
    {
        SetCurrentValue(ImageScaleXProperty, _imageViewer.ImageScaleX);
        SetCurrentValue(ImageScaleYProperty, _imageViewer.ImageScaleY);
        SetCurrentValue(ImageRotateProperty, _imageViewer.ImageRotate);
        SetCurrentValue(ImageScaleChangedProperty, _imageViewer.ImageScaleChanged);
        UpdateScaleCapability();
    }

    private void HandleImageSwitchKeyDown(KeyEventArgs e)
    {
        var imageCount = ItemsSource?.Count ?? Count;
        if (imageCount <= 1)
        {
            return;
        }

        if (e.Key == Key.Right)
        {
            e.Handled = true;
            if (CurrentIndex < imageCount - 1)
            {
                HandleNextImageRequest();
            }
        }
        else if (e.Key == Key.Left)
        {
            e.Handled = true;
            if (CurrentIndex > 0)
            {
                HandlePreviousRequest();
            }
        }
    }

    private void HandleDialogKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        HandleImageSwitchKeyDown(e);
    }

    private void HandleFitToWindowRequest()
    {
        if (CurrentImage == null)
        {
            return;
        }

        _imageViewer.ApplyReset();
        SetCurrentValue(IsImageFitToWindowProperty, true);
    }

    private void EnsureDialogKeyboardFocus()
    {
        if (_imageViewer.Focus() != true)
        {
            Focus();
        }

        Dispatcher.Post(() =>
        {
            if (_imageViewer.Focus() == true)
            {
                return;
            }

            Focus();
        }, DispatcherPriority.Background);
    }

    private void SwitchImage(int offset)
    {
        if (Count <= 0 || offset == 0)
        {
            return;
        }

        var targetIndex = offset > 0
            ? Math.Min(CurrentIndex + offset, Count - 1)
            : Math.Max(CurrentIndex + offset, 0);
        SetCurrentValue(CurrentIndexProperty, targetIndex);
        EnsureDialogKeyboardFocus();
    }

    private void HandleNextImageRequest()
    {
        SwitchImage(1);
    }

    private void HandlePreviousRequest()
    {
        SwitchImage(-1);
    }
}
