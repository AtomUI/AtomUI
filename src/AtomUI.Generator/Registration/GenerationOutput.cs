using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AtomUI.Generator;

// Writers share this sink. Only detached source/diagnostic values cross the incremental cache boundary.
internal sealed class GenerationOutput
{
    private readonly SourceProductionContext? _forward;
    private readonly List<GeneratedFile> _files = new();
    private readonly List<GeneratedDiagnostic> _diagnostics = new();
    internal GenerationOutput() { }
    private GenerationOutput(SourceProductionContext context) => _forward = context;
    public static implicit operator GenerationOutput(SourceProductionContext context) => new(context);
    internal void AddSource(string hintName, SourceText text)
    {
        if (_forward is { } context)
        {
            context.AddSource(hintName, text);
        }
        else
        {
            _files.Add(new(hintName, text.ToString()));
        }
    }
    internal void ReportDiagnostic(Diagnostic diagnostic)
    {
        if (_forward is { } context) { context.ReportDiagnostic(diagnostic); return; }
        var location = diagnostic.Location;
        var lineSpan = location.GetLineSpan();
        _diagnostics.Add(new(diagnostic.Descriptor, diagnostic.GetMessage(), lineSpan.Path,
            location.SourceSpan.Start, location.SourceSpan.Length,
            lineSpan.StartLinePosition.Line, lineSpan.StartLinePosition.Character,
            lineSpan.EndLinePosition.Line, lineSpan.EndLinePosition.Character));
    }
    internal GenerationResult Freeze() => new(new(_files.OrderBy(f => f.HintName, StringComparer.Ordinal)),
        new(_diagnostics.OrderBy(d => d.Descriptor.Id, StringComparer.Ordinal).ThenBy(d => d.Path, StringComparer.Ordinal).ThenBy(d => d.Start)));
}
internal sealed record GeneratedFile(string HintName, string Text);
internal sealed record GeneratedDiagnostic(DiagnosticDescriptor Descriptor, string Message, string Path,
    int Start, int Length, int StartLine, int StartColumn, int EndLine, int EndColumn)
{
    internal Diagnostic ToDiagnostic()
    {
        var descriptor = new DiagnosticDescriptor(Descriptor.Id, Descriptor.Title, "{0}", Descriptor.Category,
            Descriptor.DefaultSeverity, Descriptor.IsEnabledByDefault, Descriptor.Description, Descriptor.HelpLinkUri);
        var location = string.IsNullOrEmpty(Path) ? Location.None : Location.Create(Path, new(Start, Length), new(new(StartLine, StartColumn), new(EndLine, EndColumn)));
        return Diagnostic.Create(descriptor, location, Message);
    }
}
internal sealed record GenerationResult(ValueArray<GeneratedFile> Files, ValueArray<GeneratedDiagnostic> Diagnostics)
{
    internal void Write(SourceProductionContext context)
    {
        foreach (var file in Files)
        {
            context.AddSource(file.HintName, GeneratedSourceText.From(file.Text));
        }

        foreach (var diagnostic in Diagnostics)
        {
            context.ReportDiagnostic(diagnostic.ToDiagnostic());
        }
    }
}
