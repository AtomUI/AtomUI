using System.Globalization;
using AtomUI.Controls.Utils;

namespace AtomUI.Controls.Data;

/// <summary>
/// A group description that groups the records of a list collection view by the value a selector returns for each
/// record. Records with matching values go into the same group.
/// </summary>
public class ListGroupDescription : AbstractListGroupDescription
{
    private DefaultFilterValueSelector _propertySelector;

    /// <summary>
    /// Creates a description that groups records by the value the selector returns for them.
    /// </summary>
    /// <param name="selector">
    /// Returns the group key of a record.
    /// </param>
    public ListGroupDescription(DefaultFilterValueSelector selector)
    {
        _propertySelector = selector;
    }

    /// <summary>
    /// Returns the value that the selector returns for the record. The level and the culture are not used.
    /// </summary>
    /// <param name="item">
    /// The record to group.
    /// </param>
    /// <param name="level">
    /// The zero-based nesting level of the group that is divided; not used.
    /// </param>
    /// <param name="culture">
    /// The culture of the view; not used.
    /// </param>
    /// <returns>
    /// The group key of the record. An <see cref="System.Collections.ICollection" /> puts the record in one group per
    /// element; <see langword="null" /> puts it in none.
    /// </returns>
    public override object? GroupKeyFromItem(object item, int level, CultureInfo culture)
    {
        return _propertySelector(item);
    }

    /// <summary>
    /// Returns whether a group key and a record key name the same group. Two strings match when they are equal by
    /// ordinal comparison, so string keys that differ only in case do not match. Every other pair of keys is compared
    /// by value, as the base member does.
    /// </summary>
    /// <param name="groupKey">
    /// The key the group was created with.
    /// </param>
    /// <param name="itemKey">
    /// The key that <see cref="GroupKeyFromItem" /> returned for the record.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the record belongs to the group; otherwise <see langword="false" />.
    /// </returns>
    public override bool KeysMatch(object groupKey, object itemKey)
    {
        if (groupKey is string k1 && itemKey is string k2)
        {
            return string.Equals(k1, k2, StringComparison.Ordinal);
        }
        return base.KeysMatch(groupKey, itemKey);
    }
    
}