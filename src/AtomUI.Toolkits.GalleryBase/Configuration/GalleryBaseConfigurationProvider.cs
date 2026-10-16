using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Configuration;

public static class GalleryBaseConfigurationProvider
{
    private static readonly ConditionalWeakTable<Application, GalleryBaseConfiguration> Configurations = new();

    public static GalleryBaseConfiguration? Current => Application.Current is { } application ? Get(application) : null;

    public static GalleryBaseConfiguration? Get(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return Configurations.TryGetValue(application, out var configuration) ? configuration : null;
    }

    public static GalleryBaseConfiguration GetRequired()
    {
        return Current
               ?? throw new GalleryConfigurationException(
                   "GalleryBase configuration has not been registered. Call UseGalleryBase with product options first.");
    }

    internal static void SetCurrent(GalleryBaseConfiguration configuration)
    {
        var application = Application.Current ?? throw new InvalidOperationException("No current Avalonia application.");
        Configurations.Remove(application);
        if (configuration is not null)
        {
            Configurations.Add(application, configuration);
        }
    }

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
