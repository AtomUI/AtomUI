using System.Runtime.Versioning;
using System.Reactive.Disposables;
using System.Windows.Input;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
using AtomUI.Native;
using AtomUI.Native.MacOS;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

using AvaloniaWindow = Avalonia.Controls.Window;

[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
public partial class Window : AvaloniaWindow,
                              IOperationSystemAware,
                              IMediaBreakAwareControl
{
    private const double WindowsCsdMinimumHeightInTitleBars = 2;

    #region 公共属性定义
    public static readonly StyledProperty<object?> LogoProperty =
        WindowTitleBar.LogoProperty.AddOwner<Window>();
    
    public static readonly StyledProperty<IDataTemplate?> LogoTemplateProperty =
        WindowTitleBar.LogoTemplateProperty.AddOwner<Window>();

    public static readonly StyledProperty<WindowTitleBarLogoVisibility> LogoVisibilityProperty =
        WindowTitleBar.LogoVisibilityProperty.AddOwner<Window>();

    public static readonly StyledProperty<WindowTitleBarTitleAlignment> TitleAlignmentProperty =
        WindowTitleBar.TitleAlignmentProperty.AddOwner<Window>();

    public static readonly StyledProperty<bool> IsTitleVisibleProperty =
        WindowTitleBar.IsTitleVisibleProperty.AddOwner<Window>();

    public static readonly StyledProperty<object?> LeftAddOnProperty =
        WindowTitleBar.LeftAddOnProperty.AddOwner<Window>();

    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        WindowTitleBar.LeftAddOnTemplateProperty.AddOwner<Window>();

    public static readonly StyledProperty<object?> RightAddOnProperty =
        WindowTitleBar.RightAddOnProperty.AddOwner<Window>();

    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        WindowTitleBar.RightAddOnTemplateProperty.AddOwner<Window>();
    
    public static readonly StyledProperty<bool> IsTitleBarVisibleProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsTitleBarVisible), defaultValue: true);
    
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
    
    public static readonly StyledProperty<bool> IsMinimizeCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsMinimizeCaptionButtonVisible), true);

    public static readonly StyledProperty<bool> IsMaximizeCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsMaximizeCaptionButtonVisible), true);

    public static readonly StyledProperty<bool> IsCloseCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsCloseCaptionButtonVisible), true);

    public static readonly StyledProperty<bool> IsFullScreenCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsFullScreenCaptionButtonVisible));

    public static readonly StyledProperty<bool> IsPinCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsPinCaptionButtonVisible));
    
    public static readonly StyledProperty<bool> IsMoveEnabledProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsMoveEnabled), defaultValue: true);
    
    public static readonly StyledProperty<MediaBreakPoint> MediaBreakPointProperty = 
        MediaBreakAwareControlProperty.MediaBreakPointProperty.AddOwner<Window>();
    
    public static readonly StyledProperty<OsType> OsTypeProperty =
        OperationSystemAwareControlProperty.OsTypeProperty.AddOwner<Window>();
    
    public static readonly StyledProperty<Version> OsVersionProperty =
        OperationSystemAwareControlProperty.OsVersionProperty.AddOwner<Window>();
    
    public bool IsTitleBarVisible
    {
        get => GetValue(IsTitleBarVisibleProperty);
        set => SetValue(IsTitleBarVisibleProperty, value);
    }
    
    [DependsOn(nameof(LogoTemplate))]
    public object? Logo
    {
        get => GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }
    
    public IDataTemplate? LogoTemplate
    {
        get => GetValue(LogoTemplateProperty);
        set => SetValue(LogoTemplateProperty, value);
    }

    public WindowTitleBarLogoVisibility LogoVisibility
    {
        get => GetValue(LogoVisibilityProperty);
        set => SetValue(LogoVisibilityProperty, value);
    }

    public WindowTitleBarTitleAlignment TitleAlignment
    {
        get => GetValue(TitleAlignmentProperty);
        set => SetValue(TitleAlignmentProperty, value);
    }

    public bool IsTitleVisible
    {
        get => GetValue(IsTitleVisibleProperty);
        set => SetValue(IsTitleVisibleProperty, value);
    }

    [DependsOn(nameof(LeftAddOnTemplate))]
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }

    public IDataTemplate? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }

    [DependsOn(nameof(RightAddOnTemplate))]
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }

    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
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
    
    public bool IsMinimizeCaptionButtonVisible
    {
        get => GetValue(IsMinimizeCaptionButtonVisibleProperty);
        set => SetValue(IsMinimizeCaptionButtonVisibleProperty, value);
    }

    public bool IsMaximizeCaptionButtonVisible
    {
        get => GetValue(IsMaximizeCaptionButtonVisibleProperty);
        set => SetValue(IsMaximizeCaptionButtonVisibleProperty, value);
    }

    public bool IsCloseCaptionButtonVisible
    {
        get => GetValue(IsCloseCaptionButtonVisibleProperty);
        set => SetValue(IsCloseCaptionButtonVisibleProperty, value);
    }

    public bool IsFullScreenCaptionButtonVisible
    {
        get => GetValue(IsFullScreenCaptionButtonVisibleProperty);
        set => SetValue(IsFullScreenCaptionButtonVisibleProperty, value);
    }

    public bool IsPinCaptionButtonVisible
    {
        get => GetValue(IsPinCaptionButtonVisibleProperty);
        set => SetValue(IsPinCaptionButtonVisibleProperty, value);
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
    
    #region 内部属性定义
    internal static readonly DirectProperty<Window, Thickness> NativeChromeInsetsProperty =
        AvaloniaProperty.RegisterDirect<Window, Thickness>(
            nameof(NativeChromeInsets),
            o => o.NativeChromeInsets,
            (o, v) => o.NativeChromeInsets = v);
    
    internal static readonly DirectProperty<Window, WindowTitleBar?> TitleBarProperty =
        AvaloniaProperty.RegisterDirect<Window, WindowTitleBar?>(
            nameof(TitleBar),
            o => o.TitleBar,
            (o, v) => o.TitleBar = v);
    
    internal static readonly DirectProperty<Window, bool> IsCsdEnabledProperty =
        AvaloniaProperty.RegisterDirect<Window, bool>(
            nameof(IsCsdEnabled),
            o => o.IsCsdEnabled,
            (o, v) => o.IsCsdEnabled = v);

    internal static readonly DirectProperty<Window, Thickness> EffectiveContentFrameMarginProperty =
        AvaloniaProperty.RegisterDirect<Window, Thickness>(
            nameof(EffectiveContentFrameMargin),
            o => o.EffectiveContentFrameMargin);

    internal static readonly DirectProperty<Window, bool> IsCustomResizerVisibleProperty =
        AvaloniaProperty.RegisterDirect<Window, bool>(
            nameof(IsCustomResizerVisible),
            o => o.IsCustomResizerVisible,
            (o, v) => o.IsCustomResizerVisible = v);

    internal static readonly DirectProperty<Window, bool> IsDrawnChromeOverlayVisibleProperty =
        AvaloniaProperty.RegisterDirect<Window, bool>(
            nameof(IsDrawnChromeOverlayVisible),
            o => o.IsDrawnChromeOverlayVisible);

    internal static readonly DirectProperty<Window, bool> IsEffectiveFullscreenLogoVisibleProperty =
        AvaloniaProperty.RegisterDirect<Window, bool>(
            nameof(IsEffectiveFullscreenLogoVisible),
            o => o.IsEffectiveFullscreenLogoVisible);

    internal static readonly DirectProperty<Window, object?> EffectiveLogoProperty =
        AvaloniaProperty.RegisterDirect<Window, object?>(
            nameof(EffectiveLogo),
            o => o.EffectiveLogo);

    internal static readonly DirectProperty<Window, IDataTemplate?> EffectiveLogoTemplateProperty =
        AvaloniaProperty.RegisterDirect<Window, IDataTemplate?>(
            nameof(EffectiveLogoTemplate),
            o => o.EffectiveLogoTemplate);

    internal static readonly DirectProperty<Window, object?> EffectiveFullscreenLogoProperty =
        AvaloniaProperty.RegisterDirect<Window, object?>(
            nameof(EffectiveFullscreenLogo),
            o => o.EffectiveFullscreenLogo);

    internal static readonly DirectProperty<Window, IDataTemplate?> EffectiveFullscreenLogoTemplateProperty =
        AvaloniaProperty.RegisterDirect<Window, IDataTemplate?>(
            nameof(EffectiveFullscreenLogoTemplate),
            o => o.EffectiveFullscreenLogoTemplate);

    internal static readonly DirectProperty<Window, bool> IsEffectiveFullscreenTitleVisibleProperty =
        AvaloniaProperty.RegisterDirect<Window, bool>(
            nameof(IsEffectiveFullscreenTitleVisible),
            o => o.IsEffectiveFullscreenTitleVisible);

    internal static readonly DirectProperty<Window, bool> IsPinCaptionButtonSupportedProperty =
        AvaloniaProperty.RegisterDirect<Window, bool>(
            nameof(IsPinCaptionButtonSupported),
            o => o.IsPinCaptionButtonSupported);

    internal static readonly DirectProperty<Window, ICommand> CaptionButtonCommandProperty =
        AvaloniaProperty.RegisterDirect<Window, ICommand>(
            nameof(CaptionButtonCommand),
            o => o.CaptionButtonCommand);
    
    internal static readonly StyledProperty<double> TitleBarHeightProperty =
        AvaloniaProperty.Register<Window, double>(nameof(TitleBarHeight));
    
    internal static readonly StyledProperty<BoxShadows> FrameShadowProperty =
        AvaloniaProperty.Register<Window, BoxShadows>(nameof(FrameShadow));
    
    internal static readonly StyledProperty<Thickness> FrameShadowThicknessProperty =
        AvaloniaProperty.Register<Window, Thickness>(nameof(FrameShadowThickness));

    internal static readonly DirectProperty<Window, Thickness> VisibleFrameBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<Window, Thickness>(
            nameof(VisibleFrameBorderThickness),
            o => o.VisibleFrameBorderThickness,
            (o, v) => o.VisibleFrameBorderThickness = v);

    private static readonly IDataTemplate s_windowIconLogoTemplate =
        new FuncDataTemplate<WindowIcon>((icon, _) => CreateWindowIconLogo(icon));
    
    private Thickness _nativeChromeInsets;

    internal Thickness NativeChromeInsets
    {
        get => _nativeChromeInsets;
        private set => SetAndRaise(NativeChromeInsetsProperty, ref _nativeChromeInsets, value);
    }
    
    private WindowTitleBar? _titleBar;

    internal WindowTitleBar? TitleBar
    {
        get => _titleBar;
        set => SetAndRaise(TitleBarProperty, ref _titleBar, value);
    }
    
    private bool _isCsdEnabled;

    internal bool IsCsdEnabled
    {
        get => _isCsdEnabled;
        set => SetAndRaise(IsCsdEnabledProperty, ref _isCsdEnabled, value);
    }

    private Thickness _effectiveContentFrameMargin;

    internal Thickness EffectiveContentFrameMargin
    {
        get => _effectiveContentFrameMargin;
        private set => SetAndRaise(
            EffectiveContentFrameMarginProperty,
            ref _effectiveContentFrameMargin,
            value);
    }

    internal void RaiseRoutedEventFromOverlay(Interactive source, RoutedEventArgs args)
    {
        if (args.RoutedEvent is null)
        {
            return;
        }

        using var route = BuildEventRoute(args.RoutedEvent);
        route.RaiseEvent(source, args);
    }

    private bool _isCustomResizerVisible;

    internal bool IsCustomResizerVisible
    {
        get => _isCustomResizerVisible;
        set => SetAndRaise(IsCustomResizerVisibleProperty, ref _isCustomResizerVisible, value);
    }

    private bool _isDrawnChromeOverlayVisible = true;

    internal bool IsDrawnChromeOverlayVisible
    {
        get => _isDrawnChromeOverlayVisible;
        private set => SetAndRaise(
            IsDrawnChromeOverlayVisibleProperty,
            ref _isDrawnChromeOverlayVisible,
            value);
    }

    private bool _isEffectiveFullscreenLogoVisible;

    internal bool IsEffectiveFullscreenLogoVisible
    {
        get => _isEffectiveFullscreenLogoVisible;
        private set => SetAndRaise(IsEffectiveFullscreenLogoVisibleProperty, ref _isEffectiveFullscreenLogoVisible, value);
    }

    private object? _effectiveLogo;

    internal object? EffectiveLogo
    {
        get => _effectiveLogo;
        private set => SetAndRaise(EffectiveLogoProperty, ref _effectiveLogo, value);
    }

    private IDataTemplate? _effectiveLogoTemplate;

    internal IDataTemplate? EffectiveLogoTemplate
    {
        get => _effectiveLogoTemplate;
        private set => SetAndRaise(EffectiveLogoTemplateProperty, ref _effectiveLogoTemplate, value);
    }

    private object? _effectiveFullscreenLogo;

    internal object? EffectiveFullscreenLogo
    {
        get => _effectiveFullscreenLogo;
        private set => SetAndRaise(
            EffectiveFullscreenLogoProperty,
            ref _effectiveFullscreenLogo,
            value);
    }

    private IDataTemplate? _effectiveFullscreenLogoTemplate;

    internal IDataTemplate? EffectiveFullscreenLogoTemplate
    {
        get => _effectiveFullscreenLogoTemplate;
        private set => SetAndRaise(
            EffectiveFullscreenLogoTemplateProperty,
            ref _effectiveFullscreenLogoTemplate,
            value);
    }

    private bool _isEffectiveFullscreenTitleVisible;

    internal bool IsEffectiveFullscreenTitleVisible
    {
        get => _isEffectiveFullscreenTitleVisible;
        private set => SetAndRaise(IsEffectiveFullscreenTitleVisibleProperty, ref _isEffectiveFullscreenTitleVisible, value);
    }

    private bool _isPinCaptionButtonSupported;

    internal bool IsPinCaptionButtonSupported
    {
        get => _isPinCaptionButtonSupported;
        private set => SetAndRaise(
            IsPinCaptionButtonSupportedProperty,
            ref _isPinCaptionButtonSupported,
            value);
    }

    internal ICommand CaptionButtonCommand => _captionButtonCommand;
    
    internal double TitleBarHeight
    {
        get => GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }
    
    internal BoxShadows FrameShadow
    {
        get => GetValue(FrameShadowProperty);
        set => SetValue(FrameShadowProperty, value);
    }
    
    internal Thickness FrameShadowThickness
    {
        get => GetValue(FrameShadowThicknessProperty);
        set => SetValue(FrameShadowThicknessProperty, value);
    }

    /// <summary>
    /// The visible native or drawn frame border around the client surface.
    /// This is separate from <see cref="FrameShadowThickness"/>, which only
    /// describes AtomUI's shadow/transparent expansion area.
    /// </summary>
    internal Thickness VisibleFrameBorderThickness
    {
        get => _visibleFrameBorderThickness;
        set => SetAndRaise(
            VisibleFrameBorderThicknessProperty,
            ref _visibleFrameBorderThickness,
            value);
    }
    #endregion

    private Thickness _visibleFrameBorderThickness;
    
    protected override Type StyleKeyOverride { get; } = typeof(Window);
    private protected bool CloseByClickCloseCaptionButton;
    private WindowTitleBar? _moveDragTitleBar;
    private Point? _lastMousePressedPoint;
    private PointerPressedEventArgs? _lastMousePressedEventArgs;
    private readonly IWindowChromeManager? _platformChromeManager;
    private readonly WindowCaptionButtonCommand _captionButtonCommand;
    private FullscreenPopoverLayer? _fullscreenPopoverLayer;
    private WindowResizer? _windowResizer;
    private Border? _transparencyFallbackBorder;
    private MediaBreakPointIndicator? _mediaBreakPointIndicator;
    private CompositeDisposable? _defaultTitleBarBindings;
    private Window? _mainWindowLogoFallbackSource;
    private CompositeDisposable? _mainWindowLogoFallbackLease;
    private bool _isOpened;
    private int _drawnChromeOverlaySuppressionCount;
    private ThemeContextLease? _themeContextLease;
    private WindowState _windowStateBeforeFullScreen = WindowState.Normal;

    // macOS 下 ConfigureMacOsWindow 的输入缓存，用于在 live resize 时短路，避免重复 P/Invoke
    private double? _macOsCachedTitleBarHeight;
    private double? _macOsCachedOffsetX;
    private double? _macOsCachedSpacing;
    private Size _macOsCachedClientSize;
    private double? _macOsCachedButtonX;
    private double? _macOsCachedButtonY;
    private bool _macOsCacheValid;
    private bool _macOsWindowConfigurationQueued;
    private bool _macOsWindowConfigurationFollowUpQueued;
    private IDisposable? _macOsWindowButtonObserver;
    private bool _macOsWindowConfigurationApplying;
    private int _macOsNativeButtonChangeQueued;
    
    static Window()
    {
        AffectsRender<Window>(TitleBarFrameBackgroundProperty, 
            ContentFrameBackgroundProperty);
        ConfigureOsType();
        FrameShadowProperty.Changed.AddClassHandler<Window>((window, args) =>
            window.HandleFrameShadowPropertyChanged(args.GetNewValue<BoxShadows>()));
    }

    public Window()
    {
        _captionButtonCommand = new WindowCaptionButtonCommand(this);
        ConfigureCsdStatus();
        _platformChromeManager = WindowChromeManager.Attach(this);
        UpdateCaptionButtonCapabilities();
    }

    internal IDisposable SuppressDrawnChromeOverlay()
    {
        _drawnChromeOverlaySuppressionCount++;
        IsDrawnChromeOverlayVisible = false;
        return Disposable.Create(this, static window => window.ReleaseDrawnChromeOverlaySuppression());
    }

    private void ReleaseDrawnChromeOverlaySuppression()
    {
        if (_drawnChromeOverlaySuppressionCount == 0)
        {
            return;
        }

        _drawnChromeOverlaySuppressionCount--;
        if (_drawnChromeOverlaySuppressionCount == 0)
        {
            IsDrawnChromeOverlayVisible = true;
        }
    }

    public override void Show()
    {
        var newThemeContextLease = PrepareThemeContextLease(this);
        Action? restoreInitialShowState = null;
        try
        {
            using var initialSurfaceThemeValues = CreateInitialSurfaceThemeValues();
            restoreInitialShowState = _platformChromeManager?.PrepareInitialShowState();
            ApplyCurrentWindowsCsdFrameTheme();
            base.Show();
        }
        catch
        {
            ReleaseFailedThemeContextLease(newThemeContextLease);
            throw;
        }
        finally
        {
            restoreInitialShowState?.Invoke();
        }
    }

    public new void Show(AvaloniaWindow owner)
    {
        var newThemeContextLease = PrepareThemeContextLease(owner);
        Action? restoreInitialShowState = null;
        try
        {
            using var initialSurfaceThemeValues = CreateInitialSurfaceThemeValues();
            restoreInitialShowState = _platformChromeManager?.PrepareInitialShowState();
            ApplyCurrentWindowsCsdFrameTheme();
            base.Show(owner);
        }
        catch
        {
            ReleaseFailedThemeContextLease(newThemeContextLease);
            throw;
        }
        finally
        {
            restoreInitialShowState?.Invoke();
        }
    }

    public new Task ShowDialog(AvaloniaWindow owner)
    {
        var newThemeContextLease = PrepareThemeContextLease(owner);
        Action? restoreInitialShowState = null;
        try
        {
            using var initialSurfaceThemeValues = CreateInitialSurfaceThemeValues();
            restoreInitialShowState = _platformChromeManager?.PrepareInitialShowState();
            ApplyCurrentWindowsCsdFrameTheme();
            return base.ShowDialog(owner);
        }
        catch
        {
            ReleaseFailedThemeContextLease(newThemeContextLease);
            throw;
        }
        finally
        {
            restoreInitialShowState?.Invoke();
        }
    }

    public new Task<TResult> ShowDialog<TResult>(AvaloniaWindow owner)
    {
        var newThemeContextLease = PrepareThemeContextLease(owner);
        Action? restoreInitialShowState = null;
        try
        {
            using var initialSurfaceThemeValues = CreateInitialSurfaceThemeValues();
            restoreInitialShowState = _platformChromeManager?.PrepareInitialShowState();
            ApplyCurrentWindowsCsdFrameTheme();
            return base.ShowDialog<TResult>(owner);
        }
        catch
        {
            ReleaseFailedThemeContextLease(newThemeContextLease);
            throw;
        }
        finally
        {
            restoreInitialShowState?.Invoke();
        }
    }

    private CompositeDisposable CreateInitialSurfaceThemeValues()
    {
        var values = new CompositeDisposable();
        if (!this.TryFindResource(WindowTokenKind.DefaultBackground, out var resource))
        {
            return values;
        }

        IBrush? background = resource switch
        {
            IBrush brush => brush,
            Color color  => new SolidColorBrush(color),
            _            => null
        };
        if (background is null)
        {
            return values;
        }

        // WindowOpenedEvent precedes ApplyStyling. Prime only the synchronous show interval;
        // local values still win, and WindowTheme owns the surface after these frames are removed.
        try
        {
            if (SetValue(BackgroundProperty, background, BindingPriority.Template) is { } backgroundValue)
            {
                values.Add(backgroundValue);
            }
            if (SetValue(
                    TransparencyBackgroundFallbackProperty,
                    background,
                    BindingPriority.Template) is { } fallbackValue)
            {
                values.Add(fallbackValue);
            }
            return values;
        }
        catch
        {
            values.Dispose();
            throw;
        }
    }

    private ThemeContextLease? PrepareThemeContextLease(StyledElement owner)
    {
        var context = ThemeScope.ResolveContext(owner);
        if (context is null || _themeContextLease?.IsOwnedBy(context) == true)
        {
            return null;
        }

        var lease = ThemeContextLease.Attach(this, context);
        _themeContextLease = lease;
        return lease;
    }

    private void ReleaseFailedThemeContextLease(ThemeContextLease? lease)
    {
        if (lease is null || !ReferenceEquals(_themeContextLease, lease))
        {
            return;
        }

        _themeContextLease = null;
        lease.Dispose();
        ResetThemeContextState();
    }

    private void ResetThemeContextState()
    {
        SetValue(RequestedThemeVariantProperty, null);
        SetValue(ThemeScope.ContextProperty, null);
    }

    internal void PreparePlatformChromeInitialShowLayout()
    {
        EnsureInitialized();
        ApplyStyling();
    }

    internal void PreparePlatformChromeInitialShowHandle()
    {
        EnsureInitialized();
    }

    internal void SetPlatformChromeClientSize(Size clientSize)
    {
        ClientSize = clientSize;
    }

    internal void ConfigureManagedResizeGrip(Thickness gripThickness)
    {
        if (_windowResizer is null)
        {
            return;
        }

        var shadow = FrameShadowThickness;
        _windowResizer.GripThickness = gripThickness;
        _windowResizer.Margin = new Thickness(
            Math.Max(0, shadow.Left - gripThickness.Left),
            Math.Max(0, shadow.Top - gripThickness.Top),
            Math.Max(0, shadow.Right - gripThickness.Right),
            Math.Max(0, shadow.Bottom - gripThickness.Bottom));
    }

    private void HandleFrameShadowPropertyChanged(BoxShadows frameShadow)
    {
        if (_platformChromeManager is not null)
        {
            _platformChromeManager.HandleFrameShadowChanged(frameShadow);
            return;
        }

        FrameShadowThickness = frameShadow.Thickness();
    }

    private static void ConfigureOsType()
    {
        if (OperatingSystem.IsWindows())
        {
            OsTypeProperty.OverrideDefaultValue<Window>(OsType.Windows);
        }
        else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
        {
            OsTypeProperty.OverrideDefaultValue<Window>(OsType.macOS);
        }
        else if (OperatingSystem.IsLinux())
        {
            OsTypeProperty.OverrideDefaultValue<Window>(OsType.Linux);
        }
        else
        {
            OsTypeProperty.OverrideDefaultValue<Window>(OsType.Unknown);
        }
        OsVersionProperty.OverrideDefaultValue<Window>(Environment.OSVersion.Version);
    }
    
    void IOperationSystemAware.SetOsType(OsType osType)
    {
        SetValue(OsTypeProperty, osType);
    }
    
    void IOperationSystemAware.SetOsVersion(Version version)
    {
        SetValue(OsVersionProperty, version);
    }
    
    private bool _mediaQueryReady;

    internal void NotifyMediaBreakPointChanged(MediaBreakPoint breakPoint)
    {
        if (MediaBreakPoint == breakPoint)
        {
            return;
        }
        SetCurrentValue(MediaBreakPointProperty, breakPoint);
        if (!_mediaQueryReady)
        {
            return;
        }
        MediaBreakPointChanged?.Invoke(this, new MediaBreakPointChangedEventArgs(breakPoint));
    }

    private void HandleFirstLayoutUpdatedForMediaQuery(object? sender, EventArgs e)
    {
        LayoutUpdated -= HandleFirstLayoutUpdatedForMediaQuery;
        _mediaQueryReady = true;
        MediaBreakPointChanged?.Invoke(this, new MediaBreakPointChangedEventArgs(MediaBreakPoint));
    }

    private void HandleIndicatorMediaBreakPointChanged(object? sender, MediaBreakPointChangedEventArgs args)
    {
        NotifyMediaBreakPointChanged(args.MediaBreakPoint);
    }
    
    internal void NotifyCloseRequestByUser()
    {
        CloseByClickCloseCaptionButton = true;
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _transparencyFallbackBorder = null;
        base.OnApplyTemplate(e);
        _transparencyFallbackBorder = e.NameScope.Find<Border>("PART_TransparencyFallback");

        HandleCreateTitleBar();
        EnsureWindowsCsdMinimumHeight();

        if (_mediaBreakPointIndicator != null)
        {
            _mediaBreakPointIndicator.MediaBreakPointChanged -= HandleIndicatorMediaBreakPointChanged;
        }

        _mediaBreakPointIndicator =
            e.NameScope.Find<MediaBreakPointIndicator>(MediaBreakPointIndicator.MediaQueryIndicatorName);
        if (_mediaBreakPointIndicator != null)
        {
            _mediaBreakPointIndicator.MediaBreakPointChanged += HandleIndicatorMediaBreakPointChanged;
            NotifyMediaBreakPointChanged(_mediaBreakPointIndicator.MediaBreakPoint);
        }

        _windowResizer = e.NameScope.Find<WindowResizer>("PART_WindowResizer");
        if (_windowResizer != null)
        {
            _windowResizer.TargetWindow = this;
        }

        ConfigureCustomResizerVisible();

        if (!IsCsdEnabled)
        {
            _fullscreenPopoverLayer?.Detach();
            _fullscreenPopoverLayer = e.NameScope.Find<FullscreenPopoverLayer>("PART_FullscreenPopoverLayer");
            _fullscreenPopoverLayer?.Attach(this);
        }
    }

    private void HandleCreateTitleBar()
    {
        if (_titleBar != null)
        {
            DetachTitleBar(_titleBar);
        }
        var titleBar = NotifyCreateTitleBar(_titleBar);
        
        if (titleBar != null)
        {
            titleBar.AttachHost(this);
            titleBar.SizeChanged += HandleTitleBarSizeChanged;
            NotifyConfigureTitleBar(titleBar);
        }
        
        TitleBar = titleBar;
    }

    private void DetachTitleBar(WindowTitleBar titleBar)
    {
        _defaultTitleBarBindings?.Dispose();
        _defaultTitleBarBindings = null;
        titleBar.SizeChanged -= HandleTitleBarSizeChanged;
        titleBar.DetachHost(this);
    }
    
    private void HandleTitleDoubleClicked(object? sender, EventArgs e)
    {
        if (sender is not WindowTitleBar titleBar ||
            !ReferenceEquals(titleBar.HostWindow, this) ||
            !CanResize)
        {
            return;
        }

        ExecuteCaptionButtonAction(CaptionButtonAction.ToggleMaximize);
    }

    private void HandleTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not WindowTitleBar titleBar ||
            !ReferenceEquals(titleBar.HostWindow, this) ||
            !IsMoveEnabled ||
            WindowState == WindowState.FullScreen ||
            !e.Properties.IsLeftButtonPressed)
        {
            ResetTitleBarMoveDragState();
            return;
        }
        _moveDragTitleBar          = titleBar;
        _lastMousePressedPoint     = e.GetPosition(this);
        _lastMousePressedEventArgs = e;
    }

    private void HandleTitleBarPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!ReferenceEquals(sender, _moveDragTitleBar))
        {
            return;
        }

        if (!IsMoveEnabled || WindowState == WindowState.FullScreen || !e.Properties.IsLeftButtonPressed)
        {
            ResetTitleBarMoveDragState();
            return;
        }

        Point mousePosition = e.GetPosition(this);
        if (_lastMousePressedPoint != null)
        {
            var   distanceFromInitial = (Vector)(mousePosition - _lastMousePressedPoint);
            if (distanceFromInitial.Length > Constants.DragThreshold)
            {
                if (_lastMousePressedEventArgs is not { } pointerPressedEventArgs)
                {
                    ResetTitleBarMoveDragState();
                    return;
                }

                ResetTitleBarMoveDragState();
                BeginMoveDrag(pointerPressedEventArgs);
            }
        }
    }

    private void HandleTitleBarSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        EnsureWindowsCsdMinimumHeight(e.NewSize.Height);
        if (_platformChromeManager is not null)
        {
            _platformChromeManager.ConfigureTitleBarHeightHint(e.NewSize.Height);
        }
        else
        {
            SetCurrentValue(ExtendClientAreaTitleBarHeightHintProperty, e.NewSize.Height);
        }
    }

    internal void QueueMacOsWindowConfiguration(bool forceFollowUp = false)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        _macOsCacheValid = false;
        if (_macOsWindowConfigurationQueued)
        {
            _macOsWindowConfigurationFollowUpQueued |= forceFollowUp;
            return;
        }

        _macOsWindowConfigurationQueued = true;
        Dispatcher.Post(ApplyQueuedMacOsWindowConfiguration, Avalonia.Threading.DispatcherPriority.Render);
    }

    [SupportedOSPlatform("macos")]
    private void EnsureMacOsWindowButtonObserver()
    {
        if (_macOsWindowButtonObserver is null)
        {
            _macOsWindowButtonObserver = this.ObserveStandardWindowButtonChanges(
                HandleMacOsNativeWindowButtonChange);
        }
    }

    private void StopMacOsWindowButtonObserver()
    {
        _macOsWindowButtonObserver?.Dispose();
        _macOsWindowButtonObserver = null;
    }

    private void HandleMacOsNativeWindowButtonChange()
    {
        if (Volatile.Read(ref _macOsWindowConfigurationApplying))
        {
            return;
        }

        if (Interlocked.Exchange(ref _macOsNativeButtonChangeQueued, 1) != 0)
        {
            return;
        }

        Dispatcher.Post(HandleMacOsNativeWindowButtonChangeOnUi, Avalonia.Threading.DispatcherPriority.Render);
    }

    private void HandleMacOsNativeWindowButtonChangeOnUi()
    {
        Interlocked.Exchange(ref _macOsNativeButtonChangeQueued, 0);
        if (!OperatingSystem.IsMacOS() ||
            _macOsWindowConfigurationApplying ||
            !IsVisible ||
            !ExtendClientAreaToDecorationsHint ||
            WindowState == WindowState.FullScreen ||
            !this.IsStandardWindowButtonsVisible())
        {
            return;
        }

        var closeFrame = this.GetStandardWindowButtonFrame(WindowUtilsInterop.NSWindowButton.CloseButton);
        if (closeFrame is not { } frame ||
            _macOsCachedButtonX is not { } expectedX ||
            _macOsCachedButtonY is not { } expectedY ||
            Math.Abs(frame.X - expectedX) > 0.5 ||
            Math.Abs(frame.Y - expectedY) > 0.5)
        {
            QueueMacOsWindowConfiguration();
        }
    }

    private void ApplyQueuedMacOsWindowConfiguration()
    {
        _macOsWindowConfigurationQueued = false;
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        _macOsWindowConfigurationApplying = true;
        try
        {
            ConfigureMacOsWindow();
        }
        finally
        {
            _macOsWindowConfigurationApplying = false;
        }

        if (IsVisible &&
            ExtendClientAreaToDecorationsHint &&
            WindowState != WindowState.FullScreen)
        {
            EnsureMacOsWindowButtonObserver();
        }
        else
        {
            StopMacOsWindowButtonObserver();
        }

        if (_macOsWindowConfigurationFollowUpQueued)
        {
            _macOsWindowConfigurationFollowUpQueued = false;
            QueueMacOsWindowConfiguration();
        }
    }

    private void HandleTitleBarPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (ReferenceEquals(sender, _moveDragTitleBar) && e.InitialPressMouseButton == MouseButton.Left)
        {
            ResetTitleBarMoveDragState();
        }
    }

    private void HandleTitleBarPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (ReferenceEquals(sender, _moveDragTitleBar))
        {
            ResetTitleBarMoveDragState();
        }
    }

    private void ResetTitleBarMoveDragState()
    {
        _moveDragTitleBar          = null;
        _lastMousePressedPoint     = null;
        _lastMousePressedEventArgs = null;
    }

    internal static double CalculateWindowsCsdMinimumHeight(double titleBarHeight)
    {
        return double.IsFinite(titleBarHeight) && titleBarHeight > 0
            ? titleBarHeight * WindowsCsdMinimumHeightInTitleBars
            : 0;
    }

    internal static Thickness CalculateEffectiveContentFrameMargin(
        Thickness windowDecorationMargin,
        bool isCsdEnabled,
        bool isTitleBarVisible,
        double drawnTitleBarHeight)
    {
        if (!isCsdEnabled ||
            isTitleBarVisible ||
            !double.IsFinite(drawnTitleBarHeight) ||
            drawnTitleBarHeight <= 0)
        {
            return windowDecorationMargin;
        }

        return new Thickness(
            windowDecorationMargin.Left,
            Math.Max(0, windowDecorationMargin.Top - drawnTitleBarHeight),
            windowDecorationMargin.Right,
            windowDecorationMargin.Bottom);
    }

    private void UpdateEffectiveContentFrameMargin()
    {
        EffectiveContentFrameMargin = CalculateEffectiveContentFrameMargin(
            WindowDecorationMargin,
            IsCsdEnabled,
            IsTitleBarVisible,
            this.GetDrawnDecorationsTitleBarHeight());
    }

    private void EnsureWindowsCsdMinimumHeight(double measuredTitleBarHeight = 0)
    {
        if (!OperatingSystem.IsWindows() || !IsCsdEnabled)
        {
            return;
        }

        var titleBarHeight = Math.Max(measuredTitleBarHeight, TitleBarHeight);
        var minimumHeight  = CalculateWindowsCsdMinimumHeight(titleBarHeight);
        if (minimumHeight > 0 && MinHeight < minimumHeight)
        {
            SetCurrentValue(MinHeightProperty, minimumHeight);
        }
    }
    
    internal IDisposable CreateTitleBarHostProjection(WindowTitleBar titleBar)
    {
        var lease = new CompositeDisposable();
        titleBar.MaximizeWindowRequested += HandleTitleDoubleClicked;
        titleBar.PointerPressed          += HandleTitleBarPointerPressed;
        titleBar.PointerReleased         += HandleTitleBarPointerReleased;
        titleBar.PointerMoved            += HandleTitleBarPointerMoved;
        titleBar.PointerCaptureLost      += HandleTitleBarPointerCaptureLost;
        lease.Add(Disposable.Create(() =>
        {
            titleBar.MaximizeWindowRequested -= HandleTitleDoubleClicked;
            titleBar.PointerPressed          -= HandleTitleBarPointerPressed;
            titleBar.PointerReleased         -= HandleTitleBarPointerReleased;
            titleBar.PointerMoved            -= HandleTitleBarPointerMoved;
            titleBar.PointerCaptureLost      -= HandleTitleBarPointerCaptureLost;
            if (ReferenceEquals(_moveDragTitleBar, titleBar))
            {
                ResetTitleBarMoveDragState();
            }
        }));

        try
        {
            lease.Add(titleBar.Bind(
                WindowTitleBar.NativeChromeInsetsProperty,
                this.GetObservable(NativeChromeInsetsProperty)));
            lease.Add(titleBar.Bind(
                WindowTitleBar.IsCsdEnabledProperty,
                this.GetObservable(IsCsdEnabledProperty)));
            lease.Add(titleBar.Bind(
                WindowTitleBar.HostWindowStateProperty,
                this.GetObservable(WindowStateProperty)));
            lease.Add(titleBar.Bind(
                WindowTitleBar.IsWindowActiveProperty,
                this.GetObservable(WindowBase.IsActiveProperty)));
            lease.Add(titleBar.Bind(
                WindowTitleBar.IsWindowTopmostProperty,
                this.GetObservable(TopmostProperty)));
            lease.Add(titleBar.Bind(
                WindowTitleBar.IsMinimizeCaptionButtonVisibleProperty,
                this.GetObservable(IsMinimizeCaptionButtonVisibleProperty)));
            lease.Add(titleBar.Bind(
                WindowTitleBar.IsMaximizeCaptionButtonVisibleProperty,
                this.GetObservable(IsMaximizeCaptionButtonVisibleProperty)));
            lease.Add(titleBar.Bind(
                WindowTitleBar.IsCloseCaptionButtonVisibleProperty,
                this.GetObservable(IsCloseCaptionButtonVisibleProperty)));
            lease.Add(titleBar.Bind(
                WindowTitleBar.IsFullScreenCaptionButtonVisibleProperty,
                this.GetObservable(IsFullScreenCaptionButtonVisibleProperty)));
            lease.Add(titleBar.Bind(
                WindowTitleBar.IsPinCaptionButtonVisibleProperty,
                this.GetObservable(IsPinCaptionButtonVisibleProperty)));
            lease.Add(titleBar.Bind(
                WindowTitleBar.CanMinimizeProperty,
                this.GetObservable(CanMinimizeProperty)));
            lease.Add(titleBar.Bind(
                WindowTitleBar.CanMaximizeProperty,
                this.GetObservable(CanMaximizeProperty)));
            lease.Add(titleBar.Bind(
                WindowTitleBar.IsPinCaptionButtonSupportedProperty,
                this.GetObservable(IsPinCaptionButtonSupportedProperty)));
            lease.Add(titleBar.Bind(
                WindowTitleBar.CaptionButtonCommandProperty,
                this.GetObservable(CaptionButtonCommandProperty)));
            lease.Add(this.GetObservable(EffectiveLogoProperty).Subscribe(
                _ => titleBar.NotifyHostEffectiveLogoChanged()));
            lease.Add(this.GetObservable(EffectiveLogoTemplateProperty).Subscribe(
                _ => titleBar.NotifyHostEffectiveLogoChanged()));
            return lease;
        }
        catch
        {
            lease.Dispose();
            throw;
        }
    }

    protected virtual void NotifyConfigureTitleBar(WindowTitleBar titleBar)
    {
        _defaultTitleBarBindings?.Dispose();
        _defaultTitleBarBindings = new CompositeDisposable
        {
            titleBar.Bind(WindowTitleBar.TitleProperty, this.GetObservable(TitleProperty)),
            titleBar.Bind(WindowTitleBar.LogoProperty, this.GetObservable(LogoProperty)),
            titleBar.Bind(WindowTitleBar.LogoTemplateProperty, this.GetObservable(LogoTemplateProperty)),
            titleBar.Bind(WindowTitleBar.LogoVisibilityProperty, this.GetObservable(LogoVisibilityProperty)),
            titleBar.Bind(WindowTitleBar.TitleAlignmentProperty, this.GetObservable(TitleAlignmentProperty)),
            titleBar.Bind(WindowTitleBar.IsTitleVisibleProperty, this.GetObservable(IsTitleVisibleProperty)),
            titleBar.Bind(
                WindowTitleBar.LeftAddOnProperty,
                this.GetObservable(LeftAddOnProperty),
                BindingPriority.Template),
            titleBar.Bind(
                WindowTitleBar.LeftAddOnTemplateProperty,
                this.GetObservable(LeftAddOnTemplateProperty),
                BindingPriority.Template),
            titleBar.Bind(
                WindowTitleBar.RightAddOnProperty,
                this.GetObservable(RightAddOnProperty),
                BindingPriority.Template),
            titleBar.Bind(
                WindowTitleBar.RightAddOnTemplateProperty,
                this.GetObservable(RightAddOnTemplateProperty),
                BindingPriority.Template)
        };
    }

    private bool CanExecuteCaptionButtonAction(CaptionButtonAction action)
    {
        return action switch
        {
            CaptionButtonAction.Minimize => CanMinimize && WindowState != WindowState.FullScreen,
            CaptionButtonAction.ToggleMaximize => CanMaximize && WindowState != WindowState.FullScreen,
            CaptionButtonAction.ToggleFullScreen => WindowState != WindowState.Maximized,
            CaptionButtonAction.TogglePin => IsPinCaptionButtonSupported,
            CaptionButtonAction.Close => true,
            _ => false
        };
    }

    private void ExecuteCaptionButtonAction(CaptionButtonAction action)
    {
        if (!CanExecuteCaptionButtonAction(action))
        {
            return;
        }

        switch (action)
        {
            case CaptionButtonAction.Minimize:
                WindowState = WindowState.Minimized;
                break;
            case CaptionButtonAction.ToggleMaximize:
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
                break;
            case CaptionButtonAction.ToggleFullScreen:
                WindowState = WindowState == WindowState.FullScreen
                    ? _windowStateBeforeFullScreen
                    : WindowState.FullScreen;
                break;
            case CaptionButtonAction.TogglePin:
                Topmost = !Topmost;
                break;
            case CaptionButtonAction.Close:
                NotifyCloseRequestByUser();
                Close();
                break;
        }
    }

    private void UpdateCaptionButtonCapabilities()
    {
        IsPinCaptionButtonSupported = _platformChromeManager?.SupportsPinCaptionButton == true;
    }

    protected virtual WindowTitleBar? NotifyCreateTitleBar(WindowTitleBar? oldTitleBar)
    {
        return new WindowTitleBar
        {
            Name = "PART_TitleBar"
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
    
    [SupportedOSPlatform("macos")]
    private void ConfigureMacOsWindow()
    {
        if (!ExtendClientAreaToDecorationsHint)
        {
            StopMacOsWindowButtonObserver();
            NativeChromeInsets = default;
            _macOsCachedButtonX = null;
            _macOsCachedButtonY = null;
            _macOsCacheValid   = false;
            return;
        }

        // AppKit 在录屏/窗口共享期间会暂时隐藏标准按钮。隐藏期间不写回坐标，
        // 等收到恢复通知后再执行一次完整布局，避免覆盖 AppKit 的共享控件状态。
        if (!this.IsStandardWindowButtonsVisible())
        {
            return;
        }

        double? titleBarHeight = null;
        if (!MathUtils.AreClose(ExtendClientAreaTitleBarHeightHint, -1))
        {
            titleBarHeight = ExtendClientAreaTitleBarHeightHint;
        }

        double? offsetX = null;
        double? spacing = null;
        if (IsSet(MacStandardWindowButtons.OffsetXProperty))
        {
            offsetX = MacStandardWindowButtons.GetOffsetX(this);
        }

        if (IsSet(MacStandardWindowButtons.SpacingProperty))
        {
            spacing = MacStandardWindowButtons.GetSpacing(this);
        }

        var effectSpacing = spacing ?? 20;
        var currentClientSize = ClientSize;

        // live resize 期间，除 ClientSize 外的输入都不变化；即使高度变了，AppKit 的 autoresizing
        // 会把按钮维持在相对标题栏的同一视觉位置。当 4 个输入都和上次完全一致时，
        // 底层 SetStandardWindowButtonsLayout 的 GetFrame 短路也会命中，这里提前返回，
        // 避免一次 P/Invoke 往返。
        if (_macOsCacheValid &&
            Nullable.Equals(_macOsCachedTitleBarHeight, titleBarHeight) &&
            Nullable.Equals(_macOsCachedOffsetX, offsetX) &&
            Nullable.Equals(_macOsCachedSpacing, spacing) &&
            _macOsCachedClientSize == currentClientSize)
        {
            return;
        }

        MacStandardWindowButtons.SetStandardWindowButtonsLayout(this, titleBarHeight, offsetX, null, effectSpacing);
        var offset = this.GetRecommendedTitleBarContentLeftMargin(effectSpacing);
        if (offset is not { } titleBarOffset)
        {
            // The title bar can measure before the native NSWindow standard buttons exist.
            // Do not cache that no-op pass, otherwise OnOpened may skip the first real correction.
            NativeChromeInsets = default;
            _macOsCacheValid = false;
            return;
        }

        NativeChromeInsets = new Thickness(titleBarOffset, 0, 0, 0);

        var closeFrame = this.GetStandardWindowButtonFrame(
            WindowUtilsInterop.NSWindowButton.CloseButton);
        if (closeFrame is not { } configuredCloseFrame)
        {
            NativeChromeInsets = default;
            _macOsCachedButtonX = null;
            _macOsCachedButtonY = null;
            _macOsCacheValid = false;
            return;
        }

        _macOsCachedTitleBarHeight = titleBarHeight;
        _macOsCachedOffsetX        = offsetX;
        _macOsCachedSpacing        = spacing;
        _macOsCachedClientSize     = currentClientSize;
        _macOsCachedButtonX        = configuredCloseFrame.X;
        _macOsCachedButtonY        = configuredCloseFrame.Y;
        _macOsCacheValid           = true;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        _isOpened = true;
        UpdateCaptionButtonCapabilities();
        ApplyCurrentWindowsCsdFrameTheme();
        _platformChromeManager?.UpdateFrameGeometry();
        UpdateEffectiveContentFrameMargin();
        UpdateEffectiveLogo();
        if (OperatingSystem.IsMacOS())
        {
            QueueMacOsWindowConfiguration();
            EnsureMacOsWindowButtonObserver();
        }

        if (!_mediaQueryReady)
        {
            LayoutUpdated += HandleFirstLayoutUpdatedForMediaQuery;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _isOpened = false;
        _transparencyFallbackBorder = null;
        StopMacOsWindowButtonObserver();
        if (_titleBar != null)
        {
            DetachTitleBar(_titleBar);
        }
        _fullscreenPopoverLayer?.Detach();
        _themeContextLease?.Dispose();
        _themeContextLease = null;
        ResetThemeContextState();
        ReleaseMainWindowLogoFallbackSubscription();
        base.OnClosed(e);
    }

    private void UpdateEffectiveLogo()
    {
        var mainWindow = GetMainWindow();
        UpdateMainWindowLogoFallbackSubscription(mainWindow);
        var (logo, template) = ResolveEffectiveLogo(this, mainWindow);
        EffectiveLogo         = logo;
        EffectiveLogoTemplate = template;
    }

    private void UpdateMainWindowLogoFallbackSubscription(Window? mainWindow)
    {
        var fallbackSource = _isOpened &&
                             Logo is null &&
                             LogoTemplate is null &&
                             Icon is null &&
                             mainWindow is not null &&
                             !ReferenceEquals(mainWindow, this)
            ? mainWindow
            : null;
        if (ReferenceEquals(_mainWindowLogoFallbackSource, fallbackSource))
        {
            return;
        }

        ReleaseMainWindowLogoFallbackSubscription();
        if (fallbackSource is null)
        {
            return;
        }

        var lease = new CompositeDisposable();
        _mainWindowLogoFallbackSource = fallbackSource;
        _mainWindowLogoFallbackLease  = lease;
        try
        {
            lease.Add(fallbackSource.GetObservable(LogoProperty).Subscribe(_ => UpdateEffectiveLogo()));
            lease.Add(fallbackSource.GetObservable(LogoTemplateProperty).Subscribe(_ => UpdateEffectiveLogo()));
            lease.Add(fallbackSource.GetObservable(IconProperty).Subscribe(_ => UpdateEffectiveLogo()));
        }
        catch
        {
            ReleaseMainWindowLogoFallbackSubscription();
            throw;
        }
    }

    private void ReleaseMainWindowLogoFallbackSubscription()
    {
        _mainWindowLogoFallbackSource = null;
        _mainWindowLogoFallbackLease?.Dispose();
        _mainWindowLogoFallbackLease = null;
    }

    // Logo 回退解析（纯函数）：显式 Logo/LogoTemplate 优先；都未设置时依次回退到
    // 本窗口 Icon → 主窗口显式 Logo/LogoTemplate → 主窗口 Icon。
    // 回退结果只进 EffectiveLogo/EffectiveLogoTemplate 供渲染使用，绝不写回公开属性，
    // 避免"运行时设置 Logo 时残留框架注入模板"之类的状态污染。
    internal static (object? Logo, IDataTemplate? Template) ResolveEffectiveLogo(
        Window window,
        Window? mainWindow)
    {
        if (window.Logo is not null || window.LogoTemplate is not null)
        {
            return (window.Logo, window.LogoTemplate);
        }

        if (window.Icon is { } hostIcon)
        {
            return (hostIcon, s_windowIconLogoTemplate);
        }

        if (mainWindow is not null && !ReferenceEquals(mainWindow, window))
        {
            if (mainWindow.Logo is not null || mainWindow.LogoTemplate is not null)
            {
                return (mainWindow.Logo, mainWindow.LogoTemplate);
            }

            if (mainWindow.Icon is { } mainIcon)
            {
                return (mainIcon, s_windowIconLogoTemplate);
            }
        }

        return (null, null);
    }

    private static Control? CreateWindowIconLogo(WindowIcon? icon)
    {
        if (icon == null)
        {
            return null;
        }

        try
        {
            using var stream = new MemoryStream();
            icon.Save(stream);
            stream.Position = 0;
            return new Image
            {
                Source  = new Bitmap(stream),
                Stretch = Stretch.Uniform
            };
        }
        catch
        {
            return null;
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (OperatingSystem.IsMacOS())
        {
            QueueMacOsWindowConfiguration();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TransparencyBackgroundFallbackProperty &&
            ActualTransparencyLevel == WindowTransparencyLevel.None &&
            _transparencyFallbackBorder is not null)
        {
            // TopLevel writes this part's Background as a local value when applying the template
            // or changing transparency, overriding ControlTheme setters and TemplateBinding.
            // Mirror later fallback changes here; TopLevel still owns transparency transitions.
            _transparencyFallbackBorder.Background = TransparencyBackgroundFallback;
        }
        if (change.Property == WindowStateProperty)
        {
            var (oldState, newState) = change.GetOldAndNewValue<WindowState>();
            if (newState == WindowState.FullScreen && oldState != WindowState.FullScreen)
            {
                _windowStateBeforeFullScreen = oldState == WindowState.Minimized
                    ? WindowState.Normal
                    : oldState;
            }
        }
        if (change.Property == CanMinimizeProperty ||
            change.Property == CanMaximizeProperty ||
            change.Property == WindowStateProperty ||
            change.Property == IsPinCaptionButtonSupportedProperty)
        {
            _captionButtonCommand?.RaiseCanExecuteChanged();
        }
        if (change.Property == WindowStateProperty ||
            change.Property == ExtendClientAreaTitleBarHeightHintProperty ||
            change.Property == TitleProperty ||
            change.Property == WindowDecorationsProperty)
        {
            if (OperatingSystem.IsMacOS())
            {
                // WindowState / Title / WindowDecorations 变化都可能让 AppKit 重置 standard button frame。
                // 即使 AtomUI 的布局输入没有变化，原生按钮当前位置也可能已经偏离目标，
                // 所以必须先让缓存失效再重新下发布局。
                QueueMacOsWindowConfiguration();
                if (change.Property == TitleProperty ||
                    change.Property == WindowDecorationsProperty)
                {
                    QueueMacOsWindowConfiguration(forceFollowUp: true);
                }
            }
        }
        if (change.Property == ExtendClientAreaTitleBarHeightHintProperty ||
            change.Property == IsCsdEnabledProperty ||
            change.Property == ActualThemeVariantProperty)
        {
            ApplyCurrentWindowsCsdFrameTheme();
        }
        _platformChromeManager?.HandlePropertyChanged(change.Property);
        if (change.Property == IsExtendedIntoWindowDecorationsProperty)
        {
            RefreshPlatformCsdStatus();
        }
        if (change.Property == CanResizeProperty || change.Property == WindowStateProperty)
        {
            ConfigureCustomResizerVisible();
        }
        if (change.Property == MinHeightProperty ||
            change.Property == TitleBarHeightProperty ||
            change.Property == IsCsdEnabledProperty)
        {
            EnsureWindowsCsdMinimumHeight();
        }
        if (change.Property == AvaloniaWindow.WindowDecorationMarginProperty ||
            change.Property == IsTitleBarVisibleProperty ||
            change.Property == IsCsdEnabledProperty)
        {
            UpdateEffectiveContentFrameMargin();
        }
        if (change.Property == IsMoveEnabledProperty ||
            change.Property == WindowStateProperty)
        {
            ResetTitleBarMoveDragState();
        }
        if (change.Property == TitleProperty ||
            change.Property == IsTitleVisibleProperty)
        {
            UpdateEffectiveFullscreenTitleVisible();
        }
        if (change.Property == LogoProperty ||
            change.Property == LogoTemplateProperty ||
            change.Property == IconProperty)
        {
            UpdateEffectiveLogo();
        }
        if (change.Property == EffectiveLogoProperty ||
            change.Property == EffectiveLogoTemplateProperty ||
            change.Property == LogoVisibilityProperty ||
            change.Property == TitleProperty ||
            change.Property == IsTitleVisibleProperty ||
            change.Property == WindowStateProperty)
        {
            UpdateEffectiveFullscreenLogoVisible();
        }
    }

    private void UpdateEffectiveFullscreenTitleVisible()
    {
        IsEffectiveFullscreenTitleVisible = IsTitleVisible && Title is not null;
    }

    private void UpdateEffectiveFullscreenLogoVisible()
    {
        var hasLogo = EffectiveLogo is not null || EffectiveLogoTemplate is not null;
        var isLogoVisibleByMode = LogoVisibility switch
        {
            WindowTitleBarLogoVisibility.Always => hasLogo,
            WindowTitleBarLogoVisibility.Never => false,
            _ => hasLogo && HasTitleContent(Title) && IsEffectiveFullscreenTitleVisible
        };
        IsEffectiveFullscreenLogoVisible = WindowState == WindowState.FullScreen && isLogoVisibleByMode;
        UpdateEffectiveFullscreenLogo();
    }

    private void UpdateEffectiveFullscreenLogo()
    {
        if (IsEffectiveFullscreenLogoVisible)
        {
            EffectiveFullscreenLogo         = EffectiveLogo;
            EffectiveFullscreenLogoTemplate = EffectiveLogoTemplate;
        }
        else
        {
            EffectiveFullscreenLogo         = null;
            EffectiveFullscreenLogoTemplate = null;
        }
    }

    private static bool HasTitleContent(object? title)
    {
        return title switch
        {
            null => false,
            string text => !string.IsNullOrWhiteSpace(text),
            _ => true
        };
    }

    private void ConfigureCsdStatus()
    {
        if (OperatingSystem.IsMacOS())
        {
            IsCsdEnabled = false;
        }
        else if (OperatingSystem.IsLinux())
        {
            IsCsdEnabled = PlatformImpl?.NeedsManagedDecorations == true;
        }
        else if (OperatingSystem.IsWindows())
        {
            IsCsdEnabled = true;
        }
    }

    internal void RefreshPlatformCsdStatus()
    {
        ConfigureCsdStatus();
        UpdateCaptionButtonCapabilities();
        ApplyCurrentWindowsCsdFrameTheme();
        ConfigureCustomResizerVisible();
    }

    private void ApplyCurrentWindowsCsdFrameTheme()
    {
        if ((PlatformThemeVariant?)ActualThemeVariant is { } platformThemeVariant)
        {
            ApplyWindowsCsdFrameTheme(platformThemeVariant == PlatformThemeVariant.Dark);
        }
    }

    private void ApplyWindowsCsdFrameTheme(bool isDarkMode)
    {
        if (!OperatingSystem.IsWindows() || !IsCsdEnabled)
        {
            return;
        }

        this.SetWindowsCsdFrameDarkMode(isDarkMode);
    }

    private void ConfigureCustomResizerVisible()
    {
        IsCustomResizerVisible = _platformChromeManager?.UsesCustomResizer == true &&
                                 CanResize &&
                                 WindowState == WindowState.Normal;
    }

    private sealed class WindowCaptionButtonCommand(Window owner) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        internal void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object? parameter)
        {
            return parameter is CaptionButtonAction action && owner.CanExecuteCaptionButtonAction(action);
        }

        public void Execute(object? parameter)
        {
            if (parameter is CaptionButtonAction action)
            {
                owner.ExecuteCaptionButtonAction(action);
            }
        }
    }
}
