using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DialogPositioning;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class ImagePreviewerDialog : Window,
                                      IDialogHost,
                                      IHostedVisualTreeRoot,
                                      IStyleHost,
                                      IMotionAwareControl,
                                      IManagedDialogPositionerDialog
{
    #region 公共属性定义
    public static readonly StyledProperty<IList<IImage>?> SourcesProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, IList<IImage>?>(nameof(Sources));
    
    public static readonly StyledProperty<bool> IsImageMovableProperty =
        ImagePreviewer.IsImageMovableProperty.AddOwner<ImagePreviewerDialog>();
    
    public static readonly StyledProperty<double> ScaleStepProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, double>(nameof(ScaleStep), 0.5);
    
    public static readonly StyledProperty<double> MinScaleProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, double>(nameof(MinScale), 1.0);
    
    public static readonly StyledProperty<double> MaxScaleProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, double>(nameof(MaxScale), 50.0);

    public static readonly StyledProperty<Transform?> TransformProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, Transform?>(nameof(Transform));
    
    public static readonly StyledProperty<bool> IsModalProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, bool>(nameof(IsModal));
    
    public static readonly StyledProperty<int> CurrentIndexProperty =
        AvaloniaProperty.Register<ImagePreviewerDialog, int>(nameof(CurrentIndex), 0);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ImagePreviewerDialog>();
    
    public IList<IImage>? Sources
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
    
    public Transform? Transform
    {
        get => GetValue(TransformProperty);
        set => SetValue(TransformProperty, value);
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
    
    IStyleHost? IStyleHost.StylingParent => Parent;
    
    public TopLevel ParentTopLevel { get; }

    Visual IDialogHost.HostedVisualTreeRoot => this;
    
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
    
    Control IMotionAwareControl.PropertyBindTarget => this;
    
    #endregion
    
    protected override Type StyleKeyOverride { get; } = typeof(ImagePreviewerDialog);
    
    private DialogPositionRequest? _dialogPositionRequest;
    private Size _dialogSize;
    private bool _needsUpdate;
    private readonly ManagedDialogPositioner _positioner;
    private ImagePreviewer _imagePreviewer;
    private PixelPoint _latestDialogPosition;
    
    public ImagePreviewerDialog(TopLevel parent, ImagePreviewer imagePreviewer)
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
        var size = base.MeasureOverride(availableSize);
        return new Size(Math.Max(size.Width, MinWidth), Math.Max(size.Height, MinHeight));
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
            SetCurrentValue(IsMultiImagesProperty, Sources?.Count > 0);
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
}