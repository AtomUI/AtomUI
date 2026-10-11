namespace AtomUI.Theme.Schema;

/// <summary>
/// FNV-1a 指纹构造器。与 AtomUI.Generator 的 RegistrationFingerprint 逐字节等价
/// （每个 UTF-16 单元/整数按 8 个 little-endian 字节进入哈希；ushort/int 的零后缀折叠
/// 在数学上与逐字节形态相等）。供运行时契约校验、Revision 计算与生成资产指纹续算使用。
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

    /// <summary>捕获当前中间状态（原始值，保留 0=未初始化约定）。</summary>
    internal SchemaPrefixState Save()
    {
        return new SchemaPrefixState(_value);
    }

    /// <summary>从先前捕获的中间状态恢复构造器，继续追加后续字段。</summary>
    internal static SchemaFingerprintBuilder Restore(SchemaPrefixState state)
    {
        var builder = new SchemaFingerprintBuilder { _value = state.Value };
        return builder;
    }
}
