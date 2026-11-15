using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls;

internal readonly record struct OverlayPlacementResult(
    Point Position,
    ArrowPosition Placement,
    ArrowPosition ArrowPosition,
    double CrossAxisTranslation,
    double ArrowAnchorOffset);

/// <summary>Resolves an anchored overlay in root coordinates, without a platform popup host.</summary>
internal static class OverlayPlacementSolver
{
    private const double AnchorGap = 12;
    private const double ViewportInset = 4;
    private const double ArrowInset = 12;

    internal static OverlayPlacementResult Solve(
        Rect anchorRect,
        Size popupSize,
        Rect availableBounds,
        ArrowPosition placement,
        FlowDirection flowDirection = FlowDirection.LeftToRight)
    {
        if (flowDirection == FlowDirection.RightToLeft)
        {
            placement = PlacementGeometry.MirrorPlacement(placement);
        }

        var direction = AbstractArrowDecoratedBox.GetDirection(placement);
        var horizontal = direction is Direction.Left or Direction.Right;
        var opposite = ArrowPositionUtils.FlipArrowPosition(placement, horizontal, !horizontal);
        var position = Position(anchorRect, popupSize, placement, direction);
        if (!FitsMainAxis(position, popupSize, availableBounds, horizontal))
        {
            var flippedPosition = Position(anchorRect, popupSize, opposite, PlacementGeometry.FlipDirection(direction, horizontal, !horizontal));
            if (FitsMainAxis(flippedPosition, popupSize, availableBounds, horizontal))
            {
                (placement, opposite) = (opposite, placement);
                position = flippedPosition;
            }
        }

        var origin = horizontal ? position.Y : position.X;
        var length = horizontal ? popupSize.Height : popupSize.Width;
        var anchorStart = horizontal ? anchorRect.Top : anchorRect.Left;
        var anchorEnd = horizontal ? anchorRect.Bottom : anchorRect.Right;
        var viewportStart = horizontal ? availableBounds.Top : availableBounds.Left;
        var viewportEnd = horizontal ? availableBounds.Bottom : availableBounds.Right;
        var crossPosition = SlideAlongAnchor(origin, length, anchorStart, anchorEnd, viewportStart, viewportEnd);
        var arrowInset = Math.Min(ArrowInset, length / 2);
        var arrowOffset = Math.Clamp((anchorStart + anchorEnd) / 2 - crossPosition, arrowInset, length - arrowInset);

        position = horizontal ? new Point(position.X, crossPosition) : new Point(crossPosition, position.Y);
        return new OverlayPlacementResult(position, placement, opposite, crossPosition - origin, arrowOffset);
    }

    private static Point Position(Rect anchorRect, Size popupSize, ArrowPosition placement, Direction direction)
    {
        var (anchor, gravity) = PlacementGeometry.GetAnchorAndGravity(placement);
        var position = PlacementGeometry.Gravitate(PlacementGeometry.GetAnchorPoint(anchorRect, anchor), popupSize, gravity);
        return direction switch
        {
            Direction.Left => new Point(position.X - AnchorGap, position.Y),
            Direction.Right => new Point(position.X + AnchorGap, position.Y),
            Direction.Top => new Point(position.X, position.Y - AnchorGap),
            _ => new Point(position.X, position.Y + AnchorGap)
        };
    }

    private static bool FitsMainAxis(Point position, Size popupSize, Rect bounds, bool horizontal) =>
        horizontal ? position.X >= bounds.Left && position.X + popupSize.Width <= bounds.Right
                   : position.Y >= bounds.Top && position.Y + popupSize.Height <= bounds.Bottom;

    private static double SlideAlongAnchor(
        double origin, double length, double anchorStart, double anchorEnd, double viewportStart, double viewportEnd)
    {
        var minOrigin = viewportStart + ViewportInset;
        var maxOrigin = viewportEnd - ViewportInset - length;
        // For oversized content, align to the nearest end of the interval with least viewport overflow.
        var fitted = Math.Clamp(origin, Math.Min(minOrigin, maxOrigin), Math.Max(minOrigin, maxOrigin));
        // An offscreen anchor can make the viewport inset impossible. Preserve contact with the anchor.
        return Math.Clamp(fitted, anchorStart - length, anchorEnd);
    }
}
