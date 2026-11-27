using AtomUI.Theme.Definitions.Compiled;
using System.Security.Cryptography;
using System.Text;
using AtomUI.Theme.Build;
using AtomUI.Theme.Definitions;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>
/// Converts theme definition documents into the encoded <c>CompiledThemeInput</c> intermediates for the compiled
/// theme generator, and reports document diagnostics to the build log.
/// </summary>
public sealed class GenerateCompiledThemeInputTask : ITask
{
    // Scalar inputs remain useful to direct task callers. MSBuild supplies the whole item
    // batch so output-path collisions are checked within one build, not against old builds.
    /// <summary>
    /// Path of the theme document to compile when <see cref="ThemeInputs" /> is empty. With item inputs it only
    /// locates the error for an input that lacks a path, an identity or an output path.
    /// </summary>
    public string SourcePath { get; set; } = "";
    /// <summary>
    /// Logical identity of the theme document when <see cref="ThemeInputs" /> is empty. The build targets pass the
    /// assembly name followed by the document's logical path. The reader receives it as given; the compiled input
    /// stores it with backslashes replaced by <c>/</c>.
    /// </summary>
    public string SourceIdentity { get; set; } = "";
    /// <summary>
    /// Path of the intermediate to write when <see cref="ThemeInputs" /> is empty. The file is replaced only when
    /// its content differs, and it is deleted when the input fails.
    /// </summary>
    public string OutputPath { get; set; } = "";
    /// <summary>
    /// Theme documents to compile in one run. When it is empty, the task compiles the single document described by
    /// <see cref="SourcePath" />, <see cref="SourceIdentity" /> and <see cref="OutputPath" /> instead.
    /// An item is read from its full path; its <c>SourceIdentity</c> and <c>InputPath</c> metadata give the
    /// identity and the intermediate to write.
    /// </summary>
    public ITaskItem[] ThemeInputs { get; set; } = [];
    /// <summary>
    /// Receives the errors and warnings of the task. It is <see langword="null" /> after construction and must be
    /// assigned before <see cref="Execute" /> is called.
    /// </summary>
    public IBuildEngine BuildEngine { get; set; } = null!;
    /// <summary>
    /// Satisfies <see cref="ITask" />. This task never reads it.
    /// </summary>
    public ITaskHost HostObject { get; set; } = null!;

    /// <summary>
    /// Compiles every input, logging document diagnostics as build warnings and errors. Two inputs that resolve to
    /// the same output path fail the run and the shared output is deleted. Every input is processed even after one
    /// fails.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when each intermediate was written or was already identical; otherwise
    /// <see langword="false" />.
    /// </returns>
    public bool Execute()
    {
        _literalCache.Clear();
        var inputs = ThemeInputs.Length == 0
            ? [new Input(SourcePath, SourceIdentity, OutputPath)]
            : ThemeInputs.Select(item => new Input(item.GetMetadata("FullPath"), item.GetMetadata("SourceIdentity"), item.GetMetadata("InputPath"))).ToArray();
        if (inputs.Any(input => string.IsNullOrWhiteSpace(input.SourcePath) || string.IsNullOrWhiteSpace(input.SourceIdentity) || string.IsNullOrWhiteSpace(input.OutputPath)))
        {
            Error(SourcePath, "Compiled theme task requires a source path, logical identity and output path.");
            return false;
        }

        var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var collisions = inputs.GroupBy(input => Path.GetFullPath(input.OutputPath), comparer).Where(group => group.Count() > 1).ToArray();
        if (collisions.Length != 0)
        {
            foreach (var collision in collisions)
            {
                Error(collision.Last().SourcePath, "Compiled theme intermediate identity collision: multiple inputs target '" + collision.Key + "'.");
                DeleteOutput(collision.Key);
            }
            return false;
        }

        var success = true;
        foreach (var input in inputs) { success &= ExecuteOne(input); }
        return success;
    }

