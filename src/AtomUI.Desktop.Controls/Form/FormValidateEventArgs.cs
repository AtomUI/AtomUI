namespace AtomUI.Desktop.Controls;

public class FormValidatedEventArgs : EventArgs
{
    public IFormValues? Values { get; }
    public FormValidateResult Result { get; }
    public IList<FormValidateMessage>? ErrorMessages { get; }
    
    public FormValidatedEventArgs(FormValidateResult result, IFormValues? values, IList<FormValidateMessage>? errorMessages)
    {
        Values        = values;
        ErrorMessages = errorMessages;
        Result        = result;
    }
}