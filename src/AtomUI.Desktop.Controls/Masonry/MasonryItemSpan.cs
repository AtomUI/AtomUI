namespace AtomUI.Desktop.Controls;

/// <summary>
/// Declares how a child of <see cref="Masonry" /> occupies columns.
/// </summary>
public enum MasonryItemSpan
{
    /// <summary>
    /// The item occupies a single column. It is placed into the column assigned via
    /// <c>Masonry.Column</c> when that is set, otherwise into the current shortest column, where
    /// <see cref="MasonryLayoutStrategy.StableColumns" /> keeps the item container while the effective
    /// column count stays unchanged. This is the default behavior.
    /// </summary>
    Auto,

    /// <summary>
    /// The item occupies the full Masonry width. It is arranged after the current tallest column
    /// and advances every column height to its bottom, breaking the masonry flow rhythm.
    /// </summary>
    Full
}
