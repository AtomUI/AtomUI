namespace AtomUI.Controls;

/// <summary>
/// Determines how the selection indicator moves between items.
/// </summary>
public enum IndicatorMotionMode
{
    /// <summary>
    /// Snaps the indicator directly to the new selection without motion.
    /// </summary>
    None,

    /// <summary>
    /// Slides the indicator position smoothly to the new selection.
    /// </summary>
    Slide,

    /// <summary>
    /// Animates both indicator position and span length to match the new selection.
    /// </summary>
    Stretch
}
