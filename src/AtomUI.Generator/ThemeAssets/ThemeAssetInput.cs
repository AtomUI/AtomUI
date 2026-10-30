using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AtomUI.Generator;

// Comparable additional-file input. The XML document is parsed once in the binding transform,
// shared with semantic/resource analysis there, and never stored in the incremental cache.
internal sealed record ThemeAssetInput(string Path, string Text, string? Directory, string? Link)
{
    internal AdditionalText AsAdditionalText() => new InputText(Path, Text);
    private sealed class InputText : AdditionalText
    {
        private readonly string _text;
        internal InputText(string path, string text) { Path = path; _text = text; }
        public override string Path { get; }
        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(_text);
    }
}
