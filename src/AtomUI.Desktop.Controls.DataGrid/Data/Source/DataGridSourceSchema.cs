using System.Collections.Immutable;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The scalar value kinds that a filter operator accepts for the values of its filters, as a flag set.
/// Each kind is the <c>DataGridScalarKind</c> of the same name. Request validation rejects a filter when the
/// kind of one of its values is not accepted by the operator.
/// </summary>
[Flags]
public enum DataGridScalarKinds
{
    /// <summary>
    /// An empty set: no kind is accepted. The <see cref="DataGridFilterOperatorSchema" /> constructor rejects it.
    /// </summary>
    None = 0,
    /// <summary>
    /// A filter value that is <see langword="null" />.
    /// </summary>
    Null = 1 << 0,
    /// <summary>
    /// A <see langword="bool" /> value.
    /// </summary>
    Boolean = 1 << 1,
    /// <summary>
    /// A signed integer value.
    /// </summary>
    SignedInteger = 1 << 2,
    /// <summary>
    /// An unsigned integer value.
    /// </summary>
    UnsignedInteger = 1 << 3,
    /// <summary>
    /// A <see langword="double" /> value.
    /// </summary>
    Double = 1 << 4,
    /// <summary>
    /// A <see langword="decimal" /> value.
    /// </summary>
    Decimal = 1 << 5,
    /// <summary>
    /// A <see langword="string" /> value.
    /// </summary>
    String = 1 << 6,
    /// <summary>
    /// A <see cref="Guid" /> value.
    /// </summary>
    Guid = 1 << 7,
    /// <summary>
    /// A <see cref="DateOnly" /> value.
    /// </summary>
    DateOnly = 1 << 8,
    /// <summary>
    /// A <see cref="TimeOnly" /> value.
    /// </summary>
    TimeOnly = 1 << 9,
    /// <summary>
    /// A <see cref="DateTimeOffset" /> value.
    /// </summary>
    DateTimeOffset = 1 << 10,
    /// <summary>
    /// Every kind except <see cref="None" />. The <see cref="DataGridFilterOperatorSchema" /> constructor rejects
    /// accepted kinds with a bit outside this value.
    /// </summary>
    All = Null | Boolean | SignedInteger | UnsignedInteger | Double | Decimal |
          String | Guid | DateOnly | TimeOnly | DateTimeOffset
}

/// <summary>
/// The immutable contract of one filter operator that a field offers: how many values a filter with this
/// operator carries and which scalar kinds these values may have. Request validation rejects a filter that
/// breaks either.
/// </summary>
public sealed class DataGridFilterOperatorSchema
{
    /// <summary>
    /// Creates the schema of one filter operator and validates its arguments.
    /// </summary>
    /// <param name="id">
    /// The identity of the operator. It has to be unique among the filter operators of a field.
    /// </param>
    /// <param name="minimumValueCount">
    /// The fewest values a filter with this operator carries.
    /// </param>
    /// <param name="maximumValueCount">
    /// The most values a filter with this operator carries.
    /// </param>
    /// <param name="acceptedKinds">
    /// The scalar kinds that a value of such a filter may have.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="id" /> is not a valid identity, or <paramref name="acceptedKinds" /> is
    /// <see cref="DataGridScalarKinds.None" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="minimumValueCount" /> is negative, <paramref name="maximumValueCount" /> is less than
    /// <paramref name="minimumValueCount" /> or greater than 1024, or <paramref name="acceptedKinds" /> contains a
    /// kind outside <see cref="DataGridScalarKinds.All" />.
    /// </exception>
    public DataGridFilterOperatorSchema(
        DataGridOperatorId id,
        int minimumValueCount,
        int maximumValueCount,
        DataGridScalarKinds acceptedKinds)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException("The operator identity must be valid.", nameof(id));
        }
        if (minimumValueCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumValueCount));
        }
        if (maximumValueCount < minimumValueCount || maximumValueCount > 1024)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumValueCount));
        }
        if (acceptedKinds == DataGridScalarKinds.None)
        {
            throw new ArgumentException("At least one scalar kind must be accepted.", nameof(acceptedKinds));
        }
        if ((acceptedKinds & ~DataGridScalarKinds.All) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(acceptedKinds));
        }

        Id = id;
        MinimumValueCount = minimumValueCount;
        MaximumValueCount = maximumValueCount;
        AcceptedKinds = acceptedKinds;
    }

    /// <summary>
    /// The identity of the operator, as the filters of a query name it. It is unique among the filter operators
    /// of a field.
    /// </summary>
    public DataGridOperatorId Id { get; }

    /// <summary>
    /// The fewest values a filter with this operator has to carry to pass request validation. Zero allows a filter
    /// without values.
    /// </summary>
    public int MinimumValueCount { get; }

    /// <summary>
    /// The most values a filter with this operator may carry to pass request validation. At most 1024.
    /// </summary>
    public int MaximumValueCount { get; }

    /// <summary>
    /// The scalar kinds that the values of a filter with this operator may have. Request validation rejects the
    /// filter when the kind of one of its values is not in this set. Never
    /// <see cref="DataGridScalarKinds.None" /> and never a kind outside <see cref="DataGridScalarKinds.All" />.
    /// </summary>
    public DataGridScalarKinds AcceptedKinds { get; }
}

