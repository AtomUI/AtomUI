using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.SourceCode;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Hosts the Gallery page content and a drawer on its right side that shows the source code of a showcase.
/// The drawer opens when a <see cref="ShowCaseItem.SourceCodeRequestedEvent" /> bubbles up from the page content
/// and source code display is allowed by the configuration; the request is then marked as handled. The drawer
/// closes and releases its content when the host is detached from the visual tree.
/// </summary>
public sealed class GalleryShowCaseCodeDrawerHost : UserControl
{
    /// <summary>
    /// Identifies the <see cref="PageContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> PageContentProperty =
        AvaloniaProperty.Register<GalleryShowCaseCodeDrawerHost, object?>(nameof(PageContent));

    private readonly GalleryBaseConfiguration _configuration;
    private readonly ContentPresenter _contentPresenter;
    private readonly Drawer _drawer;
    private GalleryShowCaseCodeDrawerContent? _drawerContent;

    /// <summary>
    /// Creates the host. The drawer it owns opens on the right side of the page content.
    /// </summary>
    /// <param name="configuration">
    /// The Gallery configuration. Its <see cref="GalleryBaseConfiguration.SourceCodeDisplay" /> decides whether source
    /// code can be shown and supplies the snippets for the drawer.
    /// </param>
    public GalleryShowCaseCodeDrawerHost(GalleryBaseConfiguration configuration)
    {
        _configuration = configuration;
        _contentPresenter = new ContentPresenter();
        _drawer = new Drawer
        {
            Placement = AtomUI.Desktop.Controls.DrawerPlacement.Right,
            DialogSize = new Dimension(736),
            ContentPadding = new Thickness(8, 1, 1, 1),
            IsShowCloseButton = true,
            IsCloseOnMaskClick = true,
            Title = "Source Code"
        };

        AddHandler(ShowCaseItem.SourceCodeRequestedEvent, HandleSourceCodeRequested);
        _drawer.Closed += HandleDrawerClosed;

        var layout = new Grid
        {
            Children =
            {
                _contentPresenter,
                _drawer
            }
        };

        Content = layout;
    }

    /// <summary>
    /// The page content shown behind the drawer; <see langword="null" /> shows nothing.
    /// </summary>
    public object? PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    /// <summary>
    /// Sets this host as the control the drawer is opened on when the host is attached to a visual tree. The base
    /// method is called first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _drawer.OpenOn = this;
    }

    /// <summary>
    /// Closes the drawer, releases its content and clears the control the drawer is opened on when the host is
    /// detached from a visual tree. The base method is called last.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _drawer.IsOpen = false;
        ClearDrawerContent();
        _drawer.OpenOn = null;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Shows the new <see cref="PageContent" /> in the content presenter when the property changes. The base method
    /// is called first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PageContentProperty)
        {
            _contentPresenter.Content = PageContent;
        }
    }

    private void HandleSourceCodeRequested(object? sender, ShowCaseSourceCodeRequestedEventArgs e)
    {
        if (!_configuration.SourceCodeDisplay.CanShowSourceCode ||
            _configuration.SourceCodeDisplay.SnippetProvider is null)
        {
            return;
        }

        var title = e.Title;
        if (_configuration.SourceCodeDisplay.SnippetProvider.TryGetSnippetGroup(e.Key, out var group))
        {
            title = string.IsNullOrWhiteSpace(title) ? group.Title : title;
            SetDrawerContent(new GalleryShowCaseCodeDrawerContent(group, e.Key));
        }
        else
        {
            SetDrawerContent(new GalleryShowCaseCodeDrawerContent(null, e.Key));
        }

        _drawer.Title = string.IsNullOrWhiteSpace(title) ? "Source Code" : title!;
        _drawer.IsOpen = true;
        e.Handled = true;
    }

    private void HandleDrawerClosed(object? sender, EventArgs e)
    {
        ClearDrawerContent();
    }

    private void SetDrawerContent(GalleryShowCaseCodeDrawerContent content)
    {
        ClearDrawerContent();
        _drawerContent = content;
        _drawer.Content = content;
    }

    private void ClearDrawerContent()
    {
        _drawerContent?.Dispose();
        _drawerContent = null;
        _drawer.Content = null;
    }
}
