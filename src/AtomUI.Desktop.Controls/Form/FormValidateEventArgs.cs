namespace AtomUI.Desktop.Controls;

public class FormAboutToValidateEventArgs : EventArgs
{
    public IFormValue Value { get; }
    
    public FormAboutToValidateEventArgs(IFormValue value)
    {
        Value = value;
    }
}

public class FormValidatedEventArgs : EventArgs
{
    public IFormValue Value { get; }
    public bool IsValid => ErrorMessages == null || ErrorMessages.Count == 0;
    public IList<FormValidateMessage>? ErrorMessages { get; }
    
    public FormValidatedEventArgs(IFormValue value, IList<FormValidateMessage>? errorMessages)
    {
        Value         = value;
        ErrorMessages = errorMessages;
    }
}