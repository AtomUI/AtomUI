namespace AtomUI.Controls;

/// <summary>
/// On which side of its node a timeline item places its content.
/// </summary>
public enum TimelineMode
{
    /// <summary>
    /// The content lies after the node, toward the end of the axis: to its right on a vertical timeline, below
    /// it on a horizontal one.
    /// </summary>
    Start,
    /// <summary>
    /// The content lies before the node, toward the start of the axis: to its left on a vertical timeline, above
    /// it on a horizontal one.
    /// </summary>
    End,
    /// <summary>
    /// The side alternates from item to item: the content of the first item lies as in <see cref="Start" />, that
    /// of the second as in <see cref="End" />, and so on. The nodes are placed in the middle of the timeline.
    /// </summary>
    Alternate
}
