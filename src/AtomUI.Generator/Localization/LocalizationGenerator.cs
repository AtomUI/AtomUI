using AtomUI.Generator.Localization;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

[Generator]
public sealed class LocalizationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(LocalizationCompilationProvider.Create(context), static (sourceContext, result) =>
        {
            LocalizationSourceEmitter.Emit(sourceContext, result);
        });
    }
}
