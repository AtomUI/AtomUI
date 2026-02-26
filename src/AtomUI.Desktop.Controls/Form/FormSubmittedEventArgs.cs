namespace AtomUI.Desktop.Controls;

public class FormSubmittedEventArgs : EventArgs
{
    public IFormValue Value { get; }
    
    public FormSubmittedEventArgs(IFormValue value)
    {
        Value = value;
    }
}