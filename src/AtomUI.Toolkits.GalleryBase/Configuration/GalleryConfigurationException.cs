namespace AtomUI.Toolkits.GalleryBase.Configuration;

/// <summary>
/// Thrown when the Gallery configuration, a route key or a navigation node is invalid, or when code needs a
/// Gallery configuration that was not registered.
/// </summary>
public class GalleryConfigurationException : InvalidOperationException
{
    /// <summary>
    /// Creates the exception with a message that states what is invalid or missing.
    /// </summary>
    /// <param name="message">
    /// The text that describes what is invalid or missing.
    /// </param>
    public GalleryConfigurationException(string message)
        : base(message)
    {
    }
}
