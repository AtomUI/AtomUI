namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// Converts the text of a token value into a value of one target type. Theme definitions use it to turn the
/// literal of a token value into its typed value. The built-in converters parse with the invariant culture and
/// throw an <c>InvalidOperationException</c> when the text is not a valid literal.
/// </summary>
public interface ITokenValueConverter
{
    /// <summary>
    /// Returns the type that <see cref="Convert(string)" /> produces. A converter is registered under this type.
    /// </summary>
    public Type TargetType();
    /// <summary>
    /// Converts the text of a token value into a value of the type returned by <see cref="TargetType" />.
    /// </summary>
    /// <param name="value">
    /// The text of the token value, for example <c>12</c> or <c>1.5</c>.
    /// </param>
    public object Convert(string value);
}