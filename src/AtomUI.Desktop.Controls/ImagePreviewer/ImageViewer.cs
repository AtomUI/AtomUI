using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Utilities;

namespace AtomUI.Desktop.Controls;

internal class ImageViewer : TemplatedControl, IMotionAwareControl
{
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
    
    #region 内部属性定义
    
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
    
    private bool _isDragging;

    internal bool IsDragging
    {
        get => _isDragging;
        set => SetAndRaise(IsDraggingProperty, ref _isDragging, value);
    }
        
    private bool _isImageFitToWindow;

    internal bool IsImageFitToWindow
    {
        get => _isImageFitToWindow;
        set => SetAndRaise(IsImageFitToWindowProperty, ref _isImageFitToWindow, value);
    }
    
    Control IMotionAwareControl.PropertyBindTarget => this;
    #endregion
    
    private Image? _image;
    private bool _isSelfChangedPosition;
    private Point? _lastestPoint;
    private double _originalTranslateX;
    private double _originalTranslateY;
    private Point _delta;
    
    static ImageViewer()
    {
        AffectsArrange<ImageViewer>(ImageTranslateXProperty, ImageTranslateYProperty);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _image = e.NameScope.Find<Image>(ImagePreviewerThemeConstants.ImageRendererPart);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
        
        if (change.Property == ImageScaleXProperty ||
            change.Property == ImageScaleYProperty ||
            change.Property == ImageRotateProperty)
        {
            GenerateImageRenderTransform();
        }

        if (change.Property == ImageScaleXProperty ||
            change.Property == ImageScaleYProperty)
        {
            HandleScaleChanged();
        }
        else if (change.Property == IsImageFitToWindowProperty)
        {
            HandleFitToWindowChanged();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_image != null)
        {
            if (IsImageFitToWindow && !ImageScaleChanged)
            {
                if (availableSize.Width < availableSize.Height)
                {
                    _image.Width = availableSize.Width;
                }
                else
                {
                    _image.Height = availableSize.Height;
                }
            }
        }
        return base.MeasureOverride(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (_image != null)
        {
            if (!_isSelfChangedPosition)
            {
                var offsetX = (finalSize.Width - _image.DesiredSize.Width) / 2;
                var offsetY = (finalSize.Height - _image.DesiredSize.Height) / 2;
                // 一直居中
                Canvas.SetLeft(_image, offsetX);
                Canvas.SetTop(_image, offsetY);
                ImageTranslateX = offsetX;
                ImageTranslateY = offsetY;
            }
            else
            {
                Canvas.SetLeft(_image, ImageTranslateX);
                Canvas.SetTop(_image, ImageTranslateY);
            }
        }
        return size;
    }

    private void GenerateImageRenderTransform()
    {
        var builder = TransformOperations.CreateBuilder(2);
        builder.AppendScale(ImageScaleX, ImageScaleY);
        builder.AppendRotate(ImageRotate);
        ImageRenderTransform = builder.Build();
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<TransformOperationsTransition>(ImageRenderTransformProperty),
                    TransitionUtils.CreateTransition<DoubleTransition>(ImageTranslateXProperty, SharedTokenKey.MotionDurationFast),
                    TransitionUtils.CreateTransition<DoubleTransition>(ImageTranslateYProperty, SharedTokenKey.MotionDurationFast)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }
    
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (IsImageMovable && _lastestPoint.HasValue && e.Properties.IsLeftButtonPressed)
        {
            var delta             = e.GetPosition(this) - _lastestPoint.Value;
            var manhattanDistance = Math.Abs(delta.X) + Math.Abs(delta.Y);
            if (manhattanDistance > Constants.DragThreshold)
            {
                if (!IsDragging)
                {
                    _isSelfChangedPosition = true;
                    SetCurrentValue(IsDraggingProperty, true);
                }

                HandleDragging(e.GetPosition(this), delta);
            }
        }
    }
    
    private void HandleDragging(Point position, Point delta)
    {
        _delta = delta;
        var offsetX = _originalTranslateX + delta.X;
        var offsetY = _originalTranslateY + delta.Y;
        
        ImageTranslateX = offsetX;
        ImageTranslateY = offsetY;
    }

    private void HandleDragCompleted()
    {
        ConstrainImagePosition();
        _originalTranslateX = 0.0;
        _originalTranslateY = 0.0;
        _lastestPoint       = null;
        IsDragging          = false;
    }

