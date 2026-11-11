using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Controls;

[PseudoClasses(":loading", ":loaded", ":failed", ":fallback", ":has-image")]
public class AsyncImage : TemplatedControl, IImageLoadControl, IImageLoadControllerHost
{
    public static readonly StyledProperty<ImageSource?> SourceProperty =
        AvaloniaProperty.Register<AsyncImage, ImageSource?>(nameof(Source));

    public static readonly StyledProperty<ImageSource?> FallbackSourceProperty =
        AvaloniaProperty.Register<AsyncImage, ImageSource?>(nameof(FallbackSource));

    public static readonly StyledProperty<ImageRequestOptions?> RequestOptionsProperty =
        AvaloniaProperty.Register<AsyncImage, ImageRequestOptions?>(nameof(RequestOptions));

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<AsyncImage, Stretch>(nameof(Stretch), Stretch.Uniform);

    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<AsyncImage, StretchDirection>(nameof(StretchDirection), StretchDirection.Both);

    public static readonly StyledProperty<ImageDecodeMode> DecodeModeProperty =
        AvaloniaProperty.Register<AsyncImage, ImageDecodeMode>(nameof(DecodeMode), ImageDecodeMode.Auto);

    public static readonly StyledProperty<int> DecodePixelWidthProperty =
        AvaloniaProperty.Register<AsyncImage, int>(nameof(DecodePixelWidth), validate: value => value >= 0);

    public static readonly StyledProperty<int> DecodePixelHeightProperty =
        AvaloniaProperty.Register<AsyncImage, int>(nameof(DecodePixelHeight), validate: value => value >= 0);

    public static readonly StyledProperty<ImageRequestPriority> PriorityProperty =
        AvaloniaProperty.Register<AsyncImage, ImageRequestPriority>(nameof(Priority), ImageRequestPriority.Normal);

    public static readonly StyledProperty<bool> IsLazyProperty =
        AvaloniaProperty.Register<AsyncImage, bool>(nameof(IsLazy));

    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<AsyncImage, object?>(nameof(LoadingContent));

    public static readonly StyledProperty<IDataTemplate?> LoadingContentTemplateProperty =
        AvaloniaProperty.Register<AsyncImage, IDataTemplate?>(nameof(LoadingContentTemplate));

    public static readonly StyledProperty<object?> ErrorContentProperty =
        AvaloniaProperty.Register<AsyncImage, object?>(nameof(ErrorContent));

    public static readonly StyledProperty<IDataTemplate?> ErrorContentTemplateProperty =
        AvaloniaProperty.Register<AsyncImage, IDataTemplate?>(nameof(ErrorContentTemplate));

    public static readonly DirectProperty<AsyncImage, ImageLoadState> LoadStateProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, ImageLoadState>(nameof(LoadState), control => control.LoadState);

    public static readonly DirectProperty<AsyncImage, ImageLoadError?> LoadErrorProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, ImageLoadError?>(nameof(LoadError), control => control.LoadError);

    public static readonly DirectProperty<AsyncImage, ImageLoadProgress?> LoadProgressProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, ImageLoadProgress?>(nameof(LoadProgress), control => control.LoadProgress);

    public static readonly DirectProperty<AsyncImage, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, bool>(nameof(IsLoading), control => control.IsLoading);

    public static readonly DirectProperty<AsyncImage, bool> IsLoadedProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, bool>(nameof(IsLoaded), control => control.IsLoaded);

    public static readonly DirectProperty<AsyncImage, bool> IsFailedProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, bool>(nameof(IsFailed), control => control.IsFailed);

    internal static readonly DirectProperty<AsyncImage, IImage?> LoadedImageProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, IImage?>(nameof(LoadedImage), control => control.LoadedImage);

