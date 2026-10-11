using System.ComponentModel;

namespace AtomUI.Theme.Schema;

/// <summary>
/// 全局 Token 名列表的指纹前缀中间状态。由生成器在编译期算出并以常量嵌入，
/// 运行时从该状态续算资产专有字段，避免每个资产重复哈希同一串全局名。
/// 仅生成代码使用；语义与 <see cref="SchemaFingerprintBuilder"/> 的 0=未初始化约定一致。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct SchemaPrefixState : IEquatable<SchemaPrefixState>
{
    public SchemaPrefixState(ulong value)
    {
        Value = value;
    }

    public ulong Value { get; }

    public bool Equals(SchemaPrefixState other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is SchemaPrefixState other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(SchemaPrefixState left, SchemaPrefixState right) => left.Equals(right);

    public static bool operator !=(SchemaPrefixState left, SchemaPrefixState right) => !left.Equals(right);
}
