using AtomUI.Controls;
using AtomUI.Animations;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.LogicalTree;
using Avalonia.Platform;
using Avalonia.VisualTree;

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
        if (placement == PlacementMode.Center || placement == PlacementMode.Pointer)
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

    internal static (AbstractMotion? open, AbstractMotion? close) CreateMotionForPlacement(PlacementMode placement)
    {
        if (!IsCanonicalAnchorPlacementMode(placement))
        {
            return (null, null);
        }

        return (new ZoomBigInMotion(), new ZoomBigOutMotion());
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

    /// <summary>
    /// 判断在给定的自定义放置参数下，<c>PopupRoot</c>（独立窗口弹窗）最终会在 X / Y 轴上发生翻转。
    /// 实现与 Avalonia 的 <c>ManagedPopupPositioner.Calculate</c> 完全一致。
    /// </summary>
    /// <param name="placement">自定义放置参数（来源于 <c>Popup.CustomPopupPlacementCallback</c>）。</param>
    /// <returns>X 轴是否翻转、Y 轴是否翻转。</returns>
    internal static (bool flipX, bool flipY) CalculatePopupRootFlipInfo(CustomPopupPlacement placement)
    {
        var target = placement.Target;
        if (placement.PopupSize.Width <= 0 || placement.PopupSize.Height <= 0)
        {
            return (false, false);
        }

        var topLevel = TopLevel.GetTopLevel(target);
        var screens  = topLevel?.Screens;
        if (topLevel == null || screens == null || screens.ScreenCount == 0)
        {
            return (false, false);
        }

        // —— 与 Avalonia ManagedPopupPositioner 保持单位一致 ——
        // 关键：使用 DesktopScaling（不是 RenderScaling），原因：
        //   * Win32:  DesktopScaling == RenderScaling，整套换算到物理像素；
        //   * macOS:  DesktopScaling == 1（原生硬编码），PointToScreen 与 NSScreen.frame 都是 Cocoa 逻辑点，
        //             ManagedPopupPositionerPopupImplHelper.Scaling 用的也是 DesktopScaling，整套在逻辑点系。
        // 这样保证 anchorRect、clientScreenPos、screen.Bounds 三者单位完全一致。
        var scaling         = topLevel.PlatformImpl?.DesktopScaling ?? 1.0;
        var clientScreenPos = topLevel.PointToScreen(default);
        var anchorRectPx    = new Rect(
            placement.AnchorRectangle.X * scaling + clientScreenPos.X,
            placement.AnchorRectangle.Y * scaling + clientScreenPos.Y,
            placement.AnchorRectangle.Width  * scaling,
            placement.AnchorRectangle.Height * scaling);
        var popupSizePx = placement.PopupSize.Deflate(placement.Deflate) * scaling;
        var offsetPx    = placement.Offset    * scaling;

        // —— 选屏：与 ManagedPopupPositioner.GetBounds 优先级一致：先 anchor 所在屏，再 TopLevel 所在屏，再主屏 ——
        var anchorPixelRect = new PixelRect(
            (int)anchorRectPx.X, (int)anchorRectPx.Y,
            (int)anchorRectPx.Width, (int)anchorRectPx.Height);
        var screen = screens.ScreenFromBounds(anchorPixelRect)
                  ?? screens.ScreenFromTopLevel(topLevel)
                  ?? screens.Primary
                  ?? GetFirstScreen(screens);
        if (screen == null)
        {
            return (false, false);
        }

        var workingAreaPx = screen.WorkingArea;
        var bounds = (workingAreaPx.Width == 0 && workingAreaPx.Height == 0)
            ? screen.Bounds.ToRect(1.0)
            : workingAreaPx.ToRect(1.0);

        return CalculateFlipInfoCore(placement, anchorRectPx, bounds, popupSizePx, offsetPx);
    }

    private static Screen? GetFirstScreen(Screens screens)
    {
        var allScreens = screens.All;
        return allScreens.Count > 0 ? allScreens[0] : null;
    }

    /// <summary>
    /// 判断在给定的自定义放置参数下，<c>OverlayPopupHost</c>（Overlay 层弹窗）最终会在 X / Y 轴上发生翻转。
    /// 调整顺序与 Avalonia 的 <c>OverlayPopupHost + ManagedPopupPositioner.Calculate</c> 一致；
    /// 可用区域还会排除 AtomUI CSD 阴影和 SafeAreaPadding。
    /// </summary>
    /// <param name="placement">自定义放置参数（来源于 <c>Popup.CustomPopupPlacementCallback</c>）。</param>
    /// <returns>X 轴是否翻转、Y 轴是否翻转。</returns>
    internal static (bool flipX, bool flipY) CalculateOverlayPopupHostFlipInfo(CustomPopupPlacement placement)
    {
        var target = placement.Target;
        if (placement.PopupSize.Width <= 0 || placement.PopupSize.Height <= 0)
        {
            return (false, false);
        }

        var topLevel = TopLevel.GetTopLevel(target);
        if (topLevel == null)
        {
            return (false, false);
        }

        var bounds = GetOverlayPopupPositioningBounds(topLevel);

        return CalculateFlipInfoCore(
            placement,
            placement.AnchorRectangle,
            bounds,
            placement.PopupSize.Deflate(placement.Deflate),
            placement.Offset);
    }

    private static (bool flipX, bool flipY) CalculateFlipInfoCore(
        CustomPopupPlacement placement,
        Rect anchorRect,
        Rect bounds,
        Size popupSize,
        Point offset)
    {
        var (_, flipX, flipY) = CalculateConstrainedGeometry(
            anchorRect,
            popupSize,
            placement.Anchor,
            placement.Gravity,
            placement.ConstraintAdjustment,
            offset,
            bounds);
        return (flipX, flipY);
    }

    internal static (Rect geometry, bool flipX, bool flipY) CalculateConstrainedGeometry(
        Rect anchorRect,
        Size popupSize,
        PopupAnchor anchor,
        PopupGravity gravity,
        PopupPositionerConstraintAdjustment constraintAdjustment,
        Point offset,
        Rect bounds)
    {
        Rect GetUnconstrained(PopupAnchor a, PopupGravity g) =>
            new Rect(Gravitate(GetAnchorPoint(anchorRect, a), popupSize, g) + offset, popupSize);

        bool FitsInBounds(Rect rc, PopupAnchor edge)
        {
            if (edge.HasAllFlags(PopupAnchor.Left)   && rc.X      < bounds.X      ||
                edge.HasAllFlags(PopupAnchor.Top)    && rc.Y      < bounds.Y      ||
                edge.HasAllFlags(PopupAnchor.Right)  && rc.Right  > bounds.Right  ||
                edge.HasAllFlags(PopupAnchor.Bottom) && rc.Bottom > bounds.Bottom)
            {
                return false;
            }

            return true;
        }

        static bool IsValid(in Rect rect) => rect.Width > 0 && rect.Height > 0;

        var geo   = GetUnconstrained(anchor, gravity);
        var flipX = false;
        var flipY = false;

        if (!FitsInBounds(geo, PopupAnchor.HorizontalMask)
            && constraintAdjustment.HasAllFlags(PopupPositionerConstraintAdjustment.FlipX))
        {
            var flipped = GetUnconstrained(anchor.FlipX(), gravity.FlipX());
            if (FitsInBounds(flipped, PopupAnchor.HorizontalMask))
            {
                flipX = true;
                geo   = geo.WithX(flipped.X);
            }
        }

        if (constraintAdjustment.HasAllFlags(PopupPositionerConstraintAdjustment.SlideX))
        {
            geo = geo.WithX(Math.Max(geo.X, bounds.X));
            if (geo.Right > bounds.Right)
            {
                geo = geo.WithX(bounds.Right - geo.Width);
            }
        }

        if (constraintAdjustment.HasAllFlags(PopupPositionerConstraintAdjustment.ResizeX))
        {
            var resized = geo;
            if (!FitsInBounds(resized, PopupAnchor.Left))
            {
                resized = resized.WithX(bounds.X);
            }

            if (!FitsInBounds(resized, PopupAnchor.Right))
            {
                resized = resized.WithWidth(bounds.Width - resized.X);
            }

            if (IsValid(resized))
            {
                geo = resized;
            }
        }

        if (!FitsInBounds(geo, PopupAnchor.VerticalMask)
            && constraintAdjustment.HasAllFlags(PopupPositionerConstraintAdjustment.FlipY))
        {
            var flipped = GetUnconstrained(anchor.FlipY(), gravity.FlipY());
            if (FitsInBounds(flipped, PopupAnchor.VerticalMask))
            {
                flipY = true;
                geo   = geo.WithY(flipped.Y);
            }
        }

        if (constraintAdjustment.HasAllFlags(PopupPositionerConstraintAdjustment.SlideY))
        {
            geo = geo.WithY(Math.Max(geo.Y, bounds.Y));
            if (geo.Bottom > bounds.Bottom)
            {
                geo = geo.WithY(bounds.Bottom - geo.Height);
            }
        }

        if (constraintAdjustment.HasAllFlags(PopupPositionerConstraintAdjustment.ResizeY))
        {
            var resized = geo;
            if (!FitsInBounds(resized, PopupAnchor.Top))
            {
                resized = resized.WithY(bounds.Y);
            }

            if (!FitsInBounds(resized, PopupAnchor.Bottom))
            {
                resized = resized.WithHeight(bounds.Height - resized.Y);
            }

            if (IsValid(resized))
            {
                geo = resized;
            }
        }

        return (geo, flipX, flipY);
    }

    private static Rect GetOverlayPopupPositioningBounds(TopLevel topLevel)
    {
        var bounds = new Rect(default, topLevel.ClientSize);
        if (topLevel is Window
            {
                IsCsdEnabled: true,
                WindowState: WindowState.Normal
            } window && HasPositiveInset(window.FrameShadowThickness))
        {
            bounds = bounds.Deflate(window.FrameShadowThickness);
        }

        var padding = topLevel.InsetsManager?.SafeAreaPadding ?? default;
        if (padding != default)
        {
            bounds = bounds.Deflate(padding);
        }

        return bounds;
    }

    private static bool TryConstrainOverlayPopupToVisibleFrame(
        CustomPopupPlacement placement,
        out bool flipX,
        out bool flipY)
    {
        flipX = false;
        flipY = false;

        var topLevel = TopLevel.GetTopLevel(placement.Target);
        if (topLevel is not Window
            {
                IsCsdEnabled: true,
                WindowState: WindowState.Normal
            } window || !HasPositiveInset(window.FrameShadowThickness))
        {
            return false;
        }

        var bounds = GetOverlayPopupPositioningBounds(topLevel);
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return false;
        }

        var popupSize = placement.PopupSize.Deflate(placement.Deflate);
        var (geometry, horizontalFlipped, verticalFlipped) = CalculateConstrainedGeometry(
            placement.AnchorRectangle,
            popupSize,
            placement.Anchor,
            placement.Gravity,
            placement.ConstraintAdjustment,
            placement.Offset,
            bounds);

        // OverlayPopupHost positions against the full CSD surface. Convert the result into an
        // unconstrained, RTL-invariant placement of the deflated content rectangle.
        // Avalonia inflates that rectangle by the child margin after positioning.
        placement.AnchorRectangle      = new Rect(geometry.Position, popupSize);
        placement.Anchor               = PopupAnchor.None;
        placement.Gravity              = PopupGravity.None;
        placement.ConstraintAdjustment = PopupPositionerConstraintAdjustment.None;
        placement.Offset               = default;
        flipX                           = horizontalFlipped;
        flipY                           = verticalFlipped;
        return true;
    }

    private static bool HasPositiveInset(Thickness thickness)
    {
        return thickness.Left > 0 ||
               thickness.Top > 0 ||
               thickness.Right > 0 ||
               thickness.Bottom > 0;
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

    internal static Point CalculatePointAtCenterDelta(Control anchorTarget,
                                                      AbstractArrowDecoratedBox arrowDecoratedBox,
                                                      PlacementMode placement,
                                                      PopupAnchor? anchor,
                                                      PopupGravity? gravity)
    {
        if (!CanEnabledArrow(placement, anchor, gravity))
        {
            return default;
        }

        var arrowVertexPoint = arrowDecoratedBox.ArrowVertexPoint;
        var anchorSize = anchorTarget.Bounds.Size;
        var centerX = anchorSize.Width / 2;
        var centerY = anchorSize.Height / 2;
        var offsetX = 0d;
        var offsetY = 0d;

        if (placement == PlacementMode.TopEdgeAlignedLeft ||
            placement == PlacementMode.BottomEdgeAlignedLeft)
        {
            offsetX = centerX - arrowVertexPoint.Item1;
        }
        else if (placement == PlacementMode.TopEdgeAlignedRight ||
                 placement == PlacementMode.BottomEdgeAlignedRight)
        {
            offsetX = -(centerX - arrowVertexPoint.Item2);
        }
        else if (placement == PlacementMode.RightEdgeAlignedTop ||
                 placement == PlacementMode.LeftEdgeAlignedTop)
        {
            offsetY = centerY - arrowVertexPoint.Item1;
        }
        else if (placement == PlacementMode.RightEdgeAlignedBottom ||
                 placement == PlacementMode.LeftEdgeAlignedBottom)
        {
            offsetY = -(centerY - arrowVertexPoint.Item2);
        }

        return new Point(offsetX, offsetY);
    }
    
    internal static (double offsetX, double offsetY) CalculateShadowOffset(ArrowPosition arrowPosition, Thickness shadowThickness, Rect arrowBounds)
    {
        var shadowOffsetX = 0.0d;
        var shadowOffsetY = 0.0d;

        var arrowWidth              = arrowBounds.Width;
        var arrowHeight             = arrowBounds.Height;
        var leftResidualShadow      = Math.Max(shadowThickness.Left   - arrowWidth, 0);
        var rightResidualShadow     = Math.Max(shadowThickness.Right  - arrowWidth, 0);
        var topResidualShadow       = Math.Max(shadowThickness.Top    - arrowHeight, 0);
        var bottomResidualShadow    = Math.Max(shadowThickness.Bottom - arrowHeight, 0);
        var centeredVerticalDelta   = (shadowThickness.Bottom - shadowThickness.Top) / 2;
        var centeredHorizontalDelta = (shadowThickness.Right  - shadowThickness.Left) / 2;

        switch (arrowPosition)
        {
            case ArrowPosition.Left:
                shadowOffsetX = leftResidualShadow;
                shadowOffsetY = centeredVerticalDelta;
                break;
            case ArrowPosition.LeftEdgeAlignedTop:
                shadowOffsetX = leftResidualShadow;
                shadowOffsetY = -shadowThickness.Top;
                break;
            case ArrowPosition.LeftEdgeAlignedBottom:
                shadowOffsetX = leftResidualShadow;
                shadowOffsetY = shadowThickness.Bottom;
                break;
            case ArrowPosition.Right:
                shadowOffsetX = -rightResidualShadow;
                shadowOffsetY = centeredVerticalDelta;
                break;
            case ArrowPosition.RightEdgeAlignedTop:
                shadowOffsetX = -rightResidualShadow;
                shadowOffsetY = -shadowThickness.Top;
                break;
            case ArrowPosition.RightEdgeAlignedBottom:
                shadowOffsetX = -rightResidualShadow;
                shadowOffsetY = shadowThickness.Bottom;
                break;
            case ArrowPosition.Top:
                shadowOffsetX = centeredHorizontalDelta;
                shadowOffsetY = topResidualShadow;
                break;
            case ArrowPosition.TopEdgeAlignedLeft:
                shadowOffsetX = -shadowThickness.Left;
                shadowOffsetY = topResidualShadow;
                break;
            case ArrowPosition.TopEdgeAlignedRight:
                shadowOffsetX = shadowThickness.Right;
                shadowOffsetY = topResidualShadow;
                break;
            case ArrowPosition.Bottom:
                shadowOffsetX = centeredHorizontalDelta;
                shadowOffsetY = -bottomResidualShadow;
                break;
            case ArrowPosition.BottomEdgeAlignedLeft:
                shadowOffsetX = -shadowThickness.Left;
                shadowOffsetY = -bottomResidualShadow;
                break;
            case ArrowPosition.BottomEdgeAlignedRight:
                shadowOffsetX = shadowThickness.Right;
                shadowOffsetY = -bottomResidualShadow;
                break;
        }

        return (shadowOffsetX, shadowOffsetY);
    }

    internal static (bool flipX, bool flipY) ApplyCustomPlacement(
        CustomPopupPlacement placement,
        PlacementMode requestedPlacement,
        bool isUseOverlayHost,
        double hOffset,
        double vOffset,
        double marginToAnchor,
        Thickness shadowThickness,
        PopupAnchor anchor,
        PopupGravity gravity,
        Rect arrowIndicatorLayoutBounds = default)
    {
        placement.Anchor  = anchor;
        placement.Gravity = gravity;
        placement.Offset  = new Point(hOffset, vOffset);

        var (flipX, flipY) = isUseOverlayHost
            ? CalculateOverlayPopupHostFlipInfo(placement)
            : CalculatePopupRootFlipInfo(placement);
        if (requestedPlacement == PlacementMode.Pointer)
        {
            hOffset += marginToAnchor;
            vOffset += marginToAnchor;
            if (!isUseOverlayHost)
            {
                hOffset += shadowThickness.Left;
                vOffset += shadowThickness.Top;
            }
        }
        else if (requestedPlacement != PlacementMode.Center)
        {
            Direction? direction = null;
            if (CanEnabledArrow(requestedPlacement))
            {
                direction = GetDirection(requestedPlacement);
                direction = FlipDirection(direction.Value, flipX, flipY);
            }
            
            if (!isUseOverlayHost)
            {
                var arrowPosition = CalculateArrowPosition(requestedPlacement, anchor, gravity);
                if (arrowPosition != null)
                {
                    arrowPosition = ArrowPositionUtils.FlipArrowPosition(arrowPosition.Value, flipX, flipY);
                    
                    var (shadowOffsetX, shadowOffsetY) = CalculateShadowOffset(arrowPosition.Value,
                        shadowThickness, arrowIndicatorLayoutBounds);

                    var primaryIsHorizontal = direction is Direction.Left or Direction.Right;
                    if (primaryIsHorizontal)
                    {
                        var primaryMargin = MathUtils.AreClose(marginToAnchor, 0.0d) ? shadowOffsetX : marginToAnchor;
                        hOffset += direction == Direction.Right ? primaryMargin : -primaryMargin;
                        vOffset += shadowOffsetY;
                    }
                    else
                    {
                        var primaryMargin = MathUtils.AreClose(marginToAnchor, 0.0d) ? shadowOffsetY : marginToAnchor;
                        vOffset += direction == Direction.Bottom ? primaryMargin : -primaryMargin;
                        hOffset += shadowOffsetX;
                    }
                }
                else
                {
                    ApplyMarginToAnchor(direction, marginToAnchor, ref hOffset, ref vOffset);
                }
            }
            else
            {
                ApplyMarginToAnchor(direction, marginToAnchor, ref hOffset, ref vOffset);
            }
        }
        
        placement.Offset = new Point(hOffset, vOffset);
        if (isUseOverlayHost && TryConstrainOverlayPopupToVisibleFrame(
                placement,
                out var constrainedFlipX,
                out var constrainedFlipY))
        {
            flipX = constrainedFlipX;
            flipY = constrainedFlipY;
        }
        return (flipX, flipY);
    }

    private static void ApplyMarginToAnchor(Direction? direction, double marginToAnchor, ref double hOffset, ref double vOffset)
    {
        if (direction != null)
        {
            switch (direction)
            {
                case Direction.Left:
                    hOffset -= marginToAnchor;
                    break;
                case Direction.Right:
                    hOffset += marginToAnchor;
                    break;
                case Direction.Top:
                    vOffset -= marginToAnchor;
                    break;
                case Direction.Bottom:
                    vOffset += marginToAnchor;
                    break;
            }
        }
        else
        {
            vOffset += marginToAnchor;
            hOffset += marginToAnchor;
        }

    }

    /// <summary>
    /// 判断 <paramref name="visual"/> 是否落在 <paramref name="anchor"/> + <paramref name="popupChild"/>
    /// 组成的 popup 作用域内（含嵌套 popup）。
    /// </summary>
    /// <remarks>
    /// 用于 anchor 控件的 Click handler（过滤 popup 内部点击）和 Hover 模式的全局指针追踪
    /// （判断指针是否仍在作用域内）。Avalonia 的 popup 连接走 visual/logical/InteractiveParent
    /// 三条线叠加，单棵树检查会漏嵌套 popup 场景（popup 里开 ComboBox/Select 等）。
    /// 本方法沿 <c>popup host → Popup → PlacementTarget</c> 跳跃模拟事件路由。
    /// 详见 <c>docs/engineering/development/popup-anchor-scope.md</c>。
    /// </remarks>
    internal static bool IsVisualInPopupScope(Visual? visual, Visual anchor, Visual? popupChild)
    {
        if (visual == null)
        {
            return false;
        }

        if (visual == anchor || anchor.IsVisualAncestorOf(visual))
        {
            return true;
        }

        if (popupChild == null)
        {
            return false;
        }

        if (visual == popupChild
            || popupChild.IsLogicalAncestorOf(visual)
            || popupChild.IsVisualAncestorOf(visual))
        {
            return true;
        }

        var cursor = visual;
        var guard = 0;
        while (guard++ < 8)
        {
            var owningPopup = FindOwningPopup(cursor);
            if (owningPopup == null)
            {
                break;
            }

            var target = owningPopup.PlacementTarget
                         ?? owningPopup.FindLogicalAncestorOfType<Control>();
            if (target == null || target == cursor)
            {
                break;
            }

            if (target == anchor
                || anchor.IsVisualAncestorOf(target)
                || target == popupChild
                || popupChild.IsLogicalAncestorOf(target)
                || popupChild.IsVisualAncestorOf(target))
            {
                return true;
            }

            cursor = target;
        }

        return false;
    }

    /// <summary>
    /// 从 <paramref name="visual"/> 向上查找承载它的 <see cref="OverlayPopupHost"/> 或
    /// <see cref="PopupRoot"/>，再取对应的 <see cref="Avalonia.Controls.Primitives.Popup"/> 控件。
    /// 用于 <see cref="IsVisualInPopupScope"/> 的嵌套 popup 跳跃。
    /// </summary>
    internal static Avalonia.Controls.Primitives.Popup? FindOwningPopup(Visual visual)
    {
        Visual? host = visual.FindAncestorOfType<OverlayPopupHost>();
        if (host == null)
        {
            host = visual.FindAncestorOfType<PopupRoot>();
        }
        if (host is not ILogical logical)
        {
            return null;
        }
        // OverlayPopupHost.LogicalParent 在 Popup.Open 里通过
        // ((ISetLogicalParent)popupHost).SetParent(this) 被设成 Popup 控件本身
        return logical.LogicalParent as Avalonia.Controls.Primitives.Popup
               ?? (host as StyledElement)?.FindLogicalAncestorOfType<Avalonia.Controls.Primitives.Popup>();
    }
}
