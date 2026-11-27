using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// An image that is loaded and decoded asynchronously and can show a loading placeholder, an error
/// placeholder and a fallback image.
/// </summary>
/// <remarks>
/// A load starts when a <see cref="Source" /> is set and the control is attached to a visual tree. With
/// the automatic decode mode it starts once a size to decode to is known, and with <see cref="IsLazy" />
/// once the control is also visible inside the viewport. It starts again when the source or the fallback
/// source, the request options, the priority or the decode settings change, when the size to decode to
/// changes, and when the control is attached again. Detaching the control cancels the load and clears
/// the shown image. <see cref="LoadState" /> reports the load, and the pseudo-classes <c>:loading</c>,
/// <c>:loaded</c>, <c>:failed</c> and <c>:fallback</c> follow that state; <c>:fallback</c> when the shown
/// image came from <see cref="FallbackSource" />. <c>:has-image</c> is set while a decoded image is shown.
/// The default theme shows the image, <see cref="LoadingContent" /> or <see cref="ErrorContent" />, one
/// at a time.
/// </remarks>
[PseudoClasses(":loading", ":loaded", ":failed", ":fallback", ":has-image")]
public class AsyncImage : TemplatedControl, IImageLoadControl, IImageLoadControllerHost
{
    /// <summary>
    /// Identifies the <see cref="Source" /> property.
    /// </summary>
    public static readonly StyledProperty<ImageSource?> SourceProperty =
        AvaloniaProperty.Register<AsyncImage, ImageSource?>(nameof(Source));

    /// <summary>
    /// Identifies the <see cref="FallbackSource" /> property.
    /// </summary>
    public static readonly StyledProperty<ImageSource?> FallbackSourceProperty =
        AvaloniaProperty.Register<AsyncImage, ImageSource?>(nameof(FallbackSource));

    /// <summary>
    /// Identifies the <see cref="RequestOptions" /> property.
    /// </summary>
    public static readonly StyledProperty<ImageRequestOptions?> RequestOptionsProperty =
        AvaloniaProperty.Register<AsyncImage, ImageRequestOptions?>(nameof(RequestOptions));

    /// <summary>
    /// Identifies the <see cref="Stretch" /> property.
    /// </summary>
    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<AsyncImage, Stretch>(nameof(Stretch), Stretch.Uniform);

    /// <summary>
    /// Identifies the <see cref="StretchDirection" /> property.
    /// </summary>
    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<AsyncImage, StretchDirection>(nameof(StretchDirection), StretchDirection.Both);

    /// <summary>
    /// Identifies the <see cref="DecodeMode" /> property.
    /// </summary>
    public static readonly StyledProperty<ImageDecodeMode> DecodeModeProperty =
        AvaloniaProperty.Register<AsyncImage, ImageDecodeMode>(nameof(DecodeMode), ImageDecodeMode.Auto);

    /// <summary>
    /// Identifies the <see cref="DecodePixelWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<int> DecodePixelWidthProperty =
        AvaloniaProperty.Register<AsyncImage, int>(nameof(DecodePixelWidth), validate: value => value >= 0);

    /// <summary>
    /// Identifies the <see cref="DecodePixelHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<int> DecodePixelHeightProperty =
        AvaloniaProperty.Register<AsyncImage, int>(nameof(DecodePixelHeight), validate: value => value >= 0);

    /// <summary>
    /// Identifies the <see cref="Priority" /> property.
    /// </summary>
    public static readonly StyledProperty<ImageRequestPriority> PriorityProperty =
        AvaloniaProperty.Register<AsyncImage, ImageRequestPriority>(nameof(Priority), ImageRequestPriority.Normal);

    /// <summary>
    /// Identifies the <see cref="IsLazy" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLazyProperty =
        AvaloniaProperty.Register<AsyncImage, bool>(nameof(IsLazy));

    /// <summary>
    /// Identifies the <see cref="LoadingContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<AsyncImage, object?>(nameof(LoadingContent));

    /// <summary>
    /// Identifies the <see cref="LoadingContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LoadingContentTemplateProperty =
        AvaloniaProperty.Register<AsyncImage, IDataTemplate?>(nameof(LoadingContentTemplate));

    /// <summary>
    /// Identifies the <see cref="ErrorContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ErrorContentProperty =
        AvaloniaProperty.Register<AsyncImage, object?>(nameof(ErrorContent));

