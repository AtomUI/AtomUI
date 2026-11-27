using System.Text;
using AtomUI.Build.Tasks.LocalizationBuild;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>
/// MSBuild task that generates the MSBuild props file listing the catalog entries of a language package.
/// </summary>
public sealed class GenerateLanguagePackagePropsTask : AtomUILocalizationTask
{
    /// <summary>
    /// The ID of the language package the props file is generated for.
    /// </summary>
    [Required]
    public string PackageId { get; set; } = string.Empty;

    /// <summary>
    /// The XLIFF documents of the package, one catalog entry each. Every item needs the <c>AtomUILanguageModuleId</c>
    /// and <c>AtomUILanguagePackagePath</c> metadata; <c>AtomUILanguageContractValidation</c> is optional.
    /// </summary>
    [Required]
    public ITaskItem[] LanguageFiles { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// The path of the props file to write. Its directory is created when it does not exist.
    /// </summary>
    [Required]
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// When <see langword="true" />, every language file must be a target-language XLIFF document; when
    /// <see langword="false" />, every file must be an en-US document without <c>trgLang</c>.
    /// </summary>
    public bool RequireTargetLanguage { get; set; } = true;

    /// <summary>
    /// The source kind of the language files, <c>StaticLanguagePack</c> by default. <c>ModuleBuiltIn</c> makes an item
    /// without <c>AtomUILanguageContractValidation</c> metadata default to <c>Verified</c>.
    /// </summary>
    public string SourceKind { get; set; } = "StaticLanguagePack";

    /// <summary>
    /// Validates every language file and writes the props file with one catalog entry per file.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the props file was written; <see langword="false" /> when an item is invalid.
    /// Validation stops at the first invalid item and logs the reason as <c>ATOMUILOC005</c> or <c>ATOMUILOC009</c>.
    /// </returns>
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
