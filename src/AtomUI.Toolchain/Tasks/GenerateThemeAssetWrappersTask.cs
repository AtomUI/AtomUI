using System.Security;
using System.Text;
using System.Xml.Linq;
using AtomUI.SourceGeneration;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>
/// Generates a <c>ResourceDictionary</c> wrapper, with an internal code-behind class, for each theme asset of an
/// assembly, and writes the code-behind classes to one C# file. Each wrapper merges one dictionary by resource
/// URI: the asset itself, or, for the default typed <c>ControlTheme</c> of a control, a generated deferred
/// dictionary that instantiates the theme class, keyed by its target type.
/// </summary>
public sealed class GenerateThemeAssetWrappersTask : ITask
{
    /// <summary>
    /// The XAML assets to wrap. The logical resource path is the <c>Link</c> metadata, or the item spec when it is
    /// empty; the file is read from the item's full path.
    /// Only a <c>.axaml</c> file in a <c>Themes</c> directory is processed, never one named <c>*Themes.axaml</c>,
    /// and only when its root is a <c>ResourceDictionary</c> or the default typed <c>ControlTheme</c> of a
    /// control. Such a theme has an <c>x:Class</c>, a target type whose name starts with neither <c>Abstract</c>
    /// nor <c>Base</c>, and the file name <c>&lt;type name&gt;Theme.axaml</c>.
    /// </summary>
    [Required]
    public ITaskItem[] ThemeAssets { get; set; } = [];

    /// <summary>
    /// Directory that receives the generated <c>.axaml</c> files. It is created when it does not exist.
    /// </summary>
    [Required]
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Name of the assembly that owns the assets. It forms the <c>avares://</c> URIs of the merged sources and the
    /// namespace of the generated classes.
    /// </summary>
    [Required]
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Path of the C# file that declares an internal class per generated wrapper dictionary. The deferred
    /// dictionaries are merged only and get no class.
    /// </summary>
    [Required]
    public string GeneratedCodePath { get; set; } = string.Empty;

    /// <summary>
    /// The generated files: one wrapper per processed asset and, for a typed <c>ControlTheme</c>, its deferred
    /// dictionary. Each item's spec is the file path and its <c>Link</c> metadata is
    /// <c>AtomUI.Generated/&lt;class name&gt;.axaml</c>.
    /// </summary>
    [Output]
    public ITaskItem[] GeneratedAssets { get; set; } = [];

    /// <summary>
    /// Satisfies <see cref="ITask" />. This task logs nothing and never reads it.
    /// </summary>
    public IBuildEngine BuildEngine { get; set; } = null!;

    /// <summary>
    /// Satisfies <see cref="ITask" />. This task never reads it.
    /// </summary>
    public ITaskHost HostObject { get; set; } = null!;

