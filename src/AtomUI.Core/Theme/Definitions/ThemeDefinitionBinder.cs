using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Definitions;

internal static class ThemeDefinitionBinder
{
    private const string UnknownAlgorithmCode = "ATMTHM2001";
    private const string UnknownControlCode = "ATMTHM2002";
    private const string UnknownTokenCode = "ATMTHM2003";
    private const string InvalidAlgorithmPolicyCode = "ATMTHM3001";
    private const string InvalidTokenValueCode = "ATMTHM3002";
    private const string InvalidAppearanceCode = "ATMTHM3003";
    private const string ConflictingAliasCode = "ATMTHM3004";

    internal static ThemeDefinitionBindResult Bind(
        ThemeDocument document,
        ThemeSchemaRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(registry);

        var diagnostics = new List<ThemeDefinitionDiagnostic>();
        var algorithms = BindAlgorithms(document.Algorithms, registry, diagnostics);
        var tokens = BindGlobalTokens(document.Tokens, registry, diagnostics);
        var controls = BindControls(document.Controls, registry, diagnostics);
        var effectiveAppearance = ResolveAppearance(algorithms);
        if (effectiveAppearance != document.Appearance)
        {
            AddError(
                diagnostics,
                InvalidAppearanceCode,
                document.Location,
                $"Theme appearance '{document.Appearance}' does not match algorithm result '{effectiveAppearance}'.");
        }

        if (diagnostics.Any(static diagnostic =>
                diagnostic.Severity == ThemeDefinitionDiagnosticSeverity.Error))
        {
            return new ThemeDefinitionBindResult(null, diagnostics);
        }

        return new ThemeDefinitionBindResult(
            new PreparedThemeDefinition(
                document.Id,
                document.Name,
                document.Appearance,
                effectiveAppearance,
                document.IsDefault,
                algorithms,
                tokens,
                controls,
                document.Location),
            diagnostics);
    }

    private static IReadOnlyList<ThemeAlgorithmDescriptor> BindAlgorithms(
        IReadOnlyList<ThemeAlgorithmDocument> documents,
        ThemeSchemaRegistry registry,
        List<ThemeDefinitionDiagnostic> diagnostics)
    {
        var algorithms = new List<ThemeAlgorithmDescriptor>(documents.Count);
        foreach (var document in documents)
        {
            if (!registry.TryGetAlgorithm(document.Algorithm, out var descriptor))
            {
                AddError(
                    diagnostics,
                    UnknownAlgorithmCode,
                    document.Location,
                    $"Theme algorithm '{document.Algorithm}' is not registered.");
                continue;
            }

            algorithms.Add(descriptor);
        }

        return algorithms;
    }

    private static IReadOnlyList<BoundTokenValue> BindGlobalTokens(
        IReadOnlyList<ThemeTokenDocument> documents,
        ThemeSchemaRegistry registry,
        List<ThemeDefinitionDiagnostic> diagnostics)
    {
        var tokens = new List<BoundTokenValue>(documents.Count);
        foreach (var document in documents)
        {
            if (!registry.TryGetGlobalToken(document.Name, out var descriptor))
            {
                AddError(
                    diagnostics,
                    UnknownTokenCode,
                    document.Location,
                    $"Global Token '{document.Name}' is not registered.");
                continue;
            }

            if (TryBindToken(document, descriptor, diagnostics, out var value))
            {
                tokens.Add(value);
            }
        }

        return tokens;
    }

