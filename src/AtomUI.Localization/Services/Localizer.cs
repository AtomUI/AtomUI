namespace AtomUI.Localization;

/// <summary>
/// Lookup of translated text behind <see cref="ILocalizer" />: it resolves an enum key in the current language
/// and formats it in the formatting culture of that language. AtomUI.Core exposes it as the localizer of the
/// application scope.
/// </summary>
/// <remarks>
/// <c>Format</c> returns the text unchanged when the unit is not declared as a formatted unit. When formatting
/// fails, for example because an argument does not fit a placeholder, a <see cref="LanguageCatalogException" />
/// that names the catalog, the unit and the language is thrown instead of the <see cref="FormatException" />.
/// </remarks>
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
        var found = catalog is LanguageCatalogDescriptor<TResourceKind> typedCatalog
            ? typedCatalog.TryGetUnitSlot(key, out var unitSlot)
            : catalog.TryGetUnitSlot(key, out unitSlot);
        if (!found)
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
