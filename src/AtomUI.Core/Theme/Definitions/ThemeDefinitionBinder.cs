using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Definitions;

/// <summary>
/// Binds a v2 theme document to the registry and runs the semantic checks of spec 7.10. Every diagnostic carries the
/// source, line, column and element path of the offending element.
/// </summary>
internal static class ThemeDefinitionBinder
{
    private const string UnknownLanguageCode = "ATMTHM2001";
    private const string UnknownControlCode = "ATMTHM2002";
    private const string UnknownTokenCode = "ATMTHM2003";
    private const string UnknownLanguageTokenCode = "ATMTHM2004";
    private const string InvalidControlCode = "ATMTHM3001";
    private const string InvalidTokenValueCode = "ATMTHM3002";
    private const string InvalidAxisValueCode = "ATMTHM3003";
    private const string InvalidLockedCode = "ATMTHM3004";
    private const string InvalidConditionCode = "ATMTHM3005";
    private const string StaleDerivedTokenCode = "ATMTHM3006";
    private const string ContradictingConditionCode = "ATMTHM3007";
    private const string DuplicateConditionCode = "ATMTHM3008";

    internal static ThemeDefinitionBindResult Bind(
        ThemeDocument document,
        ThemeSchemaRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(registry);

        var diagnostics = new List<ThemeDefinitionDiagnostic>();
        if (!registry.TryGetLanguage(document.Language, out var language))
        {
            AddError(diagnostics, UnknownLanguageCode, document.Location,
                $"Design language '{document.Language}' is not enabled.");
            return new ThemeDefinitionBindResult(null, diagnostics);
        }

        var style = BindStyle(document.Style, language, document.Location, diagnostics);
        var density = BindDensity(document.Density, language, document.Location, diagnostics);
        var lockedSeeds = new HashSet<string>(StringComparer.Ordinal);
        var values = BindSection(document.Values, language, registry, isCondition: false, lockedSeeds, diagnostics);
        var conditions = new List<ThemeConditionDefinition>(document.Conditions.Count);
        var seenConditions = new List<(DesignStyle?, ThemeAppearance?, DesignDensity?, DesignIdiom?)>();
        foreach (var condition in document.Conditions)
        {
            var bound = BindCondition(condition, language, registry, diagnostics);
            if (bound is null)
            {
                continue;
            }

            var key = (bound.Style, bound.Appearance, bound.Density, bound.Idiom);
            if (seenConditions.Contains(key))
            {
                AddWarning(diagnostics, DuplicateConditionCode, condition.Location,
                    "Another condition section has exactly the same condition; the later one replaces its values.");
            }
            seenConditions.Add(key);

            if ((style is { } fixedStyle && bound.Style is { } conditionStyle && conditionStyle != fixedStyle) ||
                (document.Appearance is { } fixedAppearance && bound.Appearance is { } conditionAppearance &&
                 conditionAppearance != fixedAppearance) ||
                (density is { } fixedDensity && bound.Density is { } conditionDensity && conditionDensity != fixedDensity))
            {
                AddWarning(diagnostics, ContradictingConditionCode, condition.Location,
                    "The condition contradicts an axis the theme fixes; the section applies only where a scope or a control uses another value explicitly.");
            }

            conditions.Add(bound);
        }

        ReportStaleDerivedTokens(document, language, lockedSeeds, diagnostics);
        if (diagnostics.Any(static diagnostic => diagnostic.Severity == ThemeDefinitionDiagnosticSeverity.Error))
        {
            return new ThemeDefinitionBindResult(null, diagnostics);
        }

        return new ThemeDefinitionBindResult(
            new PreparedThemeDefinition(
                document.Id,
                document.Name,
                language,
                style,
                document.Appearance,
                density,
                lockedSeeds,
                values,
                conditions,
                document.Location),
            diagnostics);
    }

