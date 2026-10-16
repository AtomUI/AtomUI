namespace AtomUI;

/// <summary>Application-owned AtomUI runtime operations.</summary>
public interface IAtomUI
{
    /// <summary>Atomically appends explicitly enabled packages on the application's UI thread.</summary>
    void LoadPackages(Action<IAtomUIBuilder> configure);
}
