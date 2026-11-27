namespace AtomUI.Controls.Utils;

/// <summary>
/// Creates the built-in <see cref="IValueFilter" /> for each <see cref="ValueFilterMode" />.
/// </summary>
public static class ValueFilterFactory
{
    /// <summary>
    /// Creates the built-in filter for a matching mode.
    /// </summary>
    /// <param name="mode">
    /// The built-in matching rule of the filter.
    /// </param>
    /// <returns>
    /// A new filter for <paramref name="mode" />, or <see langword="null" /> when the mode has none:
    /// <see cref="ValueFilterMode.None" />, <see cref="ValueFilterMode.Custom" /> or an undefined value.
    /// </returns>
    /// <remarks>
    /// The built-in filters hold no state, so one instance can be shared.
    /// </remarks>
    public static IValueFilter? BuildFilter(ValueFilterMode mode)
    {
        switch (mode)
        {
            case ValueFilterMode.Contains:
                return new StringContainsFilter();

            case ValueFilterMode.ContainsCaseSensitive:
                return new StringContainsCaseSensitiveFilter();

            case ValueFilterMode.ContainsOrdinal:
                return new StringContainsOrdinalFilter();

            case ValueFilterMode.ContainsOrdinalCaseSensitive:
                return new StringContainsOrdinalCaseSensitiveFilter();

            case ValueFilterMode.Equals:
                return new StringEqualsFilter();

            case ValueFilterMode.EqualsCaseSensitive:
                return new StringEqualsCaseSensitiveFilter();

            case ValueFilterMode.EqualsOrdinal:
                return new StringEqualsOrdinalFilter();

            case ValueFilterMode.EqualsOrdinalCaseSensitive:
                return new StringEqualsOrdinalCaseSensitiveFilter();

            case ValueFilterMode.StartsWith:
                return new StringStartsWithFilter();

            case ValueFilterMode.StartsWithCaseSensitive:
                return new StringStartsWithCaseSensitiveFilter();

            case ValueFilterMode.StartsWithOrdinal:
                return new StringStartsWithOrdinalFilter();

            case ValueFilterMode.StartsWithOrdinalCaseSensitive:
                return new StringStartsWithOrdinalCaseSensitiveFilter();

            case ValueFilterMode.None:
            case ValueFilterMode.Custom:
            default:
                return null;
        }
    }
}