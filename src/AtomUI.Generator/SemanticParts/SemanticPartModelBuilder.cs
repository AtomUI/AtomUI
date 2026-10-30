using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

internal static class SemanticPartModelBuilder
{
    internal static IReadOnlyList<SemanticControlDeclaration> Build(
        Compilation compilation,
        IEnumerable<SemanticControlDeclaration> declarations,
        IReadOnlyList<ThemeAssetInfo> assets,
        RegistrationModelBuilder resourceFacts,
        Action<Diagnostic> reportDiagnostic)
    {
        var mergedDeclarations = declarations
                                 .GroupBy(static declaration => declaration.ControlType, SymbolEqualityComparer.Default)
                                 .Select(static group => SemanticControlDeclaration.Merge(group))
                                 .OrderBy(
                                     static item => item.ControlType.ToDisplayString(
                                         GeneratorSymbolDisplay.FullyQualifiedType),
                                     StringComparer.Ordinal)
                                 .ToArray();
        if (mergedDeclarations.Length == 0)
        {
            return Array.Empty<SemanticControlDeclaration>();
        }

        var sourceControls = ControlThemeModelBuilder
                             .GetPublicControls(compilation.Assembly.GlobalNamespace)
                             .ToArray();
        var typeResolver = new SemanticPartTypeResolver(compilation, sourceControls);
        var contractValidator = new SemanticPartContractValidator(
            compilation,
            resourceFacts,
            reportDiagnostic);
        var templateValidator = new SemanticPartTemplateValidator(
            assets,
            typeResolver,
            reportDiagnostic);
        var result = new List<SemanticControlDeclaration>();

        foreach (var declaration in mergedDeclarations)
        {
            if (!contractValidator.ValidateControl(declaration))
            {
                continue;
            }

            var validParts = new List<SemanticPartDeclaration>();
            foreach (var part in declaration.Parts)
            {
                if (contractValidator.ValidatePart(declaration, part, out var validatedPart))
                {
                    validParts.Add(validatedPart);
                }
            }

            if (!contractValidator.ValidateUniqueParts(declaration, validParts) ||
                validParts.Count != declaration.Parts.Count)
            {
                continue;
            }

            if (!templateValidator.Validate(declaration, validParts))
            {
                continue;
            }

            result.Add(declaration.WithParts(validParts));
        }

        return new SemanticPartStyleTypeValidator(compilation, reportDiagnostic).Validate(result);
    }
}
