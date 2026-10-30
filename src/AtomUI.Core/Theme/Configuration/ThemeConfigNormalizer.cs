using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Configuration;

internal static class ThemeConfigNormalizer
{
    private const string UnknownGlobalTokenCode = "ATMTHM4001";
    private const string UnknownControlCode = "ATMTHM4002";
    private const string UnknownControlTokenCode = "ATMTHM4003";
    private const string UnknownAlgorithmCode = "ATMTHM4004";
    private const string InvalidAlgorithmPolicyCode = "ATMTHM4005";
    private const string InvalidTokenValueCode = "ATMTHM4006";
    private const string InvalidInputCode = "ATMTHM4007";
    private const string Source = "<ThemeConfig>";

    internal static ThemeConfigNormalizeResult Normalize(ThemeConfig config, ThemeSchemaRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(registry);

        var diagnostics = new List<ThemeDefinitionDiagnostic>();
        var algorithmsSpecified = config.Algorithms is not null;
        var algorithms = algorithmsSpecified && config.Algorithms!.Count == 0
            ? BindDefaultAlgorithm(registry, diagnostics, "$.Algorithms")
            : BindAlgorithms(config.Algorithms, registry, diagnostics, "$.Algorithms");
        var globalTokens = BindGlobalTokens(config.Tokens, registry, diagnostics);
        var controls = BindControls(config.Controls, registry, diagnostics);

        if (diagnostics.Count != 0)
        {
            return new ThemeConfigNormalizeResult(null, diagnostics);
        }

        return new ThemeConfigNormalizeResult(
            new NormalizedThemeConfig(
                config.Inherit,
                algorithmsSpecified,
                algorithms,
                globalTokens,
                controls),
            Array.Empty<ThemeDefinitionDiagnostic>());
    }

    private static IReadOnlyList<ThemeAlgorithmDescriptor> BindAlgorithms(
        IReadOnlyList<ThemeAlgorithm>? algorithms,
        ThemeSchemaRegistry registry,
        List<ThemeDefinitionDiagnostic> diagnostics,
        string path)
    {
        if (algorithms is null)
        {
            return Array.Empty<ThemeAlgorithmDescriptor>();
        }

        var descriptors = new List<ThemeAlgorithmDescriptor>(algorithms.Count);
        var seen = new HashSet<ThemeAlgorithm>();
        for (var index = 0; index < algorithms.Count; index++)
        {
            var algorithm = algorithms[index];
            var itemPath = $"{path}[{index}]";
            if (!Enum.IsDefined(algorithm))
            {
                AddError(diagnostics, UnknownAlgorithmCode, itemPath, $"Algorithm value '{(int)algorithm}' is invalid.");
                continue;
            }

            if (!seen.Add(algorithm))
            {
                AddError(diagnostics, UnknownAlgorithmCode, itemPath, $"Algorithm '{algorithm}' is duplicated.");
                continue;
            }

            if (!registry.TryGetAlgorithm(algorithm, out var descriptor))
            {
                AddError(diagnostics, UnknownAlgorithmCode, itemPath, $"Algorithm '{algorithm}' is not registered.");
                continue;
            }

            descriptors.Add(descriptor);
        }

        return descriptors;
    }

    private static IReadOnlyList<ThemeAlgorithmDescriptor> BindDefaultAlgorithm(
        ThemeSchemaRegistry registry,
        List<ThemeDefinitionDiagnostic> diagnostics,
        string path)
    {
        if (!registry.TryGetAlgorithm(ThemeAlgorithm.Default, out var descriptor))
        {
            AddError(
                diagnostics,
                UnknownAlgorithmCode,
                path,
                "Default algorithm is not registered.");
            return Array.Empty<ThemeAlgorithmDescriptor>();
        }

        return [descriptor];
    }

    private static IReadOnlyList<NormalizedTokenValue> BindGlobalTokens(
        IReadOnlyDictionary<string, string>? values,
        ThemeSchemaRegistry registry,
        List<ThemeDefinitionDiagnostic> diagnostics)
    {
        if (values is null)
        {
            AddError(diagnostics, InvalidInputCode, "$.Tokens", "Tokens cannot be null.");
            return Array.Empty<NormalizedTokenValue>();
        }

        var entries = ThemeConfigArray.Copy(values);
        Array.Sort(entries, static (left, right) => string.Compare(left.Key, right.Key, StringComparison.Ordinal));
        var result = new List<NormalizedTokenValue>(entries.Length);
        foreach (var entry in entries)
        {
            var path = $"$.Tokens['{entry.Key}']";
            if (!registry.TryGetGlobalToken(entry.Key, out var descriptor))
            {
                AddError(diagnostics, UnknownGlobalTokenCode, path, $"Global Token '{entry.Key}' is not registered.");
                continue;
            }

            if (TryParseToken(entry.Value, descriptor, diagnostics, path, out var value))
            {
                result.Add(value);
            }
        }

        return result;
    }

