using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Lays out a Gallery page as a header (<see cref="Header" />), a sticky area (<see cref="StickyContent" />)
/// and scrollable content (<see cref="Content" />) inside one ScrollViewer.
/// While the sticky area is pinned, its real host is moved into a window-level adorner layer so that it
/// stays visible above the scrolling content; a placeholder keeps its place in the layout.
/// </summary>
[TemplatePart(ScrollViewerPart, typeof(ScrollViewer))]
[TemplatePart(StickyPanelPart, typeof(GalleryStickyTabsPanel))]
[TemplatePart(HeaderHostPart, typeof(ContentPresenter))]
[TemplatePart(StickyContentHostPart, typeof(Control))]
[TemplatePart(ContentHostPart, typeof(ContentPresenter))]
public class GalleryStickyTabsHost : TemplatedControl
{
    private const string ScrollViewerPart      = "PART_ScrollViewer";
    private const string StickyPanelPart       = "PART_StickyPanel";
    private const string HeaderHostPart        = "PART_HeaderHost";
    private const string StickyContentHostPart = "PART_StickyContentHost";
    private const string ContentHostPart       = "PART_ContentHost";

    // While pinned, the real host of StickyContent is elevated into the controlled adorner layer
    // (ScopeAwareAdornerLayer). It must stay below the other adorners of that layer and below real overlays
    // such as Drawer, Dialog and Tour.
    private const int StickyElevationZIndex = -1;

    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<GalleryStickyTabsHost, object?>(nameof(Header));

    /// <summary>
    /// Identifies the <see cref="StickyContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> StickyContentProperty =
        AvaloniaProperty.Register<GalleryStickyTabsHost, object?>(nameof(StickyContent));

    /// <summary>
    /// Identifies the <see cref="Content" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<GalleryStickyTabsHost>();

    /// <summary>
    /// Identifies the <see cref="StickyContentPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> StickyContentPaddingProperty =
        AvaloniaProperty.Register<GalleryStickyTabsHost, Thickness>(nameof(StickyContentPadding));

    /// <summary>
    /// Identifies the <see cref="StickyBackground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> StickyBackgroundProperty =
        Border.BackgroundProperty.AddOwner<GalleryStickyTabsHost>();

    /// <summary>
    /// Identifies the <see cref="StickyBorderBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> StickyBorderBrushProperty =
        Border.BorderBrushProperty.AddOwner<GalleryStickyTabsHost>();

    /// <summary>
    /// Identifies the <see cref="IsStickyMirrorEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsStickyMirrorEnabledProperty =
        AvaloniaProperty.Register<GalleryStickyTabsHost, bool>(nameof(IsStickyMirrorEnabled), true);

    /// <summary>
    /// Identifies the <see cref="IsStickyElevationSuppressed" /> property.
    /// </summary>
    internal static readonly StyledProperty<bool> IsStickyElevationSuppressedProperty =
        AvaloniaProperty.Register<GalleryStickyTabsHost, bool>(nameof(IsStickyElevationSuppressed));

    /// <summary>
    /// Suppresses the sticky elevation while it is <see langword="true" />: the sticky content host is not
    /// elevated into the window-level adorner layer even when it is pinned, and a host that is already
    /// elevated is demoted back into the panel. It serves modal covers such as the zoom overlay of a
    /// <see cref="ShowCaseItem" />: that overlay lives inside the content tree and has to cover what the page
    /// elevated, since pinned tabs would otherwise float above the overlay.
    /// </summary>
    internal bool IsStickyElevationSuppressed
    {
        get => GetValue(IsStickyElevationSuppressedProperty);
        set => SetValue(IsStickyElevationSuppressedProperty, value);
    }

    internal static readonly DirectProperty<GalleryStickyTabsHost, bool> HasStickyContentProperty =
        AvaloniaProperty.RegisterDirect<GalleryStickyTabsHost, bool>(
            nameof(HasStickyContent),
            host => host.HasStickyContent,
            (host, value) => host.HasStickyContent = value);

