namespace AtomUI.Desktop.Controls;

public class FormItemValueChangedEventArgs : EventArgs
{
    public object? Value { get; }
    public IFormItem? Item { get; }
    
    public FormItemValueChangedEventArgs(IFormItem item, object? value)
    {
        Item  = item;
        Value = value;
    }
}