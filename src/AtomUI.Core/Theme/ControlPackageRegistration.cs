using AtomUI.Registration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme;

/// <summary>
/// What one control package hands to the theme manager: the control tokens, theme assets and semantic part
/// descriptors it contributes, the provider of its control themes, and its theme resources.
/// </summary>
/// <remarks>
/// Register it with <see cref="IThemeManagerBuilder.AddControlPackage(ControlPackageRegistration)" />. The theme
/// manager builder reads the lists when it merges the package into the theme manager.
/// </remarks>
public sealed class ControlPackageRegistration
{
    /// <summary>
    /// Validates the package and copies the descriptors and resources into read-only lists.
    /// </summary>
    /// <param name="id">
    /// The package identity, which must be identical to the id of the theme provider.
    /// </param>
    /// <param name="controls">
    /// The control token descriptors, without <see langword="null" /> entries. Their identities must be unique.
    /// </param>
    /// <param name="themeAssets">
    /// The theme asset descriptors, without <see langword="null" /> entries. Their asset ids and asset URIs must each
    /// be unique.
    /// </param>
    /// <param name="semanticControls">
    /// The semantic part descriptors. Their identities and control types must each be unique.
    /// </param>
    /// <param name="controlThemesProvider">
    /// Supplies the control themes of the package.
    /// </param>
    /// <param name="resources">
    /// The theme resource registrations. Their asset ids must be unique.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// An argument is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="id" /> is blank or differs from the id of <paramref name="controlThemesProvider" />, a list
    /// holds a duplicate, or <paramref name="resources" /> or <paramref name="semanticControls" /> holds a
    /// <see langword="null" /> entry.
    /// </exception>
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

    /// <summary>
    /// The theme resource registrations of the package, sorted by phase, then order, then asset id.
    /// </summary>
    /// <remarks>
    /// Each registration pairs an asset id with the factory that produces the theme resources of that asset. When
    /// control packages are loaded, the theme manager adds every registration to its resources as a provider that
    /// creates them lazily.
    /// </remarks>
    public IReadOnlyList<ControlThemeResourceRegistration> Resources { get; }
    /// <summary>
    /// The position of this package among the control packages the theme manager builder has accepted, in the order
    /// of acceptance. It is <c>-1</c> until the builder accepts the package.
    /// </summary>
    public long PackageCommitOrdinal { get; internal set; } = -1;
    /// <summary>
    /// The package-level schema fingerprint that generated registration code precomputes at compile time.
    /// </summary>
    /// <remarks>
    /// It is <see langword="null" /> for a package registered by hand and for a package whose deferred registration
    /// fragments are still pending. The startup theme schema composes its revision from these constants only when
    /// every package of the builder has one; a single <see langword="null" /> makes it compute the revision from the
    /// full schema instead.
    /// </remarks>
    public ulong? SchemaFingerprint { get; internal set; }

    // Only the configuration builder owns this continuation. It is cleared on completion or cancellation.
    internal Func<ControlPackageRegistration>? CompleteRegistration { get; set; }

    /// <summary>
    /// The package identity, which is identical to the id of <see cref="ControlThemesProvider" />.
    /// </summary>
    public string Id { get; }
    /// <summary>
    /// The control token descriptors of the package, sorted by identity (catalog, then id).
    /// </summary>
    public IReadOnlyList<ControlTokenDescriptor> Controls { get; }
    /// <summary>
    /// The theme asset descriptors of the package, sorted by asset URI.
    /// </summary>
    public IReadOnlyList<ControlThemeAssetDescriptor> ThemeAssets { get; }
    /// <summary>
    /// The semantic part descriptors of the package, sorted by identity (catalog, then id). The theme manager builds
    /// <see cref="IThemeManager.SemanticParts" /> from them.
    /// </summary>
    public IReadOnlyList<ControlSemanticDescriptor> SemanticControls { get; }
    /// <summary>
    /// The provider that supplies the control themes of the package. Its id is the package <see cref="Id" />.
    /// </summary>
    /// <remarks>
    /// The theme manager builder rejects a second package whose provider id is already registered.
    /// </remarks>
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
