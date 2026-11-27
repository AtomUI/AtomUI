namespace AtomUI.Controls;

/// <summary>
/// A validator with a fixed verdict that does not look at the value. Use it to reproduce a passing or a failing
/// rule, optionally after a <see cref="Delay" />.
/// </summary>
public class FormAssertValidator : AbstractFormValidator
{
    /// <summary>
    /// The verdict of the rule, returned for every value: <see langword="true" /> passes and
    /// <see langword="false" /> fails.
    /// </summary>
    public bool AssertResult { get; set; }
    /// <summary>
    /// How long the rule waits before it returns <see cref="AssertResult" />. The default,
    /// <see cref="System.TimeSpan.Zero" />, returns at once.
    /// </summary>
    public TimeSpan Delay { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Waits for <see cref="Delay" /> when it is not zero, then returns <see cref="AssertResult" />. Canceling the
    /// token during the wait ends the rule with an operation canceled exception instead of a verdict. The field name
    /// and the value are not used.
    /// </summary>
    /// <returns>
    /// <see cref="AssertResult" />, after the wait.
    /// </returns>
    protected override async Task<bool> ValidateCoreAsync(string fieldName, object? value, CancellationToken cancellationToken)
    {
        if (Delay != TimeSpan.Zero)
        {
            await Task.Delay(Delay, cancellationToken);
        }
        return AssertResult;
    }
}