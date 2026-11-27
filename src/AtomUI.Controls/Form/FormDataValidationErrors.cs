using Avalonia.Controls;

namespace AtomUI.Controls;

/// <summary>
/// Names the two groups of errors that the form writes to <see cref="DataValidationErrors" /> of a control.
/// The two groups are written and read back separately.
/// </summary>
public enum FormDataValidationErrorKind
{
    /// <summary>
    /// A message produced by validating the item.
    /// </summary>
    Validation,
    /// <summary>
    /// A message whose source is outside the validation of the item. The form lists these before the
    /// <see cref="Validation" /> errors.
    /// </summary>
    External
}

/// <summary>
/// An error message that the form stores in <see cref="DataValidationErrors" /> of a control,
/// tagged with the group it belongs to.
/// </summary>
public sealed class FormDataValidationError : Exception
{
    /// <summary>
    /// Creates an error for one message of a form error group.
    /// </summary>
    /// <param name="message">
    /// The error text. It becomes the <see cref="Exception.Message" /> of the error.
    /// </param>
    /// <param name="kind">
    /// The group the message belongs to.
    /// </param>
    public FormDataValidationError(string message, FormDataValidationErrorKind kind = FormDataValidationErrorKind.Validation)
        : base(message)
    {
        Kind = kind;
    }

    /// <summary>
    /// Gets the group the message belongs to. The form replaces the message together with the other
    /// messages of that group.
    /// </summary>
    public FormDataValidationErrorKind Kind { get; }
}

internal static class FormDataValidationErrors
{
    internal static void Write(Control control, IReadOnlyList<string>? external, IReadOnlyList<string>? validation)
    {
        var current = DataValidationErrors.GetErrors(control);
        if (external is not { Count: > 0 } && validation is not { Count: > 0 } &&
            current?.Any(error => error is FormDataValidationError) != true)
        {
            return;
        }

        if (HasSameMessages(current, external, FormDataValidationErrorKind.External) &&
            HasSameMessages(current, validation, FormDataValidationErrorKind.Validation))
        {
            return;
        }

        var errors = current?.Where(error => error is not FormDataValidationError).ToList() ?? [];
        if (external is not null)
        {
            errors.AddRange(external.Select(message => new FormDataValidationError(message, FormDataValidationErrorKind.External)));
        }
        if (validation is not null)
        {
            errors.AddRange(validation.Select(message => new FormDataValidationError(message)));
        }
        DataValidationErrors.SetErrors(control, errors.Count > 0 ? errors : null);
    }

    internal static void ClearFormErrors(Control control) => Write(control, null, null);

    internal static IList<string> GetErrorMessages(Control control) =>
        ResolveMessages(DataValidationErrors.GetErrors(control)?.OrderBy(error => error switch
        {
            FormDataValidationError { Kind: FormDataValidationErrorKind.External } => 1,
            FormDataValidationError => 2,
            _ => 0
        }));

    internal static IList<string> GetOtherErrorMessages(Control? control) =>
        ResolveMessages(control is null ? null : DataValidationErrors.GetErrors(control)?.Where(error =>
            error is not FormDataValidationError { Kind: FormDataValidationErrorKind.Validation }));

    private static bool HasSameMessages(IEnumerable<object>? errors, IReadOnlyList<string>? messages, FormDataValidationErrorKind kind)
    {
        var existing = errors?.OfType<FormDataValidationError>().Where(error => error.Kind == kind).Select(error => error.Message);
        return (existing ?? []).SequenceEqual(messages ?? []);
    }

    private static IList<string> ResolveMessages(IEnumerable<object>? errors) => errors?
        .Select(error => error is Exception exception ? exception.Message : error?.ToString())
        .Where(message => !string.IsNullOrWhiteSpace(message)).Select(message => message!).ToArray() ?? [];
}