    private static IReadOnlyList<ControlThemeDefinition> BindControls(
        IReadOnlyList<ControlThemeDocument> documents,
        ThemeSchemaRegistry registry,
        List<ThemeDefinitionDiagnostic> diagnostics)
    {
        if (registry.IsGlobalOnly)
        {
            return Array.Empty<ControlThemeDefinition>();
        }

        var controls = new List<ControlThemeDefinition>(documents.Count);
        foreach (var document in documents)
        {
            var identity = new ControlTokenIdentity(document.Identity.Catalog, document.Identity.Id);
            if (!registry.TryGetControl(identity, out var descriptor))
            {
                AddError(
                    diagnostics,
                    UnknownControlCode,
                    document.Location,
                    $"Control '{identity}' is not registered.");
                continue;
            }

            if (document.AlgorithmMode == ControlAlgorithmMode.Unspecified &&
                document.Algorithms.Count == 0 &&
                document.Tokens.Count == 0)
            {
                AddError(
                    diagnostics,
                    InvalidAlgorithmPolicyCode,
                    document.Location,
                    $"Control '{identity}' must declare an algorithm or at least one Token.");
                continue;
            }

            var hasCustomAlgorithms = document.Algorithms.Count != 0;
            if (hasCustomAlgorithms != (document.AlgorithmMode == ControlAlgorithmMode.Custom))
            {
                AddError(
                    diagnostics,
                    InvalidAlgorithmPolicyCode,
                    document.Location,
                    $"Control '{identity}' must use either Algorithm or Algorithms, not both.");
                continue;
            }

            var algorithms = BindAlgorithms(document.Algorithms, registry, diagnostics);
            var globalTokens = new List<BoundTokenValue>(document.Tokens.Count);
            var ownTokens = new List<BoundTokenValue>(document.Tokens.Count);
            foreach (var token in document.Tokens)
            {
                var matched = false;
                if (descriptor.TryGetOwnToken(token.Name, out var ownDescriptor))
                {
                    matched = true;
                    if (TryBindToken(token, descriptor.GetCanonicalOwnToken(ownDescriptor), diagnostics, out var value))
                    {
                        ownTokens.Add(ReferenceEquals(value.Descriptor, ownDescriptor)
                            ? value : value with { Descriptor = ownDescriptor });
                    }
                }

                if (!matched && registry.TryGetGlobalToken(token.Name, out var globalDescriptor))
                {
                    matched = true;
                    if (TryBindToken(token, globalDescriptor, diagnostics, out var value))
                    {
                        globalTokens.Add(value);
                    }
                }

                if (!matched)
                {
                    AddError(
                        diagnostics,
                        UnknownTokenCode,
                        token.Location,
                        $"Token '{token.Name}' is not registered for Control '{identity}'.");
                }
            }

            controls.Add(new ControlThemeDefinition(
                descriptor,
                document.AlgorithmMode,
                algorithms,
                globalTokens,
                ControlTokenAliasNormalizer.Normalize(descriptor, ownTokens, ConflictingAliasCode, diagnostics),
                document.Location));
        }

        return controls;
    }

    private static bool TryBindToken(
        ThemeTokenDocument document,
        TokenDescriptor descriptor,
        List<ThemeDefinitionDiagnostic> diagnostics,
        out BoundTokenValue value)
    {
        try
        {
            var parsed = descriptor.Parse(document.Value);
            if (!IsCompatibleValue(descriptor.ValueType, parsed))
            {
                throw new InvalidOperationException("The Token parser returned an incompatible value.");
            }

            value = new BoundTokenValue(descriptor, parsed, document.Location);
            return true;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            AddError(
                diagnostics,
                InvalidTokenValueCode,
                document.Location,
                $"Token '{document.Name}' cannot be converted to '{descriptor.ValueType.Name}'.");
            value = null!;
            return false;
        }
    }

    private static bool IsCompatibleValue(Type expectedType, object? value)
    {
        if (value is null)
        {
            return !expectedType.IsValueType || Nullable.GetUnderlyingType(expectedType) is not null;
        }

        return (Nullable.GetUnderlyingType(expectedType) ?? expectedType).IsInstanceOfType(value);
    }

    internal static ThemeAppearance ResolveAppearance(
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms)
    {
        var appearance = ThemeAppearance.Light;
        foreach (var algorithm in algorithms)
        {
            appearance = algorithm.AppearanceEffect switch
            {
                ThemeAppearanceEffect.Preserve => appearance,
                ThemeAppearanceEffect.Light    => ThemeAppearance.Light,
                ThemeAppearanceEffect.Dark     => ThemeAppearance.Dark,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(algorithm.AppearanceEffect),
                    algorithm.AppearanceEffect,
                    "Unsupported theme appearance effect.")
            };
        }

        return appearance;
    }

    private static void AddError(
        List<ThemeDefinitionDiagnostic> diagnostics,
        string code,
        ThemeSourceLocation location,
        string message)
    {
        diagnostics.Add(new ThemeDefinitionDiagnostic(
                            code,
                            ThemeDefinitionDiagnosticSeverity.Error,
                            location.Source,
                            location.Line,
                            location.Column,
                            location.Path,
                            message));
    }
}
