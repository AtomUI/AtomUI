using System.Collections.Immutable;
using System.Globalization;

namespace AtomUI.Desktop.Controls;

public readonly struct DataGridLocalFilter<TValue>
{
    public DataGridLocalFilter(
        DataGridOperatorId @operator,
        int minimumValueCount,
        int maximumValueCount,
        DataGridScalarKinds acceptedKinds,
        Func<TValue, ImmutableArray<DataGridScalar>, bool> evaluate)
    {
        ArgumentNullException.ThrowIfNull(evaluate);
        Schema = new DataGridFilterOperatorSchema(
            @operator,
            minimumValueCount,
            maximumValueCount,
            acceptedKinds);
        Evaluate = evaluate;
    }

    public DataGridOperatorId Operator => Schema.Id;

    public int MinimumValueCount => Schema.MinimumValueCount;

    public int MaximumValueCount => Schema.MaximumValueCount;

    public DataGridScalarKinds AcceptedKinds => Schema.AcceptedKinds;

    public Func<TValue, ImmutableArray<DataGridScalar>, bool> Evaluate { get; }

    internal DataGridFilterOperatorSchema Schema { get; }
}

internal interface IDataGridLocalField<T>
{
    DataGridFieldSchema Schema { get; }

    int Compare(T left, T right);

    DataGridScalar GetScalar(T item);

    bool Evaluate(T item, DataGridFilter filter);

    /// <summary>
    /// Converts the scalar to the field value without writing it, so a multi-field edit can fail before any write.
    /// </summary>
    Action<T> PrepareWrite(DataGridScalar value);
}

public sealed class DataGridLocalField<T, TValue> : IDataGridLocalField<T>
{
    private readonly Dictionary<DataGridOperatorId, DataGridLocalFilter<TValue>> _filtersById;

    public DataGridLocalField(
        DataGridFieldId fieldId,
        Func<T, TValue> getter,
        IComparer<TValue> sortComparer,
        Func<TValue, DataGridScalar> toScalar,
        ImmutableArray<DataGridLocalFilter<TValue>> filters = default,
        DataGridSortDirections sortDirections = DataGridSortDirections.All,
        bool canGroup = false,
        string? propertyChangedName = null,
        Action<T, TValue>? setter = null,
        Func<DataGridScalar, TValue>? fromScalar = null)
    {
        ArgumentNullException.ThrowIfNull(getter);
        ArgumentNullException.ThrowIfNull(sortComparer);
        ArgumentNullException.ThrowIfNull(toScalar);
        if (setter is null && fromScalar is not null)
        {
            throw new ArgumentException("A scalar reader requires a setter.", nameof(fromScalar));
        }

        FieldId = fieldId;
        Getter = getter;
        SortComparer = sortComparer;
        ToScalar = toScalar;
        Setter = setter;
        FromScalar = setter is null ? null : fromScalar ?? DataGridLocalScalarConverter<TValue>.ConvertBack;
        Filters = filters.IsDefault ? ImmutableArray<DataGridLocalFilter<TValue>>.Empty : filters;

        var filterSchemas = ImmutableArray.CreateBuilder<DataGridFilterOperatorSchema>(Filters.Length);
        _filtersById = new Dictionary<DataGridOperatorId, DataGridLocalFilter<TValue>>(Filters.Length);
        foreach (var filter in Filters)
        {
            if (filter.Evaluate is null || filter.Schema is null)
            {
                throw new ArgumentException("Local filters cannot contain default values.", nameof(filters));
            }
            if (!_filtersById.TryAdd(filter.Operator, filter))
            {
                throw new ArgumentException(
                    $"Filter operator '{filter.Operator}' occurs more than once.", nameof(filters));
            }
            filterSchemas.Add(filter.Schema);
        }
        Schema = new DataGridFieldSchema(
            fieldId,
            typeof(TValue),
            sortDirections,
            filterSchemas.MoveToImmutable(),
            canGroup,
            DataGridFieldDisplayAccessor.Create(getter, propertyChangedName: propertyChangedName),
            isEditable: setter is not null);
    }

