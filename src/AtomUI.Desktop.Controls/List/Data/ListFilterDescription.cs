using AtomUI.Controls.Utils;
using Avalonia.Collections;

namespace AtomUI.Desktop.Controls.Data;

public class ListFilterDescription : IListFilterDescription
{
    public ListFilterPropertySelector? FilterPropertySelector { get; set; }

    public ValueFilterMode FallbackFilterMode { get; init; } = ValueFilterMode.Contains;
    
    public List<object> FilterConditions { get; set; } = new ();
    
    public Func<object, object, bool>? Filter { get; set; }
    
    private IValueFilter? _fallbackFilter;
    
    public virtual bool FilterBy(object record)
    {
        foreach (var filterValue in FilterConditions)
        {
            object? value = record;
            if (FilterPropertySelector != null)
            {
                value = FilterPropertySelector(record);
            }
            // 为空就不比较
            if (value != null)
            {
                if (Filter != null)
                {
                    return Filter(value, filterValue);
                }
                var fallbackFilter = GetFallbackFilter();
                if (fallbackFilter != null)
                {
                    return fallbackFilter.Filter(value, filterValue);
                }
            }
        }
        return false;
    }
    
    private IValueFilter? GetFallbackFilter()
    {
        _fallbackFilter ??= ValueFilterFactory.BuildFilter(FallbackFilterMode);
        return _fallbackFilter;
    }
}

public class ListFilterDescriptionList : AvaloniaList<IListFilterDescription> {}