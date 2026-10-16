using System.Windows.Input;
using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.Active)]
[PseudoClasses(StdPseudoClass.Normal, StdPseudoClass.Minimized, StdPseudoClass.Maximized, StdPseudoClass.Fullscreen)]
[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
public class WindowTitleBar : TemplatedControl, 
                              IMotionAwareControl, 
                              IOperationSystemAware
{
    #region 公共属性定义

    public static readonly StyledProperty<object?> LogoProperty =
        AvaloniaProperty.Register<WindowTitleBar, object?>(nameof(Logo));
    
    public static readonly StyledProperty<IDataTemplate?> LogoTemplateProperty =
        AvaloniaProperty.Register<WindowTitleBar, IDataTemplate?>(nameof(LogoTemplate));

    public static readonly StyledProperty<WindowTitleBarLogoVisibility> LogoVisibilityProperty =
        AvaloniaProperty.Register<WindowTitleBar, WindowTitleBarLogoVisibility>(
            nameof(LogoVisibility),
            WindowTitleBarLogoVisibility.Auto);
    
    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<WindowTitleBar, object?>(nameof(Title));
    
    public static readonly StyledProperty<IDataTemplate?> TitleTemplateProperty = 
        AvaloniaProperty.Register<WindowTitleBar, IDataTemplate?>(nameof (TitleTemplate));

    public static readonly StyledProperty<WindowTitleBarTitleAlignment> TitleAlignmentProperty =
        AvaloniaProperty.Register<WindowTitleBar, WindowTitleBarTitleAlignment>(
            nameof(TitleAlignment),
            WindowTitleBarTitleAlignment.Auto);

    public static readonly StyledProperty<bool> IsTitleVisibleProperty =
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(IsTitleVisible), true);
    
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AvaloniaProperty.Register<WindowTitleBar, object?>(nameof(LeftAddOn));

    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AvaloniaProperty.Register<WindowTitleBar, IDataTemplate?>(nameof(LeftAddOnTemplate));
    
    public static readonly StyledProperty<object?> RightAddOnProperty =
        AvaloniaProperty.Register<WindowTitleBar, object?>(nameof(RightAddOn));

    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AvaloniaProperty.Register<WindowTitleBar, IDataTemplate?>(nameof(RightAddOnTemplate));
    
    public static readonly StyledProperty<bool> IsWindowActiveProperty = 
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(IsWindowActive));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<WindowTitleBar>();
    
    public static readonly StyledProperty<OsType> OsTypeProperty =
        OperationSystemAwareControlProperty.OsTypeProperty.AddOwner<WindowTitleBar>();
    
    public static readonly StyledProperty<Version> OsVersionProperty =
        OperationSystemAwareControlProperty.OsVersionProperty.AddOwner<WindowTitleBar>();

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
    
    [DependsOn(nameof(TitleTemplate))]
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public IDataTemplate? TitleTemplate
    {
        get => GetValue(TitleTemplateProperty);
        set => SetValue(TitleTemplateProperty, value);
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

    public bool IsWindowActive
    {
        get => GetValue(IsWindowActiveProperty);
        set => SetValue(IsWindowActiveProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public OsType OsType => GetValue(OsTypeProperty);
    public Version OsVersion => GetValue(OsVersionProperty);
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<WindowTitleBar, object?> EffectiveLogoProperty =
        AvaloniaProperty.RegisterDirect<WindowTitleBar, object?>(
            nameof(EffectiveLogo),
            o => o.EffectiveLogo);

    internal static readonly DirectProperty<WindowTitleBar, IDataTemplate?> EffectiveLogoTemplateProperty =
        AvaloniaProperty.RegisterDirect<WindowTitleBar, IDataTemplate?>(
            nameof(EffectiveLogoTemplate),
            o => o.EffectiveLogoTemplate);

    internal static readonly DirectProperty<WindowTitleBar, object?> EffectiveLogoPresenterContentProperty =
        AvaloniaProperty.RegisterDirect<WindowTitleBar, object?>(
            nameof(EffectiveLogoPresenterContent),
            o => o.EffectiveLogoPresenterContent);

    internal static readonly DirectProperty<WindowTitleBar, IDataTemplate?> EffectiveLogoPresenterContentTemplateProperty =
        AvaloniaProperty.RegisterDirect<WindowTitleBar, IDataTemplate?>(
            nameof(EffectiveLogoPresenterContentTemplate),
            o => o.EffectiveLogoPresenterContentTemplate);

    internal static readonly DirectProperty<WindowTitleBar, bool> IsEffectiveLogoVisibleProperty =
        AvaloniaProperty.RegisterDirect<WindowTitleBar, bool>(
            nameof(IsEffectiveLogoVisible),
            o => o.IsEffectiveLogoVisible);

    internal static readonly DirectProperty<WindowTitleBar, bool> IsEffectiveTitleVisibleProperty =
        AvaloniaProperty.RegisterDirect<WindowTitleBar, bool>(
            nameof(IsEffectiveTitleVisible),
            o => o.IsEffectiveTitleVisible);

    internal static readonly StyledProperty<Thickness> NativeChromeInsetsProperty =
        AvaloniaProperty.Register<WindowTitleBar, Thickness>(nameof(NativeChromeInsets));

    internal static readonly StyledProperty<bool> IsCsdEnabledProperty =
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(IsCsdEnabled));

    internal static readonly StyledProperty<WindowState> HostWindowStateProperty =
        AvaloniaProperty.Register<WindowTitleBar, WindowState>(nameof(HostWindowState));

    internal static readonly StyledProperty<bool> IsWindowTopmostProperty =
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(IsWindowTopmost));

    internal static readonly StyledProperty<bool> IsMinimizeCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(IsMinimizeCaptionButtonVisible), true);

    internal static readonly StyledProperty<bool> IsMaximizeCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(IsMaximizeCaptionButtonVisible), true);

    internal static readonly StyledProperty<bool> IsCloseCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(IsCloseCaptionButtonVisible), true);

    internal static readonly StyledProperty<bool> IsFullScreenCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(IsFullScreenCaptionButtonVisible));

    internal static readonly StyledProperty<bool> IsPinCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(IsPinCaptionButtonVisible));

    internal static readonly StyledProperty<bool> CanMinimizeProperty =
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(CanMinimize), true);

    internal static readonly StyledProperty<bool> CanMaximizeProperty =
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(CanMaximize), true);

    internal static readonly StyledProperty<bool> IsPinCaptionButtonSupportedProperty =
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(IsPinCaptionButtonSupported));

    internal static readonly StyledProperty<ICommand?> CaptionButtonCommandProperty =
        AvaloniaProperty.Register<WindowTitleBar, ICommand?>(nameof(CaptionButtonCommand));

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

    private object? _effectiveLogoPresenterContent;

    internal object? EffectiveLogoPresenterContent
    {
        get => _effectiveLogoPresenterContent;
        private set => SetAndRaise(
            EffectiveLogoPresenterContentProperty,
            ref _effectiveLogoPresenterContent,
            value);
    }

    private IDataTemplate? _effectiveLogoPresenterContentTemplate;

    internal IDataTemplate? EffectiveLogoPresenterContentTemplate
    {
        get => _effectiveLogoPresenterContentTemplate;
        private set => SetAndRaise(
            EffectiveLogoPresenterContentTemplateProperty,
            ref _effectiveLogoPresenterContentTemplate,
            value);
    }

    private bool _isEffectiveLogoVisible;

    internal bool IsEffectiveLogoVisible
    {
        get => _isEffectiveLogoVisible;
        private set => SetAndRaise(IsEffectiveLogoVisibleProperty, ref _isEffectiveLogoVisible, value);
    }

    private bool _isEffectiveTitleVisible;

    internal bool IsEffectiveTitleVisible
    {
        get => _isEffectiveTitleVisible;
        private set => SetAndRaise(IsEffectiveTitleVisibleProperty, ref _isEffectiveTitleVisible, value);
    }

    internal Thickness NativeChromeInsets
    {
        get => GetValue(NativeChromeInsetsProperty);
        set => SetValue(NativeChromeInsetsProperty, value);
    }

    internal bool IsCsdEnabled
    {
        get => GetValue(IsCsdEnabledProperty);
        set => SetValue(IsCsdEnabledProperty, value);
    }

    internal WindowState HostWindowState
    {
        get => GetValue(HostWindowStateProperty);
        set => SetValue(HostWindowStateProperty, value);
    }

    internal bool IsWindowTopmost
    {
        get => GetValue(IsWindowTopmostProperty);
        set => SetValue(IsWindowTopmostProperty, value);
    }

    internal bool IsMinimizeCaptionButtonVisible
    {
        get => GetValue(IsMinimizeCaptionButtonVisibleProperty);
        set => SetValue(IsMinimizeCaptionButtonVisibleProperty, value);
    }

    internal bool IsMaximizeCaptionButtonVisible
    {
        get => GetValue(IsMaximizeCaptionButtonVisibleProperty);
        set => SetValue(IsMaximizeCaptionButtonVisibleProperty, value);
    }

    internal bool IsCloseCaptionButtonVisible
    {
        get => GetValue(IsCloseCaptionButtonVisibleProperty);
        set => SetValue(IsCloseCaptionButtonVisibleProperty, value);
    }

    internal bool IsFullScreenCaptionButtonVisible
    {
        get => GetValue(IsFullScreenCaptionButtonVisibleProperty);
        set => SetValue(IsFullScreenCaptionButtonVisibleProperty, value);
    }

    internal bool IsPinCaptionButtonVisible
    {
        get => GetValue(IsPinCaptionButtonVisibleProperty);
        set => SetValue(IsPinCaptionButtonVisibleProperty, value);
    }

    internal bool CanMinimize
    {
        get => GetValue(CanMinimizeProperty);
        set => SetValue(CanMinimizeProperty, value);
    }

    internal bool CanMaximize
    {
        get => GetValue(CanMaximizeProperty);
        set => SetValue(CanMaximizeProperty, value);
    }

    internal bool IsPinCaptionButtonSupported
    {
        get => GetValue(IsPinCaptionButtonSupportedProperty);
        set => SetValue(IsPinCaptionButtonSupportedProperty, value);
    }

    internal ICommand? CaptionButtonCommand
    {
        get => GetValue(CaptionButtonCommandProperty);
        set => SetValue(CaptionButtonCommandProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler? MaximizeWindowRequested;

    #endregion

    private Window? _window;
    private IDisposable? _hostProjectionLease;

    internal Window? HostWindow => _window;

    static WindowTitleBar()
    {
        FontSizeProperty.OverrideDefaultValue<WindowTitleBar>(13);
        FontWeightProperty.OverrideDefaultValue<WindowTitleBar>(FontWeight.Bold);
    }

    public WindowTitleBar()
    {
        this.ConfigureOsType();
        UpdateWindowStatePseudoClasses(HostWindowState);
        UpdateEffectiveTitleVisible();
        UpdateEffectiveLogo();
    }

    internal void AttachHost(Window window)
    {
        if (ReferenceEquals(_window, window))
        {
            return;
        }

        ReleaseHost();
        _window = window;
        window.Closed += HandleHostWindowClosed;
        try
        {
            _hostProjectionLease = window.CreateTitleBarHostProjection(this);
            UpdateEffectiveLogo();
        }
        catch
        {
            window.Closed -= HandleHostWindowClosed;
            _window = null;
            UpdateEffectiveLogo();
            throw;
        }
    }

    internal void DetachHost(Window window)
    {
        if (ReferenceEquals(_window, window))
        {
            ReleaseHost();
        }
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (this.FindLogicalAncestorOfType<Window>() is { } window)
        {
            AttachHost(window);
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        ReleaseHost();
        base.OnDetachedFromLogicalTree(e);
    }

    private void HandleHostWindowClosed(object? sender, EventArgs e)
    {
        if (sender is Window window)
        {
            DetachHost(window);
        }
    }

    internal void NotifyHostEffectiveLogoChanged()
    {
        UpdateEffectiveLogo();
    }

    private void ReleaseHost()
    {
        if (_window is { } window)
        {
            window.Closed -= HandleHostWindowClosed;
            _window = null;
        }

        _hostProjectionLease?.Dispose();
        _hostProjectionLease = null;
        UpdateEffectiveLogo();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TitleProperty ||
            change.Property == IsTitleVisibleProperty)
        {
            UpdateEffectiveTitleVisible();
        }
        if (change.Property == LogoProperty ||
            change.Property == LogoTemplateProperty)
        {
            UpdateEffectiveLogo();
        }
        if (change.Property == LogoVisibilityProperty ||
            change.Property == TitleProperty ||
            change.Property == IsTitleVisibleProperty ||
            change.Property == OsTypeProperty ||
            change.Property == HostWindowStateProperty)
        {
            UpdateEffectiveLogoVisible();
        }
        if (change.Property == HostWindowStateProperty)
        {
            UpdateWindowStatePseudoClasses(change.GetNewValue<WindowState>());
        }
        else if (change.Property == IsWindowActiveProperty)
        {
            PseudoClasses.Set(StdPseudoClass.Active, change.GetNewValue<bool>());
        }
    }

    private void UpdateWindowStatePseudoClasses(WindowState state)
    {
        PseudoClasses.Set(StdPseudoClass.Minimized, state == WindowState.Minimized);
        PseudoClasses.Set(StdPseudoClass.Normal, state == WindowState.Normal);
        PseudoClasses.Set(StdPseudoClass.Maximized, state == WindowState.Maximized);
        PseudoClasses.Set(StdPseudoClass.Fullscreen, state == WindowState.FullScreen);
    }

    private void UpdateEffectiveLogo()
    {
        if (Logo is not null || LogoTemplate is not null)
        {
            EffectiveLogo         = Logo;
            EffectiveLogoTemplate = LogoTemplate;
        }
        else
        {
            EffectiveLogo         = HostWindow?.EffectiveLogo;
            EffectiveLogoTemplate = HostWindow?.EffectiveLogoTemplate;
        }

        UpdateEffectiveLogoVisible();
    }

    private void UpdateEffectiveLogoVisible()
    {
        var hasLogo = EffectiveLogo is not null || EffectiveLogoTemplate is not null;
        var isLogoVisibleByMode = LogoVisibility switch
        {
            WindowTitleBarLogoVisibility.Always => hasLogo,
            WindowTitleBarLogoVisibility.Never => false,
            _ => hasLogo && ShouldShowLogoInAutoMode()
        };
        IsEffectiveLogoVisible = HostWindowState != WindowState.FullScreen && isLogoVisibleByMode;
        UpdateEffectiveLogoPresenterContent();
    }

    private void UpdateEffectiveLogoPresenterContent()
    {
        if (IsEffectiveLogoVisible)
        {
            EffectiveLogoPresenterContent         = EffectiveLogo;
            EffectiveLogoPresenterContentTemplate = EffectiveLogoTemplate;
        }
        else
        {
            EffectiveLogoPresenterContent         = null;
            EffectiveLogoPresenterContentTemplate = null;
        }
    }

    private void UpdateEffectiveTitleVisible()
    {
        IsEffectiveTitleVisible = IsTitleVisible && Title is not null;
    }

    private bool ShouldShowLogoInAutoMode()
    {
        if (HasTitleContent(Title) && IsEffectiveTitleVisible)
        {
            return true;
        }

        if (OsType == OsType.macOS)
        {
            return false;
        }

        return HostWindowState != WindowState.FullScreen;
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
    
    private bool _doubleClickPending;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.ClickCount == 2 && e.Properties.IsLeftButtonPressed)
        {
            // 不在 PointerPressed 里直接修改 WindowState：在 Windows 上，maximized → normal
            // 的切换由 DWM 同步执行 resize，而 Avalonia 在 PointerPressed 阶段对 source 做了
            // 隐式 pointer capture，resize 会破坏当前 pointer 事件链，导致 PointerReleased /
            // 后续 hover 失效（必须再点一次才会恢复）。把动作延后到 PointerReleased，
            // 跟点击最大化按钮的路径一致，可彻底规避此问题。
            _doubleClickPending = true;
            e.Handled           = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_doubleClickPending && e.InitialPressMouseButton == MouseButton.Left)
        {
            _doubleClickPending = false;
            MaximizeWindowRequested?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            _doubleClickPending = false;
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _doubleClickPending = false;
    }

    void IOperationSystemAware.SetOsType(OsType osType)
    {
        SetValue(OsTypeProperty, osType);
    }
    
    void IOperationSystemAware.SetOsVersion(Version version)
    {
        SetValue(OsVersionProperty, version);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.DisableTransitions();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Dispatcher.Post(this.EnableTransitions);
    }
}
