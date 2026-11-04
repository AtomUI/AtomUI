using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>Prepares a definition using layout values, without retaining a popup.</summary>
internal interface IPopupMotion
{
    AbstractMotion Resolve(PopupMotionGeometry geometry, RelativePoint? retainedOrigin);
}

/// <summary>Layout and menu semantics by value; no owner or running presentation is retained.</summary>
internal readonly record struct PopupMotionGeometry(
    PlacementMode Placement,
    bool HorizontalFlipped,
    bool VerticalFlipped,
    FlowDirection FlowDirection,
    Rect ContentBounds,
    Rect AnchorBounds,
    bool HasAlignment,
    double BorderRadius,
    double BorderRadiusLG,
    MenuSemanticLevel? MenuLevel = null,
    bool IsHorizontalMenu = false);

/// <summary>Stateless theme defaults. Definitions never retain a popup or owner.</summary>
internal static class PopupMotionDefaults
{
    public static readonly AbstractMotion SelectOpen = new SlideIn(GetSelectDirection);
    public static readonly AbstractMotion SelectClose = new SlideOut(GetSelectDirection);
    public static readonly AbstractMotion DropdownOpen = new SlideIn(GetDirection);
    public static readonly AbstractMotion DropdownClose = new SlideOut(GetDirection);
    public static readonly AbstractMotion HorizontalMenuOpen = new SlideIn(GetBottomDirection);
    public static readonly AbstractMotion HorizontalMenuClose = new SlideOut(GetBottomDirection);
    public static readonly AbstractMotion MenuOpen = new ZoomIn(GetMenuOrigin);
    public static readonly AbstractMotion MenuClose = new ZoomOut(GetMenuOrigin);
    public static readonly AbstractMotion NavMenuOpen = new NavMenuIn();
    public static readonly AbstractMotion NavMenuClose = new NavMenuOut();
    public static readonly AbstractMotion PopoverOpen = new ZoomIn(GetPopoverOrigin);
    public static readonly AbstractMotion PopoverClose = new ZoomOut(GetPopoverOrigin);
    public static readonly AbstractMotion TooltipOpen = new ZoomIn(GetTooltipOrigin);
    public static readonly AbstractMotion TooltipClose = new ZoomOut(GetTooltipOrigin);

    // The AXAML-visible members are genuine AbstractMotion values. Setter
    // materializes their ITemplate implementation for each style instance;
    // non-visual Flyouts explicitly use the same copy path at Style priority.
    internal static AbstractMotion Create(AbstractMotion definition) =>
        (AbstractMotion)((ITemplate)definition).Build()!;

    private static Direction GetSelectDirection(PopupMotionGeometry geometry) =>
        GetDirection(geometry) is Direction.Top ? Direction.Top : Direction.Bottom;

    private static Direction GetBottomDirection(PopupMotionGeometry geometry) => Direction.Bottom;
    private static Point GetPopoverOrigin(PopupMotionGeometry geometry) => GetAlignedOrigin(geometry, geometry.BorderRadiusLG);
    private static Point GetTooltipOrigin(PopupMotionGeometry geometry) => GetAlignedOrigin(geometry, geometry.BorderRadius);

    private static Point GetMenuOrigin(PopupMotionGeometry geometry)
    {
        if (geometry.MenuLevel == MenuSemanticLevel.None)
        {
            return geometry.ContentBounds.TopLeft;
        }
        var point = geometry.Placement switch
        {
            PlacementMode.RightEdgeAlignedTop or PlacementMode.BottomEdgeAlignedLeft => new Point(0, 0),
            PlacementMode.LeftEdgeAlignedTop or PlacementMode.BottomEdgeAlignedRight => new Point(1, 0),
            PlacementMode.LeftEdgeAlignedBottom or PlacementMode.TopEdgeAlignedRight => new Point(1, 1),
            PlacementMode.RightEdgeAlignedBottom or PlacementMode.TopEdgeAlignedLeft => new Point(0, 1),
            PlacementMode.Bottom => new Point(.5, 0),
            PlacementMode.Top => new Point(.5, 1),
            PlacementMode.Right => new Point(0, .5),
            PlacementMode.Left => new Point(1, .5),
            _ => new Point(.5, .5)
        };
        point = geometry.FlowDirection == FlowDirection.RightToLeft
            ? new Point(1, 0)
            : new Point(geometry.HorizontalFlipped ? 1 - point.X : point.X, geometry.VerticalFlipped ? 1 - point.Y : point.Y);
        return ToSurface(point, geometry.ContentBounds);
    }

    private static Direction GetDirection(PopupMotionGeometry geometry)
    {
        var direction = PopupUtils.IsCanonicalAnchorPlacementMode(geometry.Placement)
            ? PopupUtils.GetDirection(geometry.Placement) : Direction.Bottom;
        return PopupUtils.FlipDirection(direction, geometry.HorizontalFlipped, geometry.VerticalFlipped);
    }

    private static AbstractMotion CreateSlide(Direction direction, bool entering, Rect bounds)
    {
        (AbstractMotion motion, Point point) = (direction, entering) switch
        {
            (Direction.Top, true) => (new SlideDownInMotion(), new Point(1, 1)),
            (Direction.Top, false) => (new SlideDownOutMotion(), new Point(1, 1)),
            (Direction.Right, true) => (new SlideLeftInMotion(), new Point(0, 0)),
            (Direction.Right, false) => (new SlideLeftOutMotion(), new Point(0, 0)),
            (Direction.Left, true) => (new SlideRightInMotion(), new Point(1, 0)),
            (Direction.Left, false) => (new SlideRightOutMotion(), new Point(1, 0)),
            (_, true) => ((AbstractMotion)new SlideUpInMotion(), new Point(0, 0)),
            _ => (new SlideUpOutMotion(), new Point(0, 0))
        };
        motion.Origin = new RelativePoint(ToSurface(point, bounds), RelativeUnit.Absolute);
        return motion;
    }

