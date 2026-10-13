using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace AtomUI.Localization;

internal sealed class LanguageResourceProvider : ResourceProvider
{
    private readonly LanguageContext _context;

    internal LanguageResourceProvider(LanguageContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public override bool HasResources => true;

    public override bool TryGetResource(
        object key,
        ThemeVariant? theme,
        out object? value)
    {
        ArgumentNullException.ThrowIfNull(key);

        var view = _context.Capture();
        var revision = view.Revision;
        if (ReferenceEquals(key, LanguageResourceKeys.FlowDirection))
        {
            value = revision.State.TextDirection == LanguageTextDirection.RightToLeft
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;
            return true;
        }
        if (!view.Registry.TryGetCatalogSlot(key.GetType(), out var catalogSlot))
        {
            value = null;
            return false;
        }

        var catalog = view.Registry.Catalogs[catalogSlot];
        if (!catalog.TryGetUnitSlot(key, out var unitSlot))
        {
            value = null;
            return false;
        }

        value = revision.Snapshot.GetEntry(catalogSlot, unitSlot).Text;
        return true;
    }

    internal void PublishResourcesChanged()
    {
        RaiseResourcesChanged();
    }
}
