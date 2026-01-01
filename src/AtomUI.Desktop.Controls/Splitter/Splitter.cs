using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal)]
public class Splitter : Panel, IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Splitter, Orientation>(nameof(Orientation), Orientation.Vertical);

    public static readonly StyledProperty<bool> IsLazyProperty =
        AvaloniaProperty.Register<Splitter, bool>(nameof(IsLazy));

    public static readonly StyledProperty<double> HandleSizeProperty =
        AvaloniaProperty.Register<Splitter, double>(nameof(HandleSize));

    public static readonly StyledProperty<IconTemplate?> CollapsePreviousIconProperty =
        AvaloniaProperty.Register<Splitter, IconTemplate?>(nameof(CollapsePreviousIcon));

    public static readonly StyledProperty<IconTemplate?> CollapseNextIconProperty =
        AvaloniaProperty.Register<Splitter, IconTemplate?>(nameof(CollapseNextIcon));

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool IsLazy
    {
        get => GetValue(IsLazyProperty);
        set => SetValue(IsLazyProperty, value);
    }

    public double HandleSize
    {
        get => GetValue(HandleSizeProperty);
        set => SetValue(HandleSizeProperty, value);
    }

    public IconTemplate? CollapsePreviousIcon
    {
        get => GetValue(CollapsePreviousIconProperty);
        set => SetValue(CollapsePreviousIconProperty, value);
    }

    public IconTemplate? CollapseNextIcon
    {
        get => GetValue(CollapseNextIconProperty);
        set => SetValue(CollapseNextIconProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler<SplitterResizeEventArgs>? ResizeStarted;
    public event EventHandler<SplitterResizeEventArgs>? ResizeDelta;
    public event EventHandler<SplitterResizeEventArgs>? ResizeCompleted;

    #endregion

    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => SplitterToken.ID;

    #endregion

    private readonly List<SplitterPanel> _panels = new();
    private readonly List<SplitterHandle> _handles = new();
    private readonly HashSet<SplitterPanel> _trackedPanels = new();

    private bool _suppressChildrenChanged;
    private bool _suppressPanelChange;
    private Size _lastLayoutSize;
    private DragContext? _dragContext;

    static Splitter()
    {
        AffectsMeasure<Splitter>(OrientationProperty, HandleSizeProperty);
    }

    public Splitter()
    {
        Children.CollectionChanged += HandleChildrenChanged;
        UpdatePseudoClasses();
        this.RegisterResources();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OrientationProperty)
        {
            UpdatePseudoClasses();
            UpdateHandleLayouts();
            InvalidateMeasure();
        }

        if (change.Property == HandleSizeProperty)
        {
            UpdateHandleLayouts();
        }

        if (change.Property == CollapsePreviousIconProperty ||
            change.Property == CollapseNextIconProperty)
        {
            UpdateHandleLayouts();
        }
    }

    private void HandleChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_suppressChildrenChanged)
        {
            return;
        }

        RefreshPanelsAndHandles();
    }

    private void RefreshPanelsAndHandles()
    {
        _suppressChildrenChanged = true;
        try
        {
            foreach (var handle in _handles)
            {
                handle.DragStarted -= HandleDragStarted;
                handle.DragDelta -= HandleDragDelta;
                handle.DragCompleted -= HandleDragCompleted;
                handle.CollapsePreviousRequested -= HandleCollapsePreviousRequested;
                handle.CollapseNextRequested -= HandleCollapseNextRequested;
                Children.Remove(handle);
            }
            _handles.Clear();

            var panels = Children.OfType<SplitterPanel>().ToList();
            SyncTrackedPanels(panels);
            _panels.Clear();
            _panels.AddRange(panels);

            for (var i = 0; i < _panels.Count - 1; i++)
            {
                var handle = CreateHandle(i);
                _handles.Add(handle);
                Children.Add(handle);
            }
        }
        finally
        {
            _suppressChildrenChanged = false;
        }

        InvalidateMeasure();
    }

    private void SyncTrackedPanels(IReadOnlyList<SplitterPanel> panels)
    {
        foreach (var panel in _trackedPanels.ToList())
        {
            if (!panels.Contains(panel))
            {
                panel.PropertyChanged -= HandlePanelPropertyChanged;
                panel.Owner = null;
                _trackedPanels.Remove(panel);
            }
        }

        foreach (var panel in panels)
        {
            if (_trackedPanels.Add(panel))
            {
                panel.Owner = this;
                panel.UpdatePseudoClasses();
                panel.PropertyChanged += HandlePanelPropertyChanged;
            }
        }
    }

    private SplitterHandle CreateHandle(int index)
    {
        var handle = new SplitterHandle
        {
            Orientation = Orientation,
            HandleIndex = index
        };

        handle.DragStarted += HandleDragStarted;
        handle.DragDelta += HandleDragDelta;
        handle.DragCompleted += HandleDragCompleted;
        handle.CollapsePreviousRequested += HandleCollapsePreviousRequested;
        handle.CollapseNextRequested += HandleCollapseNextRequested;

        UpdateHandleState(index, handle);
        return handle;
    }

    private void HandlePanelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not SplitterPanel panel)
        {
            return;
        }

        if (e.Property == SplitterPanel.IsCollapsedProperty)
        {
            if (_suppressPanelChange)
            {
                return;
            }
            ApplyPanelCollapse(panel, panel.IsCollapsed);
            return;
        }

        if (e.Property == SplitterPanel.IsResizableProperty ||
            e.Property == SplitterPanel.CollapsibleProperty)
        {
            UpdateHandlesForPanel(panel);
            return;
        }

        if (e.Property == SplitterPanel.SizeProperty ||
            e.Property == SplitterPanel.DefaultSizeProperty ||
            e.Property == SplitterPanel.MinProperty ||
            e.Property == SplitterPanel.MaxProperty)
        {
            InvalidateMeasure();
        }
    }

    private void UpdateHandlesForPanel(SplitterPanel panel)
    {
        var index = _panels.IndexOf(panel);
        if (index < 0)
        {
            return;
        }

        if (index > 0)
        {
            UpdateHandleState(index - 1);
        }

        if (index < _handles.Count)
        {
            UpdateHandleState(index);
        }
    }

    private void UpdateHandleLayouts()
    {
        for (var i = 0; i < _handles.Count; i++)
        {
            var handle = _handles[i];
            handle.Orientation = Orientation;
            UpdateHandleState(i, handle);
        }
    }

    private void UpdateHandleState(int index)
    {
        if (index < 0 || index >= _handles.Count)
        {
            return;
        }

        UpdateHandleState(index, _handles[index]);
    }

    private void UpdateHandleState(int index, SplitterHandle handle)
    {
        if (index < 0 || index + 1 >= _panels.Count)
        {
            return;
        }

        var previous = _panels[index];
        var next = _panels[index + 1];
        var availableLength = GetAvailablePanelLength(GetLayoutLength(_lastLayoutSize));

        var canDrag = CanDragBetween(previous, next, availableLength);
        if (!canDrag)
        {
            if (previous.IsCollapsed && index - 1 >= 0)
            {
                canDrag = CanDragBetween(_panels[index - 1], previous, availableLength);
            }

            if (!canDrag && next.IsCollapsed && index + 2 < _panels.Count)
            {
                canDrag = CanDragBetween(next, _panels[index + 2], availableLength);
            }
        }
        handle.IsDragEnabled = canDrag;
        handle.Cursor = canDrag
            ? Orientation == Orientation.Vertical
                ? new Cursor(StandardCursorType.SizeWestEast)
                : new Cursor(StandardCursorType.SizeNorthSouth)
            : Cursor.Default;

        handle.PreviousButtonControlsNext = false;
        handle.NextButtonControlsPrevious = false;
        handle.PreviousIconTemplate = CollapsePreviousIcon;
        handle.NextIconTemplate = CollapseNextIcon;

        if (!TryGetBoundaryForHandle(index, out var leftVisible, out var rightVisible))
        {
            handle.IsPreviousCollapsible = false;
            handle.IsNextCollapsible = false;
            handle.ShowPreviousButton = false;
            handle.ShowNextButton = false;
            handle.IsPreviousCollapsed = false;
            handle.IsNextCollapsed = false;
            handle.UpdateCollapseButtons();
            return;
        }

        var hasLeftButton = TryGetExpandLeftState(leftVisible, rightVisible, out var leftShowMode);
        var hasRightButton = TryGetExpandRightState(leftVisible, rightVisible, out var rightShowMode);
        var showBothOnHover = leftVisible >= 0 && rightVisible >= 0 && rightVisible == leftVisible + 1;

        handle.IsPreviousCollapsible = hasLeftButton;
        handle.IsNextCollapsible = hasRightButton;
        handle.ShowPreviousButton = hasLeftButton;
        handle.ShowNextButton = hasRightButton;
        handle.ShowBothButtonsOnHover = showBothOnHover;
        handle.PreviousShowMode = leftShowMode;
        handle.NextShowMode = rightShowMode;
        handle.IsPreviousCollapsed = false;
        handle.IsNextCollapsed = false;

        handle.UpdateCollapseButtons();
    }

    private bool CanDragBetween(SplitterPanel previous, SplitterPanel next, double availableLength)
    {
        if (!previous.IsResizable || !next.IsResizable)
        {
            return false;
        }

        if (previous.IsCollapsed && !CanExpandByDrag(previous, availableLength))
        {
            return false;
        }

        if (next.IsCollapsed && !CanExpandByDrag(next, availableLength))
        {
            return false;
        }

        return true;
    }

    private bool CanExpandByDrag(SplitterPanel panel, double availableLength)
    {
        var min = GetPanelMin(panel, availableLength);
        return min <= 0.0;
    }

    private void HandleCollapsePreviousRequested(object? sender, EventArgs e)
    {
        if (sender is SplitterHandle handle)
        {
            ExpandLeftAtHandle(handle);
        }
    }

    private void HandleCollapseNextRequested(object? sender, EventArgs e)
    {
        if (sender is SplitterHandle handle)
        {
            ExpandRightAtHandle(handle);
        }
    }

    private void ExpandLeftAtHandle(SplitterHandle handle)
    {
        if (!TryGetBoundaryForHandle(handle.HandleIndex, out var leftVisible, out var rightVisible))
        {
            return;
        }

        if (!IsPrimaryHandleForBoundary(handle.HandleIndex, leftVisible, rightVisible))
        {
            return;
        }

        if (leftVisible < 0)
        {
            return;
        }

        var availableLength = GetAvailablePanelLength(GetLayoutLength(_lastLayoutSize));
        var restoreIndex = FindRestoreCandidateLeft(leftVisible, rightVisible);
        if (restoreIndex.HasValue)
        {
            var ownerIndex = ResolveOwnerIndex(restoreIndex.Value);
            if (ownerIndex >= 0 &&
                _panels[restoreIndex.Value].IsResizable &&
                _panels[ownerIndex].IsResizable)
            {
                ApplyCollapseForIndices(restoreIndex.Value, ownerIndex, availableLength);
            }
            return;
        }

        if (rightVisible < 0)
        {
            return;
        }

        if (!_panels[leftVisible].IsResizable || !_panels[rightVisible].IsResizable)
        {
            return;
        }

        ApplyCollapseForIndices(leftVisible, rightVisible, availableLength);
    }

    private void ExpandRightAtHandle(SplitterHandle handle)
    {
        if (!TryGetBoundaryForHandle(handle.HandleIndex, out var leftVisible, out var rightVisible))
        {
            return;
        }

        if (!IsPrimaryHandleForBoundary(handle.HandleIndex, leftVisible, rightVisible))
        {
            return;
        }

        if (rightVisible < 0)
        {
            return;
        }

        var availableLength = GetAvailablePanelLength(GetLayoutLength(_lastLayoutSize));
        var restoreIndex = FindRestoreCandidateRight(leftVisible, rightVisible);
        if (restoreIndex.HasValue)
        {
            var ownerIndex = ResolveOwnerIndex(restoreIndex.Value);
            if (ownerIndex >= 0 &&
                _panels[restoreIndex.Value].IsResizable &&
                _panels[ownerIndex].IsResizable)
            {
                ApplyCollapseForIndices(restoreIndex.Value, ownerIndex, availableLength);
            }
            return;
        }

        if (leftVisible < 0)
        {
            return;
        }

        if (!_panels[leftVisible].IsResizable || !_panels[rightVisible].IsResizable)
        {
            return;
        }

        ApplyCollapseForIndices(rightVisible, leftVisible, availableLength);
    }

    private bool TryGetBoundaryForHandle(int handleIndex, out int leftVisible, out int rightVisible)
    {
        leftVisible = FindLeftVisibleIndex(handleIndex);
        rightVisible = FindRightVisibleIndex(handleIndex);
        return leftVisible != -1 || rightVisible != -1;
    }

    private int FindLeftVisibleIndex(int handleIndex)
    {
        for (var i = handleIndex; i >= 0; i--)
        {
            if (!_panels[i].IsCollapsed)
            {
                return i;
            }
        }

        return -1;
    }

    private int FindRightVisibleIndex(int handleIndex)
    {
        for (var i = handleIndex + 1; i < _panels.Count; i++)
        {
            if (!_panels[i].IsCollapsed)
            {
                return i;
            }
        }

        return -1;
    }

    private bool IsPrimaryHandleForBoundary(int handleIndex, int leftVisible, int rightVisible)
    {
        if (rightVisible >= 0)
        {
            return handleIndex == rightVisible - 1;
        }

        if (leftVisible >= 0)
        {
            return handleIndex == leftVisible;
        }

        return false;
    }

    private bool TryGetExpandLeftState(
        int leftVisible,
        int rightVisible,
        out SplitterCollapsibleIconDisplayMode showMode)
    {
        showMode = SplitterCollapsibleIconDisplayMode.Hover;
        if (leftVisible < 0)
        {
            return false;
        }

        var restoreIndex = FindRestoreCandidateLeft(leftVisible, rightVisible);
        if (restoreIndex.HasValue)
        {
            var ownerIndex = ResolveOwnerIndex(restoreIndex.Value);
            if (ownerIndex < 0)
            {
                return false;
            }

            if (!_panels[restoreIndex.Value].IsResizable || !_panels[ownerIndex].IsResizable)
            {
                return false;
            }
        }
        else if (rightVisible < 0)
        {
            return false;
        }

        if (!restoreIndex.HasValue &&
            (!_panels[leftVisible].IsResizable || !_panels[rightVisible].IsResizable))
        {
            return false;
        }

        var targetIndex = restoreIndex ?? leftVisible;
        if (targetIndex < 0)
        {
            return false;
        }

        var targetPanel = _panels[targetIndex];
        if (!targetPanel.IsCollapsible || !targetPanel.IsResizable)
        {
            return false;
        }

        showMode = targetPanel.Collapsible?.ShowCollapsibleIcon ?? SplitterCollapsibleIconDisplayMode.Hover;
        return true;
    }

    private bool TryGetExpandRightState(
        int leftVisible,
        int rightVisible,
        out SplitterCollapsibleIconDisplayMode showMode)
    {
        showMode = SplitterCollapsibleIconDisplayMode.Hover;
        if (rightVisible < 0)
        {
            return false;
        }

        var restoreIndex = FindRestoreCandidateRight(leftVisible, rightVisible);
        if (restoreIndex.HasValue)
        {
            var ownerIndex = ResolveOwnerIndex(restoreIndex.Value);
            if (ownerIndex < 0)
            {
                return false;
            }

            if (!_panels[restoreIndex.Value].IsResizable || !_panels[ownerIndex].IsResizable)
            {
                return false;
            }
        }
        else if (leftVisible < 0)
        {
            return false;
        }

        if (!restoreIndex.HasValue &&
            (!_panels[leftVisible].IsResizable || !_panels[rightVisible].IsResizable))
        {
            return false;
        }

        var targetIndex = restoreIndex ?? rightVisible;
        if (targetIndex < 0)
        {
            return false;
        }

        var targetPanel = _panels[targetIndex];
        if (!targetPanel.IsCollapsible || !targetPanel.IsResizable)
        {
            return false;
        }

        showMode = targetPanel.Collapsible?.ShowCollapsibleIcon ?? SplitterCollapsibleIconDisplayMode.Hover;
        return true;
    }

    private int? FindRestoreCandidateLeft(int leftVisible, int rightVisible)
    {
        if (leftVisible < 0)
        {
            return null;
        }

        if (rightVisible >= 0)
        {
            var start = rightVisible - 1;
            var end = leftVisible;

            var owner = leftVisible;
            for (var i = start; i > end; i--)
            {
                var panel = _panels[i];
                if (!panel.IsCollapsed || !panel.IsCollapsible)
                {
                    continue;
                }

                if (!panel.LastCollapsedIntoIndex.HasValue || panel.LastCollapsedIntoIndex.Value != owner)
                {
                    continue;
                }

                return i;
            }
        }
        else
        {
            var start = _panels.Count - 1;
            var end = leftVisible + 1;

            var owner = leftVisible;
            for (var i = start; i >= end; i--)
            {
                var panel = _panels[i];
                if (!panel.IsCollapsed || !panel.IsCollapsible)
                {
                    continue;
                }

                if (!panel.LastCollapsedIntoIndex.HasValue || panel.LastCollapsedIntoIndex.Value != owner)
                {
                    continue;
                }

                return i;
            }
        }

        return null;
    }

    private int? FindRestoreCandidateRight(int leftVisible, int rightVisible)
    {
        if (rightVisible < 0)
        {
            return null;
        }

        if (leftVisible >= 0)
        {
            var start = leftVisible + 1;
            var end = rightVisible;

            var owner = rightVisible;
            for (var i = start; i < end; i++)
            {
                var panel = _panels[i];
                if (!panel.IsCollapsed || !panel.IsCollapsible)
                {
                    continue;
                }

                if (!panel.LastCollapsedIntoIndex.HasValue || panel.LastCollapsedIntoIndex.Value != owner)
                {
                    continue;
                }

                return i;
            }
        }
        else
        {
            var start = 0;
            var end = rightVisible;

            var owner = rightVisible;
            for (var i = start; i < end; i++)
            {
                var panel = _panels[i];
                if (!panel.IsCollapsed || !panel.IsCollapsible)
                {
                    continue;
                }

                if (!panel.LastCollapsedIntoIndex.HasValue || panel.LastCollapsedIntoIndex.Value != owner)
                {
                    continue;
                }

                return i;
            }
        }

        return null;
    }

    private int ResolveOwnerIndex(int panelIndex)
    {
        if (panelIndex < 0 || panelIndex >= _panels.Count)
        {
            return -1;
        }

        var index = panelIndex;
        var guard = 0;
        while (index >= 0 && index < _panels.Count && _panels[index].IsCollapsed)
        {
            var next = _panels[index].LastCollapsedIntoIndex;
            if (!next.HasValue || next.Value == index)
            {
                return -1;
            }

            index = next.Value;
            guard++;
            if (guard > _panels.Count)
            {
                return -1;
            }
        }

        return index;
    }

    private void ApplyCollapseForIndices(int collapseIndex, int partnerIndex, double availableLength)
    {
        if (collapseIndex < 0 || collapseIndex >= _panels.Count ||
            partnerIndex < 0 || partnerIndex >= _panels.Count ||
            collapseIndex == partnerIndex)
        {
            return;
        }

        if (collapseIndex < partnerIndex)
        {
            var previous = _panels[collapseIndex];
            var next = _panels[partnerIndex];
            if (!previous.IsCollapsed && previous.EffectiveSize > 0)
            {
                previous.LastNonCollapsedSize = previous.EffectiveSize;
            }

            var targetPrevious = previous.IsCollapsed
                ? GetRestoreSize(previous, availableLength)
                : 0;
            var targetDelta = targetPrevious - previous.EffectiveSize;
            ApplyCollapseDelta(previous, next, targetDelta, availableLength, collapseIndex, partnerIndex);
        }
        else
        {
            var previous = _panels[partnerIndex];
            var next = _panels[collapseIndex];
            if (!next.IsCollapsed && next.EffectiveSize > 0)
            {
                next.LastNonCollapsedSize = next.EffectiveSize;
            }

            var targetNext = next.IsCollapsed
                ? GetRestoreSize(next, availableLength)
                : 0;
            var targetDelta = next.EffectiveSize - targetNext;
            ApplyCollapseDelta(previous, next, targetDelta, availableLength, partnerIndex, collapseIndex);
        }
    }

    private double GetRestoreSize(SplitterPanel panel, double availableLength)
    {
        var restoreSize = panel.LastNonCollapsedSize
                          ?? ResolvePanelInitialSize(panel, availableLength)
                          ?? GetFallbackPanelSize(panel, availableLength);
        return ClampPanelSize(panel, restoreSize, availableLength);
    }

    private void ApplyCollapseDelta(
        SplitterPanel previous,
        SplitterPanel next,
        double targetDelta,
        double availableLength,
        int previousIndex,
        int nextIndex)
    {
        var prevSize = previous.EffectiveSize;
        var nextSize = next.EffectiveSize;
        var prevMin = GetPanelMin(previous, availableLength);
        var prevMax = GetPanelMax(previous, availableLength);
        if (prevMax < prevMin)
        {
            prevMax = prevMin;
        }

        var nextMin = GetPanelMin(next, availableLength);
        var nextMax = GetPanelMax(next, availableLength);
        if (nextMax < nextMin)
        {
            nextMax = nextMin;
        }

        var minDelta = Math.Max(prevMin - prevSize, nextSize - nextMax);
        var maxDelta = Math.Min(prevMax - prevSize, nextSize - nextMin);
        if (minDelta > maxDelta)
        {
            return;
        }

        var clampedDelta = Math.Clamp(targetDelta, minDelta, maxDelta);
        var newPrev = prevSize + clampedDelta;
        var newNext = nextSize - clampedDelta;

        if (prevSize > 0 && newPrev <= 0)
        {
            previous.LastNonCollapsedSize = prevSize;
        }

        if (nextSize > 0 && newNext <= 0)
        {
            next.LastNonCollapsedSize = nextSize;
        }

        if (newPrev > 0)
        {
            previous.LastCollapsedIntoIndex = null;
        }
        else if (prevSize > 0 && newPrev <= 0)
        {
            previous.LastCollapsedIntoIndex = nextIndex;
        }

        if (newNext > 0)
        {
            next.LastCollapsedIntoIndex = null;
        }
        else if (nextSize > 0 && newNext <= 0)
        {
            next.LastCollapsedIntoIndex = previousIndex;
        }

        SetPanelSize(previous, newPrev, availableLength, updateLastSize: newPrev > 0);
        SetPanelSize(next, newNext, availableLength, updateLastSize: newNext > 0);

        UpdatePanelCollapsedState(previous, newPrev);
        UpdatePanelCollapsedState(next, newNext);

        InvalidateArrange();
    }

    private void SetPanelCollapsed(SplitterPanel panel, bool collapsed)
    {
        _suppressPanelChange = true;
        try
        {
            ApplyPanelCollapse(panel, collapsed);
            panel.SetCurrentValue(SplitterPanel.IsCollapsedProperty, collapsed);
            panel.UpdatePseudoClasses();
        }
        finally
        {
            _suppressPanelChange = false;
        }
    }

    private void ApplyPanelCollapse(SplitterPanel panel, bool collapsed)
    {
        var availableLength = GetAvailablePanelLength(GetLayoutLength(_lastLayoutSize));
        if (collapsed)
        {
            if (panel.EffectiveSize > 0)
            {
                panel.LastNonCollapsedSize = panel.EffectiveSize;
            }

            SetPanelSize(panel, 0, availableLength, updateLastSize: false);
        }
        else
        {
            var restoreSize = panel.LastNonCollapsedSize
                              ?? ResolvePanelInitialSize(panel, availableLength)
                              ?? GetFallbackPanelSize(panel, availableLength);
            restoreSize = ClampPanelSize(panel, restoreSize, availableLength);
            SetPanelSize(panel, restoreSize, availableLength, updateLastSize: true);
        }

        UpdateHandlesForPanel(panel);
        InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_panels.Count == 0)
        {
            return default;
        }

        var isVertical      = Orientation == Orientation.Vertical;
        var availableLength = GetLayoutLength(availableSize);
        var availableCross  = GetCrossLength(availableSize);

        if (double.IsInfinity(availableLength))
        {
            return MeasureWithInfiniteLength(availableSize);
        }

        var sizes = ComputePanelSizes(availableLength);
        for (var i = 0; i < _panels.Count; i++)
        {
            var panel = _panels[i];
            var panelSize = sizes[i];
            var panelAvailable = isVertical
                ? new Size(panelSize, availableCross)
                : new Size(availableCross, panelSize);
            panel.Measure(panelAvailable);
        }

        foreach (var handle in _handles)
        {
            var handleAvailable = isVertical
                ? new Size(HandleSize, availableCross)
                : new Size(availableCross, HandleSize);
            handle.Measure(handleAvailable);
        }

        return availableSize;
    }

    private Size MeasureWithInfiniteLength(Size availableSize)
    {
        var isVertical     = Orientation == Orientation.Vertical;
        var availableCross = GetCrossLength(availableSize);
        var length         = 0d;
        var cross          = 0d;

        foreach (var panel in _panels)
        {
            var panelAvailable = isVertical
                ? new Size(double.PositiveInfinity, availableCross)
                : new Size(availableCross, double.PositiveInfinity);
            panel.Measure(panelAvailable);
            var desired = panel.DesiredSize;
            length += isVertical ? desired.Width : desired.Height;
            cross = Math.Max(cross, isVertical ? desired.Height : desired.Width);
        }

        length += Math.Max(0, GetEffectiveHandleCount()) * GetHandleSpacing();

        return isVertical
            ? new Size(length, double.IsInfinity(availableCross) ? cross : availableCross)
            : new Size(double.IsInfinity(availableCross) ? cross : availableCross, length);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _lastLayoutSize = finalSize;

        if (_panels.Count == 0)
        {
            return finalSize;
        }

        var isVertical      = Orientation == Orientation.Vertical;
        var availableLength = GetLayoutLength(finalSize);
        var sizes           = ComputePanelSizes(availableLength);
        var handleSpacing   = GetHandleSpacing();
        var handleOffset    = (HandleSize - handleSpacing) / 2d;

        var offset = 0d;
        for (var i = 0; i < _panels.Count; i++)
        {
            var panel = _panels[i];
            var panelSize = sizes[i];
            panel.EffectiveSize = panelSize;

            var rect = isVertical
                ? new Rect(offset, 0, panelSize, finalSize.Height)
                : new Rect(0, offset, finalSize.Width, panelSize);
            panel.Arrange(rect);
            offset += panelSize;

            if (i < _handles.Count)
            {
                var handlePosition = offset - handleOffset;
                var handleRect = isVertical
                    ? new Rect(handlePosition, 0, HandleSize, finalSize.Height)
                    : new Rect(0, handlePosition, finalSize.Width, HandleSize);
                _handles[i].Arrange(handleRect);
                _handles[i].IsVisible = IsHandleVisibleForLayout(i);
                offset += GetHandleSpacingForIndex(i, handleSpacing);
            }
        }

        UpdateHandleLayouts();
        return finalSize;
    }

    private void HandleDragStarted(object? sender, VectorEventArgs e)
    {
        if (sender is not SplitterHandle handle)
        {
            return;
        }

        if (!TryCreateBoundaryDragContext(handle, e.Vector, out var context, out var index))
        {
            index = ResolveHandleIndexForDrag(handle, e.Vector);
            context = CreateDragContext(handle, index, index, index + 1);
            if (context == null)
            {
                var fallbackIndex = FindFallbackDragIndex(index);
                if (fallbackIndex.HasValue)
                {
                    index = fallbackIndex.Value;
                    context = CreateDragContext(handle, index, index, index + 1);
                }
            }
        }
        if (context == null)
        {
            return;
        }

        _dragContext = context;

        handle.SetDragging(true);
        RaiseResizeStarted(index, BuildSizesWithDelta(context.StartPreviousSize, context.StartNextSize, 0));
    }

    private bool TryCreateBoundaryDragContext(
        SplitterHandle handle,
        Vector startVector,
        out DragContext? context,
        out int handleIndex)
    {
        context = null;
        handleIndex = handle.HandleIndex;

        if (!TryGetBoundaryForHandle(handle.HandleIndex, out var leftVisible, out var rightVisible))
        {
            return false;
        }

        if (leftVisible < 0 || rightVisible < 0 || rightVisible == leftVisible + 1)
        {
            return false;
        }

        var preferLeft = !IsPointerOnNextSide(handle, startVector);
        var leftCandidate = FindRestoreCandidateLeft(leftVisible, rightVisible);
        var rightCandidate = FindRestoreCandidateRight(leftVisible, rightVisible);

        if (!leftCandidate.HasValue && !rightCandidate.HasValue)
        {
            return false;
        }

        var candidates = preferLeft
            ? new[] { leftCandidate, rightCandidate }
            : new[] { rightCandidate, leftCandidate };

        foreach (var candidate in candidates)
        {
            if (!candidate.HasValue)
            {
                continue;
            }

            var ownerIndex = ResolveOwnerIndex(candidate.Value);
            if (ownerIndex < 0)
            {
                continue;
            }

            var previousIndex = Math.Min(ownerIndex, candidate.Value);
            var nextIndex = Math.Max(ownerIndex, candidate.Value);
            var dragContext = CreateDragContext(handle, handle.HandleIndex, previousIndex, nextIndex, false);
            if (dragContext == null)
            {
                continue;
            }

            context = dragContext;
            handleIndex = previousIndex;
            return true;
        }

        return false;
    }

    private bool IsPointerOnNextSide(SplitterHandle handle, Vector startVector)
    {
        var root = handle.GetVisualRoot() as Visual ?? handle;
        var origin = handle.TranslatePoint(new Point(0, 0), root);
        if (!origin.HasValue)
        {
            return true;
        }

        var local   = new Point(startVector.X - origin.Value.X, startVector.Y - origin.Value.Y);
        var compare = Orientation == Orientation.Vertical ? local.X : local.Y;
        var center  = Orientation == Orientation.Vertical ? handle.Bounds.Width * 0.5 : handle.Bounds.Height * 0.5;
        return compare >= center;
    }

    private int? FindFallbackDragIndex(int handleIndex)
    {
        if (handleIndex < 0 || handleIndex + 1 >= _panels.Count)
        {
            return null;
        }

        var availableLength = GetAvailablePanelLength(GetLayoutLength(_lastLayoutSize));

        if (_panels[handleIndex].IsCollapsed && handleIndex - 1 >= 0)
        {
            if (CanDragBetween(_panels[handleIndex - 1], _panels[handleIndex], availableLength))
            {
                return handleIndex - 1;
            }
        }

        if (handleIndex + 2 < _panels.Count && _panels[handleIndex + 1].IsCollapsed)
        {
            if (CanDragBetween(_panels[handleIndex + 1], _panels[handleIndex + 2], availableLength))
            {
                return handleIndex + 1;
            }
        }

        return null;
    }

    private void HandleDragDelta(object? sender, VectorEventArgs e)
    {
        if (_dragContext == null)
        {
            return;
        }

        var handle = _dragContext.Handle;
        if (handle != sender)
        {
            return;
        }

        var delta = Orientation == Orientation.Vertical ? e.Vector.X : e.Vector.Y;
        if (TrySwitchDragContext(handle, delta))
        {
            delta = 0;
        }
        var clampedDelta = Math.Clamp(delta, _dragContext.MinDelta, _dragContext.MaxDelta);
        var adjust = delta - clampedDelta;
        if (Math.Abs(adjust) > 0.001)
        {
            handle.AdjustDrag(Orientation == Orientation.Vertical
                ? new Vector(adjust, 0)
                : new Vector(0, adjust));
        }

        var previewSizes = BuildSizesWithDelta(_dragContext.StartPreviousSize, _dragContext.StartNextSize, clampedDelta);
        RaiseResizeDelta(_dragContext.HandleIndex, previewSizes);

        if (IsLazy)
        {
            _dragContext.CurrentDelta = clampedDelta;
            ApplyHandlePreview(handle, clampedDelta);
            return;
        }

        ApplyResize(_dragContext, clampedDelta);
    }

    private void HandleDragCompleted(object? sender, VectorEventArgs e)
    {
        if (_dragContext == null)
        {
            return;
        }

        var handle = _dragContext.Handle;
        if (handle != sender)
        {
            return;
        }

        var delta = IsLazy ? _dragContext.CurrentDelta :
            Math.Clamp(Orientation == Orientation.Vertical ? e.Vector.X : e.Vector.Y,
                _dragContext.MinDelta, _dragContext.MaxDelta);

        if (IsLazy)
        {
            ClearHandlePreview(handle);
            ApplyResize(_dragContext, delta);
        }

        handle.SetDragging(false);
        FinalizeResizeSizes(_dragContext);
        var completedSizes = BuildSizesWithDelta(_dragContext.StartPreviousSize, _dragContext.StartNextSize, delta);
        RaiseResizeCompleted(_dragContext.HandleIndex, completedSizes);
        _dragContext = null;
    }

    private void ApplyResize(DragContext context, double delta)
    {
        var previousSize = context.StartPreviousSize + delta;
        var nextSize = context.StartNextSize - delta;
        previousSize = ClampPanelSize(context.PreviousPanel, previousSize, context.AvailableLength);
        nextSize = ClampPanelSize(context.NextPanel, nextSize, context.AvailableLength);

        UpdateCollapseRecordForResize(context, previousSize, nextSize);

        SetPanelSize(context.PreviousPanel, previousSize, context.AvailableLength, updateLastSize: false);
        SetPanelSize(context.NextPanel, nextSize, context.AvailableLength, updateLastSize: false);

        UpdatePanelCollapsedState(context.PreviousPanel, previousSize);
        UpdatePanelCollapsedState(context.NextPanel, nextSize);

        InvalidateArrange();
    }

    private void ApplyHandlePreview(SplitterHandle handle, double delta)
    {
        handle.RenderTransform = Orientation == Orientation.Vertical
            ? new TranslateTransform(delta, 0)
            : new TranslateTransform(0, delta);
    }

    private void ClearHandlePreview(SplitterHandle handle)
    {
        handle.RenderTransform = null;
    }

    private void UpdatePanelCollapsedState(SplitterPanel panel, double size)
    {
        var collapsed = size <= 0.0;
        if (panel.IsCollapsed == collapsed)
        {
            return;
        }

        _suppressPanelChange = true;
        try
        {
            panel.SetCurrentValue(SplitterPanel.IsCollapsedProperty, collapsed);
            panel.UpdatePseudoClasses();
        }
        finally
        {
            _suppressPanelChange = false;
        }

        UpdateHandlesForPanel(panel);
    }

    private void UpdateCollapseRecordForResize(DragContext context, double newPrev, double newNext)
    {
        var previous = context.PreviousPanel;
        var next = context.NextPanel;
        var prevSize = previous.EffectiveSize;
        var nextSize = next.EffectiveSize;

        if (prevSize > 0 && newPrev <= 0)
        {
            previous.LastNonCollapsedSize = context.StartPreviousSize;
            previous.LastCollapsedIntoIndex = context.NextIndex;
        }
        else if (newPrev > 0)
        {
            previous.LastCollapsedIntoIndex = null;
        }

        if (nextSize > 0 && newNext <= 0)
        {
            next.LastNonCollapsedSize = context.StartNextSize;
            next.LastCollapsedIntoIndex = context.PreviousIndex;
        }
        else if (newNext > 0)
        {
            next.LastCollapsedIntoIndex = null;
        }
    }

    private void FinalizeResizeSizes(DragContext context)
    {
        var previous = context.PreviousPanel;
        var next = context.NextPanel;

        if (previous.EffectiveSize > 0)
        {
            previous.LastNonCollapsedSize = previous.EffectiveSize;
        }

        if (next.EffectiveSize > 0)
        {
            next.LastNonCollapsedSize = next.EffectiveSize;
        }
    }

    private void SetPanelSize(SplitterPanel panel, double size, double availableLength, bool updateLastSize)
    {
        if (double.IsNaN(size) || double.IsInfinity(size))
        {
            return;
        }

        size = Math.Max(0, size);
        var unit = panel.PreferredSizeUnit;
        Dimension dimension;
        if (unit == DimensionUnitType.Percentage && availableLength > 0)
        {
            var percentValue = Math.Clamp(size / availableLength * 100.0, 0, 100);
            dimension = new Dimension(percentValue, DimensionUnitType.Percentage);
        }
        else
        {
            dimension = new Dimension(size, DimensionUnitType.Pixel);
        }

        panel.SetCurrentValue(SplitterPanel.SizeProperty, dimension);
        panel.EffectiveSize = size;
        if (updateLastSize && size > 0)
        {
            panel.LastNonCollapsedSize = size;
        }
    }

    private double? ResolvePanelInitialSize(SplitterPanel panel, double availableLength)
    {
        if (panel.Size.HasValue)
        {
            return panel.Size.Value.Resolve(availableLength);
        }

        if (panel.DefaultSize.HasValue)
        {
            return panel.DefaultSize.Value.Resolve(availableLength);
        }

        return null;
    }

    private double GetFallbackPanelSize(SplitterPanel panel, double availableLength)
    {
        if (_panels.Count == 0)
        {
            return 0;
        }

        var remaining = availableLength;
        foreach (var other in _panels)
        {
            if (other == panel)
            {
                continue;
            }

            var size = ResolvePanelInitialSize(other, availableLength);
            if (size.HasValue)
            {
                remaining -= size.Value;
            }
        }

        if (remaining < 0)
        {
            remaining = availableLength / _panels.Count;
        }

        return remaining;
    }

    private List<double> ComputePanelSizes(double totalLength)
    {
        var handleSpace = Math.Max(0, GetEffectiveHandleCount()) * GetHandleSpacing();
        var availableLength = Math.Max(0, totalLength - handleSpace);
        var sizes = new List<double>(_panels.Count);
        var flexibleIndices = new List<int>();
        var minSizes = new double[_panels.Count];
        var maxSizes = new double[_panels.Count];

        var sumExplicit = 0d;
        for (var i = 0; i < _panels.Count; i++)
        {
            var panel = _panels[i];
            var min = GetPanelMin(panel, availableLength);
            var max = GetPanelMax(panel, availableLength);
            if (max < min)
            {
                max = min;
            }
            minSizes[i] = min;
            maxSizes[i] = max;

            if (panel.IsCollapsed)
            {
                sizes.Add(0);
                sumExplicit += 0;
                continue;
            }

            var initial = ResolvePanelInitialSize(panel, availableLength);
            if (initial.HasValue)
            {
                var clamped = Math.Clamp(initial.Value, min, max);
                sizes.Add(clamped);
                sumExplicit += clamped;
            }
            else if (panel.EffectiveSize > 0)
            {
                var clamped = Math.Clamp(panel.EffectiveSize, min, max);
                sizes.Add(clamped);
                sumExplicit += clamped;
            }
            else
            {
                sizes.Add(double.NaN);
                flexibleIndices.Add(i);
            }
        }

        var remaining = availableLength - sumExplicit;
        if (flexibleIndices.Count > 0)
        {
            var each = remaining / flexibleIndices.Count;
            foreach (var index in flexibleIndices)
            {
                sizes[index] = each;
            }
        }
        else if (_panels.Count > 0 && Math.Abs(remaining) > 0.001)
        {
            var index = _panels.Count - 1;
            sizes[index] = Math.Max(0, sizes[index] + remaining);
        }

        for (var i = 0; i < sizes.Count; i++)
        {
            if (_panels[i].IsCollapsed)
            {
                sizes[i] = 0;
                continue;
            }

            sizes[i] = Math.Clamp(sizes[i], minSizes[i], maxSizes[i]);
        }

        NormalizeSizes(sizes, minSizes, maxSizes, availableLength);
        return sizes;
    }

    private void NormalizeSizes(List<double> sizes, double[] minSizes, double[] maxSizes, double availableLength)
    {
        var delta = availableLength - sizes.Sum();
        var guard = 0;
        while (Math.Abs(delta) > 0.5 && guard < 100)
        {
            var indices = new List<int>();
            for (var i = 0; i < sizes.Count; i++)
            {
                if (_panels[i].IsCollapsed)
                {
                    continue;
                }

                if (delta > 0 && sizes[i] < maxSizes[i])
                {
                    indices.Add(i);
                }
                else if (delta < 0 && sizes[i] > minSizes[i])
                {
                    indices.Add(i);
                }
            }

            if (indices.Count == 0)
            {
                break;
            }

            var per = delta / indices.Count;
            var adjusted = 0d;

            foreach (var index in indices)
            {
                var target = sizes[index] + per;
                var newSize = delta > 0
                    ? Math.Min(target, maxSizes[index])
                    : Math.Max(target, minSizes[index]);
                var diff = newSize - sizes[index];
                sizes[index] = newSize;
                adjusted += diff;
            }

            delta -= adjusted;
            guard++;
        }
    }

    private double ClampPanelSize(SplitterPanel panel, double size, double availableLength)
    {
        var min = GetPanelMin(panel, availableLength);
        var max = GetPanelMax(panel, availableLength);
        if (max < min)
        {
            max = min;
        }
        return Math.Clamp(size, min, max);
    }

    private double GetPanelMin(SplitterPanel panel, double availableLength)
    {
        if (panel.Min.HasValue)
        {
            return Math.Max(0, panel.Min.Value.Resolve(availableLength));
        }

        return 0;
    }

    private double GetPanelMax(SplitterPanel panel, double availableLength)
    {
        if (panel.Max.HasValue)
        {
            var max = panel.Max.Value.Resolve(availableLength);
            if (max < 0)
            {
                return 0;
            }
            return max;
        }

        return double.PositiveInfinity;
    }

    private double GetLayoutLength(Size size)
    {
        return Orientation == Orientation.Vertical ? size.Width : size.Height;
    }

    private double GetCrossLength(Size size)
    {
        return Orientation == Orientation.Vertical ? size.Height : size.Width;
    }

    private double GetAvailablePanelLength(double totalLength)
    {
        if (_panels.Count <= 1)
        {
            return Math.Max(0, totalLength);
        }

        var handleSpace = Math.Max(0, GetEffectiveHandleCount()) * GetHandleSpacing();
        return Math.Max(0, totalLength - handleSpace);
    }

    private DragContext? CreateDragContext(
        SplitterHandle handle,
        int handleIndex,
        int previousIndex,
        int nextIndex,
        bool? allowSwitchOverride = null)
    {
        if (previousIndex < 0 || nextIndex >= _panels.Count || previousIndex == nextIndex)
        {
            return null;
        }

        var availableLength = GetAvailablePanelLength(GetLayoutLength(_lastLayoutSize));
        var previous = _panels[previousIndex];
        var next = _panels[nextIndex];

        if (!CanDragBetween(previous, next, availableLength))
        {
            return null;
        }

        var previousSize = previous.EffectiveSize;
        var nextSize = next.EffectiveSize;
        var prevMin = GetPanelMin(previous, availableLength);
        var prevMax = GetPanelMax(previous, availableLength);
        var nextMin = GetPanelMin(next, availableLength);
        var nextMax = GetPanelMax(next, availableLength);
        if (prevMax < prevMin)
        {
            prevMax = prevMin;
        }
        if (nextMax < nextMin)
        {
            nextMax = nextMin;
        }

        var minDelta = Math.Max(prevMin - previousSize, nextSize - nextMax);
        var maxDelta = Math.Min(prevMax - previousSize, nextSize - nextMin);
        if (minDelta > maxDelta)
        {
            return null;
        }

        var allowSwitch = allowSwitchOverride ?? (previous.IsCollapsed || next.IsCollapsed);
        return new DragContext
        {
            Handle = handle,
            HandleIndex = handleIndex,
            PreviousIndex = previousIndex,
            NextIndex = nextIndex,
            PreviousPanel = previous,
            NextPanel = next,
            StartPreviousSize = previousSize,
            StartNextSize = nextSize,
            MinDelta = minDelta,
            MaxDelta = maxDelta,
            AvailableLength = availableLength,
            AllowSwitch = allowSwitch
        };
    }

    private bool TrySwitchDragContext(SplitterHandle handle, double delta)
    {
        if (_dragContext == null || !_dragContext.AllowSwitch || Math.Abs(delta) < 0.001)
        {
            return false;
        }

        var baseIndex = handle.HandleIndex;
        var prevIndex = _dragContext.PreviousIndex;
        var nextIndex = _dragContext.NextIndex;

        var newPrevIndex = -1;
        var newNextIndex = -1;

        if (delta > 0 &&
            _dragContext.NextPanel.IsCollapsed &&
            nextIndex + 1 < _panels.Count &&
            _dragContext.MaxDelta <= 0)
        {
            newPrevIndex = nextIndex;
            newNextIndex = nextIndex + 1;
        }
        else if (delta < 0 &&
                 _dragContext.PreviousPanel.IsCollapsed &&
                 prevIndex - 1 >= 0 &&
                 _dragContext.MinDelta >= 0)
        {
            newPrevIndex = prevIndex - 1;
            newNextIndex = prevIndex;
        }

        if (newPrevIndex < 0 || newNextIndex < 0 ||
            (newPrevIndex == prevIndex && newNextIndex == nextIndex))
        {
            return false;
        }

        var context = CreateDragContext(handle, baseIndex, newPrevIndex, newNextIndex, false);
        if (context == null)
        {
            return false;
        }

        _dragContext = context;
        handle.AdjustDrag(Orientation == Orientation.Vertical
            ? new Vector(delta, 0)
            : new Vector(0, delta));

        return true;
    }


    private int ResolveHandleIndexForDrag(SplitterHandle handle, Vector startVector)
    {
        var index = handle.HandleIndex;
        if (index < 0 || index + 1 >= _panels.Count)
        {
            return index;
        }

        var root = handle.GetVisualRoot() as Visual ?? handle;
        var origin = handle.TranslatePoint(new Point(0, 0), root);
        if (!origin.HasValue)
        {
            return index;
        }

        var local      = new Point(startVector.X - origin.Value.X, startVector.Y - origin.Value.Y);
        var compare    = Orientation == Orientation.Vertical ? local.X : local.Y;
        var center     = Orientation == Orientation.Vertical ? handle.Bounds.Width * 0.5 : handle.Bounds.Height * 0.5;
        var isNextSide = compare >= center;

        if (isNextSide && index + 2 < _panels.Count && _panels[index + 1].IsCollapsed)
        {
            return index + 1;
        }

        if (!isNextSide && index - 1 >= 0 && _panels[index].IsCollapsed)
        {
            return index - 1;
        }

        return index;
    }

    private double GetHandleSpacing()
    {
        var value = TokenResourceUtils.FindTokenResource(this, SplitterTokenKey.SplitBarSize);
        if (value is double size && !double.IsNaN(size) && !double.IsInfinity(size))
        {
            return Math.Max(0, size);
        }

        return Math.Max(0, HandleSize);
    }

    private int GetEffectiveHandleCount()
    {
        if (_panels.Count <= 1)
        {
            return 0;
        }

        var count = 0;
        for (var i = 0; i < _panels.Count - 1; i++)
        {
            if (IsHandleVisibleForLayout(i))
            {
                count++;
            }
        }

        return count;
    }

    private double GetHandleSpacingForIndex(int handleIndex, double spacing)
    {
        if (handleIndex < 0 || handleIndex >= _handles.Count)
        {
            return 0;
        }

        if (!IsHandleVisibleForLayout(handleIndex))
        {
            return 0;
        }

        return spacing;
    }

    private bool IsHandleVisibleForLayout(int handleIndex)
    {
        if (handleIndex < 0 || handleIndex + 1 >= _panels.Count)
        {
            return false;
        }

        if (!TryGetBoundaryForHandle(handleIndex, out var leftVisible, out var rightVisible))
        {
            return false;
        }

        return IsPrimaryHandleForBoundary(handleIndex, leftVisible, rightVisible);
    }

    private IReadOnlyList<double> BuildSizesWithDelta(double previousSize, double nextSize, double delta)
    {
        var sizes = GetCurrentSizes().ToArray();
        if (_dragContext == null)
        {
            return sizes;
        }

        var previousIndex = _dragContext.PreviousIndex;
        var nextIndex = _dragContext.NextIndex;
        if (previousIndex < 0 || nextIndex < 0 ||
            previousIndex >= sizes.Length || nextIndex >= sizes.Length)
        {
            return sizes;
        }

        sizes[previousIndex] = previousSize + delta;
        sizes[nextIndex] = nextSize - delta;
        return sizes;
    }

    private IReadOnlyList<double> GetCurrentSizes()
    {
        return _panels.Select(p => p.EffectiveSize).ToArray();
    }

    private void RaiseResizeStarted(int handleIndex, IReadOnlyList<double> sizes)
    {
        ResizeStarted?.Invoke(this, new SplitterResizeEventArgs(handleIndex, sizes));
    }

    private void RaiseResizeDelta(int handleIndex, IReadOnlyList<double> sizes)
    {
        ResizeDelta?.Invoke(this, new SplitterResizeEventArgs(handleIndex, sizes));
    }

    private void RaiseResizeCompleted(int handleIndex, IReadOnlyList<double> sizes)
    {
        ResizeCompleted?.Invoke(this, new SplitterResizeEventArgs(handleIndex, sizes));
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Vertical, Orientation == Orientation.Vertical);
        PseudoClasses.Set(StdPseudoClass.Horizontal, Orientation == Orientation.Horizontal);
    }

    private class DragContext
    {
        public required SplitterHandle Handle { get; init; }
        public required int HandleIndex { get; init; }
        public required int PreviousIndex { get; init; }
        public required int NextIndex { get; init; }
        public required SplitterPanel PreviousPanel { get; init; }
        public required SplitterPanel NextPanel { get; init; }
        public required double StartPreviousSize { get; init; }
        public required double StartNextSize { get; init; }
        public required double MinDelta { get; init; }
        public required double MaxDelta { get; init; }
        public required double AvailableLength { get; init; }
        public bool AllowSwitch { get; set; }
        public double CurrentDelta { get; set; }
    }
}
