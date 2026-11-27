using AtomUI.Controls;
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
    internal static Thickness CalculateZoomMotionMargin(Size size, Point origin, double renderScaling)
    {
        // ZoomBig stays between .8 and 1. Its endpoint union bounds the entire
        // non-overshooting default curve. Round outwards to physical pixels.
        var left = Math.Max(0, -origin.X * .2);
        var top = Math.Max(0, -origin.Y * .2);
        var right = Math.Max(0, (origin.X - size.Width) * .2);
        var bottom = Math.Max(0, (origin.Y - size.Height) * .2);
        return new Thickness(Math.Ceiling(left * renderScaling) / renderScaling,
            Math.Ceiling(top * renderScaling) / renderScaling,
            Math.Ceiling(right * renderScaling) / renderScaling,
            Math.Ceiling(bottom * renderScaling) / renderScaling);
    }

    internal static ArrowPosition? CalculateArrowPosition(PlacementMode placement, PopupAnchor? anchor, PopupGravity? gravity) =>
        PlacementGeometry.CalculateArrowPosition(placement, anchor, gravity);

    internal static bool IsCanonicalAnchorPlacementMode(PlacementMode placement) =>
        PlacementGeometry.IsCanonicalAnchorPlacementMode(placement);

    internal static (PopupAnchor, PopupGravity) GetAnchorAndGravity(PlacementMode placement) =>
        PlacementGeometry.GetAnchorAndGravity(placement);

    internal static Direction FlipDirection(Direction direction, bool isHorizontalFlip, bool isVerticalFlip) =>
        PlacementGeometry.FlipDirection(direction, isHorizontalFlip, isVerticalFlip);

    /// <summary>
    /// Determines whether an arrow can be drawn for a placement. Center and Pointer never get one, and
    /// AnchorAndGravity gets one only for a pair of anchor and gravity that
    /// <c>PlacementGeometry.IsCanonicalAnchorType</c> accepts.
    /// </summary>
    /// <param name="placement">
    /// The requested placement of the popup.
    /// </param>
    /// <param name="anchor">
    /// The custom anchor. It is read only for AnchorAndGravity.
    /// </param>
    /// <param name="gravity">
    /// The custom gravity. It is read only for AnchorAndGravity.
    /// </param>
    internal static bool CanEnabledArrow(PlacementMode placement, PopupAnchor? anchor = null,
        PopupGravity? gravity = null)
    {
        if (placement == PlacementMode.Center || placement == PlacementMode.Pointer)
        {
            return false;
        }

        return PlacementGeometry.IsCanonicalAnchorType(placement, anchor, gravity);
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

    /// <summary>
    /// Determines on which axes a popup hosted in a separate window (<c>PopupRoot</c>) finally flips for the given
    /// custom placement. The decision follows Avalonia's <c>ManagedPopupPositioner.Calculate</c>.
    /// </summary>
    /// <param name="placement">
    /// The custom placement to check.
    /// </param>
    /// <returns>
    /// Whether the popup flips on the X axis and whether it flips on the Y axis. Both are
    /// <see langword="false" /> when the popup has no size or its screen cannot be determined.
    /// </returns>
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

        // Keep the same units as the Avalonia ManagedPopupPositioner.
        // DesktopScaling is used on purpose instead of RenderScaling:
        //   * Win32: DesktopScaling == RenderScaling, so the whole calculation runs in physical pixels.
        //   * macOS: DesktopScaling == 1 (hard-coded in the native layer). PointToScreen and NSScreen.frame work in
        //           Cocoa logical points, and ManagedPopupPositionerPopupImplHelper.Scaling also uses DesktopScaling,
        //           so the whole calculation runs in logical points.
        // This keeps anchorRectPx, clientScreenPos and screen.Bounds in the same unit.
        var scaling         = topLevel.PlatformImpl?.DesktopScaling ?? 1.0;
        var clientScreenPos = topLevel.PointToScreen(default);
        var anchorRectPx    = new Rect(
            placement.AnchorRectangle.X * scaling + clientScreenPos.X,
            placement.AnchorRectangle.Y * scaling + clientScreenPos.Y,
            placement.AnchorRectangle.Width  * scaling,
            placement.AnchorRectangle.Height * scaling);
        var popupSizePx = placement.PopupSize.Deflate(placement.Deflate) * scaling;
        var offsetPx    = placement.Offset    * scaling;

        // Screen selection follows the priority of ManagedPopupPositioner.GetBounds: the screen that contains the
        // anchor first, then the screen of the TopLevel, then the primary screen.
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
    /// Determines on which axes a popup hosted in an overlay (<c>OverlayPopupHost</c>) finally flips for the given
    /// custom placement. The adjustment order follows Avalonia's <c>OverlayPopupHost</c> with
    /// <c>ManagedPopupPositioner.Calculate</c>. The available area excludes the AtomUI CSD shadow and SafeAreaPadding.
    /// </summary>
    /// <param name="placement">
    /// The custom placement to check.
    /// </param>
    /// <returns>
    /// Whether the popup flips on the X axis and whether it flips on the Y axis. Both are
    /// <see langword="false" /> when the popup has no size or the top level is unknown.
    /// </returns>
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
        Rect GetUnconstrained(PopupAnchor a, PopupGravity g)
        {
            var anchorPoint = PlacementGeometry.GetAnchorPoint(anchorRect, a);
            return new Rect(PlacementGeometry.Gravitate(anchorPoint, popupSize, g) + offset, popupSize);
        }

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
            var flipped = GetUnconstrained(PlacementGeometry.FlipX(anchor), PlacementGeometry.FlipX(gravity));
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
            var flipped = GetUnconstrained(PlacementGeometry.FlipY(anchor), PlacementGeometry.FlipY(gravity));
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

                    // Main axis: the window keeps a residual shadow of "shadow thickness minus the space taken by the
                    // arrow" on the side that faces the anchor, so the content (the arrow tip) moves back toward the
                    // anchor by that distance and then keeps marginToAnchor on top of it.
                    var primaryMargin = marginToAnchor -
                                        GetAnchorSideResidualShadow(direction, shadowThickness, arrowIndicatorLayoutBounds);
                    var primaryIsHorizontal = direction is Direction.Left or Direction.Right;
                    if (primaryIsHorizontal)
                    {
                        hOffset += direction == Direction.Right ? primaryMargin : -primaryMargin;
                        vOffset += shadowOffsetY;
                    }
                    else
                    {
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

    /// <summary>
    /// Returns the residual shadow thickness on the side of a window popup that faces the anchor. It matches the
    /// effective shadow thickness of ShadowsAwareContainer: the space taken by the arrow is deducted while the arrow
    /// is visible, and the full shadow thickness is returned when the arrow is hidden or absent.
    /// </summary>
    /// <param name="direction">
    /// The side of the popup relative to the anchor, after flipping. <see langword="null" /> returns 0.
    /// </param>
    /// <param name="shadowThickness">
    /// The shadow thickness around the popup.
    /// </param>
    /// <param name="arrowIndicatorLayoutBounds">
    /// The layout bounds of the arrow indicator, or an empty rect when no arrow is shown.
    /// </param>
    internal static double GetAnchorSideResidualShadow(Direction? direction, Thickness shadowThickness, Rect arrowIndicatorLayoutBounds)
    {
        return direction switch
        {
            Direction.Top => Math.Max(shadowThickness.Bottom - arrowIndicatorLayoutBounds.Height, 0),
            Direction.Bottom => Math.Max(shadowThickness.Top - arrowIndicatorLayoutBounds.Height, 0),
            Direction.Left => Math.Max(shadowThickness.Right - arrowIndicatorLayoutBounds.Width, 0),
            Direction.Right => Math.Max(shadowThickness.Left - arrowIndicatorLayoutBounds.Width, 0),
            _ => 0
        };
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
    /// Determines whether <paramref name="visual" /> lies inside the popup scope formed by
    /// <paramref name="anchor" /> and <paramref name="popupChild" />, nested popups included.
    /// </summary>
    /// <param name="visual">
    /// The visual to test.
    /// </param>
    /// <param name="anchor">
    /// The control the popup is anchored to.
    /// </param>
    /// <param name="popupChild">
    /// The popup content, or <see langword="null" /> to test the anchor only.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the visual is the anchor or one of its visual descendants, the popup child or
    /// one of its logical or visual descendants, or lies in a nested popup whose placement target leads back to the
    /// anchor or the popup child.
    /// </returns>
    /// <remarks>
    /// Anchor controls call it from their click handlers to drop clicks inside the popup, and from the global pointer
    /// tracking of hover mode to test whether the pointer is still inside the scope.
    /// An Avalonia popup is connected through the visual tree, the logical tree and the interactive parent at the
    /// same time, so checking a single tree misses nested popups such as a ComboBox or Select opened inside a popup.
    /// The method walks from the popup host over its Popup to the PlacementTarget, at most eight levels, which
    /// mirrors the event route.
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
    /// Finds the <see cref="OverlayPopupHost" /> or <see cref="PopupRoot" /> that hosts <paramref name="visual" />
    /// and returns the <see cref="Avalonia.Controls.Primitives.Popup" /> control that owns it, or
    /// <see langword="null" /> when no owning popup is found. Used by <see cref="IsVisualInPopupScope" /> to jump
    /// across a nested popup.
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
        // Popup.Open sets the logical parent of the OverlayPopupHost to the Popup control itself, through
        // ((ISetLogicalParent)popupHost).SetParent(this).
        return logical.LogicalParent as Avalonia.Controls.Primitives.Popup
               ?? (host as StyledElement)?.FindLogicalAncestorOfType<Avalonia.Controls.Primitives.Popup>();
    }
}
