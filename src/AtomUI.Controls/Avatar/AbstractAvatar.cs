using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Controls;

internal enum AvatarContentType
{
    Icon,
    Image,
    Text
}

/// <summary>
/// Base class of the avatar, which shows an image, a text or an icon in a circular or square frame. The loaded
/// image is shown when there is one, else <see cref="Text" />, else <see cref="Icon" />.
/// </summary>
/// <remarks>
/// The image is loaded asynchronously. A load starts when a <see cref="Source" /> is set and the avatar is
/// attached to a visual tree and arranged. It starts again when <see cref="Source" />,
/// <see cref="FallbackSource" /> or <see cref="RequestOptions" /> changes, when the decode size changes, and
/// when the avatar is attached again. <see cref="LoadState" /> reports the load, and the pseudo-classes
/// <c>:loading</c>, <c>:loaded</c>, <c>:failed</c> and <c>:fallback</c> follow that state; <c>:fallback</c> is
/// set in the loaded state when the shown image came from <see cref="FallbackSource" />. The default theme
/// targets <see cref="Avatar" />, so another subclass needs its own theme.
/// </remarks>
[PseudoClasses(":loading", ":loaded", ":failed", ":fallback")]
[TemplatePart("PART_TextPresenter", typeof(TextBlock))]
public abstract class AbstractAvatar : TemplatedControl, IMotionAwareControl, IImageLoadControl, IImageLoadControllerHost
{
    /// <summary>
    /// Identifies the <see cref="Gap" /> property.
    /// </summary>
    public static readonly StyledProperty<double> GapProperty =
        AvaloniaProperty.Register<AbstractAvatar, double>(nameof(Gap), 4.0);

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractAvatar, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="Source" /> property.
    /// </summary>
    public static readonly StyledProperty<ImageSource?> SourceProperty =
        AvaloniaProperty.Register<AbstractAvatar, ImageSource?>(nameof(Source));

    /// <summary>
    /// Identifies the <see cref="FallbackSource" /> property.
    /// </summary>
    public static readonly StyledProperty<ImageSource?> FallbackSourceProperty =
        AvaloniaProperty.Register<AbstractAvatar, ImageSource?>(nameof(FallbackSource));

    /// <summary>
    /// Identifies the <see cref="RequestOptions" /> property.
    /// </summary>
    public static readonly StyledProperty<ImageRequestOptions?> RequestOptionsProperty =
        AvaloniaProperty.Register<AbstractAvatar, ImageRequestOptions?>(nameof(RequestOptions));

    /// <summary>
    /// Identifies the <see cref="Text" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AbstractAvatar, string?>(nameof(Text));

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        AvaloniaProperty.Register<AbstractAvatar, CustomizableSizeType>(
            nameof(SizeType),
            CustomizableSizeType.Middle);

    /// <summary>
    /// Identifies the <see cref="Size" /> property.
    /// </summary>
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<AbstractAvatar, double>(nameof(Size), double.NaN);

    /// <summary>
    /// Identifies the <see cref="Shape" /> property.
    /// </summary>
    public static readonly StyledProperty<AvatarShape> ShapeProperty =
        AvaloniaProperty.Register<AbstractAvatar, AvatarShape>(nameof(Shape), AvatarShape.Circle);

    /// <summary>
    /// Identifies the <see cref="Stretch" /> property.
    /// </summary>
    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<AbstractAvatar, Stretch>(nameof(Stretch), Stretch.Uniform);

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractAvatar>();

    /// <summary>
    /// Identifies the <see cref="LoadState" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractAvatar, ImageLoadState> LoadStateProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, ImageLoadState>(nameof(LoadState), control => control.LoadState);

    /// <summary>
    /// Identifies the <see cref="LoadError" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractAvatar, ImageLoadError?> LoadErrorProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, ImageLoadError?>(nameof(LoadError), control => control.LoadError);

    /// <summary>
    /// Identifies the <see cref="LoadProgress" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractAvatar, ImageLoadProgress?> LoadProgressProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, ImageLoadProgress?>(nameof(LoadProgress), control => control.LoadProgress);

    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractAvatar, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, bool>(nameof(IsLoading), control => control.IsLoading);

    /// <summary>
    /// Identifies the <see cref="IsLoaded" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractAvatar, bool> IsLoadedProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, bool>(nameof(IsLoaded), control => control.IsLoaded);

    /// <summary>
    /// Identifies the <see cref="IsFailed" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractAvatar, bool> IsFailedProperty =
        AvaloniaProperty.RegisterDirect<AbstractAvatar, bool>(nameof(IsFailed), control => control.IsFailed);

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

