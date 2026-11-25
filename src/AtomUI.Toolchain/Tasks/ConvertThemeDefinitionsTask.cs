using System.Text;
using AtomUI.Theme.Build;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>
/// <c>dotnet msbuild -t:ConvertAtomUIThemeDefinitions -p:AtomUIThemeDefinitionInput=&lt;directory&gt;</c>: rewrites every
/// v1 <c>*.theme.xml</c> of a directory as v2 (spec 7.11). Output goes to <see cref="OutputDirectory"/>, or replaces the
/// sources in place when it is empty; the variant files merged into another file are then removed.
/// </summary>
public sealed class ConvertThemeDefinitionsTask : ITask
{
    [Required]
    public string InputDirectory { get; set; } = string.Empty;

    public string OutputDirectory { get; set; } = string.Empty;

    [Output]
    public ITaskItem[] ConvertedFiles { get; private set; } = [];

    public IBuildEngine BuildEngine { get; set; } = null!;
    public ITaskHost HostObject { get; set; } = null!;

    public bool Execute()
    {
        if (!Directory.Exists(InputDirectory))
        {
            Log(true, InputDirectory, $"The theme definition directory '{InputDirectory}' does not exist.");
            return false;
        }

        var sources = Directory.GetFiles(InputDirectory, "*.theme.xml", SearchOption.TopDirectoryOnly)
                               .OrderBy(static path => path, StringComparer.Ordinal)
                               .Select(static path => (Path.GetFileName(path), File.ReadAllText(path)))
                               .ToArray();
        var result = ThemeDefinitionV1Converter.Convert(sources);
        foreach (var note in result.Notes)
        {
            Log(note.IsError, Path.Combine(InputDirectory, note.FileName), note.Message);
        }
        if (!result.Success)
        {
            return false;
        }

        var inPlace = string.IsNullOrWhiteSpace(OutputDirectory);
        var target = inPlace ? InputDirectory : OutputDirectory;
        Directory.CreateDirectory(target);
        var written = new List<ITaskItem>();
        foreach (var file in result.Files)
        {
            var path = Path.Combine(target, file.FileName);
            File.WriteAllText(path, file.Xml, new UTF8Encoding(false));
            written.Add(new GeneratedTaskItem(path));
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                $"Converted {string.Join(", ", file.SourceFileNames)} -> {file.FileName}", null, nameof(ConvertThemeDefinitionsTask),
                MessageImportance.High));
        }
        if (inPlace)
        {
            foreach (var merged in result.MergedFileNames)
            {
                File.Delete(Path.Combine(InputDirectory, merged));
            }
        }

        ConvertedFiles = written.ToArray();
        return true;
    }

    private void Log(bool error, string file, string message)
    {
        if (error)
        {
            BuildEngine.LogErrorEvent(new BuildErrorEventArgs(null, "ATMTHM1004", file, 0, 0, 0, 0, message, null,
                nameof(ConvertThemeDefinitionsTask)));
        }
        else
        {
            BuildEngine.LogWarningEvent(new BuildWarningEventArgs(null, "ATMTHM1005", file, 0, 0, 0, 0, message, null,
                nameof(ConvertThemeDefinitionsTask)));
        }
    }
}
