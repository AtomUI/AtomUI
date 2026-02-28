namespace AtomUI.Desktop.Controls;

public class FormValidateMessage
{
    public string FieldName { get; }
    public string Messages { get; }

    public FormValidateMessage(string fieldName, string messages)
    {
        FieldName = fieldName;
        Messages = messages;
    }
}