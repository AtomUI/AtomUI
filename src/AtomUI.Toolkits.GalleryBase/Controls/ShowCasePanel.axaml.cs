using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.Threading;
using AvaloniaControlList = Avalonia.Controls.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

public class ShowCasePanel : TemplatedControl
{
    internal const string MainPanelPart = "PART_MainPanel";
    private const int BrowserInitialShowCaseItemCount = 4;
    private static readonly TimeSpan s_browserProgressiveMountInterval = TimeSpan.FromMilliseconds(650);

    public static readonly StyledProperty<double> MinItemWidthProperty =
        AvaloniaProperty.Register<ShowCasePanel, double>(nameof(MinItemWidth), 320);

    public static readonly StyledProperty<int> MaxColumnsProperty =
        AvaloniaProperty.Register<ShowCasePanel, int>(nameof(MaxColumns), 2);

    public static readonly StyledProperty<double> ColumnGapProperty =
        AvaloniaProperty.Register<ShowCasePanel, double>(nameof(ColumnGap), 16);

    public static readonly StyledProperty<double> RowGapProperty =
        AvaloniaProperty.Register<ShowCasePanel, double>(nameof(RowGap), 16);

    public static readonly StyledProperty<Thickness> ContentMarginProperty =
        AvaloniaProperty.Register<ShowCasePanel, Thickness>(nameof(ContentMargin));

    public static readonly StyledProperty<bool> IsScrollEnabledProperty =
        AvaloniaProperty.Register<ShowCasePanel, bool>(nameof(IsScrollEnabled), true);

    public static readonly StyledProperty<bool> IsDeferredLoadingEnabledProperty =
        AvaloniaProperty.Register<ShowCasePanel, bool>(nameof(IsDeferredLoadingEnabled), false);

    public static readonly StyledProperty<int> InitialDeferredLoadItemCountProperty =
        AvaloniaProperty.Register<ShowCasePanel, int>(nameof(InitialDeferredLoadItemCount), 4);

    public static readonly StyledProperty<int> DeferredLoadBatchSizeProperty =
        AvaloniaProperty.Register<ShowCasePanel, int>(nameof(DeferredLoadBatchSize), 2);

    public static readonly StyledProperty<double> DeferredLoadViewportBufferProperty =
        AvaloniaProperty.Register<ShowCasePanel, double>(nameof(DeferredLoadViewportBuffer), 600);

    private ShowCaseMasonryPanel? _layoutPanel;
    private DispatcherTimer? _progressiveMountTimer;
    private int _nextProgressiveMountIndex;
    private Rect? _lastEffectiveViewport;
    private bool _deferredViewportMaterializationQueued;
    private bool _isEffectiveViewportSubscribed;
    private bool _isAttachedToVisualTree;

    [Content]
    public AvaloniaControlList Children { get; } = new();

    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    public int MaxColumns
    {
        get => GetValue(MaxColumnsProperty);
        set => SetValue(MaxColumnsProperty, value);
    }

    public double ColumnGap
    {
        get => GetValue(ColumnGapProperty);
        set => SetValue(ColumnGapProperty, value);
    }

    public double RowGap
    {
        get => GetValue(RowGapProperty);
        set => SetValue(RowGapProperty, value);
    }

    public Thickness ContentMargin
    {
        get => GetValue(ContentMarginProperty);
        set => SetValue(ContentMarginProperty, value);
    }

    public bool IsScrollEnabled
    {
        get => GetValue(IsScrollEnabledProperty);
        set => SetValue(IsScrollEnabledProperty, value);
    }

    public bool IsDeferredLoadingEnabled
    {
        get => GetValue(IsDeferredLoadingEnabledProperty);
        set => SetValue(IsDeferredLoadingEnabledProperty, value);
    }

    public int InitialDeferredLoadItemCount
    {
        get => GetValue(InitialDeferredLoadItemCountProperty);
        set => SetValue(InitialDeferredLoadItemCountProperty, value);
    }

    public int DeferredLoadBatchSize
    {
        get => GetValue(DeferredLoadBatchSizeProperty);
        set => SetValue(DeferredLoadBatchSizeProperty, value);
    }

    public double DeferredLoadViewportBuffer
    {
        get => GetValue(DeferredLoadViewportBufferProperty);
        set => SetValue(DeferredLoadViewportBufferProperty, value);
    }

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
