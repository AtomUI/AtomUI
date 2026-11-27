using System.Collections;
using Avalonia.Interactivity;

namespace AtomUI.Controls;

/// <summary>
/// Describes which items a check box group checked and unchecked in one change of its checked items.
/// </summary>
public class CheckBoxGroupCheckedChangedEventArgs: RoutedEventArgs
{
    /// <summary>
    /// Creates the arguments of a change of the checked items.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event to raise.
    /// </param>
    /// <param name="removedItems">
    /// The items that are no longer checked.
    /// </param>
    /// <param name="addedItems">
    /// The items that are checked now.
    /// </param>
    public CheckBoxGroupCheckedChangedEventArgs(RoutedEvent routedEvent, IList removedItems, IList addedItems)
        : base(routedEvent)
    {
        RemovedItems = removedItems;
        AddedItems   = addedItems;
    }
    
    /// <summary>
    /// The items that became checked in this change.
    /// </summary>
    public IList AddedItems { get; }
    
    /// <summary>
    /// The items that became unchecked in this change.
    /// </summary>
    public IList RemovedItems { get; }
}