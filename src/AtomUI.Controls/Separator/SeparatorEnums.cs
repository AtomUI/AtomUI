namespace AtomUI.Controls;

/// <summary>
/// End of a horizontal separator line that the title is placed at, or the center of the line. The position is
/// set through <c>AbstractSeparator.TitlePosition</c>.
/// </summary>
public enum SeparatorTitlePosition
{
    /// <summary>
    /// The title sits next to the left end of the line.
    /// </summary>
    Left,
    /// <summary>
    /// The title sits next to the right end of the line.
    /// </summary>
    Right,
    /// <summary>
    /// The title sits in the center of the line.
    /// </summary>
    Center
}

/// <summary>
/// Line style of a separator, set through <c>AbstractSeparator.Variant</c>.
/// </summary>
public enum SeparatorVariant
{
    /// <summary>
    /// A continuous line.
    /// </summary>
    Solid,
    /// <summary>
    /// A line of dots: dashes of 1 and gaps of 1.
    /// </summary>
    Dotted,
    /// <summary>
    /// A line of dashes: dashes of 4 and gaps of 2.
    /// </summary>
    Dashed
}