    /// <summary>
    /// Generates the wrapper files into <see cref="OutputDirectory" /> and the code file at
    /// <see cref="GeneratedCodePath" />, collects them in <see cref="GeneratedAssets" />, and leaves files whose
    /// content is unchanged untouched.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the files are written. Assets that are skipped or cannot be loaded do not fail
    /// the task.
    /// </returns>
    public bool Execute()
    {
        var generated = new List<ITaskItem>();
        var generatedClassNames = new List<string>();
        Directory.CreateDirectory(OutputDirectory);

        foreach (var item in ThemeAssets)
        {
            var logicalPath = GetLogicalPath(item);
            if (!IsLoadableThemeAsset(logicalPath))
            {
                continue;
            }

            if (!TryLoadDocument(item, out var document) || document.Root is not { } root)
            {
                continue;
            }

            var isResourceDictionary = string.Equals(
                root.Name.LocalName,
                "ResourceDictionary",
                StringComparison.Ordinal);
            var isDefaultTypedControlTheme = TryGetDefaultTypedControlTheme(
                root,
                logicalPath,
                out var typedThemeClassName,
                out var typedThemeTargetType);
            if (!isResourceDictionary && !isDefaultTypedControlTheme)
            {
                continue;
            }

            var className = GetGeneratedResourceClassName(logicalPath);
            var generatedPath = Path.Combine(OutputDirectory, className + ".axaml");
            var generatedNamespace = GeneratedCodeNamespace.ForAssembly(AssemblyName);
            string content;
            if (isResourceDictionary)
            {
                content = CreateResourceDictionaryWrapper(
                    generatedNamespace,
                    className,
                    "avares://" + AssemblyName + "/" + logicalPath);
            }
            else
            {
                var deferredClassName = className + "_Deferred";
                var deferredPath = Path.Combine(OutputDirectory, deferredClassName + ".axaml");
                WriteIfChanged(
                    deferredPath,
                    CreateDeferredControlThemeWrapper(root, typedThemeClassName!, typedThemeTargetType!));

                var deferredOutput = new GeneratedTaskItem(deferredPath);
                deferredOutput.SetMetadata("Link", "AtomUI.Generated/" + deferredClassName + ".axaml");
                generated.Add(deferredOutput);

                content = CreateResourceDictionaryWrapper(
                    generatedNamespace,
                    className,
                    "avares://" + AssemblyName + "/AtomUI.Generated/" + deferredClassName + ".axaml");
            }

            WriteIfChanged(generatedPath, content);

            var output = new GeneratedTaskItem(generatedPath);
            output.SetMetadata("Link", "AtomUI.Generated/" + className + ".axaml");
            generated.Add(output);
            generatedClassNames.Add(className);
        }

        WriteIfChanged(GeneratedCodePath, CreateGeneratedCode(AssemblyName, generatedClassNames));
        GeneratedAssets = generated.ToArray();
        return true;
    }

    private static string GetLogicalPath(ITaskItem item)
    {
        var logicalPath = item.GetMetadata("Link");
        if (string.IsNullOrWhiteSpace(logicalPath))
        {
            logicalPath = item.ItemSpec;
        }

        return logicalPath.Replace('\\', '/').TrimStart('/');
    }

