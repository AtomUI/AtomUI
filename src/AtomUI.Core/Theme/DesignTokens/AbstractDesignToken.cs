using System.Runtime.CompilerServices;

namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// The base of every token object: the global token, a language token and a control token. A token object exists only
/// while a theme is evaluated; rendering and resource lookup read the frozen value tables the compiler builds from it.
/// </summary>
public abstract class AbstractDesignToken
{
    // Null while nothing is frozen: then the check in every generated setter is a single null test.
    private TokenFreezeSet? _freeze;

    // Null unless the compiler records what a control derivation reads (spec 12.3); then every getter adds its name.
    private HashSet<string>? _reads;

    /// <summary>
    /// Called by the generated getter of every partial token property. While the compiler records a derivation's read
    /// set, the name of the property is added to it; otherwise this is a single null test.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnRead(string tokenName) => _reads?.Add(tokenName);

    /// <summary>Starts recording the names read through token properties into <paramref name="reads"/>.</summary>
    internal void RecordReads(HashSet<string> reads) => _reads = reads;

    /// <summary>Stops recording reads.</summary>
    internal void StopRecordingReads() => _reads = null;

    /// <summary>
    /// Whether the token property <paramref name="tokenName"/> is frozen. The generated setter of every partial token
    /// property calls this first and drops the write when it returns <see langword="true"/>, so a value the user gave
    /// explicitly survives the derivation that would otherwise compute it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The property is one the current derivation must not write.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsFrozen(string tokenName) => _freeze is not null && _freeze.IsFrozen(tokenName);

    /// <summary>
    /// Freezes <paramref name="tokenName"/>: later writes through the property are ignored and reads return the current
    /// value. The caller writes the explicit value before freezing it.
    /// </summary>
    internal void Freeze(string tokenName) => (_freeze ??= new TokenFreezeSet()).Add(tokenName);

    /// <summary>
    /// Makes a write to any of <paramref name="tokenNames"/> throw; used for the global view of a control derivation,
    /// which may override Map and Alias tokens but never a seed (spec 6.1).
    /// </summary>
    internal void RejectWrites(IReadOnlySet<string> tokenNames, string reason) =>
        (_freeze ??= new TokenFreezeSet()).Reject(tokenNames, reason);

    /// <summary>Removes every frozen and rejected name; the core's final normalization runs after this.</summary>
    internal void ClearFreeze() => _freeze = null;

    /// <summary>Whether <paramref name="tokenName"/> is frozen, without the rejection check.</summary>
    internal bool IsFrozenName(string tokenName) => _freeze?.Contains(tokenName) == true;

    /// <summary>
    /// Starts tracking whether any write through a token property is accepted (not frozen); the compiler uses it to
    /// know whether a control derivation overrode a global token in the control's view.
    /// </summary>
    internal void TrackWrites() => (_freeze ??= new TokenFreezeSet()).TrackWrites = true;

    /// <summary>Whether a write was accepted since <see cref="TrackWrites"/>.</summary>
    internal bool HasAcceptedWrites => _freeze?.AcceptedWrite == true;
}

/// <summary>The names frozen on one token object, and the names a derivation must not write.</summary>
internal sealed class TokenFreezeSet
{
    private readonly HashSet<string> _frozen = new(StringComparer.Ordinal);
    private IReadOnlySet<string>? _rejected;
    private string? _rejectReason;

    internal void Add(string tokenName) => _frozen.Add(tokenName);

    internal void Reject(IReadOnlySet<string> tokenNames, string reason)
    {
        _rejected     = tokenNames;
        _rejectReason = reason;
    }

    internal bool TrackWrites { get; set; }

    internal bool AcceptedWrite { get; private set; }

    internal bool Contains(string tokenName) => _frozen.Contains(tokenName);

    internal bool IsFrozen(string tokenName)
    {
        if (_rejected is not null && _rejected.Contains(tokenName))
        {
            throw new InvalidOperationException($"Token '{tokenName}' cannot be written here: {_rejectReason}");
        }

        if (_frozen.Contains(tokenName))
        {
            return true;
        }

        if (TrackWrites)
        {
            AcceptedWrite = true;
        }

        return false;
    }
}