    private readonly ImageLoadController _controller;
    private CustomizableSizeType? _originSizeType;
    // Each derived value keeps one replaceable Template-priority entry. Repeated SetValue at that priority would add a
    // new value frame per call, and discarding the handle could never release the value when its condition ends.
    private IDisposable? _customWidthValue;
    private IDisposable? _customHeightValue;
    private IDisposable? _customIconSizeValue;
    private IDisposable? _textPaddingValue;
    private IDisposable? _circleRadiusValue;
    private AvatarContentType _contentType = AvatarContentType.Icon;
    private IImage? _loadedImage;
    private ImageLoadState _loadState;
    private ImageLoadError? _loadError;
    private ImageLoadProgress? _loadProgress;
    private bool _isLoading;
    private bool _isLoaded;
    private bool _isFailed;
    private bool _isAttached;

    static AbstractAvatar()
    {
        AffectsMeasure<AbstractAvatar>(SizeTypeProperty, TextProperty, GapProperty);
        AffectsRender<AbstractAvatar>(ShapeProperty, IconProperty, SourceProperty, GapProperty);
    }

    protected AbstractAvatar()
    {
        _controller = new ImageLoadController(this);
        ConfigureTextPadding();
    }

    /// <summary>
    /// Space, in DIPs, between the left and right edges of the avatar and the area that <see cref="Text" /> is
    /// laid out in. Text that does not fit is scaled down. The icon and the image are not affected. The default
    /// is 4.
    /// </summary>
    public double Gap
    {
        get => GetValue(GapProperty);
        set => SetValue(GapProperty, value);
    }

    /// <summary>
    /// The icon shown in the center of the avatar while no image is loaded and <see cref="Text" /> is <see
    /// langword="null" />. It is drawn with the <see cref="TemplatedControl.Foreground" /> brush. Its size is half of
    /// <see cref="Size" /> while the size is custom, and otherwise follows <see cref="SizeType" />.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// The image that fills the avatar once it has loaded. Until then, and when the load fails without a fallback,
    /// the avatar shows <see cref="Text" /> or <see cref="Icon" /> instead. <see langword="null" /> removes the
    /// image and loads nothing.
    /// </summary>
    /// <remarks>
    /// Setting another source hides the image that is shown and loads the new one.
    /// </remarks>
    public ImageSource? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// The image that is loaded and shown when loading <see cref="Source" /> fails. It is not loaded while
    /// <see cref="Source" /> is <see langword="null" />. It is loaded like the main image, with the same
    /// <see cref="RequestOptions" /> and decode size.
    /// </summary>
    /// <remarks>
    /// A shown fallback raises <see cref="ImageOpened" /> with <see cref="ImageOpenedEventArgs.IsFallback" /> set
    /// and leaves <see cref="IsFailed" /> <see langword="false" />.
    /// </remarks>
    public ImageSource? FallbackSource
    {
        get => GetValue(FallbackSourceProperty);
        set => SetValue(FallbackSourceProperty, value);
    }

    /// <summary>
    /// The <see cref="ImageRequestOptions" /> of the loads of <see cref="Source" /> and
    /// <see cref="FallbackSource" />: their cache and network options. <see langword="null" /> lets the loader use
    /// the defaults of <see cref="ImageRequestOptions" />.
    /// </summary>
    public ImageRequestOptions? RequestOptions
    {
        get => GetValue(RequestOptionsProperty);
        set => SetValue(RequestOptionsProperty, value);
    }

    /// <summary>
    /// The text shown inside the avatar while no image is loaded. It is scaled down when it does not fit into the
    /// area that <see cref="Gap" /> leaves. <see langword="null" /> shows the <see cref="Icon" /> instead. It is the
    /// content property, so text written between the start and the end tag of the avatar in XAML also sets it.
    /// </summary>
    [Content]
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// The size preset of the avatar. <see cref="CustomizableSizeType.Large" />,
    /// <see cref="CustomizableSizeType.Middle" /> and <see cref="CustomizableSizeType.Small" /> make the theme set
    /// the width, the height, the icon size and, for the square shape, the corner radius.
    /// <see cref="CustomizableSizeType.Custom" /> takes the width and the height from <see cref="Size" />, and
    /// setting <see cref="Size" /> switches this property to it.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// The width and the height of the avatar, in DIPs, while <see cref="SizeType" /> is
    /// <see cref="CustomizableSizeType.Custom" />. Setting a value other than <c>NaN</c> switches
    /// <see cref="SizeType" /> to <c>Custom</c>; setting <c>NaN</c> again restores the size type that was replaced.
    /// The icon is drawn at half of this size. The default is <c>NaN</c>, which leaves the size to
    /// <see cref="SizeType" />.
    /// </summary>
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// Whether the avatar is a circle or a square. A circle gets a corner radius of half its width. A square gets
    /// the corner radius that the theme's border radius token of its size type gives. The default is a circle.
    /// </summary>
    public AvatarShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    /// <summary>
    /// How the loaded image is scaled into the avatar. <c>Stretch.Fill</c> and
    /// <c>Stretch.UniformToFill</c> also make the image decode large enough to cover the avatar; the other
    /// values make it decode to fit inside. The default is <c>Stretch.Uniform</c>.
    /// </summary>
    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    /// <summary>
    /// Whether the avatar takes part in motion. The default theme sets the value from the <c>EnableMotion</c>
    /// token, and <c>AvatarGroup</c> passes its own value down to its avatars. No transition of the default
    /// template depends on it.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// The state of the current image load. It is <see cref="ImageLoadState.Loaded" /> also while the fallback
    /// image is shown.
    /// </summary>
    public ImageLoadState LoadState => _loadState;

