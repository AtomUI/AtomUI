using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIToolTip = AtomUI.Desktop.Controls.ToolTip;

namespace AtomUI.Desktop.Controls.Tests.Tooltip;

public class ToolTipSemanticPartTests
{
    private const string ContainerClass = "semantic-container";
    private const string ArrowClass = "semantic-arrow";
    private const string ScopeAnchorClass = "semantic-scope-arrow-decorated-box";

    private const string ToolTipThemePath =
        "src/AtomUI.Desktop.Controls/Tooltip/Themes/ToolTipTheme.axaml";
    private const string ArrowDecoratedBoxThemePath =
        "src/AtomUI.Desktop.Controls/Primitives/ArrowDecoratedBox/Themes/ArrowDecoratedBoxTheme.axaml";

    private static readonly string[] ApprovedPartNames =
    [
        "root", "arrow", "container"
    ];

    static ToolTipSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_ToolTip_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIToolTip), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUIToolTip));
        AssertPart(descriptor, "container", ContainerClass, typeof(Border),
            "/template/ .semantic-scope-arrow-decorated-box /template/ .semantic-container",
            SemanticPartCardinality.Single);
        AssertPart(descriptor, "arrow", ArrowClass, typeof(ArrowIndicator),
            "/template/ .semantic-scope-arrow-decorated-box /template/ .semantic-arrow",
            SemanticPartCardinality.Optional);
    }

    [Fact]
    public void Built_In_Templates_Implement_The_Approved_Static_Markers()
    {
        // ToolTip 宿主模板承载跨嵌套锚点
        var hostDocument = XDocument.Load(GetRepoFile(ToolTipThemePath), LoadOptions.SetLineInfo);
        var hostTemplates = hostDocument.Descendants()
                                        .Where(static element => element.Name.LocalName == "ControlTemplate")
                                        .ToArray();
        hostTemplates.Length.ShouldBe(1);

        foreach (var template in hostTemplates)
        {
            var anchors = template.Descendants()
                                  .Where(static element => element.Attributes()
                                      .Any(static attribute =>
                                          attribute.Name.LocalName == $"Classes.{ScopeAnchorClass}"))
                                  .Select(static element => element.Name.LocalName)
                                  .ToArray();
            anchors.ShouldBe(["ArrowDecoratedBox"]);
        }

        // 共享 ArrowDecoratedBox 主题承载 container / arrow marker
        var boxDocument = XDocument.Load(GetRepoFile(ArrowDecoratedBoxThemePath), LoadOptions.SetLineInfo);
        var boxTemplates = boxDocument.Descendants()
                                      .Where(static element => element.Name.LocalName == "ControlTemplate")
                                      .ToArray();
        boxTemplates.Length.ShouldBe(1);

        foreach (var template in boxTemplates)
        {
            var markers = template.Descendants()
                                  .SelectMany(static element => element.Attributes()
                                      .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                          "Classes.semantic-",
                                          StringComparison.Ordinal)))
                                  .Select(static attribute => $"{attribute.Name.LocalName}:{attribute.Parent!.Name.LocalName}")
                                  .ToArray();
            markers.ShouldBe([
                "Classes.semantic-arrow:ArrowIndicator",
                "Classes.semantic-container:Border"
            ]);
        }

        hostDocument.Descendants()
                    .Any(static element => element.Attributes()
                        .Any(static attribute => attribute.Name.LocalName == "Classes.semantic-root"))
                    .ShouldBeFalse();
    }

    [Fact]
    public void Default_Theme_Does_Not_Consume_Semantic_Selectors()
    {
        foreach (var path in new[] { ToolTipThemePath, ArrowDecoratedBoxThemePath })
        {
            var document = XDocument.Load(GetRepoFile(path), LoadOptions.SetLineInfo);
            var selectors = document.Descendants()
                                    .Where(static element => element.Name.LocalName == "Style")
                                    .Attributes("Selector")
                                    .Select(static attribute => attribute.Value)
                                    .ToArray();

            selectors.ShouldAllBe(static selector =>
                !selector.Contains("semantic-", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_Template_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIToolTip), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var tooltip = new AtomUIToolTip
        {
            Content = "tooltip prompt text",
            IsMotionEnabled = false
        };
        tooltip.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIToolTip>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        tooltip.Styles.Add(ownerStyle);

        var window = Show(tooltip);
        try
        {
            tooltip.Tag.ShouldBe("root");
            FindSemanticControl<Border>(tooltip, ContainerClass).Tag.ShouldBe("container");
            FindSemanticControl<ArrowIndicator>(tooltip, ArrowClass).Tag.ShouldBe("arrow");
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertRoot(ControlSemanticDescriptor descriptor, Type controlType)
    {
        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.SelectorRoute.ShouldBeNull();
        root.ContractType.ShouldBe(controlType);
        root.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        root.Customization.ShouldBe(SemanticPartCustomization.Root);
        root.StyleType.ShouldBeNull();
    }

    private static void AssertPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType,
        string selectorRoute,
        SemanticPartCardinality cardinality)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.CrossNestedOwners.ShouldBeTrue();
        part.RuntimeCreated.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
    }

    private static T FindSemanticControl<T>(AtomUIToolTip owner, string marker)
        where T : Control
    {
        return owner.GetVisualDescendants()
                    .OfType<T>()
                    .Single(control => control.Classes.Contains(marker));
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width = 480,
            Height = 160,
            Content = content
        };
        window.Show();
        content.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }
}
