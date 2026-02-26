namespace AtomUI.Desktop.Controls;

public class FormValidateMessage
{
    public string FieldName { get; }
    public List<string> Messages { get; }

    public FormValidateMessage(string fieldName, List<string> messages)
    {
        FieldName = fieldName;
        Messages = messages;
    }
}