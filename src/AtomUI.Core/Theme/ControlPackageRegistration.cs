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
        SortIfNeeded(resourceArray, ControlThemeResourceRegistration.CompareOrder);
        Resources = Array.AsReadOnly(resourceArray);
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

        var controlArray = controls.ToArray();
        SortIfNeeded(controlArray, static (left, right) => CompareIdentity(left.Identity, right.Identity));
        EnsureUnique(
            controlArray,
            static control => control.Identity,
            EqualityComparer<ControlTokenIdentity>.Default,
            "Control identity");
        var assetArray = themeAssets.ToArray();
        SortIfNeeded(assetArray, static (left, right) => StringComparer.Ordinal.Compare(left.AssetUri.ToString(), right.AssetUri.ToString()));
        EnsureUnique(assetArray, static asset => asset.AssetId, StringComparer.Ordinal, "theme AssetId");
        EnsureUnique(
            assetArray,
            static asset => asset.AssetUri.ToString(),
            StringComparer.Ordinal,
            "theme asset URI");
        var semanticArray = semanticControls.ToArray();
        if (semanticArray.Any(static descriptor => descriptor is null))
        {
            throw new ArgumentException(
                "Semantic Control descriptors cannot contain null.",
                nameof(semanticControls));
        }
        SortIfNeeded(semanticArray, static (left, right) => CompareIdentity(left.Identity, right.Identity));
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

    // Only the configuration builder owns this continuation. It is cleared on completion or cancellation.
    internal Func<ControlPackageRegistration>? CompleteRegistration { get; set; }

    public string Id { get; }
    public IReadOnlyList<ControlTokenDescriptor> Controls { get; }
    public IReadOnlyList<ControlThemeAssetDescriptor> ThemeAssets { get; }
    public IReadOnlyList<ControlSemanticDescriptor> SemanticControls { get; }
    public IControlThemesProvider ControlThemesProvider { get; }

    private static int CompareIdentity(ControlTokenIdentity left, ControlTokenIdentity right)
    {
        var result = StringComparer.Ordinal.Compare(left.Catalog, right.Catalog);
        return result != 0 ? result : StringComparer.Ordinal.Compare(left.Id, right.Id);
    }

    // Inputs are copied before sorting. Reuse that owned array, including for arbitrary public inputs.
    private static void SortIfNeeded<T>(T[] values, Comparison<T> comparison)
    {
        for (var index = 1; index < values.Length; index++)
        {
            if (comparison(values[index - 1], values[index]) > 0)
            {
                Array.Sort(values, comparison);
                return;
            }
        }
    }

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
