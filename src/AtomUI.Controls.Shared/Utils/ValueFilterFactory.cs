namespace AtomUI.Controls.Utils;

public static class ValueFilterFactory
{
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