    private void ConstrainImagePosition()
    {
        if (_image != null)
        {
            var originalWidth     = _image.DesiredSize.Width;
            var originalHeight    = _image.DesiredSize.Height;
            var scaledImageWidth  = _image.DesiredSize.Width * Math.Abs(ImageScaleX);
            var scaledImageHeight = _image.DesiredSize.Height * Math.Abs(ImageScaleY);
            var leftBound         = (scaledImageWidth - originalWidth) / 2;
            var topBound          = (scaledImageHeight - originalHeight) / 2;
            var rightBound        = leftBound + Bounds.Width;
            var bottomBound       = topBound + Bounds.Height;
            
            if (MathUtilities.LessThanOrClose(scaledImageWidth, Bounds.Width) && MathUtilities.LessThanOrClose(scaledImageHeight, Bounds.Height))
            {
                _isSelfChangedPosition = false;
                InvalidateArrange();
            }
            else
            {
                if (MathUtilities.GreaterThan(scaledImageWidth, Bounds.Width))
                {
                    var left = Canvas.GetLeft(_image);
                    if (_delta.X < 0)
                    {
                        var right = left + scaledImageWidth;
                        if (right < rightBound)
                        {
                            ImageTranslateX = rightBound - scaledImageWidth;
                        }
                    }
                    else
                    {
                        if (left > leftBound)
                        {
                            ImageTranslateX = leftBound;
                        }
                    }
                }
                else
                {
                    var left = Canvas.GetLeft(_image);
                    if (_delta.X < 0)
                    {
                        if (left < leftBound)
                        {
                            ImageTranslateX = leftBound;
                        }
                        
                    }
                    else
                    {
                        var right = left + scaledImageWidth;
                        if (right > rightBound)
                        {
                            ImageTranslateX = rightBound - scaledImageWidth;
                        }
                    }
                }

                if (MathUtilities.GreaterThan(scaledImageHeight, Bounds.Height))
                {
                    var top = Canvas.GetTop(_image);
                    if (_delta.Y < 0)
                    {
                   
                        var bottom = top + scaledImageHeight;
                        if (bottom < bottomBound)
                        {
                            ImageTranslateY = bottomBound - scaledImageHeight;
                        }
                    }
                    else
                    {
                        if (top > topBound)
                        {
                            ImageTranslateY = topBound;
                        }
                    }
                }
                else
                {
                    var top = Canvas.GetTop(_image);
                    if (_delta.Y > 0)
                    {
                        if (top < topBound)
                        {
                            ImageTranslateY = topBound;
                        }
                        
                    }
                    else
                    {
                        var bottom = top + scaledImageHeight;
                        if (bottom > bottomBound)
                        {
                            ImageTranslateY = bottomBound - scaledImageHeight;
                        }
                    }
                }
            }
        }
    }
    
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_lastestPoint.HasValue)
        {
            HandleDragCompleted();
        }

        base.OnPointerCaptureLost(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (_image != null && IsImageMovable && e.Properties.IsLeftButtonPressed)
        {
            e.Handled           = true;
            _lastestPoint       = e.GetPosition(this);
            _originalTranslateX = Canvas.GetLeft(_image);
            _originalTranslateY = Canvas.GetTop(_image);
            e.PreventGestureRecognition();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_lastestPoint.HasValue)
        {
            e.Handled           = true;
            HandleDragCompleted();
        }
    }

    private void HandleScaleChanged()
    {
        if (_image != null)
        {
            var originalWidth     = _image.DesiredSize.Width;
            var originalHeight    = _image.DesiredSize.Height;
            var scaledImageWidth  = _image.DesiredSize.Width * Math.Abs(ImageScaleX);
            var scaledImageHeight = _image.DesiredSize.Height * Math.Abs(ImageScaleY);
            var leftBound         = (scaledImageWidth - originalWidth) / 2;
            var topBound          = (scaledImageHeight - originalHeight) / 2;
            var rightBound        = leftBound + Bounds.Width;
            var bottomBound       = topBound + Bounds.Height;
            
            if (MathUtilities.LessThanOrClose(scaledImageWidth, Bounds.Width) && MathUtilities.LessThanOrClose(scaledImageHeight, Bounds.Height))
            {
                _isSelfChangedPosition = false;
                InvalidateArrange();
            }
            else
            {
                if (MathUtilities.GreaterThan(scaledImageWidth, Bounds.Width))
                {
                    var left  = Canvas.GetLeft(_image);
                    var right = left + scaledImageWidth;
                    if (right < rightBound)
                    {
                        ImageTranslateX = rightBound - scaledImageWidth;
                    }
                    if (left > leftBound)
                    {
                        ImageTranslateX = leftBound;
                    }
                }
                else
                {
                    var left = Canvas.GetLeft(_image);
                    if (left < leftBound)
                    {
                        ImageTranslateX = leftBound;
                    }
                    var right = left + scaledImageWidth;
                    if (right > rightBound)
                    {
                        ImageTranslateX = rightBound - scaledImageWidth;
                    }
                }

                if (MathUtilities.GreaterThan(scaledImageHeight, Bounds.Height))
                {
                    var top    = Canvas.GetTop(_image);
                    var bottom = top + scaledImageHeight;
                    if (bottom < bottomBound)
                    {
                        ImageTranslateY = bottomBound - scaledImageHeight;
                    }
                    if (top > topBound)
                    {
                        ImageTranslateY = topBound;
                    }
                }
                else
                {
                    var top = Canvas.GetTop(_image);
                    if (top < topBound)
                    {
                        ImageTranslateY = topBound;
                    }
                    var bottom = top + scaledImageHeight;
                    if (bottom > bottomBound)
                    {
                        ImageTranslateY = bottomBound - scaledImageHeight;
                    }
                }
            }
        }
    }

    private void HandleFitToWindowChanged()
    {
        if (_image != null && _currentImage != null)
        {
            if (!IsImageFitToWindow)
            {
                _image.Width = _currentImage.Size.Width;
                _image.Height = _currentImage.Size.Height;
            }
            else
            {
                if (Bounds.Width < Bounds.Height)
                {
                    _image.Height = double.NaN;
                    _image.Width  = Bounds.Width;
               
                }
                else
                {
                    _image.Width  = double.NaN;
                    _image.Height = Bounds.Height;
                }
            }
            var offsetX = (Bounds.Width - _image.Width) / 2;
            var offsetY = (Bounds.Height - _image.Height) / 2;
            // 一直居中
            Canvas.SetLeft(_image, offsetX);
            Canvas.SetTop(_image, offsetY);
            ImageTranslateX = offsetX;
            ImageTranslateY = offsetY;
        }
    }
}