    private readonly ImageLoadController _controller;
    private ImageLoadState _loadState;
    private ImageLoadError? _loadError;
    private ImageLoadProgress? _loadProgress;
    private bool _isLoading;
    private bool _isLoaded;
    private bool _isFailed;
    private IImage? _loadedImage;
    private bool _isAttached;
    private Size _lastMeasureAvailableSize;
    private bool _hasMeasureAvailableSize;
    private bool _isViewportGated;
    private bool _isReleased;
    private bool _isViewportSubscribed;
    private Rect? _lastEffectiveViewport;

    public AsyncImage()
    {
        _controller = new ImageLoadController(this);
    }

    public ImageSource? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public ImageSource? FallbackSource
    {
        get => GetValue(FallbackSourceProperty);
        set => SetValue(FallbackSourceProperty, value);
    }

    public ImageRequestOptions? RequestOptions
    {
        get => GetValue(RequestOptionsProperty);
        set => SetValue(RequestOptionsProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }

    public ImageDecodeMode DecodeMode
    {
        get => GetValue(DecodeModeProperty);
        set => SetValue(DecodeModeProperty, value);
    }

    public int DecodePixelWidth
    {
        get => GetValue(DecodePixelWidthProperty);
        set => SetValue(DecodePixelWidthProperty, value);
    }

    public int DecodePixelHeight
    {
        get => GetValue(DecodePixelHeightProperty);
        set => SetValue(DecodePixelHeightProperty, value);
    }

    public ImageRequestPriority Priority
    {
        get => GetValue(PriorityProperty);
        set => SetValue(PriorityProperty, value);
    }

    public bool IsLazy
    {
        get => GetValue(IsLazyProperty);
        set => SetValue(IsLazyProperty, value);
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

    public ImageLoadState LoadState => _loadState;

    public ImageLoadError? LoadError => _loadError;

    public ImageLoadProgress? LoadProgress => _loadProgress;

    public bool IsLoading => _isLoading;

    public new bool IsLoaded => _isLoaded;

    public bool IsFailed => _isFailed;

    internal IImage? LoadedImage => _loadedImage;

    public event EventHandler<ImageOpenedEventArgs>? ImageOpened;

    public event EventHandler<ImageFailedEventArgs>? ImageFailed;

    Visual IImageLoadControllerHost.Visual => this;

    bool IImageLoadControllerHost.IsImageLoadAttached => _isAttached && !_isViewportGated;

    public void Reload() => _controller.Reload();

    // Drops the current request and image but stays attached: the load resumes when the control next
    // intersects the viewport or Source changes. List recycling uses this to hand back image memory.
    internal void ReleaseLoad()
    {
        if (!_isAttached)
        {
            return;
        }
        _isReleased = true;
        _isViewportGated = true;
        _controller.Release();
        SubscribeViewport();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SourceProperty ||
            change.Property == FallbackSourceProperty ||
            change.Property == RequestOptionsProperty ||
            change.Property == DecodeModeProperty ||
            change.Property == DecodePixelWidthProperty ||
            change.Property == DecodePixelHeightProperty ||
            change.Property == PriorityProperty)
        {
            if (_isReleased && change.Property == SourceProperty)
            {
                OpenViewportGate();
                return;
            }
            _controller.SourceConfigurationChanged();
        }
        else if (change.Property == IsLazyProperty)
        {
            ConfigureLazy();
        }
        else if (change.Property == StretchProperty)
        {
            // Fill/UniformToFill decode to cover the box, Uniform/None fit inside it.
            _controller.RefreshSize();
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var arranged = base.ArrangeOverride(finalSize);
        _controller.RefreshSize(GetDecodePixelSize(arranged));
        return arranged;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _lastMeasureAvailableSize = availableSize;
        _hasMeasureAvailableSize = true;
        return base.MeasureOverride(availableSize);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        // The gate must be closed before Attach so that the first load waits for the viewport.
        _isViewportGated = IsLazy;
        if (_isViewportGated)
        {
            SubscribeViewport();
        }
        _controller.Attach();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        _hasMeasureAvailableSize = false;
        UnsubscribeViewport();
        _isViewportGated = false;
        _isReleased = false;
        _controller.Detach();
        base.OnDetachedFromVisualTree(e);
    }

    (int Width, int Height)? IImageLoadControllerHost.GetDecodePixelSize()
    {
        return GetDecodePixelSize(Bounds.Size);
    }

    bool IImageLoadControllerHost.DecodeToCover => DecodeMode == ImageDecodeMode.Auto && IsCoverStretch;

    private bool IsCoverStretch => Stretch is Stretch.Fill or Stretch.UniformToFill;

    private (int Width, int Height)? GetDecodePixelSize(Size arrangedSize)
    {
        return DecodeMode switch
        {
            ImageDecodeMode.Original => (0, 0),
            ImageDecodeMode.Explicit when DecodePixelWidth > 0 || DecodePixelHeight > 0 =>
                (DecodePixelWidth, DecodePixelHeight),
            ImageDecodeMode.Explicit => null,
            _ => GetAutoDecodeSize(arrangedSize)
        };
    }

    ImageLoadError? IImageLoadControllerHost.GetConfigurationError()
    {
        return DecodeMode == ImageDecodeMode.Explicit && DecodePixelWidth == 0 && DecodePixelHeight == 0
            ? new ImageLoadError(
                ImageLoadErrorCode.InvalidSource,
                "Explicit image decoding requires DecodePixelWidth or DecodePixelHeight to be greater than zero.",
                SourceDisplayName: Source?.DisplayName)
            : null;
    }

    void IImageLoadControllerHost.SetLoadedImage(IImage? image)
    {
        SetAndRaise(LoadedImageProperty, ref _loadedImage, image);
        PseudoClasses.Set(":has-image", image is not null);
    }

    void IImageLoadControllerHost.SetLoadState(
        ImageLoadState state,
        ImageLoadError? error,
        ImageLoadProgress? progress,
        bool isFallback)
    {
        SetAndRaise(LoadStateProperty, ref _loadState, state);
        SetAndRaise(LoadErrorProperty, ref _loadError, error);
        SetAndRaise(LoadProgressProperty, ref _loadProgress, progress);
        SetAndRaise(IsLoadingProperty, ref _isLoading, state == ImageLoadState.Loading);
        SetAndRaise(IsLoadedProperty, ref _isLoaded, state == ImageLoadState.Loaded);
        SetAndRaise(IsFailedProperty, ref _isFailed, state == ImageLoadState.Failed);
        PseudoClasses.Set(":loading", state == ImageLoadState.Loading);
        PseudoClasses.Set(":loaded", state == ImageLoadState.Loaded);
        PseudoClasses.Set(":failed", state == ImageLoadState.Failed);
        PseudoClasses.Set(":fallback", state == ImageLoadState.Loaded && isFallback);
    }

    void IImageLoadControllerHost.RaiseImageOpened(ImageOpenedEventArgs eventArgs) =>
        ImageLoadEventDispatcher.Dispatch(ImageOpened, this, eventArgs);

    void IImageLoadControllerHost.RaiseImageFailed(ImageFailedEventArgs eventArgs) =>
        ImageLoadEventDispatcher.Dispatch(ImageFailed, this, eventArgs);

    private void ConfigureLazy()
    {
        if (!_isAttached || _isReleased)
        {
            return;
        }
        if (IsLazy)
        {
            // An image that already started loading keeps its request; only an idle one waits for the viewport.
            if (!_isViewportGated && LoadState == ImageLoadState.Idle)
            {
                _isViewportGated = true;
                SubscribeViewport();
            }
        }
        else if (_isViewportGated)
        {
            OpenViewportGate();
        }
    }

    private void SubscribeViewport()
    {
        if (_isViewportSubscribed || !_isAttached)
        {
            return;
        }
        _isViewportSubscribed = true;
        EffectiveViewportChanged += OnEffectiveViewportChanged;
    }

    private void UnsubscribeViewport()
    {
        if (!_isViewportSubscribed)
        {
            return;
        }
        _isViewportSubscribed = false;
        _lastEffectiveViewport = null;
        EffectiveViewportChanged -= OnEffectiveViewportChanged;
    }

    private void OpenViewportGate()
    {
        UnsubscribeViewport();
        _isReleased = false;
        _isViewportGated = false;
        _controller.SourceConfigurationChanged();
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        _lastEffectiveViewport = e.EffectiveViewport;
        TryOpenViewportGate();
    }

    // A control growing from zero at a fixed origin can keep the same effective viewport, so size changes re-check.
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        TryOpenViewportGate();
    }

    private void TryOpenViewportGate()
    {
        if (_isViewportGated &&
            _lastEffectiveViewport is { } viewport &&
            IsEffectivelyVisible &&
            IntersectsViewport(viewport))
        {
            OpenViewportGate();
        }
    }

    // An auto-sized axis can measure zero while the image waits, for example width-only in a vertical StackPanel
    // without a placeholder; its edge still has a position and counts when that position lies inside the viewport.
    // An explicit zero Width or Height stays invisible.
    private bool IntersectsViewport(Rect viewport)
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (viewport.Width <= 0 ||
            viewport.Height <= 0 ||
            (width <= 0 && height <= 0) ||
            (width <= 0 && double.IsFinite(Width)) ||
            (height <= 0 && double.IsFinite(Height)))
        {
            return false;
        }
        var horizontal = width > 0
            ? viewport.X < width && viewport.Right > 0
            : viewport.X <= 0 && viewport.Right > 0;
        var vertical = height > 0
            ? viewport.Y < height && viewport.Bottom > 0
            : viewport.Y <= 0 && viewport.Bottom > 0;
        return horizontal && vertical;
    }

