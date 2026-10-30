using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AtomUICompactSpace = AtomUI.Desktop.Controls.CompactSpace;
using AtomUICompactSpaceAddOn = AtomUI.Desktop.Controls.CompactSpaceAddOn;
using AtomUISpace = AtomUI.Desktop.Controls.Space;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Space;

public class SpaceSemanticPartTests
{
    private const string ItemClass      = "semantic-item";
    private const string SeparatorClass = "semantic-separator";

    static SpaceSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_The_Ant_Design_Aligned_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUISpace), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "item", "separator"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(descriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass,
            typeof(Control),
            "> .semantic-item");
        AssertPart(descriptor.Parts.Single(static part => part.Name == "separator"),
            SeparatorClass,
            typeof(Control),
            "> .semantic-separator");

        registry.TryGetControl(typeof(AtomUICompactSpace), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(AtomUICompactSpaceAddOn), out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/Space/Themes/CompactSpaceTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/Space/Themes/CompactSpaceAddOnTheme.axaml")]
    public void Built_In_Themes_Declare_No_Static_Semantic_Markers(string relativePath)
    {
        AssertThemeMarkers(relativePath, Array.Empty<string>());
    }

    [Fact]
    public void Direct_Children_Are_Runtime_Item_Parts_And_Removal_Cleans_Owned_Markers()
    {
        var first  = new AtomUIButton { Content = "First" };
        var second = new AtomUIButton { Content = "Second" };
        var space = new AtomUISpace
        {
            Children =
            {
                first,
                second
            }
        };

        using var window = Show(space);

        first.Classes.ShouldContain(ItemClass);
        second.Classes.ShouldContain(ItemClass);
        space.Classes.ShouldNotContain("semantic-root");

        space.Children.Remove(first);
        Dispatcher.UIThread.RunJobs();

        first.Classes.ShouldNotContain(ItemClass);
        second.Classes.ShouldContain(ItemClass);
    }

    [Fact]
    public void SplitTemplate_Creates_Runtime_Separator_Parts_Between_Items()
    {
        var space = CreateSeparatedSpace();

        using var window = Show(space);

        GetSemanticElements(space, ItemClass).Length.ShouldBe(3);
        GetSemanticElements(space, SeparatorClass).Length.ShouldBe(2);
    }

    [Fact]
    public void Generated_Route_Styles_Apply_To_Direct_Items_And_Separators()
    {
        var space = CreateSeparatedSpace();
        space.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUISpace>().Class("semantic-owner"));
        ownerStyle.Children.Add(new SpaceItemStyle
        {
            Setters = { new Setter(Control.TagProperty, "item") }
        });
        ownerStyle.Children.Add(new SpaceSeparatorStyle
        {
            Setters = { new Setter(Control.TagProperty, "separator") }
        });
        space.Styles.Add(ownerStyle);

        using var window = Show(space);

        GetSemanticElements(space, ItemClass)
            .OfType<Control>()
            .ShouldAllBe(static item => Equals(item.Tag, "item"));
        GetSemanticElements(space, SeparatorClass)
            .OfType<Control>()
            .ShouldAllBe(static separator => Equals(separator.Tag, "separator"));
    }

    private static AtomUISpace CreateSeparatedSpace()
    {
        return new AtomUISpace
        {
            SplitTemplate = new SeparatorTemplate(),
            Children =
            {
                new AtomUIButton { Content = "Primary" },
                new AtomUIButton { Content = "Default" },
                new AtomUIButton { Content = "Dashed" }
            }
        };
    }

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.SelectorRoute.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUISpace));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
    }

    private static void AssertPart(
        SemanticPartDescriptor part,
        string selectorClass,
        Type contractType,
        string selectorRoute)
    {
        part.Path.ShouldBe(part.Name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Multiple);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeTrue();
        part.StyleType.ShouldNotBeNull();
        part.Since.ShouldBe("6.2.0");
    }

    private static Control[] GetSemanticElements(AtomUISpace space, string semanticClass)
    {
        return space.GetVisualDescendants()
                    .OfType<Control>()
                    .Where(control => control.Classes.Contains(semanticClass))
                    .ToArray();
    }

    private static IDisposable Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 480,
            Height  = 260,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowLifetime(window);
    }

    private static void AssertThemeMarkers(string relativePath, string[] expectedMarkers)
    {
        var document = XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);
        var literalSemanticMarkers = document.Descendants()
                                             .Attributes()
                                             .Where(static attribute =>
                                                 attribute.Name.LocalName == "Classes" &&
                                                 attribute.Value.Split(
                                                             (char[]?)null,
                                                             StringSplitOptions.RemoveEmptyEntries)
                                                         .Any(static value => value.StartsWith(
                                                             "semantic-",
                                                             StringComparison.Ordinal)))
                                             .ToArray();
        var classPropertyMarkers = document.Descendants()
                                           .SelectMany(static element => element.Attributes()
                                               .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                                   "Classes.semantic-",
                                                   StringComparison.Ordinal))
                                               .Select(attribute => (Element: element, Attribute: attribute)))
                                           .ToArray();

        literalSemanticMarkers.ShouldBeEmpty();
        classPropertyMarkers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        classPropertyMarkers.Select(static marker =>
                                $"{marker.Attribute.Name.LocalName["Classes.".Length..]}:{marker.Element.Name.LocalName}")
                            .OrderBy(static value => value, StringComparer.Ordinal)
                            .ShouldBe(expectedMarkers);
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

    private sealed class SeparatorTemplate : ITemplate<Control>
    {
        public Control Build()
        {
            return new Border
            {
                Width  = 1,
                Height = 16
            };
        }

        object ITemplate.Build() => Build();
    }

    private sealed class WindowLifetime(AvaloniaWindow window) : IDisposable
    {
        public void Dispose()
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
