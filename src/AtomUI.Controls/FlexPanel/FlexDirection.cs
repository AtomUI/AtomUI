namespace AtomUI.Controls;

/// <summary>
/// Direction of the main axis of a <see cref="FlexPanel" /> and the order in which it places the children
/// along that axis.
/// </summary>
public enum FlexDirection
{
    /// <summary>
    /// The main axis runs from left to right.
    /// </summary>
    Row,
    /// <summary>
    /// The main axis runs from right to left.
    /// </summary>
    RowReverse,
    /// <summary>
    /// The main axis runs from top to bottom.
    /// </summary>
    Column,
    /// <summary>
    /// The main axis runs from bottom to top.
    /// </summary>
    ColumnReverse
}