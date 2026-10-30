using AtomUI.Controls;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public partial class TreeView
{
    internal const double DropIntoOffsetYRange = 3d;
    internal const double DropIntoIndicatorRenderOffset = 25d;
    
    #region 内部属性定义

    internal static readonly DirectProperty<TreeView, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<TreeView, bool>(nameof(IsDragging),
            o => o.IsDragging,
            (o, v) => o.IsDragging = v);

    internal static readonly DirectProperty<TreeView, DragIndicatorRenderInfo?> DragIndicatorRenderInfoProperty =
        AvaloniaProperty.RegisterDirect<TreeView, DragIndicatorRenderInfo?>(nameof(DragIndicatorRenderInfo),
            o => o.DragIndicatorRenderInfo,
            (o, v) => o.DragIndicatorRenderInfo = v);

    internal static readonly DirectProperty<TreeView, double> DragIndicatorLineWidthProperty =
        AvaloniaProperty.RegisterDirect<TreeView, double>(nameof(DragIndicatorLineWidth),
            o => o.DragIndicatorLineWidth,
            (o, v) => o.DragIndicatorLineWidth = v);

    internal static readonly DirectProperty<TreeView, IBrush?> DragIndicatorBrushProperty =
        AvaloniaProperty.RegisterDirect<TreeView, IBrush?>(nameof(DragIndicatorBrush),
            o => o.DragIndicatorBrush,
            (o, v) => o.DragIndicatorBrush = v);

    private DragIndicatorRenderInfo? _dragIndicatorRenderInfo;

    internal DragIndicatorRenderInfo? DragIndicatorRenderInfo
    {
        get => _dragIndicatorRenderInfo;
        set => SetAndRaise(DragIndicatorRenderInfoProperty, ref _dragIndicatorRenderInfo, value);
    }

    private bool _isDragging;

    internal bool IsDragging
    {
        get => _isDragging;
        set => SetAndRaise(IsDraggingProperty, ref _isDragging, value);
    }

    private double _dragIndicatorLineWidth;

    internal double DragIndicatorLineWidth
    {
        get => _dragIndicatorLineWidth;
        set => SetAndRaise(DragIndicatorLineWidthProperty, ref _dragIndicatorLineWidth, value);
    }

    private IBrush? _dragIndicatorBrush;

    internal IBrush? DragIndicatorBrush
    {
        get => _dragIndicatorBrush;
        set => SetAndRaise(DragIndicatorBrushProperty, ref _dragIndicatorBrush, value);
    }
    
    #endregion
    
    private Point? _lastPoint;
    private TreeViewItem? _beingDraggedTreeItem;
    private DragPreviewAdorner? _dragPreview;
    private TreeViewItem? _currentDragOver; // 这个不是目标节点，有可能是在父节点上拖动
    private TreeViewItem? _dropTargetNode; // 目标释放节点
    private DropTargetInfo? _dropTargetInfo;
    private Pen? _dragIndicatorPen;
    private IBrush? _dragIndicatorPenBrush;
    private double _dragIndicatorPenLineWidth;
    
    private static void ConfigureDragAndDrop()
    {
        AffectsRender<TreeView>(DragIndicatorRenderInfoProperty,
            DragIndicatorBrushProperty,
            DragIndicatorLineWidthProperty,
            UseLayoutRoundingProperty,
            NodeHoverModeProperty);

        NodeSwitcherButton.NodeLoadRequestEvent.AddClassHandler<TreeView>((tree, args) =>
        {
            tree.HandleNodeLoadRequest(args.ViewItem);
        });
    }
    
    // 自己优先的查找，用于确认拖动发生的节点
    internal TreeViewItem? GetNodeByPositionSelfFirst(Point position)
    {
        TreeViewItem? result = null;
        for (var i = 0; i < ItemCount; i++)
        {
            var current = ContainerFromIndex(i);
            if (current is TreeViewItem currentTreeItem && currentTreeItem.IsVisible)
            {
                result = GetNodeByPositionSelfFirst(position, currentTreeItem);
            }

            if (result is not null)
            {
                break;
            }
        }

        return result;
    }

    private TreeViewItem? GetNodeByPositionSelfFirst(Point position, TreeViewItem current)
    {
        if (!IsVisibleInViewport(current))
        {
            return null;
        }

        var localPosition = this.TranslatePoint(position, current) ?? default;
        var result        = current.IsInDragHeaderBounds(localPosition) ? current : null;

        if (result is not null)
        {
            return result;
        }

        for (var i = 0; i < current.ItemCount; i++)
        {
            var child = current.ContainerFromIndex(i);
            if (child is TreeViewItem childItem)
            {
                result = GetNodeByPositionSelfFirst(position, childItem);
            }

            if (result is not null)
            {
                break;
            }
        }

        return result;
    }

    // 孩子优先的查找
    internal TreeViewItem? GetNodeByPosition(Point position)
    {
        TreeViewItem? result = null;
        for (var i = 0; i < ItemCount; i++)
        {
            var child = ContainerFromIndex(i);
            if (child is TreeViewItem childItem)
            {
                result = GetNodeByPosition(position, childItem);
            }

            if (result is not null)
            {
                break;
            }
        }

        return result;
    }

    private TreeViewItem? GetNodeByPosition(Point position, TreeViewItem current)
    {
        TreeViewItem? result = null;

        if (!IsVisibleInViewport(current))
        {
            return result;
        }

        for (var i = 0; i < current.ItemCount; i++)
        {
            var child = current.ContainerFromIndex(i);
            if (child is TreeViewItem childItem)
            {
                result = GetNodeByPosition(position, childItem);
            }

            if (result is not null)
            {
                break;
            }
        }

        var localPosition = this.TranslatePoint(position, current) ?? default;
        result ??= current.IsInDragBounds(localPosition) ? current : null;
        return result;
    }

    internal TreeViewItem? GetNodeByOffsetY(Point position)
    {
        TreeViewItem? result = null;
        for (var i = 0; i < ItemCount; i++)
        {
            var child = ContainerFromIndex(i);
            if (child is TreeViewItem childItem)
            {
                result = GetNodeByOffsetY(position, childItem);
            }

            if (result is not null)
            {
                break;
            }
        }

        return result;
    }

    private TreeViewItem? GetNodeByOffsetY(Point position, TreeViewItem current)
    {
        TreeViewItem? result = null;

        if (!IsVisibleInViewport(current))
        {
            return result;
        }

        for (var i = 0; i < current.ItemCount; i++)
        {
            var child = current.ContainerFromIndex(i);
            if (child is TreeViewItem childItem)
            {
                result = GetNodeByOffsetY(position, childItem);
            }

            if (result is not null)
            {
                break;
            }
        }

        var localPosition = this.TranslatePoint(position, current) ?? default;
        result ??= current.IsDragOverForOffsetY(localPosition) ? current : null;
        return result;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (_lastPoint.HasValue)
        {
            var delta             = e.GetPosition(this) - _lastPoint.Value;
            var manhattanDistance = Math.Abs(delta.X) + Math.Abs(delta.Y);
            if (manhattanDistance > Constants.DragThreshold)
            {
                if (!IsDragging)
                {
                    HandlePrepareDrag();
                    if (_beingDraggedTreeItem is not { } draggedItem)
                    {
                        return;
                    }
                    IsDragging = true;
                    draggedItem.IsDragging = true;
                    ItemDragStarted?.Invoke(this, new TreeViewDragStartedEventArgs(draggedItem));
                    // User callbacks may close or detach this tree and end the drag synchronously.
                    if (!IsActiveDrag(draggedItem))
                    {
                        return;
                    }
                }

                HandleDragging(e.GetPosition(this), delta);
            }
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_lastPoint.HasValue)
        {
            if (this.IsAttachedToVisualTree())
            {
                HandleDragCompleted(_lastPoint.Value);
            }
            else
            {
                FinishDrag();
            }
        }

        base.OnPointerCaptureLost(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
     
        if (_lastPoint.HasValue)
        {
            e.Handled = true;
            _lastPoint = null;
            IsDragging = false;
            HandleDragCompleted(e.GetPosition(this));
        }
    }

    private void HandlePrepareDrag()
    {
        _beingDraggedTreeItem = GetNodeByPositionSelfFirst(_lastPoint!.Value);
        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (adornerLayer == null || _beingDraggedTreeItem is null)
        {
            return;
        }

        _dragPreview = _beingDraggedTreeItem.BuildPreviewAdorner();
        AdornerLayer.SetAdornedElement(_dragPreview, TopLevel.GetTopLevel(this));
        AdornerLayer.SetIsClipEnabled(_dragPreview, false);
        adornerLayer.Children.Add(_dragPreview);
    }

    private void HandleDragging(Point position, Point delta)
    {
        if (_dragPreview is not null && _beingDraggedTreeItem is { } draggedItem)
        {
            var basePosition = _beingDraggedTreeItem.TranslatePoint(new Point(0, 0), TopLevel.GetTopLevel(this)!) ??
                               default;
            _dragPreview.OffsetX = basePosition.X + delta.X;
            _dragPreview.OffsetY = basePosition.Y + delta.Y;
            SetupDragOver(position);
            if (!IsActiveDrag(draggedItem))
            {
                return;
            }
            if (_currentDragOver is not null)
            {
                _dropTargetNode = GetNodeByOffsetY(position);
            }

            SetupDragIndicatorRenderInfo(in position);
        }
    }

    // 正常一个有效的拖动，应该 _currentDragOver 不为空
    private void SetupDragIndicatorRenderInfo(in Point position)
    {
        if (_currentDragOver is null ||
            _beingDraggedTreeItem is null ||
            _beingDraggedTreeItem is null ||
            _currentDragOver == _beingDraggedTreeItem ||
            _dropTargetNode == _beingDraggedTreeItem)
        {
            _dropTargetInfo         = null;
            DragIndicatorRenderInfo = null;
            return;
        }
        
        var effectiveDropTarget = _currentDragOver;
        if (_dropTargetNode is not null)
        {
            effectiveDropTarget = _dropTargetNode;
        }

        if (IsDescendantNodeOf(_beingDraggedTreeItem, effectiveDropTarget))
        {
            _dropTargetInfo         = null;
            DragIndicatorRenderInfo = null;
            return;
        }
        
        _dropTargetInfo        = new DropTargetInfo();
        _dropTargetInfo.IsRoot = false;

        var effectiveDragHeaderLocalBounds = effectiveDropTarget.GetDragBounds();
        var effectiveDragHeaderLocalOffset = effectiveDragHeaderLocalBounds.Position;
        var effectiveDragHeaderBounds = new Rect(
            effectiveDropTarget.TranslatePoint(effectiveDragHeaderLocalOffset, this) ?? effectiveDragHeaderLocalOffset,
            effectiveDragHeaderLocalBounds.Size);

        var   effectiveDragHeaderOffset = effectiveDragHeaderBounds.Position;
        var   dropTargetHalfOffsetY     = effectiveDragHeaderOffset.Y + effectiveDragHeaderBounds.Height / 2;
        var   offsetY                   = position.Y;
        Point startPoint                = default;
        Point endPoint                  = default;
        var   offsetYDelta              = DragIndicatorLineWidth / 2;

        var minOffsetY = DragIndicatorLineWidth / 2;
        var maxOffsetY = Bounds.Height - DragIndicatorLineWidth / 2;

        var effectiveIndex = 0;
        if (effectiveDropTarget.Parent is TreeViewItem parentItem)
        {
            effectiveIndex                 = parentItem.IndexFromContainer(effectiveDropTarget);
            _dropTargetInfo.TargetTreeItem = parentItem;
        }
        else if (effectiveDropTarget.Level == 0)
        {
            effectiveIndex = IndexFromContainer(effectiveDropTarget);
        }

        _dropTargetInfo.IsRoot = effectiveDropTarget.Level == 0;
        
        if (MathUtils.LessThan(offsetY, dropTargetHalfOffsetY - DropIntoOffsetYRange))
        {
            _dropTargetInfo.Index = effectiveIndex;
            startPoint            = effectiveDragHeaderBounds.TopLeft;
            endPoint              = effectiveDragHeaderBounds.TopRight;
            startPoint            = startPoint.WithY(Math.Max(startPoint.Y - offsetYDelta, minOffsetY));
            endPoint              = endPoint.WithY(Math.Max(endPoint.Y - offsetYDelta, minOffsetY));
        }
        else if (MathUtils.GreaterThan(offsetY, dropTargetHalfOffsetY - DropIntoOffsetYRange) &&
                 MathUtils.LessThan(offsetY, dropTargetHalfOffsetY + DropIntoOffsetYRange))
        {
            startPoint = effectiveDragHeaderBounds.BottomLeft.WithX(effectiveDragHeaderBounds.Left + DropIntoIndicatorRenderOffset);
            endPoint = effectiveDragHeaderBounds.BottomRight;
            startPoint = startPoint.WithY(Math.Min(startPoint.Y + offsetYDelta, maxOffsetY));
            endPoint = endPoint.WithY(Math.Min(endPoint.Y + offsetYDelta, maxOffsetY));
            _dropTargetInfo.Index = effectiveDropTarget.ItemCount;
            _dropTargetInfo.TargetTreeItem = effectiveDropTarget;
            _dropTargetInfo.IsRoot = false;
        }
        else
        {
            _dropTargetInfo.Index = effectiveIndex + 1;
            startPoint            = effectiveDragHeaderBounds.BottomLeft;
            endPoint              = effectiveDragHeaderBounds.BottomRight;
            startPoint            = startPoint.WithY(Math.Max(startPoint.Y + offsetYDelta, minOffsetY));
            endPoint              = endPoint.WithY(Math.Max(endPoint.Y + offsetYDelta, minOffsetY));
        }

        DragIndicatorRenderInfo = new DragIndicatorRenderInfo
        {
            TargetTreeItem = effectiveDropTarget,
            StartPoint     = startPoint,
            EndPoint       = endPoint
        };
    }

    private bool IsActiveDrag(TreeViewItem item) =>
        IsDragging && _beingDraggedTreeItem == item && this.IsAttachedToVisualTree();

    private void SetupDragOver(Point position)
    {
        if (_beingDraggedTreeItem is not { } draggedItem || !IsActiveDrag(draggedItem))
        {
            return;
        }

        var nextItem = GetNodeByPosition(position);
        if (_currentDragOver == nextItem)
        {
            if (nextItem is not null)
            {
                nextItem.IsDragOver = true;
                if (IsActiveDrag(draggedItem))
                {
                    ItemDragOver?.Invoke(this, new TreeViewDragOverEventArgs(draggedItem, nextItem));
                }
            }
            return;
        }

        var previousItem = _currentDragOver;
        _currentDragOver = null;
        if (previousItem is not null)
        {
            previousItem.IsDragOver = false;
            if (!IsActiveDrag(draggedItem))
            {
                return;
            }
            ItemDragLeave?.Invoke(this, new TreeViewDragLeaveEventArgs(draggedItem, previousItem));
            if (!IsActiveDrag(draggedItem))
            {
                return;
            }
        }

        if (nextItem is not null && nextItem.IsAttachedToVisualTree() && nextItem.OwnerTreeView == this)
        {
            _currentDragOver = nextItem;
            nextItem.IsDragOver = true;
            if (IsActiveDrag(draggedItem))
            {
                ItemDragEnter?.Invoke(this, new TreeViewDragEnterEventArgs(draggedItem, nextItem));
            }
        }
    }

    private void HandleDragCompleted(Point point)
    {
        PerformDropOperation();
        FinishDrag();
    }

    private void FinishDrag()
    {
        var preview = _dragPreview;
        var draggedItem = _beingDraggedTreeItem;
        var dragOverItem = _currentDragOver;
        _lastPoint = null;
        IsDragging = false;
        _dragPreview = null;
        _beingDraggedTreeItem = null;
        _currentDragOver = null;
        _dropTargetNode = null;
        _dropTargetInfo = null;
        DragIndicatorRenderInfo = null;

        // The tree may already be detached. Release the preview through its actual owner,
        // rather than rediscovering an adorner layer through the tree's former visual root.
        if (preview?.GetVisualParent() is Panel layer)
        {
            layer.Children.Remove(preview);
        }
        if (dragOverItem is not null)
        {
            dragOverItem.IsDragOver = false;
        }
        if (draggedItem is not null)
        {
            draggedItem.IsDragging = false;
            ItemDragCompleted?.Invoke(this, new TreeViewDragCompletedEventArgs(draggedItem));
        }
    }

    private void PerformDropOperation()
    {
        if (_dropTargetInfo is null || 
            _beingDraggedTreeItem is null || 
            (_dropTargetInfo.TargetTreeItem != null && IsDescendantNodeOf(_beingDraggedTreeItem, _dropTargetInfo.TargetTreeItem)))
        {
            return;
        }

        if (ItemsSource is not null)
        {
            if (_treeDataController.TryMove(_beingDraggedTreeItem, _dropTargetInfo, out var dropResult))
            {
                ItemDropped?.Invoke(this, new TreeViewDroppedEventArgs(
                    dropResult.DraggedViewItem,
                    dropResult.DroppedItem,
                    dropResult.DropIndex));
            }
            return;
        }

        PerformContainerDropOperation();
    }

    private void PerformContainerDropOperation()
    {
        if (_beingDraggedTreeItem is not { } draggedItem || _dropTargetInfo is not { } targetInfo)
        {
            return;
        }

        // Removing the source can detach its container and end the drag. Keep the committed
        // move independent of UI tracking, and publish ItemDropped only after insertion.
        object? sourceItem = null;
        var sourceParent = draggedItem.Parent;
        var sourceIsRoot = false;
        if (sourceParent is TreeViewItem parentItem)
        {
            sourceItem = parentItem.ItemFromContainer(draggedItem);
            if (sourceItem is not null)
            {
                parentItem.Items.Remove(sourceItem);
            }
        }
        else if (draggedItem.Level == 0)
        {
            sourceItem = ItemFromContainer(draggedItem);
            sourceIsRoot = true;
            if (sourceItem is not null)
            {
                Items.Remove(sourceItem);
            }
        }

        if (sourceItem is null)
        {
            return;
        }

        if (targetInfo.IsRoot)
        {
            var indexDelta = sourceIsRoot ? 1 : 0;
            Items.Insert(Math.Max(targetInfo.Index - indexDelta, 0), sourceItem);
            ItemDropped?.Invoke(this, new TreeViewDroppedEventArgs(draggedItem, null, targetInfo.Index));
        }
        else if (targetInfo.TargetTreeItem is { } targetItem)
        {
            var indexDelta = targetItem == sourceParent ? 1 : 0;
            targetItem.Items.Insert(Math.Max(targetInfo.Index - indexDelta, 0), sourceItem);
            ItemDropped?.Invoke(this, new TreeViewDroppedEventArgs(draggedItem, targetItem, targetInfo.Index));
        }
    }

    private bool IsVisibleInViewport(TreeViewItem viewItem)
    {
        // 先判断是否展开
        if (viewItem.Level > 0)
        {
            var isExpanded  = true;
            var currentItem = viewItem.Parent as TreeViewItem;
            while (currentItem is not null)
            {
                if (!currentItem.IsExpanded)
                {
                    isExpanded = false;
                    break;
                }

                currentItem = currentItem.Parent as TreeViewItem;
            }

            if (!isExpanded)
            {
                return false;
            }
        }

        var dragBounds   = viewItem.GetDragBounds();
        var offset       = viewItem.TranslatePoint(dragBounds.Position, this) ?? default;
        var targetBounds = new Rect(offset, dragBounds.Size);
        return new Rect(Bounds.Size).Contains(targetBounds);
    }

    private bool IsDescendantNodeOf(TreeViewItem parentViewItem, TreeViewItem descendantViewItem)
    {
        TreeViewItem? target = descendantViewItem;
        while (target != null)
        {
            if (target == parentViewItem)
            {
                return true;
            }
            target = target.Parent as TreeViewItem;
        }
        return false;
    }
    
    public override void Render(DrawingContext context)
    {
        if (IsDragging && _dragIndicatorRenderInfo is not null)
        {
            {
                using var state = context.PushRenderOptions(new RenderOptions
                {
                    EdgeMode = EdgeMode.Aliased
                });
                context.DrawLine(GetDragIndicatorPen(), _dragIndicatorRenderInfo.StartPoint, _dragIndicatorRenderInfo.EndPoint);
            }
        }
    }

    private Pen GetDragIndicatorPen()
    {
        var dragIndicatorLineWidth = BorderUtils.BuildRenderScaleAwareThickness(this, DragIndicatorLineWidth);
        if (_dragIndicatorPen is null ||
            !ReferenceEquals(_dragIndicatorPenBrush, DragIndicatorBrush) ||
            !MathUtils.AreClose(_dragIndicatorPenLineWidth, dragIndicatorLineWidth))
        {
            _dragIndicatorPenBrush     = DragIndicatorBrush;
            _dragIndicatorPenLineWidth = dragIndicatorLineWidth;
            _dragIndicatorPen          = new Pen(DragIndicatorBrush, dragIndicatorLineWidth);
        }

        return _dragIndicatorPen;
    }
    
}

internal class DropTargetInfo
{
    public TreeViewItem? TargetTreeItem { get; set; }
    public int Index { get; set; }
    public bool IsRoot { get; set; }
}

internal record DragIndicatorRenderInfo
{
    public TreeViewItem? TargetTreeItem { get; set; }
    public Point StartPoint { get; set; }
    public Point EndPoint { get; set; }
}
