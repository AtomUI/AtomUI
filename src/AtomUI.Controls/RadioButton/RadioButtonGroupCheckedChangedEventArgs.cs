namespace AtomUI.Controls;

/// <summary>
/// Carries the previous and the new checked item of a radio button group.
/// </summary>
public class RadioButtonGroupCheckedChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the checked item before the change, or <see langword="null" /> when no radio button was checked.
    /// </summary>
    public object? OldCheckedItem { get; }
    /// <summary>
    /// Gets the checked item after the change, or <see langword="null" /> when no radio button is checked.
    /// </summary>
    public object? NewCheckedItem { get; }

    /// <summary>
    /// Creates the arguments for a change of the checked item of a radio button group.
    /// </summary>
    /// <param name="oldCheckedItem">
    /// The checked item before the change.
    /// </param>
    /// <param name="newCheckedItem">
    /// The checked item after the change.
    /// </param>
    public RadioButtonGroupCheckedChangedEventArgs(object? oldCheckedItem, object? newCheckedItem)
    {
        OldCheckedItem = oldCheckedItem;
        NewCheckedItem = newCheckedItem;
    }
}