    /// <summary>
    /// Identifies the <see cref="ErrorContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ErrorContentTemplateProperty =
        AvaloniaProperty.Register<AsyncImage, IDataTemplate?>(nameof(ErrorContentTemplate));

    /// <summary>
    /// Identifies the <see cref="LoadState" /> property.
    /// </summary>
    public static readonly DirectProperty<AsyncImage, ImageLoadState> LoadStateProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, ImageLoadState>(nameof(LoadState), control => control.LoadState);

    /// <summary>
    /// Identifies the <see cref="LoadError" /> property.
    /// </summary>
    public static readonly DirectProperty<AsyncImage, ImageLoadError?> LoadErrorProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, ImageLoadError?>(nameof(LoadError), control => control.LoadError);

    /// <summary>
    /// Identifies the <see cref="LoadProgress" /> property.
    /// </summary>
    public static readonly DirectProperty<AsyncImage, ImageLoadProgress?> LoadProgressProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, ImageLoadProgress?>(nameof(LoadProgress), control => control.LoadProgress);

    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly DirectProperty<AsyncImage, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, bool>(nameof(IsLoading), control => control.IsLoading);

    /// <summary>
    /// Identifies the <see cref="IsLoaded" /> property.
    /// </summary>
    public static readonly DirectProperty<AsyncImage, bool> IsLoadedProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, bool>(nameof(IsLoaded), control => control.IsLoaded);

    /// <summary>
    /// Identifies the <see cref="IsFailed" /> property.
    /// </summary>
    public static readonly DirectProperty<AsyncImage, bool> IsFailedProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, bool>(nameof(IsFailed), control => control.IsFailed);

    // True while a Source waits for the viewport gate, so the theme can show the lazy placeholder.
    internal static readonly DirectProperty<AsyncImage, bool> IsViewportPendingProperty =
        AvaloniaProperty.RegisterDirect<AsyncImage, bool>(nameof(IsViewportPending), control => control.IsViewportPending);

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
    private bool _isViewportPending;
    private bool _isReleased;
    private CompositeDisposable? _viewportSubscription;
    private Rect? _lastEffectiveViewport;

    public AsyncImage()
    {
        _controller = new ImageLoadController(this);
    }

