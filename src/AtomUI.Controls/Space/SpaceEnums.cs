namespace AtomUI.Controls;

public enum SpaceItemsAlignment
{
    /// <summary>
    /// Items are laid out so the first one in each column/row touches the top/left of the panel.
    /// </summary>
    Start,

    /// <summary>
    /// Items are laid out so that each column/row is centred vertically/horizontally within the panel.
    /// </summary>
    Center,

    /// <summary>
    /// Items are laid out so the last one in each column/row touches the bottom/right of the panel.
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
    /// Items are packed at the centre of the line.
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