    public DataGridFieldId FieldId { get; }

    public Func<T, TValue> Getter { get; }

    public IComparer<TValue> SortComparer { get; }

    public Func<TValue, DataGridScalar> ToScalar { get; }

    public ImmutableArray<DataGridLocalFilter<TValue>> Filters { get; }

    /// <summary>
    /// Gets the setter that applies committed edits; null makes the field read-only.
    /// </summary>
    public Action<T, TValue>? Setter { get; }

    /// <summary>
    /// Gets the conversion from an edited scalar to the field value; null when the field is read-only.
    /// </summary>
    public Func<DataGridScalar, TValue>? FromScalar { get; }

    DataGridFieldSchema IDataGridLocalField<T>.Schema => Schema;

    internal DataGridFieldSchema Schema { get; }

    int IDataGridLocalField<T>.Compare(T left, T right) =>
        SortComparer.Compare(Getter(left), Getter(right));

    DataGridScalar IDataGridLocalField<T>.GetScalar(T item) => ToScalar(Getter(item));

    bool IDataGridLocalField<T>.Evaluate(T item, DataGridFilter filter)
    {
        if (!_filtersById.TryGetValue(filter.Operator, out var localFilter))
        {
            throw new DataGridSourceContractException(
                $"No local evaluator exists for operator '{filter.Operator}' on field '{FieldId}'.");
        }
        return localFilter.Evaluate(Getter(item), filter.Values);
    }

    Action<T> IDataGridLocalField<T>.PrepareWrite(DataGridScalar value)
    {
        if (Setter is not { } setter || FromScalar is not { } fromScalar)
        {
            throw new ArgumentException($"Field '{FieldId}' is not editable.");
        }
        TValue converted;
        try
        {
            converted = fromScalar(value);
        }
        catch (Exception exception) when (exception is not ArgumentException)
        {
            throw new ArgumentException(
                $"The value '{value.ToCanonicalString()}' cannot be written to field '{FieldId}'.", exception);
        }
        return item => setter(item, converted);
    }
}

public static class DataGridLocalSourceDescriptor
{
    public static DataGridLocalSourceDescriptor<T> For<T>(Func<T, DataGridRowKey> rowKey) =>
        new(rowKey);
}

public sealed class DataGridLocalSourceDescriptor<T>
{
    private readonly List<IDataGridLocalField<T>> _fields = [];
    private readonly HashSet<DataGridFieldId> _fieldIds = [];

    internal DataGridLocalSourceDescriptor(Func<T, DataGridRowKey> rowKey)
    {
        ArgumentNullException.ThrowIfNull(rowKey);
        RowKey = rowKey;
    }

    internal Func<T, DataGridRowKey> RowKey { get; }

    internal IReadOnlyList<IDataGridLocalField<T>> Fields => _fields;

    public DataGridLocalSourceDescriptor<T> Field<TValue>(
        string fieldId,
        Func<T, TValue> getter,
        IComparer<TValue>? sortComparer = null,
        Func<TValue, DataGridScalar>? toScalar = null,
        ImmutableArray<DataGridLocalFilter<TValue>> filters = default,
        DataGridSortDirections sortDirections = DataGridSortDirections.All,
        bool canGroup = false,
        string? propertyChangedName = null,
        Action<T, TValue>? setter = null,
        Func<DataGridScalar, TValue>? fromScalar = null) =>
        Field(
            new DataGridFieldId(fieldId),
            getter,
            sortComparer,
            toScalar,
            filters,
            sortDirections,
            canGroup,
            propertyChangedName,
            setter,
            fromScalar);

