using AtomUI.Controls;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(":loading", ":has-image")]
internal class ImageViewer : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsImageMovableProperty =
        ImagePreviewer.IsImageMovableProperty.AddOwner<ImageViewer>();

    public static readonly StyledProperty<int> CurrentIndexProperty =
        AvaloniaProperty.Register<ImageViewer, int>(nameof(CurrentIndex), 0);

    public static readonly DirectProperty<ImageViewer, int> CountProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, int>(
            nameof(Count),
            o => o.Count,
            (o, v) => o.Count = v);

    public static readonly StyledProperty<double> ImageTranslateXProperty =
        AvaloniaProperty.Register<ImageViewer, double>(nameof(ImageTranslateX));

    public static readonly StyledProperty<double> ImageTranslateYProperty =
        AvaloniaProperty.Register<ImageViewer, double>(nameof(ImageTranslateY));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ImageViewer>();

    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<ImageViewer, object?>(nameof(LoadingContent));

    public static readonly StyledProperty<IDataTemplate?> LoadingContentTemplateProperty =
        AvaloniaProperty.Register<ImageViewer, IDataTemplate?>(nameof(LoadingContentTemplate));

    public static readonly StyledProperty<object?> ErrorContentProperty =
        AvaloniaProperty.Register<ImageViewer, object?>(nameof(ErrorContent));

    public static readonly StyledProperty<IDataTemplate?> ErrorContentTemplateProperty =
        AvaloniaProperty.Register<ImageViewer, IDataTemplate?>(nameof(ErrorContentTemplate));

    public bool IsImageMovable
    {
        get => GetValue(IsImageMovableProperty);
        set => SetValue(IsImageMovableProperty, value);
    }

    public int CurrentIndex
    {
        get => GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    private int _count;

    public int Count
    {
        get => _count;
        internal set => SetAndRaise(CountProperty, ref _count, value);
    }

    public double ImageTranslateX
    {
        get => GetValue(ImageTranslateXProperty);
        set => SetValue(ImageTranslateXProperty, value);
    }

    public double ImageTranslateY
    {
        get => GetValue(ImageTranslateYProperty);
        set => SetValue(ImageTranslateYProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
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

    #endregion

    #region 公共事件定义

    public static RoutedEvent<RoutedEventArgs> PreviousRequestEvent =
        RoutedEvent.Register<ImageViewer, RoutedEventArgs>(nameof(PreviousRequest), RoutingStrategies.Bubble);

    public static RoutedEvent<RoutedEventArgs> NextRequestEvent =
        RoutedEvent.Register<ImageViewer, RoutedEventArgs>(nameof(NextRequest), RoutingStrategies.Bubble);

    public static RoutedEvent<RoutedEventArgs> ScaleUpRequestEvent =
        RoutedEvent.Register<ImageViewer, RoutedEventArgs>(nameof(ScaleUpRequest), RoutingStrategies.Bubble);

    public static RoutedEvent<RoutedEventArgs> ScaleDownRequestEvent =
        RoutedEvent.Register<ImageViewer, RoutedEventArgs>(nameof(ScaleDownRequest), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? PreviousRequest
    {
        add => AddHandler(PreviousRequestEvent, value);
        remove => RemoveHandler(PreviousRequestEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? NextRequest
    {
        add => AddHandler(NextRequestEvent, value);
        remove => RemoveHandler(NextRequestEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? ScaleUpRequest
    {
        add => AddHandler(ScaleUpRequestEvent, value);
        remove => RemoveHandler(ScaleUpRequestEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? ScaleDownRequest
    {
        add => AddHandler(ScaleDownRequestEvent, value);
        remove => RemoveHandler(ScaleDownRequestEvent, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> MinScaleProperty =
        ImagePreviewer.ImageMinScaleProperty.AddOwner<ImageViewer>();

    internal static readonly StyledProperty<double> MaxScaleProperty =
        ImagePreviewer.ImageMaxScaleProperty.AddOwner<ImageViewer>();

    internal static readonly StyledProperty<double> ScaleStepProperty =
        ImagePreviewer.ImageScaleStepProperty.AddOwner<ImageViewer>();

    internal static readonly StyledProperty<bool> IsWheelZoomEnabledProperty =
        ImagePreviewer.IsWheelZoomEnabledProperty.AddOwner<ImageViewer>();

    internal static readonly DirectProperty<ImageViewer, IImage?> CurrentImageProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, IImage?>(
            nameof(CurrentImage),
            o => o.CurrentImage,
            (o, v) => o.CurrentImage = v);

    internal static readonly DirectProperty<ImageViewer, bool> IsMultiImagesProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, bool>(
            nameof(IsMultiImages),
            o => o.IsMultiImages,
            (o, v) => o.IsMultiImages = v);

    internal static readonly DirectProperty<ImageViewer, bool> IsLastImageProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, bool>(
            nameof(IsLastImage),
            o => o.IsLastImage,
            (o, v) => o.IsLastImage = v);

    internal static readonly DirectProperty<ImageViewer, bool> IsFirstImageProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, bool>(
            nameof(IsFirstImage),
            o => o.IsFirstImage,
            (o, v) => o.IsFirstImage = v);

    internal static readonly DirectProperty<ImageViewer, bool> IsCurrentImageLoadingProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, bool>(
            nameof(IsCurrentImageLoading),
            o => o.IsCurrentImageLoading,
            (o, v) => o.IsCurrentImageLoading = v);

    internal static readonly DirectProperty<ImageViewer, bool> IsCurrentImageFailedProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, bool>(
            nameof(IsCurrentImageFailed),
            o => o.IsCurrentImageFailed,
            (o, v) => o.IsCurrentImageFailed = v);

    internal static readonly DirectProperty<ImageViewer, bool> IsScaleDownEnabledProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, bool>(
            nameof(IsScaleDownEnabled),
            o => o.IsScaleDownEnabled,
            (o, v) => o.IsScaleDownEnabled = v);

    internal static readonly DirectProperty<ImageViewer, bool> IsScaleUpEnabledProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, bool>(
            nameof(IsScaleUpEnabled),
            o => o.IsScaleUpEnabled,
            (o, v) => o.IsScaleUpEnabled = v);

    internal static readonly StyledProperty<ITransform?> ImageRenderTransformProperty =
        AvaloniaProperty.Register<ImageViewer, ITransform?>(nameof(ImageRenderTransform));

    internal static readonly DirectProperty<ImageViewer, double> ImageScaleXProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, double>(
            nameof(ImageScaleX),
            o => o.ImageScaleX,
            (o, v) => o.ImageScaleX = v);

    internal static readonly DirectProperty<ImageViewer, double> ImageScaleYProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, double>(
            nameof(ImageScaleY),
            o => o.ImageScaleY,
            (o, v) => o.ImageScaleY = v);

    internal static readonly DirectProperty<ImageViewer, bool> ImageScaleChangedProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, bool>(
            nameof(ImageScaleChanged),
            o => o.ImageScaleChanged,
            (o, v) => o.ImageScaleChanged = v);

    internal static readonly DirectProperty<ImageViewer, double> ImageRotateProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, double>(
            nameof(ImageRotate),
            o => o.ImageRotate,
            (o, v) => o.ImageRotate = v);

    internal static readonly DirectProperty<ImageViewer, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, bool>(nameof(IsDragging),
            o => o.IsDragging,
            (o, v) => o.IsDragging = v);

    internal static readonly DirectProperty<ImageViewer, bool> IsImageFitToWindowProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, bool>(
            nameof(IsImageFitToWindow),
            o => o.IsImageFitToWindow,
            (o, v) => o.IsImageFitToWindow = v);

    internal static readonly DirectProperty<ImageViewer, bool> SuppressTransformAnimationProperty =
        AvaloniaProperty.RegisterDirect<ImageViewer, bool>(
            nameof(SuppressTransformAnimation),
            o => o.SuppressTransformAnimation,
            (o, v) => o.SuppressTransformAnimation = v);

    internal double MinScale
    {
        get => GetValue(MinScaleProperty);
        set => SetValue(MinScaleProperty, value);
    }

    internal double MaxScale
    {
        get => GetValue(MaxScaleProperty);
        set => SetValue(MaxScaleProperty, value);
    }

    internal double ScaleStep
    {
        get => GetValue(ScaleStepProperty);
        set => SetValue(ScaleStepProperty, value);
    }

    internal bool IsWheelZoomEnabled
    {
        get => GetValue(IsWheelZoomEnabledProperty);
        set => SetValue(IsWheelZoomEnabledProperty, value);
    }

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

    internal ITransform? ImageRenderTransform
    {
        get => GetValue(ImageRenderTransformProperty);
        set => SetValue(ImageRenderTransformProperty, value);
    }

    private double _imageScaleX = 1.0;

    internal double ImageScaleX
    {
        get => _imageScaleX;
        set => SetAndRaise(ImageScaleXProperty, ref _imageScaleX, value);
    }

    private double _imageScaleY = 1.0;

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

    private bool _isDragging;

    internal bool IsDragging
    {
        get => _isDragging;
        set => SetAndRaise(IsDraggingProperty, ref _isDragging, value);
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

    internal Size DecodeImageSize => _baseImageSourceSize;

    internal PixelSize DecodeSize => ImagePreviewTransformMath.CalculateDecodeSize(
        _baseImageSize,
        Math.Max(Math.Abs(ImageScaleX), Math.Abs(ImageScaleY)),
        _decodeTopLevel?.RenderScaling ?? 1.0);

    #endregion

    #region 内部协作 API

    internal event EventHandler? PreviewChanged;

    internal void SetImageSize(Size size)
    {
        if (_imageSize != size)
        {
            _imageSize = size;
            InvalidateMeasure();
        }
    }

    internal void ApplyScaleUp(double? scaleStep = null)
    {
        ApplyZoom((scaleStep ?? ScaleStep) + 1.0);
    }

    internal void ApplyScaleDown(double? scaleStep = null)
    {
        var step = scaleStep ?? ScaleStep;
        ApplyZoom(1.0 / (MathUtils.LessThanOrClose(step, 0.0) ? 1.1 : 1.0 + step));
    }

    internal void ApplyRotate(double degrees)
    {
        SetCurrentValue(ImageRotateProperty, ImageRotate + degrees * Math.PI / 180.0);
        ClampTransform();
        PreviewChanged?.Invoke(this, EventArgs.Empty);
    }

    internal void ApplyFlip(bool horizontal)
    {
        if (horizontal)
        {
            SetCurrentValue(ImageScaleXProperty, -ImageScaleX);
        }
        else
        {
            SetCurrentValue(ImageScaleYProperty, -ImageScaleY);
        }
        ClampTransform();
        PreviewChanged?.Invoke(this, EventArgs.Empty);
    }

    internal void ApplyReset()
    {
        HandleDragCompleted();
        _pinchScale = 1.0;
        SetTransformState(ImagePreviewTransformState.Initial);
        SetCurrentValue(ImageScaleChangedProperty, false);
        SetCurrentValue(IsImageFitToWindowProperty, true);
        ClampTransform();
        PreviewChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    private ImagePreviewRenderer? _image;

    private Canvas? _scene;

    private TopLevel? _decodeTopLevel;

    private Size _imageSize;

    private Size _baseImageSize;

    private Size _baseImageSourceSize;

    private Point? _dragStart;

    private IPointer? _dragPointer;

    private double _pinchScale = 1.0;

    private double _originalTranslateX;

    private double _originalTranslateY;

    private ImagePreviewNavButton? _previousButton;

    private ImagePreviewNavButton? _nextButton;

    static ImageViewer()
    {
        FocusableProperty.OverrideDefaultValue<ImageViewer>(true);
        AffectsArrange<ImageViewer>(ImageTranslateXProperty, ImageTranslateYProperty, CurrentIndexProperty);
        PinchEvent.AddClassHandler<ImageViewer>((viewer, args) => viewer.HandlePinch(args));
        PinchEndedEvent.AddClassHandler<ImageViewer>((viewer, _) => viewer._pinchScale = 1.0);
    }

    public ImageViewer()
    {
        GestureRecognizers.Add(new PinchGestureRecognizer());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        HandleDragCompleted();
        _baseImageSize = default;
        if (_scene is not null)
        {
            _scene.SizeChanged -= HandleSceneSizeChanged;
        }
        if (_previousButton != null)
        {
            _previousButton.Click -= HandleButtonClick;
        }
        if (_nextButton != null)
        {
            _nextButton.Click -= HandleButtonClick;
        }

        _image          = e.NameScope.Find<ImagePreviewRenderer>("PART_ImageRenderer");
        _scene          = e.NameScope.Find<Canvas>("PART_ImageViewerScene");
        if (_scene is not null)
        {
            _scene.SizeChanged += HandleSceneSizeChanged;
        }
        _previousButton = e.NameScope.Find<ImagePreviewNavButton>("PART_PreviousButton");
        _nextButton     = e.NameScope.Find<ImagePreviewNavButton>("PART_NextButton");

        if (_previousButton != null)
        {
            _previousButton.Click += HandleButtonClick;
        }
        if (_nextButton != null)
        {
            _nextButton.Click += HandleButtonClick;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ImageScaleXProperty ||
            change.Property == ImageScaleYProperty ||
            change.Property == ImageRotateProperty)
        {
            GenerateImageRenderTransform();
        }

        if (change.Property == ImageScaleXProperty ||
            change.Property == ImageScaleYProperty)
        {
            UpdateScaleCapability();
            ClampTransform();
        }
        else if (change.Property == IsImageFitToWindowProperty)
        {
            HandleFitToWindowChanged();
        }
        else if (change.Property == CurrentImageProperty)
        {
            InvalidateMeasure();
            UpdateScaleCapability();
            ClampTransform();
        }

        if (change.Property == MinScaleProperty ||
            change.Property == MaxScaleProperty)
        {
            SetTransformState(ImagePreviewTransformMath.Zoom(GetTransformState(), 1.0, MinScale, MaxScale));
            ClampTransform();
            UpdateScaleCapability();
            PreviewChanged?.Invoke(this, EventArgs.Empty);
        }

        if (change.Property == CurrentIndexProperty)
        {
            HandleDragCompleted();
            ApplyReset();
        }

        if (change.Property == CurrentImageProperty ||
            change.Property == IsCurrentImageLoadingProperty)
        {
            // 与 ImagePreviewerCover/AsyncImage 相同的视觉门控契约：
            // 有图显示时不呈现加载指示器
            PseudoClasses.Set(":has-image", CurrentImage is not null);
            PseudoClasses.Set(":loading", IsCurrentImageLoading);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // The first measure precedes the scene's first arrange; subsequent sizes come from the scene.
        var viewport = _scene is { Bounds.Width: > 0, Bounds.Height: > 0 }
            ? _scene.Bounds.Size
            : availableSize;
        UpdateBaseImageSize(viewport);
        return base.MeasureOverride(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        UpdateImagePosition();
        return size;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _decodeTopLevel = TopLevel.GetTopLevel(this);
        if (_decodeTopLevel is not null)
        {
            _decodeTopLevel.ScalingChanged += HandleRenderScalingChanged;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_decodeTopLevel is not null)
        {
            _decodeTopLevel.ScalingChanged -= HandleRenderScalingChanged;
            _decodeTopLevel = null;
        }
        HandleDragCompleted();
        _pinchScale = 1.0;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (CanTransformCurrentImage && IsImageMovable && _dragStart.HasValue && e.Properties.IsLeftButtonPressed)
        {
            var delta             = e.GetPosition(_scene ?? (Visual)this) - _dragStart.Value;
            var manhattanDistance = Math.Abs(delta.X) + Math.Abs(delta.Y);
            if (manhattanDistance > Constants.DragThreshold)
            {
                if (!IsDragging)
                {
                    SetCurrentValue(IsDraggingProperty, true);
                    e.PreventGestureRecognition();
                }

                HandleDragging(new Vector(delta.X, delta.Y));
            }
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (_image == null || !CanTransformCurrentImage || !IsWheelZoomEnabled || Math.Abs(e.Delta.Y) < 0.000001)
        {
            return;
        }

        // Avalonia reports wheel detents; the shared formula uses browser-style 100-unit deltas.
        var ratio = ImagePreviewTransformMath.CalculateWheelRatio(e.Delta.Y * 100.0, ScaleStep);
        ApplyZoom(ratio, e.GetPosition(_scene ?? (Visual)this));
        RaiseEvent(new RoutedEventArgs(e.Delta.Y > 0 ? ScaleUpRequestEvent : ScaleDownRequestEvent));

        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_dragStart.HasValue)
        {
            HandleDragCompleted();
        }

        base.OnPointerCaptureLost(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (_image != null && CanTransformCurrentImage && IsImageMovable && e.Properties.IsLeftButtonPressed)
        {
            e.Handled           = true;
            _dragStart       = e.GetPosition(_scene ?? (Visual)this);
            _originalTranslateX = ImageTranslateX;
            _originalTranslateY = ImageTranslateY;
            _dragPointer = e.Pointer;
            if (e.Pointer.Type == PointerType.Mouse)
            {
                e.Pointer.Capture(this);
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_dragStart.HasValue)
        {
            e.Handled           = true;
            HandleDragCompleted();
            e.Pointer.Capture(null);
        }
    }

    protected override void OnDoubleTapped(TappedEventArgs e)
    {
        base.OnDoubleTapped(e);
        if (_image is null || !CanTransformCurrentImage ||
            !new Rect(_image.Bounds.Size).Contains(e.GetPosition(_image)))
        {
            return;
        }

        if (MathUtils.AreClose(Math.Abs(ImageScaleX), 1.0))
        {
            ApplyZoom(1.0 + ScaleStep, e.GetPosition(_scene ?? (Visual)this));
        }
        else
        {
            ApplyReset();
        }
        e.Handled = true;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        InvalidateMeasure();
        ClampTransform();
    }

    private bool CanTransformCurrentImage => CurrentImage != null;

    private void HandleButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender == _previousButton)
        {
            RaiseEvent(new RoutedEventArgs(PreviousRequestEvent));
        }
        else if (sender == _nextButton)
        {
            RaiseEvent(new RoutedEventArgs(NextRequestEvent));
        }

        Focus();
    }

    private void ApplyZoom(double ratio, Point? pointer = null)
    {
        if (!CanTransformCurrentImage)
        {
            return;
        }

        var state = GetTransformState();
        var next = pointer.HasValue
            ? ImagePreviewTransformMath.ZoomAt(
                state,
                _baseImageSize,
                ViewportSize,
                pointer.Value,
                ratio,
                MinScale,
                MaxScale)
            : ImagePreviewTransformMath.Zoom(state, ratio, MinScale, MaxScale);
        SetTransformState(next);
        SetCurrentValue(ImageScaleChangedProperty, true);
        PreviewChanged?.Invoke(this, EventArgs.Empty);
    }

    private ImagePreviewTransformState GetTransformState()
    {
        var scale = Math.Max(Math.Abs(ImageScaleX), Math.Abs(ImageScaleY));
        return new ImagePreviewTransformState(
            ImageTranslateX,
            ImageTranslateY,
            scale > 0 ? scale : 1.0,
            ImageRotate * 180.0 / Math.PI,
            ImageScaleX < 0,
            ImageScaleY < 0);
    }

    private void SetTransformState(ImagePreviewTransformState state)
    {
        state = ImagePreviewTransformMath.Clamp(state, _baseImageSize, ViewportSize);
        SetCurrentValue(ImageScaleXProperty, state.FlipX ? -state.Scale : state.Scale);
        SetCurrentValue(ImageScaleYProperty, state.FlipY ? -state.Scale : state.Scale);
        SetCurrentValue(ImageRotateProperty, state.Rotate * Math.PI / 180.0);
        SetCurrentValue(ImageTranslateXProperty, state.X);
        SetCurrentValue(ImageTranslateYProperty, state.Y);
    }

    private void ClampTransform()
    {
        if (!CanTransformCurrentImage)
        {
            return;
        }

        var clamped = ImagePreviewTransformMath.Clamp(GetTransformState(), _baseImageSize, ViewportSize);
        if (!MathUtils.AreClose(clamped.X, ImageTranslateX) ||
            !MathUtils.AreClose(clamped.Y, ImageTranslateY))
        {
            SetCurrentValue(ImageTranslateXProperty, clamped.X);
            SetCurrentValue(ImageTranslateYProperty, clamped.Y);
        }
    }

    private void UpdateScaleCapability()
    {
        var scale = Math.Max(Math.Abs(ImageScaleX), Math.Abs(ImageScaleY));
        var (min, max) = ImagePreviewTransformMath.NormalizeScaleRange(MinScale, MaxScale);
        SetCurrentValue(IsScaleDownEnabledProperty, CanTransformCurrentImage && scale > min + 0.000001);
        SetCurrentValue(IsScaleUpEnabledProperty, CanTransformCurrentImage && scale < max - 0.000001);
    }

    private Size ViewportSize => _scene is { Bounds.Width: > 0, Bounds.Height: > 0 }
        ? _scene.Bounds.Size
        : Bounds.Size;

    private void HandleSceneSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateBaseImageSize(e.NewSize);
        UpdateImagePosition();
    }

    private void UpdateBaseImageSize(Size viewport)
    {
        var sourceSize = _imageSize.Width > 0 && _imageSize.Height > 0
            ? _imageSize
            : CurrentImage?.Size ?? default;
        var size = CanTransformCurrentImage
            ? ImagePreviewTransformMath.CalculateBaseImageSize(sourceSize, viewport)
            : default;
        if (_image is null || (_baseImageSize == size && _baseImageSourceSize == sourceSize))
        {
            return;
        }

        _baseImageSize = size;
        _baseImageSourceSize = sourceSize;
        // These are computed runtime dimensions; an AXAML binding cannot calculate the base fit.
        _image.Width = size.Width;
        _image.Height = size.Height;
        ClampTransform();
        PreviewChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateImagePosition()
    {
        if (_image is not null)
        {
            ClampTransform();
            // The Canvas owns arrangement; only its offsets are supplied here.
            Canvas.SetLeft(_image, (ViewportSize.Width - _baseImageSize.Width) / 2 + ImageTranslateX);
            Canvas.SetTop(_image, (ViewportSize.Height - _baseImageSize.Height) / 2 + ImageTranslateY);
        }
    }

    private void HandleRenderScalingChanged(object? sender, EventArgs e)
    {
        PreviewChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GenerateImageRenderTransform()
    {
        // Avalonia's TransformOperations interpolator rewrites a no-op scale operation
        // to Identity during the animation. A retargeted transition then sees mismatched
        // operation types, falls back to matrix decomposition and normalizes the
        // rotation angle, which makes repeated rotate clicks spin backwards.
        var hasScale = !MathUtils.AreClose(ImageScaleX, 1.0) ||
                       !MathUtils.AreClose(ImageScaleY, 1.0);
        var builder = TransformOperations.CreateBuilder(hasScale ? 2 : 1);
        if (hasScale)
        {
            builder.AppendScale(ImageScaleX, ImageScaleY);
        }
        builder.AppendRotate(ImageRotate);
        ImageRenderTransform = builder.Build();
    }

    private void HandleDragging(Vector delta)
    {
        var offsetX = _originalTranslateX + delta.X;
        var offsetY = _originalTranslateY + delta.Y;

        ImageTranslateX = offsetX;
        ImageTranslateY = offsetY;
    }

    private void HandleDragCompleted()
    {
        ClampTransform();
        _originalTranslateX = 0.0;
        _originalTranslateY = 0.0;
        _dragStart       = null;
        IsDragging          = false;
        var pointer = _dragPointer;
        _dragPointer = null;
        if (pointer?.Captured == this)
        {
            pointer.Capture(null);
        }
    }

    private void HandlePinch(PinchEventArgs e)
    {
        if (!CanTransformCurrentImage || !double.IsFinite(e.Scale) || e.Scale <= 0)
        {
            return;
        }

        var origin = this.TranslatePoint(e.ScaleOrigin, _scene ?? (Visual)this);
        if (origin is { } point)
        {
            ApplyZoom(e.Scale / _pinchScale, point);
            _pinchScale = e.Scale;
            e.Handled = true;
        }
    }

    private void HandleFitToWindowChanged()
    {
        InvalidateMeasure();
        ClampTransform();
    }
}
