using AtomUI.Toolkits.GalleryBase.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Shows the Gallery page content with the showcase zoom overlay layered on top of it. The content of a
/// <see cref="ShowCaseItem" /> that raises its zoom request moves into the overlay, and returns to the item
/// when the overlay closes, when the item leaves the visual tree or when this host is detached. A request is
/// ignored while another item is zoomed, or when zoom is disabled in the Gallery base configuration.
/// </summary>
public sealed class ShowCaseZoomOverlayHost : UserControl
{
    /// <summary>
    /// Identifies the <see cref="PageContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> PageContentProperty =
        AvaloniaProperty.Register<ShowCaseZoomOverlayHost, object?>(nameof(PageContent));

    private readonly ContentPresenter _contentPresenter;
    private readonly ShowCaseZoomOverlay _overlay;
    private ShowCaseItem? _zoomedItem;
    private object? _zoomedContent;
    private GalleryStickyTabsHost? _suppressedStickyHost;

    public ShowCaseZoomOverlayHost()
    {
        AddHandler(ShowCaseItem.ZoomRequestedEvent, HandleZoomRequested);

        _contentPresenter = new ContentPresenter();
        _overlay = new ShowCaseZoomOverlay();
        _overlay.CloseRequested += HandleOverlayCloseRequested;

        Content = new Grid
        {
            Children =
            {
                _contentPresenter,
                _overlay
            }
        };
    }

    /// <summary>
    /// The content shown beneath the zoom overlay. This host handles the zoom requests that a
    /// <see cref="ShowCaseItem" /> inside it raises. <see langword="null" /> shows no content.
    /// </summary>
    public object? PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    /// <summary>
    /// Called when this host is detached. Before the base call it puts the content of a zoomed item back into
    /// the item, so that the item is not left empty.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        RestoreZoomedItem();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Called when a property of this host changes. Passes <see cref="PageContent" /> to the presenter beneath the
    /// overlay and leaves every other change to the base call.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PageContentProperty)
        {
            _contentPresenter.Content = PageContent;
        }
    }

    private void HandleZoomRequested(object? sender, ShowCaseZoomRequestedEventArgs e)
    {
        if (_zoomedItem is not null ||
            (GalleryBaseConfigurationProvider.Current?.ShowCaseZoom.IsEnabled ?? true) != true)
        {
            return;
        }

        var item = e.Item;
        item.MaterializeDeferredContent();

        _zoomedItem    = item;
        _zoomedContent = item.Content;
        item.SetCurrentValue(ContentControl.ContentProperty, null);
        item.DetachedFromVisualTree += HandleZoomedItemDetached;

        _overlay.Title        = e.Title;
        _overlay.Description  = e.Description;
        _overlay.DataContext  = item.DataContext;
        _overlay.SourceItem   = item;
        // Restore the logical parent first and stage the content second, as the ImagePreviewer overlay does.
        // The content moves into the overlay only visually and stays attached to its original card logically:
        // page-level Styles and Resources and the inherited DataContext resolve along the logical tree, so
        // leaving the page tree loses every style that showcases such as the Grid page apply to their
        // examples through UserControl.Styles. The panel collection claims only children that have no
        // parent, so the reverse order conflicts with the logical attach.
        if (_zoomedContent is Control stagedContent)
        {
            ((ISetLogicalParent)stagedContent).SetParent(item);
        }

        _overlay.StageContent = _zoomedContent;
        _overlay.IsOpen       = true;

        // The zoom overlay lives inside the content tree, but a pinned tabs host is elevated to a window-level
        // adorner layer that renders above all content, so it would float above the overlay unless the elevation
        // is suppressed. The sticky mechanism elevates the host again on its own once the overlay is closed.
        _suppressedStickyHost = item.FindAncestorOfType<GalleryStickyTabsHost>();
        if (_suppressedStickyHost is not null)
        {
            _suppressedStickyHost.SetCurrentValue(
                GalleryStickyTabsHost.IsStickyElevationSuppressedProperty, true);
        }

        e.Handled = true;
    }

    private void HandleOverlayCloseRequested(object? sender, EventArgs e)
    {
        RestoreZoomedItem();
    }

    private void HandleZoomedItemDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        RestoreZoomedItem();
    }

    private void RestoreZoomedItem()
    {
        if (_zoomedItem is null)
        {
            return;
        }

        var item = _zoomedItem;
        _zoomedItem = null;

        item.DetachedFromVisualTree -= HandleZoomedItemDetached;

        if (_suppressedStickyHost is not null)
        {
            _suppressedStickyHost.SetCurrentValue(
                GalleryStickyTabsHost.IsStickyElevationSuppressedProperty, false);
            _suppressedStickyHost = null;
        }

        _overlay.IsOpen           = false;
        _overlay.StageContent     = null;
        _overlay.SourceItem       = null;
        _overlay.Title            = null;
        _overlay.Description      = null;
        _overlay.DataContext      = null;

        if (_zoomedContent is Control stagedContent)
        {
            ((ISetLogicalParent)stagedContent).SetParent(null);
        }

        item.SetCurrentValue(ContentControl.ContentProperty, _zoomedContent);
        _zoomedContent = null;
    }
}
