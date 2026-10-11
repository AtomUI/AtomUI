using AtomUI.Registration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme;

public sealed class ControlPackageRegistration
{
    public ControlPackageRegistration(
        string id,
        IEnumerable<ControlTokenDescriptor> controls,
        IEnumerable<ControlThemeAssetDescriptor> themeAssets,
        IEnumerable<ControlSemanticDescriptor> semanticControls,
        IControlThemesProvider controlThemesProvider,
        IEnumerable<ControlThemeResourceRegistration> resources)
    {
        ArgumentNullException.ThrowIfNull(resources);
        var resourceArray = resources.ToArray();
        if (resourceArray.Any(static resource => resource is null))
        {
            throw new ArgumentException("Resource registrations cannot contain null.", nameof(resources));
        }
        EnsureUnique(resourceArray, static resource => resource.AssetId, StringComparer.Ordinal, "resource AssetId");
        Resources = Array.AsReadOnly(resourceArray.OrderBy(static resource => resource.Phase)
            .ThenBy(static resource => resource.Order).ThenBy(static resource => resource.AssetId, StringComparer.Ordinal).ToArray());
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(controls);
        ArgumentNullException.ThrowIfNull(themeAssets);
        ArgumentNullException.ThrowIfNull(semanticControls);
        ArgumentNullException.ThrowIfNull(controlThemesProvider);
        if (!string.Equals(id, controlThemesProvider.Id, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Control package id '{id}' must match theme provider id '{controlThemesProvider.Id}'.",
                nameof(controlThemesProvider));
        }

        var controlArray = controls.OrderBy(static control => control.Identity.Catalog, StringComparer.Ordinal)
                                   .ThenBy(static control => control.Identity.Id, StringComparer.Ordinal)
                                   .ToArray();
        EnsureUnique(
            controlArray,
            static control => control.Identity,
            EqualityComparer<ControlTokenIdentity>.Default,
            "Control identity");
        var assetArray = themeAssets.OrderBy(
            static asset => asset.AssetUri.ToString(),
            StringComparer.Ordinal).ToArray();
        EnsureUnique(assetArray, static asset => asset.AssetId, StringComparer.Ordinal, "theme AssetId");
        EnsureUnique(
            assetArray,
            static asset => asset.AssetUri.ToString(),
            StringComparer.Ordinal,
            "theme asset URI");
        ControlSemanticDescriptor?[] nullableSemanticControls = semanticControls.ToArray();
        if (nullableSemanticControls.Any(static descriptor => descriptor is null))
        {
            throw new ArgumentException(
                "Semantic Control descriptors cannot contain null.",
                nameof(semanticControls));
        }
        var semanticArray = nullableSemanticControls
                            .Select(static descriptor => descriptor!)
                            .OrderBy(static descriptor => descriptor.Identity.Catalog, StringComparer.Ordinal)
                            .ThenBy(static descriptor => descriptor.Identity.Id, StringComparer.Ordinal)
                            .ToArray();
        EnsureUnique(
            semanticArray,
            static descriptor => descriptor.Identity,
            EqualityComparer<ControlTokenIdentity>.Default,
            "Semantic Control identity");
        EnsureUnique(
            semanticArray,
            static descriptor => descriptor.ControlType,
            EqualityComparer<Type>.Default,
            "Semantic Control type");

        Id = id;
        Controls = Array.AsReadOnly(controlArray);
        ThemeAssets = Array.AsReadOnly(assetArray);
        SemanticControls = Array.AsReadOnly(semanticArray);
        ControlThemesProvider = controlThemesProvider;
    }

    public IReadOnlyList<ControlThemeResourceRegistration> Resources { get; }
    public long PackageCommitOrdinal { get; internal set; } = -1;
    /// <summary>生成器 emit 的包级 schema 指纹常量；手工注册为空，启动 Revision 回退全量计算。</summary>
    public ulong? SchemaFingerprint { get; internal set; }
    public string Id { get; }
    public IReadOnlyList<ControlTokenDescriptor> Controls { get; }
    public IReadOnlyList<ControlThemeAssetDescriptor> ThemeAssets { get; }
    public IReadOnlyList<ControlSemanticDescriptor> SemanticControls { get; }
    public IControlThemesProvider ControlThemesProvider { get; }

    private static void EnsureUnique<TItem, TKey>(
        IEnumerable<TItem> items,
        Func<TItem, TKey> keySelector,
        IEqualityComparer<TKey> comparer,
        string kind)
        where TKey : notnull
    {
        var keys = new HashSet<TKey>(comparer);
        foreach (var item in items)
        {
            var key = keySelector(item);
            if (!keys.Add(key))
            {
                throw new ArgumentException($"Control package contains duplicate {kind} '{key}'.");
            }
        }
    }
}
