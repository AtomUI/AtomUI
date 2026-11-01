using System.Collections.Immutable;

namespace AtomUI.Desktop.Controls;

[Flags]
public enum DataGridScalarKinds
{
    None = 0,
    Null = 1 << 0,
    Boolean = 1 << 1,
    SignedInteger = 1 << 2,
    UnsignedInteger = 1 << 3,
    Double = 1 << 4,
    Decimal = 1 << 5,
    String = 1 << 6,
    Guid = 1 << 7,
    DateOnly = 1 << 8,
    TimeOnly = 1 << 9,
    DateTimeOffset = 1 << 10,
    All = Null | Boolean | SignedInteger | UnsignedInteger | Double | Decimal |
          String | Guid | DateOnly | TimeOnly | DateTimeOffset
}

public sealed class DataGridFilterOperatorSchema
{
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

    public DataGridOperatorId Id { get; }

    public int MinimumValueCount { get; }

    public int MaximumValueCount { get; }

    public DataGridScalarKinds AcceptedKinds { get; }
}

public sealed class DataGridFieldSchema
{
    private readonly Dictionary<DataGridOperatorId, DataGridFilterOperatorSchema> _filterOperatorsById;

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

    public DataGridFieldId Id { get; }

    public Type ValueType { get; }

    public DataGridSortDirections SortDirections { get; }

    public ImmutableArray<DataGridFilterOperatorSchema> FilterOperators { get; }

    public bool CanGroup { get; }

    public DataGridFieldDisplayAccessor? DisplayAccessor { get; }

    /// <summary>
    /// Gets whether edit requests may write this field through <see cref="IDataGridEditableSource"/>.
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

public sealed class DataGridSourceSchema
{
    private readonly Dictionary<DataGridFieldId, DataGridFieldSchema> _fieldsById;

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

    public Type ItemType { get; }

    public ImmutableArray<DataGridFieldSchema> Fields { get; }

    public int PreferredRangeSize { get; }

    public int MaximumRangeSize { get; }

    internal bool TryGetField(DataGridFieldId id, out DataGridFieldSchema field) =>
        _fieldsById.TryGetValue(id, out field!);
}
