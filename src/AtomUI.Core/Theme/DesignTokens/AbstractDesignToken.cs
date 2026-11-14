using System.Runtime.CompilerServices;

namespace AtomUI.Theme.DesignTokens;

public abstract class AbstractDesignToken
{
    // Null means "not recording". This is the only per-instance cost of write recording: the log is
    // allocated by BeginRecordingWrites and dropped by EndRecordingWrites.
    private TokenWriteLog? _writeLog;

    /// <summary>
    /// Records that the token property <paramref name="tokenName"/> was written. Called by the generated
    /// setter of every partial token property; when recording is off it is a single null check.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void RecordWrite(string tokenName)
    {
        if (_writeLog is not null)
        {
            AddRecordedWrite(tokenName);
        }
    }

    /// <summary>
    /// Adds <paramref name="tokenName"/> to the write log. Kept out of line so that only the null check of
    /// <see cref="RecordWrite"/> is inlined into every generated setter.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddRecordedWrite(string tokenName)
    {
        _writeLog!.Add(tokenName);
    }

    /// <summary>
    /// Gets whether token property writes are currently being recorded.
    /// </summary>
    internal bool IsRecordingWrites => _writeLog is not null;

    /// <summary>
    /// Starts recording the names of the token properties written from now on. The caller must call
    /// <see cref="EndRecordingWrites"/> in a <c>finally</c> block: if the code between the two throws and recording
    /// stays on, the next call to this method throws and hides the original error.
    /// </summary>
    /// <exception cref="InvalidOperationException">Recording is already on.</exception>
    internal void BeginRecordingWrites()
    {
        if (_writeLog is not null)
        {
            throw new InvalidOperationException("Token writes are already being recorded.");
        }

        _writeLog = new TokenWriteLog();
    }

    /// <summary>
    /// Stops recording and returns the distinct names of the written token properties in first-write order.
    /// </summary>
    /// <exception cref="InvalidOperationException">Recording is not on.</exception>
    internal IReadOnlyCollection<string> EndRecordingWrites()
    {
        var log = _writeLog ?? throw new InvalidOperationException("Token writes are not being recorded.");
        _writeLog = null;
        return log.Names;
    }
}
