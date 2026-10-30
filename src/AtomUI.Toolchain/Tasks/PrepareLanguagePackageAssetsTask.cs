using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

public sealed class PrepareLanguagePackageAssetsTask : AtomUILocalizationTask
{
    [Required]
    public string PackageId { get; set; } = string.Empty;

    [Required]
    public ITaskItem[] LanguageFiles { get; set; } = Array.Empty<ITaskItem>();

    public ITaskItem[] SourceLanguageFiles { get; set; } = Array.Empty<ITaskItem>();

    public string? ExpectedLanguage { get; set; }

    public string MinimumTargetState { get; set; } = "final";

    public bool RequireVerifiedContract { get; set; }

    [Output]
    public ITaskItem[] PreparedLanguageFiles { get; private set; } = Array.Empty<ITaskItem>();

    public override bool Execute()
    {
        var result = LanguagePackagePreparationCore.Prepare(
            PackageId,
            SourceLanguageFiles,
            LanguageFiles,
            ExpectedLanguage,
            MinimumTargetState,
            RequireVerifiedContract,
            LogError,
            LogWarning);
        if (result is null)
        {
            return false;
        }

        PreparedLanguageFiles = result.PreparedLanguageFiles;
        return true;
    }
}
