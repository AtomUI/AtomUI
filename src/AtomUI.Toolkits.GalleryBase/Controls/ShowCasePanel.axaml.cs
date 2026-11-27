using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.Threading;
using AvaloniaControlList = Avalonia.Controls.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Lays its <see cref="ShowCaseItem" /> children out in the masonry grid of <see cref="ShowCaseMasonryPanel" />,
/// inside a scroll viewer while <see cref="IsScrollEnabled" /> is <see langword="true" />, and materializes the
/// deferred content of the items when they are mounted or, with <see cref="IsDeferredLoadingEnabled" />, as they
/// scroll into view.
/// </summary>
/// <remarks>
/// The <c>ShowCaseCodeSnippetCatalogGenerator</c> source generator skips a panel that sets neither <c>Name</c>
/// nor <c>x:Name</c> and reports warning ATOMUIGEN101.
/// </remarks>
public class ShowCasePanel : TemplatedControl
{
    internal const string MainPanelPart = "PART_MainPanel";
    private const int BrowserInitialShowCaseItemCount = 4;
    private static readonly TimeSpan s_browserProgressiveMountInterval = TimeSpan.FromMilliseconds(650);

    /// <summary>
    /// Identifies the <see cref="MinItemWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MinItemWidthProperty =
        AvaloniaProperty.Register<ShowCasePanel, double>(nameof(MinItemWidth), 320);

    /// <summary>
    /// Identifies the <see cref="MaxColumns" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxColumnsProperty =
        AvaloniaProperty.Register<ShowCasePanel, int>(nameof(MaxColumns), 2);

    /// <summary>
    /// Identifies the <see cref="ColumnGap" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ColumnGapProperty =
        AvaloniaProperty.Register<ShowCasePanel, double>(nameof(ColumnGap), 16);

    /// <summary>
    /// Identifies the <see cref="RowGap" /> property.
    /// </summary>
    public static readonly StyledProperty<double> RowGapProperty =
        AvaloniaProperty.Register<ShowCasePanel, double>(nameof(RowGap), 16);

    /// <summary>
    /// Identifies the <see cref="ContentMargin" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> ContentMarginProperty =
        AvaloniaProperty.Register<ShowCasePanel, Thickness>(nameof(ContentMargin));

    /// <summary>
    /// Identifies the <see cref="IsScrollEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsScrollEnabledProperty =
        AvaloniaProperty.Register<ShowCasePanel, bool>(nameof(IsScrollEnabled), true);

    /// <summary>
    /// Identifies the <see cref="IsDeferredLoadingEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDeferredLoadingEnabledProperty =
        AvaloniaProperty.Register<ShowCasePanel, bool>(nameof(IsDeferredLoadingEnabled), false);

    /// <summary>
    /// Identifies the <see cref="InitialDeferredLoadItemCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> InitialDeferredLoadItemCountProperty =
        AvaloniaProperty.Register<ShowCasePanel, int>(nameof(InitialDeferredLoadItemCount), 4);

    /// <summary>
    /// Identifies the <see cref="DeferredLoadBatchSize" /> property.
    /// </summary>
    public static readonly StyledProperty<int> DeferredLoadBatchSizeProperty =
        AvaloniaProperty.Register<ShowCasePanel, int>(nameof(DeferredLoadBatchSize), 2);

    /// <summary>
    /// Identifies the <see cref="DeferredLoadViewportBuffer" /> property.
    /// </summary>
    public static readonly StyledProperty<double> DeferredLoadViewportBufferProperty =
        AvaloniaProperty.Register<ShowCasePanel, double>(nameof(DeferredLoadViewportBuffer), 600);

    private ShowCaseMasonryPanel? _layoutPanel;
    private DispatcherTimer? _progressiveMountTimer;
    private int _nextProgressiveMountIndex;
    private Rect? _lastEffectiveViewport;
    private bool _deferredViewportMaterializationQueued;
    private bool _isEffectiveViewportSubscribed;
    private bool _isAttachedToVisualTree;

