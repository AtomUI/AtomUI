using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AtomUI.Generator;

internal sealed class ThemeAssetInfo
{
    private ThemeAssetInfo(
        string path,
        string assetPath,
        SourceText source,
        string fileName,
        IReadOnlyList<ThemeAssetSemanticThemeInfo> semanticThemes,
        bool isResourceDictionary,
        string? resourceClassName,
        string? controlThemeClassName,
        string? controlThemeTargetTypeName)
    {
        Path = path;
        AssetPath = assetPath;
        Source = source;
        FileName = fileName;
        SemanticThemes = semanticThemes;
        IsResourceDictionary = isResourceDictionary;
        ResourceClassName = resourceClassName;
        ControlThemeClassName = controlThemeClassName;
        ControlThemeTargetTypeName = controlThemeTargetTypeName;
    }

    internal string? ResourceClassName { get; }
    internal string Path { get; }
    internal string AssetPath { get; }
    internal SourceText Source { get; }
    internal string FileName { get; }
    internal IReadOnlyList<ThemeAssetSemanticThemeInfo> SemanticThemes { get; }
    internal bool IsResourceDictionary { get; }
    internal string? ControlThemeClassName { get; }
    internal string? ControlThemeTargetTypeName { get; }
    internal bool IsDefaultTypedControlTheme =>
        ControlThemeClassName is not null &&
        ControlThemeTargetTypeName is not null &&
        !ControlThemeTargetTypeName.StartsWith("Abstract", StringComparison.Ordinal) &&
        !ControlThemeTargetTypeName.StartsWith("Base", StringComparison.Ordinal) &&
        string.Equals(FileName, ControlThemeTargetTypeName + "Theme", StringComparison.Ordinal);
    internal bool HasGeneratedResourceWrapper => IsResourceDictionary || IsDefaultTypedControlTheme;

