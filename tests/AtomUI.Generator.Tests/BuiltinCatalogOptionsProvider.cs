using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator.Tests;

// Existing schema fixtures model the repository's explicit ProjectDefaults.props catalog.
// Third-party TypeMap fixtures deliberately do not use this provider.
internal sealed class BuiltinCatalogOptionsProvider(AnalyzerConfigOptionsProvider? inner) : AnalyzerConfigOptionsProvider
{
    private static readonly AnalyzerConfigOptions Empty = new Values(null, false);
    public override AnalyzerConfigOptions GlobalOptions { get; } = new Values(inner?.GlobalOptions, true);
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => inner?.GetOptions(tree) ?? Empty;
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => inner?.GetOptions(textFile) ?? Empty;
    private sealed class Values(AnalyzerConfigOptions? options, bool catalog) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value)
        {
            if (options is not null && options.TryGetValue(key, out value!)) return true;
            value = catalog && key == "build_property.AtomUIThemeControlCatalog" ? "AtomUI" : "";
            return value.Length != 0;
        }
    }
}
