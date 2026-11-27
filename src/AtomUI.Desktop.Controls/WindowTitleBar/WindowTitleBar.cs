using System.Windows.Input;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Title bar for a desktop window: the logo, the title, optional add-on content on both sides of the title and
/// the caption buttons, laid out for the host operating system.
/// </summary>
/// <remarks>
/// Once attached to the logical tree, the bar binds to the nearest ancestor <see cref="Window" />, exposed as
/// <c>HostWindow</c>. The host keeps <see cref="IsWindowActive" />, the window state and the caption button
/// settings of the bar current, and the bar falls back to the logo of the host when neither
/// <see cref="Logo" /> nor <see cref="LogoTemplate" /> is set. Without a host window the bar renders with its
/// own values.
/// </remarks>
[PseudoClasses(StdPseudoClass.Active)]
[PseudoClasses(StdPseudoClass.Normal, StdPseudoClass.Minimized, StdPseudoClass.Maximized, StdPseudoClass.Fullscreen)]
[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
public class WindowTitleBar : TemplatedControl, 
                              IMotionAwareControl, 
                              IOperationSystemAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Logo" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> LogoProperty =
        AvaloniaProperty.Register<WindowTitleBar, object?>(nameof(Logo));
    
    /// <summary>
    /// Identifies the <see cref="LogoTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LogoTemplateProperty =
        AvaloniaProperty.Register<WindowTitleBar, IDataTemplate?>(nameof(LogoTemplate));

    /// <summary>
    /// Identifies the <see cref="LogoVisibility" /> property.
    /// </summary>
    public static readonly StyledProperty<WindowTitleBarLogoVisibility> LogoVisibilityProperty =
        AvaloniaProperty.Register<WindowTitleBar, WindowTitleBarLogoVisibility>(
            nameof(LogoVisibility),
            WindowTitleBarLogoVisibility.Auto);
    
    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<WindowTitleBar, object?>(nameof(Title));
    
    /// <summary>
    /// Identifies the <see cref="TitleTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> TitleTemplateProperty = 
        AvaloniaProperty.Register<WindowTitleBar, IDataTemplate?>(nameof (TitleTemplate));

    /// <summary>
    /// Identifies the <see cref="TitleAlignment" /> property.
    /// </summary>
    public static readonly StyledProperty<WindowTitleBarTitleAlignment> TitleAlignmentProperty =
        AvaloniaProperty.Register<WindowTitleBar, WindowTitleBarTitleAlignment>(
            nameof(TitleAlignment),
            WindowTitleBarTitleAlignment.Auto);

    /// <summary>
    /// Identifies the <see cref="IsTitleVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTitleVisibleProperty =
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(IsTitleVisible), true);
    
    /// <summary>
    /// Identifies the <see cref="LeftAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AvaloniaProperty.Register<WindowTitleBar, object?>(nameof(LeftAddOn));

    /// <summary>
    /// Identifies the <see cref="LeftAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AvaloniaProperty.Register<WindowTitleBar, IDataTemplate?>(nameof(LeftAddOnTemplate));
    
    /// <summary>
    /// Identifies the <see cref="RightAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> RightAddOnProperty =
        AvaloniaProperty.Register<WindowTitleBar, object?>(nameof(RightAddOn));

    /// <summary>
    /// Identifies the <see cref="RightAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AvaloniaProperty.Register<WindowTitleBar, IDataTemplate?>(nameof(RightAddOnTemplate));
    
    /// <summary>
    /// Identifies the <see cref="IsWindowActive" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsWindowActiveProperty = 
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(IsWindowActive));
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<WindowTitleBar>();
    
    /// <summary>
    /// Identifies the <see cref="OsType" /> property.
    /// </summary>
    public static readonly StyledProperty<OsType> OsTypeProperty =
        OperationSystemAwareControlProperty.OsTypeProperty.AddOwner<WindowTitleBar>();
    
    /// <summary>
    /// Identifies the <see cref="OsVersion" /> property.
    /// </summary>
    public static readonly StyledProperty<Version> OsVersionProperty =
        OperationSystemAwareControlProperty.OsVersionProperty.AddOwner<WindowTitleBar>();

    /// <summary>
    /// Logo shown at the leading end of the title bar, before the left add-on and the title.
    /// </summary>
    /// <remarks>
    /// <see cref="LogoTemplate" /> builds the content. When both are <see langword="null" />, the logo of the host
    /// window is used instead. <see cref="LogoVisibility" /> decides whether the logo region is shown.
    /// </remarks>
    [DependsOn(nameof(LogoTemplate))]
    public object? Logo
    {
        get => GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }
    
    /// <summary>
    /// Template that builds <see cref="Logo" />.
    /// </summary>
    /// <remarks>
    /// As soon as this template is set, the bar stops using the logo of the host window, even while
    /// <see cref="Logo" /> is <see langword="null" />.
    /// </remarks>
    public IDataTemplate? LogoTemplate
    {
        get => GetValue(LogoTemplateProperty);
        set => SetValue(LogoTemplateProperty, value);
    }

    /// <summary>
    /// When the logo region of the title bar is shown.
    /// </summary>
    /// <remarks>
    /// <see cref="WindowTitleBarLogoVisibility.Always" /> shows the logo whenever one is available and
    /// <see cref="WindowTitleBarLogoVisibility.Never" /> hides it. The default,
    /// <see cref="WindowTitleBarLogoVisibility.Auto" />, shows the logo if one is available, except on macOS when
    /// the title bar has no visible title.
    /// </remarks>
    public WindowTitleBarLogoVisibility LogoVisibility
    {
        get => GetValue(LogoVisibilityProperty);
        set => SetValue(LogoVisibilityProperty, value);
    }
    
    /// <summary>
    /// Title shown in the title bar; <see cref="TitleTemplate" /> builds the content.
    /// </summary>
    /// <remarks>
    /// The title is hidden while this value is <see langword="null" />. A null value or a blank string counts as
    /// having no title for the <c>Auto</c> mode of <see cref="LogoVisibility" />. The default template trims long
    /// text with an ellipsis instead of wrapping it.
    /// </remarks>
    [DependsOn(nameof(TitleTemplate))]
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    /// <summary>
    /// Template that builds <see cref="Title" />.
    /// </summary>
    /// <remarks>
    /// The title stays hidden while <see cref="Title" /> is <see langword="null" />, even when a template is set.
    /// </remarks>
    public IDataTemplate? TitleTemplate
    {
        get => GetValue(TitleTemplateProperty);
        set => SetValue(TitleTemplateProperty, value);
    }

    /// <summary>
    /// Alignment of the title within the title bar.
    /// </summary>
    /// <remarks>
    /// The default, <see cref="WindowTitleBarTitleAlignment.Auto" />, follows the host operating system: centered
    /// on macOS and left-aligned on Windows and Linux.
    /// </remarks>
    public WindowTitleBarTitleAlignment TitleAlignment
    {
        get => GetValue(TitleAlignmentProperty);
        set => SetValue(TitleAlignmentProperty, value);
    }

    /// <summary>
    /// Whether the title is shown in the title bar.
    /// </summary>
    /// <remarks>
    /// <see langword="false" /> hides the title region of this bar only; the window title itself is unaffected.
    /// The title also stays hidden while <see cref="Title" /> is <see langword="null" />.
    /// </remarks>
    public bool IsTitleVisible
    {
        get => GetValue(IsTitleVisibleProperty);
        set => SetValue(IsTitleVisibleProperty, value);
    }

    /// <summary>
    /// Content shown between the logo and the title.
    /// </summary>
    /// <remarks>
    /// <see cref="LeftAddOnTemplate" /> builds the content. The region is hidden while this value is
    /// <see langword="null" />. The theme provides the active state, the motion setting and the operating system
    /// of the host to this region through <c>WindowTitleBarHostContext</c> attached values for the title bar
    /// buttons placed in it.
    /// </remarks>
    [DependsOn(nameof(LeftAddOnTemplate))]
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }

    /// <summary>
    /// Template that builds <see cref="LeftAddOn" />.
    /// </summary>
    public IDataTemplate? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }
    
    /// <summary>
    /// Content shown after the title and before the caption buttons.
    /// </summary>
    /// <remarks>
    /// <see cref="RightAddOnTemplate" /> builds the content. The region is hidden while this value is
    /// <see langword="null" />. The theme provides the active state, the motion setting and the operating system
    /// of the host to this region through <c>WindowTitleBarHostContext</c> attached values for the title bar
    /// buttons placed in it.
    /// </remarks>
    [DependsOn(nameof(RightAddOnTemplate))]
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }

    /// <summary>
    /// Template that builds <see cref="RightAddOn" />.
    /// </summary>
    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Whether the host window is active.
    /// </summary>
    /// <remarks>
    /// <see langword="true" /> sets the <see cref="StdPseudoClass.Active" /> pseudo-class; the caption buttons and
    /// the title bar buttons in the add-on regions use it to choose their active and inactive appearance. For a
    /// bar bound to a host window the value follows the window, and an explicitly set value is replaced.
    /// </remarks>
    public bool IsWindowActive
    {
        get => GetValue(IsWindowActiveProperty);
        set => SetValue(IsWindowActiveProperty, value);
    }
    
    /// <summary>
    /// Whether motion is enabled for the caption buttons and the title bar buttons in the add-on regions.
    /// </summary>
    /// <remarks>
    /// <see langword="false" /> turns their motion off. The value is passed to the caption button group and,
    /// through <c>WindowTitleBarHostContext</c>, to the add-on regions.
    /// </remarks>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Operating system the title bar is laid out for.
    /// </summary>
    /// <remarks>
    /// It selects the template of the theme and the default title alignment of
    /// <see cref="WindowTitleBarTitleAlignment.Auto" />, and the <c>Auto</c> logo visibility depends on it. The
    /// operating system detection sets it through <see cref="IOperationSystemAware.SetOsType" />, not the
    /// application.
    /// </remarks>
    public OsType OsType => GetValue(OsTypeProperty);
    /// <summary>
    /// Version of the operating system the title bar runs on.
    /// </summary>
    /// <remarks>
    /// The operating system detection sets it through <see cref="IOperationSystemAware.SetOsVersion" />. This
    /// class does not use the value itself.
    /// </remarks>
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

    internal static readonly StyledProperty<bool> IsFullScreenSupportedProperty =
        AvaloniaProperty.Register<WindowTitleBar, bool>(nameof(IsFullScreenSupported), true);

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

    internal bool IsFullScreenSupported
    {
        get => GetValue(IsFullScreenSupportedProperty);
        set => SetValue(IsFullScreenSupportedProperty, value);
    }

    internal ICommand? CaptionButtonCommand
    {
        get => GetValue(CaptionButtonCommandProperty);
        set => SetValue(CaptionButtonCommandProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Occurs when the user double-clicks the title bar with the left mouse button.
    /// </summary>
    /// <remarks>
    /// It is raised when the second click is released, and only when the pointer capture was not lost in between.
    /// The bar does not change the window state itself; the host window switches between maximized and normal
    /// when it can be resized.
    /// </remarks>
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

    /// <summary>
    /// Calls the base member and then binds the bar to the nearest ancestor <see cref="Window" /> as its host.
    /// </summary>
    /// <remarks>
    /// An override must call the base member, otherwise the bar does not follow the state of its window.
    /// </remarks>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (this.FindLogicalAncestorOfType<Window>() is { } window)
        {
            AttachHost(window);
        }
    }

    /// <summary>
    /// Releases the host window before calling the base member.
    /// </summary>
    /// <remarks>
    /// An override must call the base member, otherwise the bar keeps its subscriptions on the window.
    /// </remarks>
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

    /// <summary>
    /// Calls the base member, then refreshes the effective title and logo state and the pseudo-classes when a
    /// related property changes.
    /// </summary>
    /// <remarks>
    /// This keeps the effective title and logo values, the window state pseudo-classes and
    /// <see cref="StdPseudoClass.Active" /> up to date. An override must call the base member for these to stay in
    /// sync.
    /// </remarks>
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
        IsEffectiveLogoVisible = isLogoVisibleByMode;
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

        return OsType != OsType.macOS;
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

    /// <summary>
    /// Calls the base member and records a left double-click for <see cref="MaximizeWindowRequested" /> without
    /// changing the window state.
    /// </summary>
    /// <remarks>
    /// The press of the second click is marked handled. An override must call the base member, otherwise a
    /// double-click does not raise the event.
    /// </remarks>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.ClickCount == 2 && e.Properties.IsLeftButtonPressed)
        {
            // The window state is not changed in PointerPressed: on Windows, switching from maximized to normal
            // makes DWM resize the window synchronously, while Avalonia has already captured the pointer
            // implicitly on the source during PointerPressed. The resize breaks the current pointer event chain:
            // PointerReleased and the following hover events are lost until the user clicks again. Deferring the
            // action to PointerReleased, as the maximize button does, avoids this.
            _doubleClickPending = true;
            e.Handled           = true;
        }
    }

    /// <summary>
    /// Calls the base member, then raises <see cref="MaximizeWindowRequested" /> when the release completes a
    /// recorded left double-click.
    /// </summary>
    /// <remarks>
    /// Any other release discards the record. An override must call the base member, otherwise the event is never
    /// raised.
    /// </remarks>
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

    /// <summary>
    /// Calls the base member and discards a recorded double-click.
    /// </summary>
    /// <remarks>
    /// An override must call the base member, otherwise a later release still raises
    /// <see cref="MaximizeWindowRequested" />.
    /// </remarks>
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

}
