namespace AtomUI.Desktop.Controls;

public class RadioButtonGroupCheckedChangedEventArgs : EventArgs
{
    public object? OldCheckedItem { get; }
    public object? NewCheckedItem { get; }

    public RadioButtonGroupCheckedChangedEventArgs(object? oldCheckedItem, object? newCheckedItem)
    {
        OldCheckedItem = oldCheckedItem;
        NewCheckedItem = newCheckedItem;
    }
}