/// <summary>
/// The immutable contract of one field of a data source: the sort directions, the filter operators and whether
/// it can be grouped and edited. The grid reads it to apply sort and filter gestures, and request validation
/// rejects a sort, filter or group that the field does not support.
/// </summary>
public sealed class DataGridFieldSchema
{
    private readonly Dictionary<DataGridOperatorId, DataGridFilterOperatorSchema> _filterOperatorsById;

    /// <summary>
    /// Creates the schema of one field and validates its arguments.
    /// </summary>
    /// <param name="id">
    /// The identity of the field. It has to be unique among the fields of a source.
    /// </param>
    /// <param name="valueType">
    /// The type of value the field reads from an item. A closed type that is not by-ref and not a pointer.
    /// </param>
    /// <param name="sortDirections">
    /// The directions that a sort or a group on this field may use.
    /// </param>
    /// <param name="filterOperators">
    /// The operators that a filter on this field may use. A default array counts as empty. The entries have to be
    /// unique by <see cref="DataGridFilterOperatorSchema.Id" />, and the grid uses the first entry for the filter
    /// gesture.
    /// </param>
    /// <param name="canGroup">
    /// Whether a group of a query may be built on this field. It requires at least one sort direction.
    /// </param>
    /// <param name="displayAccessor">
    /// The access to the field value of an item, or <see langword="null" /> when the field has none. Its value type
    /// has to equal <paramref name="valueType" />.
    /// </param>
    /// <param name="isEditable">
    /// Whether an edit request may write this field through <see cref="IDataGridEditableSource" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="valueType" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="id" /> is not a valid identity; <paramref name="valueType" /> is <c>void</c>, by-ref,
    /// a pointer or an open generic type; <paramref name="canGroup" /> is <see langword="true" /> while
    /// <paramref name="sortDirections" /> is <see cref="DataGridSortDirections.None" />; the value type of
    /// <paramref name="displayAccessor" /> differs from <paramref name="valueType" />; or
    /// <paramref name="filterOperators" /> contains <see langword="null" /> or two operators with the same id.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="sortDirections" /> contains a direction outside <see cref="DataGridSortDirections.All" />.
    /// </exception>
    public DataGridFieldSchema(
        DataGridFieldId id,
        Type valueType,
        DataGridSortDirections sortDirections,
        ImmutableArray<DataGridFilterOperatorSchema> filterOperators,
        bool canGroup,
        DataGridFieldDisplayAccessor? displayAccessor = null,
        bool isEditable = false)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException("The field identity must be valid.", nameof(id));
        }
        ValidateMetadataType(valueType, nameof(valueType));
        if ((sortDirections & ~DataGridSortDirections.All) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortDirections));
        }
        if (canGroup && sortDirections == DataGridSortDirections.None)
        {
            throw new ArgumentException(
                "A groupable field must support at least one sort direction.", nameof(sortDirections));
        }

        Id = id;
        ValueType = valueType;
        if (displayAccessor is not null && displayAccessor.ValueType != valueType)
        {
            throw new ArgumentException(
                "The display accessor value type must match the field value type.",
                nameof(displayAccessor));
        }
        SortDirections = sortDirections;
        FilterOperators = filterOperators.IsDefault
            ? ImmutableArray<DataGridFilterOperatorSchema>.Empty
            : filterOperators;
        CanGroup = canGroup;
        DisplayAccessor = displayAccessor;
        IsEditable = isEditable;

        _filterOperatorsById = new Dictionary<DataGridOperatorId, DataGridFilterOperatorSchema>(
            FilterOperators.Length);
        foreach (var filterOperator in FilterOperators)
        {
            if (filterOperator is null)
            {
                throw new ArgumentException("Filter operators cannot contain null.", nameof(filterOperators));
            }
            if (!_filterOperatorsById.TryAdd(filterOperator.Id, filterOperator))
            {
                throw new ArgumentException(
                    $"Filter operator '{filterOperator.Id}' occurs more than once.",
                    nameof(filterOperators));
            }
        }
    }

    /// <summary>
    /// The identity of the field, as the sorts, filters and groups of a query name it. It is unique within a
    /// source.
    /// </summary>
    public DataGridFieldId Id { get; }

    /// <summary>
    /// The type of the values this field holds. It equals the
    /// <see cref="DataGridFieldDisplayAccessor.ValueType" /> of <see cref="DisplayAccessor" /> when one is set.
    /// </summary>
    public Type ValueType { get; }

    /// <summary>
    /// The directions that a sort or a group on this field may use. Request validation rejects the other
    /// directions. A groupable field has at least one.
    /// </summary>
    public DataGridSortDirections SortDirections { get; }

    /// <summary>
    /// The operators that a filter on this field may use. Request validation rejects any other operator, and the
    /// grid uses the first entry for its filter gesture. When the array is empty, that gesture changes nothing.
    /// </summary>
    public ImmutableArray<DataGridFilterOperatorSchema> FilterOperators { get; }

    /// <summary>
    /// Whether a group of a query may be built on this field. Request validation rejects a group on a field that
    /// is not groupable, and a group on this field needs its direction in <see cref="SortDirections" />.
    /// </summary>
    public bool CanGroup { get; }

    /// <summary>
    /// The access to the field value of an item, or <see langword="null" /> when the field has none. An
    /// auto-generated column binds through it. When it is set, its value type equals <see cref="ValueType" /> and
    /// its item type equals <see cref="DataGridSourceSchema.ItemType" />.
    /// </summary>
    public DataGridFieldDisplayAccessor? DisplayAccessor { get; }

    /// <summary>
    /// Gets whether edit requests may write this field through <see cref="IDataGridEditableSource" />.
    /// </summary>
    public bool IsEditable { get; }

    internal bool TryGetFilterOperator(
        DataGridOperatorId id,
        out DataGridFilterOperatorSchema filterOperator) =>
        _filterOperatorsById.TryGetValue(id, out filterOperator!);

    internal static void ValidateMetadataType(Type? type, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(type, parameterName);
        if (type == typeof(void) || type.IsByRef || type.IsPointer || type.ContainsGenericParameters)
        {
            throw new ArgumentException("The metadata type must be a closed value or reference type.", parameterName);
        }
    }
}

