using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

internal static class SemanticPartStyleContract
{
    internal const string XmlNamespace = "https://atomui.net";
    internal const string ClrNamespace = "AtomUI.Theme.Styling";

    private const string XmlnsDefinitionAttribute = "Avalonia.Metadata.XmlnsDefinitionAttribute";

    internal static string GetTypeName(
        SemanticControlDeclaration control,
        SemanticPartDeclaration part)
    {
        return control.ControlType.Name + ToPascalCase(part.Path) + "Style";
    }

    internal static string GetMetadataName(
        SemanticControlDeclaration control,
        SemanticPartDeclaration part)
    {
        return ClrNamespace + "." + GetTypeName(control, part);
    }

    internal static bool HasCanonicalXmlnsDefinition(IAssemblySymbol assembly)
    {
        return assembly.GetAttributes().Any(IsCanonicalXmlnsDefinition);
    }

    private static bool IsCanonicalXmlnsDefinition(AttributeData attribute)
    {
        return string.Equals(
                   attribute.AttributeClass?.ToDisplayString(),
                   XmlnsDefinitionAttribute,
                   StringComparison.Ordinal) &&
               attribute.ConstructorArguments.Length >= 2 &&
               string.Equals(
                   attribute.ConstructorArguments[0].Value as string,
                   XmlNamespace,
                   StringComparison.Ordinal) &&
               string.Equals(
                   attribute.ConstructorArguments[1].Value as string,
                   ClrNamespace,
                   StringComparison.Ordinal);
    }

    private static string ToPascalCase(string value)
    {
        return string.Concat(value.Split('.').Select(static segment =>
            segment.Length == 0
                ? string.Empty
                : char.ToUpperInvariant(segment[0]) + segment.Substring(1)));
    }
}
