using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;

namespace AtomUI.Controls;

/// <summary>Pure anchor, gravity, and placement conversions for anchored overlays.</summary>
internal static class PlacementGeometry
{
    internal static ArrowPosition? CalculateArrowPosition(
        PlacementMode placement,
        PopupAnchor? anchor,
        PopupGravity? gravity)
    {
        if (placement is PlacementMode.Center or PlacementMode.Pointer ||
            !IsCanonicalAnchorType(placement, anchor, gravity))
        {
            return null;
        }

        if (placement != PlacementMode.AnchorAndGravity)
        {
            var ret = GetAnchorAndGravity(placement);
            anchor = ret.Item1;
            gravity = ret.Item2;
        }

        ArrowPosition? arrowPosition;
        switch (anchor, gravity)
        {
            case (PopupAnchor.Bottom, PopupGravity.Bottom):
                arrowPosition = ArrowPosition.Top;
                break;
            case (PopupAnchor.Right, PopupGravity.Right):
                arrowPosition = ArrowPosition.Left;
                break;
            case (PopupAnchor.Left, PopupGravity.Left):
                arrowPosition = ArrowPosition.Right;
                break;
            case (PopupAnchor.Top, PopupGravity.Top):
                arrowPosition = ArrowPosition.Bottom;
                break;
            case (PopupAnchor.TopRight, PopupGravity.TopLeft):
                arrowPosition = ArrowPosition.BottomEdgeAlignedRight;
                break;
            case (PopupAnchor.TopLeft, PopupGravity.TopRight):
                arrowPosition = ArrowPosition.BottomEdgeAlignedLeft;
                break;
            case (PopupAnchor.BottomLeft, PopupGravity.BottomRight):
                arrowPosition = ArrowPosition.TopEdgeAlignedLeft;
                break;
            case (PopupAnchor.BottomRight, PopupGravity.BottomLeft):
                arrowPosition = ArrowPosition.TopEdgeAlignedRight;
                break;
            case (PopupAnchor.TopLeft, PopupGravity.BottomLeft):
                arrowPosition = ArrowPosition.RightEdgeAlignedTop;
                break;
            case (PopupAnchor.BottomLeft, PopupGravity.TopLeft):
                arrowPosition = ArrowPosition.RightEdgeAlignedBottom;
                break;
            case (PopupAnchor.TopRight, PopupGravity.BottomRight):
                arrowPosition = ArrowPosition.LeftEdgeAlignedTop;
                break;
            case (PopupAnchor.BottomRight, PopupGravity.TopRight):
                arrowPosition = ArrowPosition.LeftEdgeAlignedBottom;
                break;
            default:
                arrowPosition = null;
                break;
        }

        return arrowPosition;
    }

    internal static bool IsCanonicalAnchorType(PlacementMode placement, PopupAnchor? anchor, PopupGravity? gravity)
    {
        if (placement == PlacementMode.AnchorAndGravity)
        {
            switch (anchor, gravity)
            {
                case (PopupAnchor.Bottom, PopupGravity.Bottom):
                case (PopupAnchor.Right, PopupGravity.Right):
                case (PopupAnchor.Left, PopupGravity.Left):
                case (PopupAnchor.Top, PopupGravity.Top):
                case (PopupAnchor.TopRight, PopupGravity.TopLeft):
                case (PopupAnchor.TopLeft, PopupGravity.TopRight):
                case (PopupAnchor.BottomLeft, PopupGravity.BottomRight):
                case (PopupAnchor.BottomRight, PopupGravity.BottomLeft):
                case (PopupAnchor.TopLeft, PopupGravity.BottomLeft):
                case (PopupAnchor.BottomLeft, PopupGravity.TopLeft):
                case (PopupAnchor.TopRight, PopupGravity.BottomRight):
                case (PopupAnchor.BottomRight, PopupGravity.TopRight):
                    break;
                default:
                    return false;
            }
        }

        return true;
    }

    internal static bool IsCanonicalAnchorPlacementMode(PlacementMode placement)
    {
        return placement switch
        {
            PlacementMode.Bottom => true,
            PlacementMode.Right => true,
            PlacementMode.Left => true,
            PlacementMode.Top => true,
            PlacementMode.TopEdgeAlignedRight => true,
            PlacementMode.TopEdgeAlignedLeft => true,
            PlacementMode.BottomEdgeAlignedLeft => true,
            PlacementMode.BottomEdgeAlignedRight => true,
            PlacementMode.LeftEdgeAlignedTop => true,
            PlacementMode.LeftEdgeAlignedBottom => true,
            PlacementMode.RightEdgeAlignedTop => true,
            PlacementMode.RightEdgeAlignedBottom => true,
            _ => false
        };
    }

    internal static (PopupAnchor, PopupGravity) GetAnchorAndGravity(PlacementMode placement)
    {
        return placement switch
        {
            PlacementMode.Bottom => (PopupAnchor.Bottom, PopupGravity.Bottom),
            PlacementMode.Right => (PopupAnchor.Right, PopupGravity.Right),
            PlacementMode.Left => (PopupAnchor.Left, PopupGravity.Left),
            PlacementMode.Top => (PopupAnchor.Top, PopupGravity.Top),
            PlacementMode.TopEdgeAlignedRight => (PopupAnchor.TopRight, PopupGravity.TopLeft),
            PlacementMode.TopEdgeAlignedLeft => (PopupAnchor.TopLeft, PopupGravity.TopRight),
            PlacementMode.BottomEdgeAlignedLeft => (PopupAnchor.BottomLeft, PopupGravity.BottomRight),
            PlacementMode.BottomEdgeAlignedRight => (PopupAnchor.BottomRight, PopupGravity.BottomLeft),
            PlacementMode.LeftEdgeAlignedTop => (PopupAnchor.TopLeft, PopupGravity.BottomLeft),
            PlacementMode.LeftEdgeAlignedBottom => (PopupAnchor.BottomLeft, PopupGravity.TopLeft),
            PlacementMode.RightEdgeAlignedTop => (PopupAnchor.TopRight, PopupGravity.BottomRight),
            PlacementMode.RightEdgeAlignedBottom => (PopupAnchor.BottomRight, PopupGravity.TopRight),
            _ => (PopupAnchor.None, PopupGravity.None)
        };
    }

