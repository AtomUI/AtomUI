using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator;

internal static class ThemeGeneratorOptions
{
    private const string ControlCatalogProperty = "build_property.AtomUIThemeControlCatalog";
    internal const string ControlCatalogMetadataKey = "AtomUIThemeControlCatalog";
    private const string AssemblyMetadataAttribute = "System.Reflection.AssemblyMetadataAttribute";
    private const string BuiltInCatalogAssembly = "AtomUI";

    internal static string GetPackageId(AnalyzerConfigOptionsProvider optionsProvider, string assemblyName) =>
        optionsProvider.GlobalOptions.TryGetValue("build_property.AtomUIRegistrationPackageId", out var value) &&
        !string.IsNullOrWhiteSpace(value) ? value.Trim() : assemblyName;

    internal static string GetControlCatalog(AnalyzerConfigOptionsProvider optionsProvider, string assemblyName)
    {
        return optionsProvider.GlobalOptions.TryGetValue(ControlCatalogProperty, out var value) &&
               !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : assemblyName;
    }

    internal static string GetReferencedControlCatalog(IAssemblySymbol assembly)
    {
        if (assembly is null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }
        foreach (var attribute in assembly.GetAttributes())
        {
            if (!string.Equals(
                    attribute.AttributeClass?.ToDisplayString(),
                    AssemblyMetadataAttribute,
                    StringComparison.Ordinal) ||
                attribute.ConstructorArguments.Length != 2 ||
                attribute.ConstructorArguments[0].Value is not string key ||
                !string.Equals(key, ControlCatalogMetadataKey, StringComparison.Ordinal) ||
                attribute.ConstructorArguments[1].Value is not string catalog ||
                string.IsNullOrWhiteSpace(catalog))
            {
                continue;
            }

            return catalog.Trim();
        }

        return assembly.Name;
    }

    internal static bool IsBuiltInControlCatalog(IAssemblySymbol assembly)
    {
        return string.Equals(
            GetReferencedControlCatalog(assembly),
            BuiltInCatalogAssembly,
            StringComparison.Ordinal);
    }
}