    internal static readonly StyledProperty<bool> IsContentHeightBoundedProperty =
        AvaloniaProperty.Register<GalleryStickyTabsHost, bool>(nameof(IsContentHeightBounded));

    internal static readonly DirectProperty<GalleryStickyTabsHost, double> ContentMaxHeightProperty =
        AvaloniaProperty.RegisterDirect<GalleryStickyTabsHost, double>(
            nameof(ContentMaxHeight),
            host => host.ContentMaxHeight);

    /// <summary>
    /// Identifies the <see cref="ContentMinHeight" /> property.
    /// </summary>
    internal static readonly StyledProperty<double> ContentMinHeightProperty =
        AvaloniaProperty.Register<GalleryStickyTabsHost, double>(nameof(ContentMinHeight));

    /// <summary>
    /// Lower bound of the MaxHeight of the content host while <see cref="IsContentHeightBounded" /> is set.
    /// When the remaining viewport height is below this bound, the clamped value stays at the bound instead
    /// of shrinking further, so the extent of the page ScrollViewer exceeds the viewport and the vertical
    /// scrollbar appears. A bound of 0 leaves the clamp as it is without this bound.
    /// </summary>
    internal double ContentMinHeight
    {
        get => GetValue(ContentMinHeightProperty);
        set => SetValue(ContentMinHeightProperty, value);
    }

    /// <summary>
    /// Content shown at the top of the page, above the sticky area. It is not sticky: it scrolls away with
    /// the page.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Content of the sticky area, usually a tab strip. The area is kept at the top while the rest of the page
    /// scrolls; while it is pinned, its host is moved into a window-level adorner layer so that it stays
    /// visible. <see langword="null" /> hides the sticky area.
    /// </summary>
    public object? StickyContent
    {
        get => GetValue(StickyContentProperty);
        set => SetValue(StickyContentProperty, value);
    }

    /// <summary>
    /// Content shown below the sticky area, inside the page ScrollViewer. This is the XAML content property.
    /// </summary>
    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Padding between the border of the sticky area and the sticky content. It also applies while the area is
    /// elevated. The theme sets it from the StickyContentPadding token.
    /// </summary>
    public Thickness StickyContentPadding
    {
        get => GetValue(StickyContentPaddingProperty);
        set => SetValue(StickyContentPaddingProperty, value);
    }

    /// <summary>
    /// Background of the sticky area, the border that hosts <see cref="StickyContent" />. It also applies while
    /// the area is elevated. The theme sets it from the StickyBackground token; <see langword="null" /> paints
    /// no background.
    /// </summary>
    public IBrush? StickyBackground
    {
        get => GetValue(StickyBackgroundProperty);
        set => SetValue(StickyBackgroundProperty, value);
    }

    /// <summary>
    /// Brush of the one pixel divider that the theme draws below the sticky area. It also applies while the
    /// area is elevated. The theme sets it from the StickyBorderBrush token; <see langword="null" /> paints no
    /// divider.
    /// </summary>
    public IBrush? StickyBorderBrush
    {
        get => GetValue(StickyBorderBrushProperty);
        set => SetValue(StickyBorderBrushProperty, value);
    }

    /// <summary>
    /// Whether the sticky area stays visible while it is pinned. <see langword="true" /> moves its real host
    /// into a window-level adorner layer for that time. <see langword="false" /> keeps the host inside the
    /// page ScrollViewer. The default is <see langword="true" />.
    /// </summary>
    public bool IsStickyMirrorEnabled
    {
        get => GetValue(IsStickyMirrorEnabledProperty);
        set => SetValue(IsStickyMirrorEnabledProperty, value);
    }

    private bool _hasStickyContent;

    internal bool HasStickyContent
    {
        get => _hasStickyContent;
        set => SetAndRaise(HasStickyContentProperty, ref _hasStickyContent, value);
    }

    internal bool IsContentHeightBounded
    {
        get => GetValue(IsContentHeightBoundedProperty);
        set => SetValue(IsContentHeightBoundedProperty, value);
    }

