using System.Collections.Immutable;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The order in which a sort or a group level arranges the values of a field.
/// </summary>
public enum DataGridSortDirection
{
    /// <summary>
    /// Orders the values from the smaller one to the larger one.
    /// </summary>
    Ascending,
    /// <summary>
    /// Orders the values from the larger one to the smaller one.
    /// </summary>
    Descending
}
/// <summary>
/// Selects what a call to <c>SetSort</c> does to the other sorts of the <see cref="DataGridQuery" />.
/// </summary>
public enum DataGridSortUpdateMode
{
    /// <summary>
    /// Discards the other sorts: the field becomes the only sort of the query.
    /// </summary>
    Replace,
    /// <summary>
    /// Keeps the other sorts. The sort of the field is replaced when the field is already sorted, and appended
    /// otherwise.
    /// </summary>
    AppendOrReplace
}

/// <summary>
/// One level of the sorts of a <see cref="DataGridQuery" />: a field and the direction in which its values are
/// ordered.
/// </summary>
public readonly struct DataGridSort : IEquatable<DataGridSort>
{
    /// <summary>
    /// Creates a sort of <paramref name="field" /> in <paramref name="direction" />.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="field" /> is not valid.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="direction" /> is not one of the <see cref="DataGridSortDirection" /> values.
    /// </exception>
    public DataGridSort(DataGridFieldId field, DataGridSortDirection direction)
    {
        if (!field.IsValid)
        {
            throw new ArgumentException("The field identity must be valid.", nameof(field));
        }
        DataGridQueryValidation.ValidateDirection(direction, nameof(direction));
        Field = field;
        Direction = direction;
    }

    /// <summary>
    /// The field whose values this sort orders.
    /// </summary>
    public DataGridFieldId Field { get; }

    /// <summary>
    /// The direction in which the values of <see cref="Field" /> are ordered.
    /// </summary>
    public DataGridSortDirection Direction { get; }

    /// <summary>
    /// Returns whether <paramref name="other" /> orders the same field in the same direction.
    /// </summary>
    public bool Equals(DataGridSort other) => Field == other.Field && Direction == other.Direction;

    /// <summary>
    /// Returns whether <paramref name="obj" /> is a <see cref="DataGridSort" /> that equals this instance.
    /// </summary>
    public override bool Equals(object? obj) => obj is DataGridSort other && Equals(other);

    /// <summary>
    /// Returns a hash code that combines <see cref="Field" /> and <see cref="Direction" />.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Field, Direction);

    /// <summary>
    /// Determines whether two sorts order the same field in the same direction.
    /// </summary>
    public static bool operator ==(DataGridSort left, DataGridSort right) => left.Equals(right);

    /// <summary>
    /// Determines whether two sorts differ in field or direction.
    /// </summary>
    public static bool operator !=(DataGridSort left, DataGridSort right) => !left.Equals(right);
}

