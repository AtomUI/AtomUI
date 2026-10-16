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

    // registry 和 snapshot 必须作为一个读取视图发布，后台查询不能组合两代 slot。
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
internal sealed class PackagePublication
{
    private int _committed;
    internal bool IsCommitted => Volatile.Read(ref _committed) != 0;
    internal void Complete() => Volatile.Write(ref _committed, 1);
}
