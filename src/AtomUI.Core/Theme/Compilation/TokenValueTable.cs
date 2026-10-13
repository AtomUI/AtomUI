using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Compilation;

internal sealed class TokenValueTable
{
    internal static readonly TokenValueTable Empty = new(Array.Empty<object?>());

    private readonly object?[] _values;

    private TokenValueTable(object?[] values)
    {
        _values                = values;
        EstimatedRetainedBytes = ThemeRetainedBytesEstimator.EstimateDenseValues(values);
    }

    internal int Count => _values.Length;
    internal long EstimatedRetainedBytes { get; }

    internal T Get<T>(int slot)
    {
        var value = ThemeResourceValue.CloneForConsumer(GetValue(slot));
        if (value is T typed)
        {
            return typed;
        }

        if (value is null && default(T) is null)
        {
            return default!;
        }

        throw new InvalidCastException(
            $"Token slot {slot} contains '{value?.GetType().FullName ?? "null"}', not '{typeof(T).FullName}'.");
    }

    internal object? GetValue(int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        if ((uint)slot >= (uint)_values.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(slot));
        }

        return _values[slot];
    }

    internal bool ValueEquals(int slot, object? value)
    {
        return Equals(GetValue(slot), value);
    }

    internal static TokenValueTable Freeze(
        AbstractDesignToken builder,
        IReadOnlyList<TokenDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(descriptors);

        var values = new object?[GetRequiredLength(descriptors)];
        foreach (var descriptor in descriptors)
        {
            values[descriptor.Slot] = descriptor.CopyValue(descriptor.GetValue(builder));
        }

        return new TokenValueTable(values);
    }

    private static int GetRequiredLength(IReadOnlyList<TokenDescriptor> descriptors)
    {
        var length = 0;
        foreach (var descriptor in descriptors)
        {
            length = Math.Max(length, descriptor.Slot + 1);
        }

        return length;
    }

}
