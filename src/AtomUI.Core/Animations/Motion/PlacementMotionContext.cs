using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Animations;

/// <summary>
/// Captures the geometry and placement decisions of one popup or overlay playback.
/// </summary>
public readonly record struct PlacementMotionContext(
    PlacementMode Placement, bool HorizontalFlipped, bool VerticalFlipped, FlowDirection FlowDirection,
    Rect ContentBounds, Rect AnchorBounds, bool HasAlignment, double SurfaceCornerRadius,
    bool IsRootMenu = false, bool IsHorizontalMenu = false, Point? TriggerOrigin = null);

/// <summary>
/// Allows a motion definition to use the actual placement without holding the owner.
/// </summary>
public interface IPlacementAwareMotion
{
    /// <summary>
    /// Indicates whether entry motion must wait for positioned anchor bounds before starting.
    /// </summary>
    bool RequiresPositionedAnchorForEntry => false;

    /// <summary>
    /// Resolves the concrete motion definition for the supplied placement geometry and optional retained origin.
    /// </summary>
    AbstractMotion Resolve(PlacementMotionContext geometry, RelativePoint? retainedOrigin);
}
