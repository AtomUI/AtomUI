using Avalonia;

namespace AtomUI;

/// <summary>
/// A service that the <c>ApplicationScope</c> creates from a factory registered with
/// <c>AtomUIBuilder.AddOwnedService</c> and binds to the <c>Application</c>. <c>Attach</c> runs when the scope
/// starts, also for a service appended later by a package; <c>Detach</c> runs when the scope is cleaned up.
/// AtomUI.Controls.Shared implements it for the image loader.
/// </summary>
internal interface IAtomUIOwnedService : IDisposable
{
    void Attach(Application application);

    void Detach(Application application);
}

internal sealed record AtomUIOwnedServiceRegistration(
    string Id,
    Func<Application, IAtomUIOwnedService> Factory);
