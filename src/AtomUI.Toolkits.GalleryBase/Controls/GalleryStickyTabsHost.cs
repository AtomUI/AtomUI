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

    // 吸顶时真实 StickyContent 宿主被提升到受控 adorner 层，必须低于同层其他
    // adorner 以及 Drawer、Dialog、Tour 等真正的浮层。
    private const int StickyElevationZIndex = -1;

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<GalleryStickyTabsHost, object?>(nameof(Header));

    public static readonly StyledProperty<object?> StickyContentProperty =
        AvaloniaProperty.Register<GalleryStickyTabsHost, object?>(nameof(StickyContent));

    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<GalleryStickyTabsHost>();

    public static readonly StyledProperty<Thickness> StickyContentPaddingProperty =
        AvaloniaProperty.Register<GalleryStickyTabsHost, Thickness>(nameof(StickyContentPadding));

    public static readonly StyledProperty<IBrush?> StickyBackgroundProperty =
        Border.BackgroundProperty.AddOwner<GalleryStickyTabsHost>();

    public static readonly StyledProperty<IBrush?> StickyBorderBrushProperty =
        Border.BorderBrushProperty.AddOwner<GalleryStickyTabsHost>();

    public static readonly StyledProperty<bool> IsStickyMirrorEnabledProperty =
        AvaloniaProperty.Register<GalleryStickyTabsHost, bool>(nameof(IsStickyMirrorEnabled), true);

    /// <summary>
    /// 挂起粘滞提升：置位期间即使处于钉住状态也不把页签宿主提升到窗口级
    /// adorner 层（已提升的会降级回面板）。供模态覆盖（如 ShowCaseItem 放大
    /// overlay）使用——overlay 位于内容树内，必须盖住页面的提升物；否则钉住
    /// 的页签会浮在 overlay 之上。
    /// </summary>
    internal static readonly StyledProperty<bool> IsStickyElevationSuppressedProperty =
        AvaloniaProperty.Register<GalleryStickyTabsHost, bool>(nameof(IsStickyElevationSuppressed));

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
    /// 限高模式下内容宿主 MaxHeight 的下限。视口剩余高度小于该下限时，钳制值
    /// 保持在下限而不是继续收缩，页面 ScrollViewer 的 extent 超过 viewport，
    /// 垂直滚动条得以出现；未限高（下限为 0）时行为不变。
    /// </summary>
    internal static readonly StyledProperty<double> ContentMinHeightProperty =
        AvaloniaProperty.Register<GalleryStickyTabsHost, double>(nameof(ContentMinHeight));

    internal double ContentMinHeight
    {
        get => GetValue(ContentMinHeightProperty);
        set => SetValue(ContentMinHeightProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object? StickyContent
    {
        get => GetValue(StickyContentProperty);
        set => SetValue(StickyContentProperty, value);
    }

    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public Thickness StickyContentPadding
    {
        get => GetValue(StickyContentPaddingProperty);
        set => SetValue(StickyContentPaddingProperty, value);
    }

    public IBrush? StickyBackground
    {
        get => GetValue(StickyBackgroundProperty);
        set => SetValue(StickyBackgroundProperty, value);
    }

    public IBrush? StickyBorderBrush
    {
        get => GetValue(StickyBorderBrushProperty);
        set => SetValue(StickyBorderBrushProperty, value);
    }

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
    /// 当前应用到内容宿主上的高度上限；未受约束时为正无穷。
    /// 内容若为模板子级（不经过本控件内容属性收养），MaxHeight 继承链会
    /// 在模板边界断裂（继承走逻辑树），需要宿主显式读取该值转发。
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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseTemplateParts();
        base.OnDetachedFromVisualTree(e);
    }

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
        // 视口剩余高度不足内容下限时保持下限：内容以最小可用高度参与页面
        // 布局并触发页面垂直滚动，而不是被钳制到视口内导致裁切且无法滚动。
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
    /// 吸顶期间把真实的 StickyContent 宿主提升到窗口级受控 adorner 层。
    /// Avalonia 12 的 VisualBrush 只在创建时录制一次内容，无法跟随选中态、
    /// hover 和窗口尺寸变化刷新，因此不能用视觉镜像代替真实控件。
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

        // 提升后 DataContext 不再从页面继承，显式固定为提升前的继承值。
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
