namespace AtomUI.Localization;

internal sealed class Localizer : ILocalizer
{
    private readonly LanguageContext _context;

    internal Localizer(LanguageContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public string Get<TResourceKind>(TResourceKind key)
        where TResourceKind : struct, Enum
    {
        var view = _context.Capture();
        var resolved = Resolve(view, key);
        return resolved.Entry.Text;
    }

    public string Format<TResourceKind>(TResourceKind key, params object?[] arguments)
        where TResourceKind : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(arguments);
        var view = _context.Capture();
        var revision = view.Revision;
        var resolved = Resolve(view, key);
        if (resolved.Entry.CompositeFormat is null)
        {
            return resolved.Entry.Text;
        }

        try
        {
            return string.Format(
                revision.State.FormattingCulture,
                resolved.Entry.CompositeFormat,
                arguments);
        }
        catch (FormatException exception)
        {
            throw new LanguageCatalogException(
                $"Formatting Catalog '{resolved.Catalog.CatalogId}' unit " +
                $"'{resolved.Unit.Key}' for language " +
                $"'{revision.State.CurrentLanguage.Value}' failed.",
                exception);
        }
    }

    private ResolvedLanguageValue Resolve<TResourceKind>(
        LanguageReadView view,
        TResourceKind key)
        where TResourceKind : struct, Enum
    {
        var resourceKindType = typeof(TResourceKind);
        if (!view.Registry.TryGetCatalogSlot(resourceKindType, out var catalogSlot))
        {
            throw new LanguageCatalogException(
                $"Resource enum type '{resourceKindType.FullName}' is not registered as a language Catalog.");
        }

        var catalog = view.Registry.Catalogs[catalogSlot];
        if (catalog is not LanguageCatalogDescriptor<TResourceKind> typedCatalog)
        {
            throw new LanguageCatalogException(
                $"Catalog '{catalog.CatalogId}' is registered with incompatible resource enum type " +
                $"'{catalog.ResourceKindType.FullName}'.");
        }
        if (!typedCatalog.TryGetUnitSlot(key, out var unitSlot))
        {
            throw new LanguageCatalogException(
                $"A value of resource enum type '{resourceKindType.FullName}' is not mapped to any unit " +
                $"in Catalog '{catalog.CatalogId}'.");
        }

        return new ResolvedLanguageValue(
            catalog,
            catalog.Units[unitSlot],
            view.Revision.Snapshot.GetEntry(catalogSlot, unitSlot));
    }

    private readonly record struct ResolvedLanguageValue(
        LanguageCatalogDescriptor Catalog,
        LanguageCatalogUnitDescriptor Unit,
        LanguageSnapshotEntry Entry);
}
