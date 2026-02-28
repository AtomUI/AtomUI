namespace AtomUI.Desktop.Controls;

public class FormSubmittedEventArgs : EventArgs
{
    public IFormValues Values { get; }
    
    public FormSubmittedEventArgs(IFormValues values)
    {
        Values = values;
    }
}