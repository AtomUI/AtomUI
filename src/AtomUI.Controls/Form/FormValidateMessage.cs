namespace AtomUI.Controls;

/// <summary>
/// One error or warning message that a form validation produced for one field.
/// </summary>
public class FormValidateMessage
{
    /// <summary>
    /// The name of the field the message belongs to.
    /// </summary>
    public string FieldName { get; }
    /// <summary>
    /// The text of the message.
    /// </summary>
    public string Message { get; }
    /// <summary>
    /// Whether the message reports an error or a warning. A form validation creates messages with
    /// <see cref="FormValidateResult.Error" /> or <see cref="FormValidateResult.Warning" /> only.
    /// </summary>
    public FormValidateResult Type { get; }

    /// <summary>
    /// Creates a message for the field <paramref name="fieldName" />.
    /// </summary>
    /// <param name="fieldName">
    /// The name of the field the message belongs to.
    /// </param>
    /// <param name="message">
    /// The text of the message.
    /// </param>
    /// <param name="type">
    /// Whether the message reports an error or a warning.
    /// </param>
    public FormValidateMessage(string fieldName, string message, FormValidateResult type)
    {
        FieldName = fieldName;
        Message   = message;
        Type      = type;
    }
}