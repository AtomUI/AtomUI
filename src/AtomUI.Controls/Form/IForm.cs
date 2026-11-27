namespace AtomUI.Controls;

/// <summary>
/// Holds form items and validates, submits and resets their values as a whole.
/// <see cref="AbstractForm" /> is the base implementation.
/// </summary>
public interface IForm
{
    /// <summary>
    /// Validates every item of the form and returns before the validation finishes. The items show the outcome
    /// once it is done.
    /// Use <see cref="ValidateAsync(FormValidateOptions, CancellationToken)" /> to await the validation or to get
    /// its report.
    /// </summary>
    void Validate();
    /// <summary>
    /// Validates the selected items, all at the same time, and returns a report with the values and the result of
    /// every validated item. The task completes when the validation finished and, unless the run is silent, the
    /// outcome is shown on the items.
    /// </summary>
    /// <param name="options">
    /// Narrows the run to the given field names and decides whether it is silent: a silent run
    /// (<see cref="FormValidateOptions.IsSilent" />) computes the report without changing the items, and
    /// <see cref="FormValidateOptions.FieldNames" /> set to <see langword="null" /> covers every item while an
    /// empty list selects no item.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the run. The task then ends with <see cref="OperationCanceledException" />; items that finished
    /// before keep their new state.
    /// </param>
    /// <returns>
    /// The report: <see cref="FormValidateResult.Error" /> when a validated item has an error, a warning does not
    /// count as an error; otherwise <see cref="FormValidateResult.Success" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="options" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The run was canceled through <paramref name="cancellationToken" /> or by a newer run with the same silence
    /// setting.
    /// </exception>
    Task<FormValidateReport> ValidateAsync(FormValidateOptions options, CancellationToken cancellationToken = default);
    /// <summary>
    /// Validates every item and submits the form when the validation passes. The method returns before the
    /// validation finishes, and the submission is asynchronous as well.
    /// </summary>
    /// <remarks>
    /// <see cref="AbstractForm" /> reports the outcome through its events: <see cref="AbstractForm.Submitted" />
    /// carries the values that were validated and <see cref="AbstractForm.SubmitFailed" /> the validation report.
    /// </remarks>
    void Submit();
    /// <summary>
    /// Restores every item to its initial value and clears its validation state at once; pending validations are
    /// canceled first.
    /// </summary>
    /// <remarks>
    /// The initial value comes from <see cref="AbstractForm.InitialValues" /> for the field name of the item when
    /// there is one, otherwise it is the item's own initial value; an item that has neither is cleared.
    /// </remarks>
    void Reset();
    /// <summary>
    /// Writes every value into the items whose field name is its key. Keys without an item and items without a key
    /// are left alone.
    /// </summary>
    /// <param name="formValues">
    /// The values keyed by field name.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="formValues" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// The writes do not count as user input: they start no validation.
    /// </remarks>
    void SetFormValues(IFormValues formValues);
    /// <summary>
    /// Removes the form item from the form. Nothing happens for an item the form does not contain.
    /// </summary>
    /// <param name="formItem">
    /// The item to remove.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="formItem" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// <see cref="AbstractForm" /> removes the entry at the item's index from the item source when that is a
    /// writable list of variable size, and from its own item collection when no item source is set. A read-only or
    /// fixed-size list is left unchanged.
    /// </remarks>
    void DeleteFormItem(IFormItem formItem);
}