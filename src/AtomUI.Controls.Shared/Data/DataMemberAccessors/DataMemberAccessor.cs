using System.Collections;

namespace AtomUI.Controls.Data;

/// <summary>
/// Accesses one data member of <typeparamref name="TData" /> through delegates, so that the member can be read,
/// written and compared without reflection.
/// </summary>
/// <typeparam name="TData">
/// The type that declares the member.
/// </typeparam>
/// <typeparam name="TValue">
/// The type of the member's value.
/// </typeparam>
/// <remarks>
/// <para>
/// The generator of <see cref="GenerateDataMemberAccessorsAttribute" /> and hand-written
/// <see cref="DataMemberAccessorDescriptor{TData}" /> instances use it as the accessor of a data member.
/// </para>
/// </remarks>
public sealed class DataMemberAccessor<TData, TValue> : IDataMemberAccessor, IDataMemberAccessorMetadataProvider
{
    private readonly Func<TData, TValue> _getter;
    private readonly Action<TData, TValue>? _setter;

    /// <inheritdoc />
    public string Path { get; }

    /// <inheritdoc />
    public Type ValueType => typeof(TValue);

    /// <summary>
    /// Compares two member values for sorting; the value is never <see langword="null" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unless another comparer was passed to the constructor, <see langword="null" /> sorts before every other value,
    /// <see langword="null" /> equals <see langword="null" />, and other values are compared with
    /// <c>Comparer&lt;TValue&gt;.Default</c>.
    /// </para>
    /// </remarks>
    public IComparer? Comparer { get; }

    /// <inheritdoc />
    public bool CanWrite => _setter is not null;

    /// <inheritdoc />
    public DataMemberAccessorMetadata Metadata { get; }

    /// <summary>
    /// Initializes the accessor with the delegates that read and write the member.
    /// </summary>
    /// <param name="path">
    /// The key that identifies the accessor in its descriptor, usually the name of the member.
    /// </param>
    /// <param name="getter">
    /// Reads the member from an item.
    /// </param>
    /// <param name="setter">
    /// Writes the member on an item. <see langword="null" />, the default, makes the accessor read-only.
    /// </param>
    /// <param name="comparer">
    /// Compares two member values. <see langword="null" />, the default, uses the comparison of the
    /// <see cref="Comparer" /> property.
    /// </param>
    /// <param name="metadata">
    /// The display and editing metadata of the member. <see langword="null" />, the default, uses
    /// <see cref="DataMemberAccessorMetadata.Default" />.
    /// </param>
    public DataMemberAccessor(string path,
                              Func<TData, TValue> getter,
                              Action<TData, TValue>? setter = null,
                              IComparer? comparer = null,
                              DataMemberAccessorMetadata? metadata = null)
    {
        Path     = path;
        _getter  = getter;
        _setter  = setter;
        Comparer = comparer ?? DataMemberValueComparer<TValue>.Default;
        Metadata = metadata ?? DataMemberAccessorMetadata.Default;
    }

    /// <inheritdoc />
    public object? GetValue(object instance)
    {
        return _getter((TData)instance);
    }

    /// <inheritdoc />
    public void SetValue(object instance, object? value)
    {
        if (_setter is null)
        {
            throw new InvalidOperationException($"Data member '{Path}' on type '{typeof(TData).FullName}' cannot be written.");
        }

        _setter((TData)instance, CastValue(value));
    }

    private static TValue CastValue(object? value)
    {
        if (value is null)
        {
            return default!;
        }

        return (TValue)value;
    }
}
