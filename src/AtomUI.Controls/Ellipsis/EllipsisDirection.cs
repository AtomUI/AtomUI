namespace AtomUI.Controls;

/// <summary>
/// The part of the text replaced by the ellipsis.
/// </summary>
public enum EllipsisDirection
{
    /// <summary>
    /// The ellipsis replaces the beginning of the text; the end is kept.
    /// </summary>
    Start,
    /// <summary>
    /// The ellipsis replaces the end of the text; the beginning is kept. This is the default.
    /// </summary>
    End,
    /// <summary>
    /// The ellipsis replaces the middle of the text. At most half of the text elements are kept at each end.
    /// </summary>
    Middle
}