/// <summary>
/// One filter of a <see cref="DataGridQuery" />: the field, the operator that is applied to it, and the values
/// that the operator compares with the field values.
/// </summary>
public readonly struct DataGridFilter : IEquatable<DataGridFilter>
{
    /// <summary>
    /// Creates a filter that applies the operator to the field with the given values.
    /// A <see langword="default" /> value array is stored as an empty one.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="field" /> is not valid, or the operator identity is not valid.
    /// </exception>
    public DataGridFilter(
        DataGridFieldId field,
        DataGridOperatorId @operator,
        ImmutableArray<DataGridScalar> values)
    {
        if (!field.IsValid)
        {
            throw new ArgumentException("The field identity must be valid.", nameof(field));
        }
        if (!@operator.IsValid)
        {
            throw new ArgumentException("The operator identity must be valid.", nameof(@operator));
        }
        Field = field;
        Operator = @operator;
        Values = values.IsDefault ? ImmutableArray<DataGridScalar>.Empty : values;
    }

    /// <summary>
    /// The field whose values the operator compares with <see cref="Values" />.
    /// </summary>
    public DataGridFieldId Field { get; }

    /// <summary>
    /// The operator that is applied to the field. Each field supports only the operators that were declared for it.
    /// </summary>
    public DataGridOperatorId Operator { get; }

    /// <summary>
    /// The values that <see cref="Operator" /> compares with the field values; the operator decides how many are
    /// required. The array is never the default array.
    /// </summary>
    public ImmutableArray<DataGridScalar> Values { get; }

    /// <summary>
    /// Returns whether <paramref name="other" /> has the same field, the same operator and the same values in the
    /// same order.
    /// </summary>
    public bool Equals(DataGridFilter other) =>
        Field == other.Field &&
        Operator == other.Operator &&
        DataGridQueryValidation.SequenceEqual(Values, other.Values);

    /// <summary>
    /// Returns whether <paramref name="obj" /> is a <see cref="DataGridFilter" /> that equals this instance.
    /// </summary>
    public override bool Equals(object? obj) => obj is DataGridFilter other && Equals(other);

    /// <summary>
    /// Returns a hash code that combines <see cref="Field" />, <see cref="Operator" /> and the values in
    /// <see cref="Values" /> in order.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Field);
        hash.Add(Operator);
        foreach (var value in Values)
        {
            hash.Add(value);
        }
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines whether two filters have the same field, operator and values.
    /// </summary>
    public static bool operator ==(DataGridFilter left, DataGridFilter right) => left.Equals(right);

    /// <summary>
    /// Determines whether two filters differ in field, operator or values.
    /// </summary>
    public static bool operator !=(DataGridFilter left, DataGridFilter right) => !left.Equals(right);
}

/// <summary>
/// One group level of a <see cref="DataGridQuery" />: the field whose values form the groups, and the direction
/// in which the groups of the level are ordered.
/// </summary>
public readonly struct DataGridGroup : IEquatable<DataGridGroup>
{
    /// <summary>
    /// Creates a group level of <paramref name="field" /> in <paramref name="direction" />.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="field" /> is not valid.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="direction" /> is not one of the <see cref="DataGridSortDirection" /> values.
    /// </exception>
    public DataGridGroup(DataGridFieldId field, DataGridSortDirection direction)
    {
        if (!field.IsValid)
        {
            throw new ArgumentException("The field identity must be valid.", nameof(field));
        }
        DataGridQueryValidation.ValidateDirection(direction, nameof(direction));
        Field = field;
        Direction = direction;
    }

    /// <summary>
    /// The field whose values form the groups of this level.
    /// </summary>
    public DataGridFieldId Field { get; }

    /// <summary>
    /// The direction in which the groups of this level are ordered.
    /// </summary>
    public DataGridSortDirection Direction { get; }

    /// <summary>
    /// Returns whether <paramref name="other" /> groups the same field in the same direction.
    /// </summary>
    public bool Equals(DataGridGroup other) => Field == other.Field && Direction == other.Direction;

    /// <summary>
    /// Returns whether <paramref name="obj" /> is a <see cref="DataGridGroup" /> that equals this instance.
    /// </summary>
    public override bool Equals(object? obj) => obj is DataGridGroup other && Equals(other);

    /// <summary>
    /// Returns a hash code that combines <see cref="Field" /> and <see cref="Direction" />.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Field, Direction);

    /// <summary>
    /// Determines whether two group levels group the same field in the same direction.
    /// </summary>
    public static bool operator ==(DataGridGroup left, DataGridGroup right) => left.Equals(right);

    /// <summary>
    /// Determines whether two group levels differ in field or direction.
    /// </summary>
    public static bool operator !=(DataGridGroup left, DataGridGroup right) => !left.Equals(right);
}

/// <summary>
/// The query of a data grid: the sorts, the filters and the group levels that a data source applies to the rows.
/// It never changes after creation; the <c>With</c> methods return a changed copy.
/// </summary>
public sealed class DataGridQuery : IEquatable<DataGridQuery>
{
    private readonly int _structuralHashCode;

