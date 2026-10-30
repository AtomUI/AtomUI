using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ReactiveUI;

namespace AtomUI.Toolkits.GalleryBase.Shell;

public class GalleryBrowserShellView : UserControl, IScreen, IMediaBreakAwareControl, IDisposable
{
    internal static readonly StyledProperty<MediaBreakPoint> MediaBreakPointProperty =
        MediaBreakAwareControlProperty.MediaBreakPointProperty.AddOwner<GalleryBrowserShellView>();

    private readonly GalleryWorkspaceViewModel _workspaceViewModel;
    private readonly GalleryShellView          _shellView;
    private readonly bool                      _isBrowserMediaBreakpointsEnabled;
    private bool _isDisposed;

    public RoutingState Router => _workspaceViewModel.Router;

    public MediaBreakPoint MediaBreakPoint => GetValue(MediaBreakPointProperty);

    public event EventHandler<MediaBreakPointChangedEventArgs>? MediaBreakPointChanged;

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
        _shellView.Dispose();
        _workspaceViewModel.Dispose();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_isBrowserMediaBreakpointsEnabled && Bounds.Width > 0)
        {
            NotifyMediaBreakPointChanged(GalleryMediaBreakPointResolver.Resolve(Bounds.Width, this));
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    private void HandleShellSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        NotifyMediaBreakPointChanged(GalleryMediaBreakPointResolver.Resolve(e.NewSize.Width, this));
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
