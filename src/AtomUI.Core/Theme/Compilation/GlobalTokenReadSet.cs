using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

/// <summary>
/// A set of global token slots: what a control token rule read, or what a child unit changed.
/// </summary>
internal sealed class GlobalTokenReadSet
{
    private static readonly ConditionalWeakTable<IReadOnlyList<TokenDescriptor>, FrozenDictionary<string, int>> s_slots = new();

    private readonly ulong[] _bits;

    private GlobalTokenReadSet(ulong[] bits) => _bits = bits;

    internal static GlobalTokenReadSet FromNames(IReadOnlyCollection<string> names, IReadOnlyList<TokenDescriptor> descriptors)
    {
        var slots = s_slots.GetValue(descriptors,
            static list => list.ToFrozenDictionary(static descriptor => descriptor.Name, static descriptor => descriptor.Slot,
                StringComparer.Ordinal));
        var bits = new ulong[(descriptors.Count + 63) >> 6];
        foreach (var name in names)
        {
            if (slots.TryGetValue(name, out var slot))
            {
                bits[slot >> 6] |= 1UL << (slot & 63);
            }
        }

        return new GlobalTokenReadSet(bits);
    }

    /// <summary>
    /// The slots whose values differ between two global value tables of the same schema.
    /// </summary>
    internal static GlobalTokenReadSet Changes(TokenValueTable before, TokenValueTable after)
    {
        var bits = new ulong[(Math.Max(before.Count, after.Count) + 63) >> 6];
        for (var slot = 0; slot < after.Count; slot++)
        {
            if (slot >= before.Count || !before.ValueEquals(slot, after.GetValue(slot)))
            {
                bits[slot >> 6] |= 1UL << (slot & 63);
            }
        }

        return new GlobalTokenReadSet(bits);
    }

    internal bool IsEmpty => Array.TrueForAll(_bits, static word => word == 0);

    internal bool Contains(int slot) => (uint)(slot >> 6) < (uint)_bits.Length && (_bits[slot >> 6] & (1UL << (slot & 63))) != 0;

    internal bool Overlaps(GlobalTokenReadSet other)
    {
        var count = Math.Min(_bits.Length, other._bits.Length);
        for (var index = 0; index < count; index++)
        {
            if ((_bits[index] & other._bits[index]) != 0)
            {
                return true;
            }
        }

        return false;
    }

    internal long EstimatedRetainedBytes => 24 + 8L * _bits.Length;
}
