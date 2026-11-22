using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The full-screen title bar surface of <see cref="Window"/>. It presents either the window's own default title bar
/// instance or the application's custom full-screen content, and reports its measured height. Mode, open state and
/// position are projected by the window; the host never resolves them itself.
/// </summary>
[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
[TemplatePart(SurfacePart, typeof(Border))]
[PseudoClasses(StdPseudoClass.Open, DockedPseudoClass)]
internal class FullScreenTitleBarHost : ContentControl, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FullScreenTitleBarHost>();

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<FullScreenTitleBarMode> ModeProperty =
        AvaloniaProperty.Register<FullScreenTitleBarHost, FullScreenTitleBarMode>(nameof(Mode));

    internal static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<FullScreenTitleBarHost, bool>(nameof(IsOpen));

    internal static readonly StyledProperty<Thickness> SurfaceMarginProperty =
        AvaloniaProperty.Register<FullScreenTitleBarHost, Thickness>(nameof(SurfaceMargin));

    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<FullScreenTitleBarHost>();

    internal static readonly DirectProperty<FullScreenTitleBarHost, double> SurfaceHeightProperty =
        AvaloniaProperty.RegisterDirect<FullScreenTitleBarHost, double>(
            nameof(SurfaceHeight),
            o => o.SurfaceHeight);

    internal static readonly DirectProperty<FullScreenTitleBarHost, bool> IsSurfaceVisibleProperty =
        AvaloniaProperty.RegisterDirect<FullScreenTitleBarHost, bool>(
            nameof(IsSurfaceVisible),
            o => o.IsSurfaceVisible);

    internal FullScreenTitleBarMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    internal bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    internal Thickness SurfaceMargin
    {
        get => GetValue(SurfaceMarginProperty);
        set => SetValue(SurfaceMarginProperty, value);
    }

    internal TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    private double _surfaceHeight;

    /// <summary>
    /// The measured height of the surface, independent of whether the surface is currently shown.
    /// </summary>
    internal double SurfaceHeight
    {
        get => _surfaceHeight;
        private set => SetAndRaise(SurfaceHeightProperty, ref _surfaceHeight, value);
    }

    private bool _isSurfaceVisible;

    /// <summary>
    /// Whether the surface participates in rendering, layout of its content and keyboard navigation. It stays true
    /// while the closing motion runs and turns false once the surface is fully closed.
    /// </summary>
    internal bool IsSurfaceVisible
    {
        get => _isSurfaceVisible;
        private set => SetAndRaise(IsSurfaceVisibleProperty, ref _isSurfaceVisible, value);
    }

    #endregion

    #region 内部协作 API

    /// <summary>Raised when Escape is pressed while keyboard focus is inside the surface.</summary>
    internal event EventHandler? DismissRequested;

    #endregion

    private const string SurfacePart = "PART_Surface";
    private const string DockedPseudoClass = ":docked";

    private Border? _surface;
    private IDisposable? _pendingSurfaceHide;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_surface is not null)
        {
            _surface.SizeChanged -= HandleSurfaceSizeChanged;
        }

        base.OnApplyTemplate(e);
        _surface = e.NameScope.Find<Border>(SurfacePart);
        if (_surface is not null)
        {
            _surface.SizeChanged += HandleSurfaceSizeChanged;
            SurfaceHeight = _surface.Bounds.Height;
        }
        else
        {
            SurfaceHeight = 0;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsOpenProperty)
        {
            PseudoClasses.Set(StdPseudoClass.Open, IsOpen);
            UpdateSurfaceVisibility();
        }
        else if (change.Property == ModeProperty)
        {
            PseudoClasses.Set(DockedPseudoClass, Mode == FullScreenTitleBarMode.Docked);
            UpdateSurfaceVisibility();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled && e.Key == Key.Escape && Mode == FullScreenTitleBarMode.AutoHide && IsOpen)
        {
            DismissRequested?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelPendingSurfaceHide();
        base.OnDetachedFromVisualTree(e);
    }

    private void UpdateSurfaceVisibility()
    {
        CancelPendingSurfaceHide();
        if (Mode != FullScreenTitleBarMode.Hidden && IsOpen)
        {
            IsSurfaceVisible = true;
            return;
        }

        // Only an AutoHide surface leaves with motion; mode changes and the other modes switch at once.
        if (IsSurfaceVisible &&
            Mode == FullScreenTitleBarMode.AutoHide &&
            IsMotionEnabled &&
            MotionDuration > TimeSpan.Zero &&
            this.IsAttachedToVisualTree())
        {
            _pendingSurfaceHide = DispatcherTimer.RunOnce(CompleteSurfaceHide, MotionDuration, DispatcherPriority.Render);
            return;
        }

        IsSurfaceVisible = false;
    }

    private void CompleteSurfaceHide()
    {
        _pendingSurfaceHide = null;
        if (!IsOpen)
        {
            IsSurfaceVisible = false;
        }
    }

    private void CancelPendingSurfaceHide()
    {
        _pendingSurfaceHide?.Dispose();
        _pendingSurfaceHide = null;
    }

    private void HandleSurfaceSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        SurfaceHeight = e.NewSize.Height;
    }
}
