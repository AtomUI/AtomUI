namespace AtomUI.Desktop.Controls;

/// <summary>
/// Declares how <see cref="Masonry" /> assigns items to columns.
/// </summary>
public enum MasonryLayoutStrategy
{
    /// <summary>
    /// Assigns automatic items by shortest column for the first arranged layout, then keeps
    /// existing item containers in their committed columns while the effective column count
    /// stays unchanged. New item containers are placed in the current shortest column.
    /// </summary>
    StableColumns,

    /// <summary>
    /// Recomputes every automatic item from the current shortest column on each layout calculation.
    /// </summary>
    Reflow
}
