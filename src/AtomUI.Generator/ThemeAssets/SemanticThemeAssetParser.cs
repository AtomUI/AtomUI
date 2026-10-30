using System.Xml.Linq;

namespace AtomUI.Generator;

internal static class SemanticThemeAssetParser
{
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    internal static IReadOnlyList<ThemeAssetSemanticThemeInfo> Parse(XElement root)
    {
        var themes = new List<ThemeAssetSemanticThemeInfo>();
        var nextTemplateIndex = 0;
        foreach (var controlTheme in root.DescendantsAndSelf().Where(static element =>
                     string.Equals(element.Name.LocalName, "ControlTheme", StringComparison.Ordinal)))
        {
            var targetTypeAttribute = controlTheme.Attributes().FirstOrDefault(static attribute =>
                string.Equals(attribute.Name.LocalName, "TargetType", StringComparison.Ordinal));
            if (targetTypeAttribute is null)
            {
                continue;
            }

            var templates = new List<ThemeAssetSemanticTemplateInfo>();
            var overridesBaseTemplate = false;
            foreach (var templateSetter in GetTemplateSetters(controlTheme))
            {
                var isConditional = templateSetter.Ancestors()
                                                  .TakeWhile(ancestor => !ReferenceEquals(
                                                      ancestor,
                                                      controlTheme))
                                                  .Any(static ancestor => string.Equals(
                                                      ancestor.Name.LocalName,
                                                      "Style",
                                                      StringComparison.Ordinal));
                overridesBaseTemplate |= !isConditional;
                foreach (var template in templateSetter.Descendants().Where(static element =>
                             string.Equals(
                                 element.Name.LocalName,
                                 "ControlTemplate",
                                 StringComparison.Ordinal)))
                {
                    var nearestSetter = template.Ancestors().FirstOrDefault(IsTemplateSetter);
                    if (!ReferenceEquals(nearestSetter, templateSetter))
                    {
                        continue;
                    }

                    templates.Add(new ThemeAssetSemanticTemplateInfo(
                        ++nextTemplateIndex,
                        ParseMarkers(template)));
                }
            }

            themes.Add(new ThemeAssetSemanticThemeInfo(
                ThemeAssetTargetTypeReference.Create(controlTheme, targetTypeAttribute.Value),
                CreateBasedOnReference(controlTheme),
                templates,
                overridesBaseTemplate,
                controlTheme.Attribute(XamlNamespace + "Class")?.Value));
        }
        return themes;
    }

    private static IEnumerable<XElement> GetTemplateSetters(XElement controlTheme)
    {
        return controlTheme.Descendants()
                           .Where(IsTemplateSetter)
                           .Where(setter => ReferenceEquals(
                               setter.Ancestors().FirstOrDefault(static ancestor => string.Equals(
                                   ancestor.Name.LocalName,
                                   "ControlTheme",
                                   StringComparison.Ordinal)),
                               controlTheme));
    }

    private static IReadOnlyList<ThemeAssetSemanticMarkerInfo> ParseMarkers(XElement template)
    {
        var markers = new List<ThemeAssetSemanticMarkerInfo>();
        var nodeId = 0;
        foreach (var element in template.Descendants())
        {
            var currentNodeId = nodeId++;
            HashSet<string>? validSelectorClasses = null;
            HashSet<string>? invalidSelectorClasses = null;
            foreach (var attribute in element.Attributes())
            {
                if (string.Equals(attribute.Name.LocalName, "Classes", StringComparison.Ordinal))
                {
                    foreach (var literalClass in attribute.Value.Split(
                                 (char[]?)null,
                                 StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (literalClass.StartsWith("semantic-", StringComparison.Ordinal))
                        {
                            (validSelectorClasses ??= new HashSet<string>(StringComparer.Ordinal))
                                .Add(literalClass);
                        }
                    }
                    continue;
                }

                const string classPropertyPrefix = "Classes.";
                if (!attribute.Name.LocalName.StartsWith(classPropertyPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                var selectorClass = attribute.Name.LocalName.Substring(classPropertyPrefix.Length);
                if (!selectorClass.StartsWith("semantic-", StringComparison.Ordinal))
                {
                    continue;
                }

                if (bool.TryParse(attribute.Value.Trim(), out var isEnabled) && isEnabled)
                {
                    (validSelectorClasses ??= new HashSet<string>(StringComparer.Ordinal)).Add(selectorClass);
                }
                else
                {
                    (invalidSelectorClasses ??= new HashSet<string>(StringComparer.Ordinal)).Add(selectorClass);
                }
            }

            AddMarkers(markers, validSelectorClasses, element, currentNodeId, isStaticallyEnabled: true);
            AddMarkers(markers, invalidSelectorClasses, element, currentNodeId, isStaticallyEnabled: false);
        }
        return markers;
    }

    private static void AddMarkers(
        ICollection<ThemeAssetSemanticMarkerInfo> markers,
        HashSet<string>? selectorClasses,
        XElement element,
        int nodeId,
        bool isStaticallyEnabled)
    {
        if (selectorClasses is null)
        {
            return;
        }

        foreach (var selectorClass in selectorClasses.OrderBy(static value => value, StringComparer.Ordinal))
        {
            markers.Add(new ThemeAssetSemanticMarkerInfo(
                selectorClass,
                element.Name.NamespaceName,
                element.Name.LocalName,
                nodeId,
                isStaticallyEnabled));
        }
    }

    private static ThemeAssetTargetTypeReference? CreateBasedOnReference(XElement controlTheme)
    {
        var basedOn = controlTheme.Attributes().FirstOrDefault(static attribute =>
            string.Equals(attribute.Name.LocalName, "BasedOn", StringComparison.Ordinal));
        if (basedOn is not null)
        {
            const string prefix = "{StaticResource ";
            var value = basedOn.Value.Trim();
            if (value.StartsWith(prefix, StringComparison.Ordinal) &&
                value.EndsWith("}", StringComparison.Ordinal))
            {
                var resourceKey = value.Substring(prefix.Length, value.Length - prefix.Length - 1).Trim();
                if (resourceKey.StartsWith("{x:Type", StringComparison.Ordinal) &&
                    resourceKey.EndsWith("}", StringComparison.Ordinal))
                {
                    return ThemeAssetTargetTypeReference.Create(controlTheme, resourceKey);
                }
            }

            return null;
        }

        var basedOnElement = controlTheme.Elements().FirstOrDefault(static element =>
            string.Equals(element.Name.LocalName, "ControlTheme.BasedOn", StringComparison.Ordinal));
        var referencedTheme = basedOnElement?.Elements().FirstOrDefault();
        return referencedTheme is null
            ? null
            : ThemeAssetTargetTypeReference.CreateFromElement(referencedTheme, controlTheme);
    }

    private static bool IsTemplateSetter(XElement element)
    {
        if (!string.Equals(element.Name.LocalName, "Setter", StringComparison.Ordinal))
        {
            return false;
        }

        var property = element.Attributes().FirstOrDefault(static attribute =>
            string.Equals(attribute.Name.LocalName, "Property", StringComparison.Ordinal));
        return string.Equals(property?.Value, "Template", StringComparison.Ordinal);
    }
}
