namespace AtomUI.Controls.Utils;

public abstract class StringAbstractContainsFilter : IValueFilter
{
    public ValueFilterMode Mode { get; }
    
    public StringAbstractContainsFilter(ValueFilterMode mode)
    {
        Mode = mode;
    }
    
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
    public StringContainsFilter()
        : base(ValueFilterMode.Contains)
    {
    }
    
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }
}

public class StringContainsCaseSensitiveFilter : StringAbstractContainsFilter
{
    public StringContainsCaseSensitiveFilter()
        : base(ValueFilterMode.ContainsCaseSensitive)
    {}
    
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCulture);
    }
}

public class StringContainsOrdinalFilter : StringAbstractContainsFilter
{
    public StringContainsOrdinalFilter()
        : base(ValueFilterMode.ContainsOrdinal)
    {}
    
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}

public class StringContainsOrdinalCaseSensitiveFilter : StringAbstractContainsFilter
{
    public StringContainsOrdinalCaseSensitiveFilter()
        : base(ValueFilterMode.ContainsOrdinalCaseSensitive)
    {}
    
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.Ordinal);
    }
}

public class StringEqualsFilter : IValueFilter
{
    public ValueFilterMode Mode => ValueFilterMode.Equals;
    
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }
}

public class StringEqualsCaseSensitiveFilter : IValueFilter
{
    public ValueFilterMode Mode => ValueFilterMode.EqualsCaseSensitive;
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCulture);
    }
}

public class StringEqualsOrdinalFilter : IValueFilter
{
    public ValueFilterMode Mode => ValueFilterMode.EqualsOrdinal;
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}

public class StringEqualsOrdinalCaseSensitiveFilter : IValueFilter
{
    public ValueFilterMode Mode => ValueFilterMode.EqualsOrdinalCaseSensitive;
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.Ordinal);
    }
}

public class StringStartsWithFilter : IValueFilter
{
    public ValueFilterMode Mode => ValueFilterMode.StartsWith;
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
    public ValueFilterMode Mode => ValueFilterMode.StartsWithCaseSensitive;
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
    public ValueFilterMode Mode => ValueFilterMode.StartsWithOrdinal;
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
    public ValueFilterMode Mode => ValueFilterMode.StartsWithOrdinalCaseSensitive;
    public bool Filter(object? value, object? filterValue)
    {
        if (value is string valueStr && filterValue is string filterValueStr)
        {
            return valueStr.StartsWith(filterValueStr, StringComparison.Ordinal);
        }
        return false;
    }
}