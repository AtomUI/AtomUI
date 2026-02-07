using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DialogPositioning;
using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Utilities;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class ImagePreviewerDialog : Window,
                                      IDialogHost,
                                      IHostedVisualTreeRoot,
                                      IMotionAwareControl,
                                      IManagedDialogPositionerDialog
{
    #region 公共属性定义
    public static readonly StyledProperty<IList<PreviewImageSource>?> SourcesProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, IList<PreviewImageSource>?>(nameof(Sources));
    
    public static readonly StyledProperty<bool> IsImageMovableProperty =
        ImagePreviewer.IsImageMovableProperty.AddOwner<ImagePreviewerDialog>();
    
    public static readonly StyledProperty<double> ScaleStepProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, double>(nameof(ScaleStep), 0.5);
    
    public static readonly StyledProperty<double> MinScaleProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, double>(nameof(MinScale), 1.0);
    
    public static readonly StyledProperty<double> MaxScaleProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, double>(nameof(MaxScale), 50.0);
    
    public static readonly StyledProperty<bool> IsModalProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, bool>(nameof(IsModal));
    
    public static readonly StyledProperty<int> CurrentIndexProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, int>(nameof(CurrentIndex), 0);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ImagePreviewerDialog>();
    
    public static readonly DirectProperty<ImagePreviewerDialog, int> CountProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, int>(
            nameof(Count),
            o => o.Count);
    
    public static readonly StyledProperty<Transform?> TransformProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, Transform?>(nameof(Transform));
    
    public IList<PreviewImageSource>? Sources
    {
        get => GetValue(SourcesProperty);
        set => SetValue(SourcesProperty, value);
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
    
    public bool IsModal
    {
        get => GetValue(IsModalProperty);
        set => SetValue(IsModalProperty, value);
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
    
    public Transform? Transform
    {
        get => GetValue(TransformProperty);
        set => SetValue(TransformProperty, value);
    }
    
    private int _count;

    public int Count
    {
        get => _count;
        internal set => SetAndRaise(CountProperty, ref _count, value);
    }
    
    Visual? IHostedVisualTreeRoot.Host
    {
        get
        {
            // If the parent is attached to a visual tree, then return that. However the parent
            // will possibly be a standalone Popup (i.e. a Popup not attached to a visual tree,
            // created by e.g. a ContextMenu): if this is the case, return the ParentTopLevel
            // if set. This helps to allow the focus manager to restore the focus to the outer
            // scope when the popup is closed.
            var parentVisual = Parent as Visual;
            if (parentVisual?.IsAttachedToVisualTree() == true)
            {
                return parentVisual;
            }
            return ParentTopLevel ?? parentVisual;
        }
    }
    
    public TopLevel ParentTopLevel { get; }

    Visual IDialogHost.HostedVisualTreeRoot => this;
    
    #endregion
    
    #region 内部属性定义
    
    internal static readonly DirectProperty<ImagePreviewerDialog, PreviewImageSource?> CurrentImageProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerDialog, PreviewImageSource?>(
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
    
    private PreviewImageSource? _currentImage;

    internal PreviewImageSource? CurrentImage
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
    
    #endregion
    
    protected override Type StyleKeyOverride { get; } = typeof(ImagePreviewerDialog);
    
    private DialogPositionRequest? _dialogPositionRequest;
    private Size _dialogSize;
    private bool _needsUpdate;
    private readonly ManagedDialogPositioner _positioner;
    private AbstractImagePreviewer _imagePreviewer;
    private PixelPoint _latestDialogPosition;
    private bool _firstSizeCalculated;

    static ImagePreviewerDialog()
    {
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
            dialog.HandleFitToWindowRequest(args.IsFitToWindow);
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
    }
    
    public ImagePreviewerDialog(TopLevel parent, AbstractImagePreviewer imagePreviewer)
    {
        ParentTopLevel  = parent;
        _positioner     = new ManagedDialogPositioner(this);
        _imagePreviewer = imagePreviewer;
#if DEBUG
        this.AttachDevTools();
#endif
    }
    
    public void SetChild(Control? control) => Content = control;

    void IDialogHost.ConfigurePosition(DialogPositionRequest request)
    {
        _dialogPositionRequest = request;
        _needsUpdate           = true;
        UpdatePosition();
    }

    protected override void ArrangeCore(Rect finalRect)
    {
        if (_dialogSize != finalRect.Size)
        {
            _dialogSize  = finalRect.Size;
            _imagePreviewer.NotifyDialogHostMeasured(_dialogSize, ClientAreaScreenGeometry);
            _needsUpdate = true;
            UpdatePosition();
        }
        
        base.ArrangeCore(finalRect);
    }
    
    private void UpdatePosition()
    {
        if (_needsUpdate && _dialogPositionRequest is not null)
        {
            _needsUpdate = false;
            _positioner.Update(_dialogPositionRequest, _dialogSize);
        }
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!e.IsProgrammatic || CloseByClickCloseCaptionButton)
        {
            e.Cancel = true;
            Dispatcher.UIThread.Post(() =>
            {
                CloseByClickCloseCaptionButton = false;
                _imagePreviewer.NotifyDialogHostCloseRequest();
            });
        }
    }
    
    IReadOnlyList<ManagedDialogPositionerScreenInfo> IManagedDialogPositionerDialog.ScreenInfos
    {
        get
        {
            return Screens.All.Select(s => new ManagedDialogPositionerScreenInfo(s.Bounds.ToRect(DesktopScaling), s.WorkingArea.ToRect(DesktopScaling)))
                          .ToArray();
        }
    }

    void IManagedDialogPositionerDialog.Move(Point logicalPoint)
    {
        if (WindowState == WindowState.Normal)
        {
            var devicePoint = new PixelPoint((int)(logicalPoint.X * DesktopScaling), (int)(logicalPoint.Y * DesktopScaling));
            if (_latestDialogPosition != devicePoint)
            {
                _latestDialogPosition = devicePoint;
                Position              = devicePoint;
            }
        }
    }

    Rect IManagedDialogPositionerDialog.ParentClientAreaScreenGeometry
    {
        get
        {
            var parentTopLevel = GetTopLevel(_imagePreviewer);
            Debug.Assert(parentTopLevel != null);
            var point = parentTopLevel.PointToScreen(default);
            var size  = parentTopLevel.ClientSize;
            return new Rect(point.X, point.Y, size.Width, size.Height);
        }
    }
    
    private Rect ClientAreaScreenGeometry
    {
        get
        {
            ManagedDialogPositionerScreenInfo? targetScreen = null;
            if (this is IManagedDialogPositionerDialog positionerDialog)
            {
                targetScreen = positionerDialog.ScreenInfos.FirstOrDefault(s => s.Bounds.ContainsExclusive(positionerDialog.ParentClientAreaScreenGeometry.TopLeft))
                               ?? positionerDialog.ScreenInfos.FirstOrDefault(s => s.Bounds.Intersects(positionerDialog.ParentClientAreaScreenGeometry))
                               ?? positionerDialog.ScreenInfos.FirstOrDefault();

                if (targetScreen != null &&
                    (targetScreen.WorkingArea.Width == 0 && targetScreen.WorkingArea.Height == 0))
                {
                    return targetScreen.Bounds;
                }
            }
            
            return targetScreen?.WorkingArea ?? new Rect(0, 0, int.MaxValue, int.MaxValue);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var calculateWindowSize = CalculateWindowSize();
        base.MeasureOverride(calculateWindowSize);
        return calculateWindowSize;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CurrentIndexProperty)
        {
            HandleCurrentIndexChanged();
        }
        else if (change.Property == SourcesProperty)
        {
            if (Sources?.Count > 0)
            {
                SetCurrentValue(CurrentIndexProperty, 0);
            }
            SetCurrentValue(IsMultiImagesProperty, Sources?.Count > 1);
            Count = Sources?.Count ?? 0;
        }

        if (change.Property == ImageScaleXProperty ||
            change.Property == ImageScaleYProperty)
        {
            SetCurrentValue(IsScaleDownEnabledProperty, MathUtilities.GreaterThan(Math.Abs(ImageScaleX), MinScale) && MathUtilities.GreaterThan(Math.Abs(ImageScaleY), MinScale));
            SetCurrentValue(IsScaleUpEnabledProperty, MathUtilities.LessThan(Math.Abs(ImageScaleX), MaxScale) && MathUtilities.LessThan(Math.Abs(ImageScaleY), MaxScale));
        }
    }

    private void HandleCurrentIndexChanged()
    {
        if (Sources?.Count > 0 && CurrentIndex >= 0 && CurrentIndex < Sources.Count)
        {
            SetCurrentValue(CurrentImageProperty, Sources[CurrentIndex]);
            SetCurrentValue(IsFirstImageProperty, CurrentIndex == 0);
            SetCurrentValue(IsLastImageProperty, CurrentIndex == Sources.Count - 1);
        }
        else if (Sources == null || Sources?.Count == 0)
        {
            SetCurrentValue(IsLastImageProperty, false);
            SetCurrentValue(IsFirstImageProperty, false);
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        HandleCurrentIndexChanged();
    }

    public void Close(Action? callback = null)
    {
        base.Close();
        callback?.Invoke();
    }

    private Size CalculateWindowSize()
    {
        if (!_firstSizeCalculated)
        {
            const double ratio  = 0.70d;
            var height = ClientAreaScreenGeometry.Height * ratio;
            var width  = ClientAreaScreenGeometry.Width * ratio;
            ClientSize = new Size(width, height);
            _firstSizeCalculated = true;
            return new Size(width, height);
        }
        return ClientSize;
    }
    
    private void HandleHorizontalFlipRequest()
    {
        SetCurrentValue(ImageScaleXProperty, -1 * ImageScaleX);
    }
    
    private void HandleVerticalFlipRequest()
    {
        SetCurrentValue(ImageScaleYProperty, -1 * ImageScaleY);
    }
    
    private void HandleRotateLeftRequest()
    {
        SetCurrentValue(ImageRotateProperty, ImageRotate - MathUtilities.Deg2Rad(90));
    }
    
    private void HandleRotateRightRequest()
    {
        SetCurrentValue(ImageRotateProperty, ImageRotate + MathUtilities.Deg2Rad(90));
    }
    
    private void HandleScaleDownRequest()
    {
        var scaleX = ImageScaleX * (1 / (1 + ScaleStep));
        var scaleY = ImageScaleY * (1 / (1 + ScaleStep));
        if (MathUtilities.LessThan(Math.Abs(scaleX), MinScale))
        {
            if (scaleX > 0)
            {
                scaleX = MinScale;
            }
            else
            {
                scaleX = -MinScale;
            }
        }
        if (MathUtilities.LessThan(Math.Abs(scaleY), MinScale))
        {
            if (scaleY > 0)
            {
                scaleY = MinScale;
            }
            else
            {
                scaleY = -MinScale;
            }
        }
        SetCurrentValue(ImageScaleXProperty, scaleX);
        SetCurrentValue(ImageScaleYProperty, scaleY);
        SetCurrentValue(ImageScaleChangedProperty, true);
    }
    
    private void HandleScaleUpRequest()
    {
        var scaleX = ImageScaleX * (1 + ScaleStep);
        var scaleY = ImageScaleY * (1 + ScaleStep);

        if (MathUtilities.GreaterThan(Math.Abs(scaleX), MaxScale))
        {
            if (scaleX > 0)
            {
                scaleX = MaxScale;
            }
            else
            {
                scaleX = -MaxScale;
            }
        }
        if (MathUtilities.GreaterThan(Math.Abs(scaleY), MaxScale))
        {
            if (scaleY > 0)
            {
                scaleY = MaxScale;
            }
            else
            {
                scaleY = -MaxScale;
            }
        }
        SetCurrentValue(ImageScaleXProperty, scaleX);
        SetCurrentValue(ImageScaleYProperty, scaleY);
        SetCurrentValue(ImageScaleChangedProperty, true);
    }

    private void HandleFitToWindowRequest(bool isFitToWindow)
    {
        SetCurrentValue(ImageScaleXProperty, ImageScaleX > 0.0 ? 1.0 : -1.0);
        SetCurrentValue(ImageScaleYProperty, ImageScaleY > 0.0 ? 1.0 : -1.0);
        SetCurrentValue(ImageScaleChangedProperty, false);
        SetCurrentValue(IsImageFitToWindowProperty, isFitToWindow);
    }
    
    private void HandleNextImageRequest()
    {
        SetCurrentValue(IsImageFitToWindowProperty, true);
        SetCurrentValue(ImageScaleChangedProperty, false);
        SetCurrentValue(ImageScaleXProperty, 1.0);
        SetCurrentValue(ImageScaleYProperty, 1.0);
        SetCurrentValue(CurrentIndexProperty, Math.Min(CurrentIndex + 1, Count - 1));
    }
    
    private void HandlePreviousRequest()
    {
        SetCurrentValue(IsImageFitToWindowProperty, true);
        SetCurrentValue(ImageScaleChangedProperty, false);
        SetCurrentValue(ImageScaleXProperty, 1.0);
        SetCurrentValue(ImageScaleYProperty, 1.0);
        SetCurrentValue(CurrentIndexProperty, Math.Max(CurrentIndex - 1, 0));
    }
    
    protected override WindowTitleBar? NotifyCreateTitleBar(WindowTitleBar? oldTitleBar, CompositeDisposable disposables)
    {
        var imagePreviewToolbar = new ImagePreviewToolbar();
        disposables.Add(BindUtils.RelayBind(this, CurrentIndexProperty, imagePreviewToolbar, ImagePreviewToolbar.CurrentIndexProperty));
        disposables.Add(BindUtils.RelayBind(this, CountProperty, imagePreviewToolbar, ImagePreviewToolbar.CountProperty));
        disposables.Add(BindUtils.RelayBind(this, IsScaleDownEnabledProperty, imagePreviewToolbar, ImagePreviewToolbar.IsScaleDownEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, IsScaleUpEnabledProperty, imagePreviewToolbar, ImagePreviewToolbar.IsScaleUpEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, IsImageFitToWindowProperty, imagePreviewToolbar, ImagePreviewToolbar.IsImageFitToWindowProperty));
        disposables.Add(BindUtils.RelayBind(this, IsFirstImageProperty, imagePreviewToolbar, ImagePreviewToolbar.IsFirstImageProperty));
        disposables.Add(BindUtils.RelayBind(this, IsLastImageProperty, imagePreviewToolbar, ImagePreviewToolbar.IsLastImageProperty));
        return new WindowTitleBar
        {
            Name = WindowThemeConstants.TitleBarPart,
            LeftAddOn = imagePreviewToolbar
        };
    }
    
    protected override void NotifyConfigureTitleBar(WindowTitleBar titleBar, CompositeDisposable disposables)
    {
        disposables.Add(BindUtils.RelayBind(this, TitleFontSizeProperty, titleBar, WindowTitleBar.FontSizeProperty));
        disposables.Add(BindUtils.RelayBind(this, TitleFontWeightProperty, titleBar, WindowTitleBar.FontWeightProperty));
        disposables.Add(BindUtils.RelayBind(this, TitleBarContextMenuProperty, titleBar, WindowTitleBar.ContextMenuProperty));
    }
}