using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace AtomUI.Toolkits.GalleryBase.Generator.ShowCaseSource;

/// <summary>
/// Source generator that builds the Gallery showcase snippet catalog (<c>ShowCaseCodeSnippetCatalog.g.cs</c>)
/// from the showcase AXAML files and the C# files that belong to them.
/// </summary>
/// <remarks>
///     <para>
///     The inputs are the additional files of the compiling project. The generator uses a file only when its
///     path ends with <c>.axaml</c> (showcase view) or <c>.axaml.cs</c> (code-behind), or when it is a
///     <c>.cs</c> file in a <c>viewmodels</c> directory. The path is the <c>GallerySourceOriginalPath</c>
///     metadata of the additional file when that is set, otherwise the path of the file itself.
///     </para>
///     <para>
///     Each <c>ShowCaseItem</c> with content yields one catalog group. It holds the AXAML snippet of the item
///     and, when the corresponding source file is found, a code-behind snippet with the event handlers the item
///     references and a view model snippet with the members the item binds to or the code-behind snippet refers
///     to. A <c>ShowCasePanel</c> without a <c>Name</c> or <c>x:Name</c> attribute is skipped and reported as
///     warning ATOMUIGEN101. An AXAML file that is not well-formed or declares no <c>x:Class</c> contributes
///     nothing.
///     </para>
///     <para>
///     The catalog is generated in the namespace <c>{RootNamespace}.Generated</c>, formed from the
///     <c>build_property.RootNamespace</c> option, or in <c>Gallery.Generated</c> when that option is not set.
///     </para>
/// </remarks>
[Generator]
public sealed class ShowCaseCodeSnippetCatalogGenerator : IIncrementalGenerator
{
    private const string CatalogFileName = "ShowCaseCodeSnippetCatalog.g.cs";
    private const string DefaultNamespace = "Gallery.Generated";
    private const string GallerySourceOriginalPathMetadataKey =
        "build_metadata.AdditionalFiles.GallerySourceOriginalPath";

#pragma warning disable RS2008
    private static readonly DiagnosticDescriptor PanelMissingNameDescriptor = new(
        "ATOMUIGEN101",
        "ShowCasePanel source key is missing",
        "ShowCasePanel in '{0}' must set Name to generate default source snippets",
        "Generator",
        DiagnosticSeverity.Warning,
        true);
#pragma warning restore RS2008

    /// <summary>
    /// Registers the pipeline that collects the showcase source files from the additional files and, in a source
    /// output step, adds the snippet catalog to the compilation.
    /// </summary>
    /// <param name="context">
    /// The context supplied by the compiler host. It provides the additional files with their metadata and the
    /// <c>build_property.RootNamespace</c> and <c>build_property.ProjectDir</c> global options, which select the
    /// catalog namespace and the base directory of the stored source paths.
    /// </param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var sourceFiles = context.AdditionalTextsProvider
                                 .Combine(context.AnalyzerConfigOptionsProvider)
                                 .Select(static (combined, cancellationToken) =>
                                 {
                                     var sourcePath = ResolveAdditionalSourcePath(combined.Left, combined.Right);
                                     if (!AdditionalSourceFile.IsSupportedPath(sourcePath))
                                     {
                                         return AdditionalSourceFile.CreateIgnored(sourcePath);
                                     }

                                     var text = combined.Left.GetText(cancellationToken);
                                     return AdditionalSourceFile.Create(sourcePath, text?.ToString() ?? string.Empty);
                                 })
                                 .Where(static file => file.Kind != AdditionalSourceFileKind.Ignored)
                                 .Collect();

        var optionsProvider = context.AnalyzerConfigOptionsProvider.Select(static (options, _) =>
            new GeneratorOptions(
                GetGlobalOption(options, "build_property.RootNamespace"),
                GetGlobalOption(options, "build_property.ProjectDir")));

