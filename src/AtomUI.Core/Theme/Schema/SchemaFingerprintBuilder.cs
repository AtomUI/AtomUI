namespace AtomUI.Theme.Schema;

/// <summary>
/// Builds an FNV-1a fingerprint over strings and integers. It is byte-for-byte equivalent to the
/// <c>RegistrationFingerprint</c> of <c>AtomUI.Generator</c>: every UTF-16 unit and every integer enters the
/// hash as 8 little-endian bytes, and folding the zero suffix of a <c>ushort</c> or <c>int</c> into one
/// multiplication is mathematically equal to the per-byte form. The runtime uses it for contract validation,
/// <c>Revision</c> calculation and for continuing the fingerprint of generated assets.
/// A state of 0 means that nothing was hashed yet and reads as the FNV offset basis.
/// </summary>
internal struct SchemaFingerprintBuilder
{
    private const ulong Offset = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;
    private const ulong Prime2 = unchecked(Prime * Prime);
    private const ulong Prime4 = unchecked(Prime2 * Prime2);
    private const ulong Prime6 = unchecked(Prime4 * Prime2);
    private const ulong Prime7 = unchecked(Prime6 * Prime);
    private ulong _value;

    internal ulong Value => _value == 0 ? Offset : _value;

    internal void Add(string value)
    {
        Add(value.Length);
        foreach (var character in value)
        {
            Add((ushort)character);
        }
    }

    internal void Add(int value)
    {
        var unsigned = unchecked((uint)value);
        unchecked
        {
            _value = (Value ^ (byte)unsigned) * Prime;
            _value = (_value ^ (byte)(unsigned >> 8)) * Prime;
            _value = (_value ^ (byte)(unsigned >> 16)) * Prime;
            _value = (_value ^ (byte)(unsigned >> 24)) * Prime * Prime4;
        }
    }

    internal void Add(byte value)
    {
        // The existing format always encodes eight bytes, including seven zero suffix bytes.
        _value = unchecked((Value ^ value) * Prime * Prime7);
    }

    internal void Add(ushort value)
    {
        unchecked
        {
            _value = (Value ^ (byte)value) * Prime;
            _value = (_value ^ (byte)(value >> 8)) * Prime * Prime6;
        }
    }

    internal void Add(ulong value)
    {
        if (_value == 0)
        {
            _value = Offset;
        }

        unchecked
        {
            for (var shift = 0; shift < 64; shift += 8)
            {
                _value ^= (byte)(value >> shift);
                _value *= Prime;
            }
        }
    }

    /// <summary>
    /// Captures the current intermediate state as the raw value, so 0 still means that nothing was hashed.
    /// </summary>
    internal SchemaPrefixState Save()
    {
        return new SchemaPrefixState(_value);
    }

    /// <summary>
    /// Creates a builder that continues from a state captured by <see cref="Save" />; fields added afterwards
    /// are hashed after that prefix.
    /// </summary>
    internal static SchemaFingerprintBuilder Restore(SchemaPrefixState state)
    {
        var builder = new SchemaFingerprintBuilder { _value = state.Value };
        return builder;
    }
}
