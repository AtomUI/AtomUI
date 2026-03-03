using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using Avalonia.Platform;
using AvaloniaControlList = Avalonia.Controls.Controls;

namespace AtomUIGallery.Controls;

public class ShowCasePanel : TemplatedControl
{
    internal const string MainPanelPart = "PART_MainPanel";
    private const string DefaultRepositoryBaseUrl = "https://github.com/AtomUI/AtomUI/blob/release/5.0";
    private bool _initialized;
    private bool _axamlSourcesInitialized;
    private Grid? _layoutPanel;

    private readonly struct ShowCaseItemSnippet
    {
        public ShowCaseItemSnippet(string content, int startLine, int endLine)
        {
            Content = content;
            StartLine = Math.Max(1, startLine);
            EndLine = Math.Max(StartLine, endLine);
        }

        public string Content { get; }
        public int StartLine { get; }
        public int EndLine { get; }
    }

    public static readonly StyledProperty<string?> RepositoryBaseUrlProperty =
        AvaloniaProperty.Register<ShowCasePanel, string?>(
            nameof(RepositoryBaseUrl),
            DefaultRepositoryBaseUrl);

    public string? RepositoryBaseUrl
    {
        get => GetValue(RepositoryBaseUrlProperty);
        set => SetValue(RepositoryBaseUrlProperty, value);
    }

    [Content]
    public AvaloniaControlList Children { get; } = new();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var effectCount = 0;
        foreach (var child in Children)
        {
            if (child is ShowCaseItem showCaseItem)
            {
                effectCount++;
                if (showCaseItem.IsOccupyEntireRow)
                {
                    effectCount++;
                }
            }
        }
        if (effectCount % 2 != 0)
        {
            var extra = new ShowCaseItem()
            {
                IsFake = true
            };
            Children.Add(extra);
        }
        base.OnApplyTemplate(e);
        _layoutPanel = e.NameScope.Get<Grid>(MainPanelPart);
        if (_layoutPanel != null && !_initialized)
        {
            var row = 0;
            var column = 0;
            
            for (var i = 0; i < Children.Count; ++i)
            {
                if (Children[i] is ShowCaseItem item)
                {
                    if (item.IsOccupyEntireRow)
                    {
                        if (column != 0)
                        {
                            row++;
                        }
                        Grid.SetRow(item, row++);
                        
                        Grid.SetColumn(item, 0);
                        Grid.SetColumnSpan(item, 2);
                    }
                    else
                    {
                        Grid.SetRow(item, row);
                        Grid.SetColumn(item, column++);
                        if (column == 2)
                        {
                            row++;
                            column = 0;
                        }
                    }
                    _layoutPanel.Children.Add(item);
                    LogicalChildren.Add(item);
                }
            }
            
            var rowDefinitions = new RowDefinitions();
            for (var i = 0; i < row; ++i)
            {
                rowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }
            _layoutPanel.RowDefinitions = rowDefinitions;
            _initialized                = true;
        }
        
