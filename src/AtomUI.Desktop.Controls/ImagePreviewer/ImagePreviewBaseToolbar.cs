using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal enum ImagePreviewToolbarSource
{
    TitleBar,
    Floating
}

internal class ImagePreviewToolbarRequestEventArgs : RoutedEventArgs
{
    public ImagePreviewToolbarSource ToolbarSource { get; }

    public ImagePreviewToolbarRequestEventArgs(ImagePreviewToolbarSource source)
    {
        ToolbarSource = source;
    }
}

internal class ImagePreviewBaseToolbar : TemplatedControl
{
    public static readonly DirectProperty<ImagePreviewBaseToolbar, int> CurrentIndexProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewBaseToolbar, int>(
            nameof(CurrentIndex),
            o => o.CurrentIndex,
            (o, v) => o.CurrentIndex = v);
    
    public static readonly DirectProperty<ImagePreviewBaseToolbar, int> CountProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewBaseToolbar, int>(
            nameof(Count),
            o => o.Count,
            (o, v) => o.Count = v);
    
    public static readonly DirectProperty<ImagePreviewBaseToolbar, bool> IsMultiImagesProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewBaseToolbar, bool>(
            nameof(IsMultiImages),
            o => o.IsMultiImages,
            (o, v) => o.IsMultiImages = v);
    
    internal static readonly DirectProperty<ImagePreviewBaseToolbar, bool> IsScaleDownEnabledProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewBaseToolbar, bool>(
            nameof(IsScaleDownEnabled),
            o => o.IsScaleDownEnabled,
            (o, v) => o.IsScaleDownEnabled = v);
    
    internal static readonly DirectProperty<ImagePreviewBaseToolbar, bool> IsScaleUpEnabledProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewBaseToolbar, bool>(
            nameof(IsScaleUpEnabled),
            o => o.IsScaleUpEnabled,
            (o, v) => o.IsScaleUpEnabled = v);
    
    internal static readonly DirectProperty<ImagePreviewBaseToolbar, bool> IsImageFitToWindowProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewBaseToolbar, bool>(
            nameof(IsImageFitToWindow),
            o => o.IsImageFitToWindow,
            (o, v) => o.IsImageFitToWindow = v);
    
    private int _currentIndex;

    public int CurrentIndex
    {
        get => _currentIndex;
        internal set => SetAndRaise(CurrentIndexProperty, ref _currentIndex, value);
    }
    
    private int _count;

    public int Count
    {
        get => _count;
        internal set => SetAndRaise(CountProperty, ref _count, value);
    }
    
    private bool _isMultiImages;

