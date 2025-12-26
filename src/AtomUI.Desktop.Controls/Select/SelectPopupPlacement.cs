namespace AtomUI.Desktop.Controls;

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