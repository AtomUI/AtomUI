using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

internal sealed class SemanticPartStyleTypeValidator
{
    private readonly Compilation _compilation;
    private readonly Action<Diagnostic> _reportDiagnostic;

    internal SemanticPartStyleTypeValidator(
        Compilation compilation,
        Action<Diagnostic> reportDiagnostic)
    {
        _compilation = compilation;
        _reportDiagnostic = reportDiagnostic;
    }

    internal IReadOnlyList<SemanticControlDeclaration> Validate(
        IReadOnlyList<SemanticControlDeclaration> controls)
    {
        var candidates = controls.SelectMany(control => control.Parts.Select(part => new StyleCandidate(
                                     control,
                                     part,
                                     SemanticPartStyleContract.GetMetadataName(control, part))))
                                 .OrderBy(static candidate => candidate.MetadataName, StringComparer.Ordinal)
                                 .ThenBy(
                                     static candidate => candidate.Control.ControlType.ToDisplayString(
                                         GeneratorSymbolDisplay.FullyQualifiedType),
                                     StringComparer.Ordinal)
                                 .ThenBy(static candidate => candidate.Part.Path, StringComparer.Ordinal)
                                 .ToArray();
        var invalidControls = new HashSet<SemanticControlDeclaration>();
        var canonicalStyleAssemblies = _compilation.SourceModule.ReferencedAssemblySymbols
                                                   .Where(SemanticPartStyleContract.HasCanonicalXmlnsDefinition)
                                                   .OrderBy(
                                                       static assembly => assembly.Identity.Name,
                                                       StringComparer.Ordinal)
                                                   .ToArray();
        var generatedConflicts = candidates.GroupBy(
                                                static candidate => candidate.MetadataName,
                                                StringComparer.Ordinal)
                                           .Where(static group => group.Count() > 1)
                                           .ToArray();

        foreach (var group in generatedConflicts)
        {
            foreach (var candidate in group)
            {
                ReportConflict(candidate, "another generated Semantic Part");
                invalidControls.Add(candidate.Control);
            }
        }

        foreach (var candidate in candidates.Where(candidate => !invalidControls.Contains(candidate.Control)))
        {
            if (_compilation.Assembly.GetTypeByMetadataName(candidate.MetadataName) is { } sourceType)
            {
                ReportConflict(
                    candidate,
                    $"existing source type '{sourceType.ToDisplayString()}'");
                invalidControls.Add(candidate.Control);
                continue;
            }

            foreach (var assembly in canonicalStyleAssemblies)
            {
                var referencedType = assembly.GetTypeByMetadataName(candidate.MetadataName);
                if (referencedType is null || referencedType.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                ReportConflict(
                    candidate,
                    $"public type '{candidate.MetadataName}' exported by referenced assembly '{assembly.Identity.Name}'");
                invalidControls.Add(candidate.Control);
                break;
            }
        }

        return controls.Where(control => !invalidControls.Contains(control)).ToArray();
    }

    private void ReportConflict(StyleCandidate candidate, string conflictingIdentity)
    {
        _reportDiagnostic(Diagnostic.Create(
            AtomUIDiagnosticDescriptors.SemanticPartStyleTypeConflict,
            candidate.Part.Location,
            candidate.Part.Name,
            candidate.Control.ControlType.ToDisplayString(),
            candidate.MetadataName,
            conflictingIdentity));
    }

    private sealed class StyleCandidate
    {
        internal StyleCandidate(
            SemanticControlDeclaration control,
            SemanticPartDeclaration part,
            string metadataName)
        {
            Control = control;
            Part = part;
            MetadataName = metadataName;
        }

        internal SemanticControlDeclaration Control { get; }
        internal SemanticPartDeclaration Part { get; }
        internal string MetadataName { get; }
    }
}