    internal static ThemeAssetInfo Create(
        AdditionalText text, string? projectDirectory, string? link,
        CancellationToken cancellationToken, out XElement? documentRoot)
    {
        documentRoot = null;
        var source = text.GetText(cancellationToken) ?? SourceText.From(string.Empty);
        var assetPath = NormalizeAssetPath(text.Path, projectDirectory, link);
        var fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        IReadOnlyList<ThemeAssetSemanticThemeInfo> semanticThemes =
            Array.Empty<ThemeAssetSemanticThemeInfo>();
        var isResourceDictionary = false;
        string? resourceClassName = null;
        string? controlThemeClassName = null;
        string? controlThemeTargetTypeName = null;

        try
        {
            var document = XDocument.Parse(source.ToString(), LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            var root = documentRoot = document.Root;
            XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
            isResourceDictionary = string.Equals(
                root?.Name.LocalName,
                "ResourceDictionary",
                StringComparison.Ordinal);
            // Other valid AXAML roots (for example Styles or UserControl) may live under
            // Themes/. They are not registration assets and must retain normal compilation.
            if (isResourceDictionary || root?.Name.LocalName == "ControlTheme")
            {
                resourceClassName = root?.Attribute(xamlNamespace + "Class")?.Value;
            }

            if (root is not null &&
                string.Equals(root.Name.LocalName, "ControlTheme", StringComparison.Ordinal))
            {
                controlThemeClassName = resourceClassName;
                controlThemeTargetTypeName = GetTypeName(
                    root.Attributes()
                        .FirstOrDefault(static attribute =>
                            string.Equals(attribute.Name.LocalName, "TargetType", StringComparison.Ordinal))
                        ?.Value);
            }
            if (root is not null)
            {
                semanticThemes = SemanticThemeAssetParser.Parse(root);
            }
        }
        catch
        {
            // Avalonia reports malformed AXAML. This generator consumes only successfully parsed structure.
        }

        return new ThemeAssetInfo(
            text.Path,
            assetPath,
            source,
            fileName,
            semanticThemes,
            isResourceDictionary,
            resourceClassName,
            controlThemeClassName,
            controlThemeTargetTypeName);
    }


    internal static bool IsAggregatePath(string path)
    {
        return System.IO.Path.GetFileNameWithoutExtension(path)
                             .EndsWith("Themes", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsThemeAssetPath(string path)
    {
        if (!path.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase) || IsAggregatePath(path))
        {
            return false;
        }

        var normalized = path.Replace('\\', '/').TrimStart('/');
        return normalized.StartsWith("Themes/", StringComparison.OrdinalIgnoreCase) ||
               normalized.IndexOf("/Themes/", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    internal static string GetGeneratedResourceClassName(string assetPath)
    {
        var hash = 14695981039346656037UL;
        foreach (var character in assetPath.Replace('\\', '/'))
        {
            hash ^= character;
            hash *= 1099511628211UL;
        }
        return $"GeneratedThemeAssetResource_{hash:X16}";
    }

    private static string? GetTypeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        var typeName = value!.Trim();
        if (typeName.StartsWith("{x:Type", StringComparison.Ordinal) &&
            typeName.EndsWith("}", StringComparison.Ordinal))
        {
            typeName = typeName.Substring("{x:Type".Length, typeName.Length - "{x:Type".Length - 1).Trim();
        }
        var separator = typeName.LastIndexOf(':');
        return separator >= 0 ? typeName.Substring(separator + 1) : typeName;
    }

    internal Location CreateLocation()
    {
        var span = new TextSpan(0, Source.Length);
        return Location.Create(Path, span, Source.Lines.GetLinePositionSpan(span));
    }

    internal static string NormalizeAssetPath(
        string path,
        string? projectDirectory,
        string? link)
    {
        if (!string.IsNullOrWhiteSpace(link))
        {
            return link!.Replace('\\', '/').TrimStart('/');
        }

        var normalized = path.Replace('\\', '/');
        if (!System.IO.Path.IsPathRooted(path))
        {
            return normalized.TrimStart('/');
        }

        if (!string.IsNullOrWhiteSpace(projectDirectory))
        {
            var normalizedProjectDirectory = projectDirectory!
                                             .Replace('\\', '/')
                                             .TrimEnd('/') + "/";
            if (normalized.StartsWith(
                    normalizedProjectDirectory,
                    StringComparison.OrdinalIgnoreCase))
            {
                return normalized.Substring(normalizedProjectDirectory.Length);
            }
        }

        var sourceMarker = normalized.IndexOf("/src/", StringComparison.OrdinalIgnoreCase);
        if (sourceMarker >= 0)
        {
            var projectStart = sourceMarker + 5;
            var relativeStart = normalized.IndexOf('/', projectStart);
            if (relativeStart >= 0 && relativeStart + 1 < normalized.Length)
            {
                return normalized.Substring(relativeStart + 1);
            }
        }

        return System.IO.Path.GetFileName(path);
    }
}

internal sealed class ThemeAssetTargetTypeReference
{
    private ThemeAssetTargetTypeReference(
        string value,
        IReadOnlyDictionary<string, string> namespaces)
    {
        Value = value;
        Namespaces = namespaces;
    }

    internal string Value { get; }
    internal IReadOnlyDictionary<string, string> Namespaces { get; }

    internal static ThemeAssetTargetTypeReference Create(XElement element, string value)
    {
        return new ThemeAssetTargetTypeReference(value, CollectNamespaces(element));
    }

    internal static ThemeAssetTargetTypeReference CreateFromElement(XElement element, XElement namespaceContext)
    {
        var namespaces = CollectNamespaces(namespaceContext);
        var elementNamespace = element.Name.NamespaceName;
        var prefix = namespaces.FirstOrDefault(pair =>
            pair.Value == elementNamespace && pair.Key.Length > 0).Key;
        var value = prefix is null
            ? element.Name.LocalName
            : $"{prefix}:{element.Name.LocalName}";
        return new ThemeAssetTargetTypeReference(value, namespaces);
    }

    private static Dictionary<string, string> CollectNamespaces(XElement element)
    {
        var namespaces = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var current in element.AncestorsAndSelf().Reverse())
        {
            foreach (var attribute in current.Attributes().Where(static attribute =>
                         attribute.IsNamespaceDeclaration))
            {
                var prefix = attribute.Name.LocalName == "xmlns"
                    ? string.Empty
                    : attribute.Name.LocalName;
                namespaces[prefix] = attribute.Value;
            }
        }
        return namespaces;
    }
}
