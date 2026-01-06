using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;

namespace AtomUI.Desktop.Controls;

internal static class PopupUtils
{
    internal static ArrowPosition? CalculateArrowPosition(
        PlacementMode placement,
        PopupAnchor? anchor,
        PopupGravity? gravity)
    {
        if (!CanEnabledArrow(placement, anchor, gravity))
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

    /// <summary>
    /// 判断是否可以启用箭头，有些组合是不能启用箭头绘制的，因为没有意义
    /// </summary>
    /// <param name="placement"></param>
    /// <param name="anchor"></param>
    /// <param name="gravity"></param>
    /// <returns></returns>
    internal static bool CanEnabledArrow(PlacementMode placement, PopupAnchor? anchor = null,
        PopupGravity? gravity = null)
    {
        if (placement == PlacementMode.Center ||
            placement == PlacementMode.Pointer)
        {
            return false;
        }

        return IsCanonicalAnchorType(placement, anchor, gravity);
    }

    /// <summary>
    /// 是否为标准的 anchor 类型
    /// </summary>
    /// <param name="placement"></param>
    /// <param name="anchor"></param>
    /// <param name="gravity"></param>
    /// <returns></returns>
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

    public static void ValidateEdge(this PopupAnchor edge)
    {
        if (edge.HasAllFlags(PopupAnchor.Left | PopupAnchor.Right) ||
            edge.HasAllFlags(PopupAnchor.Top | PopupAnchor.Bottom))
        {
            throw new ArgumentException("Opposite edges specified");
        }
    }

    public static void ValidateGravity(this PopupGravity gravity)
    {
        ValidateEdge((PopupAnchor)gravity);
    }

    public static PopupAnchor Flip(this PopupAnchor edge)
    {
        if (edge.HasAnyFlag(PopupAnchor.HorizontalMask))
        {
            edge ^= PopupAnchor.HorizontalMask;
        }

        if (edge.HasAnyFlag(PopupAnchor.VerticalMask))
        {
            edge ^= PopupAnchor.VerticalMask;
        }

        return edge;
    }

    public static PopupAnchor FlipX(this PopupAnchor edge)
    {
        if (edge.HasAnyFlag(PopupAnchor.HorizontalMask))
        {
            edge ^= PopupAnchor.HorizontalMask;
        }

        return edge;
    }

    public static PopupAnchor FlipY(this PopupAnchor edge)
    {
        if (edge.HasAnyFlag(PopupAnchor.VerticalMask))
        {
            edge ^= PopupAnchor.VerticalMask;
        }

        return edge;
    }

    public static PopupGravity FlipX(this PopupGravity gravity)
    {
        return (PopupGravity)FlipX((PopupAnchor)gravity);
    }

    public static PopupGravity FlipY(this PopupGravity gravity)
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

    internal static Direction GetDirection(PlacementMode placement)
    {
        return placement switch
        {
            PlacementMode.Left => Direction.Left,
            PlacementMode.LeftEdgeAlignedBottom => Direction.Left,
            PlacementMode.LeftEdgeAlignedTop => Direction.Left,

            PlacementMode.Top => Direction.Top,
            PlacementMode.TopEdgeAlignedLeft => Direction.Top,
            PlacementMode.TopEdgeAlignedRight => Direction.Top,

            PlacementMode.Right => Direction.Right,
            PlacementMode.RightEdgeAlignedBottom => Direction.Right,
            PlacementMode.RightEdgeAlignedTop => Direction.Right,

            PlacementMode.Bottom => Direction.Bottom,
            PlacementMode.BottomEdgeAlignedLeft => Direction.Bottom,
            PlacementMode.BottomEdgeAlignedRight => Direction.Bottom,
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, "Invalid value for PlacementMode")
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
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, "Invalid value for PlacementMode")
        };
    }
}