using AtomUI.Generator.Localization;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

/// <summary>
/// Generates the localization sources of an assembly from the types marked with <c>[LanguageCatalog]</c> and its
/// additional language files. The diagnostics of the analysis are reported before any source is written, and
/// nothing is written when the plan holds no catalog.
/// </summary>
[Generator]
public sealed class LocalizationGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Registers the pipeline that builds the localization generation result from the compilation and passes it to
    /// <c>LocalizationSourceEmitter.Emit</c>.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(LocalizationCompilationProvider.Create(context), static (sourceContext, result) =>
        {
            LocalizationSourceEmitter.Emit(sourceContext, result);
        });
    }
}
