namespace AtomUI.Desktop.Controls;

/// <summary>
/// Reports that an <see cref="IDataGridSource" /> broke the contract the grid relies on: a request or result
/// that does not follow the schema, or loaded blocks of one snapshot that disagree.
/// </summary>
public class DataGridSourceContractException : InvalidOperationException
{
    /// <summary>
    /// Initializes the exception with a message that names the broken rule.
    /// </summary>
    public DataGridSourceContractException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes the exception with a message that names the broken rule and the exception that caused it.
    /// </summary>
    public DataGridSourceContractException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Reports that a source offers more data than the grid can present continuously: without paging, more data
/// rows than a 32-bit index, or a grouped view with more display entries than that.
/// </summary>
public class DataGridPresentationLimitExceededException : InvalidOperationException
{
    /// <summary>
    /// Initializes the exception with a message that names the limit and the amount the source reported.
    /// </summary>
    public DataGridPresentationLimitExceededException(string message)
        : base(message)
    {
    }
}

internal sealed class DataGridPresentationInvariantException : InvalidOperationException
{
    public DataGridPresentationInvariantException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Reports that a request named a snapshot the source has replaced, so answering it would mix two versions
/// of the data. A source throws it from <see cref="IDataGridSource.FetchAsync" /> when the expected snapshot
/// is no longer current; the grid then restarts the load against the current snapshot.
/// </summary>
public class DataGridSnapshotExpiredException : InvalidOperationException
{
    /// <summary>
    /// Initializes the exception with a message that names the expired and the current snapshot.
    /// </summary>
    public DataGridSnapshotExpiredException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes the exception with a message that names the expired and the current snapshot, and with the
    /// exception that caused it.
    /// </summary>
    public DataGridSnapshotExpiredException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
