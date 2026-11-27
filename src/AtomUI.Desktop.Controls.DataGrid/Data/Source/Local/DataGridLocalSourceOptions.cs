namespace AtomUI.Desktop.Controls;

/// <summary>
/// Settings of a <see cref="DataGridLocalSource{T}" />: the range sizes it publishes in its schema and the
/// number of projections it caches. A value outside its range is rejected when the source is created.
/// </summary>
public sealed class DataGridLocalSourceOptions
{
    /// <summary>
    /// The range size published as <see cref="DataGridSourceSchema.PreferredRangeSize" />. The grid's range
    /// coordinator takes it as the block size of the blocks it loads, and the validation request is built from
    /// it as well. From 32 up to <see cref="MaximumRangeSize" />.
    /// </summary>
    public int PreferredRangeSize { get; init; } = 128;

    /// <summary>
    /// The largest row count one range request may ask for, published as
    /// <see cref="DataGridSourceSchema.MaximumRangeSize" />. The grid clamps the row count it derives from the
    /// viewport height to it, and a larger request is rejected. From 32 to 4096.
    /// </summary>
    public int MaximumRangeSize { get; init; } = 512;

    /// <summary>
    /// How many sorted, filtered or grouped views of the items the source keeps for reuse. When the limit is
    /// reached, the view that was unused longest is dropped. From 1 to 64.
    /// </summary>
    public int MaximumProjectionCount { get; init; } = 8;

    internal void Validate()
    {
        if (MaximumRangeSize is < 32 or > 4096)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumRangeSize));
        }
        if (PreferredRangeSize < 32 || PreferredRangeSize > MaximumRangeSize)
        {
            throw new ArgumentOutOfRangeException(nameof(PreferredRangeSize));
        }
        if (MaximumProjectionCount is < 1 or > 64)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumProjectionCount));
        }
    }
}
