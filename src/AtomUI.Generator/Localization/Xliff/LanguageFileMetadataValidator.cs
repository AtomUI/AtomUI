using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator.Localization.Xliff;

internal static class LanguageFileMetadataValidator
{
    internal static LanguageFileInputResult Validate(
        AdditionalLanguageFile file,
        AnalyzerConfigOptions fileOptions,
        string defaultModuleId)
    {
        var moduleId = LanguageGeneratorOptions.GetFileValue(
            fileOptions,
            LanguageGeneratorOptions.ModuleIdMetadata,
            defaultModuleId);
        var sourceIdentity = LanguageGeneratorOptions.GetFileValue(
            fileOptions,
            LanguageGeneratorOptions.SourceIdentityMetadata,
            moduleId);
        var sourceKindText = LanguageGeneratorOptions.GetFileValue(
            fileOptions,
            LanguageGeneratorOptions.SourceKindMetadata,
            nameof(LanguageFileSourceKind.ModuleBuiltIn));
        if (!Enum.TryParse(sourceKindText, ignoreCase: false, out LanguageFileSourceKind sourceKind))
        {
            return Invalid(file, $"AtomUILanguageSourceKind '{sourceKindText}' is not supported");
        }

        var contractValidation = LanguageFileContractValidation.Verified;
        if (sourceKind == LanguageFileSourceKind.StaticLanguagePack)
        {
            var contractValidationText = LanguageGeneratorOptions.GetFileValue(
                fileOptions,
                LanguageGeneratorOptions.ContractValidationMetadata,
                string.Empty);
            if (contractValidationText == nameof(LanguageFileContractValidation.Verified))
            {
                contractValidation = LanguageFileContractValidation.Verified;
            }
            else if (contractValidationText == nameof(LanguageFileContractValidation.Deferred))
            {
                contractValidation = LanguageFileContractValidation.Deferred;
            }
            else
            {
                return Invalid(
                    file,
                    $"AtomUILanguageContractValidation '{contractValidationText}' is not supported; " +
                    "static language packages must declare Verified or Deferred");
            }
        }

        var sourceFingerprint = LanguageGeneratorOptions.GetFileValue(
            fileOptions,
            LanguageGeneratorOptions.SourceFingerprintMetadata,
            string.Empty);
        if (sourceKind == LanguageFileSourceKind.StaticLanguagePack && sourceFingerprint.Length == 0)
        {
            return Invalid(
                file,
                "AtomUILanguageSourceFingerprint is required for static language packages");
        }

        if (sourceFingerprint.Length > 0)
        {
            if (!IsLowercaseSha256(sourceFingerprint))
            {
                return Invalid(
                    file,
                    "AtomUILanguageSourceFingerprint must contain exactly 64 lowercase hexadecimal characters");
            }

            var actualFingerprint = AtomUI.Build.Tasks.LocalizationBuild.LanguageSourceFingerprint.Compute(file.Document);
            if (!string.Equals(sourceFingerprint, actualFingerprint, StringComparison.Ordinal))
            {
                return Invalid(
                    file,
                    $"the declared source fingerprint '{sourceFingerprint}' does not match " +
                    $"the XLIFF source contract fingerprint '{actualFingerprint}'");
            }
        }

        return new LanguageFileInputResult(
            new LanguageFileInput(
                file,
                moduleId,
                sourceKind,
                sourceIdentity,
                contractValidation,
                sourceFingerprint.Length == 0 ? null : sourceFingerprint),
            ImmutableArray<Diagnostic>.Empty);
    }

    private static bool IsLowercaseSha256(string value)
    {
        if (value.Length != 64)
        {
            return false;
        }

        foreach (var character in value)
        {
            if (character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f'))
            {
                return false;
            }
        }
        return true;
    }

    private static LanguageFileInputResult Invalid(
        AdditionalLanguageFile file,
        string message)
    {
        var diagnostic = LocalizationDiagnosticFactory.InvalidXliff(
            file.Path,
            file.Text,
            new AtomUI.Build.Tasks.LocalizationBuild.XliffParseError(message, 1, 1));
        return new LanguageFileInputResult(null, [diagnostic]);
    }
}
