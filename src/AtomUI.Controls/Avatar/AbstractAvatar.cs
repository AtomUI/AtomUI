using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Controls;

internal enum AvatarContentType
{
    Icon,
    Image,
    Text
}

[PseudoClasses(":loading", ":loaded", ":failed", ":fallback")]
[TemplatePart("PART_TextPresenter", typeof(TextBlock))]
public abstract class AbstractAvatar : TemplatedControl, IMotionAwareControl, IImageLoadControl, IImageLoadControllerHost, IThemeInheritanceSource
{
    #region 公共属性定义

    public static readonly StyledProperty<double> GapProperty =
        AvaloniaProperty.Register<AbstractAvatar, double>(nameof(Gap), 4.0);

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractAvatar, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<ImageSource?> SourceProperty =
        AvaloniaProperty.Register<AbstractAvatar, ImageSource?>(nameof(Source));

    public static readonly StyledProperty<ImageSource?> FallbackSourceProperty =
        AvaloniaProperty.Register<AbstractAvatar, ImageSource?>(nameof(FallbackSource));

    public static readonly StyledProperty<ImageRequestOptions?> RequestOptionsProperty =
        AvaloniaProperty.Register<AbstractAvatar, ImageRequestOptions?>(nameof(RequestOptions));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AbstractAvatar, string?>(nameof(Text));

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        AvaloniaProperty.Register<AbstractAvatar, CustomizableSizeType>(
            nameof(SizeType),
            CustomizableSizeType.Middle);

    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<AbstractAvatar, double>(nameof(Size), double.NaN);

    public static readonly StyledProperty<AvatarShape> ShapeProperty =
        AvaloniaProperty.Register<AbstractAvatar, AvatarShape>(nameof(Shape), AvatarShape.Circle);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractAvatar>();

    public static readonly DirectProperty<AbstractAvatar, ImageLoadState> LoadStateProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, ImageLoadState>(nameof(LoadState), control => control.LoadState);

    public static readonly DirectProperty<AbstractAvatar, ImageLoadError?> LoadErrorProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, ImageLoadError?>(nameof(LoadError), control => control.LoadError);

    public static readonly DirectProperty<AbstractAvatar, ImageLoadProgress?> LoadProgressProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, ImageLoadProgress?>(nameof(LoadProgress), control => control.LoadProgress);

    public static readonly DirectProperty<AbstractAvatar, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, bool>(nameof(IsLoading), control => control.IsLoading);

    public static readonly DirectProperty<AbstractAvatar, bool> IsLoadedProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, bool>(nameof(IsLoaded), control => control.IsLoaded);

    public static readonly DirectProperty<AbstractAvatar, bool> IsFailedProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, bool>(nameof(IsFailed), control => control.IsFailed);

