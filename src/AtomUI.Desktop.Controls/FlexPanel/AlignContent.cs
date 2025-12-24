namespace AtomUI.Desktop.Controls;

/// <summary>
/// Defines how multiple lines are aligned in the cross axis (only applies when wrapping)
/// </summary>
public enum AlignContent
{
    /// <summary>Lines are packed toward the start of the cross axis</summary>
    FlexStart,

    /// <summary>Lines are packed toward the end of the cross axis</summary>
    FlexEnd,

    /// <summary>Lines are centered along the cross axis</summary>
    Center,

    /// <summary>Lines are stretched to fill the cross axis</summary>
    Stretch,

    /// <summary>Lines are evenly distributed with first line at start, last at end</summary>
    SpaceBetween,

    /// <summary>Lines are evenly distributed with equal space around them</summary>
    SpaceAround,

    /// <summary>Lines are evenly distributed with equal space between them</summary>
    SpaceEvenly
}