    public bool IsMultiImages
    {
        get => _isMultiImages;
        set => SetAndRaise(IsMultiImagesProperty, ref _isMultiImages, value);
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

    #region 公共事件定义

    public static RoutedEvent<ImagePreviewToolbarRequestEventArgs> HorizontalFlipRequestEvent =
        RoutedEvent.Register<ImagePreviewBaseToolbar, ImagePreviewToolbarRequestEventArgs>(nameof(HorizontalFlipRequest), RoutingStrategies.Bubble);
    
    public static RoutedEvent<ImagePreviewToolbarRequestEventArgs> VerticalFlipRequestEvent =
        RoutedEvent.Register<ImagePreviewBaseToolbar, ImagePreviewToolbarRequestEventArgs>(nameof(VerticalFlipRequest), RoutingStrategies.Bubble);
    
    public static RoutedEvent<ImagePreviewToolbarRequestEventArgs> ScaleUpRequestEvent =
        RoutedEvent.Register<ImagePreviewBaseToolbar, ImagePreviewToolbarRequestEventArgs>(nameof(ScaleUpRequest), RoutingStrategies.Bubble);
    
    public static RoutedEvent<ImagePreviewToolbarRequestEventArgs> ScaleDownRequestEvent =
        RoutedEvent.Register<ImagePreviewBaseToolbar, ImagePreviewToolbarRequestEventArgs>(nameof(ScaleDownRequest), RoutingStrategies.Bubble);
    
    public static RoutedEvent<ImagePreviewToolbarRequestEventArgs> PreviousRequestEvent =
        RoutedEvent.Register<ImagePreviewBaseToolbar, ImagePreviewToolbarRequestEventArgs>(nameof(PreviousRequest), RoutingStrategies.Bubble);
    
    public static RoutedEvent<ImagePreviewToolbarRequestEventArgs> NextRequestEvent =
        RoutedEvent.Register<ImagePreviewBaseToolbar, ImagePreviewToolbarRequestEventArgs>(nameof(NextRequest), RoutingStrategies.Bubble);
    
    public static RoutedEvent<ImagePreviewToolbarRequestEventArgs> RotateLeftRequestEvent =
        RoutedEvent.Register<ImagePreviewBaseToolbar, ImagePreviewToolbarRequestEventArgs>(nameof(RotateLeftRequest), RoutingStrategies.Bubble);
    
    public static RoutedEvent<ImagePreviewToolbarRequestEventArgs> RotateRightRequestEvent =
        RoutedEvent.Register<ImagePreviewBaseToolbar, ImagePreviewToolbarRequestEventArgs>(nameof(RotateRightRequest), RoutingStrategies.Bubble);
    
    public static RoutedEvent<ImageFitToWindowEventArgs> FitToWindowRequestEvent =
        RoutedEvent.Register<ImagePreviewBaseToolbar, ImageFitToWindowEventArgs>(nameof(FitToWindowRequest), RoutingStrategies.Bubble);
    
    public event EventHandler<ImagePreviewToolbarRequestEventArgs>? HorizontalFlipRequest
    {
        add => AddHandler(HorizontalFlipRequestEvent, value);
        remove => RemoveHandler(HorizontalFlipRequestEvent, value);
    }

    public event EventHandler<ImagePreviewToolbarRequestEventArgs>? VerticalFlipRequest
    {
        add => AddHandler(VerticalFlipRequestEvent, value);
        remove => RemoveHandler(VerticalFlipRequestEvent, value);
    }

    public event EventHandler<ImagePreviewToolbarRequestEventArgs>? ScaleUpRequest
    {
        add => AddHandler(ScaleUpRequestEvent, value);
        remove => RemoveHandler(ScaleUpRequestEvent, value);
    }

    public event EventHandler<ImagePreviewToolbarRequestEventArgs>? ScaleDownRequest
    {
        add => AddHandler(ScaleDownRequestEvent, value);
        remove => RemoveHandler(ScaleDownRequestEvent, value);
    }
    
    public event EventHandler<ImagePreviewToolbarRequestEventArgs>? PreviousRequest
    {
        add => AddHandler(PreviousRequestEvent, value);
        remove => RemoveHandler(PreviousRequestEvent, value);
    }

    public event EventHandler<ImagePreviewToolbarRequestEventArgs>? NextRequest
    {
        add => AddHandler(NextRequestEvent, value);
        remove => RemoveHandler(NextRequestEvent, value);
    }

    public event EventHandler<ImagePreviewToolbarRequestEventArgs>? RotateLeftRequest
    {
        add => AddHandler(RotateLeftRequestEvent, value);
        remove => RemoveHandler(RotateLeftRequestEvent, value);
    }

    public event EventHandler<ImagePreviewToolbarRequestEventArgs>? RotateRightRequest
    {
        add => AddHandler(RotateRightRequestEvent, value);
        remove => RemoveHandler(RotateRightRequestEvent, value);
    }
    
    public event EventHandler<ImageFitToWindowEventArgs>? FitToWindowRequest
    {
        add => AddHandler(FitToWindowRequestEvent, value);
        remove => RemoveHandler(FitToWindowRequestEvent, value);
    }
    #endregion

    private IconButton? _horizontalFlipButton;
    private IconButton? _verticalFlipButton;
    private IconButton? _scaleUpButton;
    private IconButton? _scaleDownButton;
    private IconButton? _previousButton;
    private IconButton? _nextButton;
    private IconButton? _rotateLeftButton;
    private IconButton? _rotateRightButton;
    private ToggleIconButton? _fitToWindowButton;

    protected virtual ImagePreviewToolbarSource ToolbarSource => ImagePreviewToolbarSource.Floating;
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CountProperty)
        {
            SetCurrentValue(IsMultiImagesProperty, Count > 1);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _horizontalFlipButton = e.NameScope.Find<IconButton>(ImagePreviewerThemeConstants.HorizontalFlipButtonPart);
        _verticalFlipButton   = e.NameScope.Find<IconButton>(ImagePreviewerThemeConstants.VerticalFlipButtonPart);
        _scaleUpButton        = e.NameScope.Find<IconButton>(ImagePreviewerThemeConstants.ScaleUpButtonPart);
        _scaleDownButton      = e.NameScope.Find<IconButton>(ImagePreviewerThemeConstants.ScaleDownButtonPart);
        _previousButton       = e.NameScope.Find<IconButton>(ImagePreviewerThemeConstants.PreviousButtonPart);
        _nextButton           = e.NameScope.Find<IconButton>(ImagePreviewerThemeConstants.NextButtonPart);
        _rotateLeftButton     = e.NameScope.Find<IconButton>(ImagePreviewerThemeConstants.RotateLeftButtonPart);
        _rotateRightButton    = e.NameScope.Find<IconButton>(ImagePreviewerThemeConstants.RotateRightButtonPart);
        _fitToWindowButton    = e.NameScope.Find<ToggleIconButton>(ImagePreviewerThemeConstants.FitToWindowButtonPart);

        if (_horizontalFlipButton != null)
        {
            _horizontalFlipButton.Click += HandleButtonClick;
        }
        if (_verticalFlipButton != null)
        {
            _verticalFlipButton.Click += HandleButtonClick;
        }
        if (_scaleUpButton != null)
        {
            _scaleUpButton.Click += HandleButtonClick;
        }
        if (_scaleDownButton != null)
        {
            _scaleDownButton.Click += HandleButtonClick;
        }
        if (_previousButton != null)
        {
            _previousButton.Click += HandleButtonClick;
        }
        if (_nextButton != null)
        {
            _nextButton.Click += HandleButtonClick;
        }
        if (_rotateLeftButton != null)
        {
            _rotateLeftButton.Click += HandleButtonClick;
        }
        if (_rotateRightButton != null)
        {
            _rotateRightButton.Click += HandleButtonClick;
        }

        if (_fitToWindowButton != null)
        {
            _fitToWindowButton.IsCheckedChanged += HandleButtonClick;
        }
    }

