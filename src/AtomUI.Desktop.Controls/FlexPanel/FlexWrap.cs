namespace AtomUI.Desktop.Controls;

/// <summary>
/// Defines how flex items wrap when they exceed the container size
/// </summary>
public enum FlexWrap
{
    /// <summary>All items are on one line (may overflow)</summary>
    NoWrap,

    /// <summary>Items wrap onto multiple lines from top to bottom</summary>
    Wrap,

    /// <summary>Items wrap onto multiple lines from bottom to top</summary>
    WrapReverse
}