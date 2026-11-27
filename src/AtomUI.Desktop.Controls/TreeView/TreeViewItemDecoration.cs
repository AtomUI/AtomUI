using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Renders the stock tree header background and connection lines inside its template.
/// </summary>
public sealed class TreeViewItemDecoration : Control
{
    private readonly BorderRenderHelper _borderRenderHelper = new();
    private Pen? _treeNodeLinePen;
    private IBrush? _treeNodeLinePenBrush;
    private double _treeNodeLinePenWidth;

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        if (TemplatedParent is not TreeViewItem item || item.HeaderForDecoration is not { } header)
        {
            return;
        }
        if (item.NodeHoverMode == TreeItemHoverMode.WholeLine)
        {
            using var state = context.PushTransform(Matrix.CreateTranslation(header.Bounds.X, 0));
            _borderRenderHelper.Render(context,
                header.Bounds.Size,
                new Thickness(),
                item.CornerRadius,
                BackgroundSizing.InnerBorderEdge,
                item.Background,
                null);
        }

        if (item.IsShowLine)
        {
            RenderTreeNodeLine(context, item, header);
        }
    }

    private void RenderTreeNodeLine(DrawingContext context, TreeViewItem item, TreeViewItemHeader header)
    {
        var penWidth = BorderUtils.BuildRenderScaleAwareThickness(item, item.BorderThickness.Top);
        var linePen = GetTreeNodeLinePen(item.BorderBrush, penWidth);
        using var state = context.PushRenderOptions(new RenderOptions
        {
            EdgeMode = EdgeMode.Aliased
        });

        var isLastChild = false;
        if (item.Parent is ItemsControl parentTreeItem)
        {
            if (parentTreeItem.ContainerFromIndex(parentTreeItem.ItemCount - 1) == item)
            {
                isLastChild = true;
            }
        }

        var switcherButtonRect = header.SwitcherButtonRect(item);

        if (!item.IsLeaf && !isLastChild && item.AreChildrenVisibleForDecoration)
        {
            var switcherMiddleBottom = new Point(switcherButtonRect.X + switcherButtonRect.Width / 2, switcherButtonRect.Bottom);
            var blockStartPoint = new Point(switcherMiddleBottom.X, switcherMiddleBottom.Y);
            var blockEndPoint   = new Point(blockStartPoint.X, item.DesiredSize.Height);
            context.DrawLine(linePen, blockStartPoint, blockEndPoint);
        }

        // Guide lines of a leaf node. They are drawn only while IsShowLeafIcon is off.
        if (!item.IsShowLeafIcon && item.IsLeaf)
        {
            {
                // Vertical line through the switcher column, cut short for the last child.
                var childStartPoint = new Point(switcherButtonRect.X + switcherButtonRect.Width / 2, switcherButtonRect.Top);
                var childEndPoint = new Point(switcherButtonRect.X + switcherButtonRect.Width / 2, isLastChild ? switcherButtonRect.Bottom : switcherButtonRect.Top + item.DesiredSize.Height);
                if (isLastChild)
                {
                    childEndPoint = childEndPoint.WithY(childEndPoint.Y / 2);
                }

                context.DrawLine(linePen, childStartPoint.WithY(0), childEndPoint);
            }

            {
                // Horizontal connector from the middle of the switcher to its right edge.
                var childStartPoint =  new Point(switcherButtonRect.X + switcherButtonRect.Width / 2 - penWidth / 2, switcherButtonRect.Top +  switcherButtonRect.Height / 2 - penWidth / 2);
                var childEndPoint = new Point(switcherButtonRect.Right, switcherButtonRect.Top +  switcherButtonRect.Height / 2 - penWidth / 2);

                context.DrawLine(linePen, childStartPoint, childEndPoint);
            }
        }
    }

    private Pen GetTreeNodeLinePen(IBrush? lineBrush, double penWidth)
    {
        if (_treeNodeLinePen is null ||
            !ReferenceEquals(_treeNodeLinePenBrush, lineBrush) ||
            !MathUtils.AreClose(_treeNodeLinePenWidth, penWidth))
        {
            _treeNodeLinePenBrush = lineBrush;
            _treeNodeLinePenWidth = penWidth;
            _treeNodeLinePen      = new Pen(lineBrush, penWidth);
        }

        return _treeNodeLinePen!;
    }

}