    private (int Width, int Height)? GetAutoDecodeSize(Size arrangedSize)
    {
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var width = Quantize(arrangedSize.Width * scaling);
        var height = Quantize((IsCoverStretch ? ResolveCoverHeight() : ResolveBoundHeight()) * scaling);
        return width == 0 && height == 0 ? null : (width, height);
    }

    // Upper bound of the displayed height for Uniform/None: Height, then MaxHeight, then the measured available height.
    private double ResolveBoundHeight()
    {
        if (double.IsFinite(Height))
        {
            return Math.Max(0, Height);
        }
        if (double.IsFinite(MaxHeight))
        {
            return Math.Max(0, MaxHeight);
        }
        if (_hasMeasureAvailableSize && double.IsFinite(_lastMeasureAvailableSize.Height))
        {
            return Math.Max(0, _lastMeasureAvailableSize.Height);
        }
        return 0;
    }

    // Fill/UniformToFill grow to any finite height bound once loaded, so that bound is the box to cover from the first
    // request; the arranged height before the load is only the placeholder's and would restart the decode afterwards.
    // The measured available height already includes MaxHeight and the parent's slot. Without a bound the height
    // follows the image's aspect ratio, so width alone decides.
    private double ResolveCoverHeight()
    {
        if (double.IsFinite(Height))
        {
            return Math.Max(0, Height);
        }
        if (_hasMeasureAvailableSize && double.IsFinite(_lastMeasureAvailableSize.Height))
        {
            return Math.Max(0, _lastMeasureAvailableSize.Height);
        }
        if (double.IsFinite(MaxHeight))
        {
            return Math.Max(0, MaxHeight);
        }
        return 0;
    }

    // Largest 16 px bucket that fits in an int; huge layout sizes (for example MaxHeight = 1e9) clamp here instead
    // of overflowing. The codec never upscales, so any bound past the source size behaves the same.
    private const int MaxDecodePixelSize = int.MaxValue / 16 * 16;

    private static int Quantize(double value)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            return 0;
        }
        var bucket = Math.Ceiling(value / 16) * 16;
        return bucket >= MaxDecodePixelSize ? MaxDecodePixelSize : (int)bucket;
    }
}
