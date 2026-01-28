namespace AtomUI.Controls.Utils;

public abstract class StringAbstractContainsFilter : IStringValueFilter
{
    public abstract bool Filter(string? value, string? filterValue);

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
    public override bool Filter(string? value, string? filterValue)
    {
        return Contains(value, filterValue, StringComparison.CurrentCultureIgnoreCase);
    }
}

public class StringContainsCaseSensitiveFilter : StringAbstractContainsFilter
{
    public override bool Filter(string? value, string? filterValue)
    {
        return Contains(value, filterValue, StringComparison.CurrentCulture);
    }
}

public class StringContainsOrdinalFilter : StringAbstractContainsFilter
{
    public override bool Filter(string? value, string? filterValue)
    {
        return Contains(value, filterValue, StringComparison.OrdinalIgnoreCase);
    }
}

public class StringContainsOrdinalCaseSensitiveFilter : StringAbstractContainsFilter
{
    public override bool Filter(string? value, string? filterValue)
    {
        return Contains(value, filterValue, StringComparison.Ordinal);
    }
}

public class StringEqualsFilter : IStringValueFilter
{
    public bool Filter(string? value, string? filterValue)
    {
        return string.Equals(value, filterValue, StringComparison.CurrentCultureIgnoreCase);
    }
}

public class StringEqualsCaseSensitiveFilter : IStringValueFilter
{
    public bool Filter(string? value, string? filterValue)
    {
        return string.Equals(value, filterValue, StringComparison.CurrentCulture);
    }
}

public class StringEqualsOrdinalFilter : IStringValueFilter
{
    public bool Filter(string? value, string? filterValue)
    {
        return string.Equals(value, filterValue, StringComparison.OrdinalIgnoreCase);
    }
}

public class StringEqualsOrdinalCaseSensitiveFilter : IStringValueFilter
{
    public bool Filter(string? value, string? filterValue)
    {
        return string.Equals(value, filterValue, StringComparison.Ordinal);
    }
}

public class StringStartsWithFilter : IStringValueFilter
{
    public bool Filter(string? value, string? filterValue)
    {
        if (value is not null && filterValue is not null)
        {
            return value.StartsWith(filterValue, StringComparison.CurrentCultureIgnoreCase);
        }
        return false;
    }
}

public class StringStartsWithCaseSensitiveFilter : IStringValueFilter
{
    public bool Filter(string? value, string? filterValue)
    {
        if (value is not null && filterValue is not null)
        {
            return value.StartsWith(filterValue, StringComparison.CurrentCulture);
        }
        return false;
    }
}

public class StringStartsWithOrdinalFilter : IStringValueFilter
{
    public bool Filter(string? value, string? filterValue)
    {
        if (value is not null && filterValue is not null)
        {
            return value.StartsWith(filterValue, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}

public class StringStartsWithOrdinalCaseSensitiveFilter : IStringValueFilter
{
    public bool Filter(string? value, string? filterValue)
    {
        if (value is not null && filterValue is not null)
        {
            return value.StartsWith(filterValue, StringComparison.Ordinal);
        }
        return false;
    }
}