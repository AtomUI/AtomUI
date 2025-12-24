namespace AtomUI.Desktop.Controls;

/// <summary>
/// Defines how flex items are aligned along the main axis
/// </summary>
public enum JustifyContent
{
    /// <summary>Items are packed toward the start of the flex direction</summary>
    FlexStart,

    /// <summary>Items are packed toward the end of the flex direction</summary>
    FlexEnd,

    /// <summary>Items are centered along the main axis</summary>
    Center,

    /// <summary>Items are evenly distributed with first item at start, last at end</summary>
    SpaceBetween,

    /// <summary>Items are evenly distributed with equal space around them</summary>
    SpaceAround,

    /// <summary>Items are evenly distributed with equal space between them</summary>
    SpaceEvenly
}