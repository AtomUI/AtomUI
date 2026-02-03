using AtomUI.Controls.Utils;

namespace AtomUI.Desktop.Controls;

public class DefaultCascaderItemFilter : ICascaderItemFilter
{
    private IValueFilter _valueFilter;

    public DefaultCascaderItemFilter(ValueFilterMode filterMode = ValueFilterMode.Contains)
    {
        if (filterMode == ValueFilterMode.None ||
            filterMode == ValueFilterMode.Contains)
        {
            filterMode = ValueFilterMode.Contains;
        }
        _valueFilter = ValueFilterFactory.BuildFilter(filterMode)!;
    }
    
    public bool Filter(CascaderView cascaderView, ICascaderItemInfo cascaderItemInfo, object? filterValue)
    {
        return _valueFilter.Filter(cascaderItemInfo.Path, filterValue) || filterValue == null;
    }
}