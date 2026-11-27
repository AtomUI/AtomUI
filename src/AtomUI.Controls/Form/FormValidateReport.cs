namespace AtomUI.Controls;

/// <summary>
/// Narrows a form validation to a set of fields and decides whether the run is silent. Pass it to
/// <see cref="AbstractForm.ValidateAsync(FormValidateOptions, System.Threading.CancellationToken)" />.
/// </summary>
public sealed record FormValidateOptions
{
    /// <summary>
    /// When <see langword="true" />, the run only computes the report: it leaves the item states and
    /// <see cref="AbstractForm.IsFormValid" /> unchanged and raises neither
    /// <see cref="AbstractForm.AboutToValidate" /> nor <see cref="AbstractForm.Validated" />. A silent run cancels
    /// only an earlier silent run; it neither cancels nor is canceled by a run that is not silent.
    /// </summary>
    public bool IsSilent { get; init; }
    /// <summary>
    /// The field names of the items to validate, compared ordinally. <see langword="null" /> covers every item that
    /// can take part, an empty list selects none, and an empty string selects the items without a field name.
    /// </summary>
    public IReadOnlyList<string>? FieldNames { get; init; }
}

/// <summary>
/// The validation outcome of one form item in one run.
/// </summary>
/// <param name="FieldName">
/// The field name of the item, or an empty string when the item has none.
/// </param>
/// <param name="Result">
/// <see cref="FormValidateResult.Error" /> when <paramref name="ErrorMessages" /> is not empty; otherwise
/// <see cref="FormValidateResult.Warning" /> or <see cref="FormValidateResult.Success" />, following the
/// status of the item's validation.
/// </param>
/// <param name="ErrorMessages">
/// The error messages reported for the item.
/// </param>
/// <param name="WarningMessages">
/// The warning messages reported for the item.
/// </param>
public sealed record FormFieldValidateResult(
    string FieldName,
    FormValidateResult Result,
    IReadOnlyList<string> ErrorMessages,
    IReadOnlyList<string> WarningMessages);

/// <summary>
/// The outcome of one form validation run. It is returned by
/// <see cref="AbstractForm.ValidateAsync(FormValidateOptions, System.Threading.CancellationToken)" /> and carried
/// by <see cref="AbstractForm.SubmitFailed" /> when a submission fails validation.
/// </summary>
public sealed class FormValidateReport : EventArgs
{
    /// <summary>
    /// Derives the overall result and the message list from the field results: <see cref="Result" /> is
    /// <see cref="FormValidateResult.Error" /> when any field has an error and
    /// <see cref="FormValidateResult.Success" /> otherwise.
    /// </summary>
    /// <param name="values">
    /// The form values that were validated.
    /// </param>
    /// <param name="fields">
    /// The result of each validated item.
    /// </param>
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

    /// <summary>
    /// <see cref="FormValidateResult.Error" /> when any entry of <see cref="Fields" /> has an error, otherwise
    /// <see cref="FormValidateResult.Success" />. Warnings do not change it; they are listed in
    /// <see cref="Messages" />.
    /// </summary>
    public FormValidateResult Result { get; }
    /// <summary>
    /// A snapshot of the item values taken when the report was created. It holds an entry for every item with a
    /// field name, whether or not the run validated that item.
    /// </summary>
    public IFormValues Values { get; }
    /// <summary>
    /// One result for each item the run validated, in the order in which the form lists its items.
    /// </summary>
    public IReadOnlyList<FormFieldValidateResult> Fields { get; }
    /// <summary>
    /// Every error and warning message of <see cref="Fields" />, each as a
    /// <see cref="FormValidateMessage" />: for each field in order, its error messages followed by its warning
    /// messages.
    /// </summary>
    public IReadOnlyList<FormValidateMessage> Messages { get; }
}
