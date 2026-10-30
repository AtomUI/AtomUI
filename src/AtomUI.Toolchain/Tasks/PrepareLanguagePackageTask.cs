using System.Text;
using AtomUI.Build.Tasks.LocalizationBuild;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

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

    [Required]
    public string PackageId { get; set; } = string.Empty;

    [Required]
    public ITaskItem[] LanguageFiles { get; set; } = Array.Empty<ITaskItem>();

    public ITaskItem[] SourceLanguageFiles { get; set; } = Array.Empty<ITaskItem>();

    public ITaskItem[] PackageFiles { get; set; } = Array.Empty<ITaskItem>();

    [Required]
    public string OutputManifestPath { get; set; } = string.Empty;

    public string? ExpectedLanguage { get; set; }

    public string MinimumTargetState { get; set; } = "final";

    public bool RequireVerifiedContract { get; set; }

    [Output]
    public ITaskItem[] PreparedLanguageFiles { get; private set; } = Array.Empty<ITaskItem>();

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
