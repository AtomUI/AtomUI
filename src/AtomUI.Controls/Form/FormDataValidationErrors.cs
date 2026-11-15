using Avalonia.Controls;

namespace AtomUI.Controls;

public enum FormDataValidationErrorKind
{
    Validation,
    External
}

public sealed class FormDataValidationError : Exception
{
    public FormDataValidationError(string message, FormDataValidationErrorKind kind = FormDataValidationErrorKind.Validation)
        : base(message)
    {
        Kind = kind;
    }

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
