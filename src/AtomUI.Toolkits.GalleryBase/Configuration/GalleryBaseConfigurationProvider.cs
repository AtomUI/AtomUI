using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Configuration;

/// <summary>
/// Holds the <see cref="GalleryBaseConfiguration" /> of each Avalonia application, so that Gallery code can
/// read the configuration of the application it runs in.
/// </summary>
public static class GalleryBaseConfigurationProvider
{
    private static readonly ConditionalWeakTable<Application, GalleryBaseConfiguration> Configurations = new();

    /// <summary>
    /// The configuration registered for the current application, or <see langword="null" /> when there is no
    /// current application or when no configuration is registered for it.
    /// </summary>
    public static GalleryBaseConfiguration? Current => Application.Current is { } application ? Get(application) : null;

    /// <summary>
    /// Returns the configuration registered for the application.
    /// </summary>
    /// <param name="application">
    /// The application whose configuration is requested.
    /// </param>
    /// <returns>
    /// The registered configuration, or <see langword="null" /> when none is registered for the application.
    /// </returns>
    public static GalleryBaseConfiguration? Get(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return Configurations.TryGetValue(application, out var configuration) ? configuration : null;
    }

    /// <summary>
    /// Returns the configuration registered for the current application.
    /// </summary>
    /// <returns>
    /// The configuration of the current application.
    /// </returns>
    /// <exception cref="GalleryConfigurationException">
    /// Thrown when no configuration is registered for the current application, for example because
    /// <see cref="ThemeManagerBuilderExtensions.UseGalleryBase" /> was called without product options.
    /// </exception>
    public static GalleryBaseConfiguration GetRequired()
    {
        return Current
               ?? throw new GalleryConfigurationException(
                   "GalleryBase configuration has not been registered. Call UseGalleryBase with product options first.");
    }

    /// <summary>
    /// Replaces the configuration of the current application with the given one. A
    /// <see langword="null" /> value removes the registration.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when there is no current application.
    /// </exception>
    internal static void SetCurrent(GalleryBaseConfiguration configuration)
    {
        var application = Application.Current ?? throw new InvalidOperationException("No current Avalonia application.");
        Configurations.Remove(application);
        if (configuration is not null)
        {
            Configurations.Add(application, configuration);
        }
    }

    /// <summary>
    /// Registers the configuration for the application. The returned disposable removes the registration again,
    /// unless a newer registration has replaced it in the meantime.
    /// </summary>
    /// <param name="application">
    /// The application the configuration belongs to.
    /// </param>
    /// <param name="configuration">
    /// The configuration to register.
    /// </param>
    /// <returns>
    /// A disposable that ends the registration.
    /// </returns>
    internal static IDisposable Register(Application application, GalleryBaseConfiguration configuration)
    {
        Configurations.Add(application, configuration);
        return Disposable.Create(() =>
        {
            if (ReferenceEquals(Get(application), configuration))
            {
                Configurations.Remove(application);
            }
        });
    }
}
