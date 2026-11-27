using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ReactiveUI;

namespace AtomUI.Toolkits.GalleryBase.Shell;

/// <summary>
/// The Gallery page shell for the browser host: the screen that owns the workspace view model and displays a
/// <see cref="GalleryShellView" />.
/// </summary>
/// <remarks>
/// When <c>Platform.EnableBrowserMediaBreakpoints</c> is set, the view is the media owner of its content and
/// <see cref="MediaBreakPoint" /> follows its own width. Otherwise <see cref="MediaBreakPoint" /> stays
/// <see cref="MediaBreakPoint.ExtraLarge" />.
/// </remarks>
public class GalleryBrowserShellView : UserControl, IScreen, IMediaBreakAwareControl, IDisposable
{
    internal static readonly StyledProperty<MediaBreakPoint> MediaBreakPointProperty =
        MediaBreakAwareControlProperty.MediaBreakPointProperty.AddOwner<GalleryBrowserShellView>();

    private readonly GalleryWorkspaceViewModel _workspaceViewModel;
    private readonly GalleryShellView          _shellView;
    private readonly bool                      _isBrowserMediaBreakpointsEnabled;
    private bool _isDisposed;
    private GalleryMediaBreakPointResolver? _breakpoints;

    /// <summary>
    /// The routing state of the workspace, which <see cref="GalleryShellView.RoutedViewHost" /> displays.
    /// </summary>
    public RoutingState Router => _workspaceViewModel.Router;

    /// <summary>
    /// The breakpoint that responsive content inside the shell resolves its values from.
    /// </summary>
    /// <remarks>
    /// The value stays <see cref="MediaBreakPoint.ExtraLarge" /> while browser media breakpoints are disabled. With
    /// them enabled it follows the width of this view.
    /// </remarks>
    public MediaBreakPoint MediaBreakPoint => GetValue(MediaBreakPointProperty);

    /// <summary>
    /// Occurs after <see cref="MediaBreakPoint" /> has been set to a new breakpoint. The arguments carry the new
    /// breakpoint. A size change that resolves to the current breakpoint does not raise the event.
    /// </summary>
    public event EventHandler<MediaBreakPointChangedEventArgs>? MediaBreakPointChanged;

    /// <summary>
    /// Creates the workspace view model, the navigation view and the shell of the Gallery page.
    /// </summary>
    /// <param name="configuration">
    /// The Gallery configuration. Its platform options decide whether media breakpoints follow the width of this view
    /// and whether the overlay layers are enabled on the visual layer manager.
    /// </param>
    /// <param name="workspaceFactory">
    /// Creates the workspace view model. This view owns it and disposes it in <see cref="Dispose" />.
    /// </param>
    /// <param name="navigationViewFactory">
    /// Creates the navigation view of the sidebar for the workspace view model.
    /// </param>
    protected GalleryBrowserShellView(GalleryBaseConfiguration configuration,
                                      Func<GalleryBaseConfiguration, GalleryWorkspaceViewModel> workspaceFactory,
                                      Func<GalleryWorkspaceViewModel, Control> navigationViewFactory)
    {
        _workspaceViewModel = workspaceFactory(configuration);
        var navigationView = navigationViewFactory(_workspaceViewModel);
        _isBrowserMediaBreakpointsEnabled = configuration.Platform.EnableBrowserMediaBreakpoints;
        if (!_isBrowserMediaBreakpointsEnabled)
        {
            SetCurrentValue(MediaBreakPointProperty, MediaBreakPoint.ExtraLarge);
        }
        _shellView = new GalleryShellView(configuration,
                                          navigationView,
                                          Router,
                                          _isBrowserMediaBreakpointsEnabled);
        if (_isBrowserMediaBreakpointsEnabled)
        {
            SizeChanged += HandleShellSizeChanged;
        }

        var visualLayerManager = new VisualLayerManager
        {
            Child = _shellView
        };
        if (configuration.Platform.ConfigureBrowserOverlayLayers)
        {
            ConfigureOverlayLayers(visualLayerManager);
        }

        Content = visualLayerManager;
    }

    /// <summary>
    /// Stops following the size of the view and disposes the breakpoint resolver, the shell view and the workspace
    /// view model. Calling it again does nothing.
    /// </summary>
    /// <remarks>
    /// Detaching the view from the visual tree disposes it.
    /// </remarks>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        if (_isBrowserMediaBreakpointsEnabled)
        {
            SizeChanged -= HandleShellSizeChanged;
        }
        _breakpoints?.Dispose();
        _breakpoints = null;
        _shellView.Dispose();
        _workspaceViewModel.Dispose();
    }

    /// <summary>
    /// Calls the base member. When media breakpoints follow the width of this view, it creates the breakpoint
    /// resolver and, if the width is already known, resolves <see cref="MediaBreakPoint" /> from it.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_isBrowserMediaBreakpointsEnabled)
        {
            _breakpoints ??= new GalleryMediaBreakPointResolver(this, () =>
                NotifyMediaBreakPointChanged(_breakpoints!.Resolve(Bounds.Width)));
        }
        if (_isBrowserMediaBreakpointsEnabled && Bounds.Width > 0)
        {
            NotifyMediaBreakPointChanged(_breakpoints!.Resolve(Bounds.Width));
        }
    }

    /// <summary>
    /// Disposes the view, then calls the base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    private void HandleShellSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        NotifyMediaBreakPointChanged(_breakpoints?.Resolve(e.NewSize.Width) ?? MediaBreakPoint);
    }

    private void NotifyMediaBreakPointChanged(MediaBreakPoint breakPoint)
    {
        if (MediaBreakPoint == breakPoint)
        {
            return;
        }

        SetCurrentValue(MediaBreakPointProperty, breakPoint);
        MediaBreakPointChanged?.Invoke(this, new MediaBreakPointChangedEventArgs(breakPoint));
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicProperties, typeof(VisualLayerManager))]
    private static void ConfigureOverlayLayers(VisualLayerManager visualLayerManager)
    {
        visualLayerManager.EnableOverlayLayer = true;
        SetVisualLayerManagerProperty(visualLayerManager, "EnablePopupOverlayLayer", true);

        _ = OverlayLayer.GetOverlayLayer(visualLayerManager);
        _ = GetVisualLayerManagerPropertyValue(visualLayerManager, "PopupOverlayLayer");
        _ = GetVisualLayerManagerPropertyValue(visualLayerManager, "LightDismissOverlayLayer");
    }

    private static void SetVisualLayerManagerProperty(VisualLayerManager visualLayerManager,
                                                      string propertyName,
                                                      object? value)
    {
        var propertyInfo = typeof(VisualLayerManager)
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (propertyInfo is null)
        {
            throw new InvalidOperationException($"Unable to find {propertyName} on {nameof(VisualLayerManager)}.");
        }

        propertyInfo.SetValue(visualLayerManager, value);
    }

    private static object? GetVisualLayerManagerPropertyValue(VisualLayerManager visualLayerManager,
                                                              string propertyName)
    {
        var propertyInfo = typeof(VisualLayerManager)
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (propertyInfo is null)
        {
            throw new InvalidOperationException($"Unable to find {propertyName} on {nameof(VisualLayerManager)}.");
        }

        return propertyInfo.GetValue(visualLayerManager);
    }
}
