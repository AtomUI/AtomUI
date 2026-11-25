using System.Collections.Immutable;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Describes an inline edit that closed its editors but could not be committed to the data source.
/// </summary>
public class DataGridEditCommitFailedEventArgs : EventArgs
{
    public DataGridEditCommitFailedEventArgs(
        DataGridRowKey rowKey,
        ImmutableDictionary<DataGridFieldId, DataGridScalar> values,
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(exception);
        RowKey    = rowKey;
        Values    = values;
        Exception = exception;
    }

    /// <summary>
    /// The key of the edited row.
    /// </summary>
    public DataGridRowKey RowKey { get; }

    /// <summary>
    /// The edited field values that were not committed.
    /// </summary>
    public ImmutableDictionary<DataGridFieldId, DataGridScalar> Values { get; }

    /// <summary>
    /// The failure reported by the data source, or the reason the grid could not submit the edit.
    /// </summary>
    public Exception Exception { get; }
}