    /// <summary>
    /// The height limit currently applied to the content host, or positive infinity when the height is not
    /// bounded. When the content is a template child that was not adopted through the content property of this
    /// control, the MaxHeight inheritance chain breaks at the template boundary, because inheritance follows
    /// the logical tree. The owner of such content has to read this value and forward it itself.
    /// </summary>
    internal double ContentMaxHeight
    {
        get => _contentMaxHeight;
        private set => SetAndRaise(ContentMaxHeightProperty, ref _contentMaxHeight, value);
    }

    private ScrollViewer? _scrollViewer;
    private double _contentMaxHeight = double.PositiveInfinity;
    private GalleryStickyTabsPanel? _stickyPanel;
    private ContentPresenter? _headerHost;
    private Control? _inlineStickyContentHost;
    private ContentPresenter? _contentHost;
    private ScopeAwareAdornerLayer? _stickyElevationLayer;
    private Border? _stickySlotPlaceholder;
    private IDisposable? _stickyContentHostBoundsSubscription;
    private IDisposable? _stickySlotBoundsSubscription;
    private IDisposable? _headerHostBoundsSubscription;
    private bool _stickyElevationUpdateQueued;
    private bool _elevatedDataContextApplied;

    public GalleryStickyTabsHost()
    {
    }

    /// <summary>
    /// Called when the control template is applied. It releases the parts of a previous template, resolves the
    /// new parts, starts observing the pinned state, the scroll offset and the sizes of the header and sticky
    /// hosts, and then updates the content height limit and the sticky elevation.
    /// Overrides must call the base member; without it no part is resolved and the sticky elevation never runs.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ReleaseTemplateParts();

        base.OnApplyTemplate(e);

        _scrollViewer            = e.NameScope.Get<ScrollViewer>(ScrollViewerPart);
        _stickyPanel             = e.NameScope.Get<GalleryStickyTabsPanel>(StickyPanelPart);
        _headerHost              = e.NameScope.Get<ContentPresenter>(HeaderHostPart);
        _inlineStickyContentHost = e.NameScope.Get<Control>(StickyContentHostPart);
        _contentHost             = e.NameScope.Get<ContentPresenter>(ContentHostPart);

        _stickyPanel.PropertyChanged += HandleStickyPanelPropertyChanged;
        _scrollViewer.ScrollChanged  += HandleScrollChanged;
        _stickyContentHostBoundsSubscription = _inlineStickyContentHost.GetObservable(BoundsProperty)
                                                   .Subscribe(_ =>
                                                   {
                                                       QueueStickyElevationUpdate();
                                                       UpdateContentMaxHeight();
                                                   });
        _headerHostBoundsSubscription = _headerHost.GetObservable(BoundsProperty)
                                                       .Subscribe(_ => UpdateContentMaxHeight());

