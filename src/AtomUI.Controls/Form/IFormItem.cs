using Avalonia.Controls;

namespace AtomUI.Controls;

/// <summary>
/// One entry of a form. Exposes the field name, the value control and the value of the entry to the owner
/// form, which reads and writes the value, resets it and validates it through this interface.
/// <see cref="AbstractFormItem" /> is the base implementation.
/// </summary>
public interface IFormItem
{
    /// <summary>
    /// Gets the name that identifies the item in the owner form: it keys the item's value in the form values,
    /// selects the item when the form validates given field names and is passed to the validators.
    /// <see langword="null" /> when the item is not bound to a field.
    /// </summary>
    string? FieldName { get; }
    /// <summary>
    /// Gets the value control of the item. The item reads and writes the value through
    /// <see cref="IFormItemAware" />; when there is no content or the content does not implement it,
    /// <see cref="GetItemValue" /> returns <see langword="null" /> and <see cref="SetItemValue" /> does nothing.
    /// </summary>
    public Control? Content { get; }
    /// <summary>
    /// Returns the value reported by <see cref="Content" />.
    /// </summary>
    /// <returns>
    /// The value of the content, or <see langword="null" /> when there is no content or when the content does not
    /// implement <see cref="IFormItemAware" />.
    /// </returns>
    object? GetItemValue();
    /// <summary>
    /// Writes a value into <see cref="Content" />. Nothing happens when there is no content or when the content
    /// does not implement <see cref="IFormItemAware" />.
    /// </summary>
    /// <param name="value">
    /// The value the content receives; the item passes it on unchanged.
    /// </param>
    /// <remarks>
    /// The write counts as a programmatic change, not as user input: it starts no validation and the validation
    /// state is left as it is.
    /// </remarks>
    void SetItemValue(object? value);
    /// <summary>
    /// Restores the initial value of the item, or clears the value when the item has none, and clears its
    /// validation state. Running validations are canceled and the write starts no validation.
    /// </summary>
    void ResetItemValue();
    /// <summary>
    /// Validates the current value of the item and updates its validation state with the outcome, whatever the
    /// validate trigger of the item is.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancels the validation. The outcome of a canceled run is discarded and the validation state stays as it is.
    /// </param>
    /// <returns>
    /// A task that completes when the validators have finished.
    /// </returns>
    Task ValidateValueAsync(CancellationToken cancellationToken);
}