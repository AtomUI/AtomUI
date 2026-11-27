using System.Collections;
using Avalonia.Interactivity;

namespace AtomUI.Controls;

/// <summary>
/// Carries the data of the <c>CheckedChanged</c> event of a checkable tag group: the value checked before and the
/// value checked after the change when the group allows one checked tag, or the added and the removed values when
/// it allows several.
/// </summary>
public class CheckableTagGroupCheckedChangedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Creates the arguments of a change of the checked tags.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event that carries the arguments.
    /// </param>
    /// <param name="isMultiple">
    /// The value for <see cref="IsMultiple" />.
    /// </param>
    /// <param name="oldCheckedItem">
    /// The value for <see cref="OldCheckedItem" />.
    /// </param>
    /// <param name="newCheckedItem">
    /// The value for <see cref="NewCheckedItem" />.
    /// </param>
    /// <param name="addedItems">
    /// The value for <see cref="AddedItems" />.
    /// </param>
    /// <param name="removedItems">
    /// The value for <see cref="RemovedItems" />.
    /// </param>
    public CheckableTagGroupCheckedChangedEventArgs(
        RoutedEvent routedEvent,
        bool isMultiple,
        object? oldCheckedItem,
        object? newCheckedItem,
        IList addedItems,
        IList removedItems)
        : base(routedEvent)
    {
        IsMultiple    = isMultiple;
        OldCheckedItem = oldCheckedItem;
        NewCheckedItem = newCheckedItem;
        AddedItems    = addedItems;
        RemovedItems  = removedItems;
    }

    /// <summary>
    /// Whether the group is in multiple mode after the change. Then <see cref="AddedItems" /> and
    /// <see cref="RemovedItems" /> describe the change; otherwise <see cref="OldCheckedItem" /> and
    /// <see cref="NewCheckedItem" /> do.
    /// </summary>
    public bool IsMultiple { get; }

    /// <summary>
    /// The value that was checked before the change, or <see langword="null" /> when nothing was. Always
    /// <see langword="null" /> when <see cref="IsMultiple" /> is <see langword="true" />.
    /// </summary>
    public object? OldCheckedItem { get; }

    /// <summary>
    /// The value that is checked after the change, or <see langword="null" /> when nothing is. Always
    /// <see langword="null" /> when <see cref="IsMultiple" /> is <see langword="true" />.
    /// </summary>
    public object? NewCheckedItem { get; }

    /// <summary>
    /// The values that became checked with the change. When <see cref="IsMultiple" /> is <see langword="false" />,
    /// the list contains <see cref="NewCheckedItem" /> when that is not <see langword="null" />, and is empty
    /// otherwise.
    /// </summary>
    public IList AddedItems { get; }

    /// <summary>
    /// The values that became unchecked with the change. When <see cref="IsMultiple" /> is
    /// <see langword="false" />, the list contains <see cref="OldCheckedItem" /> when that is not
    /// <see langword="null" />, and is empty otherwise.
    /// </summary>
    public IList RemovedItems { get; }
}