    /// <summary>
    /// The items to display. They are the XAML content of the panel, and only items that are
    /// <see cref="ShowCaseItem" /> instances are mounted into the layout; other elements are ignored. Add all items
    /// before the template is applied: the panel does not watch the list for changes.
    /// </summary>
    [Content]
    public AvaloniaControlList Children { get; } = new();

    /// <summary>
    /// The minimum width of a column of items in DIPs. The layout uses as many columns as fit into the available
    /// width at this width, up to <see cref="MaxColumns" />. The theme takes the value from the
    /// <c>MinItemWidth</c> token of <c>ShowCasePanelToken</c>; without a theme it is 320.
    /// </summary>
    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    /// <summary>
    /// The largest number of columns the layout uses, however wide the available area is; narrower areas get as many
    /// columns of <see cref="MinItemWidth" /> as fit. The theme takes the value from the <c>MaxColumns</c> token of
    /// <c>ShowCasePanelToken</c>; without a theme it is 2.
    /// </summary>
    public int MaxColumns
    {
        get => GetValue(MaxColumnsProperty);
        set => SetValue(MaxColumnsProperty, value);
    }

    /// <summary>
    /// The horizontal space between adjacent columns of items in DIPs. The theme takes the value from the
    /// <c>ColumnGap</c> token of <c>ShowCasePanelToken</c>; without a theme it is 16.
    /// </summary>
    public double ColumnGap
    {
        get => GetValue(ColumnGapProperty);
        set => SetValue(ColumnGapProperty, value);
    }

    /// <summary>
    /// The vertical space in DIPs between an item and the item above it. The theme takes the value from the
    /// <c>RowGap</c> token of <c>ShowCasePanelToken</c>; without a theme it is 16.
    /// </summary>
    public double RowGap
    {
        get => GetValue(RowGapProperty);
        set => SetValue(RowGapProperty, value);
    }

    /// <summary>
    /// The margin around the item layout. It adds no space between items; use <see cref="ColumnGap" /> and
    /// <see cref="RowGap" /> for that. The theme takes the value from the <c>ContentMargin</c> token of
    /// <c>ShowCasePanelToken</c>; without a theme it is 0.
    /// </summary>
    public Thickness ContentMargin
    {
        get => GetValue(ContentMarginProperty);
        set => SetValue(ContentMarginProperty, value);
    }

    /// <summary>
    /// Whether the item layout is wrapped in a vertical scroll viewer. With <see langword="false" /> the layout is
    /// the content of the panel itself and does not scroll, while the panel still measures at the full height of the
    /// layout. The default is <see langword="true" />.
    /// </summary>
    public bool IsScrollEnabled
    {
        get => GetValue(IsScrollEnabledProperty);
        set => SetValue(IsScrollEnabledProperty, value);
    }

    /// <summary>
    /// Whether items with deferred content (<c>IsDeferredContentEnabled</c> is <see langword="true" /> and
    /// <c>DeferredContentTemplate</c> is set on a <see cref="ShowCaseItem" />) stay unmaterialized until they count
    /// among the first <see cref="InitialDeferredLoadItemCount" /> such items or enter the viewport, expanded by
    /// <see cref="DeferredLoadViewportBuffer" />. Deferred loading is in effect only while this property is
    /// <see langword="true" /> and <c>GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled</c> is
    /// <see langword="false" />; otherwise every item is materialized when it is mounted. Changing this property
    /// applies at once: turning it on materializes the initial items and the viewport, and turning it off
    /// materializes all remaining deferred content. The default is <see langword="false" />.
    /// </summary>
    public bool IsDeferredLoadingEnabled
    {
        get => GetValue(IsDeferredLoadingEnabledProperty);
        set => SetValue(IsDeferredLoadingEnabledProperty, value);
    }

