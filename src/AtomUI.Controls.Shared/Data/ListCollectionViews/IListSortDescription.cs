using System.ComponentModel;

namespace AtomUI.Controls.Data;

/// <summary>
/// One sort level of a list collection view. The view applies the entries of
/// <see cref="IListCollectionView.SortDescriptions" /> in list order: the first entry orders the records, and each
/// following entry orders the records that the entries before it left equal.
/// </summary>
public interface IListSortDescription
{
    /// <summary>
    /// The order this level puts the records in, ascending or descending. <see cref="Comparer" />,
    /// <see cref="OrderBy" /> and <see cref="ThenBy" /> already apply it.
    /// </summary>
    ListSortDirection Direction { get; }
    /// <summary>
    /// Compares two records for this level in the order of <see cref="Direction" />: a negative value puts the first
    /// record first. The view also compares two records directly with it, outside <see cref="OrderBy" /> and
    /// <see cref="ThenBy" />.
    /// </summary>
    IComparer<object> Comparer { get; }
    /// <summary>
    /// Gets whether this level sorts by a property path of the record instead of by the record itself.
    /// </summary>
    bool HasPropertyPath { get; }
    /// <summary>
    /// Orders an unsorted sequence of records by this level. The view calls it for the first sort description, and
    /// <see cref="ThenBy" /> for the following ones.
    /// </summary>
    /// <param name="seq">
    /// The records to order.
    /// </param>
    /// <returns>
    /// The records in the order of this level, as a sequence that <see cref="ThenBy" /> can order further.
    /// </returns>
    IOrderedEnumerable<object> OrderBy(IEnumerable<object> seq);
    /// <summary>
    /// Adds this level to a sequence that the levels before it have already ordered: records that compare equal under
    /// those levels are ordered by this level.
    /// </summary>
    /// <param name="seq">
    /// The sequence ordered by the preceding sort levels.
    /// </param>
    /// <returns>
    /// The sequence with this level added as the next sort level.
    /// </returns>
    IOrderedEnumerable<object> ThenBy(IOrderedEnumerable<object> seq);
    /// <summary>
    /// Returns a description that sorts by the same property path or comparer with the opposite
    /// <see cref="Direction" />. This description is not changed.
    /// </summary>
    /// <returns>
    /// A description with the opposite direction, or the same instance when the implementation cannot switch.
    /// </returns>
    ListSortDescription SwitchSortDirection();
}