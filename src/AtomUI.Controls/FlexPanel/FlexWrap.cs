namespace AtomUI.Controls;

/// <summary>
/// Defines how flex items wrap when they exceed the container size
/// </summary>
public enum FlexWrap
{
    /// <summary>
    /// All items are on one line (may overflow)
    /// </summary>
    NoWrap,

    /// <summary>
    /// Items wrap onto multiple lines from top to bottom in a row direction,
    /// or from left to right in a column direction.
    /// </summary>
    Wrap,

    /// <summary>
    /// Items wrap onto multiple lines from bottom to top in a row direction,
    /// or from right to left in a column direction.
    /// </summary>
    WrapReverse
}