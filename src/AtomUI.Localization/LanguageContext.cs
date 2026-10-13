namespace AtomUI.Localization;

internal sealed class LanguageContext
{
    private LanguageReadView _view;

    internal LanguageContext(
        LanguageCatalogRegistry registry,
        LanguageRevision initialRevision)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(initialRevision);
        _view = new LanguageReadView(registry, initialRevision);
    }

    internal LanguageReadView Capture() => Volatile.Read(ref _view);
    internal LanguageCatalogRegistry Registry => Capture().Registry;
    internal LanguageRevision Current => Capture().Revision;

    internal void Publish(LanguageRevision revision)
    {
        ArgumentNullException.ThrowIfNull(revision);
        Commit(new LanguageReadView(Registry, revision));
    }

    // registry 和 snapshot 必须作为一个读取视图发布，后台查询不能组合两代 slot。
    internal void Commit(LanguageReadView view) => Volatile.Write(ref _view, view);
}

internal sealed record LanguageReadView(LanguageCatalogRegistry Registry, LanguageRevision Revision);