    /// <summary>
    /// Why the image load failed. It is <see langword="null" /> unless <see cref="LoadState" /> is
    /// <see cref="ImageLoadState.Failed" />. When the main image and the fallback both failed, it describes the
    /// fallback.
    /// </summary>
    public ImageLoadError? LoadError => _loadError;

    /// <summary>
    /// How far the running image load has advanced. It is <see langword="null" /> unless <see cref="LoadState" />
    /// is <see cref="ImageLoadState.Loading" />.
    /// </summary>
    public ImageLoadProgress? LoadProgress => _loadProgress;

    /// <summary>
    /// <see langword="true" /> while an image load is running.
    /// </summary>
    public bool IsLoading => _isLoading;

    /// <summary>
    /// <see langword="true" /> when an image load has ended with an image, which includes the fallback image. It is
    /// <see langword="false" /> again while a new load runs, even when the image of the previous load is still
    /// shown.
    /// </summary>
    /// <remarks>
    /// This property hides <see cref="Control.IsLoaded" />, which tells whether the avatar itself has completed
    /// layout and rendering. A reference of type <see cref="Control" /> or one of its base types still reads that
    /// property.
    /// </remarks>
    public new bool IsLoaded => _isLoaded;

    /// <summary>
    /// <see langword="true" /> when the image load ended with an error and no image is shown. It is
    /// <see langword="false" /> while the fallback image is shown.
    /// </summary>
    public bool IsFailed => _isFailed;

    /// <summary>
    /// Raised on the UI thread after a loaded image has become the shown image and <see cref="LoadState" /> has
    /// been set to loaded, so the handler sees the final state. This includes the fallback image;
    /// <see cref="ImageOpenedEventArgs.IsFallback" /> tells which image was shown.
    /// </summary>
    public event EventHandler<ImageOpenedEventArgs>? ImageOpened;

    /// <summary>
    /// Raised on the UI thread after a load ended with an error and no image is shown. <see cref="LoadState" />
    /// is <see cref="ImageLoadState.Failed" /> and <see cref="LoadError" /> is set when the handler runs. When the
    /// main image and the fallback both failed, the event reports the fallback.
    /// </summary>
    public event EventHandler<ImageFailedEventArgs>? ImageFailed;

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

    internal AvatarContentType ContentType
    {
        get => _contentType;
        set => SetAndRaise(ContentTypeProperty, ref _contentType, value);
    }

    internal IImage? LoadedImage => _loadedImage;

    Visual IImageLoadControllerHost.Visual => this;

    ImageRequestPriority IImageLoadControllerHost.Priority => ImageRequestPriority.Normal;

    bool IImageLoadControllerHost.IsImageLoadAttached => _isAttached;

    /// <summary>
    /// Loads <see cref="Source" /> again, reading the source instead of taking the cached content, so changes at
    /// the origin become visible. The image that is shown stays until the new load ends. Nothing happens while the
    /// avatar is detached from the visual tree or <see cref="Source" /> is <see langword="null" />.
    /// </summary>
    public void Reload() => _controller.Reload();

    /// <summary>
    /// Called when an Avalonia property of the avatar changes: it updates the size, the corner radius, the text
    /// padding and the shown content, and it restarts the image load or refreshes its decode size when an image
    /// property changes. An override must call the base method, otherwise the avatar no longer follows the
    /// changes of its properties.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SizeProperty)
        {
            var sizeType = SizeType;
            ConfigureCustomSize();
            // A SizeType change already re-ran the size configuration; SizeType stays Custom across consecutive Size
            // changes, so those run it here.
            if (SizeType == sizeType)
            {
                ConfigureIconSize();
            }
        }
        else if (change.Property == SizeTypeProperty)
        {
            ConfigureIconSize();
        }

