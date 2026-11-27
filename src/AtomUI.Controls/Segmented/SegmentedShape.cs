namespace AtomUI.Controls;

/// <summary>
/// Corner outline of the track, the thumb and the items of a Segmented control.
/// </summary>
public enum SegmentedShape
{
    /// <summary>
    /// Keeps the corner radii that the size preset of the control defines.
    /// </summary>
    Default,
    /// <summary>
    /// Replaces the corner radii of the size preset with a radius larger than half of any height, which turns the
    /// track, the thumb and the items into capsules.
    /// </summary>
    Round
}
