// Referenced from https://github.com/kikipoulet/SukiUI project

using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Native;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaWindow = Avalonia.Controls.Window;

public class Window : AvaloniaWindow, 
                      IOperationSystemAware,
                      IMediaBreakAwareControl,
                      IDisposable
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsTitleBarVisibleProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsTitleBarVisible), defaultValue: true);
    
    public static readonly StyledProperty<double> MaxWidthScreenRatioProperty =
        AvaloniaProperty.Register<Window, double>(nameof(MaxWidthScreenRatio), double.NaN);
    
    public static readonly StyledProperty<double> MaxHeightScreenRatioProperty =
        AvaloniaProperty.Register<Window, double>(nameof(MaxHeightScreenRatio), double.NaN);
    
    public static readonly StyledProperty<double> TitleFontSizeProperty =
        AvaloniaProperty.Register<Window, double>(nameof(TitleFontSize), defaultValue: 13);
    
    public static readonly StyledProperty<FontWeight> TitleFontWeightProperty =
        AvaloniaProperty.Register<Window, FontWeight>(nameof(TitleFontWeight), defaultValue: FontWeight.Bold);
    
    public static readonly StyledProperty<ContextMenu?> TitleBarContextMenuProperty =
        AvaloniaProperty.Register<Window, ContextMenu?>(nameof(TitleBarContextMenu));
    
    public static readonly StyledProperty<Control?> LogoProperty =
        WindowTitleBar.LogoProperty.AddOwner<Window>();
    
    public static readonly StyledProperty<object?> WindowFrameLayerProperty =
        AvaloniaProperty.Register<Window, object?>(nameof(WindowFrameLayer));
    
    public static readonly StyledProperty<IDataTemplate?> WindowFrameLayerTemplateProperty =
        AvaloniaProperty.Register<Window, IDataTemplate?>(nameof(WindowFrameLayerTemplate));
    
    public static readonly StyledProperty<double> WindowFrameLayerOpacityProperty =
        AvaloniaProperty.Register<Window, double>(nameof(WindowFrameLayerOpacity), 1.0);
    
    public static readonly StyledProperty<object?> ContentFrameLayerProperty =
        AvaloniaProperty.Register<Window, object?>(nameof(ContentFrameLayer));
    
    public static readonly StyledProperty<IDataTemplate?> ContentFrameLayerTemplateProperty =
        AvaloniaProperty.Register<Window, IDataTemplate?>(nameof(ContentFrameLayerTemplate));
    
    public static readonly StyledProperty<double> ContentFrameLayerOpacityProperty =
        AvaloniaProperty.Register<Window, double>(nameof(ContentFrameLayerOpacity), 1.0);
    
    public static readonly StyledProperty<IBrush?> ContentFrameBackgroundProperty =
        AvaloniaProperty.Register<Window, IBrush?>(nameof(ContentFrameBackground));
    
    public static readonly StyledProperty<object?> TitleBarFrameLayerProperty =
        AvaloniaProperty.Register<Window, object?>(nameof(TitleBarFrameLayer));
    
    public static readonly StyledProperty<IDataTemplate?> TitleBarFrameLayerTemplateProperty =
        AvaloniaProperty.Register<Window, IDataTemplate?>(nameof(TitleBarFrameLayerTemplate));
    
    public static readonly StyledProperty<double> TitleBarFrameLayerOpacityProperty =
        AvaloniaProperty.Register<Window, double>(nameof(TitleBarFrameLayerOpacity), 1.0);
    
    public static readonly StyledProperty<IBrush?> TitleBarFrameBackgroundProperty =
        AvaloniaProperty.Register<Window, IBrush?>(nameof(TitleBarFrameBackground));
    
    public static readonly StyledProperty<bool> IsFullScreenCaptionButtonEnabledProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsFullScreenCaptionButtonEnabled));

    public static readonly StyledProperty<bool> IsPinCaptionButtonEnabledProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsPinCaptionButtonEnabled));
    
    public static readonly StyledProperty<bool> IsCloseCaptionButtonEnabledProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsCloseCaptionButtonEnabled), true);
    
    public static readonly StyledProperty<bool> IsMoveEnabledProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsMoveEnabled), defaultValue: true);
    
    public static readonly StyledProperty<OsType> OsTypeProperty =
        OperationSystemAwareControlProperty.OsTypeProperty.AddOwner<Window>();
    
    public static readonly StyledProperty<Version> OsVersionProperty =
        OperationSystemAwareControlProperty.OsVersionProperty.AddOwner<Window>();
    
    public static readonly StyledProperty<MediaBreakPoint> MediaBreakPointProperty = 
        MediaBreakAwareControlProperty.MediaBreakPointProperty.AddOwner<Window>();
    
    public double TitleFontSize
    {
        get => GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }
    
    public FontWeight TitleFontWeight
    {
        get => GetValue(TitleFontWeightProperty);
        set => SetValue(TitleFontWeightProperty, value);
    }
    
    public bool IsTitleBarVisible
    {
        get => GetValue(IsTitleBarVisibleProperty);
        set => SetValue(IsTitleBarVisibleProperty, value);
    }
    
    public double MaxWidthScreenRatio
    {
        get => GetValue(MaxWidthScreenRatioProperty);
        set => SetValue(MaxWidthScreenRatioProperty, value);
    }
    
    public double MaxHeightScreenRatio
    {
        get => GetValue(MaxHeightScreenRatioProperty);
        set => SetValue(MaxHeightScreenRatioProperty, value);
    }
    
    public ContextMenu? TitleBarContextMenu
    {
        get => GetValue(TitleBarContextMenuProperty);
        set => SetValue(TitleBarContextMenuProperty, value);
    }
    
    [DependsOn(nameof(WindowFrameLayerTemplate))]
    public object? WindowFrameLayer
    {
        get => GetValue(WindowFrameLayerProperty);
        set => SetValue(WindowFrameLayerProperty, value);
    }
    
    public object? WindowFrameLayerTemplate
    {
        get => GetValue(WindowFrameLayerTemplateProperty);
        set => SetValue(WindowFrameLayerTemplateProperty, value);
    }
    
    public double WindowFrameLayerOpacity
    {
        get => GetValue(WindowFrameLayerOpacityProperty);
        set => SetValue(WindowFrameLayerOpacityProperty, value);
    }
    
    [DependsOn(nameof(ContentFrameLayerTemplate))]
    public object? ContentFrameLayer
    {
        get => GetValue(ContentFrameLayerProperty);
        set => SetValue(ContentFrameLayerProperty, value);
    }
    
    public object? ContentFrameLayerTemplate
    {
        get => GetValue(ContentFrameLayerTemplateProperty);
        set => SetValue(ContentFrameLayerTemplateProperty, value);
    }
    
    public double ContentFrameLayerOpacity
    {
        get => GetValue(ContentFrameLayerOpacityProperty);
        set => SetValue(ContentFrameLayerOpacityProperty, value);
    }
    
    public IBrush? ContentFrameBackground
    {
        get => GetValue(ContentFrameBackgroundProperty);
        set => SetValue(ContentFrameBackgroundProperty, value);
    }
    
    [DependsOn(nameof(TitleBarFrameLayerTemplate))]
    public object? TitleBarFrameLayer
    {
        get => GetValue(TitleBarFrameLayerProperty);
        set => SetValue(TitleBarFrameLayerProperty, value);
    }
    
    public object? TitleBarFrameLayerTemplate
    {
        get => GetValue(TitleBarFrameLayerTemplateProperty);
        set => SetValue(TitleBarFrameLayerTemplateProperty, value);
    }
    
    public double TitleBarFrameLayerOpacity
    {
        get => GetValue(TitleBarFrameLayerOpacityProperty);
        set => SetValue(TitleBarFrameLayerOpacityProperty, value);
    }
    
    public IBrush? TitleBarFrameBackground
    {
        get => GetValue(TitleBarFrameBackgroundProperty);
        set => SetValue(TitleBarFrameBackgroundProperty, value);
    }
    
    public object? Logo
    {
        get => GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }
    
    public bool IsFullScreenCaptionButtonEnabled
    {
        get => GetValue(IsFullScreenCaptionButtonEnabledProperty);
        set => SetValue(IsFullScreenCaptionButtonEnabledProperty, value);
    }
    
    public bool IsPinCaptionButtonEnabled
    {
        get => GetValue(IsPinCaptionButtonEnabledProperty);
        set => SetValue(IsPinCaptionButtonEnabledProperty, value);
    }
    
    public bool IsCloseCaptionButtonEnabled
    {
        get => GetValue(IsCloseCaptionButtonEnabledProperty);
        set => SetValue(IsCloseCaptionButtonEnabledProperty, value);
    }
    
    public bool IsMoveEnabled
    {
        get => GetValue(IsMoveEnabledProperty);
        set => SetValue(IsMoveEnabledProperty, value);
    }
    
    public MediaBreakPoint MediaBreakPoint
    {
        get => GetValue(MediaBreakPointProperty);
        internal set => SetValue(MediaBreakPointProperty, value);
    }
    
    public OsType OsType => GetValue(OsTypeProperty);
    public Version OsVersion => GetValue(OsVersionProperty);
    
    #endregion

    #region 公共事件定义
    
    public event EventHandler<MediaBreakPointChangedEventArgs>? MediaBreakPointChanged;

    #endregion

    #region macOS 特有属性

    [SupportedOSPlatform("macos")]
    public static readonly StyledProperty<Point> MacOSCaptionGroupOffsetProperty =
        AvaloniaProperty.Register<Window, Point>(nameof(MacOSCaptionGroupOffset), defaultValue: new Point(10, 0));
    
    [SupportedOSPlatform("macos")]
    public static readonly StyledProperty<double> MacOSCaptionGroupSpacingProperty =
        AvaloniaProperty.Register<Window, double>(nameof(MacOSCaptionGroupSpacing), 10.0);
    
    [SupportedOSPlatform("macos")]
    public Point MacOSCaptionGroupOffset
    {
        get => GetValue(MacOSCaptionGroupOffsetProperty);
        set => SetValue(MacOSCaptionGroupOffsetProperty, value);
    }

    [SupportedOSPlatform("macos")]
    public double MacOSCaptionGroupSpacing
    {
        get => GetValue(MacOSCaptionGroupSpacingProperty);
        set => SetValue(MacOSCaptionGroupSpacingProperty, value);
    }

    #endregion

    #region 内部事件定义
    
    internal event EventHandler? ScrollOccurred;

    #endregion

    #region 内部属性定义
    internal static readonly DirectProperty<Window, Thickness> TitleBarOSOffsetMarginProperty = 
        AvaloniaProperty.RegisterDirect<Window, Thickness>(nameof (TitleBarOSOffsetMargin), 
            o => o.TitleBarOSOffsetMargin,
            (o, v) => o.TitleBarOSOffsetMargin = v);

    internal static readonly DirectProperty<Window, WindowState> PreviousVisibleWindowStateProperty =
        AvaloniaProperty.RegisterDirect<Window, WindowState>(
            nameof(PreviousVisibleWindowState),
            o => o.PreviousVisibleWindowState);
    
    internal static readonly DirectProperty<Window, bool> IsCustomResizerVisibleProperty =
        AvaloniaProperty.RegisterDirect<Window, bool>(
            nameof(IsCustomResizerVisible),
            o => o.IsCustomResizerVisible,
            (o, v) => o.IsCustomResizerVisible = v);
    
    internal static readonly DirectProperty<Window, WindowTitleBar?> TitleBarProperty =
        AvaloniaProperty.RegisterDirect<Window, WindowTitleBar?>(
            nameof(TitleBar),
            o => o.TitleBar,
            (o, v) => o.TitleBar = v);
    
    private Thickness _titleBarOSOffsetMargin;
    public Thickness TitleBarOSOffsetMargin
    {
        get => _titleBarOSOffsetMargin;
        private set => SetAndRaise(TitleBarOSOffsetMarginProperty, ref _titleBarOSOffsetMargin, value);
    }
    
    private WindowState _previousVisibleWindowState = WindowState.Normal;

    internal WindowState PreviousVisibleWindowState
    {
        get => _previousVisibleWindowState;
        private set => SetAndRaise(PreviousVisibleWindowStateProperty, ref _previousVisibleWindowState, value);
    }

    private bool _isCustomResizerVisible;

    internal bool IsCustomResizerVisible
    {
        get => _isCustomResizerVisible;
        set => SetAndRaise(IsCustomResizerVisibleProperty, ref _isCustomResizerVisible, value);
    }
    
    private WindowTitleBar? _titleBar;

    internal WindowTitleBar? TitleBar
    {
        get => _titleBar;
        set => SetAndRaise(TitleBarProperty, ref _titleBar, value);
    }
    #endregion
    
    protected override Type StyleKeyOverride { get; } = typeof(Window);
    private WindowResizer? _windowResizer;
    private bool _isDisposed;
    private Point? _lastMousePressedPoint;
    private PointerPressedEventArgs? _lastMousePressedEventArgs;
    private bool _isDragging;
    private CompositeDisposable? _titleBarDisposable;

    protected bool CloseByClickCloseCaptionButton;

    static Window()
    {
        AffectsRender<Window>(TitleBarFrameBackgroundProperty, 
            ContentFrameBackgroundProperty);
        ScrollViewer.ScrollChangedEvent.AddClassHandler<Window>((window, args) => window.HandleScrollOccurred());
    }

    public Window()
    {
        ScalingChanged += HandleScalingChanged;
        this.ConfigureOsType();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MaxWidthScreenRatioProperty)
        {
            this.ConstrainMaxSizeToScreenRatio(MaxWidthScreenRatio, double.NaN);
        }
        else if (change.Property == MaxHeightScreenRatioProperty)
        {
            this.ConstrainMaxSizeToScreenRatio(double.NaN, MaxHeightScreenRatio);
        }
        else if (change.Property == CanResizeProperty)
        {
            ConfigureCustomResizerVisible();
        }
        if (change.Property == WindowStateProperty)
        {
            if (change.OldValue is not WindowState oldWindowState ||
                change.NewValue is not WindowState newWindowState)
            {
                return;
            }
            HandleWindowStateChanged(oldWindowState, newWindowState);
        }

        if (OperatingSystem.IsMacOS())
        {
            if (this.IsAttachedToVisualTree())
            {
                if (change.Property == IsCloseCaptionButtonEnabledProperty)
                {
                    this.SetMacOSWindowClosable(IsCloseCaptionButtonEnabled);
                }
            }
        }
    }
    
    private void HandleWindowStateChanged(WindowState oldState, WindowState newState)
    {
        if (oldState != WindowState.Minimized)
        {
            PreviousVisibleWindowState = oldState;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Margin = new Thickness(newState == WindowState.Maximized
                ? 7
                : 0);
        }

        this.ConstrainMaxSizeToScreenRatio(MaxWidthScreenRatio, MaxHeightScreenRatio);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _windowResizer = e.NameScope.Find<WindowResizer>(WindowThemeConstants.WindowResizerPart);
        if (_windowResizer != null)
        {
            _windowResizer.TargetWindow = this;
        }

        HandleCreateTitleBar();

        var mediaQueryIndicator = e.NameScope.Find<WindowMediaQueryIndicator>(WindowThemeConstants.MediaQueryIndicatorPart);
        if (mediaQueryIndicator != null)
        {
            mediaQueryIndicator.OwnerWindow = this;
        }

        if (OperatingSystem.IsMacOS())
        {
            this.SetMacOSWindowClosable(IsCloseCaptionButtonEnabled);
        }
        ConfigureCustomResizerVisible();
    }

    private void HandleCreateTitleBar()
    {
        if (_titleBar != null)
        {
            _titleBar.MaximizeWindowRequested -= HandleTitleDoubleClicked;
            _titleBar.PointerPressed          -= HandleTitleBarPointerPressed;
            _titleBar.PointerReleased         -= HandleTitleBarPointerReleased;
            _titleBar.PointerMoved            -= HandleTitleBarPointerMoved;
        }
        _titleBarDisposable?.Dispose();
        _titleBarDisposable = new CompositeDisposable(8);

        var titleBar = NotifyCreateTitleBar(_titleBar, _titleBarDisposable);
        
        if (titleBar != null)
        {
            titleBar.MaximizeWindowRequested += HandleTitleDoubleClicked;
            titleBar.PointerPressed          += HandleTitleBarPointerPressed;
            titleBar.PointerReleased         += HandleTitleBarPointerReleased;
            titleBar.PointerMoved            += HandleTitleBarPointerMoved;
            
            _titleBarDisposable.Add(BindUtils.RelayBind(this, TitleProperty, titleBar, WindowTitleBar.TitleProperty));
            _titleBarDisposable.Add(BindUtils.RelayBind(this, LogoProperty, titleBar, WindowTitleBar.LogoProperty));
            _titleBarDisposable.Add(BindUtils.RelayBind(this, TitleFontSizeProperty, titleBar, WindowTitleBar.FontSizeProperty));
            _titleBarDisposable.Add(BindUtils.RelayBind(this, TitleFontWeightProperty, titleBar, WindowTitleBar.FontWeightProperty));
            _titleBarDisposable.Add(BindUtils.RelayBind(this, TitleBarContextMenuProperty, titleBar, WindowTitleBar.ContextMenuProperty));
        }
        
        TitleBar = titleBar;
    }

    protected virtual WindowTitleBar? NotifyCreateTitleBar(WindowTitleBar? oldTitleBar, CompositeDisposable disposables)
    {
        return new WindowTitleBar
        {
            Name = WindowThemeConstants.TitleBarPart
        };
    }

    public static Window? GetMainWindow()
    {
        var lifetime = Application.Current?.ApplicationLifetime;
        if (lifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.MainWindow as Window;
        }
        return null;
    }
    
    protected override void OnClosed(EventArgs e)
    {
        Dispose();
        base.OnClosed(e);
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }
        
        if (desktop.MainWindow is Window window && window != this)
        {
            Icon ??= window.Icon;
        }

        if (OperatingSystem.IsMacOS())
        {
            this.SetMacOSWindowClosable(IsCloseCaptionButtonEnabled);
            ConfigureMacOSCaptionGroupOffset();
            // 不加这个最大化，最小化那个按钮位置有问题
            ClientSize = new Size(ClientSize.Width, ClientSize.Height + 0.00001);
        }
    }

    private void HandleScalingChanged(object? sender, EventArgs e)
    {
        this.ConstrainMaxSizeToScreenRatio(MaxWidthScreenRatio, MaxHeightScreenRatio);
    }

    private void HandleTitleDoubleClicked(object? sender, EventArgs e)
    {
        if (CanResize)
        {
            var windowState = WindowState;
            if (windowState == WindowState.FullScreen)
            {
                return;
            }
        
            if (windowState == WindowState.Normal && (OsType == OsType.macOS || CanMaximize))
            {
                WindowState =  WindowState.Maximized;
            }
            else if (windowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
        }
    }

    private void HandleTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsMoveEnabled || WindowState == WindowState.FullScreen)
        {
            return;
        }
        _lastMousePressedPoint     = e.GetPosition(this);
        _lastMousePressedEventArgs = e;
    }

    private void HandleTitleBarPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!IsMoveEnabled || WindowState == WindowState.FullScreen || !e.Properties.IsLeftButtonPressed)
        {
            return;
        }
        Point mousePosition       = e.GetPosition(this);
        if (_lastMousePressedPoint != null)
        {
            var   distanceFromInitial = (Vector)(mousePosition - _lastMousePressedPoint);
            if (distanceFromInitial.Length > Constants.DragThreshold)
            {
                _isDragging = true;
                if (_lastMousePressedEventArgs is not null)
                {
                    BeginMoveDrag(_lastMousePressedEventArgs);
                }
            }
        }
    }
    
    private void HandleTitleBarPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!IsMoveEnabled || e.InitialPressMouseButton != MouseButton.Left || !_isDragging)
        {
            return;
        }
        this.ConstrainMaxSizeToScreenRatio(MaxWidthScreenRatio, MaxHeightScreenRatio);
        _lastMousePressedPoint     = null;
        _lastMousePressedEventArgs = null;
        _isDragging                = false;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed     =  true;
        
        ScalingChanged -= HandleScalingChanged;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (OperatingSystem.IsMacOS())
        {
            ConfigureMacOSCaptionGroupOffset();
        }
    }

    #region macOS 特有方法

    [SupportedOSPlatform("macos")]
    private void ConfigureMacOSCaptionGroupOffset()
    {
        this.SetMacOSOptionButtonsPosition(MacOSCaptionGroupOffset.X, MacOSCaptionGroupOffset.Y, MacOSCaptionGroupSpacing);
        var cationsSize = this.GetMacOSOptionsSize(MacOSCaptionGroupSpacing);
        SetCurrentValue(TitleBarOSOffsetMarginProperty, new Thickness(cationsSize.Width + MacOSCaptionGroupOffset.X, 0, 0, 0));
    }

    #endregion
    
    void IOperationSystemAware.SetOsType(OsType osType)
    {
        SetValue(OsTypeProperty, osType);
    }
    
    void IOperationSystemAware.SetOsVersion(Version version)
    {
        SetValue(OsVersionProperty, version);
    }

    internal void NotifyCloseRequestByUser()
    {
        CloseByClickCloseCaptionButton = true;
    }

    private void ConfigureCustomResizerVisible()
    {
        if (OsType != OsType.Linux)
        {
            SetCurrentValue(IsCustomResizerVisibleProperty, false);
        }
        else
        {
            SetCurrentValue(IsCustomResizerVisibleProperty, CanResize);
        }
    }

    internal void NotifyMediaBreakPointChanged(MediaBreakPoint breakPoint)
    {
        SetCurrentValue(MediaBreakPointProperty, breakPoint);
        MediaBreakPointChanged?.Invoke(this, new MediaBreakPointChangedEventArgs(breakPoint));
    }

    private void HandleScrollOccurred()
    {
        ScrollOccurred?.Invoke(this, EventArgs.Empty);
    }
}