    /// <summary>
    /// A query without sorts, filters or groups.
    /// </summary>
    public static DataGridQuery Empty { get; } = new(default, default, default);

    /// <summary>
    /// Creates a query from the given arrays and validates them.
    /// A <see langword="default" /> array is stored as an empty one.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// A field identity is invalid, a field occurs more than once in one array, or a field occurs in both
    /// <paramref name="sorts" /> and <paramref name="groups" />.
    /// </exception>
    public DataGridQuery(
        ImmutableArray<DataGridSort> sorts,
        ImmutableArray<DataGridFilter> filters,
        ImmutableArray<DataGridGroup> groups)
    {
        Sorts = sorts.IsDefault ? ImmutableArray<DataGridSort>.Empty : sorts;
        Filters = filters.IsDefault ? ImmutableArray<DataGridFilter>.Empty : filters;
        Groups = groups.IsDefault ? ImmutableArray<DataGridGroup>.Empty : groups;

        DataGridQueryValidation.ValidateUniqueFields(Sorts, static sort => sort.Field, nameof(sorts));
        DataGridQueryValidation.ValidateUniqueFields(Filters, static filter => filter.Field, nameof(filters));
        DataGridQueryValidation.ValidateUniqueFields(Groups, static group => group.Field, nameof(groups));
        ValidateGroupSortOverlap();
        _structuralHashCode = ComputeStructuralHashCode();
    }

    /// <summary>
    /// The sorts in the order in which the row comparison applies them; the first sort has the highest priority.
    /// </summary>
    public ImmutableArray<DataGridSort> Sorts { get; }

    /// <summary>
    /// The filters of the query. Each field has at most one filter.
    /// </summary>
    public ImmutableArray<DataGridFilter> Filters { get; }

    /// <summary>
    /// The group levels, the outermost first.
    /// </summary>
    public ImmutableArray<DataGridGroup> Groups { get; }

    /// <summary>
    /// Returns a query that differs from this one only in its sorts.
    /// </summary>
    /// <param name="sorts">
    /// The new sorts, in the order of their priority. A <see langword="default" /> array means no sorts.
    /// </param>
    /// <returns>
    /// This instance when <paramref name="sorts" /> equals <see cref="Sorts" /> element by element; otherwise
    /// a new instance.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The new query is rejected by the <see cref="DataGridQuery" /> constructor.
    /// </exception>
    public DataGridQuery WithSorts(ImmutableArray<DataGridSort> sorts)
    {
        var normalized = sorts.IsDefault ? ImmutableArray<DataGridSort>.Empty : sorts;
        return DataGridQueryValidation.SequenceEqual(Sorts, normalized)
            ? this
            : new DataGridQuery(normalized, Filters, Groups);
    }

    /// <summary>
    /// Returns a query that differs from this one only in its filters.
    /// </summary>
    /// <param name="filters">
    /// The new filters. A <see langword="default" /> array means no filters.
    /// </param>
    /// <returns>
    /// This instance when <paramref name="filters" /> equals <see cref="Filters" /> element by element; otherwise
    /// a new instance.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The new query is rejected by the <see cref="DataGridQuery" /> constructor.
    /// </exception>
    public DataGridQuery WithFilters(ImmutableArray<DataGridFilter> filters)
    {
        var normalized = filters.IsDefault ? ImmutableArray<DataGridFilter>.Empty : filters;
        return DataGridQueryValidation.SequenceEqual(Filters, normalized)
            ? this
            : new DataGridQuery(Sorts, normalized, Groups);
    }

    /// <summary>
    /// Returns a query that differs from this one only in its groups.
    /// </summary>
    /// <param name="groups">
    /// The new group levels, the outermost first. A <see langword="default" /> array means no groups.
    /// </param>
    /// <returns>
    /// This instance when <paramref name="groups" /> equals <see cref="Groups" /> element by element; otherwise
    /// a new instance.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The new query is rejected by the <see cref="DataGridQuery" /> constructor.
    /// </exception>
    public DataGridQuery WithGroups(ImmutableArray<DataGridGroup> groups)
    {
        var normalized = groups.IsDefault ? ImmutableArray<DataGridGroup>.Empty : groups;
        return DataGridQueryValidation.SequenceEqual(Groups, normalized)
            ? this
            : new DataGridQuery(Sorts, Filters, normalized);
    }

