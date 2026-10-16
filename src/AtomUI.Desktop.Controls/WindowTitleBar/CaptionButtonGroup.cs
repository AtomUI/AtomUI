using System.Windows.Input;
using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.Normal, StdPseudoClass.Minimized, StdPseudoClass.Maximized, StdPseudoClass.Fullscreen)]
[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
internal class CaptionButtonGroup : TemplatedControl, IOperationSystemAware
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsMinimizeCaptionButtonVisibleProperty =
        WindowTitleBar.IsMinimizeCaptionButtonVisibleProperty.AddOwner<CaptionButtonGroup>();

    public static readonly StyledProperty<bool> IsMaximizeCaptionButtonVisibleProperty =
        WindowTitleBar.IsMaximizeCaptionButtonVisibleProperty.AddOwner<CaptionButtonGroup>();

    public static readonly StyledProperty<bool> IsCloseCaptionButtonVisibleProperty =
        WindowTitleBar.IsCloseCaptionButtonVisibleProperty.AddOwner<CaptionButtonGroup>();

    public static readonly StyledProperty<bool> IsFullScreenCaptionButtonVisibleProperty =
        WindowTitleBar.IsFullScreenCaptionButtonVisibleProperty.AddOwner<CaptionButtonGroup>();

    public static readonly StyledProperty<bool> IsPinCaptionButtonVisibleProperty =
        WindowTitleBar.IsPinCaptionButtonVisibleProperty.AddOwner<CaptionButtonGroup>();

    public static readonly StyledProperty<bool> CanMinimizeProperty =
        WindowTitleBar.CanMinimizeProperty.AddOwner<CaptionButtonGroup>();

    public static readonly StyledProperty<bool> CanMaximizeProperty =
        WindowTitleBar.CanMaximizeProperty.AddOwner<CaptionButtonGroup>();

    public static readonly StyledProperty<bool> IsWindowActiveProperty =
        WindowTitleBar.IsWindowActiveProperty.AddOwner<CaptionButtonGroup>();

    public static readonly StyledProperty<OsType> OsTypeProperty =
        OperationSystemAwareControlProperty.OsTypeProperty.AddOwner<CaptionButtonGroup>();

    public static readonly StyledProperty<Version> OsVersionProperty =
        OperationSystemAwareControlProperty.OsVersionProperty.AddOwner<CaptionButtonGroup>();

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

    public bool CanMinimize
    {
        get => GetValue(CanMinimizeProperty);
        set => SetValue(CanMinimizeProperty, value);
    }

    public bool CanMaximize
    {
        get => GetValue(CanMaximizeProperty);
        set => SetValue(CanMaximizeProperty, value);
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

    internal static readonly StyledProperty<WindowState> HostWindowStateProperty =
        WindowTitleBar.HostWindowStateProperty.AddOwner<CaptionButtonGroup>();

    internal static readonly StyledProperty<bool> IsWindowTopmostProperty =
        WindowTitleBar.IsWindowTopmostProperty.AddOwner<CaptionButtonGroup>();

    internal static readonly StyledProperty<bool> IsPinCaptionButtonSupportedProperty =
        WindowTitleBar.IsPinCaptionButtonSupportedProperty.AddOwner<CaptionButtonGroup>();

    internal static readonly StyledProperty<ICommand?> CaptionButtonCommandProperty =
        WindowTitleBar.CaptionButtonCommandProperty.AddOwner<CaptionButtonGroup>();

    internal static readonly DirectProperty<CaptionButtonGroup, bool> IsWindowMaximizedProperty =
        AvaloniaProperty.RegisterDirect<CaptionButtonGroup, bool>(
            nameof(IsWindowMaximized),
            o => o.IsWindowMaximized);

    internal static readonly DirectProperty<CaptionButtonGroup, bool> IsWindowFullScreenProperty =
        AvaloniaProperty.RegisterDirect<CaptionButtonGroup, bool>(
            nameof(IsWindowFullScreen),
            o => o.IsWindowFullScreen);

    internal static readonly DirectProperty<CaptionButtonGroup, bool> IsWindowPinnedProperty =
        AvaloniaProperty.RegisterDirect<CaptionButtonGroup, bool>(
            nameof(IsWindowPinned),
            o => o.IsWindowPinned);

    internal static readonly DirectProperty<CaptionButtonGroup, bool> IsMinimizeButtonEffectivelyVisibleProperty =
        AvaloniaProperty.RegisterDirect<CaptionButtonGroup, bool>(
            nameof(IsMinimizeButtonEffectivelyVisible),
            o => o.IsMinimizeButtonEffectivelyVisible);

    internal static readonly DirectProperty<CaptionButtonGroup, bool> IsMaximizeButtonEffectivelyVisibleProperty =
        AvaloniaProperty.RegisterDirect<CaptionButtonGroup, bool>(
            nameof(IsMaximizeButtonEffectivelyVisible),
            o => o.IsMaximizeButtonEffectivelyVisible);

    internal static readonly DirectProperty<CaptionButtonGroup, bool> IsCloseButtonEffectivelyVisibleProperty =
        AvaloniaProperty.RegisterDirect<CaptionButtonGroup, bool>(
            nameof(IsCloseButtonEffectivelyVisible),
            o => o.IsCloseButtonEffectivelyVisible);

    internal static readonly DirectProperty<CaptionButtonGroup, bool> IsFullScreenButtonEffectivelyVisibleProperty =
        AvaloniaProperty.RegisterDirect<CaptionButtonGroup, bool>(
            nameof(IsFullScreenButtonEffectivelyVisible),
            o => o.IsFullScreenButtonEffectivelyVisible);

    internal static readonly DirectProperty<CaptionButtonGroup, bool> IsPinButtonEffectivelyVisibleProperty =
        AvaloniaProperty.RegisterDirect<CaptionButtonGroup, bool>(
            nameof(IsPinButtonEffectivelyVisible),
            o => o.IsPinButtonEffectivelyVisible);

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
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

    private bool _isWindowMaximized;

    internal bool IsWindowMaximized
    {
        get => _isWindowMaximized;
        private set => SetAndRaise(IsWindowMaximizedProperty, ref _isWindowMaximized, value);
    }

    private bool _isWindowFullScreen;

    internal bool IsWindowFullScreen
    {
        get => _isWindowFullScreen;
        private set => SetAndRaise(IsWindowFullScreenProperty, ref _isWindowFullScreen, value);
    }

    private bool _isWindowPinned;

    internal bool IsWindowPinned
    {
        get => _isWindowPinned;
        private set => SetAndRaise(IsWindowPinnedProperty, ref _isWindowPinned, value);
    }

    private bool _isMinimizeButtonEffectivelyVisible;

    internal bool IsMinimizeButtonEffectivelyVisible
    {
        get => _isMinimizeButtonEffectivelyVisible;
        private set => SetAndRaise(
            IsMinimizeButtonEffectivelyVisibleProperty,
            ref _isMinimizeButtonEffectivelyVisible,
            value);
    }

    private bool _isMaximizeButtonEffectivelyVisible;

    internal bool IsMaximizeButtonEffectivelyVisible
    {
        get => _isMaximizeButtonEffectivelyVisible;
        private set => SetAndRaise(
            IsMaximizeButtonEffectivelyVisibleProperty,
            ref _isMaximizeButtonEffectivelyVisible,
            value);
    }

    private bool _isCloseButtonEffectivelyVisible;

    internal bool IsCloseButtonEffectivelyVisible
    {
        get => _isCloseButtonEffectivelyVisible;
        private set => SetAndRaise(
            IsCloseButtonEffectivelyVisibleProperty,
            ref _isCloseButtonEffectivelyVisible,
            value);
    }

    private bool _isFullScreenButtonEffectivelyVisible;

    internal bool IsFullScreenButtonEffectivelyVisible
    {
        get => _isFullScreenButtonEffectivelyVisible;
        private set => SetAndRaise(
            IsFullScreenButtonEffectivelyVisibleProperty,
            ref _isFullScreenButtonEffectivelyVisible,
            value);
    }

    private bool _isPinButtonEffectivelyVisible;

    internal bool IsPinButtonEffectivelyVisible
    {
        get => _isPinButtonEffectivelyVisible;
        private set => SetAndRaise(
            IsPinButtonEffectivelyVisibleProperty,
            ref _isPinButtonEffectivelyVisible,
            value);
    }

    #endregion

    static CaptionButtonGroup()
    {
        IsMinimizeCaptionButtonVisibleProperty.Changed.AddClassHandler<CaptionButtonGroup>(OnCaptionInputChanged);
        IsMaximizeCaptionButtonVisibleProperty.Changed.AddClassHandler<CaptionButtonGroup>(OnCaptionInputChanged);
        IsCloseCaptionButtonVisibleProperty.Changed.AddClassHandler<CaptionButtonGroup>(OnCaptionInputChanged);
        IsFullScreenCaptionButtonVisibleProperty.Changed.AddClassHandler<CaptionButtonGroup>(OnCaptionInputChanged);
        IsPinCaptionButtonVisibleProperty.Changed.AddClassHandler<CaptionButtonGroup>(OnCaptionInputChanged);
        CanMinimizeProperty.Changed.AddClassHandler<CaptionButtonGroup>(OnCaptionInputChanged);
        CanMaximizeProperty.Changed.AddClassHandler<CaptionButtonGroup>(OnCaptionInputChanged);
        HostWindowStateProperty.Changed.AddClassHandler<CaptionButtonGroup>(OnCaptionInputChanged);
        IsWindowTopmostProperty.Changed.AddClassHandler<CaptionButtonGroup>(OnCaptionInputChanged);
        IsPinCaptionButtonSupportedProperty.Changed.AddClassHandler<CaptionButtonGroup>(OnCaptionInputChanged);
    }

    public CaptionButtonGroup()
    {
        this.ConfigureOsType();
        UpdateCaptionButtonState();
    }

    private static void OnCaptionInputChanged(CaptionButtonGroup group, AvaloniaPropertyChangedEventArgs args)
    {
        group.UpdateCaptionButtonState();
    }

    private void UpdateCaptionButtonState()
    {
        var isFullScreen = HostWindowState == WindowState.FullScreen;
        var isMaximized = HostWindowState == WindowState.Maximized;

        PseudoClasses.Set(StdPseudoClass.Minimized, HostWindowState == WindowState.Minimized);
        PseudoClasses.Set(StdPseudoClass.Normal, HostWindowState == WindowState.Normal);
        PseudoClasses.Set(StdPseudoClass.Maximized, isMaximized);
        PseudoClasses.Set(StdPseudoClass.Fullscreen, isFullScreen);

        IsWindowMaximized = isMaximized;
        IsWindowFullScreen = isFullScreen;
        IsWindowPinned = IsWindowTopmost;
        IsMinimizeButtonEffectivelyVisible = IsMinimizeCaptionButtonVisible && CanMinimize && !isFullScreen;
        IsMaximizeButtonEffectivelyVisible = IsMaximizeCaptionButtonVisible && CanMaximize && !isFullScreen;
        IsCloseButtonEffectivelyVisible = IsCloseCaptionButtonVisible;
        IsFullScreenButtonEffectivelyVisible = IsFullScreenCaptionButtonVisible && !isMaximized;
        IsPinButtonEffectivelyVisible = IsPinCaptionButtonVisible && IsPinCaptionButtonSupported;
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
