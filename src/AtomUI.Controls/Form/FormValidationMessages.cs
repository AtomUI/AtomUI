using System.Globalization;
using AtomUI.Controls.Localization;
using Avalonia;

namespace AtomUI.Controls;

internal static class FormValidationMessages
{
    internal static string Resolve(AbstractFormItem item, IFormValidator validator)
    {
        var label = item.LabelText ?? item.FieldName ?? string.Empty;
        if (validator.Message is { } explicitMessage && !string.IsNullOrWhiteSpace(explicitMessage))
        {
            return explicitMessage.Replace("{label}", label, StringComparison.Ordinal);
        }

        var localizer = Application.Current?.GetLocalizer();
        var (kind, fallback) = validator switch
        {
            FormStringNotEmptyValidator or FormNotNullValidator =>
                (FormLangResourceKind.FieldRequired, "{0} is required."),
            FormStringLengthValidator =>
                (FormLangResourceKind.InvalidLength, "{0} must be between {1} and {2} characters."),
            FormUrlValidator => (FormLangResourceKind.InvalidUrl, "Enter a valid URL for {0}."),
            _ => (FormLangResourceKind.InvalidValue, "Enter a valid value for {0}.")
        };
        object?[] values = validator is FormStringLengthValidator length
            ? [label, length.MinimumLength, length.MaximumLength]
            : [label];
        return localizer?.Format(kind, values) ?? string.Format(CultureInfo.CurrentCulture, fallback, values);
    }
}