    /// <summary>
    /// The number of items with deferred content, counted from the start of <see cref="Children" />, that are
    /// materialized without waiting for the viewport while deferred loading is in effect. A value below 1
    /// materializes none of them. Raising the value materializes more items at once; lowering it leaves materialized
    /// items as they are. The default is 4.
    /// </summary>
    public int InitialDeferredLoadItemCount
    {
        get => GetValue(InitialDeferredLoadItemCountProperty);
        set => SetValue(InitialDeferredLoadItemCountProperty, value);
    }

    /// <summary>
    /// The largest number of items with deferred content that one viewport pass materializes. When more of them
    /// lie inside the viewport, another pass is queued. A value below 1 is treated as 1. The value is used only
    /// while deferred loading is in effect; changing it requests a viewport pass. The default is 2.
    /// </summary>
    public int DeferredLoadBatchSize
    {
        get => GetValue(DeferredLoadBatchSizeProperty);
        set => SetValue(DeferredLoadBatchSizeProperty, value);
    }

    /// <summary>
    /// The distance in DIPs by which the viewport is enlarged on every side when the panel decides which items
    /// with deferred content to materialize, so that items beyond the visible area count as in view. A negative
    /// value is treated as 0. The value is used only while deferred loading is in effect; changing it requests a
    /// viewport pass. The default is 600.
    /// </summary>
    public double DeferredLoadViewportBuffer
    {
        get => GetValue(DeferredLoadViewportBufferProperty);
        set => SetValue(DeferredLoadViewportBufferProperty, value);
    }

    /// <summary>
    /// Empties the layout panel of the previously applied template, calls the base method and then mounts the items
    /// into the <c>PART_MainPanel</c> panel: all at once, or on the browser, while deferred loading is not disabled
    /// globally, the first four and then one per timer tick. It then materializes all deferred content, or only
    /// the initial items and the viewport when deferred loading is in effect. An override must call this member,
    /// since it is what mounts the items.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        StopProgressiveMountTimer();
        _layoutPanel?.Children.Clear();