    /// <summary>
    /// Returns whether <paramref name="other" /> is this instance, or is not <see langword="null" /> and has the
    /// same sorts, filters and groups, element by element in the same order.
    /// </summary>
    public bool Equals(DataGridQuery? other) =>
        ReferenceEquals(this, other) ||
        other is not null &&
        _structuralHashCode == other._structuralHashCode &&
        DataGridQueryValidation.SequenceEqual(Sorts, other.Sorts) &&
        DataGridQueryValidation.SequenceEqual(Filters, other.Filters) &&
        DataGridQueryValidation.SequenceEqual(Groups, other.Groups);

    /// <summary>
    /// Returns whether <paramref name="obj" /> is a <see cref="DataGridQuery" /> that equals this instance.
    /// </summary>
    public override bool Equals(object? obj) => obj is DataGridQuery other && Equals(other);

    /// <summary>
    /// Returns the hash code over the sorts, filters and groups. It is computed once, when the instance is created.
    /// </summary>
    public override int GetHashCode() => _structuralHashCode;

    /// <summary>
    /// Determines whether two queries are equal, or are both <see langword="null" />.
    /// </summary>
    public static bool operator ==(DataGridQuery? left, DataGridQuery? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>
    /// Determines whether two queries differ, or exactly one of them is <see langword="null" />.
    /// </summary>
    public static bool operator !=(DataGridQuery? left, DataGridQuery? right) => !(left == right);

    private void ValidateGroupSortOverlap()
    {
        if (Groups.IsEmpty || Sorts.IsEmpty)
        {
            return;
        }
        var groupFields = new HashSet<DataGridFieldId>();
        foreach (var group in Groups)
        {
            groupFields.Add(group.Field);
        }
        foreach (var sort in Sorts)
        {
            if (groupFields.Contains(sort.Field))
            {
                throw new ArgumentException(
                    $"Field '{sort.Field}' cannot be present in both groups and sorts.",
                    nameof(Sorts));
            }
        }
    }

    private int ComputeStructuralHashCode()
    {
        var hash = new HashCode();
        hash.Add(Sorts.Length);
        foreach (var sort in Sorts)
        {
            hash.Add(sort);
        }
        hash.Add(Filters.Length);
        foreach (var filter in Filters)
        {
            hash.Add(filter);
        }
        hash.Add(Groups.Length);
        foreach (var group in Groups)
        {
            hash.Add(group);
        }
        return hash.ToHashCode();
    }
}

internal static class DataGridQueryValidation
{
    public static void ValidateDirection(DataGridSortDirection direction, string parameterName)
    {
        if (direction is not DataGridSortDirection.Ascending and not DataGridSortDirection.Descending)
        {
            throw new ArgumentOutOfRangeException(parameterName, direction, "Unknown sort direction.");
        }
    }

    public static void ValidateUniqueFields<T>(
        ImmutableArray<T> values,
        Func<T, DataGridFieldId> getField,
        string parameterName)
    {
        var fields = new HashSet<DataGridFieldId>();
        foreach (var value in values)
        {
            var field = getField(value);
            if (!field.IsValid)
            {
                throw new ArgumentException("Every field identity must be valid.", parameterName);
            }
            if (!fields.Add(field))
            {
                throw new ArgumentException($"Field '{field}' occurs more than once.", parameterName);
            }
        }
    }

    public static bool SequenceEqual<T>(ImmutableArray<T> left, ImmutableArray<T> right)
        where T : IEquatable<T>
    {
        if (left.Length != right.Length)
        {
            return false;
        }
        for (var index = 0; index < left.Length; index++)
        {
            if (!left[index].Equals(right[index]))
            {
                return false;
            }
        }
        return true;
    }
}
