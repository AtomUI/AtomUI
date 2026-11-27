namespace AtomUI.Localization;

internal sealed class LanguageContext
{
    private LanguageReadView _view;
    private StagedLanguageView? _stagedView;

    internal LanguageContext(
        LanguageCatalogRegistry registry,
        LanguageRevision initialRevision)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(initialRevision);
        _view = new LanguageReadView(registry, initialRevision);
    }

    internal LanguageReadView Capture()
    {
        var staged = Volatile.Read(ref _stagedView);
        return staged is null ? Volatile.Read(ref _view) :
            staged.Publication?.IsCommitted == true || staged.ThreadId == Environment.CurrentManagedThreadId
                ? staged.Candidate : staged.Committed;
    }
    internal LanguageCatalogRegistry Registry => Capture().Registry;
    internal LanguageRevision Current => Capture().Revision;

    internal void Publish(LanguageRevision revision)
    {
        ArgumentNullException.ThrowIfNull(revision);
        Commit(new LanguageReadView(Registry, revision));
    }

    // The registry and the snapshot must be published as one read view, so that a background query never
    // combines slots of two generations.
    internal void Commit(LanguageReadView view) => Volatile.Write(ref _view, view);

    // Synchronous UI hooks can resolve candidate translations, while background localizers keep
    // reading the committed view until every hook and owned-service attach has succeeded.
    internal void Stage(LanguageReadView view, PackagePublication? publication = null)
    {
        Volatile.Write(ref _stagedView, new StagedLanguageView(Volatile.Read(ref _view), view,
            Environment.CurrentManagedThreadId, publication));
    }

    internal void EndStaging()
    {
        Volatile.Write(ref _stagedView, null);
    }

    private sealed record StagedLanguageView(LanguageReadView Committed, LanguageReadView Candidate,
        int ThreadId, PackagePublication? Publication);
}

internal sealed record LanguageReadView(LanguageCatalogRegistry Registry, LanguageRevision Revision);

// Core and Localization publish one application package transaction through this shared fence.
/// <summary>
/// Commit flag that AtomUI.Core creates for one package append and shares between the theme update and the
/// language update.
/// </summary>
/// <remarks>
/// While a language view is staged, <see cref="LanguageContext.Capture" /> returns the candidate view only to
/// the thread that staged it and the previously committed view to every other thread. Once
/// <see cref="IsCommitted" /> is <see langword="true" />, all threads get the candidate view. The theme update
/// reads the same flag, so background readers switch to the new theme and to the new language together.
/// AtomUI.Core calls <see cref="Complete" /> after every fallible hook of the append has run and before it
/// commits the two updates.
/// </remarks>
internal sealed class PackagePublication
{
    private int _committed;
    internal bool IsCommitted => Volatile.Read(ref _committed) != 0;
    internal void Complete() => Volatile.Write(ref _committed, 1);
}
