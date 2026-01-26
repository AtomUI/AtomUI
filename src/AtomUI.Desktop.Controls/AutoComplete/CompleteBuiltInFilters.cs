namespace AtomUI.Desktop.Controls;

public abstract class CompleteAbstractContainsFilter : ICompleteOptionFilter
{
    public abstract bool Filter(object? value, object? filterValue);

    protected static bool Contains(string? value, string? filterValue, StringComparison comparison)
    {
        if (value is not null && filterValue is not null)
        {
            return value.IndexOf(value, comparison) >= 0;
        }
        return false;
    }
}

public class CompleteContainsFilter : CompleteAbstractContainsFilter
{
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }
}

public class CompleteContainsCaseSensitiveFilter : CompleteAbstractContainsFilter
{
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCulture);
    }
}

public class CompleteContainsOrdinalFilter : CompleteAbstractContainsFilter
{
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}

public class CompleteContainsOrdinalCaseSensitiveFilter : CompleteAbstractContainsFilter
{
    public override bool Filter(object? value, object? filterValue)
    {
        return Contains(value?.ToString(), filterValue?.ToString(), StringComparison.Ordinal);
    }
}

public class CompleteEqualsFilter : ICompleteOptionFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }
}

public class CompleteEqualsCaseSensitiveFilter : ICompleteOptionFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.CurrentCulture);
    }
}

public class CompleteEqualsOrdinalFilter : ICompleteOptionFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}

public class CompleteEqualsOrdinalCaseSensitiveFilter : ICompleteOptionFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        return string.Equals(value?.ToString(), filterValue?.ToString(), StringComparison.Ordinal);
    }
}

public class CompleteStartsWithFilter : ICompleteOptionFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        var valueStr       = value?.ToString();
        var filterValueStr = filterValue?.ToString();
        if (valueStr is not null && filterValueStr is not null)
        {
            return valueStr.StartsWith(filterValueStr, StringComparison.CurrentCultureIgnoreCase);
        }
        return false;
    }
}

public class CompleteStartsWithCaseSensitiveFilter : ICompleteOptionFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        var valueStr       = value?.ToString();
        var filterValueStr = filterValue?.ToString();
        if (valueStr is not null && filterValueStr is not null)
        {
            return valueStr.StartsWith(filterValueStr, StringComparison.CurrentCulture);
        }
        return false;
    }
}

public class CompleteStartsWithOrdinalFilter : ICompleteOptionFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        var valueStr       = value?.ToString();
        var filterValueStr = filterValue?.ToString();
        if (valueStr is not null && filterValueStr is not null)
        {
            return valueStr.StartsWith(filterValueStr, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}

public class CompleteStartsWithOrdinalCaseSensitiveFilter : ICompleteOptionFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        var valueStr       = value?.ToString();
        var filterValueStr = filterValue?.ToString();
        if (valueStr is not null && filterValueStr is not null)
        {
            return valueStr.StartsWith(filterValueStr, StringComparison.Ordinal);
        }
        return false;
    }
}