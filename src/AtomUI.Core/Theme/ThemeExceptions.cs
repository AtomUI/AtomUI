namespace AtomUI.Theme;

/// <summary>
/// A theme could not be loaded: the theme catalog is invalid or empty, a package configuration failed
/// validation, or the compiled catalog does not contain the requested theme.
/// </summary>
public class ThemeLoadException : SystemException
{
    /// <summary>
    /// Creates the exception without an inner exception.
    /// </summary>
    /// <param name="message">
    /// The text that names the theme step that failed and why.
    /// </param>
    public ThemeLoadException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates the exception with the exception that caused the failure.
    /// </summary>
    /// <param name="message">
    /// The text that names the theme step that failed and why.
    /// </param>
    /// <param name="innerException">
    /// The exception that caused the failure, or <see langword="null" /> when there is none.
    /// </param>
    public ThemeLoadException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// The meta information of a language is malformed and could not be parsed.
/// </summary>
public class LanguageMetaInfoParseException : SystemException
{
    /// <summary>
    /// Creates the exception without an inner exception.
    /// </summary>
    /// <param name="message">
    /// The text that describes the meta information that could not be parsed.
    /// </param>
    public LanguageMetaInfoParseException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates the exception with the exception that caused the failure.
    /// </summary>
    /// <param name="message">
    /// The text that describes the meta information that could not be parsed.
    /// </param>
    /// <param name="innerException">
    /// The exception that caused the failure, or <see langword="null" /> when there is none.
    /// </param>
    public LanguageMetaInfoParseException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// A registration conflicts with an existing one: a theme resource, a theme definition resolver, a compiled theme
/// package, or a control package whose id, provider id or descriptor identity is already registered.
/// </summary>
public class ThemeResourceRegisterException : SystemException
{
    /// <summary>
    /// Creates the exception without an inner exception.
    /// </summary>
    /// <param name="message">
    /// The text that names the registration that conflicts.
    /// </param>
    public ThemeResourceRegisterException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates the exception with the exception that caused the failure.
    /// </summary>
    /// <param name="message">
    /// The text that names the registration that conflicts.
    /// </param>
    /// <param name="innerException">
    /// The exception that caused the failure, or <see langword="null" /> when there is none.
    /// </param>
    public ThemeResourceRegisterException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
