using System.Collections.Immutable;
using System.Globalization;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Pairs a filter operator with the predicate that applies it to the value of one field of a local source.
/// </summary>
/// <typeparam name="TValue">
/// Type of the field value that the predicate receives.
/// </typeparam>
public readonly struct DataGridLocalFilter<TValue>
{
    /// <summary>
    /// Creates a filter that checks query filters by value count and scalar kind, and evaluates them with a predicate.
    /// </summary>
    /// <param name="operator">
    /// The operator that a query filter names to be evaluated by the predicate.
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
    /// <param name="evaluate">
    /// The predicate that evaluates one field value against the values of the filter.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="evaluate" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The operator is not a valid identity, or <paramref name="acceptedKinds" /> is
    /// <see cref="DataGridScalarKinds.None" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="minimumValueCount" /> is negative, <paramref name="maximumValueCount" /> is less than
    /// <paramref name="minimumValueCount" /> or greater than 1024, or <paramref name="acceptedKinds" /> contains
    /// a kind outside <see cref="DataGridScalarKinds.All" />.
    /// </exception>
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

    /// <summary>
    /// Gets the operator of the query filters that <see cref="Evaluate" /> evaluates.
    /// </summary>
    public DataGridOperatorId Operator => Schema.Id;

    /// <summary>
    /// Gets the fewest values that a filter with this operator must carry to be valid.
    /// </summary>
    public int MinimumValueCount => Schema.MinimumValueCount;

    /// <summary>
    /// Gets the most values that a filter with this operator may carry to be valid.
    /// </summary>
    public int MaximumValueCount => Schema.MaximumValueCount;

    /// <summary>
    /// Gets the scalar kinds that every value of a filter with this operator may have.
    /// </summary>
    public DataGridScalarKinds AcceptedKinds => Schema.AcceptedKinds;

    /// <summary>
    /// Gets the predicate that tests one field value against the values of a query filter.
    /// </summary>
    /// <value>
    /// Receives the field value and the filter's values, and returns <see langword="true" /> when the row matches.
    /// A local source leaves a row out of the result when any filter returns <see langword="false" />.
    /// </value>
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

/// <summary>
/// Defines one field of a local source: how its value is read, compared, filtered and edited on an item.
/// The <c>Field</c> methods of <see cref="DataGridLocalSourceDescriptor{T}" /> create and collect these fields.
/// </summary>
/// <typeparam name="T">
/// Type of the items of the local source.
/// </typeparam>
/// <typeparam name="TValue">
/// Type of the field value.
/// </typeparam>
public sealed class DataGridLocalField<T, TValue> : IDataGridLocalField<T>
{
    private readonly Dictionary<DataGridOperatorId, DataGridLocalFilter<TValue>> _filtersById;

    /// <summary>
    /// Creates a field for a local source and validates its configuration.
    /// </summary>
    /// <param name="fieldId">
    /// Identity that queries and edits use to address the field.
    /// </param>
    /// <param name="getter">
    /// Reads the field value from an item, for display, sorting and filtering.
    /// </param>
    /// <param name="sortComparer">
    /// Orders two field values of the items that the grid sorts.
    /// </param>
    /// <param name="toScalar">
    /// Converts a field value to the <see cref="DataGridScalar" /> that the grid reads for the item.
    /// </param>
    /// <param name="filters">
    /// Operators that the field supports for filtering, each with its predicate. The default value means none.
    /// </param>
    /// <param name="sortDirections">
    /// Directions in which the field can be sorted.
    /// </param>
    /// <param name="canGroup">
    /// <see langword="true" /> to let rows be grouped by the field; the field must then support a sort direction.
    /// </param>
    /// <param name="propertyChangedName">
    /// Name of the item property whose change notification refreshes the field, or <see langword="null" /> to
    /// refresh on every change.
    /// </param>
    /// <param name="setter">
    /// Writes a committed value to an item. <see langword="null" /> makes the field read-only.
    /// </param>
    /// <param name="fromScalar">
    /// Converts an edited scalar to the field value. <see langword="null" /> uses the built-in conversion, which
    /// accepts a value of the field's own type and converts between numeric types. A value requires
    /// <paramref name="setter" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getter" />, <paramref name="sortComparer" /> or <paramref name="toScalar" /> is
    /// <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="fromScalar" /> is set while <paramref name="setter" /> is <see langword="null" />,
    /// <paramref name="filters" /> contains a default value or two filters with the same operator,
    /// <paramref name="fieldId" /> is not a valid identity, or <paramref name="canGroup" /> is
    /// <see langword="true" /> while <paramref name="sortDirections" /> is <see cref="DataGridSortDirections.None" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="sortDirections" /> contains a value outside <see cref="DataGridSortDirections.All" />.
    /// </exception>
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

    /// <summary>
    /// Gets the identity that queries and edits use to address the field.
    /// </summary>
    public DataGridFieldId FieldId { get; }

    /// <summary>
    /// Gets the function that reads the field value from an item; sorting, filtering and display use it.
    /// </summary>
    public Func<T, TValue> Getter { get; }

    /// <summary>
    /// Gets the comparer that orders the field values of two items when the grid sorts by the field.
    /// </summary>
    public IComparer<TValue> SortComparer { get; }

    /// <summary>
    /// Gets the function that converts a field value to the <see cref="DataGridScalar" /> the grid reads for the item.
    /// </summary>
    public Func<TValue, DataGridScalar> ToScalar { get; }

    /// <summary>
    /// Gets the operators that the field supports for filtering, each with its predicate.
    /// The array is empty when the field cannot be filtered.
    /// </summary>
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

/// <summary>
/// Entry point that creates a <see cref="DataGridLocalSourceDescriptor{T}" /> for a local source.
/// </summary>
public static class DataGridLocalSourceDescriptor
{
    /// <summary>
    /// Creates a descriptor for the rows of a local source, without fields.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the items of the local source.
    /// </typeparam>
    /// <param name="rowKey">
    /// Returns the key that identifies a row when a request addresses it, for example to move the row.
    /// </param>
    /// <returns>
    /// A descriptor without fields. Add its fields with <c>Field</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="rowKey" /> is <see langword="null" />.
    /// </exception>
    public static DataGridLocalSourceDescriptor<T> For<T>(Func<T, DataGridRowKey> rowKey) =>
        new(rowKey);
}

/// <summary>
/// Describes the fields of a local source and how its rows are identified.
/// Create it with <c>DataGridLocalSourceDescriptor.For</c> and pass it to a <c>DataGridLocalSource&lt;T&gt;</c>.
/// </summary>
/// <typeparam name="T">
/// Type of the items of the local source.
/// </typeparam>
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

    /// <summary>
    /// Adds a field that queries and edits address by a name.
    /// </summary>
    /// <typeparam name="TValue">
    /// Type of the field value.
    /// </typeparam>
    /// <param name="fieldId">
    /// Name of the field. The <see cref="DataGridFieldId" /> created from it validates the name.
    /// </param>
    /// <param name="getter">
    /// Reads the field value from an item, for display, sorting and filtering.
    /// </param>
    /// <param name="sortComparer">
    /// Orders two field values of the items that the grid sorts. <see langword="null" /> uses the default comparer
    /// of the type.
    /// </param>
    /// <param name="toScalar">
    /// Converts a field value to the <see cref="DataGridScalar" /> that the grid reads for the item.
    /// <see langword="null" /> uses the built-in conversion, which supports Boolean, numeric and string values,
    /// <c>Guid</c> and date and time types. Other types need an explicit converter.
    /// </param>
    /// <param name="filters">
    /// Operators that the field supports for filtering, each with its predicate. The default value means none.
    /// </param>
    /// <param name="sortDirections">
    /// Directions in which the field can be sorted.
    /// </param>
    /// <param name="canGroup">
    /// <see langword="true" /> to let rows be grouped by the field; the field must then support a sort direction.
    /// </param>
    /// <param name="propertyChangedName">
    /// Name of the item property whose change notification refreshes the field, or <see langword="null" /> to
    /// refresh on every change.
    /// </param>
    /// <param name="setter">
    /// Writes a committed value to an item. <see langword="null" /> makes the field read-only.
    /// </param>
    /// <param name="fromScalar">
    /// Converts an edited scalar to the field value. <see langword="null" /> uses the built-in conversion, which
    /// accepts a value of the field's own type and converts between numeric types. A value requires
    /// <paramref name="setter" />.
    /// </param>
    /// <returns>
    /// This descriptor, so that calls can be chained.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getter" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// A field with this identity was added before, or an argument is rejected as documented for the
    /// <see cref="DataGridLocalField{T, TValue}" /> constructor.
    /// </exception>
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

    /// <summary>
    /// Adds a field that queries and edits address by an identity.
    /// </summary>
    /// <typeparam name="TValue">
    /// Type of the field value.
    /// </typeparam>
    /// <param name="fieldId">
    /// Identity that queries and edits use to address the field.
    /// </param>
    /// <param name="getter">
    /// Reads the field value from an item, for display, sorting and filtering.
    /// </param>
    /// <param name="sortComparer">
    /// Orders two field values of the items that the grid sorts. <see langword="null" /> uses the default comparer
    /// of the type.
    /// </param>
    /// <param name="toScalar">
    /// Converts a field value to the <see cref="DataGridScalar" /> that the grid reads for the item.
    /// <see langword="null" /> uses the built-in conversion, which supports Boolean, numeric and string values,
    /// <c>Guid</c> and date and time types. Other types need an explicit converter.
    /// </param>
    /// <param name="filters">
    /// Operators that the field supports for filtering, each with its predicate. The default value means none.
    /// </param>
    /// <param name="sortDirections">
    /// Directions in which the field can be sorted.
    /// </param>
    /// <param name="canGroup">
    /// <see langword="true" /> to let rows be grouped by the field; the field must then support a sort direction.
    /// </param>
    /// <param name="propertyChangedName">
    /// Name of the item property whose change notification refreshes the field, or <see langword="null" /> to
    /// refresh on every change.
    /// </param>
    /// <param name="setter">
    /// Writes a committed value to an item. <see langword="null" /> makes the field read-only.
    /// </param>
    /// <param name="fromScalar">
    /// Converts an edited scalar to the field value. <see langword="null" /> uses the built-in conversion, which
    /// accepts a value of the field's own type and converts between numeric types. A value requires
    /// <paramref name="setter" />.
    /// </param>
    /// <returns>
    /// This descriptor, so that calls can be chained.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getter" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// A field with this identity was added before, or an argument is rejected as documented for the
    /// <see cref="DataGridLocalField{T, TValue}" /> constructor.
    /// </exception>
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
