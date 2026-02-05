using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.Normal, StdPseudoClass.Minimized, StdPseudoClass.Maximized, StdPseudoClass.Fullscreen)]
internal class CaptionButtonGroup : TemplatedControl, IOperationSystemAware
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsFullScreenCaptionButtonEnabledProperty =
        Window.IsFullScreenCaptionButtonEnabledProperty.AddOwner<CaptionButtonGroup>();
    
    public static readonly StyledProperty<bool> IsMaximizeCaptionButtonEnabledProperty =
        AvaloniaProperty.Register<CaptionButtonGroup, bool>(nameof(IsMaximizeCaptionButtonEnabled), defaultValue: true);

    public static readonly StyledProperty<bool> IsMinimizeCaptionButtonEnabledProperty =
        AvaloniaProperty.Register<CaptionButtonGroup, bool>(nameof(IsMinimizeCaptionButtonEnabled), defaultValue: true);
    
    public static readonly StyledProperty<bool> IsPinCaptionButtonEnabledProperty =
        Window.IsPinCaptionButtonEnabledProperty.AddOwner<CaptionButtonGroup>();
    
    public static readonly StyledProperty<bool> IsCloseCaptionButtonEnabledProperty =
        Window.IsCloseCaptionButtonEnabledProperty.AddOwner<CaptionButtonGroup>();
    
    public static readonly StyledProperty<bool> IsWindowActiveProperty = 
        WindowTitleBar.IsWindowActiveProperty.AddOwner<CaptionButtonGroup>();
    
    public static readonly StyledProperty<OsType> OsTypeProperty =
        OperationSystemAwareControlProperty.OsTypeProperty.AddOwner<CaptionButtonGroup>();
    
    public static readonly StyledProperty<Version> OsVersionProperty =
        OperationSystemAwareControlProperty.OsVersionProperty.AddOwner<CaptionButtonGroup>();

    public bool IsFullScreenCaptionButtonEnabled
    {
        get => GetValue(IsFullScreenCaptionButtonEnabledProperty);
        set => SetValue(IsFullScreenCaptionButtonEnabledProperty, value);
    }

    public bool IsMaximizeCaptionButtonEnabled
    {
        get => GetValue(IsMaximizeCaptionButtonEnabledProperty);
        set => SetValue(IsMaximizeCaptionButtonEnabledProperty, value);
    }

    public bool IsMinimizeCaptionButtonEnabled
    {
        get => GetValue(IsMinimizeCaptionButtonEnabledProperty);
        set => SetValue(IsMinimizeCaptionButtonEnabledProperty, value);
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
    
    public bool IsWindowActive
    {
        get => GetValue(IsWindowActiveProperty);
        set => SetValue(IsWindowActiveProperty, value);
    }
    
    public OsType OsType => GetValue(OsTypeProperty);
    public Version OsVersion => GetValue(OsVersionProperty);

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CaptionButtonGroup>();
    
    internal static readonly DirectProperty<CaptionButtonGroup, bool> IsWindowMaximizedProperty =
        AvaloniaProperty.RegisterDirect<CaptionButtonGroup, bool>(
            nameof(IsWindowMaximized),
            o => o.IsWindowMaximized,
            (o, v) => o.IsWindowMaximized = v);
    
    internal static readonly DirectProperty<CaptionButtonGroup, bool> IsWindowFullScreenProperty =
        AvaloniaProperty.RegisterDirect<CaptionButtonGroup, bool>(
            nameof(IsWindowFullScreen),
            o => o.IsWindowFullScreen,
            (o, v) => o.IsWindowFullScreen = v);
    
    internal static readonly DirectProperty<CaptionButtonGroup, bool> IsWindowPinnedProperty =
        AvaloniaProperty.RegisterDirect<CaptionButtonGroup, bool>(
            nameof(IsWindowPinned),
            o => o.IsWindowPinned,
            (o, v) => o.IsWindowPinned = v);
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private bool _isWindowMaximized;

    internal bool IsWindowMaximized
    {
        get => _isWindowMaximized;
        set => SetAndRaise(IsWindowMaximizedProperty, ref _isWindowMaximized, value);
    }
    
    private bool _isWindowFullScreen;

    internal bool IsWindowFullScreen
    {
        get => _isWindowFullScreen;
        set => SetAndRaise(IsWindowFullScreenProperty, ref _isWindowFullScreen, value);
    }
    
    private bool _isWindowPinned;

    internal bool IsWindowPinned
    {
        get => _isWindowPinned;
        set => SetAndRaise(IsWindowPinnedProperty, ref _isWindowPinned, value);
    }
    
    protected Window? HostWindow { get; private set; }

    #endregion
    
    private WindowState? _originWindowState;
    private CaptionButton? _fullScreenButton;
    private CaptionButton? _pinButton;
    private CaptionButton? _minimizeButton;
    private CaptionButton? _maximizeButton;
    private CaptionButton? _closeButton;
    
    private CompositeDisposable? _disposables;
    private readonly List<Action> _disposeActions = new();

    static CaptionButtonGroup()
    {
        AffectsArrange<CaptionButtonGroup>(IsWindowMaximizedProperty,
            IsWindowFullScreenProperty,
            IsMaximizeCaptionButtonEnabledProperty,
            IsPinCaptionButtonEnabledProperty,
            IsMinimizeCaptionButtonEnabledProperty);
    }

    public CaptionButtonGroup()
    {
        this.ConfigureOsType();
    }

    public virtual void Attach(Window hostWindow)
    {
        if (_disposables != null)
        {
            return;
        }
           
        HostWindow                                      = hostWindow;
        
        _disposables = new CompositeDisposable(7);
        _disposables.Add(BindUtils.RelayBind(hostWindow, Window.IsFullScreenCaptionButtonEnabledProperty, this, IsFullScreenCaptionButtonEnabledProperty));
        _disposables.Add(BindUtils.RelayBind(hostWindow, Window.IsPinCaptionButtonEnabledProperty, this, IsPinCaptionButtonEnabledProperty));
        _disposables.Add(BindUtils.RelayBind(hostWindow, Window.CanMaximizeProperty, this, IsMaximizeCaptionButtonEnabledProperty));
        _disposables.Add(BindUtils.RelayBind(hostWindow, Window.CanMinimizeProperty, this, IsMinimizeCaptionButtonEnabledProperty));
        _disposables.Add(BindUtils.RelayBind(hostWindow, Window.IsCloseCaptionButtonEnabledProperty, this, IsCloseCaptionButtonEnabledProperty));
        _disposables.Add(HostWindow.GetObservable(Window.WindowStateProperty)
                                   .Subscribe(x =>
                                   {
                                       PseudoClasses.Set(StdPseudoClass.Minimized, x == WindowState.Minimized);
                                       PseudoClasses.Set(StdPseudoClass.Normal, x == WindowState.Normal);
                                       PseudoClasses.Set(StdPseudoClass.Maximized, x == WindowState.Maximized);
                                       PseudoClasses.Set(StdPseudoClass.Fullscreen, x == WindowState.FullScreen);
                                       IsWindowMaximized  = x == WindowState.Maximized;
                                       IsWindowFullScreen = x == WindowState.FullScreen;
                                   }));
        _disposables.Add(HostWindow.GetObservable(Window.TopmostProperty)
                                   .Subscribe(x =>
                                   {
                                       IsWindowPinned = HostWindow.Topmost;
                                   }));
    }

    public virtual void Detach()
    {
        if (_disposables == null)
        {
            return;
        }
        _disposables.Dispose();
        foreach (var disposeAction in _disposeActions)
        {
            disposeAction.Invoke();
        }
        _disposables = null;
        HostWindow   = null;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        foreach (var disposeAction in _disposeActions)
        {
            disposeAction.Invoke();
        }
        _disposeActions.Clear();
        
        _closeButton      = e.NameScope.Find<CaptionButton>(CaptionButtonGroupThemeConstants.CloseButtonPart);
        _maximizeButton   = e.NameScope.Find<CaptionButton>(CaptionButtonGroupThemeConstants.MaximizeButtonPart);
        _minimizeButton   = e.NameScope.Find<CaptionButton>(CaptionButtonGroupThemeConstants.MinimizeButtonPart);
        _pinButton        = e.NameScope.Find<CaptionButton>(CaptionButtonGroupThemeConstants.PinButtonPart);
        _fullScreenButton = e.NameScope.Find<CaptionButton>(CaptionButtonGroupThemeConstants.FullScreenButtonPart);

        if (_closeButton != null)
        {
            _closeButton.Click += HandleCloseButtonClicked;
            _disposeActions.Add(() => _closeButton.Click -= HandleCloseButtonClicked);
        }

        if (_maximizeButton != null)
        {
            _maximizeButton.Click += HandleMaximizeButtonClicked;
            _disposeActions.Add(() => _maximizeButton.Click -= HandleMaximizeButtonClicked);
            // TODO 目前有点问题暂时关闭
            // EnableWindowsSnapLayout(_maximizeButton);
        }

        if (_fullScreenButton != null)
        {
            _fullScreenButton.Click += HandleFullScreenButtonClicked;
            _disposeActions.Add(() => _fullScreenButton.Click -= HandleFullScreenButtonClicked);
        }

        if (_minimizeButton != null)
        {
            _minimizeButton.Click += HandleMinimizeButtonClicked;
            _disposeActions.Add(() => _minimizeButton.Click -= HandleMinimizeButtonClicked);
        }

        if (_pinButton != null)
        {
            _pinButton.Click += HandlePinButtonClicked;
            _disposeActions.Add(() => _pinButton.Click -= HandlePinButtonClicked);
        }
    }
    
    private void HandleFullScreenButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (HostWindow == null)
        {
            return;
        }

        if (!IsWindowFullScreen)
        {
            _originWindowState = HostWindow.WindowState;
        }
        HostWindow.WindowState = IsWindowFullScreen
            ? _originWindowState ?? WindowState.Normal
            : WindowState.FullScreen;
        IsWindowFullScreen = HostWindow.WindowState == WindowState.FullScreen;
        IsWindowMaximized  = HostWindow.WindowState == WindowState.Maximized;
    }

    private void HandleMaximizeButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (HostWindow == null)
        {
            return;
        }
        var windowState = HostWindow.WindowState;
        if (!HostWindow.CanMaximize || windowState == WindowState.FullScreen)
        {
            return;
        }
        HostWindow.WindowState = windowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
        IsWindowFullScreen = HostWindow.WindowState == WindowState.FullScreen;
        IsWindowMaximized  = HostWindow.WindowState == WindowState.Maximized;
    }
    
    private void HandleCloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (HostWindow != null)
        {
            HostWindow.NotifyCloseRequestByUser();
        }
        HostWindow?.Close();
    }
    
    private void HandleMinimizeButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (HostWindow == null)
        {
            return;
        }
        HostWindow.WindowState = WindowState.Minimized;
    }

    private void HandlePinButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (HostWindow == null || !HostWindow.IsPinCaptionButtonEnabled)
        {
            return;
        }
        HostWindow.Topmost = !HostWindow.Topmost;
        IsWindowPinned = HostWindow.Topmost;
    }
    
    // Referenced from https://github.com/kikipoulet/SukiUI project
    private void EnableWindowsSnapLayout(CaptionButton maximizeButton)
    {
        if (HostWindow == null)
        {
            return;
        }
        const int HTMAXBUTTON = 9;
        const uint WM_NCHITTEST = 0x0084;
        const uint WM_CAPTURECHANGED = 0x0215;

        var pointerOnButton = false;
        var pointerOverSetter = typeof(CaptionButton).GetProperty(nameof(IsPointerOver));
        if (pointerOverSetter is null)
        {
            throw new NullReferenceException($"Unable to find Button.{nameof(IsPointerOver)} property.");
        }

        nint ProcHookCallback(nint hWnd, uint msg, nint wParam, nint lParam, ref bool handled)
        {
            if (!maximizeButton.IsVisible) return 0;

            if (msg == WM_NCHITTEST)
            {
                var point = new PixelPoint((short)(ToInt32(lParam) & 0xffff), (short)(ToInt32(lParam) >> 16));

                var buttonSize = maximizeButton.DesiredSize;

                var buttonLeftTop = maximizeButton.PointToScreen(FlowDirection == FlowDirection.LeftToRight
                                                           ? new Point(buttonSize.Width, 0)
                                                           : new Point(0, 0));

                var x = (buttonLeftTop.X - point.X) / HostWindow.RenderScaling;
                var y = (point.Y - buttonLeftTop.Y) / HostWindow.RenderScaling;

                if (new Rect(default, buttonSize).Contains(new Point(x, y)))
                {
                    handled = true;

                    if (pointerOnButton == false)
                    {
                        pointerOnButton = true;
                        pointerOverSetter.SetValue(maximizeButton, true);
                    }
                    return HTMAXBUTTON;
                }
                if (pointerOnButton)
                {
                    pointerOnButton = false;
                    pointerOverSetter.SetValue(maximizeButton, false);
                }
            }
            else if (msg == WM_CAPTURECHANGED)
            {
                if (pointerOnButton && HostWindow.CanMaximize)
                {
                    HostWindow.WindowState = HostWindow.WindowState == WindowState.Maximized
                                  ? WindowState.Normal
                                  : WindowState.Maximized;

                    pointerOverSetter.SetValue(maximizeButton, false);
                }
            }

            return 0;
        }

        static int ToInt32(IntPtr ptr) => IntPtr.Size == 4 ? ptr.ToInt32() : (int)(ptr.ToInt64() & 0xffffffff);
        
        var wndProcHookCallback = new Win32Properties.CustomWndProcHookCallback(ProcHookCallback);
        Win32Properties.AddWndProcHookCallback(HostWindow, wndProcHookCallback);

        _disposeActions.Add(() => Win32Properties.RemoveWndProcHookCallback(HostWindow, wndProcHookCallback));
    }
    
    protected override Size MeasureOverride(Size availableSize)
    {
        if (OsType == OsType.Windows)
        {
            if (_closeButton != null)
            {
                _closeButton.Width = MinHeight;
                _closeButton.Height = MinHeight;
            }
            if (_maximizeButton != null)
            {
                _maximizeButton.Width  = MinHeight;
                _maximizeButton.Height = MinHeight;
            }
            if (_minimizeButton != null)
            {
                _minimizeButton.Width  = MinHeight;
                _minimizeButton.Height = MinHeight;
            }
            if (_pinButton != null)
            {
                _pinButton.Width  = MinHeight;
                _pinButton.Height = MinHeight;
            }
            if (_fullScreenButton != null)
            {
                _fullScreenButton.Width  = MinHeight;
                _fullScreenButton.Height = MinHeight;
            }
        }
        var size = base.MeasureOverride(availableSize);
        return size;
    }
    
    void IOperationSystemAware.SetOsType(OsType osType)
    {
        SetValue(OsTypeProperty, osType);
    }
    
    void IOperationSystemAware.SetOsVersion(Version version)
    {
        SetValue(OsVersionProperty, version);
    }
}