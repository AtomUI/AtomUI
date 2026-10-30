using AtomUI.Build.Tasks.LocalizationBuild;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

internal static class LanguagePackagePreparationCore
{
    private const string InvalidXliffCode = "ATOMUILOC005";
    private const string CatalogMismatchCode = "ATOMUILOC006";
    private const string InvalidTranslationCode = "ATOMUILOC007";
    private const string InvalidPackageCode = "ATOMUILOC009";

    internal static LanguagePackagePreparationResult? Prepare(
        string packageId,
        ITaskItem[] sourceLanguageFiles,
        ITaskItem[] languageFiles,
        string? expectedLanguage,
        string minimumTargetState,
        bool requireVerifiedContract,
        Action<string, string, int, int, string> logError,
        Action<string, string, int, int, string> logWarning)
    {
        var succeeded = true;
        var targetModuleIds = languageFiles
            .Select(static item => item.GetMetadata("AtomUILanguageModuleId").Trim())
            .ToArray();
        foreach (var moduleId in targetModuleIds)
        {
            if (moduleId.Length == 0)
            {
                logError(
                    InvalidPackageCode,
                    string.Empty,
                    1,
                    1,
                    "AtomUILanguageModuleId is required.");
                succeeded = false;
            }
        }

        var distinctModuleIds = targetModuleIds
            .Where(static moduleId => moduleId.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (distinctModuleIds.Length > 1)
        {
            logError(
                InvalidPackageCode,
                string.Empty,
                1,
                1,
                "A module language package must target exactly one AtomUILanguageModuleId.");
            succeeded = false;
        }

        var targetModuleId = distinctModuleIds.Length == 1 ? distinctModuleIds[0] : string.Empty;
        var targets = ParseTargetFiles(
            languageFiles,
            logError,
            ref succeeded,
            out var language);
        if (!succeeded || targets.Count == 0 || targetModuleId.Length == 0 || language is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(expectedLanguage))
        {
            var expectedText = expectedLanguage!.Trim();
            if (!Bcp47LanguageTagParser.TryParse(expectedText, out var canonicalLanguage) ||
                !string.Equals(expectedText, canonicalLanguage, StringComparison.Ordinal))
            {
                logError(
                    InvalidPackageCode,
                    string.Empty,
                    1,
                    1,
                    "ExpectedLanguage must be a canonical BCP 47 language tag.");
                return null;
            }
            if (!string.Equals(language, canonicalLanguage, StringComparison.Ordinal))
            {
                logError(
                    InvalidPackageCode,
                    string.Empty,
                    1,
                    1,
                    $"The package target language is '{language}', but ExpectedLanguage is '{canonicalLanguage}'.");
                return null;
            }
        }

        var relevantSourceFiles = sourceLanguageFiles
            .Where(item => string.Equals(
                item.GetMetadata("AtomUILanguageModuleId").Trim(),
                targetModuleId,
                StringComparison.Ordinal))
            .ToArray();
        var entries = relevantSourceFiles.Length == 0
            ? PrepareDeferredEntries(
                packageId,
                targetModuleId,
                targets,
                language,
                minimumTargetState,
                requireVerifiedContract,
                logError,
                logWarning)
            : PrepareVerifiedEntries(
                packageId,
                targetModuleId,
                targets,
                relevantSourceFiles,
                minimumTargetState,
                logError);
        if (entries is null)
        {
            return null;
        }

        return new LanguagePackagePreparationResult(
            language,
            entries,
            languageFiles.ToArray());
    }

    private static List<ParsedTargetLanguageFile> ParseTargetFiles(
        IReadOnlyList<ITaskItem> languageFiles,
        Action<string, string, int, int, string> logError,
        ref bool succeeded,
        out string? language)
    {
        language = null;
        var targets = new List<ParsedTargetLanguageFile>(languageFiles.Count);
        foreach (var item in languageFiles)
        {
            var document = ParseDocument(item.ItemSpec, logError, ref succeeded);
            if (document is null)
            {
                continue;
            }
            if (document.TargetLanguage is null)
            {
                logError(
                    InvalidXliffCode,
                    item.ItemSpec,
                    1,
                    1,
                    "A static language package cannot contain an en-US source document.");
                succeeded = false;
                continue;
            }
            if (language is null)
            {
                language = document.TargetLanguage;
            }
            else if (!string.Equals(language, document.TargetLanguage, StringComparison.Ordinal))
            {
                logError(
                    InvalidPackageCode,
                    item.ItemSpec,
                    1,
                    1,
                    "A static language package must contain exactly one target language.");
                succeeded = false;
            }

            if (!LanguagePackagePath.TryNormalize(
                    item.GetMetadata("AtomUILanguagePackagePath"),
                    out var packagePath))
            {
                logError(
                    InvalidPackageCode,
                    item.ItemSpec,
                    1,
                    1,
                    "AtomUILanguagePackagePath must be a normalized relative path.");
                succeeded = false;
                continue;
            }
            targets.Add(new ParsedTargetLanguageFile(item, document, packagePath));
        }
        return targets;
    }

    private static IReadOnlyList<LanguagePackageCatalogEntry>? PrepareDeferredEntries(
        string packageId,
        string moduleId,
        IReadOnlyList<ParsedTargetLanguageFile> targets,
        string language,
        string minimumTargetState,
        bool requireVerifiedContract,
        Action<string, string, int, int, string> logError,
        Action<string, string, int, int, string> logWarning)
    {
        if (requireVerifiedContract)
        {
            logError(
                InvalidPackageCode,
                string.Empty,
                1,
                1,
                $"Language package '{packageId}' requires a verified language contract for module " +
                $"'{moduleId}', but no authoritative en-US source assets were found.");
            return null;
        }

        var entries = new List<LanguagePackageCatalogEntry>(targets.Count);
        var succeeded = true;
        foreach (var target in targets)
        {
            succeeded &= ValidateTarget(
                target,
                source: null,
                minimumTargetState,
                logError);
            var sourceFingerprint = LanguageSourceFingerprint.Compute(target.Document);
            EnrichPreparedItem(
                target.Item,
                packageId,
                LanguagePackageContractValidation.Deferred,
                target.PackagePath,
                sourceFingerprint);
            entries.Add(new LanguagePackageCatalogEntry(
                moduleId,
                target.Document.File.Id,
                LanguagePackageContractValidation.Deferred,
                target.PackagePath,
                sourceFingerprint));
        }

        if (!succeeded)
        {
            return null;
        }

        logWarning(
            "ATOMUILOC010",
            string.Empty,
            1,
            1,
            $"Language package '{packageId}' is being packed without the authoritative en-US contract " +
            $"for module '{moduleId}'. Contract validation is deferred to consuming applications. Add an " +
            "authoring-only component PackageReference with PrivateAssets=all to enable verified validation.");
        return entries;
    }

    private static IReadOnlyList<LanguagePackageCatalogEntry>? PrepareVerifiedEntries(
        string packageId,
        string moduleId,
        IReadOnlyList<ParsedTargetLanguageFile> targets,
        IReadOnlyList<ITaskItem> sourceItems,
        string minimumTargetState,
        Action<string, string, int, int, string> logError)
    {
        var sources = new Dictionary<string, ParsedSourceLanguageFile>(StringComparer.Ordinal);
        foreach (var item in sourceItems)
        {
            var sourceDocumentIsValid = true;
            var document = ParseDocument(item.ItemSpec, logError, ref sourceDocumentIsValid);
            if (document is null)
            {
                return null;
            }
            if (document.TargetLanguage is not null)
            {
                logError(
                    CatalogMismatchCode,
                    item.ItemSpec,
                    1,
                    1,
                    "An authoritative language contract must be an en-US source XLIFF.");
                return null;
            }
            if (sources.ContainsKey(document.File.Id))
            {
                logError(
                    CatalogMismatchCode,
                    item.ItemSpec,
                    1,
                    1,
                    $"Authoritative Catalog '{document.File.Id}' is declared more than once.");
                return null;
            }
            sources.Add(
                document.File.Id,
                new ParsedSourceLanguageFile(item, document));
        }

        var targetsByCatalog = new Dictionary<string, ParsedTargetLanguageFile>(StringComparer.Ordinal);
        foreach (var target in targets)
        {
            if (targetsByCatalog.ContainsKey(target.Document.File.Id))
            {
                logError(
                    CatalogMismatchCode,
                    target.Item.ItemSpec,
                    1,
                    1,
                    $"Target Catalog '{target.Document.File.Id}' is declared more than once in language package " +
                    $"'{packageId}'.");
                return null;
            }
            targetsByCatalog.Add(target.Document.File.Id, target);
        }

        foreach (var source in sources.Values)
        {
            if (!targetsByCatalog.ContainsKey(source.Document.File.Id))
            {
                logError(
                    CatalogMismatchCode,
                    source.Item.ItemSpec,
                    1,
                    1,
                    $"Authoritative Catalog '{source.Document.File.Id}' does not contain a target XLIFF " +
                    $"in language package '{packageId}'.");
                return null;
            }
        }
        foreach (var target in targets)
        {
            if (!sources.ContainsKey(target.Document.File.Id))
            {
                logError(
                    CatalogMismatchCode,
                    target.Item.ItemSpec,
                    1,
                    1,
                    $"Target Catalog '{target.Document.File.Id}' is not declared by the authoritative " +
                    $"en-US contract for module '{moduleId}'.");
                return null;
            }
        }

        var succeeded = true;
        var entries = new List<LanguagePackageCatalogEntry>(targets.Count);
        foreach (var target in targets)
        {
            var source = sources[target.Document.File.Id];
            succeeded &= ValidateTarget(target, source, minimumTargetState, logError);

            var sourceFingerprint = LanguageSourceFingerprint.Compute(source.Document);
            var targetFingerprint = LanguageSourceFingerprint.Compute(target.Document);
            if (!string.Equals(sourceFingerprint, targetFingerprint, StringComparison.Ordinal))
            {
                logError(
                    CatalogMismatchCode,
                    target.Item.ItemSpec,
                    1,
                    1,
                    $"Target Catalog '{target.Document.File.Id}' source fingerprint does not match the " +
                    "authoritative en-US contract.");
                succeeded = false;
            }

            EnrichPreparedItem(
                target.Item,
                packageId,
                LanguagePackageContractValidation.Verified,
                target.PackagePath,
                sourceFingerprint);
            entries.Add(new LanguagePackageCatalogEntry(
                moduleId,
                target.Document.File.Id,
                LanguagePackageContractValidation.Verified,
                target.PackagePath,
                sourceFingerprint));
        }

        return succeeded ? entries : null;
    }

    private static bool ValidateTarget(
        ParsedTargetLanguageFile target,
        ParsedSourceLanguageFile? source,
        string minimumTargetState,
        Action<string, string, int, int, string> logError)
    {
        var options = new LanguageFileValidationOptions(
            requireCompleteBundle: true,
            minimumTargetState);
        var diagnostics = source is null
            ? LanguageFileValidation.ValidateTargetContent(target.Document, options)
            : LanguageFileValidation.ValidateTarget(source.Document, target.Document, options);
        foreach (var diagnostic in diagnostics)
        {
            logError(
                GetDiagnosticCode(diagnostic.Kind),
                target.Item.ItemSpec,
                diagnostic.Line,
                diagnostic.Column,
                diagnostic.Message);
        }
        return diagnostics.Count == 0;
    }

    private static string GetDiagnosticCode(LanguageFileValidationDiagnosticKind kind)
    {
        return kind switch
        {
            LanguageFileValidationDiagnosticKind.CatalogMismatch => CatalogMismatchCode,
            LanguageFileValidationDiagnosticKind.InvalidTranslation => InvalidTranslationCode,
            _ => InvalidPackageCode
        };
    }

    private static XliffDocumentModel? ParseDocument(
        string path,
        Action<string, string, int, int, string> logError,
        ref bool succeeded)
    {
        XliffParseResult parsed;
        try
        {
            parsed = Xliff21Parser.Parse(File.ReadAllText(path));
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            logError(
                InvalidXliffCode,
                path,
                1,
                1,
                $"Cannot read XLIFF language document: {exception.Message}");
            succeeded = false;
            return null;
        }

        if (parsed.Errors.Count == 0)
        {
            return parsed.Document;
        }

        foreach (var error in parsed.Errors)
        {
            logError(
                InvalidXliffCode,
                path,
                error.Line,
                error.Column,
                $"XLIFF language document is invalid: {error.Message}");
        }
        succeeded = false;
        return null;
    }

    private static void EnrichPreparedItem(
        ITaskItem item,
        string packageId,
        LanguagePackageContractValidation contractValidation,
        string packagePath,
        string sourceFingerprint)
    {
        item.SetMetadata("AtomUILanguageSourceKind", "StaticLanguagePack");
        item.SetMetadata("AtomUILanguageSourceIdentity", packageId);
        item.SetMetadata("AtomUILanguageContractValidation", contractValidation.ToString());
        item.SetMetadata("AtomUILanguagePackagePath", packagePath);
        item.SetMetadata("AtomUILanguageSourceFingerprint", sourceFingerprint);
    }

    private sealed class ParsedTargetLanguageFile
    {
        internal ParsedTargetLanguageFile(
            ITaskItem item,
            XliffDocumentModel document,
            string packagePath)
        {
            Item = item;
            Document = document;
            PackagePath = packagePath;
        }

        internal ITaskItem Item { get; }

        internal XliffDocumentModel Document { get; }

        internal string PackagePath { get; }
    }

    private sealed class ParsedSourceLanguageFile
    {
        internal ParsedSourceLanguageFile(
            ITaskItem item,
            XliffDocumentModel document)
        {
            Item = item;
            Document = document;
        }

        internal ITaskItem Item { get; }

        internal XliffDocumentModel Document { get; }

    }
}

internal sealed class LanguagePackagePreparationResult
{
    internal LanguagePackagePreparationResult(
        string language,
        IReadOnlyList<LanguagePackageCatalogEntry> catalogEntries,
        ITaskItem[] preparedLanguageFiles)
    {
        Language = language;
        CatalogEntries = catalogEntries;
        PreparedLanguageFiles = preparedLanguageFiles;
    }

    internal string Language { get; }

    internal IReadOnlyList<LanguagePackageCatalogEntry> CatalogEntries { get; }

    internal ITaskItem[] PreparedLanguageFiles { get; }
}
