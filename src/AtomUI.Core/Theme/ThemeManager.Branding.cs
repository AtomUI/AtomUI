using AtomUI.Theme.Branding;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;
using System.Globalization;

namespace AtomUI.Theme;

internal partial class ThemeManager
{
    private ThemeStyleAssetRegistry _styleAssetRegistry = ThemeStyleAssetRegistry.Empty;
    internal ThemeStyleAssetRegistry StyleAssetRegistry => PackageReadUpdate?.StyleAssets ?? _styleAssetRegistry;

    internal void RegisterStyleAssets(IEnumerable<ThemeStyleAssetRegistration> registrations)
    {
        _styleAssetRegistry = _styleAssetRegistry.Append(registrations, _startupRegistry!, SemanticParts);
    }

    internal void VerifyBrandReadAccess()
    {
        if (!_checkTransitionAccess() || _loadingPackages)
        {
            throw new InvalidOperationException("Brand recipes require a stable runtime on its UI thread.");
        }
        ObjectDisposedException.ThrowIf(_disposeRequested, this);
        if (!_applicationInitialized || CurrentSnapshot is null)
        {
            throw new InvalidOperationException("Brand recipes require an initialized runtime.");
        }
    }

    internal ThemeBrandRecipeReadResult ValidateBrandRecipe(ThemeBrandRecipe recipe)
    {
        VerifyBrandReadAccess();
        ArgumentNullException.ThrowIfNull(recipe);
        var diagnostics = new List<ThemeDiagnostic>();
        var entry = CompiledThemeCatalog.OrderedDefinitions.FirstOrDefault(item => item.Definition.Id == recipe.ThemeId);
        if (entry is null)
        {
            diagnostics.Add(BrandDiagnostic("ATMTHM7205", "$.themeId", "The recipe base theme is not registered."));
        }
        else
        {
            diagnostics.AddRange(ValidateBrandRequirements(recipe.Requirements, CurrentSnapshot!.RegistryRevision, entry.Revision));
        }
        foreach (var reference in recipe.Assets)
        {
            if (!StyleAssetRegistry.TryGet(reference.Id, reference.Revision, out var registration))
            {
                diagnostics.Add(BrandDiagnostic("ATMTHM7203", "$.assets", $"Asset '{reference.Id}@{reference.Revision}' is not registered."));
            }
            else if ((registration!.RequiredCapabilities & ~recipe.Requirements.Capabilities) != 0)
            {
                diagnostics.Add(BrandDiagnostic("ATMTHM7202", "$.requires.capabilities", $"Asset '{reference.Id}' requires additional capabilities."));
            }
        }
        try
        {
            diagnostics.AddRange(ConvertDiagnostics(ThemeConfigNormalizer.Normalize(recipe.Config, CurrentSnapshot!.Registry).Diagnostics));
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or ThemeSchemaException)
        {
            // Disabled-package metadata deliberately throws; data validation reports it without enabling the package.
            diagnostics.Add(BrandDiagnostic("ATMTHM7206", "$.config", exception.Message));
        }
        return new ThemeBrandRecipeReadResult(diagnostics.Any(static item => item.Severity == ThemeDiagnosticSeverity.Error)
            ? null : recipe, diagnostics);
    }

    private static IReadOnlyList<ThemeDiagnostic> ValidateBrandRequirements(ThemeBrandRequirements? requirements,
        ThemeSchemaRevision schema, ThemeDefinitionRevision definition)
    {
        if (requirements is null)
        {
            return [];
        }
        var diagnostics = new List<ThemeDiagnostic>();
        if ((requirements.Capabilities & ~ThemeStyleAssetRegistry.SupportedCapabilities) != 0)
        {
            diagnostics.Add(BrandDiagnostic("ATMTHM7202", "$.requires.capabilities", "The runtime does not support all required capabilities."));
        }
        if (requirements.SchemaRevision != schema.Value.ToString("X16", CultureInfo.InvariantCulture) ||
            requirements.DefinitionDigest != definition.ContentDigest || requirements.ProgramDigest != definition.ProgramDigest)
        {
            diagnostics.Add(BrandDiagnostic("ATMTHM7205", "$.requires", "The recipe requires a different schema or base theme revision."));
        }
        return diagnostics;
    }

    private static ThemeDiagnostic BrandDiagnostic(string code, string path, string message) =>
        new(code, ThemeDiagnosticSeverity.Error, "BrandRecipe", path, message);

    private IReadOnlyList<ThemeDiagnostic> ValidateBrandConfig(ThemeConfig? config, ThemeSchemaRevision schema,
        ThemeDefinitionRevision definition, ThemeStyleAssetRegistry? assets = null)
    {
        if (config?.BrandRequirements is null)
        {
            return [];
        }
        var diagnostics = ValidateBrandRequirements(config.BrandRequirements, schema, definition).ToList();
        assets ??= StyleAssetRegistry;
        foreach (var asset in config.StyleAssets)
        {
            if (!assets.TryGet(asset.Id, asset.Revision, out var registration) || !ReferenceEquals(registration!.Asset, asset))
            {
                diagnostics.Add(BrandDiagnostic("ATMTHM7203", "$.assets", $"Bound asset '{asset.Id}@{asset.Revision}' is not canonical in this runtime."));
            }
        }
        return diagnostics;
    }

