namespace AtomUI.Controls;

public sealed record FormValidateOptions
{
    public bool IsSilent { get; init; }
    public IReadOnlyList<string>? FieldNames { get; init; }
}

public sealed record FormFieldValidateResult(
    string FieldName,
    FormValidateResult Result,
    IReadOnlyList<string> ErrorMessages,
    IReadOnlyList<string> WarningMessages);

public sealed class FormValidateReport : EventArgs
{
    public FormValidateReport(IFormValues values, IReadOnlyList<FormFieldValidateResult> fields)
    {
        Values = values;
        Fields = fields;
        Result = fields.Any(field => field.Result == FormValidateResult.Error)
            ? FormValidateResult.Error : FormValidateResult.Success;
        Messages = fields.SelectMany(field =>
            field.ErrorMessages.Select(message => new FormValidateMessage(field.FieldName, message, FormValidateResult.Error))
                .Concat(field.WarningMessages.Select(message => new FormValidateMessage(field.FieldName, message, FormValidateResult.Warning))))
            .ToArray();
    }

    public FormValidateResult Result { get; }
    public IFormValues Values { get; }
    public IReadOnlyList<FormFieldValidateResult> Fields { get; }
    public IReadOnlyList<FormValidateMessage> Messages { get; }
}