    internal static (PopupAnchor, PopupGravity) GetAnchorAndGravity(ArrowPosition placement) =>
        GetAnchorAndGravity(placement switch
        {
            ArrowPosition.Bottom => PlacementMode.Bottom,
            ArrowPosition.Right => PlacementMode.Right,
            ArrowPosition.Left => PlacementMode.Left,
            ArrowPosition.Top => PlacementMode.Top,
            ArrowPosition.TopEdgeAlignedLeft => PlacementMode.TopEdgeAlignedLeft,
            ArrowPosition.TopEdgeAlignedRight => PlacementMode.TopEdgeAlignedRight,
            ArrowPosition.BottomEdgeAlignedLeft => PlacementMode.BottomEdgeAlignedLeft,
            ArrowPosition.BottomEdgeAlignedRight => PlacementMode.BottomEdgeAlignedRight,
            ArrowPosition.LeftEdgeAlignedTop => PlacementMode.LeftEdgeAlignedTop,
            ArrowPosition.LeftEdgeAlignedBottom => PlacementMode.LeftEdgeAlignedBottom,
            ArrowPosition.RightEdgeAlignedTop => PlacementMode.RightEdgeAlignedTop,
            ArrowPosition.RightEdgeAlignedBottom => PlacementMode.RightEdgeAlignedBottom,
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        });

    internal static ArrowPosition MirrorPlacement(ArrowPosition placement) => placement switch
    {
        ArrowPosition.TopEdgeAlignedLeft => ArrowPosition.TopEdgeAlignedRight,
        ArrowPosition.TopEdgeAlignedRight => ArrowPosition.TopEdgeAlignedLeft,
        ArrowPosition.BottomEdgeAlignedLeft => ArrowPosition.BottomEdgeAlignedRight,
        ArrowPosition.BottomEdgeAlignedRight => ArrowPosition.BottomEdgeAlignedLeft,
        _ => ArrowPositionUtils.FlipArrowPosition(placement, true, false)
    };

    internal static PopupAnchor FlipX(PopupAnchor edge)
    {
        if (edge.HasAnyFlag(PopupAnchor.HorizontalMask))
        {
            edge ^= PopupAnchor.HorizontalMask;
        }

        return edge;
    }

    internal static PopupAnchor FlipY(PopupAnchor edge)
    {
        if (edge.HasAnyFlag(PopupAnchor.VerticalMask))
        {
            edge ^= PopupAnchor.VerticalMask;
        }

        return edge;
    }

    internal static PopupGravity FlipX(PopupGravity gravity)
    {
        return (PopupGravity)FlipX((PopupAnchor)gravity);
    }

    internal static PopupGravity FlipY(PopupGravity gravity)
    {
        return (PopupGravity)FlipY((PopupAnchor)gravity);
    }

    internal static Point GetAnchorPoint(Rect anchorRect, PopupAnchor edge)
    {
        double x, y;
        if (edge.HasAllFlags(PopupAnchor.Left))
        {
            x = anchorRect.X;
        }
        else if (edge.HasAllFlags(PopupAnchor.Right))
        {
            x = anchorRect.Right;
        }
        else
        {
            x = anchorRect.X + anchorRect.Width / 2;
        }

        if (edge.HasAllFlags(PopupAnchor.Top))
        {
            y = anchorRect.Y;
        }
        else if (edge.HasAllFlags(PopupAnchor.Bottom))
        {
            y = anchorRect.Bottom;
        }
        else
        {
            y = anchorRect.Y + anchorRect.Height / 2;
        }

        return new Point(x, y);
    }

    internal static Point Gravitate(Point anchorPoint, Size size, PopupGravity gravity)
    {
        double x, y;
        if (gravity.HasAllFlags(PopupGravity.Left))
        {
            x = -size.Width;
        }
        else if (gravity.HasAllFlags(PopupGravity.Right))
        {
            x = 0;
        }
        else
        {
            x = -size.Width / 2;
        }

        if (gravity.HasAllFlags(PopupGravity.Top))
        {
            y = -size.Height;
        }
        else if (gravity.HasAllFlags(PopupGravity.Bottom))
        {
            y = 0;
        }
        else
        {
            y = -size.Height / 2;
        }

        return anchorPoint + new Point(x, y);
    }

    internal static Direction FlipDirection(Direction direction, bool isHorizontalFlip, bool isVerticalFlip)
    {
        if (isHorizontalFlip)
        {
            if (direction == Direction.Left)
            {
                direction = Direction.Right;
            }
            else if (direction == Direction.Right)
            {
                direction = Direction.Left;
            }
        }

        if (isVerticalFlip)
        {
            if (direction == Direction.Top)
            {
                direction = Direction.Bottom;
            }
            else if (direction == Direction.Bottom)
            {
                direction = Direction.Top;
            }
        }
        return direction;
    }
}
