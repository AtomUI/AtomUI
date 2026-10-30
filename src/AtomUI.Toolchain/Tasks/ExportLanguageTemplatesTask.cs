using System.Text;
using AtomUI.Build.Tasks.LocalizationBuild;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

public sealed class ExportLanguageTemplatesTask : AtomUILocalizationTask
{
    [Required]
    public ITaskItem[] SourceFiles { get; set; } = Array.Empty<ITaskItem>();

    [Required]
    public string TargetLanguage { get; set; } = string.Empty;

    public string? OutputRootDirectory { get; set; }

    [Output]
    public ITaskItem[] ExportedFiles { get; private set; } = Array.Empty<ITaskItem>();

    public override bool Execute()
    {
        if (!Bcp47LanguageTagParser.TryParse(TargetLanguage, out var canonicalLanguage) ||
            !string.Equals(TargetLanguage, canonicalLanguage, StringComparison.Ordinal))
        {
            LogError(
                "ATOMUILOC005",
                string.Empty,
                1,
                1,
                "TargetLanguage must be a canonical BCP 47 language tag.");
            return false;
        }

        var exported = new List<ITaskItem>();
        var succeeded = true;
        foreach (var sourceItem in SourceFiles)
        {
            var source = Parse(sourceItem.ItemSpec, ref succeeded);
            if (source is null)
            {
                continue;
            }
            if (source.TargetLanguage is not null)
            {
                LogError(
                    "ATOMUILOC005",
                    sourceItem.ItemSpec,
                    1,
                    1,
                    "A language template source must be an en-US document without trgLang.");
                succeeded = false;
                continue;
            }

            var outputPath = ResolveOutputPath(sourceItem, canonicalLanguage);
            if (outputPath is null)
            {
                succeeded = false;
                continue;
            }

            XliffDocumentModel? existing = null;
            if (File.Exists(outputPath))
            {
                existing = Parse(outputPath, ref succeeded);
                if (existing is null)
                {
                    continue;
                }
            }

            XliffDocumentModel merged;
            try
            {
                merged = XliffMergeEngine.Merge(source, existing, canonicalLanguage);
            }
            catch (ArgumentException exception)
            {
                LogError("ATOMUILOC006", outputPath, 1, 1, exception.Message);
                succeeded = false;
                continue;
            }

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(outputPath, Xliff21Writer.Write(merged), new UTF8Encoding(false));
            exported.Add(new GeneratedTaskItem(outputPath));
        }

        ExportedFiles = exported.ToArray();
        return succeeded;
    }

    private string? ResolveOutputPath(ITaskItem sourceItem, string targetLanguage)
    {
        var explicitPath = sourceItem.GetMetadata("AtomUILanguageTemplateOutputPath");
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            return explicitPath;
        }

        if (!string.IsNullOrWhiteSpace(OutputRootDirectory))
        {
            var packagePathValue = sourceItem.GetMetadata("AtomUILanguagePackagePath");
            if (!LanguagePackagePath.TryNormalize(packagePathValue, out var packagePath))
            {
                LogError(
                    "ATOMUILOC009",
                    sourceItem.ItemSpec,
                    1,
                    1,
                    "AtomUILanguagePackagePath must be a normalized relative path before exporting a template.");
                return null;
            }

            const string localizationPrefix = "Localization/";
            if (packagePath.StartsWith(localizationPrefix, StringComparison.Ordinal))
            {
                packagePath = packagePath.Substring(localizationPrefix.Length);
            }

            var relativeDirectory = Path.GetDirectoryName(packagePath.Replace('/', Path.DirectorySeparatorChar));
            return Path.Combine(
                OutputRootDirectory!,
                relativeDirectory ?? string.Empty,
                targetLanguage + ".xlf");
        }

        return Path.Combine(
            Path.GetDirectoryName(sourceItem.ItemSpec) ?? string.Empty,
            targetLanguage + ".xlf");
    }

    private XliffDocumentModel? Parse(string path, ref bool succeeded)
    {
        XliffParseResult parsed;
        try
        {
            parsed = Xliff21Parser.Parse(File.ReadAllText(path));
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            LogError("ATOMUILOC005", path, 1, 1, $"Cannot read XLIFF document: {exception.Message}");
            succeeded = false;
            return null;
        }

        if (parsed.Errors.Count == 0)
        {
            return parsed.Document;
        }
        foreach (var error in parsed.Errors)
        {
            LogError("ATOMUILOC005", path, error.Line, error.Column, error.Message);
        }
        succeeded = false;
        return null;
    }
}