        UpdateContentMaxHeight();
        UpdateStickyElevation();
    }

    /// <summary>
    /// Called when this control is detached from the visual tree. Before the base call it releases the template
    /// parts: a sticky host that is elevated is moved back into the panel with the values of the elevation
    /// cleared, and all subscriptions are disposed. Overrides must call the base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseTemplateParts();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Called when a property of this control changes. After the base call it keeps the layout in step with the
    /// new values: a change of <see cref="StickyContent" /> updates the visibility of the sticky area, a change
    /// of <see cref="IsStickyMirrorEnabled" />, of a sticky appearance property or of
    /// <see cref="IsStickyElevationSuppressed" /> queues an elevation update, and a change of
    /// <see cref="IsContentHeightBounded" /> or <see cref="ContentMinHeight" /> updates the content height limit.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StickyContentProperty ||
            change.Property == IsStickyMirrorEnabledProperty)
        {
            if (change.Property == StickyContentProperty)
            {
                HasStickyContent = StickyContent is not null;
            }

            QueueStickyElevationUpdate();
        }
        else if (change.Property == StickyBackgroundProperty ||
                 change.Property == StickyBorderBrushProperty ||
                 change.Property == StickyContentPaddingProperty)
        {
            QueueStickyElevationUpdate();
        }
        else if (change.Property == IsContentHeightBoundedProperty ||
                 change.Property == ContentMinHeightProperty)
        {
            UpdateContentMaxHeight();
        }
        else if (change.Property == IsStickyElevationSuppressedProperty)
        {
            QueueStickyElevationUpdate();
        }
    }

    private void ReleaseTemplateParts()
    {
        DemoteStickyContent();

        if (_stickyPanel is not null)
        {
            _stickyPanel.PropertyChanged -= HandleStickyPanelPropertyChanged;
            _stickyPanel = null;
        }

        if (_scrollViewer is not null)
        {
            _scrollViewer.ScrollChanged -= HandleScrollChanged;
            _scrollViewer = null;
        }

        _stickyContentHostBoundsSubscription?.Dispose();
        _stickyContentHostBoundsSubscription = null;

        _stickySlotBoundsSubscription?.Dispose();
        _stickySlotBoundsSubscription = null;

        _headerHostBoundsSubscription?.Dispose();
        _headerHostBoundsSubscription = null;

        _inlineStickyContentHost = null;
        _headerHost              = null;
        _contentHost             = null;
        _stickyElevationUpdateQueued = false;
    }

    private void HandleStickyPanelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == GalleryStickyTabsPanel.IsStickyPinnedProperty)
        {
            QueueStickyElevationUpdate();
        }
    }

    private void HandleScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        QueueStickyElevationUpdate();
        UpdateContentMaxHeight();
    }

    private void UpdateContentMaxHeight()
    {
        if (_contentHost is null || _scrollViewer is null)
        {
            return;
        }

        if (!IsContentHeightBounded)
        {
            _contentHost.MaxHeight = double.PositiveInfinity;
            ContentMaxHeight       = double.PositiveInfinity;
            return;
        }

        var headerHeight = _headerHost?.Bounds.Height ?? 0;
        var stickyHeight = _stickySlotPlaceholder?.Bounds.Height
                           ?? _inlineStickyContentHost?.Bounds.Height ?? 0;
        // Keep the lower bound when the remaining viewport height is below it: the content then takes part in
        // the page layout with its minimum usable height and makes the page scroll vertically, instead of
        // being clamped into the viewport, where it would be clipped and could not be scrolled to.
        _contentHost.MaxHeight = Math.Max(
            ContentMinHeight,
            Math.Max(0, _scrollViewer.Viewport.Height - headerHeight - stickyHeight));
        ContentMaxHeight       = _contentHost.MaxHeight;
    }

    private void QueueStickyElevationUpdate()
    {
        if (_stickyElevationUpdateQueued)
        {
            return;
        }

        _stickyElevationUpdateQueued = true;
        Dispatcher.UIThread.Post(() =>
        {
            _stickyElevationUpdateQueued = false;
            UpdateStickyElevation();
        }, DispatcherPriority.Render);
    }

    private void UpdateStickyElevation()
    {
        if (!IsStickyMirrorEnabled || IsStickyElevationSuppressed)
        {
            DemoteStickyContent();
            return;
        }

        if (HasStickyContent &&
            _stickyPanel?.IsStickyPinned == true &&
            _inlineStickyContentHost is not null)
        {
            ElevateStickyContent();
            UpdateElevatedStickyContentBounds();
        }
        else
        {
            DemoteStickyContent();
        }
    }

    /// <summary>
    /// Moves the real host of the sticky content into the window-level controlled adorner layer while it is
    /// pinned. The VisualBrush of Avalonia 12 records its content only once, at creation, and does not
    /// refresh with the selected state, hover or window size changes, so a visual mirror cannot replace the
    /// real control.
    /// </summary>
    private void ElevateStickyContent()
    {
        if (_stickySlotPlaceholder is not null ||
            _stickyPanel is null ||
            _inlineStickyContentHost is null)
        {
            return;
        }

        var stickyPanel = _stickyPanel;
        var stickyHost  = _inlineStickyContentHost;
        if (stickyPanel is null || stickyHost is null)
        {
            return;
        }

        var elevationLayer = ResolveStickyElevationLayer();
        if (elevationLayer is null ||
            !ReferenceEquals(_stickyPanel, stickyPanel) ||
            !ReferenceEquals(_inlineStickyContentHost, stickyHost) ||
            !stickyPanel.IsStickyPinned)
        {
            return;
        }

        var stickyIndex   = stickyPanel.StickyIndex;
        var desiredHeight = stickyHost.DesiredSize.Height;

        _stickySlotPlaceholder = new Border
        {
            Height           = desiredHeight,
            Focusable        = false,
            IsHitTestVisible = false
        };
        stickyPanel.Children.Insert(stickyIndex + 1, _stickySlotPlaceholder);
        stickyPanel.Children.Remove(stickyHost);
        _stickySlotBoundsSubscription = _stickySlotPlaceholder.GetObservable(BoundsProperty)
                                              .Subscribe(_ =>
                                              {
                                                  QueueStickyElevationUpdate();
                                                  UpdateContentMaxHeight();
                                              });

        // After the elevation the DataContext is no longer inherited from the page, so the value inherited so far
        // is set as a local value.
        var inheritedDataContext = stickyHost.DataContext;
        stickyHost.SetValue(DataContextProperty, inheritedDataContext);
        _elevatedDataContextApplied = true;

        stickyHost.ZIndex = StickyElevationZIndex;
        _stickyElevationLayer = elevationLayer;
        elevationLayer.Children.Add(stickyHost);
    }

    private void DemoteStickyContent()
    {
        if (_stickySlotPlaceholder is null)
        {
            return;
        }

        var stickyHost = _inlineStickyContentHost;
        if (stickyHost is not null &&
            ReferenceEquals(stickyHost.GetVisualParent(), _stickyElevationLayer))
        {
            _stickyElevationLayer?.Children.Remove(stickyHost);
            stickyHost.ClearValue(WidthProperty);
            stickyHost.ClearValue(HeightProperty);
            stickyHost.ClearValue(Canvas.LeftProperty);
            stickyHost.ClearValue(Canvas.TopProperty);
            stickyHost.ClearValue(ZIndexProperty);
            if (_elevatedDataContextApplied)
            {
                stickyHost.ClearValue(DataContextProperty);
                _elevatedDataContextApplied = false;
            }

            if (_stickyPanel is not null &&
                _stickyPanel.Children.Contains(_stickySlotPlaceholder))
            {
                var stickyIndex = _stickyPanel.Children.IndexOf(_stickySlotPlaceholder);
                _stickyPanel.Children.RemoveAt(stickyIndex);
                _stickyPanel.Children.Insert(stickyIndex, stickyHost);
            }
        }

        _stickySlotBoundsSubscription?.Dispose();
        _stickySlotBoundsSubscription = null;

        _stickySlotPlaceholder = null;
        _stickyElevationLayer  = null;
    }

    private void UpdateElevatedStickyContentBounds()
    {
        if (_inlineStickyContentHost is null ||
            _stickySlotPlaceholder is null ||
            _stickyElevationLayer is null)
        {
            return;
        }

        var transform = _stickySlotPlaceholder.TransformToVisual(_stickyElevationLayer);
        if (!transform.HasValue)
        {
            return;
        }

        var position = transform.Value.Transform(default);
        var width    = Math.Max(0, _stickySlotPlaceholder.Bounds.Width);
        var height   = Math.Max(0, _stickySlotPlaceholder.Bounds.Height);

        Canvas.SetLeft(_inlineStickyContentHost, position.X);
        Canvas.SetTop(_inlineStickyContentHost, position.Y);
        _inlineStickyContentHost.Width  = width;
        _inlineStickyContentHost.Height = height;
    }

    private ScopeAwareAdornerLayer? ResolveStickyElevationLayer()
    {
        if (this.FindAncestorOfType<VisualLayerManager>() is { } visualLayerManager)
        {
            return ScopeAwareAdornerLayer.GetLayer(visualLayerManager);
        }

        if (TopLevel.GetTopLevel(this) is { } topLevel)
        {
            return ScopeAwareAdornerLayer.GetLayer(topLevel);
        }

        return ScopeAwareAdornerLayer.GetLayer(this);
    }

}