        context.RegisterSourceOutput(sourceFiles.Combine(optionsProvider), static (context, combined) =>
        {
            var groups = new List<GeneratedSnippetGroup>();
            var sourceIndex = new SourceFileIndex(combined.Left, combined.Right);
            foreach (var file in combined.Left)
            {
                if (file.Kind != AdditionalSourceFileKind.Axaml ||
                    string.IsNullOrWhiteSpace(file.Text))
                {
                    continue;
                }

                ExtractFile(file, combined.Right, context, sourceIndex, groups);
            }

            var rootNamespace = combined.Right.RootNamespace;
            var outputNamespace = string.IsNullOrWhiteSpace(rootNamespace)
                ? DefaultNamespace
                : $"{rootNamespace}.Generated";

            context.AddSource(CatalogFileName, SourceText.From(WriteCatalog(outputNamespace, groups), Encoding.UTF8));
        });
    }

    private static string ResolveAdditionalSourcePath(AdditionalText file, AnalyzerConfigOptionsProvider optionsProvider)
    {
        var options = optionsProvider.GetOptions(file);
        return options.TryGetValue(GallerySourceOriginalPathMetadataKey, out var sourcePath) &&
               !string.IsNullOrWhiteSpace(sourcePath)
            ? sourcePath
            : file.Path;
    }

    private static void ExtractFile(AdditionalSourceFile file,
                                    GeneratorOptions options,
                                    SourceProductionContext context,
                                    SourceFileIndex sourceIndex,
                                    List<GeneratedSnippetGroup> groups)
    {
        var sourceText = SourceText.From(file.Text, Encoding.UTF8);
        XDocument document;
        try
        {
            document = XDocument.Parse(
                file.Text,
                LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        }
        catch (XmlException)
        {
            return;
        }

        var root = document.Root;
        if (root is null)
        {
            return;
        }

        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        var viewTypeName = root.Attribute(xamlNamespace + "Class")?.Value;
        if (string.IsNullOrWhiteSpace(viewTypeName))
        {
            return;
        }

        foreach (var panel in root.Descendants().Where(static element => element.Name.LocalName == "ShowCasePanel"))
        {
            var panelKey = panel.Attribute("Name")?.Value
                           ?? panel.Attribute(xamlNamespace + "Name")?.Value;
            if (string.IsNullOrWhiteSpace(panelKey))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    PanelMissingNameDescriptor,
                    CreateLocation(file.Path, sourceText, panel),
                    MakeRelativePath(file.Path, options.ProjectDir)));
                continue;
            }

            var itemIndex = 0;
            foreach (var item in panel.Elements().Where(static element => element.Name.LocalName == "ShowCaseItem"))
            {
                if (TryCreateGroup(file.Path,
                                   sourceText,
                                   viewTypeName!,
                                   panelKey!,
                                   itemIndex,
                                   item,
                                   options,
                                   sourceIndex,
                                   out var group))
                {
                    groups.Add(group);
                }

                itemIndex++;
            }
        }
    }

    private static bool TryCreateGroup(string path,
                                       SourceText sourceText,
                                       string viewTypeName,
                                       string panelKey,
                                       int itemIndex,
                                       XElement item,
                                       GeneratorOptions options,
                                       SourceFileIndex sourceIndex,
                                       out GeneratedSnippetGroup group)
    {
        group = default;
        var sourceKey = item.Attribute("SourceKey")?.Value;
        var title = item.Attribute("Title")?.Value;
        var contentElements = GetSnippetContentElements(item).ToArray();
        if (contentElements.Length == 0)
        {
            return false;
        }

        var snippetText = ExtractSnippetText(sourceText, contentElements);
        if (string.IsNullOrWhiteSpace(snippetText))
        {
            return false;
        }

        var startLine = GetLineNumber(contentElements[0]);
        var endLine = startLine + snippetText.Count(static ch => ch == '\n');
        var relativePath = MakeRelativePath(path, options.ProjectDir);
        var snippets = new List<GeneratedSnippet>
        {
            new("AXAML", "axaml", snippetText, relativePath, startLine, endLine)
        };

        var references = AnalyzeItemReferences(item, contentElements);
        var viewTypeSimpleName = GetSimpleTypeName(viewTypeName);
        ParsedCSharpSourceFile? codeBehindFile = sourceIndex.TryGetCodeBehindFile(path, viewTypeSimpleName);
        var viewModelTypeName = references.ViewModelTypeName
                                ?? TryInferViewModelTypeName(codeBehindFile, viewTypeSimpleName)
                                ?? TryCreateConventionalViewModelTypeName(viewTypeSimpleName);
        var viewModelMembers = new HashSet<string>(references.BindingMemberNames, StringComparer.Ordinal);

        if (codeBehindFile is not null &&
            TryCreateCSharpSnippet(
                codeBehindFile,
                viewTypeSimpleName,
                references.EventHandlerNames,
                "Code-behind",
                out var codeBehindSnippet,
                out var codeBehindReferencedNames))
        {
            snippets.Add(codeBehindSnippet);
            foreach (var referencedName in codeBehindReferencedNames)
            {
                viewModelMembers.Add(referencedName);
            }
        }

        if (!string.IsNullOrWhiteSpace(viewModelTypeName) &&
            viewModelMembers.Count > 0 &&
            sourceIndex.TryGetViewModelFile(viewModelTypeName!, out var viewModelFile) &&
            TryCreateCSharpSnippet(
                viewModelFile,
                viewModelTypeName!,
                viewModelMembers,
                "ViewModel",
                out var viewModelSnippet,
                out _))
        {
            snippets.Add(viewModelSnippet);
        }

        group = new GeneratedSnippetGroup(
            viewTypeName,
            panelKey,
            itemIndex,
            string.IsNullOrWhiteSpace(sourceKey) ? null : sourceKey,
            string.IsNullOrWhiteSpace(title) ? "Source" : title!,
            snippets);
        return true;
    }

    private static IEnumerable<XElement> GetSnippetContentElements(XElement item)
    {
        var deferredTemplate = item.Elements()
                                   .FirstOrDefault(static element =>
                                       element.Name.LocalName == "ShowCaseItem.DeferredContentTemplate");
        var dataTemplate = deferredTemplate?.Elements()
                                           .FirstOrDefault(static element => element.Name.LocalName == "DataTemplate");
        if (dataTemplate is not null)
        {
            return dataTemplate.Elements();
        }

        return item.Elements()
                   .Where(static element => !element.Name.LocalName.StartsWith("ShowCaseItem.", StringComparison.Ordinal));
    }

    private static ItemReferences AnalyzeItemReferences(XElement item, IReadOnlyList<XElement> contentElements)
    {
        var eventHandlerNames = new HashSet<string>(StringComparer.Ordinal);
        var bindingMemberNames = new HashSet<string>(StringComparer.Ordinal);
        string? viewModelTypeName = null;

        foreach (var element in item.Descendants())
        {
            foreach (var attribute in element.Attributes())
            {
                if (attribute.Name.LocalName == "DataType")
                {
                    viewModelTypeName ??= ExtractSimpleTypeName(attribute.Value);
                }
            }
        }

        foreach (var element in contentElements.SelectMany(GetSelfAndDescendants))
        {
            foreach (var attribute in element.Attributes())
            {
                if (TryGetBindingRoot(attribute.Value, out var bindingRoot))
                {
                    bindingMemberNames.Add(bindingRoot);
                }

                if (LooksLikeEventHandlerAttribute(attribute))
                {
                    eventHandlerNames.Add(attribute.Value.Trim());
                }
            }
        }

        return new ItemReferences(eventHandlerNames, bindingMemberNames, viewModelTypeName);
    }

    private static IEnumerable<XElement> GetSelfAndDescendants(XElement element)
    {
        yield return element;
        foreach (var descendant in element.Descendants())
        {
            yield return descendant;
        }
    }

    private static bool TryGetBindingRoot(string value, out string bindingRoot)
    {
        bindingRoot = string.Empty;
        var text = value.Trim();
        if (!text.StartsWith("{Binding", StringComparison.Ordinal) &&
            !text.StartsWith("{CompiledBinding", StringComparison.Ordinal))
        {
            return false;
        }

        if (!text.EndsWith("}", StringComparison.Ordinal))
        {
            return false;
        }

        text = text.Substring(1, text.Length - 2).Trim();
        string remainder;
        if (text.StartsWith("CompiledBinding", StringComparison.Ordinal))
        {
            remainder = text.Substring("CompiledBinding".Length).Trim();
        }
        else if (text.StartsWith("Binding", StringComparison.Ordinal))
        {
            remainder = text.Substring("Binding".Length).Trim();
        }
        else
        {
            return false;
        }

        if (remainder.Length == 0)
        {
            return false;
        }

        var separatorIndex = FindBindingPathSeparator(remainder);
        var path = separatorIndex >= 0 ? remainder.Substring(0, separatorIndex).Trim() : remainder.Trim();
        const string pathPrefix = "Path=";
        if (path.StartsWith(pathPrefix, StringComparison.Ordinal))
        {
            path = path.Substring(pathPrefix.Length).Trim();
        }

        if (path.Length == 0 ||
            path == "." ||
            path.StartsWith("$", StringComparison.Ordinal))
        {
            return false;
        }

        var rootEnd = path.IndexOfAny(new[] { '.', '[', '/', '(', ')' });
        bindingRoot = rootEnd >= 0 ? path.Substring(0, rootEnd) : path;
        return IsIdentifier(bindingRoot);
    }

    private static int FindBindingPathSeparator(string text)
    {
        var commaIndex = text.IndexOf(',');
        var spaceIndex = text.IndexOf(' ');
        if (commaIndex < 0)
        {
            return spaceIndex;
        }

        if (spaceIndex < 0)
        {
            return commaIndex;
        }

        return Math.Min(commaIndex, spaceIndex);
    }

    private static bool LooksLikeEventHandlerAttribute(XAttribute attribute)
    {
        var attributeName = attribute.Name.LocalName;
        var value = attribute.Value.Trim();
        if (!IsIdentifier(value))
        {
            return false;
        }

        return attributeName == "Click" ||
               attributeName == "Loaded" ||
               attributeName == "Unloaded" ||
               attributeName == "AttachedToVisualTree" ||
               attributeName == "DetachedFromVisualTree" ||
               attributeName == "KeyDown" ||
               attributeName == "KeyUp" ||
               attributeName.EndsWith("Changed", StringComparison.Ordinal) ||
               attributeName.EndsWith("Changing", StringComparison.Ordinal) ||
               attributeName.EndsWith("Click", StringComparison.Ordinal) ||
               attributeName.EndsWith("Clicked", StringComparison.Ordinal) ||
               attributeName.EndsWith("Request", StringComparison.Ordinal) ||
               attributeName.EndsWith("Requested", StringComparison.Ordinal) ||
               attributeName.EndsWith("Opened", StringComparison.Ordinal) ||
               attributeName.EndsWith("Closed", StringComparison.Ordinal) ||
               attributeName.StartsWith("Pointer", StringComparison.Ordinal);
    }

    private static bool IsIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var first = value[0];
        if (first != '_' && !char.IsLetter(first))
        {
            return false;
        }

        for (var i = 1; i < value.Length; i++)
        {
            var ch = value[i];
            if (ch != '_' && !char.IsLetterOrDigit(ch))
            {
                return false;
            }
        }

        return true;
    }

    private static string? ExtractSimpleTypeName(string value)
    {
        var typeName = value.Trim();
        const string xTypePrefix = "{x:Type ";
        if (typeName.StartsWith(xTypePrefix, StringComparison.Ordinal) &&
            typeName.EndsWith("}", StringComparison.Ordinal))
        {
            typeName = typeName.Substring(xTypePrefix.Length, typeName.Length - xTypePrefix.Length - 1);
        }

        var colonIndex = typeName.LastIndexOf(':');
        if (colonIndex >= 0)
        {
            typeName = typeName.Substring(colonIndex + 1);
        }

        var dotIndex = typeName.LastIndexOf('.');
        if (dotIndex >= 0)
        {
            typeName = typeName.Substring(dotIndex + 1);
        }

        return IsIdentifier(typeName) ? typeName : null;
    }

    private static string GetSimpleTypeName(string typeName)
    {
        var dotIndex = typeName.LastIndexOf('.');
        return dotIndex >= 0 ? typeName.Substring(dotIndex + 1) : typeName;
    }

    private static string? TryCreateConventionalViewModelTypeName(string viewTypeSimpleName)
    {
        const string suffix = "ShowCase";
        if (!viewTypeSimpleName.EndsWith(suffix, StringComparison.Ordinal))
        {
            return null;
        }

        return viewTypeSimpleName.Substring(0, viewTypeSimpleName.Length - suffix.Length) + "ViewModel";
    }

    private static string ExtractSnippetText(SourceText sourceText, IReadOnlyList<XElement> elements)
    {
        var firstLine = GetLineNumber(elements[0]);
        var lastLine = FindElementEndLine(sourceText, elements[elements.Count - 1]);
        if (firstLine <= 0 || lastLine < firstLine)
        {
            return string.Empty;
        }

        var lines = new List<string>();
        for (var index = firstLine - 1; index <= lastLine - 1 && index < sourceText.Lines.Count; index++)
        {
            lines.Add(sourceText.Lines[index].ToString());
        }

        return TrimCommonIndent(lines);
    }

    private static int FindElementEndLine(SourceText sourceText, XElement element)
    {
        var startLine = GetLineNumber(element);
        if (startLine <= 0 || startLine > sourceText.Lines.Count)
        {
            return startLine;
        }

        var text = sourceText.ToString();
        var searchPosition = GetElementStartPosition(sourceText, element, startLine);
        var localName = element.Name.LocalName;
        var depth = 0;
        while (searchPosition < text.Length)
        {
            var tagStart = text.IndexOf('<', searchPosition);
            if (tagStart < 0)
            {
                break;
            }

            if (TrySkipSpecialTag(text, tagStart, out var afterSpecialTag))
            {
                searchPosition = afterSpecialTag;
                continue;
            }

            var nameStart = tagStart + 1;
            var isClosingTag = nameStart < text.Length && text[nameStart] == '/';
            if (isClosingTag)
            {
                nameStart++;
            }

            if (!TryReadTagLocalName(text, nameStart, out var tagLocalName))
            {
                searchPosition = tagStart + 1;
                continue;
            }

            var tagEnd = FindTagEnd(text, nameStart);
            if (tagEnd < 0)
            {
                break;
            }

            if (string.Equals(tagLocalName, localName, StringComparison.Ordinal))
            {
                if (isClosingTag)
                {
                    if (depth > 0)
                    {
                        depth--;
                        if (depth == 0)
                        {
                            return GetLineNumber(sourceText, tagEnd);
                        }
                    }
                }
                else if (IsSelfClosingTag(text, tagEnd))
                {
                    if (depth == 0)
                    {
                        return GetLineNumber(sourceText, tagEnd);
                    }
                }
                else
                {
                    depth++;
                }
            }

            searchPosition = tagEnd + 1;
        }

        return startLine;
    }

    private static int GetElementStartPosition(SourceText sourceText, XElement element, int startLine)
    {
        var line = sourceText.Lines[startLine - 1];
        var offset = 0;
        if (element is IXmlLineInfo lineInfo && lineInfo.HasLineInfo())
        {
            offset = Math.Max(lineInfo.LinePosition - 1, 0);
        }

        var lineText = line.ToString();
        if (lineText.Length == 0)
        {
            return line.Start;
        }

        var scanStart = Math.Min(offset, lineText.Length - 1);
        var tagStart = lineText.LastIndexOf('<', scanStart);
        return tagStart >= 0
            ? line.Start + tagStart
            : Math.Min(line.Start + offset, sourceText.Length);
    }

    private static bool TrySkipSpecialTag(string text, int tagStart, out int nextPosition)
    {
        nextPosition = tagStart;
        if (tagStart + 1 >= text.Length)
        {
            return false;
        }

        if (MatchesAt(text, tagStart, "<!--"))
        {
            var commentEnd = text.IndexOf("-->", tagStart + 4, StringComparison.Ordinal);
            nextPosition = commentEnd < 0 ? text.Length : commentEnd + 3;
            return true;
        }

        if (MatchesAt(text, tagStart, "<![CDATA["))
        {
            var cdataEnd = text.IndexOf("]]>", tagStart + 9, StringComparison.Ordinal);
            nextPosition = cdataEnd < 0 ? text.Length : cdataEnd + 3;
            return true;
        }

        if (text[tagStart + 1] == '!' || text[tagStart + 1] == '?')
        {
            var endToken = text[tagStart + 1] == '?' ? "?>" : ">";
            var specialEnd = text.IndexOf(endToken, tagStart + 2, StringComparison.Ordinal);
            nextPosition = specialEnd < 0 ? text.Length : specialEnd + endToken.Length;
            return true;
        }

        return false;
    }

    private static bool TryReadTagLocalName(string text, int nameStart, out string localName)
    {
        localName = string.Empty;
        var nameEnd = nameStart;
        while (nameEnd < text.Length && IsXmlNameChar(text[nameEnd]))
        {
            nameEnd++;
        }

        if (nameEnd == nameStart)
        {
            return false;
        }

        var colonIndex = text.LastIndexOf(':', nameEnd - 1, nameEnd - nameStart);
        var localNameStart = colonIndex >= nameStart ? colonIndex + 1 : nameStart;
        localName = text.Substring(localNameStart, nameEnd - localNameStart);
        return localName.Length > 0;
    }

    private static int FindTagEnd(string text, int searchStart)
    {
        var quote = '\0';
        for (var index = searchStart; index < text.Length; index++)
        {
            var ch = text[index];
            if (quote != '\0')
            {
                if (ch == quote)
                {
                    quote = '\0';
                }

                continue;
            }

            if (ch == '"' || ch == '\'')
            {
                quote = ch;
            }
            else if (ch == '>')
            {
                return index;
            }
        }

        return -1;
    }

    private static bool IsSelfClosingTag(string text, int tagEnd)
    {
        for (var index = tagEnd - 1; index >= 0; index--)
        {
            var ch = text[index];
            if (char.IsWhiteSpace(ch))
            {
                continue;
            }

            return ch == '/';
        }

        return false;
    }

    private static int GetLineNumber(SourceText sourceText, int position)
    {
        return sourceText.Lines.GetLinePosition(position).Line + 1;
    }

    private static bool MatchesAt(string text, int index, string value)
    {
        return index >= 0 &&
               index + value.Length <= text.Length &&
               string.CompareOrdinal(text, index, value, 0, value.Length) == 0;
    }

    private static bool IsXmlNameChar(char ch)
    {
        return char.IsLetterOrDigit(ch) ||
               ch == '_' ||
               ch == '-' ||
               ch == '.' ||
               ch == ':';
    }

    private static int GetLineNumber(XObject node)
    {
        return node is IXmlLineInfo lineInfo && lineInfo.HasLineInfo()
            ? lineInfo.LineNumber
            : 0;
    }

    private static string TrimCommonIndent(IReadOnlyList<string> rawLines)
    {
        var start = 0;
        var end = rawLines.Count - 1;
        while (start <= end && string.IsNullOrWhiteSpace(rawLines[start]))
        {
            start++;
        }

        while (end >= start && string.IsNullOrWhiteSpace(rawLines[end]))
        {
            end--;
        }

        if (start > end)
        {
            return string.Empty;
        }

        var minIndent = rawLines.Skip(start)
                                .Take(end - start + 1)
                                .Where(static line => !string.IsNullOrWhiteSpace(line))
                                .Select(CountIndent)
                                .DefaultIfEmpty(0)
                                .Min();

        var builder = new StringBuilder();
        for (var index = start; index <= end; index++)
        {
            var line = rawLines[index];
            builder.Append(line.Length >= minIndent ? line.Substring(minIndent) : line.TrimStart());
            if (index < end)
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    private static int CountIndent(string line)
    {
        var count = 0;
        while (count < line.Length && char.IsWhiteSpace(line[count]))
        {
            count++;
        }

        return count;
    }

    private static string? TryInferViewModelTypeName(ParsedCSharpSourceFile? codeBehindFile, string viewTypeSimpleName)
    {
        if (codeBehindFile is null ||
            !codeBehindFile.TryGetType(viewTypeSimpleName, out var viewType))
        {
            return null;
        }

        var baseTypes = viewType.BaseList?.Types;
        if (baseTypes is null)
        {
            return null;
        }

        foreach (var baseType in baseTypes.Value)
        {
            if (baseType.Type is GenericNameSyntax genericName &&
                genericName.Identifier.ValueText == "GalleryReactiveUserControl" &&
                genericName.TypeArgumentList.Arguments.Count == 1)
            {
                return ExtractSimpleTypeName(genericName.TypeArgumentList.Arguments[0].ToString());
            }

            if (baseType.Type is QualifiedNameSyntax qualifiedName &&
                qualifiedName.Right is GenericNameSyntax qualifiedGenericName &&
                qualifiedGenericName.Identifier.ValueText == "GalleryReactiveUserControl" &&
                qualifiedGenericName.TypeArgumentList.Arguments.Count == 1)
            {
                return ExtractSimpleTypeName(qualifiedGenericName.TypeArgumentList.Arguments[0].ToString());
            }
        }

        return null;
    }

    private static bool TryCreateCSharpSnippet(ParsedCSharpSourceFile sourceFile,
                                               string targetTypeName,
                                               IEnumerable<string> initialMemberNames,
                                               string tabTitle,
                                               out GeneratedSnippet snippet,
                                               out HashSet<string> referencedNames)
    {
        snippet = default;
        referencedNames = new HashSet<string>(StringComparer.Ordinal);
        if (!sourceFile.TryGetType(targetTypeName, out var targetType))
        {
            return false;
        }

        var membersByName = BuildMemberMap(targetType);
        if (membersByName.Count == 0)
        {
            return false;
        }

        var selectedMembers = new HashSet<MemberDeclarationSyntax>();
        var selectedNames = new HashSet<string>(StringComparer.Ordinal);
        var pendingNames = new Queue<string>();
        foreach (var name in initialMemberNames.Where(IsIdentifier))
        {
            pendingNames.Enqueue(name);
        }

        while (pendingNames.Count > 0)
        {
            var name = pendingNames.Dequeue();
            referencedNames.Add(name);
            if (!selectedNames.Add(name))
            {
                continue;
            }

            if (!membersByName.TryGetValue(name, out var members))
            {
                continue;
            }

            foreach (var member in members)
            {
                if (!selectedMembers.Add(member))
                {
                    continue;
                }

                foreach (var referencedName in CollectReferencedIdentifiers(member))
                {
                    referencedNames.Add(referencedName);
                    if (membersByName.TryGetValue(referencedName, out var referencedMembers) &&
                        !selectedNames.Contains(referencedName) &&
                        !referencedMembers.All(IsPageLevelDataMember))
                    {
                        pendingNames.Enqueue(referencedName);
                    }
                }
            }
        }

        if (selectedMembers.Count == 0)
        {
            return false;
        }

        IncludeRelevantConstructors(targetType, selectedMembers, selectedNames, referencedNames, membersByName);

        var relatedTypes = CollectRelatedTypes(sourceFile, targetType, selectedMembers, referencedNames);
        var snippetText = BuildCSharpSnippetText(sourceFile, targetType, selectedMembers, relatedTypes);
        if (string.IsNullOrWhiteSpace(snippetText))
        {
            return false;
        }

        var includedNodes = selectedMembers.Cast<SyntaxNode>().Concat(relatedTypes);
        var startLine = includedNodes.Select(node => GetLineNumber(sourceFile.SyntaxTree, node.Span.Start))
                                     .Where(static line => line > 0)
                                     .DefaultIfEmpty(1)
                                     .Min();
        var endLine = includedNodes.Select(node => GetLineNumber(sourceFile.SyntaxTree, node.Span.End))
                                   .Where(static line => line > 0)
                                   .DefaultIfEmpty(startLine)
                                   .Max();

        snippet = new GeneratedSnippet(tabTitle, "csharp", snippetText, sourceFile.RelativePath, startLine, endLine);
        return true;
    }

    private static Dictionary<string, List<MemberDeclarationSyntax>> BuildMemberMap(TypeDeclarationSyntax type)
    {
        var membersByName = new Dictionary<string, List<MemberDeclarationSyntax>>(StringComparer.Ordinal);
        foreach (var member in type.Members)
        {
            switch (member)
            {
                case MethodDeclarationSyntax method:
                    AddMember(membersByName, method.Identifier.ValueText, member);
                    break;

                case PropertyDeclarationSyntax property:
                    AddMember(membersByName, property.Identifier.ValueText, member);
                    break;

                case FieldDeclarationSyntax field:
                    foreach (var variable in field.Declaration.Variables)
                    {
                        AddMember(membersByName, variable.Identifier.ValueText, member);
                    }
                    break;

                case EventFieldDeclarationSyntax eventField:
                    foreach (var variable in eventField.Declaration.Variables)
                    {
                        AddMember(membersByName, variable.Identifier.ValueText, member);
                    }
                    break;

                case TypeDeclarationSyntax nestedType:
                    AddMember(membersByName, nestedType.Identifier.ValueText, member);
                    break;
            }
        }

        return membersByName;
    }

    private static void AddMember(Dictionary<string, List<MemberDeclarationSyntax>> membersByName,
                                  string name,
                                  MemberDeclarationSyntax member)
    {
        if (!membersByName.TryGetValue(name, out var members))
        {
            members = new List<MemberDeclarationSyntax>();
            membersByName.Add(name, members);
        }

        members.Add(member);
    }

    // Page-level localization and API/Design-Token table helpers are shared infrastructure,
    // not part of any single item's behavior. The reference closure therefore stops at members such as
    // Lang, FallbackLang, EnsureApiRows and EnsureDesignTokenRows, instead of dragging their large switch and
    // row-initialization bodies into every snippet.
    private static bool IsPageLevelDataMember(MemberDeclarationSyntax member)
    {
        if (member is not MethodDeclarationSyntax method)
        {
            return false;
        }

        var name = method.Identifier.ValueText;
        if (NameLooksLikePageLevelData(name))
        {
            return true;
        }

        // Structural fallback: a method whose body is dominated by a switch over a
        // "*ResourceKind" / "*Kind" selector is a localization lookup table.
        return BodyIsResourceKindSwitch(method);
    }

    private static bool NameLooksLikePageLevelData(string name)
    {
        if (name is "Lang" or "FallbackLang")
        {
            return true;
        }

        // EnsureApiRows / EnsureDesignTokenRows / *Rows table builders.
        if (name.StartsWith("Ensure", StringComparison.Ordinal) &&
            name.EndsWith("Rows", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private static bool BodyIsResourceKindSwitch(MethodDeclarationSyntax method)
    {
        var switchExpressions = method.DescendantNodes().OfType<SwitchExpressionSyntax>().ToArray();
        var switchStatements = method.DescendantNodes().OfType<SwitchStatementSyntax>().ToArray();
        if (switchExpressions.Length == 0 && switchStatements.Length == 0)
        {
            return false;
        }

        foreach (var switchExpression in switchExpressions)
        {
            if (ExpressionMentionsResourceKind(switchExpression.GoverningExpression))
            {
                return true;
            }
        }

        foreach (var switchStatement in switchStatements)
        {
            if (ExpressionMentionsResourceKind(switchStatement.Expression))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ExpressionMentionsResourceKind(SyntaxNode expression)
    {
        foreach (var identifier in expression.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            var text = identifier.Identifier.ValueText;
            if (text.EndsWith("ResourceKind", StringComparison.Ordinal) ||
                text.EndsWith("LangResourceKind", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static void IncludeRelevantConstructors(
        TypeDeclarationSyntax targetType,
        HashSet<MemberDeclarationSyntax> selectedMembers,
        HashSet<string> selectedNames,
        HashSet<string> referencedNames,
        Dictionary<string, List<MemberDeclarationSyntax>> membersByName)
    {
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var constructor in targetType.Members.OfType<ConstructorDeclarationSyntax>())
            {
                if (selectedMembers.Contains(constructor))
                {
                    continue;
                }

                var constructorReferences = CollectReferencedIdentifiers(constructor);
                if (!constructorReferences.Any(selectedNames.Contains))
                {
                    continue;
                }

                selectedMembers.Add(constructor);
                changed = true;
                foreach (var referencedName in constructorReferences)
                {
                    referencedNames.Add(referencedName);
                    if (!selectedNames.Contains(referencedName) &&
                        membersByName.TryGetValue(referencedName, out var members) &&
                        !members.All(IsPageLevelDataMember))
                    {
                        selectedNames.Add(referencedName);
                        foreach (var member in members)
                        {
                            selectedMembers.Add(member);
                        }
                    }
                }
            }
        }
    }

    private static HashSet<string> CollectReferencedIdentifiers(SyntaxNode node)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var identifierName in node.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            names.Add(identifierName.Identifier.ValueText);
        }

        foreach (var genericName in node.DescendantNodes().OfType<GenericNameSyntax>())
        {
            names.Add(genericName.Identifier.ValueText);
        }

        foreach (var memberAccess in node.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
        {
            names.Add(memberAccess.Name switch
            {
                IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
                GenericNameSyntax genericName       => genericName.Identifier.ValueText,
                _                                   => memberAccess.Name.ToString()
            });
        }

        return names;
    }

    private static List<TypeDeclarationSyntax> CollectRelatedTypes(ParsedCSharpSourceFile sourceFile,
                                                                   TypeDeclarationSyntax targetType,
                                                                   HashSet<MemberDeclarationSyntax> selectedMembers,
                                                                   HashSet<string> referencedNames)
    {
        var selectedTypes = new HashSet<TypeDeclarationSyntax>();
        var pendingNames = new Queue<string>(referencedNames);
        while (pendingNames.Count > 0)
        {
            var name = pendingNames.Dequeue();
            if (!sourceFile.TopLevelTypesByName.TryGetValue(name, out var type) ||
                ReferenceEquals(type, targetType) ||
                !selectedTypes.Add(type))
            {
                continue;
            }

            foreach (var referencedName in CollectReferencedIdentifiers(type))
            {
                if (referencedNames.Add(referencedName))
                {
                    pendingNames.Enqueue(referencedName);
                }
            }
        }

        return sourceFile.TopLevelTypes
                         .Where(selectedTypes.Contains)
                         .OrderBy(static type => type.SpanStart)
                         .ToList();
    }

    private static string BuildCSharpSnippetText(ParsedCSharpSourceFile sourceFile,
                                                 TypeDeclarationSyntax targetType,
                                                 HashSet<MemberDeclarationSyntax> selectedMembers,
                                                 IReadOnlyList<TypeDeclarationSyntax> relatedTypes)
    {
        var builder = new StringBuilder();
        foreach (var usingDirective in sourceFile.UsingDirectives)
        {
            builder.AppendLine(usingDirective.ToFullString().Trim());
        }

        if (sourceFile.UsingDirectives.Count > 0)
        {
            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(sourceFile.NamespaceName))
        {
            builder.Append("namespace ").Append(sourceFile.NamespaceName).AppendLine(";");
            builder.AppendLine();
        }

        builder.AppendLine(ExtractTypeHeader(sourceFile.SourceText, targetType));
        builder.AppendLine("{");

        var members = targetType.Members
                                .Where(selectedMembers.Contains)
                                .OrderBy(static member => member.SpanStart)
                                .ToArray();
        for (var i = 0; i < members.Length; i++)
        {
            builder.Append(IndentText(DedentMemberText(sourceFile.SourceText.ToString(members[i].Span)), "    "));
            builder.AppendLine();
            if (i < members.Length - 1)
            {
                builder.AppendLine();
            }
        }

        builder.AppendLine("}");

        foreach (var relatedType in relatedTypes)
        {
            builder.AppendLine();
            builder.AppendLine(DedentMemberText(sourceFile.SourceText.ToString(relatedType.Span)));
        }

        return builder.ToString().TrimEnd();
    }

    // Member spans keep their original file indentation on every line except the first
    // (which the lexer span trims). Strip the common leading indentation across all
    // non-blank lines so the snippet can be re-indented uniformly without stacking.
    private static string DedentMemberText(string text)
    {
        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');
        if (lines.Length == 0)
        {
            return string.Empty;
        }

        // The first line already starts at column 0 (span trimmed); ignore it when
        // computing the shared indent of the continuation lines.
        var minIndent = int.MaxValue;
        for (var i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            minIndent = Math.Min(minIndent, CountIndent(lines[i]));
        }

        if (minIndent == int.MaxValue)
        {
            minIndent = 0;
        }

        var builder = new StringBuilder();
        builder.Append(lines[0].TrimStart());
        for (var i = 1; i < lines.Length; i++)
        {
            builder.Append('\n');
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            builder.Append(line.Length >= minIndent ? line.Substring(minIndent) : line.TrimStart());
        }

        return builder.ToString();
    }

    private static string ExtractTypeHeader(SourceText sourceText, TypeDeclarationSyntax targetType)
    {
        if (targetType.OpenBraceToken.RawKind == 0)
        {
            return targetType.Identifier.ValueText;
        }

        return sourceText.ToString(TextSpan.FromBounds(targetType.SpanStart, targetType.OpenBraceToken.SpanStart))
                         .Trim();
    }

    private static string IndentText(string text, string indent)
    {
        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');
        var builder = new StringBuilder();
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length == 0)
            {
                builder.AppendLine();
            }
            else
            {
                builder.Append(indent).Append(lines[i]);
                if (i < lines.Length - 1)
                {
                    builder.AppendLine();
                }
            }
        }

        return builder.ToString();
    }

    private static int GetLineNumber(SyntaxTree syntaxTree, int position)
    {
        return syntaxTree.GetLineSpan(new TextSpan(position, 0)).StartLinePosition.Line + 1;
    }

    private static Location CreateLocation(string path, SourceText sourceText, XElement element)
    {
        var lineNumber = GetLineNumber(element);
        if (lineNumber <= 0 || lineNumber > sourceText.Lines.Count)
        {
            return Location.None;
        }

        var line = sourceText.Lines[lineNumber - 1];
        return Location.Create(path, line.Span, sourceText.Lines.GetLinePositionSpan(line.Span));
    }

    private static string GetGlobalOption(AnalyzerConfigOptionsProvider options, string key)
    {
        return options.GlobalOptions.TryGetValue(key, out var value)
            ? value
            : string.Empty;
    }

    private static string MakeRelativePath(string path, string projectDir)
    {
        var normalizedPath = path.Replace('\\', '/');
        var normalizedProjectDir = projectDir.Replace('\\', '/');
        if (!string.IsNullOrWhiteSpace(normalizedProjectDir))
        {
            if (!normalizedProjectDir.EndsWith("/", StringComparison.Ordinal))
            {
                normalizedProjectDir += "/";
            }

            if (normalizedPath.StartsWith(normalizedProjectDir, StringComparison.OrdinalIgnoreCase))
            {
                return normalizedPath.Substring(normalizedProjectDir.Length);
            }
        }

        return normalizedPath;
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    private static string WriteCatalog(string outputNamespace, IReadOnlyList<GeneratedSnippetGroup> groups)
    {
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.Append("namespace ").Append(outputNamespace).AppendLine(";");
        builder.AppendLine();
        builder.AppendLine("internal static partial class ShowCaseCodeSnippetCatalog");
        builder.AppendLine("{");
        builder.AppendLine("    public static bool TryGetSnippetGroup(");
        builder.AppendLine("        string viewTypeName,");
        builder.AppendLine("        string panelKey,");
        builder.AppendLine("        int itemIndex,");
        builder.AppendLine("        string? sourceKey,");
        builder.AppendLine("        out global::AtomUI.Toolkits.GalleryBase.SourceCode.ShowCaseCodeSnippetGroup group)");
        builder.AppendLine("    {");

        foreach (var group in groups)
        {
            builder.Append("        if (viewTypeName == ")
                   .Append(CSharpSyntaxTreeString(group.ViewTypeName))
                   .Append(" && panelKey == ")
                   .Append(CSharpSyntaxTreeString(group.PanelKey))
                   .Append(" && itemIndex == ")
                   .Append(group.ItemIndex)
                   .Append(" && ");

            if (group.SourceKey is null)
            {
                builder.Append("string.IsNullOrEmpty(sourceKey)");
            }
            else
            {
                builder.Append("sourceKey == ").Append(CSharpSyntaxTreeString(group.SourceKey));
            }

            builder.AppendLine(")");
            builder.AppendLine("        {");
            builder.Append("            group = new global::AtomUI.Toolkits.GalleryBase.SourceCode.ShowCaseCodeSnippetGroup(")
                   .AppendLine();
            builder.Append("                Title: ").Append(CSharpSyntaxTreeString(group.Title)).AppendLine(",");
            builder.AppendLine("                Snippets: new global::AtomUI.Toolkits.GalleryBase.SourceCode.ShowCaseCodeSnippet[]");
            builder.AppendLine("                {");
            for (var i = 0; i < group.Snippets.Count; i++)
            {
                var snippet = group.Snippets[i];
                builder.Append("                    new global::AtomUI.Toolkits.GalleryBase.SourceCode.ShowCaseCodeSnippet(")
                       .Append("TabTitle: ")
                       .Append(CSharpSyntaxTreeString(snippet.TabTitle))
                       .Append(", Language: ")
                       .Append(CSharpSyntaxTreeString(snippet.Language))
                       .Append(", Text: ")
                       .Append(CSharpSyntaxTreeString(snippet.Text))
                       .Append(", SourceFilePath: ")
                       .Append(CSharpSyntaxTreeString(snippet.SourceFilePath))
                       .Append(", StartLine: ")
                       .Append(snippet.StartLine)
                       .Append(", EndLine: ")
                       .Append(snippet.EndLine)
                       .Append(")");
                if (i < group.Snippets.Count - 1)
                {
                    builder.Append(",");
                }
                builder.AppendLine();
            }
            builder.AppendLine("                });");
            builder.AppendLine("            return true;");
            builder.AppendLine("        }");
        }

        builder.AppendLine();
        builder.AppendLine("        group = default!;");
        builder.AppendLine("        return false;");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string CSharpSyntaxTreeString(string? value)
    {
        return SymbolDisplay.FormatLiteral(value ?? string.Empty, true);
    }

    private enum AdditionalSourceFileKind
    {
        Ignored,
        Axaml,
        CodeBehind,
        ViewModel
    }

    private readonly struct AdditionalSourceFile
    {
        private AdditionalSourceFile(string path, string text, AdditionalSourceFileKind kind)
        {
            Path = path;
            Text = text;
            Kind = kind;
        }

        public string Path { get; }
        public string Text { get; }
        public AdditionalSourceFileKind Kind { get; }

        public static AdditionalSourceFile Create(string path, string text)
        {
            return new AdditionalSourceFile(path, text, Classify(path));
        }

        public static AdditionalSourceFile CreateIgnored(string path)
        {
            return new AdditionalSourceFile(path, string.Empty, AdditionalSourceFileKind.Ignored);
        }

        public static bool IsSupportedPath(string path)
        {
            return Classify(path) != AdditionalSourceFileKind.Ignored;
        }

        private static AdditionalSourceFileKind Classify(string path)
        {
            var normalizedPath = NormalizePath(path);
            if (normalizedPath.EndsWith(".axaml.cs", StringComparison.OrdinalIgnoreCase))
            {
                return AdditionalSourceFileKind.CodeBehind;
            }

            if (normalizedPath.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase))
            {
                return AdditionalSourceFileKind.Axaml;
            }

            if (normalizedPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                normalizedPath.IndexOf("/viewmodels/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return AdditionalSourceFileKind.ViewModel;
            }

            return AdditionalSourceFileKind.Ignored;
        }
    }

    private sealed class SourceFileIndex
    {
        private readonly Dictionary<string, ParsedCSharpSourceFile> _codeBehindByAxamlPath =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ParsedCSharpSourceFile> _codeBehindByTypeName =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, ParsedCSharpSourceFile> _viewModelByTypeName =
            new(StringComparer.Ordinal);

        public SourceFileIndex(IEnumerable<AdditionalSourceFile> sourceFiles, GeneratorOptions options)
        {
            foreach (var file in sourceFiles)
            {
                if (file.Kind != AdditionalSourceFileKind.CodeBehind &&
                    file.Kind != AdditionalSourceFileKind.ViewModel)
                {
                    continue;
                }

                if (!ParsedCSharpSourceFile.TryCreate(file, options, out var parsedFile))
                {
                    continue;
                }

                if (file.Kind == AdditionalSourceFileKind.CodeBehind)
                {
                    var axamlPath = NormalizePath(file.Path.Substring(0, file.Path.Length - ".cs".Length));
                    _codeBehindByAxamlPath[axamlPath] = parsedFile;
                    foreach (var typeName in parsedFile.TopLevelTypesByName.Keys)
                    {
                        if (!_codeBehindByTypeName.ContainsKey(typeName))
                        {
                            _codeBehindByTypeName.Add(typeName, parsedFile);
                        }
                    }
                }
                else
                {
                    foreach (var typeName in parsedFile.TopLevelTypesByName.Keys)
                    {
                        if (!_viewModelByTypeName.ContainsKey(typeName))
                        {
                            _viewModelByTypeName.Add(typeName, parsedFile);
                        }
                    }
                }
            }
        }

        public ParsedCSharpSourceFile? TryGetCodeBehindFile(string axamlPath, string viewTypeSimpleName)
        {
            var normalizedAxamlPath = NormalizePath(axamlPath);
            if (_codeBehindByAxamlPath.TryGetValue(normalizedAxamlPath, out var codeBehindFile))
            {
                return codeBehindFile;
            }

            return _codeBehindByTypeName.TryGetValue(viewTypeSimpleName, out codeBehindFile)
                ? codeBehindFile
                : null;
        }

        public bool TryGetViewModelFile(string viewModelTypeName, out ParsedCSharpSourceFile viewModelFile)
        {
            return _viewModelByTypeName.TryGetValue(viewModelTypeName, out viewModelFile!);
        }
    }

    private sealed class ParsedCSharpSourceFile
    {
        private ParsedCSharpSourceFile(AdditionalSourceFile file,
                                       GeneratorOptions options,
                                       SourceText sourceText,
                                       SyntaxTree syntaxTree,
                                       CompilationUnitSyntax root,
                                       IReadOnlyList<UsingDirectiveSyntax> usingDirectives,
                                       string? namespaceName,
                                       IReadOnlyList<TypeDeclarationSyntax> topLevelTypes)
        {
            Path = file.Path;
            RelativePath = MakeRelativePath(file.Path, options.ProjectDir);
            SourceText = sourceText;
            SyntaxTree = syntaxTree;
            Root = root;
            UsingDirectives = usingDirectives;
            NamespaceName = namespaceName;
            TopLevelTypes = topLevelTypes;
            TopLevelTypesByName = topLevelTypes
                                  .GroupBy(static type => type.Identifier.ValueText, StringComparer.Ordinal)
                                  .ToDictionary(static group => group.Key,
                                                static group => group.First(),
                                                StringComparer.Ordinal);
        }

        public string Path { get; }
        public string RelativePath { get; }
        public SourceText SourceText { get; }
        public SyntaxTree SyntaxTree { get; }
        public CompilationUnitSyntax Root { get; }
        public IReadOnlyList<UsingDirectiveSyntax> UsingDirectives { get; }
        public string? NamespaceName { get; }
        public IReadOnlyList<TypeDeclarationSyntax> TopLevelTypes { get; }
        public IReadOnlyDictionary<string, TypeDeclarationSyntax> TopLevelTypesByName { get; }

        public bool TryGetType(string typeName, out TypeDeclarationSyntax type)
        {
            return TopLevelTypesByName.TryGetValue(typeName, out type!);
        }

        public static bool TryCreate(AdditionalSourceFile file,
                                     GeneratorOptions options,
                                     out ParsedCSharpSourceFile parsedFile)
        {
            parsedFile = null!;
            if (string.IsNullOrWhiteSpace(file.Text))
            {
                return false;
            }

            var sourceText = SourceText.From(file.Text, Encoding.UTF8);
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, path: file.Path);
            if (syntaxTree.GetRoot() is not CompilationUnitSyntax root)
            {
                return false;
            }

            var namespaceDeclaration = root.Members.OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
            var namespaceName = namespaceDeclaration?.Name.ToString();
            var usingDirectives = root.Usings
                                      .Concat(namespaceDeclaration?.Usings ?? default)
                                      .ToArray();
            var topLevelTypes = GetTopLevelTypes(root).ToArray();
            if (topLevelTypes.Length == 0)
            {
                return false;
            }

            parsedFile = new ParsedCSharpSourceFile(
                file,
                options,
                sourceText,
                syntaxTree,
                root,
                usingDirectives,
                namespaceName,
                topLevelTypes);
            return true;
        }

        private static IEnumerable<TypeDeclarationSyntax> GetTopLevelTypes(CompilationUnitSyntax root)
        {
            foreach (var member in root.Members)
            {
                if (member is TypeDeclarationSyntax type)
                {
                    yield return type;
                }
                else if (member is BaseNamespaceDeclarationSyntax namespaceDeclaration)
                {
                    foreach (var namespaceMember in namespaceDeclaration.Members.OfType<TypeDeclarationSyntax>())
                    {
                        yield return namespaceMember;
                    }
                }
            }
        }
    }

    private sealed class ItemReferences
    {
        public ItemReferences(IReadOnlyCollection<string> eventHandlerNames,
                              IReadOnlyCollection<string> bindingMemberNames,
                              string? viewModelTypeName)
        {
            EventHandlerNames = eventHandlerNames;
            BindingMemberNames = bindingMemberNames;
            ViewModelTypeName = viewModelTypeName;
        }

        public IReadOnlyCollection<string> EventHandlerNames { get; }
        public IReadOnlyCollection<string> BindingMemberNames { get; }
        public string? ViewModelTypeName { get; }
    }

    private readonly struct GeneratorOptions
    {
        public GeneratorOptions(string rootNamespace, string projectDir)
        {
            RootNamespace = rootNamespace;
            ProjectDir    = projectDir;
        }

        public string RootNamespace { get; }
        public string ProjectDir { get; }
    }

    private readonly struct GeneratedSnippetGroup
    {
        public GeneratedSnippetGroup(string viewTypeName,
                                     string panelKey,
                                     int itemIndex,
                                     string? sourceKey,
                                     string title,
                                     IReadOnlyList<GeneratedSnippet> snippets)
        {
            ViewTypeName = viewTypeName;
            PanelKey     = panelKey;
            ItemIndex    = itemIndex;
            SourceKey    = sourceKey;
            Title        = title;
            Snippets     = snippets;
        }

        public string ViewTypeName { get; }
        public string PanelKey { get; }
        public int ItemIndex { get; }
        public string? SourceKey { get; }
        public string Title { get; }
        public IReadOnlyList<GeneratedSnippet> Snippets { get; }
    }

    private readonly struct GeneratedSnippet
    {
        public GeneratedSnippet(string tabTitle,
                                string language,
                                string text,
                                string sourceFilePath,
                                int startLine,
                                int endLine)
        {
            TabTitle       = tabTitle;
            Language       = language;
            Text           = text;
            SourceFilePath = sourceFilePath;
            StartLine      = startLine;
            EndLine        = endLine;
        }

        public string TabTitle { get; }
        public string Language { get; }
        public string Text { get; }
        public string SourceFilePath { get; }
        public int StartLine { get; }
        public int EndLine { get; }
    }
}
