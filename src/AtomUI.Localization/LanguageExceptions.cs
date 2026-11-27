namespace AtomUI.Localization;

/// <summary>
/// The exception thrown when the supported languages are set up inconsistently, for example when no language is
/// supported, two language definitions share a language tag, or the default or initial language is not among the
/// supported languages.
/// </summary>
public class LanguageConfigurationException : InvalidOperationException
{
    /// <summary>
    /// Initializes the exception with an error message and no inner exception.
    /// </summary>
    public LanguageConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes the exception with an error message and the exception that caused it.
    /// </summary>
    public LanguageConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// The exception thrown when a language catalog is registered more than once, when one of its units has no
/// valid translation, or when a resource cannot be resolved or formatted.
/// </summary>
public class LanguageCatalogException : InvalidOperationException
{
    /// <summary>
    /// Initializes the exception with an error message and no inner exception.
    /// </summary>
    public LanguageCatalogException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes the exception with an error message and the exception that caused it.
    /// </summary>
    public LanguageCatalogException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// The exception thrown when building a snapshot finds that a supported language resolves a catalog unit only
/// through the final en-US fallback, while translations are required for that language.
/// </summary>
public class LanguageCoverageException : InvalidOperationException
{
    /// <summary>
    /// Initializes the exception with an error message and no inner exception.
    /// </summary>
    public LanguageCoverageException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes the exception with an error message and the exception that caused it.
    /// </summary>
    public LanguageCoverageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// The exception thrown when a language that the application does not support is requested.
/// </summary>
public class LanguageNotSupportedException : ArgumentException
{
    /// <summary>
    /// Initializes the exception for a language that the application does not support.
    /// </summary>
    /// <param name="language">
    /// The requested language, which becomes <see cref="Language" />.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="language" /> is <see langword="default" />.
    /// </exception>
    public LanguageNotSupportedException(LanguageTag language)
        : base(CreateMessage(language), nameof(language))
    {
        Language = language;
    }

    /// <summary>
    /// The language that was requested and is not supported.
    /// </summary>
    public LanguageTag Language { get; }

    private static string CreateMessage(LanguageTag language)
    {
        if (language == default)
        {
            throw new ArgumentException("A valid language tag is required.", nameof(language));
        }

        return $"Language '{language.Value}' is not supported by this application.";
    }
}