        TryPopulateAxamlSources();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        TryPopulateAxamlSources();
    }

    private void TryPopulateAxamlSources()
    {
        if (_axamlSourcesInitialized)
        {
            return;
        }

        var owner = this.FindLogicalAncestorOfType<UserControl>();
        if (owner is null)
        {
            return;
        }

        var axamlSnippets = LoadAxamlSnippets(owner);
        var repositoryFileUrl = ResolveOwnerRepositorySourceUrl(owner.GetType());
        if (axamlSnippets.Count == 0 && string.IsNullOrWhiteSpace(repositoryFileUrl))
        {
            return;
        }

        var snippetIndex = 0;
        foreach (var child in Children)
        {
            if (child is not ShowCaseItem item || item.IsFake)
            {
                continue;
            }

            if (snippetIndex < axamlSnippets.Count)
            {
                var snippet = axamlSnippets[snippetIndex];
                if (string.IsNullOrWhiteSpace(item.AxamlSource) &&
                    string.IsNullOrWhiteSpace(item.DeferredAxamlSource))
                {
                    if (item.IsCodeExpanded)
                    {
                        item.AxamlSource = snippet.Content;
                    }
                    else
                    {
                        item.DeferredAxamlSource = snippet.Content;
                    }
                }

                if (string.IsNullOrWhiteSpace(item.RepositorySourceUrl) &&
                    !string.IsNullOrWhiteSpace(repositoryFileUrl))
                {
                    item.RepositorySourceUrl = BuildRepositorySourceUrlWithLineAnchor(
                        repositoryFileUrl,
                        snippet.StartLine,
                        snippet.EndLine);
                }
            }
            
            snippetIndex++;
        }

        _axamlSourcesInitialized = true;
    }

    private IReadOnlyList<ShowCaseItemSnippet> LoadAxamlSnippets(UserControl owner)
    {
        var axamlText = LoadOwnerAxamlText(owner.GetType());
        if (string.IsNullOrWhiteSpace(axamlText))
        {
            return Array.Empty<ShowCaseItemSnippet>();
        }

        var panelIndex = ResolvePanelIndex(owner);
        return ExtractShowCaseItemSnippets(axamlText, panelIndex);
    }

    private string? ResolveOwnerRepositorySourceUrl(Type ownerType)
    {
        if (string.IsNullOrWhiteSpace(RepositoryBaseUrl))
        {
            return null;
        }

        var ownerAxamlFilePath = ResolveOwnerAxamlFilePath(ownerType);
        if (string.IsNullOrWhiteSpace(ownerAxamlFilePath))
        {
            return null;
        }

        var relativePath = TryResolveRepositoryRelativePath(ownerAxamlFilePath);
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        return $"{RepositoryBaseUrl.TrimEnd('/')}/{relativePath}";
    }

    private static string? TryResolveRepositoryRelativePath(string filePath)
    {
        var normalizedPath = filePath.Replace('\\', '/');
        var marker = "/controlgallery/";
        var markerIndex = normalizedPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex >= 0 && markerIndex + 1 < normalizedPath.Length)
        {
            return normalizedPath[(markerIndex + 1)..];
        }

        var fallbackPath = normalizedPath.TrimStart('/');
        return string.IsNullOrWhiteSpace(fallbackPath) ? null : fallbackPath;
    }

    private static string BuildRepositorySourceUrlWithLineAnchor(string fileUrl, int startLine, int endLine)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return fileUrl;
        }

        return startLine < 1
            ? fileUrl
            : endLine > startLine
                ? $"{fileUrl}#L{startLine}-L{endLine}"
                : $"{fileUrl}#L{startLine}";
    }

    private static string? LoadOwnerAxamlText(Type ownerType)
    {
        var axamlUri = ResolveOwnerAxamlUri(ownerType);
        if (axamlUri is not null)
        {
            try
            {
                using var stream = AssetLoader.Open(axamlUri);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch
            {
                // Ignore and fallback to file-system probing.
            }
        }

        var axamlPath = ResolveOwnerAxamlFilePath(ownerType);
        if (string.IsNullOrWhiteSpace(axamlPath))
        {
            return null;
        }

        try
        {
            return File.ReadAllText(axamlPath);
        }
        catch
        {
            return null;
        }
    }

    private static string? ResolveOwnerAxamlFilePath(Type ownerType)
    {
        var fileName = $"{ownerType.Name}.axaml";
        foreach (var searchRoot in EnumerateSearchRoots())
        {
            foreach (var viewsRoot in EnumerateViewRoots(searchRoot))
            {
                var filePath = TryFindAxamlFile(viewsRoot, fileName);
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    return filePath;
                }
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateSearchRoots()
    {
        var roots = new List<string>();
        AddSearchRoot(roots, AppContext.BaseDirectory);
        AddSearchRoot(roots, Environment.CurrentDirectory);

        var current = new DirectoryInfo(AppContext.BaseDirectory);
        for (var depth = 0; depth < 8 && current is not null; depth++)
        {
            AddSearchRoot(roots, current.FullName);
            current = current.Parent;
        }

        current = new DirectoryInfo(Environment.CurrentDirectory);
        for (var depth = 0; depth < 8 && current is not null; depth++)
        {
            AddSearchRoot(roots, current.FullName);
            current = current.Parent;
        }

        return roots;
    }

    private static void AddSearchRoot(ICollection<string> roots, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        string normalizedPath;
        try
        {
            normalizedPath = Path.GetFullPath(path);
        }
        catch
        {
            return;
        }

        foreach (var root in roots)
        {
            if (string.Equals(root, normalizedPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        roots.Add(normalizedPath);
    }

    private static IEnumerable<string> EnumerateViewRoots(string searchRoot)
    {
        var viewRoots = new[]
        {
            Path.Combine(searchRoot, "ShowCases", "Views"),
            Path.Combine(searchRoot, "AtomUIGallery", "ShowCases", "Views"),
            Path.Combine(searchRoot, "controlgallery", "AtomUIGallery", "ShowCases", "Views")
        };

        foreach (var viewRoot in viewRoots)
        {
            if (Directory.Exists(viewRoot))
            {
                yield return viewRoot;
            }
        }
    }

    private static string? TryFindAxamlFile(string viewsRoot, string fileName)
    {
        try
        {
            return Directory.EnumerateFiles(viewsRoot, fileName, SearchOption.AllDirectories)
                            .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private int ResolvePanelIndex(UserControl owner)
    {
        var panels = owner.GetLogicalDescendants().OfType<ShowCasePanel>().ToList();
        var index  = panels.IndexOf(this);
        return index >= 0 ? index : 0;
    }

    private static Uri? ResolveOwnerAxamlUri(Type ownerType)
    {
        var assemblyName = ownerType.Assembly.GetName().Name;
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return null;
        }

        var rootUri         = new Uri($"avares://{assemblyName}/");
        var expectedFileEnd = $"/{ownerType.Name}.axaml";

        IReadOnlyList<Uri> matchedUris;
        try
        {
            matchedUris = AssetLoader.GetAssets(rootUri, null)
                                     .Where(uri => uri.AbsolutePath.EndsWith(expectedFileEnd, StringComparison.OrdinalIgnoreCase))
                                     .ToList();
        }
        catch
        {
            return null;
        }
        
        if (matchedUris.Count == 0)
        {
            return null;
        }
        
        if (matchedUris.Count == 1)
        {
            return matchedUris[0];
        }

        return matchedUris.FirstOrDefault(uri => uri.AbsolutePath.Contains("/ShowCases/Views/", StringComparison.OrdinalIgnoreCase))
               ?? matchedUris[0];
    }

    private static IReadOnlyList<ShowCaseItemSnippet> ExtractShowCaseItemSnippets(string axamlText, int panelIndex)
    {
        try
        {
            var document      = XDocument.Parse(axamlText, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            var panelElements = document.Descendants()
                                        .Where(element => element.Name.LocalName == nameof(ShowCasePanel))
                                        .ToList();
            if (panelElements.Count == 0)
            {
                return Array.Empty<ShowCaseItemSnippet>();
            }
            
            var selectedPanel = panelIndex >= 0 && panelIndex < panelElements.Count
                ? panelElements[panelIndex]
                : panelElements[0];
            var lineStartOffsets = BuildLineStartOffsets(axamlText);

            return selectedPanel.Elements()
                                .Where(element => element.Name.LocalName == nameof(ShowCaseItem))
                                .Select(element => BuildSnippet(axamlText, element, lineStartOffsets))
                                .ToList();
        }
        catch
        {
            return Array.Empty<ShowCaseItemSnippet>();
        }
    }

    private static ShowCaseItemSnippet BuildSnippet(string axamlText, XElement itemElement, IReadOnlyList<int> lineStartOffsets)
    {
        if (TryExtractRawItemContent(axamlText, itemElement, lineStartOffsets, out var rawSnippet, out var startLine, out var endLine))
        {
            return new ShowCaseItemSnippet(NormalizeIndent(rawSnippet), startLine, endLine);
        }

        var serializedSnippet = BuildSnippetBySerialization(itemElement);
        var fallbackStartLine = itemElement is IXmlLineInfo lineInfo && lineInfo.HasLineInfo()
            ? lineInfo.LineNumber
            : 1;
        var fallbackEndLine = fallbackStartLine + Math.Max(1, serializedSnippet.Replace("\r\n", "\n").Split('\n').Length) - 1;
        return new ShowCaseItemSnippet(serializedSnippet, fallbackStartLine, fallbackEndLine);
    }

    private static string BuildSnippetBySerialization(XElement itemElement)
    {
        var builder = new StringBuilder();
        foreach (var node in itemElement.Nodes())
        {
            if (node is XText text && string.IsNullOrWhiteSpace(text.Value))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(node.ToString(SaveOptions.None));
        }

        return NormalizeIndent(builder.ToString());
    }

    private static bool TryExtractRawItemContent(
        string source,
        XElement itemElement,
        IReadOnlyList<int> lineStartOffsets,
        out string content,
        out int startLine,
        out int endLine)
    {
        content = string.Empty;
        startLine = 1;
        endLine = 1;

        if (itemElement is not IXmlLineInfo lineInfo || !lineInfo.HasLineInfo())
        {
            return false;
        }

        var offset = ResolveOffsetByLineInfo(source.Length, lineStartOffsets, lineInfo.LineNumber, lineInfo.LinePosition);
        if (offset < 0)
        {
            return false;
        }

        var elementStart = FindTagStartInLine(source, offset);
        if (elementStart < 0)
        {
            return false;
        }

        if (!TryExtractElementInnerContent(source, elementStart, itemElement.Name.LocalName, out content, out var elementEnd))
        {
            return false;
        }

        startLine = ResolveLineNumberByOffset(lineStartOffsets, elementStart);
        endLine = ResolveLineNumberByOffset(lineStartOffsets, elementEnd);
        return true;
    }

    private static IReadOnlyList<int> BuildLineStartOffsets(string source)
    {
        var offsets = new List<int> { 0 };
        for (var i = 0; i < source.Length; i++)
        {
            if (source[i] == '\n')
            {
                offsets.Add(i + 1);
            }
        }

        return offsets;
    }

    private static int ResolveOffsetByLineInfo(int textLength, IReadOnlyList<int> lineStartOffsets, int lineNumber, int linePosition)
    {
        if (lineNumber < 1 || lineNumber > lineStartOffsets.Count || linePosition < 1)
        {
            return -1;
        }

        var offset = lineStartOffsets[lineNumber - 1] + linePosition - 1;
        return offset >= 0 && offset < textLength ? offset : -1;
    }

    private static int ResolveLineNumberByOffset(IReadOnlyList<int> lineStartOffsets, int offset)
    {
        if (lineStartOffsets.Count == 0)
        {
            return 1;
        }

        var left = 0;
        var right = lineStartOffsets.Count - 1;
        var matchedLineIndex = 0;
        while (left <= right)
        {
            var mid = left + ((right - left) >> 1);
            var startOffset = lineStartOffsets[mid];
            if (startOffset <= offset)
            {
                matchedLineIndex = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        return matchedLineIndex + 1;
    }

    private static int FindTagStartInLine(string source, int offset)
    {
        if (offset < 0 || offset >= source.Length)
        {
            return -1;
        }

        var lineStart = offset;
        while (lineStart > 0 && source[lineStart - 1] != '\n' && source[lineStart - 1] != '\r')
        {
            lineStart--;
        }

        for (var i = offset; i >= lineStart; i--)
        {
            if (source[i] == '<')
            {
                return i;
            }
        }

        var lineEnd = offset;
        while (lineEnd < source.Length && source[lineEnd] != '\n' && source[lineEnd] != '\r')
        {
            lineEnd++;
        }

        for (var i = offset; i < lineEnd; i++)
        {
            if (source[i] == '<')
            {
                return i;
            }
        }

        return -1;
    }

    private static bool TryExtractElementInnerContent(
        string source,
        int elementStart,
        string targetLocalName,
        out string content,
        out int elementEnd)
    {
        content = string.Empty;
        elementEnd = -1;

        if (!TryParseTag(source, elementStart, out var startTag) || startTag.IsClosing || !IsSameLocalName(startTag.LocalName, targetLocalName))
        {
            return false;
        }

        if (startTag.IsSelfClosing)
        {
            elementEnd = startTag.TagEnd;
            return true;
        }

        var depth     = 1;
        var scanIndex = startTag.TagEnd + 1;

        while (scanIndex < source.Length)
        {
            if (source[scanIndex] != '<')
            {
                scanIndex++;
                continue;
            }

            if (StartsWith(source, scanIndex, "<!--"))
            {
                scanIndex = MoveAfter(source, scanIndex + 4, "-->");
                continue;
            }

            if (StartsWith(source, scanIndex, "<![CDATA["))
            {
                scanIndex = MoveAfter(source, scanIndex + 9, "]]>");
                continue;
            }

            if (StartsWith(source, scanIndex, "<?"))
            {
                scanIndex = MoveAfter(source, scanIndex + 2, "?>");
                continue;
            }

            if (StartsWith(source, scanIndex, "<!"))
            {
                scanIndex = MoveAfter(source, scanIndex + 2, ">");
                continue;
            }

            if (!TryParseTag(source, scanIndex, out var tag))
            {
                scanIndex++;
                continue;
            }

            if (IsSameLocalName(tag.LocalName, targetLocalName))
            {
                if (tag.IsClosing)
                {
                    depth--;
                    if (depth == 0)
                    {
                        var innerStart = startTag.TagEnd + 1;
                        if (scanIndex < innerStart)
                        {
                            return false;
                        }

                        content = source[innerStart..scanIndex];
                        elementEnd = tag.TagEnd;
                        return true;
                    }
                }
                else if (!tag.IsSelfClosing)
                {
                    depth++;
                }
            }

            scanIndex = tag.TagEnd + 1;
        }

        return false;
    }

    private readonly struct ParsedTag
    {
        public ParsedTag(string localName, bool isClosing, bool isSelfClosing, int tagEnd)
        {
            LocalName    = localName;
            IsClosing    = isClosing;
            IsSelfClosing = isSelfClosing;
            TagEnd       = tagEnd;
        }

        public string LocalName { get; }
        public bool IsClosing { get; }
        public bool IsSelfClosing { get; }
        public int TagEnd { get; }
    }

    private static bool TryParseTag(string source, int tagStart, out ParsedTag parsedTag)
    {
        parsedTag = default;
        if (tagStart < 0 || tagStart >= source.Length || source[tagStart] != '<')
        {
            return false;
        }

        var index = tagStart + 1;
        var isClosing = false;
        if (index < source.Length && source[index] == '/')
        {
            isClosing = true;
            index++;
        }

        while (index < source.Length && char.IsWhiteSpace(source[index]))
        {
            index++;
        }

        var nameStart = index;
        while (index < source.Length && IsTagNameChar(source[index]))
        {
            index++;
        }

        if (index == nameStart)
        {
            return false;
        }

        var qualifiedName = source[nameStart..index];
        var separator     = qualifiedName.LastIndexOf(':');
        var localName     = separator >= 0 ? qualifiedName[(separator + 1)..] : qualifiedName;

        var quote = '\0';
        while (index < source.Length)
        {
            var c = source[index];
            if (quote != '\0')
            {
                if (c == quote)
                {
                    quote = '\0';
                }
            }
            else if (c is '"' or '\'')
            {
                quote = c;
            }
            else if (c == '>')
            {
                break;
            }

            index++;
        }

        if (index >= source.Length || source[index] != '>')
        {
            return false;
        }

        var cursor = index - 1;
        while (cursor > tagStart && char.IsWhiteSpace(source[cursor]))
        {
            cursor--;
        }

        var isSelfClosing = !isClosing && cursor > tagStart && source[cursor] == '/';
        parsedTag = new ParsedTag(localName, isClosing, isSelfClosing, index);
        return true;
    }

    private static int MoveAfter(string source, int startIndex, string terminator)
    {
        var end = source.IndexOf(terminator, startIndex, StringComparison.Ordinal);
        return end < 0 ? source.Length : end + terminator.Length;
    }

    private static bool IsSameLocalName(string left, string right)
    {
        return string.Equals(left, right, StringComparison.Ordinal);
    }

    private static bool IsTagNameChar(char c)
    {
        return char.IsLetterOrDigit(c) || c is '_' or ':' or '-' or '.';
    }

    private static bool StartsWith(string source, int index, string value)
    {
        if (index < 0 || index + value.Length > source.Length)
        {
            return false;
        }

        for (var i = 0; i < value.Length; i++)
        {
            if (source[index + i] != value[i])
            {
                return false;
            }
        }

        return true;
    }

    private static string NormalizeIndent(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return string.Empty;
        }

        var lines = source.Replace("\r\n", "\n").Split('\n');
        var start = 0;
        var end   = lines.Length - 1;
        while (start <= end && string.IsNullOrWhiteSpace(lines[start]))
        {
            start++;
        }

        while (end >= start && string.IsNullOrWhiteSpace(lines[end]))
        {
            end--;
        }

        if (start > end)
        {
            return string.Empty;
        }

        var effectiveLines = lines[start..(end + 1)];
        var minIndent      = int.MaxValue;
        foreach (var line in effectiveLines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var indent = 0;
            while (indent < line.Length && char.IsWhiteSpace(line[indent]))
            {
                indent++;
            }
            minIndent = Math.Min(minIndent, indent);
        }

        if (minIndent == int.MaxValue)
        {
            minIndent = 0;
        }

        var normalized = new StringBuilder();
        for (var i = 0; i < effectiveLines.Length; i++)
        {
            var line = effectiveLines[i];
            normalized.Append(line.Length >= minIndent ? line[minIndent..] : line);
            if (i < effectiveLines.Length - 1)
            {
                normalized.Append(Environment.NewLine);
            }
        }

        return normalized.ToString();
    }

    internal virtual void NotifyAboutToActive()
    {
    }

    internal virtual void NotifyActivated()
    {
    }

    internal virtual void NotifyAboutToDeactivated()
    {
    }

    internal virtual void NotifyDeactivated()
    {
    }
}