    private bool ExecuteOne(Input input)
    {
        string? candidate = null;
        try
        {
            using var source = File.OpenRead(input.SourcePath);
            using var snapshot = new MemoryStream();
            var buffer = new byte[4096];
            int count;
            while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                if (snapshot.Length + count > ThemeDocumentReaderOptions.DefaultMaxDocumentBytes) { throw new InvalidDataException("Theme exceeds the document byte limit."); }
                snapshot.Write(buffer, 0, count);
            }
            var digest = Convert.ToHexString(SHA256.HashData(snapshot.ToArray()));
            snapshot.Position = 0;
            var result = ThemeDocumentReader.Read(snapshot, input.SourceIdentity);
            foreach (var diagnostic in result.Diagnostics)
            {
                if (diagnostic.Severity == ThemeDefinitionDiagnosticSeverity.Error)
                {
                    BuildEngine.LogErrorEvent(new BuildErrorEventArgs(null, diagnostic.Code, input.SourcePath, diagnostic.Line, diagnostic.Column, 0, 0, diagnostic.Message, null, nameof(GenerateCompiledThemeInputTask)));
                }
                else
                {
                    BuildEngine.LogWarningEvent(new BuildWarningEventArgs(null, diagnostic.Code, input.SourcePath, diagnostic.Line, diagnostic.Column, 0, 0, diagnostic.Message, null, nameof(GenerateCompiledThemeInputTask)));
                }
            }
            if (!result.Success) { DeleteOutput(input.OutputPath); return false; }
            var document = result.Document!;
            var neutral = new CompiledThemeInput
            {
                SourceIdentity = input.SourceIdentity.Replace('\\', '/'), SourcePath = Path.GetFullPath(input.SourcePath), ContentDigest = digest,
                Id = document.Id, Name = document.Name, Language = document.Language, Style = document.Style ?? "",
                Appearance = document.Appearance is { } appearance ? (int)appearance : -1, Density = document.Density ?? "",
                Line = document.Location.Line, Column = document.Location.Column,
                Base = Section(document.Values),
                Conditions = document.Conditions.Select(c => new ThemeConditionInput
                {
                    Style = c.Style ?? "", Appearance = c.Appearance is { } a ? (int)a : -1, Density = c.Density ?? "", Idiom = c.Idiom ?? "",
                    Line = c.Location.Line, Column = c.Location.Column, Section = Section(c.Values)
                }).ToArray()
            };
            var content = neutral.Encode();
            var output = Path.GetFullPath(input.OutputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            if (File.Exists(output) && File.ReadAllText(output) == content) { return true; }
            candidate = output + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(candidate, content, new UTF8Encoding(false));
            File.Move(candidate, output, true);
            candidate = null;
            return true;
        }
        catch (Exception error)
        {
            Error(input.SourcePath, error.GetBaseException().Message);
            DeleteOutput(input.OutputPath);
            return false;
        }
        finally
        {
            if (candidate is not null) { DeleteOutput(candidate); }
        }
    }

    private void Error(string path, string message) => BuildEngine.LogErrorEvent(new BuildErrorEventArgs(null, "ATOMUITOKEN001", path, 0, 0, 0, 0, message, null, nameof(GenerateCompiledThemeInputTask)));
    private void DeleteOutput(string path)
    {
        try { if (File.Exists(path)) { File.Delete(path); } }
        catch (Exception cleanup) { Error(path, "Could not delete stale compiled theme intermediate: " + cleanup.Message); }
    }
    private readonly Dictionary<string, BuiltInThemeLiteralValue[]> _literalCache = new(StringComparer.Ordinal);

    private BuiltInThemeLiteralValue[] CompileLiteral(string value)
    {
        if (!_literalCache.TryGetValue(value, out var result))
        {
            result = BuiltInThemeLiteralCodec.Compile(value).ToArray();
            _literalCache.Add(value, result);
        }
        return result;
    }

    private ThemeSectionInput Section(ThemeSectionDocument section) => new()
    {
        Tokens = Tokens(section.Tokens), LanguageTokens = Tokens(section.LanguageTokens),
        Controls = section.Controls.Select(c => new ThemeControlInput { Catalog = c.Identity.Catalog, Id = c.Identity.Id,
            Appearance = c.Appearance is { } appearance ? (int)appearance : -1, Density = c.Density ?? "",
            Tokens = Tokens(c.Tokens), Line = c.Location.Line, Column = c.Location.Column }).ToArray()
    };
    private ThemeLiteralInput[] Tokens(IReadOnlyList<ThemeTokenDocument> tokens) => tokens.Select(t => new ThemeLiteralInput
    { Name = t.Name, Value = t.Value, Locked = t.Locked, Values = CompileLiteral(t.Value), Line = t.Location.Line, Column = t.Location.Column, Path = t.Location.Path }).ToArray();

    private readonly record struct Input(string SourcePath, string SourceIdentity, string OutputPath);
}
