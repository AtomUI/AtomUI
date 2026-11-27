namespace AtomUI.Controls;

/// <summary>
/// Carries the item that reported a new value and that value for the
/// <see cref="AbstractForm.ItemValueChanged" /> event of a form.
/// </summary>
public class FormItemValueChangedEventArgs : EventArgs
{
    /// <summary>
    /// The new value of <see cref="Item" />, read when the event is raised. It may be <see langword="null" />.
    /// </summary>
    public object? Value { get; }
    /// <summary>
    /// The item whose content reported the new value.
    /// </summary>
    public IFormItem? Item { get; }
    
    /// <summary>
    /// Creates event args that report <paramref name="value" /> as the new value of <paramref name="item" />.
    /// </summary>
    public FormItemValueChangedEventArgs(IFormItem item, object? value)
    {
        Item  = item;
        Value = value;
    }
}