using System.Security.Cryptography;
using System.Text;
using AtomUI.Theme.Build;
using AtomUI.Theme.Definitions;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

public sealed class GenerateCompiledThemeInputTask : ITask
{
    // Scalar inputs remain useful to direct task callers. MSBuild supplies the whole item
    // batch so output-path collisions are checked within one build, not against old builds.
    public string SourcePath { get; set; } = "";
    public string SourceIdentity { get; set; } = "";
    public string OutputPath { get; set; } = "";
    public ITaskItem[] ThemeInputs { get; set; } = [];
    public IBuildEngine BuildEngine { get; set; } = null!;
    public ITaskHost HostObject { get; set; } = null!;

    public bool Execute()
    {
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
                Id = document.Id, Name = document.Name, Appearance = (int)document.Appearance, IsDefault = document.IsDefault,
                Algorithms = document.Algorithms.Select(a => (int)a.Algorithm).ToArray(), Tokens = Tokens(document.Tokens),
                Controls = document.Controls.Select(c => new ThemeControlInput { Catalog = c.Identity.Catalog, Id = c.Identity.Id,
                    Mode = (int)c.AlgorithmMode, Algorithms = c.Algorithms.Select(a => (int)a.Algorithm).ToArray(), Tokens = Tokens(c.Tokens), Line = c.Location.Line, Column = c.Location.Column }).ToArray()
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
    private static ThemeLiteralInput[] Tokens(IReadOnlyList<ThemeTokenDocument> tokens) => tokens.Select(t => new ThemeLiteralInput
    { Name = t.Name, Value = t.Value, Line = t.Location.Line, Column = t.Location.Column, Path = t.Location.Path }).ToArray();

    private readonly record struct Input(string SourcePath, string SourceIdentity, string OutputPath);
}
