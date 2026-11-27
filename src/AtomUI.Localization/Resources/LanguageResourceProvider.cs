using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace AtomUI.Localization;

/// <summary>
/// Resource provider that serves the text of the current language to the Avalonia resource system. AtomUI.Core
/// exposes it as the resource provider of the application scope.
/// </summary>
/// <remarks>
/// A key is an enum value: its enum type selects the catalog and its value the unit inside it. A lookup returns
/// the text of that unit in the current language, or <see langword="false" /> with a <see langword="null" />
/// value when the enum type is not a registered catalog or the value is not a unit of it. The key
/// <c>LanguageResourceKeys.FlowDirection</c> is an exception: it yields <see cref="FlowDirection.RightToLeft" />
/// or <see cref="FlowDirection.LeftToRight" /> from the text direction of the current language.
/// </remarks>
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
