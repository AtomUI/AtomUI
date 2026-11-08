using System.Runtime.CompilerServices;

namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// 所有的控件 Token 定义是除了全局 Token 之外专属于当前控件的 Token 值。
/// </summary>
public abstract class AbstractControlDesignToken : AbstractDesignToken
{
    private ulong[]? _explicitSlots;

    protected DesignToken EffectiveGlobalToken { get; private set; } = default!;

    internal void AssignEffectiveGlobalToken(DesignToken effectiveGlobalToken)
    {
        EffectiveGlobalToken = effectiveGlobalToken;
    }

    /// <summary>
    /// 标记指定 Own Token 槽位已被用户显式覆盖，默认计算据此跳过对该字段的写入。
    /// </summary>
    internal void MarkExplicit(int ownSlot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ownSlot);
        var word = ownSlot >> 6;
        if (_explicitSlots is null)
        {
            _explicitSlots = new ulong[word + 1];
        }
        else if (word >= _explicitSlots.Length)
        {
            Array.Resize(ref _explicitSlots, word + 1);
        }

        _explicitSlots[word] |= 1UL << (ownSlot & 63);
    }

    internal bool IsExplicitSlot(int ownSlot)
    {
        if (ownSlot < 0)
        {
            return false;
        }

        var word = ownSlot >> 6;
        var slots = _explicitSlots;
        return slots is not null && word < slots.Length && (slots[word] & (1UL << (ownSlot & 63))) != 0;
    }

    /// <summary>
    /// 判断指定 Own Token 是否被用户显式覆盖。<paramref name="kind"/> 的整数值即 Own Token 槽位，
    /// 由生成的 <c>XxxTokenKind</c> 枚举保证（int 底层类型，按 Own Token 名称序号赋值）。
    /// </summary>
    protected bool IsExplicit<TKind>(TKind kind) where TKind : struct, Enum
    {
        if (Unsafe.SizeOf<TKind>() != sizeof(int))
        {
            throw new ArgumentException(
                $"Token kind enum '{typeof(TKind).FullName}' must be backed by int.", nameof(kind));
        }

        return IsExplicitSlot(Unsafe.As<TKind, int>(ref kind));
    }

    internal void ResetExplicit()
    {
        if (_explicitSlots is not null)
        {
            Array.Clear(_explicitSlots);
        }
    }

    public virtual void CalculateTokenValues(bool isDarkMode)
    {
    }

}
