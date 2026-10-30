using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;

namespace AtomUI.Theme;

internal sealed class ThemeContext
{
    private ThemeSnapshot _snapshot;

    internal ThemeContext(
        ThemeManager manager,
        ThemeSnapshot snapshot,
        long registrationId)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(snapshot);

        Manager         = manager;
        RegistrationId  = registrationId;
        _snapshot       = snapshot;
        ResourceProvider = new ThemeTokenResourceProvider(this);
    }

    internal ThemeManager Manager { get; }
    internal long RegistrationId { get; }
    internal ThemeSnapshot Snapshot => Manager.PackageReadUpdate?.GetContextSnapshot(this) ?? Volatile.Read(ref _snapshot);
    internal ThemeAppearance Appearance => Snapshot.Appearance;
    internal ThemeTokenResourceProvider ResourceProvider { get; }

    internal event EventHandler? Published;

    internal void Commit(ThemeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Volatile.Write(ref _snapshot, snapshot);
    }

    internal IReadOnlyList<ThemeDiagnostic> Publish(bool notifyResources = true)
    {
        var diagnostics = new List<ThemeDiagnostic>();
        if (notifyResources)
        {
            ThemePublishBoundary.Dispatch(
                ResourceProvider.PublishSnapshotChanged,
                this,
                diagnostics,
                $"ThemeContext[{RegistrationId}].Resources");
        }
        ThemeEventDispatcher.Dispatch(
            Published,
            this,
            EventArgs.Empty,
            diagnostics,
            $"ThemeContext[{RegistrationId}]");
        return diagnostics.AsReadOnly();
    }
}