    private static ThemeConditionDefinition? BindCondition(ThemeConditionDocument condition,
        DesignLanguageDescriptor language, ThemeSchemaRegistry registry, List<ThemeDefinitionDiagnostic> diagnostics)
    {
        if (!condition.HasCondition)
        {
            AddError(diagnostics, InvalidConditionCode, condition.Location,
                "A condition section must declare at least one of Style, Appearance, Density or Idiom.");
            return null;
        }
        if (condition.Values.IsEmpty)
        {
            AddError(diagnostics, InvalidConditionCode, condition.Location,
                "A condition section must contain Tokens, LanguageTokens or Controls.");
            return null;
        }

        var errors = diagnostics.Count;
        var style = BindStyle(condition.Style, language, condition.Location, diagnostics);
        var density = BindDensity(condition.Density, language, condition.Location, diagnostics);
        DesignIdiom? idiom = null;
        if (condition.Idiom is { } idiomId)
        {
            var candidate = new DesignIdiom(idiomId);
            if (language.Axes.Idioms.Contains(candidate))
            {
                idiom = candidate;
            }
            else
            {
                AddError(diagnostics, InvalidAxisValueCode, condition.Location,
                    $"Design language '{language.Id}' does not declare the idiom '{idiomId}'.");
            }
        }

        var values = BindSection(condition.Values, language, registry, isCondition: true, null, diagnostics);
        return diagnostics.Count != errors && diagnostics.Skip(errors).Any(static d => d.Severity == ThemeDefinitionDiagnosticSeverity.Error)
            ? null
            : new ThemeConditionDefinition(style, condition.Appearance, density, idiom, values, condition.Location);
    }

    private static DesignStyle? BindStyle(string? value, DesignLanguageDescriptor language, ThemeSourceLocation location,
        List<ThemeDefinitionDiagnostic> diagnostics)
    {
        if (value is null)
        {
            return null;
        }

        var style = new DesignStyle(value);
        if (!language.Axes.Styles.Contains(style))
        {
            AddError(diagnostics, InvalidAxisValueCode, location,
                $"Design language '{language.Id}' does not declare the style '{value}'.");
            return null;
        }

        return style;
    }

    private static DesignDensity? BindDensity(string? value, DesignLanguageDescriptor language,
        ThemeSourceLocation location, List<ThemeDefinitionDiagnostic> diagnostics)
    {
        if (value is null)
        {
            return null;
        }

        var density = new DesignDensity(value);
        if (!language.Axes.Densities.Contains(density))
        {
            AddError(diagnostics, InvalidAxisValueCode, location,
                $"Design language '{language.Id}' does not declare the density '{value}'.");
            return null;
        }

        return density;
    }

    private static ThemeSectionDefinition BindSection(
        ThemeSectionDocument section,
        DesignLanguageDescriptor language,
        ThemeSchemaRegistry registry,
        bool isCondition,
        HashSet<string>? lockedSeeds,
        List<ThemeDefinitionDiagnostic> diagnostics)
    {
        var tokens = new List<BoundTokenValue>(section.Tokens.Count);
        foreach (var document in section.Tokens)
        {
            if (!registry.TryGetGlobalToken(document.Name, out var descriptor))
            {
                AddError(diagnostics, UnknownTokenCode, document.Location,
                    $"Global Token '{document.Name}' is not registered.");
                continue;
            }

            ValidateLocked(document, descriptor, language, isCondition, lockedSeeds, diagnostics);
            if (TryBindToken(document, descriptor, diagnostics, out var value))
            {
                tokens.Add(value);
            }
        }

        var languageTokens = new List<BoundTokenValue>(section.LanguageTokens.Count);
        foreach (var document in section.LanguageTokens)
        {
            if (!language.TryGetLanguageToken(document.Name, out var descriptor))
            {
                AddError(diagnostics, UnknownLanguageTokenCode, document.Location,
                    $"'{document.Name}' is not a language Token of '{language.Id}'.");
                continue;
            }

            ValidateLocked(document, descriptor, language, isCondition, lockedSeeds, diagnostics);
            if (TryBindToken(document, descriptor, diagnostics, out var value))
            {
                languageTokens.Add(value);
            }
        }

        var controls = BindControls(section.Controls, language, registry, isCondition, diagnostics);
        return new ThemeSectionDefinition(tokens, languageTokens, controls);
    }

    private static void ValidateLocked(ThemeTokenDocument document, TokenDescriptor descriptor,
        DesignLanguageDescriptor language, bool isCondition, HashSet<string>? lockedSeeds,
        List<ThemeDefinitionDiagnostic> diagnostics)
    {
        if (!document.Locked)
        {
            return;
        }

        if (isCondition || lockedSeeds is null)
        {
            AddError(diagnostics, InvalidLockedCode, document.Location, "Locked can only be written in the base section.");
            return;
        }

        if (descriptor.Stage != TokenStage.Seed ||
            !language.Axes.AdjustableSeeds.Any(seed => string.Equals(seed.Name, descriptor.Name, StringComparison.Ordinal)))
        {
            AddError(diagnostics, InvalidLockedCode, document.Location,
                $"Locked applies only to an adjustable seed of '{language.Id}'; '{document.Name}' is not one.");
            return;
        }

        lockedSeeds.Add(descriptor.Name);
    }

