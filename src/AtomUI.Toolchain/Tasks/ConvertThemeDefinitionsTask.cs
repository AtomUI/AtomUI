using System.Text;
using AtomUI.Theme.Build;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>
/// <c>dotnet msbuild -t:ConvertAtomUIThemeDefinitions -p:AtomUIThemeDefinitionInput=&lt;directory&gt;</c>: rewrites
/// every v1 <c>*.theme.xml</c> of a directory as v2. Output goes to <see cref="OutputDirectory" />, or replaces the
/// sources in place when it is empty; the variant files merged into another file are then removed.
/// </summary>
public sealed class ConvertThemeDefinitionsTask : ITask
{
    /// <summary>
    /// The directory to read the v1 <c>*.theme.xml</c> files from. Subdirectories are not searched.
    /// </summary>
    [Required]
    public string InputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// The directory the converted files are written to. When it is <see langword="null" /> or blank, the sources are
    /// replaced in <see cref="InputDirectory" /> and the variant files that were merged into another file are deleted.
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// The v2 files the task wrote, one item per output file.
    /// </summary>
    [Output]
    public ITaskItem[] ConvertedFiles { get; private set; } = [];

    /// <summary>
    /// The build engine that receives the messages, errors and warnings the task logs. The build host assigns it
    /// before <see cref="Execute" /> runs.
    /// </summary>
    public IBuildEngine BuildEngine { get; set; } = null!;
    /// <summary>
    /// The host object MSBuild passes to the task. It is required by <see cref="ITask" /> and is not read by this task.
    /// </summary>
    public ITaskHost HostObject { get; set; } = null!;

    /// <summary>
    /// Converts every <c>*.theme.xml</c> in <see cref="InputDirectory" /> and writes the v2 files. Nothing is written
    /// when the conversion fails. The notes of the conversion are logged as build errors or warnings.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the files were written; <see langword="false" /> when the input directory
    /// does not exist or the conversion failed.
    /// </returns>
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