    public double Gap
    {
        get => GetValue(GapProperty);
        set => SetValue(GapProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
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

    [Content]
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public AvatarShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    private ImageLoadState _loadState;

    public ImageLoadState LoadState => _loadState;

    private ImageLoadError? _loadError;

    public ImageLoadError? LoadError => _loadError;

    private ImageLoadProgress? _loadProgress;

    public ImageLoadProgress? LoadProgress => _loadProgress;

    private bool _isLoading;

    public bool IsLoading => _isLoading;

    private bool _isLoaded;

    public new bool IsLoaded => _isLoaded;

    private bool _isFailed;

    public bool IsFailed => _isFailed;

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> EffectiveIconSizeProperty =
        AvaloniaProperty.Register<AbstractAvatar, double>(nameof(EffectiveIconSize));

    internal static readonly StyledProperty<Thickness> TextPaddingProperty =
        AvaloniaProperty.Register<AbstractAvatar, Thickness>(nameof(TextPadding));

    internal static readonly DirectProperty<AbstractAvatar, AvatarContentType> ContentTypeProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, AvatarContentType>(
            nameof(ContentType),
            control => control.ContentType,
            (control, value) => control.ContentType = value);

    internal static readonly DirectProperty<AbstractAvatar, IImage?> LoadedImageProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, IImage?>(nameof(LoadedImage), control => control.LoadedImage);

    internal double EffectiveIconSize
    {
        get => GetValue(EffectiveIconSizeProperty);
        set => SetValue(EffectiveIconSizeProperty, value);
    }

    internal Thickness TextPadding
    {
        get => GetValue(TextPaddingProperty);
        set => SetValue(TextPaddingProperty, value);
    }

    private AvatarContentType _contentType = AvatarContentType.Icon;

    internal AvatarContentType ContentType
    {
        get => _contentType;
        set => SetAndRaise(ContentTypeProperty, ref _contentType, value);
    }

    private IImage? _loadedImage;

    internal IImage? LoadedImage => _loadedImage;

    #endregion

    #region 运行时字段

    private readonly ImageLoadController _controller;

    private CustomizableSizeType? _originSizeType;

    private bool _isAttached;

    #endregion

    #region 公共事件定义

    public event EventHandler<ImageOpenedEventArgs>? ImageOpened;

    public event EventHandler<ImageFailedEventArgs>? ImageFailed;

    #endregion

    static AbstractAvatar()
    {
        AffectsMeasure<AbstractAvatar>(SizeTypeProperty, TextProperty, GapProperty);
        AffectsRender<AbstractAvatar>(ShapeProperty, IconProperty, SourceProperty, GapProperty);
    }

    protected AbstractAvatar()
    {
        _controller = new ImageLoadController(this);
    }

    #region 公共与受保护 API

    public void Reload() => _controller.Reload();

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SizeProperty)
        {
            ConfigureCustomSize();
        }

        if (change.Property == SourceProperty ||
            change.Property == FallbackSourceProperty ||
            change.Property == RequestOptionsProperty)
        {
            _controller.SourceConfigurationChanged();
        }
        if (change.Property == IconProperty || change.Property == TextProperty)
        {
            ConfigureContentType();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureContentType();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var arranged = base.ArrangeOverride(finalSize);
        _controller.RefreshSize(GetDecodePixelSize(arranged));
        return arranged;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        _controller.Attach();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        _controller.Detach();
        base.OnDetachedFromVisualTree(e);
    }

    #endregion

    #region 实现 IImageLoadControllerHost 接口

    Visual IImageLoadControllerHost.Visual => this;

    ImageRequestPriority IImageLoadControllerHost.Priority => ImageRequestPriority.Normal;

    bool IImageLoadControllerHost.IsImageLoadAttached => _isAttached;

    (int Width, int Height)? IImageLoadControllerHost.GetDecodePixelSize()
    {
        return GetDecodePixelSize(Bounds.Size);
    }

    ImageLoadError? IImageLoadControllerHost.GetConfigurationError() => null;

    void IImageLoadControllerHost.SetLoadedImage(IImage? image)
    {
        SetAndRaise(LoadedImageProperty, ref _loadedImage, image);
        ConfigureContentType();
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

    #endregion

    #region 实现 IThemeInheritanceSource 接口

    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

    private (int Width, int Height)? GetDecodePixelSize(Size arrangedSize)
    {
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var width = Quantize(arrangedSize.Width * scaling);
        var height = Quantize(arrangedSize.Height * scaling);
        return width == 0 && height == 0 ? null : (width, height);
    }

    private void ConfigureCustomSize()
    {
        if (!double.IsNaN(Size))
        {
            if (SizeType != CustomizableSizeType.Custom)
            {
                _originSizeType = SizeType;
            }
            SizeType = CustomizableSizeType.Custom;
        }
        else if (_originSizeType.HasValue)
        {
            SizeType = _originSizeType.Value;
            _originSizeType = null;
        }
    }

    private void ConfigureContentType()
    {
        ContentType = LoadedImage is not null
            ? AvatarContentType.Image
            : Text is not null
                ? AvatarContentType.Text
                : AvatarContentType.Icon;
    }

    private static int Quantize(double value)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            return 0;
        }
        return checked((int)(Math.Ceiling(value / 16) * 16));
    }

}