    private static IReadOnlyList<NormalizedControlThemeConfig> BindControls(
        IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig>? values,
        ThemeSchemaRegistry registry,
        List<ThemeDefinitionDiagnostic> diagnostics)
    {
        if (values is null)
        {
            AddError(diagnostics, InvalidInputCode, "$.Controls", "Controls cannot be null.");
            return Array.Empty<NormalizedControlThemeConfig>();
        }

        var entries = ThemeConfigArray.Copy(values);
        Array.Sort(entries, static (left, right) =>
        {
            var catalog = string.Compare(left.Key.Catalog, right.Key.Catalog, StringComparison.Ordinal);
            return catalog != 0 ? catalog : string.Compare(left.Key.Id, right.Key.Id, StringComparison.Ordinal);
        });
        var result = new List<NormalizedControlThemeConfig>(entries.Length);
        foreach (var entry in entries)
        {
            var identity = entry.Key;
            var path = $"$.Controls['{identity}']";
            if (!registry.TryGetControl(identity, out var descriptor))
            {
                AddError(diagnostics, UnknownControlCode, path, $"Control '{identity}' is not registered.");
                continue;
            }

            if (entry.Value is null)
            {
                AddError(diagnostics, InvalidInputCode, path, $"Control '{identity}' config cannot be null.");
                continue;
            }

            var config = entry.Value;
            if (!Enum.IsDefined(config.Algorithm))
            {
                AddError(
                    diagnostics,
                    InvalidAlgorithmPolicyCode,
                    $"{path}.Algorithm",
                    $"Control '{identity}' has an invalid algorithm mode.");
                continue;
            }

            var hasCustomAlgorithms = config.Algorithms is { Count: > 0 };
            if (config.Algorithm == ControlAlgorithmMode.Custom != hasCustomAlgorithms)
            {
                AddError(
                    diagnostics,
                    InvalidAlgorithmPolicyCode,
                    $"{path}.Algorithms",
                    $"Control '{identity}' custom algorithms must be present only when Algorithm is Custom.");
            }

            var algorithms = config.Algorithm == ControlAlgorithmMode.Custom
                ? BindAlgorithms(config.Algorithms, registry, diagnostics, $"{path}.Algorithms")
                : Array.Empty<ThemeAlgorithmDescriptor>();
            BindControlTokens(
                config.Tokens,
                descriptor,
                registry,
                diagnostics,
                path,
                out var globalTokens,
                out var ownTokens);
            result.Add(new NormalizedControlThemeConfig(
                ControlTokenIdentity.ForControl(descriptor.ControlType, identity.Catalog, identity.Id),
                config.Algorithm,
                algorithms,
                globalTokens,
                ownTokens));
        }

        return result;
    }

    private static void BindControlTokens(
        IReadOnlyDictionary<string, string>? values,
        ControlTokenDescriptor control,
        ThemeSchemaRegistry registry,
        List<ThemeDefinitionDiagnostic> diagnostics,
        string controlPath,
        out IReadOnlyList<NormalizedTokenValue> globalTokens,
        out IReadOnlyList<NormalizedTokenValue> ownTokens)
    {
        if (values is null)
        {
            AddError(diagnostics, InvalidInputCode, $"{controlPath}.Tokens", "Control Tokens cannot be null.");
            globalTokens = Array.Empty<NormalizedTokenValue>();
            ownTokens = Array.Empty<NormalizedTokenValue>();
            return;
        }

        var globalResult = new List<NormalizedTokenValue>(values.Count);
        var ownResult = new List<NormalizedTokenValue>(values.Count);
        var entries = ThemeConfigArray.Copy(values);
        Array.Sort(entries, static (left, right) => string.Compare(left.Key, right.Key, StringComparison.Ordinal));
        foreach (var entry in entries)
        {
            var path = $"{controlPath}.Tokens['{entry.Key}']";
            var matched = false;
            if (control.TryGetOwnToken(entry.Key, out var ownDescriptor))
            {
                matched = true;
                if (TryParseToken(entry.Value, ownDescriptor, diagnostics, path, out var value))
                {
                    ownResult.Add(value);
                }
            }

            if (!matched && registry.TryGetGlobalToken(entry.Key, out var globalDescriptor))
            {
                matched = true;
                if (TryParseToken(entry.Value, globalDescriptor, diagnostics, path, out var value))
                {
                    globalResult.Add(value);
                }
            }

            if (!matched)
            {
                AddError(
                    diagnostics,
                    UnknownControlTokenCode,
                    path,
                    $"Control Token '{control.Identity}:{entry.Key}' is not registered.");
            }
        }

        globalTokens = globalResult;
        ownTokens = ownResult;
    }

    private static bool TryParseToken(
        string? rawValue,
        TokenDescriptor descriptor,
        List<ThemeDefinitionDiagnostic> diagnostics,
        string path,
        out NormalizedTokenValue value)
    {
        if (rawValue is null)
        {
            AddError(diagnostics, InvalidTokenValueCode, path, $"Token '{descriptor.Name}' value cannot be null.");
            value = null!;
            return false;
        }

        try
        {
            var parsed = descriptor.Parse(rawValue);
            if (!IsCompatibleValue(descriptor.ValueType, parsed))
            {
                throw new InvalidOperationException(
                    "The Token parser returned an incompatible value.");
            }
            value = new NormalizedTokenValue(descriptor, parsed, descriptor.Format(parsed));
            return true;
        }
        catch (Exception exception)
        {
            AddError(
                diagnostics,
                InvalidTokenValueCode,
                path,
                $"Token '{descriptor.Name}' value '{rawValue}' is invalid: {exception.GetBaseException().Message}");
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

    private static void AddError(
        List<ThemeDefinitionDiagnostic> diagnostics,
        string code,
        string path,
        string message)
    {
        diagnostics.Add(new ThemeDefinitionDiagnostic(
            code,
            Definitions.ThemeDefinitionDiagnosticSeverity.Error,
            Source,
            0,
            0,
            path,
            message));
    }
}

internal sealed class ThemeConfigNormalizeResult
{
    internal ThemeConfigNormalizeResult(
        NormalizedThemeConfig? config,
        IEnumerable<ThemeDefinitionDiagnostic> diagnostics)
    {
        Config      = config;
        Diagnostics = Array.AsReadOnly(ThemeConfigArray.Copy(diagnostics));
    }

    public NormalizedThemeConfig? Config { get; }
    public IReadOnlyList<ThemeDefinitionDiagnostic> Diagnostics { get; }
    public bool Success => Config is not null && Diagnostics.Count == 0;
}
