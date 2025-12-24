namespace AtomUI.Desktop.Controls;

/// <summary>
/// Defines how flex items are aligned along the cross axis
/// </summary>
public enum AlignItems
{
    /// <summary>Items are packed toward the start of the cross axis</summary>
    FlexStart,

    /// <summary>Items are packed toward the end of the cross axis</summary>
    FlexEnd,

    /// <summary>Items are centered along the cross axis</summary>
    Center,

    /// <summary>Items are stretched to fill the cross axis (only if they don't have explicit size)</summary>
    Stretch
}