namespace AtomUI.Controls.Data;

/// <summary>
/// A descriptor that is assembled by hand from a list of <see cref="IDataMemberAccessor" /> instances, for data
/// types that cannot carry <see cref="GenerateDataMemberAccessorsAttribute" />.
/// </summary>
/// <typeparam name="TData">
/// The type whose members the accessors describe.
/// </typeparam>
public sealed class DataMemberAccessorDescriptor<TData> : IDataMemberAccessorDescriptor,
                                                          IDataMemberAccessorDescriptorMetadataProvider
{
    private readonly Dictionary<string, IDataMemberAccessor> _accessorsByPath;

    /// <inheritdoc />
    public Type DataType => typeof(TData);

    /// <inheritdoc />
    public Func<object>? NewItemFactory { get; }

    /// <inheritdoc />
    public IReadOnlyList<IDataMemberAccessor> Accessors { get; }

    /// <inheritdoc />
    public bool IsDataTypeReadOnly { get; }

    /// <summary>
    /// Initializes the descriptor with the accessors of the data type.
    /// </summary>
    /// <param name="newItemFactory">
    /// Creates a new item of the data type for <see cref="IListCollectionView.AddNew" />.
    /// <see langword="null" /> leaves the descriptor without a factory.
    /// </param>
    /// <param name="accessors">
    /// The accessors of the data type. When two of them share a <see cref="IDataMemberAccessor.Path" />, the later one
    /// is found by <see cref="TryGetAccessor" />.
    /// </param>
    /// <param name="isDataTypeReadOnly">
    /// Whether the data type is read-only as a whole. The default is <see langword="false" />.
    /// </param>
    public DataMemberAccessorDescriptor(Func<TData>? newItemFactory,
                                        IReadOnlyList<IDataMemberAccessor> accessors,
                                        bool isDataTypeReadOnly = false)
    {
        NewItemFactory     = newItemFactory is null ? null : () => newItemFactory()!;
        Accessors          = accessors;
        IsDataTypeReadOnly = isDataTypeReadOnly;
        _accessorsByPath   = new Dictionary<string, IDataMemberAccessor>(accessors.Count, StringComparer.Ordinal);
        foreach (var accessor in accessors)
        {
            _accessorsByPath[accessor.Path] = accessor;
        }
    }

    /// <inheritdoc />
    public bool TryGetAccessor(string path, out IDataMemberAccessor accessor)
    {
        return _accessorsByPath.TryGetValue(path, out accessor!);
    }
}
