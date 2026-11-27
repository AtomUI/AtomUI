using AtomUI.Theme.Resources;
using System.Runtime.Versioning;
using System.Reactive.Disposables;
using System.Windows.Input;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
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
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Avalonia.Platform;

namespace AtomUI.Desktop.Controls;

using AvaloniaWindow = Avalonia.Controls.Window;

/// <summary>
/// Desktop window whose template draws an AtomUI frame: a default <see cref="WindowTitleBar" /> with
/// caption buttons, logo and add-on areas, the content area, and optional decorative layers behind the
/// window, the title bar and the content. Client-side decorations are always enabled on Windows, enabled
/// on Linux only when the windowing platform needs managed decorations, and disabled on macOS.
/// </summary>
[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
public partial class Window : AvaloniaWindow,
                              IOperationSystemAware,
                              IMediaBreakAwareControl
{
    private const double WindowsCsdMinimumHeightInTitleBars = 2;

    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Logo" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> LogoProperty =
        WindowTitleBar.LogoProperty.AddOwner<Window>();
    
    /// <summary>
    /// Identifies the <see cref="LogoTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LogoTemplateProperty =
        WindowTitleBar.LogoTemplateProperty.AddOwner<Window>();

    /// <summary>
    /// Identifies the <see cref="LogoVisibility" /> property.
    /// </summary>
    public static readonly StyledProperty<WindowTitleBarLogoVisibility> LogoVisibilityProperty =
        WindowTitleBar.LogoVisibilityProperty.AddOwner<Window>();

    /// <summary>
    /// Identifies the <see cref="TitleAlignment" /> property.
    /// </summary>
    public static readonly StyledProperty<WindowTitleBarTitleAlignment> TitleAlignmentProperty =
        WindowTitleBar.TitleAlignmentProperty.AddOwner<Window>();

    /// <summary>
    /// Identifies the <see cref="IsTitleVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTitleVisibleProperty =
        WindowTitleBar.IsTitleVisibleProperty.AddOwner<Window>();

    /// <summary>
    /// Identifies the <see cref="LeftAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        WindowTitleBar.LeftAddOnProperty.AddOwner<Window>();

    /// <summary>
    /// Identifies the <see cref="LeftAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        WindowTitleBar.LeftAddOnTemplateProperty.AddOwner<Window>();

    /// <summary>
    /// Identifies the <see cref="RightAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> RightAddOnProperty =
        WindowTitleBar.RightAddOnProperty.AddOwner<Window>();

    /// <summary>
    /// Identifies the <see cref="RightAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        WindowTitleBar.RightAddOnTemplateProperty.AddOwner<Window>();
    
    /// <summary>
    /// Identifies the <see cref="IsTitleBarVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTitleBarVisibleProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsTitleBarVisible), defaultValue: true);

    /// <summary>
    /// Identifies the <see cref="FullScreenTitleBarMode" /> property.
    /// </summary>
    public static readonly StyledProperty<FullScreenTitleBarMode> FullScreenTitleBarModeProperty =
        AvaloniaProperty.Register<Window, FullScreenTitleBarMode>(
            nameof(FullScreenTitleBarMode),
            FullScreenTitleBarMode.AutoHide);

    /// <summary>
    /// Identifies the <see cref="IsFullScreenTitleBarOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFullScreenTitleBarOpenProperty =
        AvaloniaProperty.Register<Window, bool>(
            nameof(IsFullScreenTitleBarOpen),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceIsFullScreenTitleBarOpen);

    /// <summary>
    /// Identifies the <see cref="FullScreenTitleBar" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> FullScreenTitleBarProperty =
        AvaloniaProperty.Register<Window, object?>(nameof(FullScreenTitleBar));

    /// <summary>
    /// Identifies the <see cref="FullScreenTitleBarTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> FullScreenTitleBarTemplateProperty =
        AvaloniaProperty.Register<Window, IDataTemplate?>(nameof(FullScreenTitleBarTemplate));

    /// <summary>
    /// Identifies the <see cref="FullScreenTitleBarMouseEnterDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> FullScreenTitleBarMouseEnterDelayProperty =
        AvaloniaProperty.Register<Window, int>(nameof(FullScreenTitleBarMouseEnterDelay), 150);

    /// <summary>
    /// Identifies the <see cref="FullScreenTitleBarMouseLeaveDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> FullScreenTitleBarMouseLeaveDelayProperty =
        AvaloniaProperty.Register<Window, int>(nameof(FullScreenTitleBarMouseLeaveDelay), 300);
    
    /// <summary>
    /// Identifies the <see cref="WindowFrameLayer" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> WindowFrameLayerProperty =
        AvaloniaProperty.Register<Window, object?>(nameof(WindowFrameLayer));
    
    /// <summary>
    /// Identifies the <see cref="WindowFrameLayerTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> WindowFrameLayerTemplateProperty =
        AvaloniaProperty.Register<Window, IDataTemplate?>(nameof(WindowFrameLayerTemplate));
    
    /// <summary>
    /// Identifies the <see cref="WindowFrameLayerOpacity" /> property.
    /// </summary>
    public static readonly StyledProperty<double> WindowFrameLayerOpacityProperty =
        AvaloniaProperty.Register<Window, double>(nameof(WindowFrameLayerOpacity), 1.0);
    
    /// <summary>
    /// Identifies the <see cref="ContentFrameLayer" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentFrameLayerProperty =
        AvaloniaProperty.Register<Window, object?>(nameof(ContentFrameLayer));
    
    /// <summary>
    /// Identifies the <see cref="ContentFrameLayerTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentFrameLayerTemplateProperty =
        AvaloniaProperty.Register<Window, IDataTemplate?>(nameof(ContentFrameLayerTemplate));
    
    /// <summary>
    /// Identifies the <see cref="ContentFrameLayerOpacity" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ContentFrameLayerOpacityProperty =
        AvaloniaProperty.Register<Window, double>(nameof(ContentFrameLayerOpacity), 1.0);
    
    /// <summary>
    /// Identifies the <see cref="ContentFrameBackground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ContentFrameBackgroundProperty =
        AvaloniaProperty.Register<Window, IBrush?>(nameof(ContentFrameBackground));
    
    /// <summary>
    /// Identifies the <see cref="TitleBarFrameLayer" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> TitleBarFrameLayerProperty =
        AvaloniaProperty.Register<Window, object?>(nameof(TitleBarFrameLayer));
    
    /// <summary>
    /// Identifies the <see cref="TitleBarFrameLayerTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> TitleBarFrameLayerTemplateProperty =
        AvaloniaProperty.Register<Window, IDataTemplate?>(nameof(TitleBarFrameLayerTemplate));
    
    /// <summary>
    /// Identifies the <see cref="TitleBarFrameLayerOpacity" /> property.
    /// </summary>
    public static readonly StyledProperty<double> TitleBarFrameLayerOpacityProperty =
        AvaloniaProperty.Register<Window, double>(nameof(TitleBarFrameLayerOpacity), 1.0);
    
    /// <summary>
    /// Identifies the <see cref="TitleBarFrameBackground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TitleBarFrameBackgroundProperty =
        AvaloniaProperty.Register<Window, IBrush?>(nameof(TitleBarFrameBackground));
    
    /// <summary>
    /// Identifies the <see cref="IsMinimizeCaptionButtonVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMinimizeCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsMinimizeCaptionButtonVisible), true);

    /// <summary>
    /// Identifies the <see cref="IsMaximizeCaptionButtonVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMaximizeCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsMaximizeCaptionButtonVisible), true);

    /// <summary>
    /// Identifies the <see cref="IsCloseCaptionButtonVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCloseCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsCloseCaptionButtonVisible), true);

    /// <summary>
    /// Identifies the <see cref="IsFullScreenCaptionButtonVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFullScreenCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsFullScreenCaptionButtonVisible));

    /// <summary>
    /// Identifies the <see cref="IsPinCaptionButtonVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPinCaptionButtonVisibleProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsPinCaptionButtonVisible));
    
    /// <summary>
    /// Identifies the <see cref="IsMoveEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMoveEnabledProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsMoveEnabled), defaultValue: true);
    
    /// <summary>
    /// Identifies the <see cref="MediaBreakPoint" /> property.
    /// </summary>
    public static readonly StyledProperty<MediaBreakPoint> MediaBreakPointProperty = 
        MediaBreakAwareControlProperty.MediaBreakPointProperty.AddOwner<Window>();
    
    /// <summary>
    /// Identifies the <see cref="OsType" /> property.
    /// </summary>
    public static readonly StyledProperty<OsType> OsTypeProperty =
        OperationSystemAwareControlProperty.OsTypeProperty.AddOwner<Window>();
    
    /// <summary>
    /// Identifies the <see cref="OsVersion" /> property.
    /// </summary>
    public static readonly StyledProperty<Version> OsVersionProperty =
        OperationSystemAwareControlProperty.OsVersionProperty.AddOwner<Window>();
    
    /// <summary>
    /// Whether the title bar is presented. When it is <c>false</c>, the default title bar, the
    /// <see cref="TitleBarFrameLayer" /> and the title bar parts of the drawn decorations are hidden, and
    /// with client-side decorations enabled the content grows into the space that the title bar would have
    /// occupied. The default is <c>true</c>.
    /// </summary>
    public bool IsTitleBarVisible
    {
        get => GetValue(IsTitleBarVisibleProperty);
        set => SetValue(IsTitleBarVisibleProperty, value);
    }

    /// <summary>
    /// How the title bar is presented while the window is full screen. Defaults to
    /// <see cref="Controls.FullScreenTitleBarMode.AutoHide" /> on every platform.
    /// </summary>
    public FullScreenTitleBarMode FullScreenTitleBarMode
    {
        get => GetValue(FullScreenTitleBarModeProperty);
        set => SetValue(FullScreenTitleBarModeProperty, value);
    }

    /// <summary>
    /// Whether the full-screen title bar is currently presented. Writing <c>true</c> or <c>false</c> opens or closes an
    /// <see cref="Controls.FullScreenTitleBarMode.AutoHide" /> title bar; in every other state the value is coerced
    /// (always open for Pinned and Docked, always closed when not full screen or Hidden).
    /// </summary>
    public bool IsFullScreenTitleBarOpen
    {
        get => GetValue(IsFullScreenTitleBarOpenProperty);
        set => SetValue(IsFullScreenTitleBarOpenProperty, value);
    }

    /// <summary>
    /// Custom content of the full-screen title bar. When this or <see cref="FullScreenTitleBarTemplate" /> is set it
    /// replaces the default title bar in full screen.
    /// </summary>
    [DependsOn(nameof(FullScreenTitleBarTemplate))]
    public object? FullScreenTitleBar
    {
        get => GetValue(FullScreenTitleBarProperty);
        set => SetValue(FullScreenTitleBarProperty, value);
    }

    /// <summary>
    /// Template used to present <see cref="FullScreenTitleBar" /> in the full-screen title bar. Setting only
    /// this counts as a custom full-screen title bar and replaces the default title bar in full screen.
    /// </summary>
    public IDataTemplate? FullScreenTitleBarTemplate
    {
        get => GetValue(FullScreenTitleBarTemplateProperty);
        set => SetValue(FullScreenTitleBarTemplateProperty, value);
    }

    /// <summary>
    /// Milliseconds the pointer rests in the top trigger band before an AutoHide title bar opens. Negative values act as 0.
    /// </summary>
    public int FullScreenTitleBarMouseEnterDelay
    {
        get => GetValue(FullScreenTitleBarMouseEnterDelayProperty);
        set => SetValue(FullScreenTitleBarMouseEnterDelayProperty, value);
    }

    /// <summary>
    /// Milliseconds between the last hold ending and an AutoHide title bar closing. Negative values act as 0.
    /// </summary>
    public int FullScreenTitleBarMouseLeaveDelay
    {
        get => GetValue(FullScreenTitleBarMouseLeaveDelayProperty);
        set => SetValue(FullScreenTitleBarMouseLeaveDelayProperty, value);
    }
    
    /// <summary>
    /// Logo shown in the default title bar; <see cref="LogoVisibility" /> decides whether it appears. When
    /// this and <see cref="LogoTemplate" /> are both <c>null</c>, the title bar shows the <c>Icon</c> of
    /// this window instead, then the explicit logo of the main window and last the <c>Icon</c> of the main
    /// window. Nothing is shown when all of them are absent.
    /// </summary>
    [DependsOn(nameof(LogoTemplate))]
    public object? Logo
    {
        get => GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }
    
    /// <summary>
    /// Template used to present <see cref="Logo" /> in the default title bar. Setting it alone counts as an
    /// explicit logo, so the icon fallback of <see cref="Logo" /> does not apply.
    /// </summary>
    public IDataTemplate? LogoTemplate
    {
        get => GetValue(LogoTemplateProperty);
        set => SetValue(LogoTemplateProperty, value);
    }

    /// <summary>
    /// When the default title bar shows the logo. The default is <see cref="WindowTitleBarLogoVisibility.Auto" />,
    /// which shows the logo when one is set, except on macOS when the title bar has no visible title.
    /// <see cref="WindowTitleBarLogoVisibility.Always" /> shows it whenever a logo or logo template is set
    /// and <see cref="WindowTitleBarLogoVisibility.Never" /> hides it.
    /// </summary>
    public WindowTitleBarLogoVisibility LogoVisibility
    {
        get => GetValue(LogoVisibilityProperty);
        set => SetValue(LogoVisibilityProperty, value);
    }

    /// <summary>
    /// Horizontal placement of the title in the default title bar. The default is
    /// <see cref="WindowTitleBarTitleAlignment.Auto" />, which follows the host operating system:
    /// <see cref="WindowTitleBarTitleAlignment.WindowCenter" /> on macOS and
    /// <see cref="WindowTitleBarTitleAlignment.Left" /> on Windows and Linux.
    /// </summary>
    public WindowTitleBarTitleAlignment TitleAlignment
    {
        get => GetValue(TitleAlignmentProperty);
        set => SetValue(TitleAlignmentProperty, value);
    }

    /// <summary>
    /// Whether the default title bar shows the title text. The default is <c>true</c>. Setting <c>false</c>
    /// hides only the text inside the title bar; the operating system still shows the window title in the
    /// taskbar and window switching.
    /// </summary>
    public bool IsTitleVisible
    {
        get => GetValue(IsTitleVisibleProperty);
        set => SetValue(IsTitleVisibleProperty, value);
    }

    /// <summary>
    /// Content of the left add-on area of the default title bar, meant for controls owned by the
    /// application such as buttons. It is forwarded to the title bar at template priority, so a value set
    /// directly on the title bar replaces it.
    /// </summary>
    [DependsOn(nameof(LeftAddOnTemplate))]
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }

    /// <summary>
    /// Template used to present <see cref="LeftAddOn" /> in the left add-on area of the default title bar.
    /// </summary>
    public IDataTemplate? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Content of the right add-on area of the default title bar, meant for controls owned by the
    /// application such as buttons. It is forwarded to the title bar at template priority, so a value set
    /// directly on the title bar replaces it.
    /// </summary>
    [DependsOn(nameof(RightAddOnTemplate))]
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }

    /// <summary>
    /// Template used to present <see cref="RightAddOn" /> in the right add-on area of the default title bar.
    /// </summary>
    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }
    
    /// <summary>
    /// Decorative content drawn inside the window frame behind everything else. It covers the whole window
    /// surface including the title bar and is clipped to the corner radius of the window. While it is set,
    /// the window does not cache its content as a bitmap.
    /// </summary>
    [DependsOn(nameof(WindowFrameLayerTemplate))]
    public object? WindowFrameLayer
    {
        get => GetValue(WindowFrameLayerProperty);
        set => SetValue(WindowFrameLayerProperty, value);
    }
    
    /// <summary>
    /// Template used to present <see cref="WindowFrameLayer" />.
    /// </summary>
    public object? WindowFrameLayerTemplate
    {
        get => GetValue(WindowFrameLayerTemplateProperty);
        set => SetValue(WindowFrameLayerTemplateProperty, value);
    }
    
    /// <summary>
    /// Opacity of <see cref="WindowFrameLayer" />. The default is 1, fully opaque. It does not affect the
    /// window background, the title bar or the content.
    /// </summary>
    public double WindowFrameLayerOpacity
    {
        get => GetValue(WindowFrameLayerOpacityProperty);
        set => SetValue(WindowFrameLayerOpacityProperty, value);
    }
    
    /// <summary>
    /// Decorative content drawn behind the window content. Its area is the content area below the title
    /// bar, minus the space that a Docked full-screen title bar reserves above the content, and it clips
    /// the layer.
    /// </summary>
    [DependsOn(nameof(ContentFrameLayerTemplate))]
    public object? ContentFrameLayer
    {
        get => GetValue(ContentFrameLayerProperty);
        set => SetValue(ContentFrameLayerProperty, value);
    }
    
    /// <summary>
    /// Template used to present <see cref="ContentFrameLayer" />.
    /// </summary>
    public object? ContentFrameLayerTemplate
    {
        get => GetValue(ContentFrameLayerTemplateProperty);
        set => SetValue(ContentFrameLayerTemplateProperty, value);
    }
    
    /// <summary>
    /// Opacity of the layer behind the window content. The default is 1, fully opaque. It applies to
    /// <see cref="ContentFrameLayer" /> and to <see cref="ContentFrameBackground" /> alike, because one
    /// presenter carries both; the window content itself is not affected.
    /// </summary>
    public double ContentFrameLayerOpacity
    {
        get => GetValue(ContentFrameLayerOpacityProperty);
        set => SetValue(ContentFrameLayerOpacityProperty, value);
    }
    
    /// <summary>
    /// Brush painted behind the window content, inside the area that <see cref="ContentFrameLayer" />
    /// occupies. The default is <c>null</c>, which leaves the background of the window visible there.
    /// </summary>
    public IBrush? ContentFrameBackground
    {
        get => GetValue(ContentFrameBackgroundProperty);
        set => SetValue(ContentFrameBackgroundProperty, value);
    }
    
    /// <summary>
    /// Decorative content drawn in the title bar row behind the default title bar, clipped to that row. It
    /// is shown only while <see cref="IsTitleBarVisible" /> is <c>true</c>.
    /// </summary>
    [DependsOn(nameof(TitleBarFrameLayerTemplate))]
    public object? TitleBarFrameLayer
    {
        get => GetValue(TitleBarFrameLayerProperty);
        set => SetValue(TitleBarFrameLayerProperty, value);
    }
    
    /// <summary>
    /// Template used to present <see cref="TitleBarFrameLayer" />.
    /// </summary>
    public object? TitleBarFrameLayerTemplate
    {
        get => GetValue(TitleBarFrameLayerTemplateProperty);
        set => SetValue(TitleBarFrameLayerTemplateProperty, value);
    }
    
    /// <summary>
    /// Opacity of <see cref="TitleBarFrameLayer" />. The default is 1, fully opaque. It does not affect the
    /// default title bar or the window content.
    /// </summary>
    public double TitleBarFrameLayerOpacity
    {
        get => GetValue(TitleBarFrameLayerOpacityProperty);
        set => SetValue(TitleBarFrameLayerOpacityProperty, value);
    }
    
    /// <summary>
    /// Background of the title bar area. It is projected onto the default title bar at template priority,
    /// so a value assigned directly on the title bar still wins, and the drawn decorations fill their title
    /// bar background with it. The default is <c>null</c>, which leaves the theme background of the title
    /// bar. On Linux with client-side decorations enabled, the default title bar stays transparent while it
    /// is in the title bar row, because the drawn decorations paint the background there.
    /// </summary>
    public IBrush? TitleBarFrameBackground
    {
        get => GetValue(TitleBarFrameBackgroundProperty);
        set => SetValue(TitleBarFrameBackgroundProperty, value);
    }
    
    /// <summary>
    /// Whether the default title bar shows the minimize caption button. The default is <c>true</c>. The
    /// window also forwards <c>CanMinimize</c> to the title bar, so this is a request rather than a
    /// guarantee.
    /// </summary>
    public bool IsMinimizeCaptionButtonVisible
    {
        get => GetValue(IsMinimizeCaptionButtonVisibleProperty);
        set => SetValue(IsMinimizeCaptionButtonVisibleProperty, value);
    }

    /// <summary>
    /// Whether the default title bar shows the maximize caption button. The default is <c>true</c>. The
    /// window also forwards <c>CanMaximize</c> to the title bar, so this is a request rather than a
    /// guarantee.
    /// </summary>
    public bool IsMaximizeCaptionButtonVisible
    {
        get => GetValue(IsMaximizeCaptionButtonVisibleProperty);
        set => SetValue(IsMaximizeCaptionButtonVisibleProperty, value);
    }

    /// <summary>
    /// Whether the default title bar shows the close caption button. The default is <c>true</c>.
    /// </summary>
    public bool IsCloseCaptionButtonVisible
    {
        get => GetValue(IsCloseCaptionButtonVisibleProperty);
        set => SetValue(IsCloseCaptionButtonVisibleProperty, value);
    }

    /// <summary>
    /// Whether the default title bar shows the full-screen caption button. The default is <c>false</c>.
    /// The button also needs the platform to support full screen, and it runs the same operation as
    /// <see cref="ToggleFullScreen" />.
    /// </summary>
    public bool IsFullScreenCaptionButtonVisible
    {
        get => GetValue(IsFullScreenCaptionButtonVisibleProperty);
        set => SetValue(IsFullScreenCaptionButtonVisibleProperty, value);
    }

    /// <summary>
    /// Whether the default title bar shows the pin caption button, which toggles <c>Topmost</c>. The
    /// default is <c>false</c>. The button also needs the platform to support pinning.
    /// </summary>
    public bool IsPinCaptionButtonVisible
    {
        get => GetValue(IsPinCaptionButtonVisibleProperty);
        set => SetValue(IsPinCaptionButtonVisibleProperty, value);
    }
    
    /// <summary>
    /// Whether dragging the default title bar moves the window. The default is <c>true</c>. Dragging is
    /// refused while the window is full screen whatever the value, and changing the value cancels a drag in
    /// progress.
    /// </summary>
    public bool IsMoveEnabled
    {
        get => GetValue(IsMoveEnabledProperty);
        set => SetValue(IsMoveEnabledProperty, value);
    }
    
    /// <summary>
    /// Responsive breakpoint that matches the window width. Applications only read it: the framework
    /// updates it from the media query indicator of the template and raises
    /// <see cref="MediaBreakPointChanged" />.
    /// </summary>
    public MediaBreakPoint MediaBreakPoint
    {
        get => GetValue(MediaBreakPointProperty);
        internal set => SetValue(MediaBreakPointProperty, value);
    }
    
    /// <summary>
    /// Operating system the window is styled for; the theme selects its template by it. The default is the
    /// running system, or <c>Unknown</c> on any other system. <c>IOperationSystemAware.SetOsType</c>
    /// changes it.
    /// </summary>
    public OsType OsType => GetValue(OsTypeProperty);
    /// <summary>
    /// Version of <see cref="OsType" />. The default is <c>Environment.OSVersion.Version</c>; together
    /// with <see cref="OsType" /> it decides whether the drawn top border of Windows 10 is used.
    /// <c>IOperationSystemAware.SetOsVersion</c> changes it.
    /// </summary>
    public Version OsVersion => GetValue(OsVersionProperty);
    
    #endregion
    
    #region 公共事件定义
    
    /// <summary>
    /// Raised after the first layout pass with the breakpoint measured then, and afterwards whenever the
    /// breakpoint of the window changes. <see cref="MediaBreakPoint" /> already holds the new value when
    /// handlers run.
    /// </summary>
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

    internal static readonly DirectProperty<Window, bool> IsLegacyWindowsFrameBorderVisibleProperty =
        AvaloniaProperty.RegisterDirect<Window, bool>(
            nameof(IsLegacyWindowsFrameBorderVisible),
            o => o.IsLegacyWindowsFrameBorderVisible);

    internal static readonly DirectProperty<Window, double> LegacyWindowsFrameBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<Window, double>(
            nameof(LegacyWindowsFrameBorderThickness),
            o => o.LegacyWindowsFrameBorderThickness);

    internal static readonly DirectProperty<Window, object?> EffectiveLogoProperty =
        AvaloniaProperty.RegisterDirect<Window, object?>(
            nameof(EffectiveLogo),
            o => o.EffectiveLogo);

    internal static readonly DirectProperty<Window, IDataTemplate?> EffectiveLogoTemplateProperty =
        AvaloniaProperty.RegisterDirect<Window, IDataTemplate?>(
            nameof(EffectiveLogoTemplate),
            o => o.EffectiveLogoTemplate);

    internal static readonly DirectProperty<Window, WindowTitleBar?> NormalSlotTitleBarProperty =
        AvaloniaProperty.RegisterDirect<Window, WindowTitleBar?>(
            nameof(NormalSlotTitleBar),
            o => o.NormalSlotTitleBar);

    internal static readonly DirectProperty<Window, object?> FullScreenSlotContentProperty =
        AvaloniaProperty.RegisterDirect<Window, object?>(
            nameof(FullScreenSlotContent),
            o => o.FullScreenSlotContent);

    internal static readonly DirectProperty<Window, IDataTemplate?> FullScreenSlotContentTemplateProperty =
        AvaloniaProperty.RegisterDirect<Window, IDataTemplate?>(
            nameof(FullScreenSlotContentTemplate),
            o => o.FullScreenSlotContentTemplate);

    internal static readonly DirectProperty<Window, FullScreenTitleBarMode> EffectiveFullScreenTitleBarModeProperty =
        AvaloniaProperty.RegisterDirect<Window, FullScreenTitleBarMode>(
            nameof(EffectiveFullScreenTitleBarMode),
            o => o.EffectiveFullScreenTitleBarMode);

    internal static readonly DirectProperty<Window, Thickness> FullScreenTitleBarSurfaceMarginProperty =
        AvaloniaProperty.RegisterDirect<Window, Thickness>(
            nameof(FullScreenTitleBarSurfaceMargin),
            o => o.FullScreenTitleBarSurfaceMargin);

    internal static readonly DirectProperty<Window, Thickness> FullScreenTitleBarContentPaddingProperty =
        AvaloniaProperty.RegisterDirect<Window, Thickness>(
            nameof(FullScreenTitleBarContentPadding),
            o => o.FullScreenTitleBarContentPadding);

    internal static readonly DirectProperty<Window, Thickness> FullScreenContentInsetProperty =
        AvaloniaProperty.RegisterDirect<Window, Thickness>(
            nameof(FullScreenContentInset),
            o => o.FullScreenContentInset);

    internal static readonly DirectProperty<Window, bool> IsPinCaptionButtonSupportedProperty =
        AvaloniaProperty.RegisterDirect<Window, bool>(
            nameof(IsPinCaptionButtonSupported),
            o => o.IsPinCaptionButtonSupported);

    internal static readonly DirectProperty<Window, bool> IsFullScreenSupportedProperty =
        AvaloniaProperty.RegisterDirect<Window, bool>(
            nameof(IsFullScreenSupported),
            o => o.IsFullScreenSupported);

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

    private bool _isLegacyWindowsFrameBorderVisible;

    /// <summary>
    /// Whether the top edge of the client area needs a drawn 1px border under Windows 10 client-side
    /// decorations: Avalonia extends the client area to the very top of the window there, which hides the
    /// top border of the DWM. On Windows 11 the visible DWM border covers it, so nothing is drawn.
    /// </summary>
    internal bool IsLegacyWindowsFrameBorderVisible
    {
        get => _isLegacyWindowsFrameBorderVisible;
        private set => SetAndRaise(
            IsLegacyWindowsFrameBorderVisibleProperty,
            ref _isLegacyWindowsFrameBorderVisible,
            value);
    }

    private double _legacyWindowsFrameBorderThickness = 1;

    /// <summary>
    /// Thickness of the drawn top border in DIPs. The native DWM border is one physical pixel, so the value
    /// is converted by the render scaling to keep it at one pixel on high-DPI displays.
    /// </summary>
    internal double LegacyWindowsFrameBorderThickness
    {
        get => _legacyWindowsFrameBorderThickness;
        private set => SetAndRaise(
            LegacyWindowsFrameBorderThicknessProperty,
            ref _legacyWindowsFrameBorderThickness,
            value);
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

    private WindowTitleBar? _normalSlotTitleBar;

    /// <summary>
    /// The default title bar while it is presented by the normal title-bar slot of the template; <c>null</c> while the
    /// full-screen title bar hosts it.
    /// </summary>
    internal WindowTitleBar? NormalSlotTitleBar
    {
        get => _normalSlotTitleBar;
        private set => SetAndRaise(NormalSlotTitleBarProperty, ref _normalSlotTitleBar, value);
    }

    private object? _fullScreenSlotContent;

    internal object? FullScreenSlotContent
    {
        get => _fullScreenSlotContent;
        private set => SetAndRaise(FullScreenSlotContentProperty, ref _fullScreenSlotContent, value);
    }

    private IDataTemplate? _fullScreenSlotContentTemplate;

    internal IDataTemplate? FullScreenSlotContentTemplate
    {
        get => _fullScreenSlotContentTemplate;
        private set => SetAndRaise(FullScreenSlotContentTemplateProperty, ref _fullScreenSlotContentTemplate, value);
    }

    private FullScreenTitleBarMode _effectiveFullScreenTitleBarMode = FullScreenTitleBarMode.Hidden;

    /// <summary>
    /// The presented full-screen title bar mode; <see cref="Controls.FullScreenTitleBarMode.Hidden" /> whenever no
    /// managed full-screen title bar is presented.
    /// </summary>
    internal FullScreenTitleBarMode EffectiveFullScreenTitleBarMode
    {
        get => _effectiveFullScreenTitleBarMode;
        private set => SetAndRaise(
            EffectiveFullScreenTitleBarModeProperty,
            ref _effectiveFullScreenTitleBarMode,
            value);
    }

    private Thickness _fullScreenTitleBarSurfaceMargin;

    internal Thickness FullScreenTitleBarSurfaceMargin
    {
        get => _fullScreenTitleBarSurfaceMargin;
        private set => SetAndRaise(
            FullScreenTitleBarSurfaceMarginProperty,
            ref _fullScreenTitleBarSurfaceMargin,
            value);
    }

    private Thickness _fullScreenTitleBarContentPadding;

    internal Thickness FullScreenTitleBarContentPadding
    {
        get => _fullScreenTitleBarContentPadding;
        private set => SetAndRaise(
            FullScreenTitleBarContentPaddingProperty,
            ref _fullScreenTitleBarContentPadding,
            value);
    }

    private Thickness _fullScreenContentInset;

    /// <summary>
    /// The top space a Docked full-screen title bar reserves above the window content.
    /// </summary>
    internal Thickness FullScreenContentInset
    {
        get => _fullScreenContentInset;
        private set => SetAndRaise(FullScreenContentInsetProperty, ref _fullScreenContentInset, value);
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

    private bool _isFullScreenSupported = true;

    /// <summary>
    /// Whether the platform can take this window into full screen. A full-screen request the platform cannot honour
    /// is rejected and the window state stays unchanged.
    /// </summary>
    internal bool IsFullScreenSupported
    {
        get => _isFullScreenSupported;
        private set => SetAndRaise(IsFullScreenSupportedProperty, ref _isFullScreenSupported, value);
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
    /// This is separate from <see cref="FrameShadowThickness" />, which only
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
    
    /// <summary>
    /// Selects the control theme of <see cref="Window" />, so that a derived window is styled by the AtomUI
    /// window theme unless it overrides this member.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(Window);
    private protected bool CloseByClickCloseCaptionButton;
    private WindowTitleBar? _moveDragTitleBar;
    private Point? _lastMousePressedPoint;
    private PointerPressedEventArgs? _lastMousePressedEventArgs;
    private readonly IWindowChromeManager? _platformChromeManager;
    private readonly WindowCaptionButtonCommand _captionButtonCommand;
    private WindowResizer? _windowResizer;
    private Border? _transparencyFallbackBorder;
    private MediaBreakPointIndicator? _mediaBreakPointIndicator;
    private CompositeDisposable? _defaultTitleBarBindings;
    private IDisposable? _titleBarFrameBackgroundProjection;
    private Window? _mainWindowLogoFallbackSource;
    private CompositeDisposable? _mainWindowLogoFallbackLease;
    private bool _isOpened;
    private int _drawnChromeOverlaySuppressionCount;
    private ThemeContextLease? _themeContextLease;
    private WindowState _windowStateBeforeFullScreen = WindowState.Normal;

    // Cache of the inputs of the last ConfigureMacOsWindow pass on macOS. It lets a live resize with unchanged
    // inputs return early and so avoids repeated P/Invoke calls.
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
        Avalonia.Controls.Primitives.Popup.IsOpenProperty.Changed
                .AddClassHandler<Avalonia.Controls.Primitives.Popup>(HandleAnyPopupIsOpenChanged);
    }

    public Window()
    {
        _captionButtonCommand = new WindowCaptionButtonCommand(this);
        ConfigureCsdStatus();
        UpdateLegacyWindowsFrameBorderVisibility();
        ScalingChanged += HandleWindowScalingChanged;
        _platformChromeManager = WindowChromeManager.Attach(this);
        UpdateCaptionButtonCapabilities();
        // Before the first layout: Wayland would otherwise lay out and render at scale 1 until the compositor
        // reports the preferred scale after mapping, then re-lay out the whole window.
        var scalingStatus = WaylandInitialScaling.Apply(PlatformImpl, out var scalingReason);
        if (scalingStatus != WaylandInitialScalingStatus.NotApplicable)
        {
            var level = scalingStatus is WaylandInitialScalingStatus.Incompatible or WaylandInitialScalingStatus.Failed
                ? LogEventLevel.Warning
                : LogEventLevel.Debug;
            Logger.TryGet(level, AtomUILogArea.System)?.Log(
                typeof(Window), "Wayland initial scaling: {Status}. {Reason}", scalingStatus, scalingReason);
        }
    }

    /// <summary>
    /// Enters full screen, or leaves it and restores the window state from before full screen. Shares the window
    /// operation and its capability check with the full-screen caption button, so it does nothing while maximized.
    /// </summary>
    public void ToggleFullScreen()
    {
        ExecuteCaptionButtonAction(CaptionButtonAction.ToggleFullScreen);
    }

    internal IDisposable SuppressDrawnChromeOverlay()
    {
        _drawnChromeOverlaySuppressionCount++;
        IsDrawnChromeOverlayVisible = false;
        UpdateFullScreenTitleBarSuppression();
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
            UpdateFullScreenTitleBarSuppression();
        }
    }

    /// <summary>
    /// Shows a non-modal window. Before the native window is shown, it adopts the theme context resolved
    /// from the window itself, primes the window background and the transparency fallback with the theme's
    /// default window background until the theme is applied, lets the platform chrome manager prepare its
    /// initial state, and applies the Windows client-side decoration frame theme. When showing fails, the
    /// theme context adopted here is released again.
    /// </summary>
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

    /// <summary>
    /// Shows a non-modal window owned by <paramref name="owner" />, with the same background, frame and
    /// initial state preparation as <see cref="Show()" />. The window adopts the theme context of the owner
    /// when the owner has one and the window does not follow it already. This member hides the base member:
    /// a caller that holds the window as <c>Avalonia.Controls.Window</c> gets the base implementation
    /// without that preparation.
    /// </summary>
    /// <param name="owner">
    /// The window that owns this window and provides its theme context.
    /// </param>
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

    /// <summary>
    /// Shows the window modally, owned by <paramref name="owner" />, with the same preparation as
    /// <c>Show(owner)</c>. This member hides the base member: a caller that holds the window as
    /// <c>Avalonia.Controls.Window</c> gets the base implementation without that preparation.
    /// </summary>
    /// <param name="owner">
    /// The window that owns this window and provides its theme context.
    /// </param>
    /// <returns>
    /// A task that completes when the window is closed.
    /// </returns>
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

    /// <summary>
    /// Shows the window modally, owned by <paramref name="owner" />, with the same preparation as
    /// <c>Show(owner)</c>. This member hides the base member: a caller that holds the window as
    /// <c>Avalonia.Controls.Window</c> gets the base implementation without that preparation.
    /// </summary>
    /// <param name="owner">
    /// The window that owns this window and provides its theme context.
    /// </param>
    /// <returns>
    /// A task that completes when the window is closed and yields the dialog result.
    /// </returns>
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
        var background = new SolidColorBrush(ControlTokenAccessor.Capture<Window>(this)
            .GetOwn<Color, WindowTokenKind>(WindowTokenKind.DefaultBackground));

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
        _windowResizer.SetCurrentValue(MarginProperty, new Thickness(
            Math.Max(0, shadow.Left - gripThickness.Left),
            Math.Max(0, shadow.Top - gripThickness.Top),
            Math.Max(0, shadow.Right - gripThickness.Right),
            Math.Max(0, shadow.Bottom - gripThickness.Bottom)));
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
    
    /// <summary>
    /// Locates the template parts and connects them to the window: the transparency fallback border, the
    /// content cache parts, the chrome layer, the full-screen title bar host, the default title bar, the
    /// media breakpoint indicator and the resizer. It also enforces the minimum height of Windows
    /// client-side decorations. Derived classes call the base member to keep the window working.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _transparencyFallbackBorder = null;
        base.OnApplyTemplate(e);
        _transparencyFallbackBorder = e.NameScope.Find<Border>("PART_TransparencyFallback");
        AttachContentCacheParts(e.NameScope);
        var fullScreenTitleBarHost = e.NameScope.Find<FullScreenTitleBarHost>("PART_FullScreenTitleBarHost");
        var visualLayerManager     = e.NameScope.Find<VisualLayerManager>("PART_VisualLayerManager");
        if (visualLayerManager is not null)
        {
            LayerLadder.EnsureContentDecorationOrder(visualLayerManager);
        }

        AttachWindowChromeLayer(visualLayerManager, fullScreenTitleBarHost);
        AttachFullScreenTitleBarHost(fullScreenTitleBarHost);

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
        UpdateFullScreenTitleBar();
    }

    private void DetachTitleBar(WindowTitleBar titleBar)
    {
        _defaultTitleBarBindings?.Dispose();
        _defaultTitleBarBindings = null;
        _titleBarFrameBackgroundProjection?.Dispose();
        _titleBarFrameBackgroundProjection = null;
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
        // While the full-screen title bar hosts the default title bar, its size describes the full-screen surface,
        // not the platform title bar, and must not drive the title-bar height hint.
        if (ReferenceEquals(FullScreenSlotContent, sender))
        {
            return;
        }

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
        var margin = CalculateEffectiveContentFrameMargin(
            WindowDecorationMargin,
            IsCsdEnabled,
            IsTitleBarVisible,
            IsCsdEnabled && !IsTitleBarVisible ? this.GetDrawnDecorationsTitleBarHeight() : 0);
        // A Docked full-screen title bar reserves its space above the content in the CSD content root.
        EffectiveContentFrameMargin = new Thickness(
            margin.Left,
            margin.Top + FullScreenContentInset.Top,
            margin.Right,
            margin.Bottom);
    }

    internal static bool ShouldDrawLegacyWindowsFrameBorder(
        OsType osType,
        Version osVersion,
        bool isCsdEnabled,
        bool extendClientAreaToDecorations,
        WindowDecorations decorations,
        WindowState windowState)
    {
        // With an extended client area, the DWM of Windows 10 hands the top edge of the window to the client area,
        // so a visible border remains on the left, right and bottom sides only.
        // The DWM of Windows 11 draws a 1px visible border inside the window, so the client area needs no compensation.
        return osType == OsType.Windows &&
               isCsdEnabled &&
               extendClientAreaToDecorations &&
               decorations == WindowDecorations.Full &&
               windowState == WindowState.Normal &&
               osVersion.Major >= 10 &&
               osVersion.Build < 22000;
    }

    private void UpdateLegacyWindowsFrameBorderVisibility()
    {
        IsLegacyWindowsFrameBorderVisible = ShouldDrawLegacyWindowsFrameBorder(
            OsType,
            OsVersion,
            IsCsdEnabled,
            ExtendClientAreaToDecorationsHint,
            WindowDecorations,
            WindowState);
        UpdateLegacyWindowsFrameBorderThickness();
    }

    private void UpdateLegacyWindowsFrameBorderThickness()
    {
        var scaling = RenderScaling;
        LegacyWindowsFrameBorderThickness = 1 / (double.IsFinite(scaling) && scaling > 0 ? scaling : 1);
    }

    private void HandleWindowScalingChanged(object? sender, EventArgs e)
    {
        UpdateLegacyWindowsFrameBorderThickness();
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
                WindowTitleBar.IsFullScreenSupportedProperty,
                this.GetObservable(IsFullScreenSupportedProperty)));
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

    /// <summary>
    /// Connects a newly created default title bar to this window. The title, logo, logo visibility, title
    /// alignment and title visibility are forwarded to it; the left and right add-ons are forwarded at
    /// template priority, so a value set directly on the title bar in an override replaces them; and
    /// <see cref="TitleBarFrameBackground" /> is projected onto it. It is called for each title bar that
    /// <see cref="NotifyCreateTitleBar" /> supplies when the template is applied. The base member first
    /// discards the forwarding it created for the previous title bar, so an override that wants the default
    /// forwarding calls it.
    /// </summary>
    /// <param name="titleBar">
    /// The title bar that replaces the previous one.
    /// </param>
    protected virtual void NotifyConfigureTitleBar(WindowTitleBar titleBar)
    {
        _defaultTitleBarBindings?.Dispose();
        _titleBarFrameBackgroundProjection?.Dispose();
        _titleBarFrameBackgroundProjection = null;
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
                BindingPriority.Template),
            // TitleBarFrameBackground cannot live in the ControlTheme: Window creates the default title bar in
            // OnApplyTemplate, and a null value must fall back to the ColorBgContainer DynamicResource of the
            // ControlTheme. Template priority overrides the ControlTheme while a LocalValue set by the user keeps
            // its priority.
            this.GetObservable(TitleBarFrameBackgroundProperty).Subscribe(
                brush => ProjectTitleBarFrameBackground(titleBar, brush)),
            this.GetObservable(IsCsdEnabledProperty).Subscribe(
                _ => ProjectTitleBarFrameBackground(titleBar, TitleBarFrameBackground)),
            // When the default title bar migrates between the normal slot and the full-screen slot, the painter of
            // its background changes with it.
            this.GetObservable(NormalSlotTitleBarProperty).Subscribe(
                _ => ProjectTitleBarFrameBackground(titleBar, TitleBarFrameBackground))
        };
    }

    private void ProjectTitleBarFrameBackground(WindowTitleBar titleBar, IBrush? brush)
    {
        _titleBarFrameBackgroundProjection?.Dispose();

        // Under Linux client-side decorations the title bar background is painted, clipped to the frame, by the
        // WindowTitleBarShadowBackground of the drawn decorations overlay, so the default title bar must stay
        // transparent; an opaque background would cover that layer. That layer serves the normal slot only: in
        // full screen the title bar moves into the full-screen title bar host and the drawn decorations no
        // longer paint its background, so the title bar must paint it itself, otherwise the full-screen title
        // bar shows the content beneath it. On the other platforms the default title bar paints it directly.
        if (OsType == OsType.Linux && IsCsdEnabled && ReferenceEquals(NormalSlotTitleBar, titleBar))
        {
            _titleBarFrameBackgroundProjection = titleBar.SetValue(
                WindowTitleBar.BackgroundProperty,
                Brushes.Transparent,
                BindingPriority.Template);
            return;
        }

        _titleBarFrameBackgroundProjection = brush is null
            ? null
            : titleBar.SetValue(
                WindowTitleBar.BackgroundProperty,
                brush,
                BindingPriority.Template);
    }

    private bool CanExecuteCaptionButtonAction(CaptionButtonAction action)
    {
        return action switch
        {
            CaptionButtonAction.Minimize => CanMinimize && WindowState != WindowState.FullScreen,
            CaptionButtonAction.ToggleMaximize => CanMaximize && WindowState != WindowState.FullScreen,
            CaptionButtonAction.ToggleFullScreen => WindowState == WindowState.FullScreen ||
                                                    (WindowState != WindowState.Maximized && IsFullScreenSupported),
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
        IsFullScreenSupported       = _platformChromeManager?.SupportsFullScreen != false;
    }

    /// <summary>
    /// Creates the default title bar of the window. It is called when the template is applied and replaces
    /// the title bar presented so far. The base member returns a new <see cref="WindowTitleBar" /> named
    /// <c>PART_TitleBar</c>.
    /// </summary>
    /// <param name="oldTitleBar">
    /// The title bar presented so far, already detached, or <c>null</c>.
    /// </param>
    /// <returns>
    /// The title bar to present, or <c>null</c> to present no default title bar.
    /// </returns>
    protected virtual WindowTitleBar? NotifyCreateTitleBar(WindowTitleBar? oldTitleBar)
    {
        return new WindowTitleBar
        {
            Name = "PART_TitleBar"
        };
    }
    
    /// <summary>
    /// Returns the main window of the application when it is an AtomUI <see cref="Window" />, otherwise
    /// <c>null</c>.
    /// </summary>
    /// <returns>
    /// The main window, or <c>null</c> when the application has no classic desktop lifetime or its main
    /// window is of another type.
    /// </returns>
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
        // In full screen AppKit hosts the standard buttons in its own title strip; the full-screen title bar owns
        // NativeChromeInsets and the button layout for the whole full-screen session.
        if (WindowState == WindowState.FullScreen)
        {
            StopMacOsWindowButtonObserver();
            _macOsCacheValid = false;
            return;
        }

        if (!ExtendClientAreaToDecorationsHint)
        {
            StopMacOsWindowButtonObserver();
            NativeChromeInsets = default;
            _macOsCachedButtonX = null;
            _macOsCachedButtonY = null;
            _macOsCacheValid   = false;
            return;
        }

        // AppKit hides the standard buttons temporarily while the screen is recorded or the window is shared.
        // While they are hidden the button positions are not written back; the restore notification runs a full
        // layout again later, so that the sharing controls of AppKit are not overwritten.
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

        // During a live resize every input except ClientSize stays unchanged, and even when the height changes, the
        // autoresizing of AppKit keeps the buttons at the same visual position relative to the title bar. When all
        // four inputs equal the previous ones, the GetFrame short circuit of the underlying
        // SetStandardWindowButtonsLayout would hit as well, so return here early and save one P/Invoke round trip.
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

    /// <summary>
    /// Calls the base member, then refreshes everything that needs the native window: caption button
    /// capabilities, the Windows client-side decoration frame theme, the frame geometry, the content frame
    /// margin and the effective logo. On macOS it also queues the layout of the standard window buttons and
    /// starts observing them. The media breakpoint is published after the first layout pass. Derived classes
    /// call the base member to keep this behavior.
    /// </summary>
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

    /// <summary>
    /// Releases what the window attached while it was open: the default title bar with its bindings, the
    /// full-screen title bar, the observer of the macOS standard window buttons and the theme context adopted
    /// when the window was shown. It runs before the <c>Closed</c> event is raised. Derived classes call the
    /// base member to keep this cleanup.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        _isOpened = false;
        ScalingChanged -= HandleWindowScalingChanged;
        _transparencyFallbackBorder = null;
        StopMacOsWindowButtonObserver();
        if (_titleBar != null)
        {
            DetachTitleBar(_titleBar);
        }
        DetachFullScreenTitleBar();
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

    // Resolves the effective logo as a pure function: an explicit Logo/LogoTemplate wins; when neither is set, it
    // falls back in turn to the Icon of the window, then to the explicit Logo/LogoTemplate of the main window, then
    // to the Icon of the main window.
    // The result only feeds EffectiveLogo/EffectiveLogoTemplate for rendering and is never written back to the
    // public properties, so that state such as a template injected by the framework does not linger after the
    // application sets Logo at run time.
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

    /// <summary>
    /// Calls the base member, then queues a new layout of the macOS standard window buttons, because their
    /// target position depends on the window size.
    /// </summary>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (OperatingSystem.IsMacOS())
        {
            QueueMacOsWindowConfiguration();
        }
    }

    /// <summary>
    /// Calls the base member, then keeps the state derived from the properties in step. A full-screen state
    /// that the platform cannot honour is rejected. Later changes to the transparency fallback, the window
    /// state, the size hints, the client-side decoration status, the title bar settings, the logo and the
    /// theme variant update the caption buttons, the macOS button layout, the Windows frame theme and
    /// border, the minimum height, the content frame margin, the move drag and the effective logo, and every
    /// change is forwarded to the platform chrome manager and the full-screen title bar. Derived classes call
    /// the base member to keep this.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty &&
            change.GetNewValue<WindowState>() == WindowState.FullScreen &&
            !IsFullScreenSupported)
        {
            // The platform cannot take this window full screen, so the request is rejected and the state reports what
            // the window actually is. Nothing else reacts to the rejected value.
            SetCurrentValue(WindowStateProperty, change.GetOldValue<WindowState>());
            return;
        }
        if (change.Property == OwnerProperty)
        {
            UpdateCaptionButtonCapabilities();
            if (WindowState == WindowState.FullScreen && !IsFullScreenSupported)
            {
                // A full-screen state requested before the window became owned cannot be honoured either.
                SetCurrentValue(WindowStateProperty, WindowState.Normal);
            }
        }
        if (change.Property == TransparencyBackgroundFallbackProperty &&
            ActualTransparencyLevel == WindowTransparencyLevel.None &&
            _transparencyFallbackBorder is not null)
        {
            // TopLevel writes this part's Background as a local value when applying the template
            // or changing transparency, overriding ControlTheme setters and TemplateBinding.
            // Mirror later fallback changes here; TopLevel still owns transparency transitions.
            _transparencyFallbackBorder.SetCurrentValue(BackgroundProperty, TransparencyBackgroundFallback);
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
            change.Property == IsPinCaptionButtonSupportedProperty ||
            change.Property == IsFullScreenSupportedProperty)
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
                // A change of WindowState, Title or WindowDecorations can make AppKit reset the standard button
                // frames. Even when the layout inputs of AtomUI are unchanged, the native buttons may have moved away
                // from their target position, so the cache must be invalidated before the layout is published again.
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
        if (change.Property == WindowStateProperty ||
            change.Property == WindowDecorationsProperty ||
            change.Property == ExtendClientAreaToDecorationsHintProperty ||
            change.Property == IsCsdEnabledProperty ||
            change.Property == OsTypeProperty ||
            change.Property == OsVersionProperty)
        {
            UpdateLegacyWindowsFrameBorderVisibility();
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
        if (change.Property == LogoProperty ||
            change.Property == LogoTemplateProperty ||
            change.Property == IconProperty)
        {
            UpdateEffectiveLogo();
        }
        HandleFullScreenTitleBarPropertyChanged(change);
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
