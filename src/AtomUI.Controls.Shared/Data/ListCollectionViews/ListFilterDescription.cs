using AtomUI.Controls.Utils;
using Avalonia.Collections;

namespace AtomUI.Controls.Data;

/// <summary>
/// A filter that keeps a list record when its value matches a filter condition. <see cref="Filter" /> makes the
/// comparison when it is set; otherwise a value filter built from <see cref="FallbackFilterMode" /> makes it.
/// </summary>
public class ListFilterDescription : IListFilterDescription
{
    /// <inheritdoc />
    public DefaultFilterValueSelector? FilterPropertySelector { get; set; }

    /// <summary>
    /// The matching rule that compares a record value with a filter condition when <see cref="Filter" /> is
    /// <see langword="null" />. The value filter is built from the mode on first use and cached, so the mode can only
    /// be set when the description is created. With <see cref="ValueFilterMode.None" /> no filter is built and no
    /// record passes.
    /// </summary>
    public ValueFilterMode FallbackFilterMode { get; init; } = ValueFilterMode.Contains;
    
    /// <inheritdoc />
    public List<object> FilterConditions { get; set; } = new ();
    
    /// <inheritdoc />
    public Func<object, object, bool>? Filter { get; set; }
    
    private IValueFilter? _fallbackFilter;
    
    /// <summary>
    /// Returns whether the record passes the filter. The value to compare is what <see cref="FilterPropertySelector" />
    /// returns for the record, or the record itself when the selector is <see langword="null" />. A
    /// <see langword="null" /> value is not compared, so it does not pass. When <see cref="FilterConditions" /> is
    /// empty, no record passes. Otherwise the value is compared with the first entry of
    /// <see cref="FilterConditions" />, by <see cref="Filter" /> or, when that is <see langword="null" />, by the
    /// filter built from <see cref="FallbackFilterMode" />, and the result is returned; the other entries are not
    /// used. The predicate that reads the filter descriptions of the view calls this for each record.
    /// </summary>
    /// <param name="record">
    /// The record of the list to test.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the value matches the first entry; otherwise <see langword="false" />.
    /// </returns>
    public virtual bool FilterBy(object record)
    {
        foreach (var filterValue in FilterConditions)
        {
            object? value = record;
            if (FilterPropertySelector != null)
            {
                value = FilterPropertySelector(record);
            }
            // A null value is not compared with any condition.
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

/// <summary>
/// The filter descriptions of a list collection view. The view does not evaluate them; the predicate that
/// <c>ListView</c> installs by default accepts a record only when every description in the list accepts it.
/// </summary>
public class FilterDescriptionList : AvaloniaList<IListFilterDescription> {}