    private static IReadOnlyList<ControlThemeDefinition> BindControls(
        IReadOnlyList<ControlThemeDocument> documents,
        DesignLanguageDescriptor language,
        ThemeSchemaRegistry registry,
        bool isCondition,
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
                AddError(diagnostics, UnknownControlCode, document.Location, $"Control '{identity}' is not registered.");
                continue;
            }

            if (isCondition && (document.Appearance is not null || document.Density is not null))
            {
                AddError(diagnostics, InvalidControlCode, document.Location,
                    $"Control '{identity}' cannot fix Appearance or Density inside a condition section.");
                continue;
            }

            if (document.Appearance is null && document.Density is null && document.Tokens.Count == 0)
            {
                AddError(diagnostics, InvalidControlCode, document.Location,
                    $"Control '{identity}' must fix an axis or declare at least one Token.");
                continue;
            }

            var density = BindDensity(document.Density, language, document.Location, diagnostics);
            var globalTokens = new List<BoundTokenValue>(document.Tokens.Count);
            var ownTokens = new List<BoundTokenValue>(document.Tokens.Count);
            foreach (var token in document.Tokens)
            {
                if (token.Locked)
                {
                    AddError(diagnostics, InvalidLockedCode, token.Location, "Locked cannot be written on a Control Token.");
                }

                if (descriptor.TryGetOwnToken(token.Name, out var ownDescriptor))
                {
                    if (TryBindToken(token, ownDescriptor, diagnostics, out var value))
                    {
                        ownTokens.Add(value);
                    }
                }
                else if (registry.TryGetGlobalToken(token.Name, out var globalDescriptor))
                {
                    if (TryBindToken(token, globalDescriptor, diagnostics, out var value))
                    {
                        globalTokens.Add(value);
                    }
                }
                else
                {
                    AddError(diagnostics, UnknownTokenCode, token.Location,
                        $"Token '{token.Name}' is not registered for Control '{identity}'.");
                }
            }

            controls.Add(new ControlThemeDefinition(descriptor, document.Appearance, density, globalTokens, ownTokens,
                document.Location));
        }

        return controls;
    }

    // Spec 7.4: a theme that leaves an adjustable seed unlocked but writes a token derived from it gets a warning; the
    // value stays fixed when the user changes the seed. The derivation sets are declared by the language.
    private static void ReportStaleDerivedTokens(ThemeDocument document, DesignLanguageDescriptor language,
        IReadOnlySet<string> lockedSeeds, List<ThemeDefinitionDiagnostic> diagnostics)
    {
        if (language.Axes.SeedDerivations.Count == 0)
        {
            return;
        }

        foreach (var (seed, derived) in language.Axes.SeedDerivations)
        {
            if (lockedSeeds.Contains(seed))
            {
                continue;
            }

            foreach (var token in document.Tokens.Concat(document.Controls.SelectMany(static control => control.Tokens)))
            {
                if (derived.Contains(token.Name))
                {
                    AddWarning(diagnostics, StaleDerivedTokenCode, token.Location,
                        $"Token '{token.Name}' is derived from the adjustable seed '{seed}', which the theme does not lock; " +
                        "it keeps this value when the user changes the seed.");
                }
            }
        }
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
            AddError(diagnostics, InvalidTokenValueCode, document.Location,
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

    private static void AddError(List<ThemeDefinitionDiagnostic> diagnostics, string code, ThemeSourceLocation location,
        string message) =>
        diagnostics.Add(new ThemeDefinitionDiagnostic(code, ThemeDefinitionDiagnosticSeverity.Error, location.Source,
            location.Line, location.Column, location.Path, message));

    private static void AddWarning(List<ThemeDefinitionDiagnostic> diagnostics, string code, ThemeSourceLocation location,
        string message) =>
        diagnostics.Add(new ThemeDefinitionDiagnostic(code, ThemeDefinitionDiagnosticSeverity.Warning, location.Source,
            location.Line, location.Column, location.Path, message));
}
