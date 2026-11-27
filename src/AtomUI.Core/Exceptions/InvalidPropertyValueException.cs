namespace AtomUI.Exceptions;

/// <summary>
/// The exception that is thrown when a property receives a value it does not support.
/// </summary>
public class InvalidPropertyValueException : ArgumentException
{
    /// <summary>
    /// The name of the property that rejected <see cref="InvalidValue" />.
    /// </summary>
    public string PropertyName { get; }
    /// <summary>
    /// The value that the property named <see cref="PropertyName" /> rejected.
    /// </summary>
    public object InvalidValue { get; }
    
    /// <summary>
    /// Creates the exception with a message that names the value and the property.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that rejected the value.
    /// </param>
    /// <param name="invalidValue">
    /// The rejected value.
    /// </param>
    public InvalidPropertyValueException(string propertyName, object invalidValue)
        : base($"The value '{invalidValue}' is not valid for property '{propertyName}'.")
    {
        PropertyName = propertyName;
        InvalidValue = invalidValue;
    }
    
    /// <summary>
    /// Creates the exception with the error text <paramref name="message" /> instead of a generated message.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that rejected the value.
    /// </param>
    /// <param name="invalidValue">
    /// The rejected value.
    /// </param>
    /// <param name="message">
    /// The text that describes the error.
    /// </param>
    public InvalidPropertyValueException(string propertyName, object invalidValue, string message)
        : base(message)
    {
        PropertyName = propertyName;
        InvalidValue = invalidValue;
    }
    
    /// <summary>
    /// Creates the exception with the error text <paramref name="message" /> and the exception that caused it.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that rejected the value.
    /// </param>
    /// <param name="invalidValue">
    /// The rejected value.
    /// </param>
    /// <param name="message">
    /// The text that describes the error.
    /// </param>
    /// <param name="innerException">
    /// The exception that caused this one.
    /// </param>
    public InvalidPropertyValueException(string propertyName, object invalidValue, string message, Exception innerException)
        : base(message, innerException)
    {
        PropertyName = propertyName;
        InvalidValue = invalidValue;
    }
}