/// <summary>
/// The immutable contract of a data source: the type of its items, the fields a query may address and the row
/// counts its range requests may have. A source publishes it as <c>IDataGridSource.Schema</c>, and request
/// validation checks sorts, filters, groups and ranges against it.
/// </summary>
public sealed class DataGridSourceSchema
{
    private readonly Dictionary<DataGridFieldId, DataGridFieldSchema> _fieldsById;

    /// <summary>
    /// Creates the schema of a data source and validates its arguments.
    /// </summary>
    /// <param name="itemType">
    /// The type of the items the source serves. A closed type that is not by-ref and not a pointer.
    /// </param>
    /// <param name="fields">
    /// The fields a query may address. A default array counts as empty. The entries have to be unique by
    /// <see cref="DataGridFieldSchema.Id" />.
    /// </param>
    /// <param name="preferredRangeSize">
    /// The row count of one range block. From 32 up to <paramref name="maximumRangeSize" />.
    /// </param>
    /// <param name="maximumRangeSize">
    /// The largest row count one range request may have. From 32 to 4096.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="itemType" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="itemType" /> is <c>void</c>, by-ref, a pointer or an open generic type; or
    /// <paramref name="fields" /> contains <see langword="null" />, two fields with the same id, or a field
    /// whose display accessor has an item type other than <paramref name="itemType" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maximumRangeSize" /> is outside 32 to 4096, or <paramref name="preferredRangeSize" /> is
    /// below 32 or above <paramref name="maximumRangeSize" />.
    /// </exception>
    public DataGridSourceSchema(
        Type itemType,
        ImmutableArray<DataGridFieldSchema> fields,
        int preferredRangeSize,
        int maximumRangeSize)
    {
        DataGridFieldSchema.ValidateMetadataType(itemType, nameof(itemType));
        if (maximumRangeSize is < 32 or > 4096)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumRangeSize));
        }
        if (preferredRangeSize < 32 || preferredRangeSize > maximumRangeSize)
        {
            throw new ArgumentOutOfRangeException(nameof(preferredRangeSize));
        }

        ItemType = itemType;
        Fields = fields.IsDefault ? ImmutableArray<DataGridFieldSchema>.Empty : fields;
        PreferredRangeSize = preferredRangeSize;
        MaximumRangeSize = maximumRangeSize;

        _fieldsById = new Dictionary<DataGridFieldId, DataGridFieldSchema>(Fields.Length);
        foreach (var field in Fields)
        {
            if (field is null)
            {
                throw new ArgumentException("Fields cannot contain null.", nameof(fields));
            }
            if (!_fieldsById.TryAdd(field.Id, field))
            {
                throw new ArgumentException(
                    $"Field '{field.Id}' occurs more than once.", nameof(fields));
            }
            if (field.DisplayAccessor is { } displayAccessor && displayAccessor.ItemType != itemType)
            {
                throw new ArgumentException(
                    $"Display accessor for field '{field.Id}' targets '{displayAccessor.ItemType}', " +
                    $"but the source item type is '{itemType}'.",
                    nameof(fields));
            }
        }
    }

    /// <summary>
    /// The type of the items the source serves. The <see cref="DataGridFieldDisplayAccessor.ItemType" /> of the
    /// display accessor of every field equals it.
    /// </summary>
    public Type ItemType { get; }

    /// <summary>
    /// The fields that the sorts, filters and groups of a request may address. Request validation rejects a field
    /// identity that is not among them.
    /// </summary>
    public ImmutableArray<DataGridFieldSchema> Fields { get; }

    /// <summary>
    /// The row count of one range block that the grid loads. The viewport decides which blocks are requested, so
    /// this value sets the granularity of loading. From 32 up to <see cref="MaximumRangeSize" />.
    /// </summary>
    public int PreferredRangeSize { get; }

    /// <summary>
    /// The largest row count one range request may have. From 32 to 4096. The grid clamps the row count it derives
    /// from the viewport height to it, and request validation rejects a larger range.
    /// </summary>
    public int MaximumRangeSize { get; }

    internal bool TryGetField(DataGridFieldId id, out DataGridFieldSchema field) =>
        _fieldsById.TryGetValue(id, out field!);
}
