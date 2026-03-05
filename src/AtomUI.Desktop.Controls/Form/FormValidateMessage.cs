namespace AtomUI.Desktop.Controls;

public class FormValidateMessage
{
    public string FieldName { get; }
    public string Message { get; }
    public FormValidateResult Type { get; }

    public FormValidateMessage(string fieldName, string message, FormValidateResult type)
    {
        FieldName = fieldName;
        Message   = message;
        Type      = type;
    }
}