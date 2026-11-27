using AtomUI.Controls.Utils;

namespace AtomUI.Controls.Data;

/// <summary>
/// One filter that decides whether a list collection view shows a record. The view only records its filters in
/// <see cref="IListCollectionView.FilterDescriptions" /> and leaves them to a predicate that calls
/// <see cref="FilterBy" /> for each record.
/// </summary>
public interface IListFilterDescription
{
    /// <summary>
    /// Selects the value of a record that is matched against the filter conditions. When <see langword="null" />, the
    /// record itself is matched.
    /// </summary>
    DefaultFilterValueSelector? FilterPropertySelector { get; }
    /// <summary>
    /// The values a record is matched against, in the order they are evaluated.
    /// </summary>
    List<object> FilterConditions { get; }
    /// <summary>
    /// Returns whether the record passes this filter. The value to compare comes from
    /// <see cref="FilterPropertySelector" /> and is matched with <see cref="FilterConditions" />.
    /// </summary>
    /// <param name="record">
    /// The record of the list to test.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the record passes; otherwise <see langword="false" />.
    /// </returns>
    bool FilterBy(object record);
    /// <summary>
    /// Matches the selected value of a record with one filter condition. When <see langword="null" />, the description
    /// decides itself how a value is matched with a condition.
    /// </summary>
    /// <value>
    /// A function that receives the value selected from the record and the filter condition, and returns whether they
    /// match.
    /// </value>
    Func<object, object, bool>? Filter { get; }
}