    private IReadOnlyList<ThemeDiagnostic> ValidateBrandRequest(ThemeRequest request, ThemeSchemaRevision schema,
        ThemeDefinitionRevision definition, ThemeStyleAssetRegistry? assets = null)
    {
        var diagnostics = ValidateBrandConfig(request.Config, schema, definition, assets);
        return request.Config?.Inherit == false || ReferenceEquals(request.Config, _initialRequest.Config)
            ? diagnostics
            : diagnostics.Concat(ValidateBrandConfig(_initialRequest.Config, schema, definition, assets)).ToArray();
    }

    private ThemeConfig BindValidatedBrandRecipe(ThemeBrandRecipe recipe)
    {
        var assets = recipe.Assets.Select(reference =>
        {
            StyleAssetRegistry.TryGet(reference.Id, reference.Revision, out var registration);
            return registration!.Asset;
        }).ToArray();
        return new ThemeConfig(recipe.Config.Inherit, recipe.Config.Algorithms, recipe.Config.Tokens, recipe.Config.Controls,
            assets, new ThemeBrandSelection(recipe.Id, recipe.Revision), recipe.Requirements);
    }

    internal ThemeConfig BindBrandRecipe(ThemeBrandRecipe recipe)
    {
        var validation = ValidateBrandRecipe(recipe);
        if (!validation.Success)
        {
            throw new ArgumentException(string.Join(" ", validation.Diagnostics.Select(static item => item.Message)), nameof(recipe));
        }
        return BindValidatedBrandRecipe(recipe);
    }

    internal Task<ThemeTransitionResult> ApplyBrandRecipeAsync(ThemeBrandRecipe recipe, CancellationToken cancellationToken)
    {
        VerifyTransitionAccess();
        cancellationToken.ThrowIfCancellationRequested();
        var validation = ValidateBrandRecipe(recipe);
        if (validation.Success)
        {
            return ApplyThemeAsync(new ThemeRequest(recipe.ThemeId, BindValidatedBrandRecipe(recipe),
                ThemeTransitionReason.UserRequest), cancellationToken);
        }
        return Task.FromResult(RejectBrandRequest(new ThemeRequest(recipe.ThemeId, recipe.Config,
            ThemeTransitionReason.UserRequest), validation.Diagnostics));
    }

    private ThemeTransitionResult RejectBrandRequest(ThemeRequest request, IReadOnlyList<ThemeDiagnostic> diagnostics)
    {
        var publishDiagnostics = new List<ThemeDiagnostic>();
        var transitionId = NextTransitionId();
        var state = CurrentTheme;
        ThemeEventDispatcher.Dispatch(ThemeChangeFailed, this,
            new ThemeChangeFailedEventArgs(request, diagnostics, null), publishDiagnostics, nameof(ThemeChangeFailed));
        return new ThemeTransitionResult(transitionId, ThemeTransitionStatus.Failed, state,
            diagnostics, publishDiagnostics, null);
    }

    internal ThemeBrandRecipe ExportBrandRecipe(string id, string revision, ThemeBrandRecipeKind kind)
    {
        VerifyBrandReadAccess();
        var snapshot = CurrentSnapshot!;
        var inputs = RootContext.StyleInputs;
        var references = new List<ThemeStyleAssetReference>();
        var capabilities = (ThemeStyleCapabilities)0;
        if (kind == ThemeBrandRecipeKind.Complete)
        {
            foreach (var asset in inputs.Assets)
            {
                if (!StyleAssetRegistry.TryGet(asset.Id, asset.Revision, out var registration) ||
                    !ReferenceEquals(registration!.Asset, asset))
                {
                    throw new InvalidOperationException($"Asset '{asset.Id}@{asset.Revision}' has no canonical registration and cannot be exported.");
                }
                references.Add(new ThemeStyleAssetReference(asset.Id, asset.Revision));
                capabilities |= registration.RequiredCapabilities;
            }
        }
        var effective = snapshot.EffectiveConfig;
        var controls = effective.Controls.ToDictionary(static item => item.Identity, static item => new ControlThemeConfig(
            item.AlgorithmMode, item.AlgorithmMode == ControlAlgorithmMode.Custom
                ? item.Algorithms.Select(static algorithm => algorithm.Algorithm).ToArray() : null,
            item.GlobalTokens.Concat(item.OwnTokens).ToDictionary(static token => token.Descriptor.Name,
                static token => token.CanonicalValue, StringComparer.Ordinal)));
        var config = new ThemeConfig(false, effective.Algorithms.Select(static algorithm => algorithm.Algorithm).ToArray(),
            effective.GlobalTokens.ToDictionary(static token => token.Descriptor.Name, static token => token.CanonicalValue,
                StringComparer.Ordinal), controls);
        return new ThemeBrandRecipe(id, revision, snapshot.ThemeId, config, references,
            new ThemeBrandRequirements(snapshot.RegistryRevision.Value.ToString("X16", CultureInfo.InvariantCulture),
                snapshot.DefinitionRevision.ContentDigest, snapshot.DefinitionRevision.ProgramDigest, capabilities), kind);
    }

    internal ThemeBrandSelection? GetBrandSelection()
    {
        VerifyBrandReadAccess();
        return RootContext.StyleInputs.Selection;
    }
}
