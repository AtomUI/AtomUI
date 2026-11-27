namespace AtomUI.Controls.Utils;

/// <summary>
/// Base class for filters that test whether the value contains the filter text. A derived class fixes the
/// comparison in its <see cref="Filter" /> override.
/// </summary>
public abstract class StringAbstractContainsFilter : IValueFilter
{
    /// <summary>
    /// Gets the matching rule that was passed to the constructor.
    /// </summary>
    public ValueFilterMode Mode { get; }
    
    /// <summary>
    /// Initializes a filter that reports the given matching rule.
    /// </summary>
    /// <param name="mode">
    /// The value of <see cref="Mode" />. It does not change the comparison, which each derived class fixes.
    /// </param>
    public StringAbstractContainsFilter(ValueFilterMode mode)
    {
        Mode = mode;
    }
    
    /// <summary>
    /// Tests whether the text of <paramref name="value" /> contains the text of <paramref name="filterValue" />.
    /// </summary>
    /// <param name="value">
    /// The value to search in. It is compared through its <see cref="object.ToString" /> result.
    /// </param>
    /// <param name="filterValue">
    /// The text to search for. It is compared through its <see cref="object.ToString" /> result.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the value contains the filter text; <see langword="false" /> when it does not or
    /// when either argument is <see langword="null" />.
    /// </returns>
    public abstract bool Filter(object? value, object? filterValue);

    /// <summary>
    /// Tests whether <paramref name="value" /> contains <paramref name="filterValue" />.
    /// </summary>
    /// <param name="value">
    /// The text to search in.
    /// </param>
    /// <param name="filterValue">
    /// The text to search for.
    /// </param>
    /// <param name="comparison">
    /// The rule that sets the culture and the case sensitivity of the comparison.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when <paramref name="value" /> contains <paramref name="filterValue" />;
    /// <see langword="false" /> when either argument is <see langword="null" />.
    /// </returns>
    protected static bool Contains(string? value, string? filterValue, StringComparison comparison)
    {
        if (value is not null && filterValue is not null)
        {
            return value.IndexOf(filterValue, comparison) >= 0;
        }
        return false;
    }
}

/// <summary>
/// The built-in filter of <see cref="ValueFilterMode.Contains" />: a culture-sensitive, case-insensitive test of
/// whether the value contains the filter text.
/// </summary>
public class StringContainsFilter : StringAbstractContainsFilter
{
    public StringContainsFilter()
        : base(ValueFilterMode.Contains)
    {
    }
    
    /// <inheritdoc />
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }
}

/// <summary>
/// The built-in filter of <see cref="ValueFilterMode.ContainsCaseSensitive" />: a culture-sensitive, case-sensitive
/// test of whether the value contains the filter text.
/// </summary>
public class StringContainsCaseSensitiveFilter : StringAbstractContainsFilter
{
    public StringContainsCaseSensitiveFilter()
        : base(ValueFilterMode.ContainsCaseSensitive)
    {}
    
    /// <inheritdoc />
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCulture);
    }
}

/// <summary>
/// The built-in filter of <see cref="ValueFilterMode.ContainsOrdinal" />: an ordinal, case-insensitive test of
/// whether the value contains the filter text.
/// </summary>
public class StringContainsOrdinalFilter : StringAbstractContainsFilter
{
    public StringContainsOrdinalFilter()
        : base(ValueFilterMode.ContainsOrdinal)
    {}
    
    /// <inheritdoc />
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// The built-in filter of <see cref="ValueFilterMode.ContainsOrdinalCaseSensitive" />: an ordinal, case-sensitive
/// test of whether the value contains the filter text.
/// </summary>
public class StringContainsOrdinalCaseSensitiveFilter : StringAbstractContainsFilter
{
    public StringContainsOrdinalCaseSensitiveFilter()
        : base(ValueFilterMode.ContainsOrdinalCaseSensitive)
    {}
    
    /// <inheritdoc />
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.Ordinal);
    }
}

/// <summary>
/// The built-in filter of <see cref="ValueFilterMode.Equals" />: a culture-sensitive, case-insensitive test of
/// whether the text of the value equals the filter text.
/// </summary>
public class StringEqualsFilter : IValueFilter
{
    /// <inheritdoc />
    public ValueFilterMode Mode => ValueFilterMode.Equals;
    
    /// <summary>
    /// Tests whether the text of <paramref name="value" /> equals the text of <paramref name="filterValue" /> with a
    /// culture-sensitive, case-insensitive comparison. Both texts come from <see cref="object.ToString" />, and two
    /// <see langword="null" /> arguments are equal.
    /// </summary>
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }
}

/// <summary>
/// The built-in filter of <see cref="ValueFilterMode.EqualsCaseSensitive" />: a culture-sensitive, case-sensitive
/// test of whether the text of the value equals the filter text.
/// </summary>
public class StringEqualsCaseSensitiveFilter : IValueFilter
{
    /// <inheritdoc />
    public ValueFilterMode Mode => ValueFilterMode.EqualsCaseSensitive;
    /// <summary>
    /// Tests whether the text of <paramref name="value" /> equals the text of <paramref name="filterValue" /> with a
    /// culture-sensitive, case-sensitive comparison. Both texts come from <see cref="object.ToString" />, and two
    /// <see langword="null" /> arguments are equal.
    /// </summary>
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCulture);
    }
}