    /// <summary>
    /// Gets or sets the image to load and display, or <see langword="null" /> to show no image.
    /// </summary>
    /// <remarks>
    /// Assigning a different image drops the shown image while the new one loads.
    /// </remarks>
    public ImageSource? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the image that is shown instead when loading <see cref="Source" /> fails.
    /// </summary>
    /// <remarks>
    /// A shown fallback leaves <see cref="LoadState" /> at <see cref="ImageLoadState.Loaded" />, sets the
    /// <c>:fallback</c> pseudo-class and raises <see cref="ImageOpened" /> with
    /// <see cref="ImageOpenedEventArgs.IsFallback" /> set. The fallback is not tried when <see cref="Source" />
    /// is <see langword="null" /> or when the explicit decode size is invalid.
    /// </remarks>
    public ImageSource? FallbackSource
    {
        get => GetValue(FallbackSourceProperty);
        set => SetValue(FallbackSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the options for the load request, such as the cache policies, or <see langword="null" /> to
    /// load with the defaults of <see cref="ImageRequestOptions" />.
    /// </summary>
    /// <remarks>
    /// Assigning new options restarts the load.
    /// </remarks>
    public ImageRequestOptions? RequestOptions
    {
        get => GetValue(RequestOptionsProperty);
        set => SetValue(RequestOptionsProperty, value);
    }

    /// <summary>
    /// Gets or sets how the image is scaled into the area of the control.
    /// </summary>
    /// <remarks>
    /// The value is passed to the image element of the theme and decides the size the image is decoded to in
    /// the automatic decode mode: <see cref="Stretch.Fill" /> and <see cref="Stretch.UniformToFill" /> make
    /// the decoded image cover the area, the other values fit inside it. The default is
    /// <see cref="Stretch.Uniform" />.
    /// </remarks>
    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the image may be scaled up, scaled down or both when <see cref="Stretch" /> applies.
    /// </summary>
    /// <remarks>
    /// The value is passed to the image element of the theme and takes no part in choosing the decode size.
    /// The default is <see cref="StretchDirection.Both" />.
    /// </remarks>
    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }

    /// <summary>
    /// Gets or sets how the pixel size that the image is decoded to is chosen.
    /// </summary>
    /// <remarks>
    /// <see cref="ImageDecodeMode.Auto" /> decodes to the displayed size,
    /// <see cref="ImageDecodeMode.Original" /> to the original size of the image and
    /// <see cref="ImageDecodeMode.Explicit" /> to <see cref="DecodePixelWidth" /> and
    /// <see cref="DecodePixelHeight" />. In the explicit mode the load fails with
    /// <see cref="ImageLoadErrorCode.InvalidSource" /> when both sizes are zero, and no fallback image is
    /// shown. In the automatic mode the load waits until a size to decode to is known. Assigning another mode
    /// restarts the load. The default is <see cref="ImageDecodeMode.Auto" />.
    /// </remarks>
    public ImageDecodeMode DecodeMode
    {
        get => GetValue(DecodeModeProperty);
        set => SetValue(DecodeModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the width in pixels that the image is decoded to in <see cref="ImageDecodeMode.Explicit" />.
    /// </summary>
    /// <remarks>
    /// 0 leaves the width unbounded; the load fails when this and <see cref="DecodePixelHeight" /> are both 0.
    /// The image is never scaled up to the value, and the SVG codec ignores it. A negative value is rejected.
    /// The other decode modes ignore the value.
    /// </remarks>
    public int DecodePixelWidth
    {
        get => GetValue(DecodePixelWidthProperty);
        set => SetValue(DecodePixelWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height in pixels that the image is decoded to in <see cref="ImageDecodeMode.Explicit" />.
    /// </summary>
    /// <remarks>
    /// 0 leaves the height unbounded; the load fails when this and <see cref="DecodePixelWidth" /> are both 0.
    /// The image is never scaled up to the value, and the SVG codec ignores it. A negative value is rejected.
    /// The other decode modes ignore the value.
    /// </remarks>
    public int DecodePixelHeight
    {
        get => GetValue(DecodePixelHeightProperty);
        set => SetValue(DecodePixelHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the position of this image among the loads that wait to start.
    /// </summary>
    /// <remarks>
    /// The priority never interrupts a load that has already started. Assigning a value applies it to the
    /// current load. The default is <see cref="ImageRequestPriority.Normal" />.
    /// </remarks>
    public ImageRequestPriority Priority
    {
        get => GetValue(PriorityProperty);
        set => SetValue(PriorityProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the load of an image waits until the control is visible inside the viewport.
    /// </summary>
    /// <remarks>
    /// <see langword="true" /> saves loading and decoding for images that are hidden or scrolled out of view.
    /// While the image waits, <see cref="LoadState" /> is <see cref="ImageLoadState.Idle" /> and the theme
    /// shows <see cref="LoadingContent" />. A load that has already started is not delayed, a
    /// <see langword="null" /> <see cref="Source" /> shows no placeholder, and assigning
    /// <see langword="false" /> starts a waiting load at once. The default is <see langword="false" />.
    /// </remarks>
    public bool IsLazy
    {
        get => GetValue(IsLazyProperty);
        set => SetValue(IsLazyProperty, value);
    }

    /// <summary>
    /// Gets or sets the content shown while an image loads and none is shown yet.
    /// </summary>
    /// <remarks>
    /// The default theme shows it from the moment a load starts, or a lazy image waits for the viewport, until
    /// an image or an error is shown, so the placeholder can size the control in an auto layout. An image that
    /// is decoded again stays visible instead. <see langword="null" /> shows nothing, as the theme defines no
    /// default placeholder. Use <see cref="LoadingContentTemplate" /> to build the content.
    /// </remarks>
    public object? LoadingContent
    {
        get => GetValue(LoadingContentProperty);
        set => SetValue(LoadingContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that builds <see cref="LoadingContent" />, or <see langword="null" /> to
    /// present the content without a template.
    /// </summary>
    public IDataTemplate? LoadingContentTemplate
    {
        get => GetValue(LoadingContentTemplateProperty);
        set => SetValue(LoadingContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the content shown when the image load failed and no image is shown.
    /// </summary>
    /// <remarks>
    /// The default theme shows it in the failed state, which an invalid explicit decode size also causes, and
    /// not while the fallback image is shown. <see langword="null" /> shows nothing, as the theme defines no
    /// default placeholder. Use <see cref="ErrorContentTemplate" /> to build the content.
    /// </remarks>
    public object? ErrorContent
    {
        get => GetValue(ErrorContentProperty);
        set => SetValue(ErrorContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that builds <see cref="ErrorContent" />, or <see langword="null" /> to present
    /// the content without a template.
    /// </summary>
    public IDataTemplate? ErrorContentTemplate
    {
        get => GetValue(ErrorContentTemplateProperty);
        set => SetValue(ErrorContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets the state of the current image load. It is <see cref="ImageLoadState.Loaded" /> also while the
    /// fallback image is shown.
    /// </summary>
    public ImageLoadState LoadState => _loadState;

    internal bool IsViewportPending
    {
        get => _isViewportPending;
        private set => SetAndRaise(IsViewportPendingProperty, ref _isViewportPending, value);
    }

    /// <summary>
    /// Gets why the image load failed. It is <see langword="null" /> unless <see cref="LoadState" /> is
    /// <see cref="ImageLoadState.Failed" />. When the main image and the fallback both failed, it describes
    /// the fallback.
    /// </summary>
    public ImageLoadError? LoadError => _loadError;

    /// <summary>
    /// Gets how far the running image load has advanced. It is <see langword="null" /> unless
    /// <see cref="LoadState" /> is <see cref="ImageLoadState.Loading" />.
    /// </summary>
    public ImageLoadProgress? LoadProgress => _loadProgress;

    /// <summary>
    /// Gets a value indicating whether an image load is running. This includes a re-decode of the shown image,
    /// which stays visible meanwhile.
    /// </summary>
    public bool IsLoading => _isLoading;

    /// <summary>
    /// Gets a value indicating whether the last image load ended with a shown image, the fallback image
    /// included. It is <see langword="false" /> again while the shown image is decoded anew. This property
    /// hides <see cref="Avalonia.Controls.Control.IsLoaded" />, which tells whether the control itself is
    /// loaded in the visual tree.
    /// </summary>
    public new bool IsLoaded => _isLoaded;

    /// <summary>
    /// Gets a value indicating whether the image load ended with an error and no image is shown. It is
    /// <see langword="false" /> while the fallback image is shown.
    /// </summary>
    public bool IsFailed => _isFailed;

    internal IImage? LoadedImage => _loadedImage;

    /// <summary>
    /// Raised on the UI thread after a loaded image has become the shown image and <see cref="LoadState" /> has
    /// been set to loaded, so the handler sees the final state. This includes the fallback image;
    /// <see cref="ImageOpenedEventArgs.IsFallback" /> tells which image was shown.
    /// </summary>
    public event EventHandler<ImageOpenedEventArgs>? ImageOpened;

    /// <summary>
    /// Raised on the UI thread after a load ended with an error and no image is shown. <see cref="LoadState" />
    /// is <see cref="ImageLoadState.Failed" /> and <see cref="LoadError" /> is set when the handler runs. When
    /// the main image and the fallback both failed, the event reports the fallback; an invalid explicit decode
    /// size fails without trying the fallback.
    /// </summary>
    public event EventHandler<ImageFailedEventArgs>? ImageFailed;

    Visual IImageLoadControllerHost.Visual => this;

    bool IImageLoadControllerHost.IsImageLoadAttached => _isAttached && !_isViewportGated;

    /// <summary>
    /// Loads <see cref="Source" /> again, reading the source instead of taking the cached content, so changes
    /// at the origin become visible. The image that is shown stays until the new load ends. Nothing happens
    /// while the control is detached from the visual tree, while a lazy image waits for the viewport, or while
    /// <see cref="Source" /> is <see langword="null" />.
    /// </summary>
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
        SetViewportGated(true);
        _controller.Release();
        SubscribeViewport();
    }

    /// <summary>
    /// Restarts the image load when <see cref="Source" />, <see cref="FallbackSource" />,
    /// <see cref="RequestOptions" />, <see cref="DecodeMode" />, <see cref="DecodePixelWidth" />,
    /// <see cref="DecodePixelHeight" /> or <see cref="Priority" /> changes, applies a changed
    /// <see cref="IsLazy" /> and re-evaluates the decode size when <see cref="Stretch" /> changes.
    /// </summary>
    /// <remarks>
    /// Calls the base implementation first. A derived override must call this implementation for the load to
    /// follow these properties.
    /// </remarks>
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
            if (change.Property == SourceProperty)
            {
                UpdateViewportPending();
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

    /// <summary>
    /// Arranges the template and hands the arranged size to the loader as the size to decode to, which starts
    /// a waiting load or decodes the image again when that size changed.
    /// </summary>
    /// <remarks>
    /// Calls the base implementation first. A derived override must call this implementation with the size the
    /// control was arranged at, or the image is not decoded to the size it is displayed at.
    /// </remarks>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var arranged = base.ArrangeOverride(finalSize);
        _controller.RefreshSize(GetDecodePixelSize(arranged));
        return arranged;
    }

    /// <summary>
    /// Measures the template and records the available size, which bounds the height the image is decoded to
    /// in the automatic decode mode when <c>Height</c> and <c>MaxHeight</c> are unset.
    /// </summary>
    /// <remarks>
    /// A derived override must call this implementation, or that bound is unknown.
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        _lastMeasureAvailableSize = availableSize;
        _hasMeasureAvailableSize = true;
        return base.MeasureOverride(availableSize);
    }

    /// <summary>
    /// Calls the base implementation, applies <see cref="IsLazy" /> and attaches the loader, which starts the
    /// image load.
    /// </summary>
    /// <remarks>
    /// A derived override must call this implementation, or no image loads while the control is attached.
    /// </remarks>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        // The gate must be closed before Attach so that the first load waits for the viewport.
        SetViewportGated(IsLazy);
        if (_isViewportGated)
        {
            SubscribeViewport();
        }
        _controller.Attach();
    }

    /// <summary>
    /// Detaches the loader, which cancels the running load and clears the shown image, and resets the lazy
    /// waiting state, each before the base implementation runs.
    /// </summary>
    /// <remarks>
    /// A derived override must call this implementation, or a detached control keeps its request and image.
    /// </remarks>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        _hasMeasureAvailableSize = false;
        UnsubscribeViewport();
        SetViewportGated(false);
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
                SetViewportGated(true);
                SubscribeViewport();
            }
        }
        else if (_isViewportGated)
        {
            OpenViewportGate();
        }
    }

    // Only a pending gate listens: a loaded image keeps no viewport or ancestor visibility handlers.
    private void SubscribeViewport()
    {
        if (_viewportSubscription is not null || !_isAttached)
        {
            return;
        }
        var subscription = new CompositeDisposable();
        _viewportSubscription = subscription;
        EffectiveViewportChanged += OnEffectiveViewportChanged;
        subscription.Add(Disposable.Create(() => EffectiveViewportChanged -= OnEffectiveViewportChanged));
        // Showing the image or an ancestor again raises no viewport or size change when the layout stays the same,
        // so a viewport recorded while hidden is re-checked on every visibility change. Reports the current value
        // synchronously, before any viewport is known, which is a no-op.
        this.TrackEffectiveVisibility(HandleEffectiveVisibilityChanged, subscription);
    }

    private void UnsubscribeViewport()
    {
        var subscription = _viewportSubscription;
        if (subscription is null)
        {
            return;
        }
        _viewportSubscription = null;
        _lastEffectiveViewport = null;
        subscription.Dispose();
    }

    private void OpenViewportGate()
    {
        UnsubscribeViewport();
        _isReleased = false;
        SetViewportGated(false);
        _controller.SourceConfigurationChanged();
    }

    private void SetViewportGated(bool isGated)
    {
        _isViewportGated = isGated;
        UpdateViewportPending();
    }

    // Without a Source there is nothing to wait for, so a lazy image shows no placeholder, like a non-lazy one.
    private void UpdateViewportPending() => IsViewportPending = _isViewportGated && Source is not null;

    private void HandleEffectiveVisibilityChanged(bool isVisible)
    {
        if (isVisible)
        {
            TryOpenViewportGate();
        }
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        _lastEffectiveViewport = e.EffectiveViewport;
        TryOpenViewportGate();
    }

    /// <summary>
    /// Calls the base implementation and then re-checks the lazy viewport gate. A control growing from zero at
    /// a fixed origin can keep the same effective viewport, so a size change may be the only sign that the
    /// control entered the viewport.
    /// </summary>
    /// <remarks>
    /// A derived override must call this implementation, or a lazy image may stay waiting.
    /// </remarks>
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
