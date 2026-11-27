namespace AtomUI.Controls;

/// <summary>
/// Numeric limits that controls in different packages share.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The distance in device-independent pixels that the pointer must move away from the point where it was pressed
    /// before a control starts a drag. A smaller movement is treated as part of a click.
    /// </summary>
    public const int DragThreshold = 5;
}