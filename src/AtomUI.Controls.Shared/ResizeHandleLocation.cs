namespace AtomUI.Controls;

/// <summary>
/// Names the edges and corners of a window or an overlay dialog that a resize handle can occupy. A corner combines
/// its two edges, so a bitwise test for an edge also matches the two corners on that edge. The resizer templates
/// store the location of each handle in the <c>Tag</c> of its border.
/// </summary>
[Flags]
public enum ResizeHandleLocation
{
    /// <summary>
    /// The handle on the top edge. Dragging it changes the height only.
    /// </summary>
    North = 0x0001,
    /// <summary>
    /// The handle on the bottom edge. Dragging it changes the height only.
    /// </summary>
    South = 0x0002,
    /// <summary>
    /// The handle on the left edge. Dragging it changes the width only.
    /// </summary>
    West = 0x0004,
    /// <summary>
    /// The handle on the right edge. Dragging it changes the width only.
    /// </summary>
    East = 0x0008,
    /// <summary>
    /// The handle in the top right corner. Dragging it changes both the width and the height.
    /// </summary>
    NorthEast = North | East,
    /// <summary>
    /// The handle in the top left corner. Dragging it changes both the width and the height.
    /// </summary>
    NorthWest = North | West,
    /// <summary>
    /// The handle in the bottom right corner. Dragging it changes both the width and the height.
    /// </summary>
    SouthEast = South | East,
    /// <summary>
    /// The handle in the bottom left corner. Dragging it changes both the width and the height.
    /// </summary>
    SouthWest = South | West,
    /// <summary>
    /// The handles on the four edges, without the corners.
    /// </summary>
    Edges = North | South | West | East,
    /// <summary>
    /// The handles in the four corners, without the edges.
    /// </summary>
    Corners = NorthEast | NorthWest | SouthEast | SouthWest,
    /// <summary>
    /// Every handle, on the edges and in the corners.
    /// </summary>
    All = Edges | Corners
}