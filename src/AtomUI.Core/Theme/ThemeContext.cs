using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme;

internal sealed class ThemeContext
{
    [ThreadStatic]
    private static int t_publishDepth;

    private ThemeSnapshot _snapshot;
    private ThemePublishDelta _lastPublish = ThemePublishDelta.Everything;
    private long _publishVersion;
    private ThemeSnapshot? _ownerSlotSnapshot;
    private Dictionary<ControlTokenIdentity, int>? _ownerSlots;

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

    /// <summary>
    /// True while a context publishes on the current thread, i.e. while the resource/Published notifications of a theme
    /// publish run.
    /// </summary>
    internal static bool IsPublishing => t_publishDepth > 0;

    /// <summary>
    /// Incremented by every <see cref="Publish" />. No binding reads it: a binding recognises the publish a
    /// notification belongs to by snapshot identity, see <see cref="ThemePublishTracker" />.
    /// </summary>
    internal long PublishVersion => Volatile.Read(ref _publishVersion);

    /// <summary>
    /// What the last <see cref="Commit" /> changed relative to the snapshot before it. <see
    /// cref="ThemePublishDelta.Everything" /> when the current snapshot was not installed by that commit (first
    /// snapshot, package read override).
    /// </summary>
    internal ThemePublishDelta LastPublish
    {
        get
        {
            var delta = Volatile.Read(ref _lastPublish);
            return ReferenceEquals(delta.Current, Snapshot) ? delta : ThemePublishDelta.Everything;
        }
    }

    internal event EventHandler? Published;

    internal void Commit(ThemeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var previous = Volatile.Read(ref _snapshot);
        Volatile.Write(ref _lastPublish, ThemePublishDelta.Between(previous, snapshot));
        Volatile.Write(ref _snapshot, snapshot);
    }

    /// <summary>
    /// Registered-owner lookup cached per snapshot for the bindings that refresh on every publish (UI thread only,
    /// like the bindings). Returns <see langword="true" /> when the owner is registered; <paramref name="slot" /> is
    /// then its Control slot and <c>-1</c> otherwise.
    /// </summary>
    internal bool TryGetRegisteredOwnerSlot(ControlTokenIdentity owner, out int slot)
    {
        var snapshot = Snapshot;
        var slots = _ownerSlots;
        if (slots is null || !ReferenceEquals(_ownerSlotSnapshot, snapshot))
        {
            slots = new Dictionary<ControlTokenIdentity, int>();
            _ownerSlots = slots;
            _ownerSlotSnapshot = snapshot;
        }
        if (!slots.TryGetValue(owner, out slot))
        {
            slot = snapshot.Registry.TryGetControl(owner, out var descriptor) ? descriptor.Slot : -1;
            slots.Add(owner, slot);
        }
        return slot >= 0;
    }

    internal IReadOnlyList<ThemeDiagnostic> Publish(bool notifyResources = true)
    {
        var diagnostics = new List<ThemeDiagnostic>();
        Interlocked.Increment(ref _publishVersion);
        t_publishDepth++;
        try
        {
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
        }
        finally
        {
            t_publishDepth--;
        }
        return diagnostics.AsReadOnly();
    }
}
