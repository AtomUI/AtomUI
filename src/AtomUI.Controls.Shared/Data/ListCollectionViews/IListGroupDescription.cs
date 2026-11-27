using System.Globalization;
using Avalonia.Collections;

namespace AtomUI.Controls.Data;

/// <summary>
/// Defines how a list collection view divides records into groups at one nesting level. The view takes one
/// description per level from <see cref="IListCollectionView.GroupDescriptions" />.
/// </summary>
public interface IListGroupDescription
{
    /// <summary>
    /// The keys of the groups that exist before any record is grouped. The view creates one group per key, in this
    /// order, ahead of the groups it derives from records, and keeps these groups when they are empty.
    /// </summary>
    AvaloniaList<object> GroupKeys { get; }
    /// <summary>
    /// Returns the key of the group that the record belongs to at the given nesting level.
    /// </summary>
    /// <param name="item">
    /// The record to group.
    /// </param>
    /// <param name="level">
    /// The zero-based nesting level of the group that is divided; 0 for the top-level groups.
    /// </param>
    /// <param name="culture">
    /// The culture of the view.
    /// </param>
    /// <returns>
    /// The group key. An <see cref="System.Collections.ICollection" /> puts the record in one group per element;
    /// <see langword="null" /> puts it in none.
    /// </returns>
    object? GroupKeyFromItem(object item, int level, CultureInfo culture);
    /// <summary>
    /// Returns whether the record key belongs to the group that was created with the given group key. The view passes
    /// the key of each existing group at that level in order, and puts the record into the first one that matches.
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
    bool KeysMatch(object groupKey, object itemKey);
}

/// <summary>
/// The group descriptions of a list collection view, one per nesting level: the first entry divides the records
/// into top-level groups, the next entry divides those groups, and so on.
/// </summary>
public class GroupDescriptionList : AvaloniaList<IListGroupDescription> {}