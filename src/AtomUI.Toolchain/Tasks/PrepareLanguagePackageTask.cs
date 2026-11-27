using System.Text;
using AtomUI.Build.Tasks.LocalizationBuild;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>
/// Checks that the files of a static language package contain no build logic, script, source code, runtime binary
/// or <c>.atomlang</c> payload, then validates the language files and writes the package manifest.
/// </summary>
public sealed class PrepareLanguagePackageTask : AtomUILocalizationTask
{
    private static readonly HashSet<string> s_runtimeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".dll",
        ".exe",
        ".so",
        ".dylib",
        ".cs",
        ".fs",
        ".vb",
        ".atomlang"
    };

    private static readonly HashSet<string> s_scriptExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ps1",
        ".sh",
        ".cmd",
        ".bat"
    };

    private static readonly HashSet<string> s_buildLogicExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".props",
        ".targets"
    };

    private static readonly HashSet<string> s_buildLogicDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "build",
        "buildMultiTargeting",
        "buildTransitive"
    };

    /// <summary>
    /// Identity of the language package. It is recorded in the manifest and used in validation messages.
    /// </summary>
    [Required]
    public string PackageId { get; set; } = string.Empty;

    /// <summary>
    /// The translated language files of the package. Their target language is recorded in the manifest and compared
    /// with <see cref="ExpectedLanguage" /> when that is set.
    /// </summary>
    [Required]
    public ITaskItem[] LanguageFiles { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// The authoritative en-US source language files. Only items whose <c>AtomUILanguageModuleId</c> metadata equals
    /// the module of <see cref="LanguageFiles" /> take part. With at least one match every translation is checked
    /// against its source; with none the entries are marked deferred, and <see cref="RequireVerifiedContract" />
    /// decides whether that is a warning or an error.
    /// </summary>
    public ITaskItem[] SourceLanguageFiles { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// Files that will be packed into the package. Each is checked by the extension of the path it takes inside the
    /// package, which is its <c>PackagePath</c> metadata or else its item spec; when that path has no extension, the
    /// extension of the item spec is used.
    /// Build logic under a <c>build</c>, <c>buildMultiTargeting</c> or <c>buildTransitive</c> directory, a script,
    /// source code, a runtime binary or an <c>.atomlang</c> payload fails the task before any language file is read.
    /// </summary>
    public ITaskItem[] PackageFiles { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// Path of the language package manifest to write. It lists the package identity, the language and the catalog
    /// entries. Its directory is created when missing, and it is written only after the package contents and the
    /// language files passed validation.
    /// </summary>
    [Required]
    public string OutputManifestPath { get; set; } = string.Empty;

    /// <summary>
    /// Language the package is expected to translate into, as a canonical BCP 47 tag. When set, the task fails unless
    /// the tag is canonical and equals the target language of <see cref="LanguageFiles" />.
    /// <see langword="null" /> or white space skips the check.
    /// </summary>
    public string? ExpectedLanguage { get; set; }

    /// <summary>
    /// Lowest XLIFF target state a translation must have to pass validation, <c>final</c> by default. A translation
    /// below it fails the task.
    /// </summary>
    public string MinimumTargetState { get; set; } = "final";

    /// <summary>
    /// <see langword="true" /> fails the task when <see cref="SourceLanguageFiles" /> holds no file of the module
    /// <see cref="LanguageFiles" /> translates, so the package is never built without validation against the en-US
    /// contract. With <see langword="false" /> that case only logs a warning.
    /// </summary>
    public bool RequireVerifiedContract { get; set; }

    /// <summary>
    /// The <see cref="LanguageFiles" /> items, enriched with the package metadata. It is set only when
    /// <see cref="Execute" /> succeeds and stays empty otherwise.
    /// </summary>
    [Output]
    public ITaskItem[] PreparedLanguageFiles { get; private set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// Checks the package contents, validates the language files and writes the manifest.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> and publishes <see cref="PreparedLanguageFiles" /> when both checks passed; otherwise
    /// <see langword="false" />, with the cause logged as an error and no manifest written.
    /// </returns>
    public override bool Execute()
    {
        if (!ValidatePackageContents())
        {
            return false;
        }

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

        var manifest = new LanguagePackageManifest(PackageId, result.Language, result.CatalogEntries);
        WriteFile(OutputManifestPath, LanguagePackageManifestWriter.Write(manifest));
        PreparedLanguageFiles = result.PreparedLanguageFiles;
        return true;
    }

    private bool ValidatePackageContents()
    {
        var succeeded = true;
        foreach (var item in PackageFiles)
        {
            foreach (var packagePath in GetPackagePaths(item))
            {
                var extension = GetEffectiveExtension(packagePath, item.ItemSpec);
                if (IsBuildLogicFile(packagePath, extension))
                {
                    Error(item.ItemSpec, "Static language packages cannot contain executable build logic.");
                    succeeded = false;
                    break;
                }

                if (s_scriptExtensions.Contains(extension))
                {
                    Error(item.ItemSpec, "Static language packages cannot contain scripts.");
                    succeeded = false;
                    break;
                }

                if (!s_runtimeExtensions.Contains(extension))
                {
                    continue;
                }

                Error(
                    item.ItemSpec,
                    extension.Equals(".atomlang", StringComparison.OrdinalIgnoreCase)
                        ? "Static language packages cannot contain .atomlang payloads."
                        : extension is ".cs" or ".fs" or ".vb"
                            ? "Static language packages cannot contain runtime source code."
                        : "Static language packages cannot contain a runtime assembly or native library.");
                succeeded = false;
                break;
            }
        }
        return succeeded;
    }

    private static IEnumerable<string> GetPackagePaths(ITaskItem item)
    {
        var packagePath = item.GetMetadata("PackagePath");
        var hasPackagePath = false;
        foreach (var path in packagePath.Split([';'], StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmedPath = path.Trim();
            if (trimmedPath.Length > 0)
            {
                hasPackagePath = true;
                yield return trimmedPath;
            }
        }

        if (!hasPackagePath)
        {
            yield return item.ItemSpec;
        }
    }

    private static string GetEffectiveExtension(string packagePath, string sourcePath)
    {
        var extension = Path.GetExtension(packagePath);
        return extension.Length == 0 ? Path.GetExtension(sourcePath) : extension;
    }

    private static bool IsBuildLogicFile(string path, string extension)
    {
        if (!s_buildLogicExtensions.Contains(extension))
        {
            return false;
        }

        return path.Replace('\\', '/')
                   .Split('/')
                   .Any(s_buildLogicDirectories.Contains);
    }

    private void Error(string file, string message)
    {
        LogError("ATOMUILOC009", file, 1, 1, message);
    }

    private static void WriteFile(string path, string content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }

}
