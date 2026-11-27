using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

internal sealed class SemanticPartContractValidator
{
    private const string AvaloniaControlType = "Avalonia.Controls.Control";
    private const string AvaloniaStyledElementType = "Avalonia.StyledElement";
    private const string AvaloniaControlThemeType = "global::Avalonia.Styling.ControlTheme";

    private readonly Compilation _compilation;
    private readonly RegistrationModelBuilder _resourceFacts;
    private readonly Action<Diagnostic> _reportDiagnostic;

    internal SemanticPartContractValidator(
        Compilation compilation,
        RegistrationModelBuilder resourceFacts,
        Action<Diagnostic> reportDiagnostic)
    {
        _compilation = compilation;
        _resourceFacts = resourceFacts;
        _reportDiagnostic = reportDiagnostic;
    }

    internal bool ValidateControl(SemanticControlDeclaration declaration)
    {
        var controlBase = _compilation.GetTypeByMetadataName(AvaloniaControlType);
        if (declaration.ControlType.DeclaredAccessibility == Accessibility.Public &&
            declaration.ControlType.Arity == 0 &&
            controlBase is not null &&
            SemanticPartTypeResolver.IsAssignableTo(declaration.ControlType, controlBase))
        {
            return true;
        }

        _reportDiagnostic(Diagnostic.Create(
            AtomUIDiagnosticDescriptors.SemanticPartInvalidDeclaration,
            declaration.Location,
            "<control>",
            declaration.ControlType.ToDisplayString(),
            "the target must be a public, non-generic Avalonia Control"));
        return false;
    }

