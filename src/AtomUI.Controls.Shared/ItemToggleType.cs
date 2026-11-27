namespace AtomUI.Controls;

/// <summary>
/// The toggle control shown in front of a selectable item, for example an item of a <c>TreeView</c>: none, a check
/// box or a radio button.
/// </summary>
public enum ItemToggleType
{
    /// <summary>
    /// No toggle control is shown.
    /// </summary>
    None,
    
    /// <summary>
    /// A check box in front of the item that toggles it between checked and unchecked.
    /// </summary>
    CheckBox,
    
    /// <summary>
    /// A radio button in front of the item. Checking an item clears the item that was checked before, so that only
    /// one item of a group is checked.
    /// </summary>
    Radio
}