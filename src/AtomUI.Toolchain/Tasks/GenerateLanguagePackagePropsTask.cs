using System.Text;
using AtomUI.Build.Tasks.LocalizationBuild;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

public sealed class GenerateLanguagePackagePropsTask : AtomUILocalizationTask
{
    [Required]
    public string PackageId { get; set; } = string.Empty;

    [Required]
    public ITaskItem[] LanguageFiles { get; set; } = Array.Empty<ITaskItem>();

    [Required]
    public string OutputPath { get; set; } = string.Empty;

    public bool RequireTargetLanguage { get; set; } = true;

    public string SourceKind { get; set; } = "StaticLanguagePack";

    public override bool Execute()
    {
        var entries = new List<LanguagePackageCatalogEntry>();
        foreach (var item in LanguageFiles)
        {
            var moduleId = item.GetMetadata("AtomUILanguageModuleId").Trim();
            if (moduleId.Length == 0 || !LanguagePackagePath.TryNormalize(
                    item.GetMetadata("AtomUILanguagePackagePath"),
                    out var packagePath))
            {
                LogError(
                    "ATOMUILOC009",
                    item.ItemSpec,
                    1,
                    1,
                    "Language package items require module ID and a normalized package path.");
                return false;
            }

            var validationText = item.GetMetadata("AtomUILanguageContractValidation").Trim();
            if (validationText.Length == 0 && string.Equals(SourceKind, "ModuleBuiltIn", StringComparison.Ordinal))
            {
                validationText = nameof(LanguagePackageContractValidation.Verified);
            }
            if (!Enum.TryParse(
                    validationText,
                    ignoreCase: false,
                    out LanguagePackageContractValidation contractValidation))
            {
                LogError(
                    "ATOMUILOC009",
                    item.ItemSpec,
                    1,
                    1,
                    "AtomUILanguageContractValidation must be Verified or Deferred.");
                return false;
            }

            var parsed = Xliff21Parser.Parse(File.ReadAllText(item.ItemSpec));
            if (parsed.Document is null)
            {
                LogError("ATOMUILOC005", item.ItemSpec, 1, 1, "The language package XLIFF is invalid.");
                return false;
            }
            if (RequireTargetLanguage && parsed.Document.TargetLanguage is null)
            {
                LogError(
                    "ATOMUILOC005",
                    item.ItemSpec,
                    1,
                    1,
                    "A static language package requires a target-language XLIFF document.");
                return false;
            }
            if (!RequireTargetLanguage && parsed.Document.TargetLanguage is not null)
            {
                LogError(
                    "ATOMUILOC005",
                    item.ItemSpec,
                    1,
                    1,
                    "A built-in language Catalog template must be an en-US document without trgLang.");
                return false;
            }
            entries.Add(new LanguagePackageCatalogEntry(
                moduleId,
                parsed.Document.File.Id,
                contractValidation,
                packagePath,
                LanguageSourceFingerprint.Compute(parsed.Document)));
        }

        var directory = Path.GetDirectoryName(OutputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(
            OutputPath,
            PackagePropsWriter.Write(PackageId, entries, SourceKind),
            new UTF8Encoding(false));
        return true;
    }
}
