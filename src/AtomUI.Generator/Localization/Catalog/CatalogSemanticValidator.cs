using System.Collections.Immutable;
using AtomUI.Generator.Diagnostics;
using AtomUI.Generator.Localization.Xliff;
using AtomUI.Build.Tasks.LocalizationBuild;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator.Localization.Catalog;

internal static class CatalogSemanticValidator
{
    private const string MinimumTargetState = "translated";

    internal static CatalogValidationResult Validate(CatalogCompilationWorkItem workItem)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var catalog = workItem.Catalog;
        var inputs = OrderInputs(workItem.Inputs);
        var englishSources = inputs.Where(static resolution =>
                                      resolution.Input.SourceKind == LanguageFileSourceKind.ModuleBuiltIn &&
                                      GetLanguage(resolution.Input) == "en-US")
                                  .ToArray();
        if (englishSources.Length != 1)
        {
            diagnostics.Add(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.LocalizationCatalogXliffMismatch,
                catalog.Location,
                catalog.MetadataName,
                catalog.CatalogId,
                englishSources.Length == 0
                    ? "a complete en-US source bundle is required"
                    : "exactly one en-US source bundle is allowed"));
            return new CatalogValidationResult(null, OrderDiagnostics(diagnostics));
        }

        var sourceFile = englishSources[0].Input;
        var authoritativeFingerprint = LanguageSourceFingerprint.Compute(sourceFile.Document);
        var bundleOwners =
            new Dictionary<(string Language, LanguageFileSourceKind SourceKind), LanguageFileInput>();
        var overrideOwners = new Dictionary<(string Language, string Key), LanguageFileInput>();

        foreach (var resolution in inputs)
        {
            var file = resolution.Input;
            var language = GetLanguage(file);
            if (file.SourceKind == LanguageFileSourceKind.ApplicationOverride)
            {
                ValidateOverrideUnitSources(catalog, file, language, overrideOwners, diagnostics);
            }
            else
            {
                var ownerKey = (language, file.SourceKind);
                if (bundleOwners.TryGetValue(ownerKey, out var owner))
                {
                    diagnostics.Add(Mismatch(
                        file,
                        catalog.CatalogId,
                        $"language '{language}' has more than one translation source at the same priority " +
                        $"('{owner.SourceIdentity}' and '{file.SourceIdentity}')"));
                    continue;
                }

                bundleOwners.Add(ownerKey, file);
            }

            ValidateCatalogUnits(
                catalog,
                file,
                requireComplete: file.SourceKind != LanguageFileSourceKind.ApplicationOverride,
                diagnostics);

            if (!ReferenceEquals(file, sourceFile))
            {
                if (language == "en-US")
                {
                    ValidateSourceText(sourceFile, file, catalog, diagnostics);
                }
                else
                {
                    var fileDiagnostics = LanguageFileValidation.ValidateTarget(
                        sourceFile.Document,
                        file.Document,
                        new LanguageFileValidationOptions(
                            requireCompleteBundle:
                            file.SourceKind != LanguageFileSourceKind.ApplicationOverride,
                            MinimumTargetState));
                    diagnostics.AddRange(fileDiagnostics.Select(diagnostic =>
                        LocalizationDiagnosticFactory.FromFileValidation(file, diagnostic)));
                }
            }

            if (file.SourceFingerprint is { } sourceFingerprint &&
                !string.Equals(sourceFingerprint, authoritativeFingerprint, StringComparison.Ordinal))
            {
                diagnostics.Add(Mismatch(
                    file,
                    catalog.CatalogId,
                    $"source fingerprint '{sourceFingerprint}' does not match the authoritative en-US " +
                    $"source fingerprint '{authoritativeFingerprint}'"));
            }
        }

        var orderedDiagnostics = OrderDiagnostics(diagnostics);
        return new CatalogValidationResult(
            orderedDiagnostics.IsEmpty
                ? new ValidatedCatalogInput(workItem, sourceFile)
                : null,
            orderedDiagnostics);
    }

    private static ImmutableArray<LanguageInputResolution> OrderInputs(
        ImmutableArray<LanguageInputResolution> inputs)
    {
        return inputs.OrderBy(static resolution => GetLanguage(resolution.Input), StringComparer.Ordinal)
                     .ThenBy(static resolution => resolution.Input.SourceKind)
                     .ThenBy(static resolution => resolution.Input.SourceIdentity, StringComparer.Ordinal)
                     .ThenBy(static resolution => resolution.Input.Path, StringComparer.Ordinal)
                     .ToImmutableArray();
    }

    private static void ValidateOverrideUnitSources(
        LanguageCatalogInfo catalog,
        LanguageFileInput file,
        string language,
        Dictionary<(string Language, string Key), LanguageFileInput> owners,
        ImmutableArray<Diagnostic>.Builder diagnostics)
    {
        foreach (var unit in file.Document.File.Units.Where(static unit =>
                     !unit.IsObsolete &&
                     XliffTranslationTarget.IsPublishable(unit)))
        {
            var ownerKey = (language, unit.Key);
            if (!owners.TryGetValue(ownerKey, out var owner))
            {
                owners.Add(ownerKey, file);
                continue;
            }

            diagnostics.Add(Mismatch(
                file,
                catalog.CatalogId,
                $"unit Key '{unit.Key}' ('{unit.Name ?? unit.Key}') language '{language}' has more than one " +
                $"translation source at the same priority ('{owner.SourceIdentity}' and " +
                $"'{file.SourceIdentity}')",
                unit.Line,
                unit.Column));
        }
    }

    private static void ValidateCatalogUnits(
        LanguageCatalogInfo catalog,
        LanguageFileInput file,
        bool requireComplete,
        ImmutableArray<Diagnostic>.Builder diagnostics)
    {
        var unitsByKey = file.Document.File.Units
                             .Where(static unit => !unit.IsObsolete)
                             .ToDictionary(static unit => unit.Key, StringComparer.Ordinal);
        foreach (var catalogUnit in catalog.Units)
        {
            if (!unitsByKey.ContainsKey(catalogUnit.Key) && requireComplete)
            {
                diagnostics.Add(Mismatch(
                    file,
                    catalog.CatalogId,
                    $"unit Key '{catalogUnit.Key}' is missing"));
            }
        }

        var catalogKeys = new HashSet<string>(
            catalog.Units.Select(static unit => unit.Key),
            StringComparer.Ordinal);
        foreach (var fileUnit in file.Document.File.Units.Where(static unit => !unit.IsObsolete))
        {
            if (!catalogKeys.Contains(fileUnit.Key))
            {
                diagnostics.Add(Mismatch(
                    file,
                    catalog.CatalogId,
                    $"unit Key '{fileUnit.Key}' is not declared by the Catalog",
                    fileUnit.Line,
                    fileUnit.Column));
            }
        }
    }

    private static void ValidateSourceText(
        LanguageFileInput sourceFile,
        LanguageFileInput file,
        LanguageCatalogInfo catalog,
        ImmutableArray<Diagnostic>.Builder diagnostics)
    {
        var sourceUnits = sourceFile.Document.File.Units
                                     .Where(static unit => !unit.IsObsolete)
                                     .ToDictionary(static unit => unit.Key, StringComparer.Ordinal);
        foreach (var unit in file.Document.File.Units.Where(static unit => !unit.IsObsolete))
        {
            if (sourceUnits.TryGetValue(unit.Key, out var expected) &&
                !string.Equals(unit.Source, expected.Source, StringComparison.Ordinal))
            {
                diagnostics.Add(Mismatch(
                    file,
                    catalog.CatalogId,
                    $"unit Key '{unit.Key}' source text differs from the en-US source bundle",
                    unit.Line,
                    unit.Column));
            }
        }
    }

    private static string GetLanguage(LanguageFileInput file)
    {
        return file.Document.TargetLanguage ?? file.Document.SourceLanguage;
    }

    private static Diagnostic Mismatch(
        LanguageFileInput file,
        string catalogId,
        string reason,
        int line = 1,
        int column = 1)
    {
        return Diagnostic.Create(
            AtomUIDiagnosticDescriptors.LocalizationCatalogXliffMismatch,
            LocalizationDiagnosticFactory.CreateLocation(file.Path, file.Text, line, column),
            file.Path,
            catalogId,
            reason);
    }

    private static ImmutableArray<Diagnostic> OrderDiagnostics(
        IEnumerable<Diagnostic> diagnostics)
    {
        return diagnostics.OrderBy(static diagnostic => diagnostic.Location.GetLineSpan().Path, StringComparer.Ordinal)
                          .ThenBy(static diagnostic =>
                              diagnostic.Location.GetLineSpan().StartLinePosition.Line)
                          .ThenBy(static diagnostic =>
                              diagnostic.Location.GetLineSpan().StartLinePosition.Character)
                          .ThenBy(static diagnostic => diagnostic.Id, StringComparer.Ordinal)
                          .ThenBy(static diagnostic => diagnostic.GetMessage(), StringComparer.Ordinal)
                          .ToImmutableArray();
    }
}
