namespace AtomUI.Desktop.Controls;

/// <summary>
/// Preferred position of the dropdown popup relative to the selector: above or below it, with the left or
/// right edge of the popup aligned to the selector's edge on the same side.
/// </summary>
public enum SelectPopupPlacement
{
    /// <summary>
    /// Preferred location is above the target element, with the left edge of the popup aligned with the left edge of the target element.
    /// </summary>
    TopEdgeAlignedLeft,
    /// <summary>
    /// Preferred location is above the target element, with the right edge of popup aligned with right edge of the target element.
    /// </summary>
    TopEdgeAlignedRight,
    /// <summary>
    /// Preferred location is below the target element, with the left edge of popup aligned with left edge of the target element.
    /// </summary>
    BottomEdgeAlignedLeft,
    /// <summary>
    /// Preferred location is below the target element, with the right edge of popup aligned with right edge of the target element.
    /// </summary>
    BottomEdgeAlignedRight,
}