/// <summary>
/// The built-in filter of <see cref="ValueFilterMode.EqualsOrdinal" />: an ordinal, case-insensitive test of
/// whether the text of the value equals the filter text.
/// </summary>
public class StringEqualsOrdinalFilter : IValueFilter
{
    /// <inheritdoc />
    public ValueFilterMode Mode => ValueFilterMode.EqualsOrdinal;
    /// <summary>
    /// Tests whether the text of <paramref name="value" /> equals the text of <paramref name="filterValue" /> with an
    /// ordinal, case-insensitive comparison. Both texts come from <see cref="object.ToString" />, and two
    /// <see langword="null" /> arguments are equal.
    /// </summary>
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// The built-in filter of <see cref="ValueFilterMode.EqualsOrdinalCaseSensitive" />: an ordinal, case-sensitive
/// test of whether the text of the value equals the filter text.
/// </summary>
public class StringEqualsOrdinalCaseSensitiveFilter : IValueFilter
{
    /// <inheritdoc />
    public ValueFilterMode Mode => ValueFilterMode.EqualsOrdinalCaseSensitive;
    /// <summary>
    /// Tests whether the text of <paramref name="value" /> equals the text of <paramref name="filterValue" /> with an
    /// ordinal, case-sensitive comparison. Both texts come from <see cref="object.ToString" />, and two
    /// <see langword="null" /> arguments are equal.
    /// </summary>
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.Ordinal);
    }
}

/// <summary>
/// The built-in filter of <see cref="ValueFilterMode.StartsWith" />: a culture-sensitive, case-insensitive test
/// of whether the value starts with the filter text.
/// </summary>
public class StringStartsWithFilter : IValueFilter
{
    /// <inheritdoc />
    public ValueFilterMode Mode => ValueFilterMode.StartsWith;
    /// <summary>
    /// Tests whether <paramref name="value" /> starts with <paramref name="filterValue" /> with a culture-sensitive,
    /// case-insensitive comparison. Both arguments must be strings; the filter returns <see langword="false" /> for a
    /// <see langword="null" /> or non-string argument.
    /// </summary>
    public bool Filter(object? value, object? filterValue)
    {
        if (value is string valueStr && filterValue is string filterValueStr)
        {
            return valueStr.StartsWith(filterValueStr, StringComparison.CurrentCultureIgnoreCase);
        }
        return false;
    }
}

/// <summary>
/// The built-in filter of <see cref="ValueFilterMode.StartsWithCaseSensitive" />: a culture-sensitive,
/// case-sensitive test of whether the value starts with the filter text.
/// </summary>
public class StringStartsWithCaseSensitiveFilter : IValueFilter
{
    /// <inheritdoc />
    public ValueFilterMode Mode => ValueFilterMode.StartsWithCaseSensitive;
    /// <summary>
    /// Tests whether <paramref name="value" /> starts with <paramref name="filterValue" /> with a culture-sensitive,
    /// case-sensitive comparison. Both arguments must be strings; the filter returns <see langword="false" /> for a
    /// <see langword="null" /> or non-string argument.
    /// </summary>
    public bool Filter(object? value, object? filterValue)
    {
        if (value is string valueStr && filterValue is string filterValueStr)
        {
            return valueStr.StartsWith(filterValueStr, StringComparison.CurrentCulture);
        }
        return false;
    }
}

/// <summary>
/// The built-in filter of <see cref="ValueFilterMode.StartsWithOrdinal" />: an ordinal, case-insensitive test of
/// whether the value starts with the filter text.
/// </summary>
public class StringStartsWithOrdinalFilter : IValueFilter
{
    /// <inheritdoc />
    public ValueFilterMode Mode => ValueFilterMode.StartsWithOrdinal;
    /// <summary>
    /// Tests whether <paramref name="value" /> starts with <paramref name="filterValue" /> with an ordinal,
    /// case-insensitive comparison. Both arguments must be strings; the filter returns <see langword="false" /> for a
    /// <see langword="null" /> or non-string argument.
    /// </summary>
    public bool Filter(object? value, object? filterValue)
    {
        if (value is string valueStr && filterValue is string filterValueStr)
        {
            return valueStr.StartsWith(filterValueStr, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}

/// <summary>
/// The built-in filter of <see cref="ValueFilterMode.StartsWithOrdinalCaseSensitive" />: an ordinal, case-sensitive
/// test of whether the value starts with the filter text.
/// </summary>
public class StringStartsWithOrdinalCaseSensitiveFilter : IValueFilter
{
    /// <inheritdoc />
    public ValueFilterMode Mode => ValueFilterMode.StartsWithOrdinalCaseSensitive;
    /// <summary>
    /// Tests whether <paramref name="value" /> starts with <paramref name="filterValue" /> with an ordinal,
    /// case-sensitive comparison. Both arguments must be strings; the filter returns <see langword="false" /> for a
    /// <see langword="null" /> or non-string argument.
    /// </summary>
    public bool Filter(object? value, object? filterValue)
    {
        if (value is string valueStr && filterValue is string filterValueStr)
        {
            return valueStr.StartsWith(filterValueStr, StringComparison.Ordinal);
        }
        return false;
    }
}