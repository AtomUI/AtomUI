namespace AtomUI;

/// <summary>
/// Application-owned AtomUI runtime operations.
/// </summary>
public interface IAtomUI
{
    /// <summary>
    /// Atomically appends explicitly enabled packages on the application's UI thread.
    /// Call it only after startup registration and application initialization are complete, and never while
    /// another call is in progress; otherwise it throws <c>InvalidOperationException</c>.
    /// </summary>
    void LoadPackages(Action<IAtomUIBuilder> configure);
}
