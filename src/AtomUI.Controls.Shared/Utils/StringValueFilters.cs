namespace AtomUI.Controls.Utils;

public abstract class StringAbstractContainsFilter : IValueFilter
{
    public abstract bool Filter(object? value, object? filterValue);

    protected static bool Contains(string? value, string? filterValue, StringComparison comparison)
    {
        if (value is not null && filterValue is not null)
        {
            return value.IndexOf(filterValue, comparison) >= 0;
        }
        return false;
    }
}

public class StringContainsFilter : StringAbstractContainsFilter
{
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }
}

public class StringContainsCaseSensitiveFilter : StringAbstractContainsFilter
{
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCulture);
    }
}

public class StringContainsOrdinalFilter : StringAbstractContainsFilter
{
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}

public class StringContainsOrdinalCaseSensitiveFilter : StringAbstractContainsFilter
{
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.Ordinal);
    }
}

public class StringEqualsFilter : IValueFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }
}

public class StringEqualsCaseSensitiveFilter : IValueFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCulture);
    }
}

public class StringEqualsOrdinalFilter : IValueFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}

public class StringEqualsOrdinalCaseSensitiveFilter : IValueFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.Ordinal);
    }
}

public class StringStartsWithFilter : IValueFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        if (value is string valueStr && filterValue is string filterValueStr)
        {
            return valueStr.StartsWith(filterValueStr, StringComparison.CurrentCultureIgnoreCase);
        }
        return false;
    }
}

public class StringStartsWithCaseSensitiveFilter : IValueFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        if (value is string valueStr && filterValue is string filterValueStr)
        {
            return valueStr.StartsWith(filterValueStr, StringComparison.CurrentCulture);
        }
        return false;
    }
}

public class StringStartsWithOrdinalFilter : IValueFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        if (value is string valueStr && filterValue is string filterValueStr)
        {
            return valueStr.StartsWith(filterValueStr, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}

public class StringStartsWithOrdinalCaseSensitiveFilter : IValueFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        if (value is string valueStr && filterValue is string filterValueStr)
        {
            return valueStr.StartsWith(filterValueStr, StringComparison.Ordinal);
        }
        return false;
    }
}