    private void HandleButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender == _horizontalFlipButton)
        {
            RaiseRequestEvent(HorizontalFlipRequestEvent);
        }
        else if (sender == _verticalFlipButton)
        {
            RaiseRequestEvent(VerticalFlipRequestEvent);
        }
        else if (sender == _scaleUpButton)
        {
            RaiseRequestEvent(ScaleUpRequestEvent);
        }
        else if (sender == _scaleDownButton)
        {
            RaiseRequestEvent(ScaleDownRequestEvent);
        }
        else if (sender == _previousButton)
        {
            RaiseRequestEvent(PreviousRequestEvent);
        }
        else if (sender == _nextButton)
        {
            RaiseRequestEvent(NextRequestEvent);
        }
        else if (sender == _rotateLeftButton)
        {
            RaiseRequestEvent(RotateLeftRequestEvent);
        }
        else if (sender == _rotateRightButton)
        {
            RaiseRequestEvent(RotateRightRequestEvent);
        }
        else if (sender == _fitToWindowButton)
        {
            RaiseEvent(new ImageFitToWindowEventArgs(_fitToWindowButton?.IsChecked == true)
            {
                RoutedEvent = FitToWindowRequestEvent
            });
        }
    }

    private void RaiseRequestEvent(RoutedEvent<ImagePreviewToolbarRequestEventArgs> routedEvent)
    {
        RaiseEvent(new ImagePreviewToolbarRequestEventArgs(ToolbarSource)
        {
            RoutedEvent = routedEvent
        });
    }
}
