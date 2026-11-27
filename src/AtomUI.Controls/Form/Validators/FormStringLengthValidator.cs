using System.Diagnostics;

namespace AtomUI.Controls;

/// <summary>
/// A validator for the length of a string value, between <see cref="MinimumLength" /> and
/// <see cref="MaximumLength" />. It does not make the field required: an absent or blank value passes while
/// <see cref="MinimumLength" /> is <c>0</c>.
/// </summary>
public class FormStringLengthValidator : AbstractFormValidator
{
    /// <summary>
    /// The smallest length that passes, inclusive. The default, <c>0</c>, accepts any length and lets an absent or
    /// blank value pass; a greater value makes such a value fail.
    /// </summary>
    public int MinimumLength { get; set; } = 0;
    /// <summary>
    /// The largest length that passes, inclusive. The default, <see cref="int.MaxValue" />, sets no upper limit. The
    /// default message is formatted with this value, so set <see cref="AbstractFormValidator.Message" /> when no
    /// limit is meant.
    /// </summary>
    public int MaximumLength { get; set; } = int.MaxValue;
    
    /// <summary>
    /// Checks the length of the value, in UTF-16 code units, against <see cref="MinimumLength" /> and
    /// <see cref="MaximumLength" />. A value that is not a string, or that is null, empty or white space only, is
    /// treated as absent and passes exactly when <see cref="MinimumLength" /> is <c>0</c>. The field name and the
    /// cancellation token are not used.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the value passes, otherwise <see langword="false" />.
    /// </returns>
    protected override async Task<bool> ValidateCoreAsync(string fieldName, object? value, CancellationToken cancellationToken)
    {
        var strValue = value as string;
        var isValid = true;
        if (string.IsNullOrWhiteSpace(strValue))
        {
            isValid = MinimumLength == 0;
        }
        else
        {
            Debug.Assert(strValue != null);
            isValid = strValue.Length >= MinimumLength && strValue.Length <= MaximumLength;
        }
        return await Task.FromResult(isValid);
    }
}