    private static bool IsLoadableThemeAsset(string logicalPath)
    {
        return logicalPath.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase) &&
               (logicalPath.StartsWith("Themes/", StringComparison.OrdinalIgnoreCase) ||
                logicalPath.IndexOf("/Themes/", StringComparison.OrdinalIgnoreCase) >= 0) &&
               !Path.GetFileNameWithoutExtension(logicalPath)
                    .EndsWith("Themes", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryLoadDocument(ITaskItem item, out XDocument document)
    {
        try
        {
            var path = item.GetMetadata("FullPath");
            if (string.IsNullOrWhiteSpace(path))
            {
                path = item.ItemSpec;
            }

            document = XDocument.Load(path, LoadOptions.None);
            return true;
        }
        catch
        {
            document = null!;
            return false;
        }
    }

    private static bool TryGetDefaultTypedControlTheme(
        XElement root,
        string logicalPath,
        out string? typedThemeClassName,
        out string? typedThemeTargetType)
    {
        typedThemeClassName = null;
        typedThemeTargetType = null;
        if (!string.Equals(root.Name.LocalName, "ControlTheme", StringComparison.Ordinal))
        {
            return false;
        }

        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        typedThemeClassName = root.Attribute(xamlNamespace + "Class")?.Value;
        typedThemeTargetType = root.Attributes()
                                   .FirstOrDefault(static attribute => string.Equals(
                                       attribute.Name.LocalName,
                                       "TargetType",
                                       StringComparison.Ordinal))?.Value;
        if (string.IsNullOrWhiteSpace(typedThemeClassName) ||
            string.IsNullOrWhiteSpace(typedThemeTargetType))
        {
            return false;
        }

        var targetTypeName = GetTypeName(typedThemeTargetType!);
        return !targetTypeName.StartsWith("Abstract", StringComparison.Ordinal) &&
               !targetTypeName.StartsWith("Base", StringComparison.Ordinal) &&
               string.Equals(
                   Path.GetFileNameWithoutExtension(logicalPath),
                   targetTypeName + "Theme",
                   StringComparison.Ordinal);
    }

    private static string CreateResourceDictionaryWrapper(
        string generatedNamespace,
        string className,
        string sourceUri)
    {
        return
            "<ResourceDictionary xmlns=\"https://github.com/avaloniaui\"\n" +
            "                    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"\n" +
            "                    x:Class=\"" + generatedNamespace + "." + className + "\"\n" +
            "                    x:ClassModifier=\"internal\">\n" +
            "    <ResourceDictionary.MergedDictionaries>\n" +
            "        <ResourceInclude Source=\"" + SecurityElement.Escape(sourceUri) + "\" />\n" +
            "    </ResourceDictionary.MergedDictionaries>\n" +
            "</ResourceDictionary>\n";
    }

    private static string CreateDeferredControlThemeWrapper(
        XElement root,
        string typedThemeClassName,
        string typedThemeTargetType)
    {
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        var deferredWrapper = new XElement(root.Name.Namespace + "ResourceDictionary");
        foreach (var namespaceAttribute in root.Attributes()
                                               .Where(static attribute => attribute.IsNamespaceDeclaration))
        {
            deferredWrapper.Add(new XAttribute(namespaceAttribute));
        }

        var classSeparator = typedThemeClassName.LastIndexOf('.');
        var typedThemeNamespace = classSeparator >= 0
            ? typedThemeClassName.Substring(0, classSeparator)
            : string.Empty;
        var typedThemeTypeName = classSeparator >= 0
            ? typedThemeClassName.Substring(classSeparator + 1)
            : typedThemeClassName;
        var themePrefix = "generatedThemeAsset";
        while (deferredWrapper.GetNamespaceOfPrefix(themePrefix) != null)
        {
            themePrefix += "_";
        }

        XNamespace typedThemeNamespaceUri = "using:" + typedThemeNamespace;
        deferredWrapper.SetAttributeValue(
            XNamespace.Xmlns + themePrefix,
            typedThemeNamespaceUri.NamespaceName);

        var targetTypeReference = typedThemeTargetType.Trim();
        if (!targetTypeReference.StartsWith("{x:Type", StringComparison.Ordinal))
        {
            targetTypeReference = "{x:Type " + targetTypeReference + "}";
        }

        deferredWrapper.Add(new XElement(
            typedThemeNamespaceUri + typedThemeTypeName,
            new XAttribute(xamlNamespace + "Key", targetTypeReference),
            new XAttribute("TargetType", typedThemeTargetType)));
        return new XDocument(deferredWrapper).ToString() + Environment.NewLine;
    }

    private static string CreateGeneratedCode(string assemblyName, IEnumerable<string> generatedClassNames)
    {
        var code = new StringBuilder();
        code.AppendLine("// <auto-generated />");
        code.Append("namespace ").Append(GeneratedCodeNamespace.ForAssembly(assemblyName)).AppendLine(";");
        code.AppendLine();
        foreach (var className in generatedClassNames.OrderBy(static name => name, StringComparer.Ordinal))
        {
            code.Append("internal sealed class ").Append(className)
                .AppendLine(" : global::Avalonia.Controls.ResourceDictionary");
            code.AppendLine("{");
            code.Append("    internal ").Append(className).AppendLine("()");
            code.AppendLine("    {");
            code.AppendLine("        global::Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);");
            code.AppendLine("    }");
            code.AppendLine("}");
            code.AppendLine();
        }

        return code.ToString();
    }

    private static void WriteIfChanged(string path, string content)
    {
        if (!File.Exists(path) || !string.Equals(File.ReadAllText(path), content, StringComparison.Ordinal))
        {
            File.WriteAllText(path, content, new UTF8Encoding(false));
        }
    }

    private static string GetTypeName(string value)
    {
        var typeName = value.Trim();
        if (typeName.StartsWith("{x:Type", StringComparison.Ordinal) &&
            typeName.EndsWith("}", StringComparison.Ordinal))
        {
            typeName = typeName.Substring(
                "{x:Type".Length,
                typeName.Length - "{x:Type".Length - 1).Trim();
        }

        var separator = typeName.LastIndexOf(':');
        return separator >= 0 ? typeName.Substring(separator + 1) : typeName;
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
}