    public DataGridLocalSourceDescriptor<T> Field<TValue>(
        DataGridFieldId fieldId,
        Func<T, TValue> getter,
        IComparer<TValue>? sortComparer = null,
        Func<TValue, DataGridScalar>? toScalar = null,
        ImmutableArray<DataGridLocalFilter<TValue>> filters = default,
        DataGridSortDirections sortDirections = DataGridSortDirections.All,
        bool canGroup = false,
        string? propertyChangedName = null,
        Action<T, TValue>? setter = null,
        Func<DataGridScalar, TValue>? fromScalar = null)
    {
        if (!_fieldIds.Add(fieldId))
        {
            throw new ArgumentException($"Field '{fieldId}' occurs more than once.", nameof(fieldId));
        }
        _fields.Add(new DataGridLocalField<T, TValue>(
            fieldId,
            getter,
            sortComparer ?? Comparer<TValue>.Default,
            toScalar ?? DataGridLocalScalarConverter<TValue>.Convert,
            filters,
            sortDirections,
            canGroup,
            propertyChangedName,
            setter,
            fromScalar));
        return this;
    }
}

internal static class DataGridLocalScalarConverter<TValue>
{
    public static DataGridScalar Convert(TValue value)
    {
        object? boxed = value;
        return boxed switch
        {
            null => DataGridScalar.Null,
            bool item => DataGridScalar.FromBoolean(item),
            sbyte item => DataGridScalar.FromInt64(item),
            short item => DataGridScalar.FromInt64(item),
            int item => DataGridScalar.FromInt64(item),
            long item => DataGridScalar.FromInt64(item),
            byte item => DataGridScalar.FromUInt64(item),
            ushort item => DataGridScalar.FromUInt64(item),
            uint item => DataGridScalar.FromUInt64(item),
            ulong item => DataGridScalar.FromUInt64(item),
            float item => DataGridScalar.FromDouble(item),
            double item => DataGridScalar.FromDouble(item),
            decimal item => DataGridScalar.FromDecimal(item),
            string item => DataGridScalar.FromString(item),
            Guid item => DataGridScalar.FromGuid(item),
            DateOnly item => DataGridScalar.FromDateOnly(item),
            TimeOnly item => DataGridScalar.FromTimeOnly(item),
            DateTimeOffset item => DataGridScalar.FromDateTimeOffset(item),
            _ => throw new InvalidOperationException(
                $"No default DataGridScalar conversion exists for '{typeof(TValue)}'. Supply an explicit converter.")
        };
    }

    public static TValue ConvertBack(DataGridScalar scalar)
    {
        var value = scalar.ToValue();
        if (value is null)
        {
            if (default(TValue) is null)
            {
                return default!;
            }
            throw new ArgumentException($"A null value cannot be written to '{typeof(TValue)}'.");
        }
        if (value is TValue typed)
        {
            return typed;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
        if (IsNumericKind(scalar.Kind) && IsNumericType(targetType))
        {
            if (IsIntegralType(targetType) && value is double or decimal &&
                System.Convert.ToDecimal(value, CultureInfo.InvariantCulture) % 1 != 0)
            {
                throw new ArgumentException($"The value '{value}' is not an integer.");
            }
            try
            {
                return (TValue)System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch (OverflowException exception)
            {
                throw new ArgumentException($"The value '{value}' is out of range for '{targetType}'.", exception);
            }
        }
        throw new ArgumentException(
            $"A scalar of kind '{scalar.Kind}' cannot be written to '{typeof(TValue)}'. Supply an explicit converter.");
    }

    private static bool IsNumericKind(DataGridScalarKind kind) =>
        kind is DataGridScalarKind.SignedInteger or DataGridScalarKind.UnsignedInteger or
            DataGridScalarKind.Double or DataGridScalarKind.Decimal;

    private static bool IsNumericType(Type type) =>
        IsIntegralType(type) || type == typeof(float) || type == typeof(double) || type == typeof(decimal);

    private static bool IsIntegralType(Type type) =>
        type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long) ||
        type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);
}
