namespace AtomUI.Controls;

/// <summary>
/// Placement of the items across their line. The cross axis is vertical when the items flow horizontally and
/// horizontal when they flow vertically.
/// </summary>
public enum SpaceItemsAlignment
{
    /// <summary>
    /// Each item is placed at the start of its line along the cross axis: at the top of a row, or at the
    /// left of a column.
    /// </summary>
    Start,

    /// <summary>
    /// Each item is centered along the cross axis of its line.
    /// </summary>
    Center,

    /// <summary>
    /// Each item is placed at the end of its line along the cross axis: at the bottom of a row,
    /// or at the right of a column.
    /// </summary>
    End,
}

/// <summary>
/// Distribution of the remaining main-axis space of each line.
/// </summary>
public enum SpaceJustify
{
    /// <summary>
    /// Items are packed at the start of the line.
    /// </summary>
    Start,

    /// <summary>
    /// Items are packed at the end of the line.
    /// </summary>
    End,

    /// <summary>
    /// Items are packed at the center of the line.
    /// </summary>
    Center,

    /// <summary>
    /// The remaining space is split evenly between adjacent items; a single item behaves as <see cref="Start" />.
    /// </summary>
    Between,

    /// <summary>
    /// Every item gets the same remaining space on both sides, so the outer gaps are half of the inner gaps.
    /// </summary>
    Around,

    /// <summary>
    /// The remaining space is split evenly between adjacent items and the line edges.
    /// </summary>
    Evenly,
}