    internal bool ValidatePart(
        SemanticControlDeclaration control,
        SemanticPartDeclaration part,
        out SemanticPartDeclaration validatedPart)
    {
        validatedPart = part;
        var valid = true;
        if (!IsPartPath(part.Name) || string.Equals(part.Name, "root", StringComparison.Ordinal))
        {
            ReportInvalidDeclaration(control, part, "name must be dot-separated camelCase segments and cannot be root");
            valid = false;
        }
        if (!IsPartPath(part.Path))
        {
            ReportInvalidDeclaration(control, part, "path must contain camelCase segments");
            valid = false;
        }
        else if (string.Equals(part.Path, "root", StringComparison.Ordinal))
        {
            ReportInvalidDeclaration(control, part, "path cannot be root");
            valid = false;
        }
        if (!IsSemanticSelectorClass(part.SelectorClass))
        {
            ReportInvalidDeclaration(
                control,
                part,
                "SelectorClass must use the semantic-* kebab-case namespace");
            valid = false;
        }
        else if (!TryNormalizeSelectorRoute(part, out var selectorRoute))
        {
            ReportInvalidDeclaration(
                control,
                part,
                "SelectorRoute must use owner-relative '/template/ .semantic-*' or '> .semantic-*' steps and end with SelectorClass");
            valid = false;
        }
        else
        {
            validatedPart = part.WithSelectorRoute(selectorRoute);
        }
        if (part.Cardinality is < 0 or > 2)
        {
            ReportInvalidDeclaration(control, part, "Cardinality is outside the supported range");
            valid = false;
        }
        if (part.Customization is < 1 or > 2)
        {
            ReportInvalidDeclaration(
                control,
                part,
                "only Selector or SelectorAndTheme customization is valid for declared Parts");
            valid = false;
        }

        var styledElement = _compilation.GetTypeByMetadataName(AvaloniaStyledElementType);
        if (part.ContractType is not INamedTypeSymbol contractType ||
            contractType.DeclaredAccessibility != Accessibility.Public ||
            styledElement is null ||
            !SemanticPartTypeResolver.IsAssignableTo(contractType, styledElement))
        {
            _reportDiagnostic(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.SemanticPartInvalidContractType,
                part.Location,
                part.Name,
                control.ControlType.ToDisplayString()));
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(part.Since))
        {
            _reportDiagnostic(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.SemanticPartMissingSince,
                part.Location,
                part.Name,
                control.ControlType.ToDisplayString()));
        }
        else if (!IsReleaseVersion(part.Since!))
        {
            _reportDiagnostic(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.SemanticPartInvalidSince,
                part.Location,
                part.Name,
                control.ControlType.ToDisplayString(),
                part.Since));
            valid = false;
        }

        if (!ValidateThemeContract(control, part, out var themeTargetType))
        {
            valid = false;
        }

        validatedPart = validatedPart.WithThemeTargetType(themeTargetType);
        return valid;
    }

    /// <summary>
    /// Checks whether <c>Since</c> names a concrete release version that a user can cite: <c>major.minor.patch</c>,
    /// three non-negative decimal numbers (for example <c>6.2.0</c>). A version line alone (<c>6.0</c>, <c>6.2</c>)
    /// would make the descriptor and the control documentation claim an introduction version that does not exist.
    /// A prerelease suffix, a leading <c>v</c> or any other string is not a comparable release version either. The
    /// text is parsed character by character instead of with a regular expression, in line with the existing
    /// convention that the Generator takes no dependency on regular expressions.
    /// </summary>
    internal static bool IsReleaseVersion(string since)
    {
        var partIndex = 0;
        for (var segment = 0; segment < 3; segment++)
        {
            var digitStart = partIndex;
            while (partIndex < since.Length && since[partIndex] >= '0' && since[partIndex] <= '9')
            {
                partIndex++;
            }

            if (partIndex == digitStart)
            {
                return false;
            }

            // A numeric part longer than five digits is almost certainly a mistake, such as a timestamp or an internal
            // build number. Reject it so that it does not pollute the compatibility contract.
            if (partIndex - digitStart > 5)
            {
                return false;
            }

            if (segment < 2)
            {
                if (partIndex >= since.Length || since[partIndex] != '.')
                {
                    return false;
                }

                partIndex++;
            }
        }

        return partIndex == since.Length;
    }

    internal bool ValidateUniqueParts(
        SemanticControlDeclaration control,
        IReadOnlyList<SemanticPartDeclaration> parts)
    {
        var valid = true;
        valid &= ValidateUnique(control, parts, static part => part.Name, "name");
        valid &= ValidateUnique(control, parts, static part => part.Path, "path");
        // The same terminal marker can be shared by an unqualified part and a qualified one (such as source.header).
        // What has to be kept unique is the fully identical resolution route.
        valid &= ValidateUnique(
            control,
            parts,
            static part => part.SelectorRoute ?? string.Empty,
            "selector route");
        return valid;
    }

    private bool ValidateThemeContract(
        SemanticControlDeclaration control,
        SemanticPartDeclaration part,
        out ITypeSymbol? themeTargetType)
    {
        themeTargetType = null;
        if (part.Customization != 2)
        {
            if (string.IsNullOrWhiteSpace(part.ThemePropertyName))
            {
                return true;
            }

            ReportInvalidTheme(control, part, "ThemePropertyName is only valid with SelectorAndTheme");
            return false;
        }

        var controlBase = _compilation.GetTypeByMetadataName(AvaloniaControlType);
        if (part.ContractType is not INamedTypeSymbol contractType ||
            controlBase is null ||
            !SemanticPartTypeResolver.IsAssignableTo(contractType, controlBase))
        {
            ReportInvalidTheme(control, part, "ContractType must be a public Control");
            return false;
        }
        if (string.IsNullOrWhiteSpace(part.ThemePropertyName))
        {
            ReportInvalidTheme(control, part, "ThemePropertyName is required");
            return false;
        }

        var property = SemanticThemePropertyResolver.FindInstanceProperty(control.ControlType, part.ThemePropertyName!);
        if (property is null ||
            property.DeclaredAccessibility != Accessibility.Public ||
            property.GetMethod is null ||
            property.GetMethod.DeclaredAccessibility != Accessibility.Public ||
            property.SetMethod is null ||
            property.SetMethod.DeclaredAccessibility != Accessibility.Public ||
            property.SetMethod.IsInitOnly ||
            !string.Equals(
                property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                AvaloniaControlThemeType,
                StringComparison.Ordinal))
        {
            ReportInvalidTheme(
                control,
                part,
                $"public property '{part.ThemePropertyName}' must have type Avalonia.Styling.ControlTheme and a public getter and setter");
            return false;
        }

        return ValidateThemeAssetTargets(control, part, contractType, out themeTargetType);
    }

    private bool ValidateThemeAssetTargets(
        SemanticControlDeclaration control,
        SemanticPartDeclaration part,
        INamedTypeSymbol contractType,
        out ITypeSymbol? themeTargetType)
    {
        themeTargetType = null;
        INamedTypeSymbol? sharedTarget = null;
        var valid = true;
        foreach (var asset in _resourceFacts.SemanticAssignments(control.ControlType, part.ThemePropertyName!))
        {
            var target = asset.Target;
            if (target is null || !SemanticPartTypeResolver.IsAssignableTo(target, contractType))
            {
                ReportInvalidTheme(
                    control,
                    part,
                    $"theme asset '{asset.AssetPath}' TargetType must be assignable to '{contractType.ToDisplayString()}'");
                valid = false;
                continue;
            }

            if (sharedTarget is not null && !SymbolEqualityComparer.Default.Equals(sharedTarget, target))
            {
                ReportInvalidTheme(
                    control,
                    part,
                    $"theme assets must use one TargetType; '{asset.AssetPath}' targets '{target.ToDisplayString()}' instead of '{sharedTarget.ToDisplayString()}'");
                valid = false;
                continue;
            }

            sharedTarget = target;
        }

        if (valid && sharedTarget is not null)
        {
            themeTargetType = sharedTarget;
        }
        return valid;
    }

    private bool ValidateUnique(
        SemanticControlDeclaration control,
        IEnumerable<SemanticPartDeclaration> parts,
        Func<SemanticPartDeclaration, string> keySelector,
        string kind)
    {
        var valid = true;
        foreach (var group in parts.GroupBy(keySelector, StringComparer.Ordinal).Where(static group => group.Count() > 1))
        {
            foreach (var duplicate in group.Skip(1))
            {
                _reportDiagnostic(Diagnostic.Create(
                    AtomUIDiagnosticDescriptors.SemanticPartDuplicateDeclaration,
                    duplicate.Location,
                    duplicate.Name,
                    control.ControlType.ToDisplayString(),
                    kind,
                    group.Key));
            }
            valid = false;
        }
        return valid;
    }

    private static bool IsPartPath(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Split('.').All(IsCamelCaseSegment);
    }

    private static bool IsCamelCaseSegment(string value)
    {
        return value.Length != 0 &&
               value[0] is >= 'a' and <= 'z' &&
               value.Skip(1).All(static character =>
                   character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9');
    }

    private static bool IsSemanticSelectorClass(string? value)
    {
        const string prefix = "semantic-";
        if (string.IsNullOrWhiteSpace(value) ||
            !value!.StartsWith(prefix, StringComparison.Ordinal) ||
            value.Length == prefix.Length ||
            string.Equals(value, "semantic-root", StringComparison.Ordinal))
        {
            return false;
        }

        var previousWasHyphen = false;
        for (var index = prefix.Length; index < value.Length; index++)
        {
            var character = value[index];
            if (character == '-')
            {
                if (previousWasHyphen || index == value.Length - 1)
                {
                    return false;
                }
                previousWasHyphen = true;
                continue;
            }
            if (character is not (>= 'a' and <= 'z') and not (>= '0' and <= '9'))
            {
                return false;
            }
            previousWasHyphen = false;
        }
        return true;
    }

    private static bool TryNormalizeSelectorRoute(
        SemanticPartDeclaration part,
        out string selectorRoute)
    {
        selectorRoute = string.Empty;
        if (part.SelectorClass is null)
        {
            return false;
        }

        if (part.SelectorRoute is null)
        {
            if (part.RuntimeCreated)
            {
                return false;
            }

            selectorRoute = $"/template/ .{part.SelectorClass}";
            return true;
        }

        selectorRoute = part.SelectorRoute;
        if (string.IsNullOrWhiteSpace(selectorRoute) ||
            !string.Equals(selectorRoute, selectorRoute.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        var tokens = selectorRoute.Split(' ');
        if (tokens.Length < 2)
        {
            return false;
        }

        // Optional leading self-anchor: a filter class that the owner node itself carries (such as a direction-specific
        // source-item). When it is present the token count is odd; the remaining tokens are still parsed as
        // step/class pairs.
        var index = 0;
        if (tokens[0].StartsWith(".", StringComparison.Ordinal))
        {
            if (!IsRouteClassToken(tokens[0]))
            {
                return false;
            }

            index = 1;
        }

        if ((tokens.Length - index) < 2 || (tokens.Length - index) % 2 != 0)
        {
            return false;
        }

        for (; index < tokens.Length; index += 2)
        {
            if (tokens[index] is not ("/template/" or ">" or ">>"))
            {
                return false;
            }

            if (!IsRouteClassToken(tokens[index + 1]))
            {
                return false;
            }
        }

        return string.Equals(tokens[tokens.Length - 1], $".{part.SelectorClass}", StringComparison.Ordinal);
    }

    private static bool IsRouteClassToken(string token)
    {
        if (token.Length < 2 || token[0] != '.')
        {
            return false;
        }

        return IsSemanticSelectorClass(token.Substring(1));
    }

    private void ReportInvalidDeclaration(
        SemanticControlDeclaration control,
        SemanticPartDeclaration part,
        string reason)
    {
        _reportDiagnostic(Diagnostic.Create(
            AtomUIDiagnosticDescriptors.SemanticPartInvalidDeclaration,
            part.Location,
            part.Name,
            control.ControlType.ToDisplayString(),
            reason));
    }

    private void ReportInvalidTheme(
        SemanticControlDeclaration control,
        SemanticPartDeclaration part,
        string reason)
    {
        _reportDiagnostic(Diagnostic.Create(
            AtomUIDiagnosticDescriptors.SemanticPartInvalidThemeContract,
            part.Location,
            part.Name,
            control.ControlType.ToDisplayString(),
            reason));
    }
}
