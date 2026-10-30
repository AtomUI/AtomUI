using System.Text.RegularExpressions;
using System.Xml.Linq;
using AtomUI.Theme.Schema;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class GalleryShowCaseStyleBoundaryTests
{
    [Theory]
    [InlineData("atom|Button /template/ .semantic-content", true)]
    [InlineData("atom|Button.semantic-demo[ButtonType=Primary] /template/ .semantic-content", true)]
    [InlineData("atom|Descriptions /template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-content", true)]
    [InlineData("atom|CountBadge > .semantic-scope-indicator /template/ .semantic-indicator", true)]
    [InlineData("atom|RibbonBadge > .semantic-indicator /template/ .semantic-content", true)]
    [InlineData("atom|Select /template/ .semantic-popup .semantic-option", false)]
    [InlineData("atom|Button /template/ atom|ContentPresenter#PART_ContentPresenter", false)]
    [InlineData("atom|Button /template/ .content", false)]
    [InlineData("atom|Descriptions /template/ .semantic-scope-items .semantic-content", false)]
    [InlineData(".semantic-demo /template/ .semantic-content", false)]
    public void Semantic_Part_Selector_Shape_Is_The_Only_Public_Template_Boundary(
        string selector,
        bool expected)
    {
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        var style = new XElement(
            "Style",
            new XAttribute("Selector", selector),
            new XAttribute(xaml + "SetterTargetType", "ContentPresenter"));

        TargetsSemanticPartSelectors(style, selector).ShouldBe(expected);
    }

    [Fact]
    public void Semantic_Part_Template_Selector_Requires_A_Setter_Target_Type()
    {
        const string selector = "atom|Button /template/ .semantic-content";
        var style = new XElement("Style", new XAttribute("Selector", selector));

        TargetsSemanticPartSelectors(style, selector).ShouldBeFalse();
    }

    [Fact]
    public void Gallery_ShowCases_Only_Enter_Templates_From_Owned_Style_Scopes()
    {
        AvaloniaTestApp.EnsureInitialized();
        var repositoryRoot = GetRepositoryRoot();
        var showCasesRoot = Path.Combine(repositoryRoot, "controlgallery", "AtomUIGallery", "ShowCases");
        var semanticParts = Application.Current.ShouldNotBeNull()
                                       .GetThemeManager().ShouldNotBeNull()
                                       .SemanticParts;
        var violations = new List<string>();

        foreach (var path in Directory.EnumerateFiles(showCasesRoot, "*.axaml", SearchOption.AllDirectories))
        {
            var document = XDocument.Load(path);
            foreach (var style in document.Descendants().Where(element => element.Name.LocalName == "Style"))
            {
                var selector = style.Attribute("Selector")?.Value;
                if (string.IsNullOrWhiteSpace(selector))
                {
                    continue;
                }

                var targetsSemanticPart = TargetsSemanticPartSelectors(style, selector);
                var containsTemplateBoundary = selector.Contains("/template/", StringComparison.Ordinal);
                var containsSemanticRoute = selector.Contains(" .semantic-", StringComparison.Ordinal) ||
                                            selector.Contains(" > .semantic-", StringComparison.Ordinal);
                if (!containsTemplateBoundary && !containsSemanticRoute)
                {
                    continue;
                }

                var ownerTheme = style.Ancestors()
                                      .FirstOrDefault(element => element.Name.LocalName == "ControlTheme");
                if ((ownerTheme is null || !TargetsGalleryOwnedControl(ownerTheme)) &&
                    !TargetsGalleryOwnedControlStyles(style, selector) &&
                    !targetsSemanticPart)
                {
                    violations.Add(
                        $"{Path.GetRelativePath(repositoryRoot, path)}: template selector is not owned by a Gallery control theme or its own Styles: {selector}");
                    continue;
                }

                if (targetsSemanticPart && !TargetsRegisteredSemanticPartRoutes(selector, semanticParts))
                {
                    violations.Add(
                        $"{Path.GetRelativePath(repositoryRoot, path)}: Semantic Part selector does not match a registered owner route: {selector}");
                }
            }
        }

        foreach (var path in Directory.EnumerateFiles(showCasesRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (File.ReadAllText(path).Contains(".Template()", StringComparison.Ordinal))
            {
                violations.Add($"{Path.GetRelativePath(repositoryRoot, path)}: .Template()");
            }
        }

        violations.ShouldBeEmpty(string.Join(Environment.NewLine, violations));
    }

    private static bool TargetsGalleryOwnedControl(XElement controlTheme)
    {
        var targetType = controlTheme.Attribute("TargetType")?.Value;
        var separatorIndex = targetType?.IndexOf(':') ?? -1;
        if (separatorIndex <= 0)
        {
            return false;
        }

        var prefix = targetType![..separatorIndex];
        var controlNamespace = controlTheme.GetNamespaceOfPrefix(prefix)?.NamespaceName;
        return controlNamespace?.StartsWith("using:AtomUIGallery", StringComparison.Ordinal) == true;
    }

    private static bool TargetsGalleryOwnedControlStyles(XElement style, string selector)
    {
        var stylesProperty = style.Ancestors()
                                  .FirstOrDefault(element => element.Name.LocalName.EndsWith(".Styles", StringComparison.Ordinal));
        var styleHost = stylesProperty?.Parent;
        if (stylesProperty is null ||
            styleHost is null ||
            stylesProperty.Name.LocalName != $"{styleHost.Name.LocalName}.Styles")
        {
            return false;
        }

        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        var className = styleHost.Attribute(xaml + "Class")?.Value;
        if (className?.StartsWith("AtomUIGallery.", StringComparison.Ordinal) != true)
        {
            return false;
        }

        return selector.Split(',').All(branch => TargetsStyleHostType(branch, styleHost));
    }

    private static bool TargetsSemanticPartSelectors(XElement style, string selector)
    {
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        if (string.IsNullOrWhiteSpace((string?)style.Attribute(xaml + "SetterTargetType")))
        {
            return false;
        }

        return selector.Split(',').All(static branch =>
        {
            if (!TryParseSemanticSelectorBranch(branch, out var ownerSelector, out _, out var selectorRoute))
            {
                return false;
            }

            return Regex.IsMatch(
                       ownerSelector,
                       @"^[A-Za-z_][A-Za-z0-9_-]*\|[A-Za-z_][A-Za-z0-9_]*(?:[.#:][A-Za-z_][A-Za-z0-9_-]*|\[[^\]\r\n]+\])*$",
                       RegexOptions.CultureInvariant) &&
                   IsSemanticSelectorRoute(selectorRoute);
        });
    }

    private static bool TargetsRegisteredSemanticPartRoutes(
        string selector,
        SemanticPartRegistry semanticParts)
    {
        return selector.Split(',').All(branch =>
        {
            if (!TryParseSemanticSelectorBranch(branch, out _, out var ownerTypeName, out var selectorRoute))
            {
                return false;
            }

            var owners = semanticParts.Controls
                                      .Where(control => string.Equals(
                                          control.ControlType.Name,
                                          ownerTypeName,
                                          StringComparison.Ordinal))
                                      .ToArray();
            return owners.Length == 1 &&
                   owners[0].Parts.Any(part => string.Equals(
                       part.SelectorRoute,
                       selectorRoute,
                       StringComparison.Ordinal));
        });
    }

    private static bool TryParseSemanticSelectorBranch(
        string branch,
        out string ownerSelector,
        out string ownerTypeName,
        out string selectorRoute)
    {
        ownerSelector = string.Empty;
        ownerTypeName = string.Empty;
        selectorRoute = string.Empty;
        var trimmed = branch.Trim();
        var templateIndex = trimmed.IndexOf(" /template/ ", StringComparison.Ordinal);
        var childIndex = trimmed.IndexOf(" > .semantic-", StringComparison.Ordinal);
        var routeIndex = templateIndex < 0
            ? childIndex
            : childIndex < 0
                ? templateIndex
                : Math.Min(templateIndex, childIndex);
        if (routeIndex <= 0)
        {
            return false;
        }

        ownerSelector = trimmed[..routeIndex];
        selectorRoute = trimmed[(routeIndex + 1)..];
        var prefixEnd = ownerSelector.IndexOf('|');
        if (prefixEnd <= 0)
        {
            return false;
        }

        var typeStart = prefixEnd + 1;
        var typeEnd = ownerSelector.IndexOfAny(['.', '#', ':', '['], typeStart);
        if (typeEnd < 0)
        {
            typeEnd = ownerSelector.Length;
        }
        ownerTypeName = ownerSelector[typeStart..typeEnd];
        return ownerTypeName.Length > 0;
    }

    private static bool IsSemanticSelectorRoute(string selectorRoute)
    {
        var tokens = selectorRoute.Split(' ');
        if (tokens.Length < 2 || tokens.Length % 2 != 0)
        {
            return false;
        }

        for (var index = 0; index < tokens.Length; index += 2)
        {
            if (tokens[index] is not ("/template/" or ">") ||
                !Regex.IsMatch(
                    tokens[index + 1],
                    @"^\.semantic-[a-z0-9]+(?:-[a-z0-9]+)*$",
                    RegexOptions.CultureInvariant))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TargetsStyleHostType(string selectorBranch, XElement styleHost)
    {
        var templateIndex = selectorBranch.IndexOf("/template/", StringComparison.Ordinal);
        if (templateIndex <= 0)
        {
            return false;
        }

        var ownerSelector = selectorBranch[..templateIndex].Trim();
        var prefixEnd = ownerSelector.IndexOf('|');
        if (prefixEnd <= 0)
        {
            return false;
        }

        var typeStart = prefixEnd + 1;
        var typeEnd = ownerSelector.IndexOfAny(['.', '#', ':', '[', ' '], typeStart);
        if (typeEnd < 0)
        {
            typeEnd = ownerSelector.Length;
        }

        var prefix = ownerSelector[..prefixEnd];
        var targetType = ownerSelector[typeStart..typeEnd];
        return targetType == styleHost.Name.LocalName &&
               styleHost.GetNamespaceOfPrefix(prefix) == styleHost.Name.Namespace;
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate repository root.");
    }
}