    private static Point GetAlignedOrigin(PopupMotionGeometry geometry, double radius)
    {
        var bounds = geometry.ContentBounds;
        var point = bounds.Center;
        if (geometry.HasAlignment)
        {
            var anchor = geometry.AnchorBounds;
            var x = (Math.Max(anchor.Left, bounds.Left) + Math.Min(anchor.Right, bounds.Right)) / 2;
            var y = (Math.Max(anchor.Top, bounds.Top) + Math.Min(anchor.Bottom, bounds.Bottom)) / 2;
            var offset = radius > 12 ? radius + 2 : 12;
            if (geometry.Placement is PlacementMode.TopEdgeAlignedLeft or PlacementMode.BottomEdgeAlignedLeft)
            {
                x = geometry.HorizontalFlipped ? bounds.Right - offset : bounds.Left + offset;
            }
            else if (geometry.Placement is PlacementMode.TopEdgeAlignedRight or PlacementMode.BottomEdgeAlignedRight)
            {
                x = geometry.HorizontalFlipped ? bounds.Left + offset : bounds.Right - offset;
            }
            point = new Point(x, y);
        }
        return point;
    }

    private static Point ToSurface(Point point, Rect bounds) => new(bounds.X + point.X * bounds.Width, bounds.Y + point.Y * bounds.Height);
    private static AbstractMotion Configure(AbstractMotion source, AbstractMotion result, RelativePoint? retainedOrigin)
    {
        result.Origin = source.Origin ?? retainedOrigin ?? result.Origin;
        result.Duration = source.Duration;
        result.Delay = source.Delay;
        result.Easing = source.Easing;
        result.FillMode = source.FillMode;
        return result;
    }

    // These definitions inherit the normal built-in Build behavior. Only Popup
    // invokes their layout preparation capability; no target, observer, task or
    // request state can be retained by a theme definition or its static factory.
    private sealed class SlideIn(Func<PopupMotionGeometry, Direction> direction) : SlideUpInMotion, IPopupMotion, ITemplate
    {
        object ITemplate.Build() => Configure(this, new SlideIn(direction), null);

        public AbstractMotion Resolve(PopupMotionGeometry geometry, RelativePoint? retainedOrigin) =>
            Configure(this, CreateSlide(direction(geometry), true, geometry.ContentBounds), retainedOrigin);
    }

    private sealed class SlideOut(Func<PopupMotionGeometry, Direction> direction) : SlideUpOutMotion, IPopupMotion, ITemplate
    {
        object ITemplate.Build() => Configure(this, new SlideOut(direction), null);

        public AbstractMotion Resolve(PopupMotionGeometry geometry, RelativePoint? retainedOrigin) =>
            Configure(this, CreateSlide(direction(geometry), false, geometry.ContentBounds), retainedOrigin);
    }

    private sealed class ZoomIn(Func<PopupMotionGeometry, Point> origin) : ZoomBigInMotion, IPopupMotion, ITemplate
    {
        object ITemplate.Build() => Configure(this, new ZoomIn(origin), null);

        public AbstractMotion Resolve(PopupMotionGeometry geometry, RelativePoint? retainedOrigin) =>
            Configure(this, new ZoomBigInMotion { Origin = new RelativePoint(origin(geometry), RelativeUnit.Absolute) }, retainedOrigin);
    }

    private sealed class ZoomOut(Func<PopupMotionGeometry, Point> origin) : ZoomBigOutMotion, IPopupMotion, ITemplate
    {
        object ITemplate.Build() => Configure(this, new ZoomOut(origin), null);

        public AbstractMotion Resolve(PopupMotionGeometry geometry, RelativePoint? retainedOrigin) =>
            Configure(this, new ZoomBigOutMotion { Origin = new RelativePoint(origin(geometry), RelativeUnit.Absolute) }, retainedOrigin);
    }

    private sealed class NavMenuIn : SlideUpInMotion, IPopupMotion, ITemplate
    {
        object ITemplate.Build() => Configure(this, new NavMenuIn(), null);

        public AbstractMotion Resolve(PopupMotionGeometry geometry, RelativePoint? retainedOrigin) =>
            Configure(this, geometry.IsHorizontalMenu
                ? CreateSlide(Direction.Bottom, true, geometry.ContentBounds)
                : new ZoomBigInMotion { Origin = new RelativePoint(GetMenuOrigin(geometry), RelativeUnit.Absolute) }, retainedOrigin);
    }

    private sealed class NavMenuOut : SlideUpOutMotion, IPopupMotion, ITemplate
    {
        object ITemplate.Build() => Configure(this, new NavMenuOut(), null);

        public AbstractMotion Resolve(PopupMotionGeometry geometry, RelativePoint? retainedOrigin) =>
            Configure(this, geometry.IsHorizontalMenu
                ? CreateSlide(Direction.Bottom, false, geometry.ContentBounds)
                : new ZoomBigOutMotion { Origin = new RelativePoint(GetMenuOrigin(geometry), RelativeUnit.Absolute) }, retainedOrigin);
    }
}