        base.OnApplyTemplate(e);
        _layoutPanel = e.NameScope.Get<ShowCaseMasonryPanel>(MainPanelPart);
        if (_layoutPanel != null)
        {
            _nextProgressiveMountIndex = 0;
            if (OperatingSystem.IsBrowser() &&
                !GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled)
            {
                _nextProgressiveMountIndex = MountShowCaseItems(0, BrowserInitialShowCaseItemCount);
                StartProgressiveMountTimer();
            }
            else
            {
                MountShowCaseItems(0, Children.Count);
            }

            if (IsDeferredLoadingEffectivelyEnabled)
            {
                MaterializeInitialDeferredContent();
                QueueDeferredViewportMaterialization();
            }
            else
            {
                MaterializeAllDeferredContent();
            }
        }
    }

    /// <summary>
    /// Subscribes to the effective viewport while deferred loading is in effect and to the global switch that
    /// disables deferred loading, and materializes all deferred content when that switch is on. The base method
    /// is called first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttachedToVisualTree = true;
        GalleryShowCaseRuntimeOptions.DeferredLoadingDisabledChanged += HandleDeferredLoadingDisabledChanged;
        UpdateEffectiveViewportSubscription();
        if (GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled)
        {
            MaterializeAllDeferredContent();
        }
    }

    /// <summary>
    /// Unsubscribes from the effective viewport and from the global switch, stops the progressive mount timer and
    /// forgets the last viewport, so that no deferred content is materialized while the panel is detached. The
    /// base method is called last.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttachedToVisualTree = false;
        GalleryShowCaseRuntimeOptions.DeferredLoadingDisabledChanged -= HandleDeferredLoadingDisabledChanged;
        UpdateEffectiveViewportSubscription(false);
        StopProgressiveMountTimer();
        _lastEffectiveViewport                  = null;
        _deferredViewportMaterializationQueued = false;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// When <see cref="IsDeferredLoadingEnabled" /> changes, it updates the viewport subscription and materializes
    /// all deferred content, or the initial items and the viewport, according to the new effective state. A
    /// change of <see cref="InitialDeferredLoadItemCount" /> materializes up to the new count, and a change of
    /// <see cref="DeferredLoadBatchSize" /> or <see cref="DeferredLoadViewportBuffer" /> requests a viewport pass.
    /// The base method is called first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsDeferredLoadingEnabledProperty)
        {
            UpdateEffectiveViewportSubscription();
            if (IsDeferredLoadingEffectivelyEnabled)
            {
                MaterializeInitialDeferredContent();
                QueueDeferredViewportMaterialization();
            }
            else
            {
                MaterializeAllDeferredContent();
            }
        }
        else if (change.Property == InitialDeferredLoadItemCountProperty)
        {
            MaterializeInitialDeferredContent();
        }
        else if (change.Property == DeferredLoadBatchSizeProperty ||
                 change.Property == DeferredLoadViewportBufferProperty)
        {
            QueueDeferredViewportMaterialization();
        }
    }

    /// <summary>
    /// Restarts the progressive mount timer on the browser when the layout panel exists, items are still unmounted
    /// and no timer is running. The base method is called last and does the measurement.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (OperatingSystem.IsBrowser() &&
            _layoutPanel is not null &&
            _progressiveMountTimer is null &&
            _nextProgressiveMountIndex < Children.Count)
        {
            StartProgressiveMountTimer();
        }

        return base.MeasureOverride(availableSize);
    }

    private int MountShowCaseItems(int startIndex, int maxCount)
    {
        if (_layoutPanel is null || maxCount <= 0)
        {
            return startIndex;
        }

        var mountedCount = 0;
        var index        = startIndex;
        while (index < Children.Count && mountedCount < maxCount)
        {
            if (Children[index] is ShowCaseItem item)
            {
                _layoutPanel.Children.Add(item);
                if (!IsDeferredLoadingEffectivelyEnabled)
                {
                    item.MaterializeDeferredContent();
                }
                mountedCount++;
            }

            index++;
        }

        return index;
    }

    private void StartProgressiveMountTimer()
    {
        if (_nextProgressiveMountIndex >= Children.Count)
        {
            return;
        }

        _progressiveMountTimer = new DispatcherTimer(
            s_browserProgressiveMountInterval,
            DispatcherPriority.SystemIdle,
            Dispatcher);
        _progressiveMountTimer.Tick += HandleProgressiveMountTimerTick;
        _progressiveMountTimer.Start();
    }

    private void HandleProgressiveMountTimerTick(object? sender, EventArgs e)
    {
        if (!IsEffectivelyVisible)
        {
            StopProgressiveMountTimer();
            return;
        }

        _nextProgressiveMountIndex = MountShowCaseItems(_nextProgressiveMountIndex, 1);
        if (_nextProgressiveMountIndex >= Children.Count)
        {
            StopProgressiveMountTimer();
        }
    }

    private void StopProgressiveMountTimer()
    {
        if (_progressiveMountTimer is null)
        {
            return;
        }

        _progressiveMountTimer.Stop();
        _progressiveMountTimer.Tick -= HandleProgressiveMountTimerTick;
        _progressiveMountTimer = null;
    }

    private void UpdateEffectiveViewportSubscription(bool? shouldSubscribe = null)
    {
        var subscribe = shouldSubscribe ?? (IsDeferredLoadingEffectivelyEnabled && _isAttachedToVisualTree);
        if (subscribe == _isEffectiveViewportSubscribed)
        {
            return;
        }

        if (subscribe)
        {
            EffectiveViewportChanged += HandleEffectiveViewportChanged;
        }
        else
        {
            EffectiveViewportChanged -= HandleEffectiveViewportChanged;
        }

        _isEffectiveViewportSubscribed = subscribe;
    }

    private void HandleEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        _lastEffectiveViewport = e.EffectiveViewport;
        QueueDeferredViewportMaterialization();
    }

    private void MaterializeAllDeferredContent()
    {
        foreach (var child in Children)
        {
            if (child is ShowCaseItem item)
            {
                item.MaterializeDeferredContent();
            }
        }
    }

    private void MaterializeInitialDeferredContent()
    {
        if (!IsDeferredLoadingEffectivelyEnabled)
        {
            return;
        }

        var remainingCount = Math.Max(0, InitialDeferredLoadItemCount);
        foreach (var child in Children)
        {
            if (remainingCount <= 0)
            {
                return;
            }

            if (child is ShowCaseItem { IsDeferredContentEnabled: true, DeferredContentTemplate: not null } item)
            {
                item.MaterializeDeferredContent();
                remainingCount--;
            }
        }
    }

    private void QueueDeferredViewportMaterialization()
    {
        if (!IsDeferredLoadingEffectivelyEnabled ||
            _deferredViewportMaterializationQueued ||
            !_isAttachedToVisualTree)
        {
            return;
        }

        _deferredViewportMaterializationQueued = true;
        Dispatcher.UIThread.Post(() =>
        {
            _deferredViewportMaterializationQueued = false;
            MaterializeDeferredContentInViewport();
        }, DispatcherPriority.Loaded);
    }

    private void MaterializeDeferredContentInViewport()
    {
        if (!IsDeferredLoadingEffectivelyEnabled ||
            _layoutPanel is null ||
            _lastEffectiveViewport is not { } viewport)
        {
            return;
        }

        var expandedViewport = ExpandViewport(viewport);
        var remainingCount   = Math.Max(1, DeferredLoadBatchSize);
        var hasMoreInViewport = false;

        foreach (var child in _layoutPanel.Children)
        {
            if (child is not ShowCaseItem item ||
                item.IsDeferredContentMaterialized ||
                !item.IsDeferredContentEnabled ||
                item.DeferredContentTemplate is null ||
                !IsItemInViewport(item, expandedViewport))
            {
                continue;
            }

            if (remainingCount <= 0)
            {
                hasMoreInViewport = true;
                break;
            }

            item.MaterializeDeferredContent();
            remainingCount--;
        }

        if (hasMoreInViewport)
        {
            QueueDeferredViewportMaterialization();
        }
    }

    private Rect ExpandViewport(Rect viewport)
    {
        var buffer = Math.Max(0, DeferredLoadViewportBuffer);
        return new Rect(
            viewport.X - buffer,
            viewport.Y - buffer,
            viewport.Width + buffer * 2,
            viewport.Height + buffer * 2);
    }

    private bool IsItemInViewport(ShowCaseItem item, Rect viewport)
    {
        var transform = item.TransformToVisual(this);
        if (!transform.HasValue)
        {
            return false;
        }

        var itemBounds = new Rect(item.Bounds.Size).TransformToAABB(transform.Value);
        return viewport.Intersects(itemBounds);
    }

    private bool IsDeferredLoadingEffectivelyEnabled =>
        IsDeferredLoadingEnabled && !GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled;

    private void HandleDeferredLoadingDisabledChanged(object? sender, EventArgs e)
    {
        UpdateEffectiveViewportSubscription();
        if (IsDeferredLoadingEffectivelyEnabled)
        {
            MaterializeInitialDeferredContent();
            QueueDeferredViewportMaterialization();
        }
        else
        {
            StopProgressiveMountTimer();
            _nextProgressiveMountIndex = MountShowCaseItems(_nextProgressiveMountIndex, Children.Count);
            MaterializeAllDeferredContent();
        }
    }

    internal virtual void NotifyAboutToActive()
    {
    }

    internal virtual void NotifyActivated()
    {
    }

    internal virtual void NotifyAboutToDeactivated()
    {
    }

    internal virtual void NotifyDeactivated()
    {
    }
}
