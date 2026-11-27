using System.Xml.Linq;
using AtomUI.SourceGeneration;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

/// <summary>
/// Transfers shape evidence from a theme producer to generator consumers at compile time.
/// </summary>
internal static class SemanticThemeFacts
{
    private const string MetadataKey = "AtomUI.SemanticThemeFacts.v1";
    private const string MetadataAttribute = "System.Reflection.AssemblyMetadataAttribute";

    internal static void Write(GenerationOutput output, IReadOnlyList<ThemeAssetInfo> assets)
    {
        var facts = new XElement("themes", assets.Where(static asset => asset.SemanticThemes.Count != 0)
            .OrderBy(static asset => asset.AssetPath, StringComparer.Ordinal)
            .Select(asset => new XElement("asset", new XAttribute("path", asset.AssetPath),
                asset.SemanticThemes.Select(WriteTheme))));
        if (!facts.HasElements)
        {
            return;
        }
        var source = $"[assembly: global::{MetadataAttribute}(\"{MetadataKey}\", " +
                     ThemeBuildContracts.Literal(facts.ToString(SaveOptions.DisableFormatting)) + ")]";
        output.AddSource("SemanticThemeFacts.g.cs", GeneratedSourceText.From(source));
    }

    internal static IEnumerable<ThemeAssetInfo> Read(Compilation compilation)
    {
        foreach (var reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            foreach (var attribute in reference.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() != MetadataAttribute ||
                    attribute.ConstructorArguments.Length != 2 ||
                    !Equals(attribute.ConstructorArguments[0].Value, MetadataKey) ||
                    attribute.ConstructorArguments[1].Value is not string text)
                {
                    continue;
                }
                var root = XElement.Parse(text);
                foreach (var asset in root.Elements("asset"))
                {
                    yield return ThemeAssetInfo.FromSemanticFacts(
                        $"{reference.Name}/{(string)asset.Attribute("path")!}",
                        asset.Elements("theme").Select(ReadTheme).ToArray());
                }
            }
        }
    }

    private static XElement WriteTheme(ThemeAssetSemanticThemeInfo theme) => new("theme",
        theme.ExportKey is { } key ? new XAttribute("key", key) : null,
        theme.XmlClass is { } type ? new XAttribute("class", type) : null,
        new XAttribute("replacement", theme.OverridesBaseTemplate),
        WriteReference("target", theme.TargetType),
        theme.BasedOn is { } basedOn ? WriteReference("base", basedOn) : null,
        theme.Templates.Select(template => new XElement("template", new XAttribute("index", template.Index),
            template.Markers.Select(marker => new XElement("marker", new XAttribute("class", marker.SelectorClass),
                new XAttribute("namespace", marker.XmlNamespace), new XAttribute("type", marker.TypeName),
                new XAttribute("node", marker.NodeId), new XAttribute("enabled", marker.IsStaticallyEnabled))))));

    private static XElement WriteReference(string kind, ThemeAssetTargetTypeReference reference) => new(kind,
        new XAttribute("value", reference.Value), reference.Namespaces.OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new XElement("xmlns", new XAttribute("prefix", pair.Key), new XAttribute("uri", pair.Value))));

    private static ThemeAssetSemanticThemeInfo ReadTheme(XElement theme) => new(
        ReadReference(theme.Element("target")!),
        theme.Element("base") is { } basedOn ? ReadReference(basedOn) : null,
        theme.Elements("template").Select(template => new ThemeAssetSemanticTemplateInfo((int)template.Attribute("index")!,
            template.Elements("marker").Select(marker => new ThemeAssetSemanticMarkerInfo(
                (string)marker.Attribute("class")!, (string)marker.Attribute("namespace")!,
                (string)marker.Attribute("type")!, (int)marker.Attribute("node")!, (bool)marker.Attribute("enabled")!)).ToArray())).ToArray(),
        (bool)theme.Attribute("replacement")!, (string?)theme.Attribute("class"), (string?)theme.Attribute("key"));

    private static ThemeAssetTargetTypeReference ReadReference(XElement value) => new(
        (string)value.Attribute("value")!, value.Elements("xmlns").ToDictionary(
            static element => (string)element.Attribute("prefix")!, static element => (string)element.Attribute("uri")!,
            StringComparer.Ordinal));
}
