using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

/// <summary>
/// Validates language templates against referenced controls without generating their APIs again.
/// </summary>
internal static class ExternalThemeSemanticValidator
{
    private const string LanguageKeyPrefix = "AtomUI.Theme.";

    internal static void Validate(Compilation compilation, IReadOnlyList<ThemeAssetInfo> localAssets,
                                  Action<Diagnostic> report, CancellationToken cancellationToken)
    {
        var contributions = localAssets.SelectMany(asset => asset.SemanticThemes
            .Where(static theme => theme.ExportKey?.StartsWith(LanguageKeyPrefix, StringComparison.Ordinal) == true)
            .Select(theme => (Asset: asset, Theme: theme))).ToArray();
        if (contributions.Length == 0)
        {
            return;
        }

        var resolver = new SemanticPartTypeResolver(compilation, Array.Empty<INamedTypeSymbol>());
        var baseline = SemanticThemeFacts.Read(compilation).ToArray();
        foreach (var language in contributions.GroupBy(static entry => GetLanguage(entry.Theme.ExportKey!)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var replacements = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var overrides = new Dictionary<INamedTypeSymbol, Location>(SymbolEqualityComparer.Default);
            foreach (var entry in language)
            {
                if (resolver.ResolveTargetType(entry.Theme.TargetType) is not { } target ||
                    SymbolEqualityComparer.Default.Equals(target.ContainingAssembly, compilation.Assembly))
                {
                    continue;
                }
                overrides[target] = entry.Asset.CreateLocation();
                if (entry.Theme.OverridesBaseTemplate)
                {
                    replacements.Add(target);
                }
            }
            if (overrides.Count == 0)
            {
                continue;
            }

            var allLocalOverrides = new HashSet<ThemeAssetSemanticThemeInfo>(language.Select(static entry => entry.Theme));
            var local = localAssets.Select(asset => Filter(asset, theme =>
                theme.ExportKey?.StartsWith(LanguageKeyPrefix, StringComparison.Ordinal) != true ||
                allLocalOverrides.Contains(theme)));
            var inherited = baseline.Select(asset => Filter(asset, theme =>
                resolver.ResolveTargetType(theme.TargetType) is not { } type || !replacements.Contains(type)));
            var effective = local.Concat(inherited).ToArray();
            var validator = new SemanticPartTemplateValidator(effective, resolver, report);

            foreach (var pair in overrides)
            {
                var declaration = SemanticControlDeclaration.CreateForThemeTarget(pair.Key, pair.Value, cancellationToken);
                validator.Validate(declaration, declaration.Parts);
            }

            // Revalidate an inherited outer contract when its anchor resolves to an overridden inner owner.
            foreach (var group in baseline.SelectMany(static asset => asset.SemanticThemes)
                         .Select(theme => (Theme: theme, Target: resolver.ResolveTargetType(theme.TargetType)))
                         .Where(static entry => entry.Target is not null)
                         .GroupBy(static entry => entry.Target!, SymbolEqualityComparer.Default))
            {
                if (group.Key is not INamedTypeSymbol target || overrides.ContainsKey(target))
                {
                    continue;
                }
                var location = overrides.Values.First();
                var declaration = SemanticControlDeclaration.CreateForThemeTarget(target, location, cancellationToken);
                var parts = declaration.Parts.Where(part => part.CrossNestedOwners && part.SelectorRoute is { } route &&
                    group.SelectMany(entry => entry.Theme.Templates).SelectMany(static template => template.Markers)
                         .Any(marker => route.Split(' ').Contains("." + marker.SelectorClass, StringComparer.Ordinal) &&
                                        resolver.ResolveMarkerType(marker) is { } inner && overrides.ContainsKey(inner))).ToArray();
                if (parts.Length != 0)
                {
                    validator.Validate(declaration, parts);
                }
            }
        }
    }

    private static string GetLanguage(string key)
    {
        var separator = key.IndexOf('.', LanguageKeyPrefix.Length);
        return separator > LanguageKeyPrefix.Length ? key.Substring(LanguageKeyPrefix.Length,
            separator - LanguageKeyPrefix.Length) : string.Empty;
    }

    private static ThemeAssetInfo Filter(ThemeAssetInfo asset, Func<ThemeAssetSemanticThemeInfo, bool> predicate)
    {
        var themes = asset.SemanticThemes.Where(predicate).ToArray();
        return themes.Length == asset.SemanticThemes.Count ? asset :
            ThemeAssetInfo.FromSemanticFacts(asset.AssetPath, themes);
    }
}
