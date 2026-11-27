using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>
/// Validates the translated XLIFF files of a language package, against the authoritative en-US sources of the
/// module when they are available, and publishes the validated items for packaging.
/// </summary>
public sealed class PrepareLanguagePackageAssetsTask : AtomUILocalizationTask
{
    /// <summary>
    /// Id of the language package being built. It is written to the
    /// <c>AtomUILanguageSourceIdentity</c> metadata of each prepared file and names the package in diagnostics.
    /// </summary>
    [Required]
    public string PackageId { get; set; } = string.Empty;

    /// <summary>
    /// The translated XLIFF files of the package. Every item must carry the same non-empty
    /// <c>AtomUILanguageModuleId</c> metadata, which selects the module being translated.
    /// </summary>
    [Required]
    public ITaskItem[] LanguageFiles { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// The authoritative en-US source XLIFF files. Only items whose <c>AtomUILanguageModuleId</c> metadata equals
    /// the module of <see cref="LanguageFiles" /> take part. With at least one match every translation is checked
    /// against its source catalog and marked verified; with none the entries are marked deferred and a warning is
    /// logged.
    /// </summary>
    public ITaskItem[] SourceLanguageFiles { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// Language the package is expected to translate into, as a canonical BCP 47 tag. When set, the task fails
    /// unless the tag is canonical and equals the target language of <see cref="LanguageFiles" />.
    /// <see langword="null" /> or white space skips the check.
    /// </summary>
    public string? ExpectedLanguage { get; set; }

    /// <summary>
    /// Lowest XLIFF target state a translation must have to pass validation, <c>final</c> by default. A target
    /// below it is logged as an error and fails the task.
    /// </summary>
    public string MinimumTargetState { get; set; } = "final";

    /// <summary>
    /// <see langword="true" /> fails the task when <see cref="SourceLanguageFiles" /> holds no file of the target
    /// module, so the package is never built without validation against the en-US contract. With
    /// <see langword="false" /> that case only logs a warning.
    /// </summary>
    public bool RequireVerifiedContract { get; set; }

    /// <summary>
    /// The <see cref="LanguageFiles" /> items, enriched with the package metadata. It is set only when
    /// <see cref="Execute" /> succeeds and stays empty otherwise.
    /// </summary>
    [Output]
    public ITaskItem[] PreparedLanguageFiles { get; private set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// Validates the language files and enriches their items with the package metadata.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> and publishes <see cref="PreparedLanguageFiles" /> when validation passed;
    /// otherwise <see langword="false" />, with the cause logged as an error.
    /// </returns>
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
