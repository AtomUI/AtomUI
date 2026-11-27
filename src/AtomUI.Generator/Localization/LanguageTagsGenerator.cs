using AtomUI.Generator.Localization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator;

/// <summary>
/// Generates <c>LanguageTags.g.cs</c> and <c>GeneratedStandardLanguageDefinitions.g.cs</c> from the language data
/// file that carries the <c>AtomUILanguageTagsData</c> additional file metadata.
/// </summary>
[Generator]
public sealed class LanguageTagsGenerator : IIncrementalGenerator
{
    private const string LanguageTagsDataMetadata =
        "build_metadata.AdditionalFiles.AtomUILanguageTagsData";

    /// <summary>
    /// Registers the source output that parses the language data files, reports their diagnostics and writes the
    /// language tags sources when exactly one file was parsed without diagnostics and has entries.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var languageData = context.AdditionalTextsProvider
                                  .Combine(context.AnalyzerConfigOptionsProvider)
                                  .Where(static pair => IsLanguageTagsData(pair.Left, pair.Right))
                                  .Select(static (pair, cancellationToken) =>
                                      LanguageDataFileParser.Parse(pair.Left, cancellationToken))
                                  .Collect();

        context.RegisterSourceOutput(languageData, static (sourceContext, results) =>
        {
            foreach (var result in results)
            {
                foreach (var diagnostic in result.Diagnostics)
                {
                    sourceContext.ReportDiagnostic(diagnostic);
                }
            }

            if (results.Length == 1 &&
                results[0].Diagnostics.IsEmpty &&
                !results[0].Entries.IsEmpty)
            {
                LanguageTagsSourceWriter.Write(sourceContext, results[0].Entries);
            }
        });
    }

    private static bool IsLanguageTagsData(
        AdditionalText additionalText,
        AnalyzerConfigOptionsProvider optionsProvider)
    {
        return optionsProvider.GetOptions(additionalText)
                              .TryGetValue(LanguageTagsDataMetadata, out var value) &&
               string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }
}