        if (change.Property == SourceProperty ||
            change.Property == FallbackSourceProperty ||
            change.Property == RequestOptionsProperty)
        {
            _controller.SourceConfigurationChanged();
        }
        else if (change.Property == StretchProperty)
        {
            // Fill/UniformToFill decode to cover the avatar box, Uniform/None fit inside it.
            _controller.RefreshSize();
        }
        if (change.Property == IconProperty || change.Property == TextProperty)
        {
            ConfigureContentType();
        }
        if (change.Property == GapProperty)
        {
            ConfigureTextPadding();
        }
        else if (change.Property == ShapeProperty || change.Property == WidthProperty)
        {
            // Width follows Size and SizeType (style or template values), so tracking it keeps the circle radius exact.
            ConfigureShape();
        }
    }

    /// <summary>
    /// Runs the configuration of the shape, the icon size and the shown content again once the template is
    /// applied, because the corner radius needs the width that the theme supplies. An override must call the base
    /// method.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureShape();
        ConfigureIconSize();
        ConfigureContentType();
    }

    /// <summary>
    /// Arranges the template and passes the arranged size to the image loader as the decode size of the image: in
    /// device pixels, rounded up to a multiple of 16. The image is decoded only after the avatar has been arranged,
    /// and again when that size changes. An override must call the base method, otherwise the image never loads.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var arranged = base.ArrangeOverride(finalSize);
        _controller.RefreshSize(GetDecodePixelSize(arranged));
        return arranged;
    }

    /// <summary>
    /// Marks the avatar as attached and starts the image load that <see cref="Source" /> requests. An override
    /// must call the base method, otherwise the image never loads.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        _controller.Attach();
    }

    /// <summary>
    /// Cancels the running image load and releases the shown image before the base method raises
    /// <see cref="Visual.DetachedFromVisualTree" />. An override must call the base method, otherwise the load
    /// keeps running and the image is not released.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        _controller.Detach();
        base.OnDetachedFromVisualTree(e);
    }

    (int Width, int Height)? IImageLoadControllerHost.GetDecodePixelSize()
    {
        return GetDecodePixelSize(Bounds.Size);
    }

    private (int Width, int Height)? GetDecodePixelSize(Size arrangedSize)
    {
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var width = Quantize(arrangedSize.Width * scaling);
        var height = Quantize(arrangedSize.Height * scaling);
        return width == 0 && height == 0 ? null : (width, height);
    }

    bool IImageLoadControllerHost.DecodeToCover => Stretch is Stretch.Fill or Stretch.UniformToFill;

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

    private void ConfigureIconSize()
    {
        if (SizeType != CustomizableSizeType.Custom || double.IsNaN(Size))
        {
            ReleaseTemplateValue(ref _customWidthValue);
            ReleaseTemplateValue(ref _customHeightValue);
            ReleaseTemplateValue(ref _customIconSizeValue);
            return;
        }
        SetTemplateValue(WidthProperty, Size, ref _customWidthValue);
        SetTemplateValue(HeightProperty, Size, ref _customHeightValue);
        SetTemplateValue(
            EffectiveIconSizeProperty,
            Icon is not null || Source is not null ? Size / 2 : 18,
            ref _customIconSizeValue);
    }

    private void ConfigureContentType()
    {
        ContentType = LoadedImage is not null
            ? AvatarContentType.Image
            : Text is not null
                ? AvatarContentType.Text
                : AvatarContentType.Icon;
    }

    private void ConfigureTextPadding() =>
        SetTemplateValue(TextPaddingProperty, new Thickness(Gap, 0), ref _textPaddingValue);

    private void ConfigureShape()
    {
        if (Shape == AvatarShape.Circle && double.IsFinite(Width))
        {
            SetTemplateValue(CornerRadiusProperty, new CornerRadius(Width / 2), ref _circleRadiusValue);
        }
        else
        {
            ReleaseTemplateValue(ref _circleRadiusValue);
        }
    }

    // The new entry goes in before the old one is released, so the effective value never falls back in between.
    private void SetTemplateValue<T>(StyledProperty<T> property, T value, ref IDisposable? handle)
    {
        var previous = handle;
        handle = SetValue(property, value, BindingPriority.Template);
        previous?.Dispose();
    }

    private static void ReleaseTemplateValue(ref IDisposable? handle)
    {
        var previous = handle;
        handle = null;
        previous?.Dispose();
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
