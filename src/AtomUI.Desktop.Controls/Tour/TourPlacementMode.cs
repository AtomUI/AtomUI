namespace AtomUI.Desktop.Controls;

/// <summary>
/// Position of the tour popup relative to the target of a step. <see cref="Tour.Placement" /> is the default for
/// every step, and <c>TourStep.Placement</c> overrides it for a single step. A step that has no target is
/// centered, whatever this value is.
/// </summary>
public enum TourPlacementMode
{
    /// <summary>
    /// The popup is centered over the target.
    /// </summary>
    Center,
    /// <summary>
    /// The popup is placed to the left of the target.
    /// </summary>
    Left,
    /// <summary>
    /// The popup is placed to the left of the target, with its top edge aligned to the target's top edge.
    /// </summary>
    LeftTop,
    /// <summary>
    /// The popup is placed to the left of the target, with its bottom edge aligned to the target's bottom edge.
    /// </summary>
    LeftBottom,
    /// <summary>
    /// The popup is placed to the right of the target.
    /// </summary>
    Right,
    /// <summary>
    /// The popup is placed to the right of the target, with its top edge aligned to the target's top edge.
    /// </summary>
    RightTop,
    /// <summary>
    /// The popup is placed to the right of the target, with its bottom edge aligned to the target's bottom edge.
    /// </summary>
    RightBottom,
    /// <summary>
    /// The popup is placed above the target.
    /// </summary>
    Top,
    /// <summary>
    /// The popup is placed above the target, with its left edge aligned to the target's left edge.
    /// </summary>
    TopLeft,
    /// <summary>
    /// The popup is placed above the target, with its right edge aligned to the target's right edge.
    /// </summary>
    TopRight,
    /// <summary>
    /// The popup is placed below the target.
    /// </summary>
    Bottom,
    /// <summary>
    /// The popup is placed below the target, with its left edge aligned to the target's left edge.
    /// </summary>
    BottomLeft,
    /// <summary>
    /// The popup is placed below the target, with its right edge aligned to the target's right edge.
    /// </summary>
    BottomRight,
}