using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISplitter = AtomUI.Desktop.Controls.Splitter;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Splitter;

public class SplitterSemanticPartTests
{
    private const string PanelClass = "semantic-panel";
    private const string DraggerClass = "semantic-dragger";
    private const string ScopePanelClass = "semantic-scope-panel";
    private const string ScopeHandleClass = "semantic-scope-handle";

    static SplitterSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_Only_The_AntDesign_Aligned_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUISplitter), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "dragger", "panel"]);
        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomUISplitter));
        AssertPart(descriptor.Parts.Single(static part => part.Name == "panel"),
            PanelClass,
            typeof(Control),
            "/template/ .semantic-scope-panel > .semantic-panel",
            SemanticPartCardinality.Multiple);
        AssertPart(descriptor.Parts.Single(static part => part.Name == "dragger"),
            DraggerClass,
            typeof(AtomUI.Controls.Primitives.Thumb),
            "/template/ .semantic-scope-panel > .semantic-scope-handle /template/ .semantic-dragger",
            SemanticPartCardinality.Multiple);
    }

    [Fact]
    public void Templates_Declare_Only_Approved_Static_Semantic_Markers()
    {
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Splitter/Themes/SplitterTheme.axaml",
            ["semantic-scope-panel:SplitterPanel"]);
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Splitter/Themes/SplitterHandleTheme.axaml",
            ["semantic-dragger:SplitterDragBar"]);
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Splitter/Themes/SplitterDragBarTheme.axaml",
            Array.Empty<string>());
    }

    [Fact]
    public void Two_Panels_Produce_Two_Panel_Markers_And_One_Dragger()
    {
        var first = CreatePanel();
        var second = CreatePanel();
        var splitter = CreateSplitter(first, second);

        ShowInWindow(splitter, () =>
        {
            GetSemanticElements(splitter, PanelClass).ShouldBe([first, second]);
            GetSemanticElements(splitter, DraggerClass).Length.ShouldBe(1);
            GetSemanticElements(splitter, ScopePanelClass).Length.ShouldBe(1);
            GetSemanticElements(splitter, ScopeHandleClass).Length.ShouldBe(1);
        });
    }

    [Fact]
    public void Children_Changes_Sync_Panel_And_Dragger_Markers()
    {
        var first = CreatePanel();
        var second = CreatePanel();
        var third = CreatePanel();
        var splitter = CreateSplitter(first, second, third);

        ShowInWindow(splitter, () =>
        {
            AssertMarkerCounts(splitter, expectedPanels: 3, expectedDraggers: 2);

            splitter.Children.Remove(second);
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(splitter, expectedPanels: 2, expectedDraggers: 1);
            second.Classes.ShouldNotContain(PanelClass);

            var replacement = CreatePanel();
            splitter.Children[1] = replacement;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(splitter, expectedPanels: 2, expectedDraggers: 1);
            third.Classes.ShouldNotContain(PanelClass);
            replacement.Classes.ShouldContain(PanelClass);
        });
    }

    [Fact]
    public void Removing_Panel_Does_Not_Remove_User_Owned_Semantic_Class()
    {
        var preMarked = CreatePanel();
        preMarked.Classes.Add(PanelClass);
        var addedBySplitter = CreatePanel();
        var splitter = CreateSplitter(preMarked, addedBySplitter);

        ShowInWindow(splitter, () =>
        {
            AssertMarkerCounts(splitter, expectedPanels: 2, expectedDraggers: 1);

            splitter.Children.Remove(preMarked);
            splitter.Children.Remove(addedBySplitter);
            Dispatcher.UIThread.RunJobs();

            preMarked.Classes.ShouldContain(PanelClass);
            addedBySplitter.Classes.ShouldNotContain(PanelClass);
            AssertMarkerCounts(splitter, expectedPanels: 0, expectedDraggers: 0);
        });
    }

    [Fact]
    public void Disabled_And_Collapsed_States_Preserve_Semantic_Marker_Counts()
    {
        var first = CreatePanel();
        var second = CreatePanel();
        var third = CreatePanel();
        AtomUISplitter.SetIsResizable(second, false);
        AtomUISplitter.SetCollapsible(first, SplitterPanelCollapsible.Parse("Always"));
        var splitter = CreateSplitter(first, second, third);

        ShowInWindow(splitter, () =>
        {
            AssertMarkerCounts(splitter, expectedPanels: 3, expectedDraggers: 2);

            AtomUISplitter.SetIsCollapsed(first, true);
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(splitter, expectedPanels: 3, expectedDraggers: 2);

            AtomUISplitter.SetIsResizable(second, true);
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(splitter, expectedPanels: 3, expectedDraggers: 2);
        });
    }

    [Fact]
    public void Generated_Route_Styles_Apply_To_Panels_And_Draggers()
    {
        var first = CreatePanel();
        var second = CreatePanel();
        var splitter = CreateSplitter(first, second);
        var panelBrush = new SolidColorBrush(Colors.Lavender);
        var draggerBrush = new SolidColorBrush(Colors.DeepSkyBlue);

        var ownerStyle = new Style(selector => selector.OfType<AtomUISplitter>());
        ownerStyle.Children.Add(new SplitterPanelStyle
        {
            Setters = { new Setter(Border.BackgroundProperty, panelBrush) }
        });
        ownerStyle.Children.Add(new SplitterDraggerStyle
        {
            Setters = { new Setter(TemplatedControl.BackgroundProperty, draggerBrush) }
        });
        splitter.Styles.Add(ownerStyle);

        ShowInWindow(splitter, () =>
        {
            GetSemanticElements(splitter, PanelClass)
                .OfType<Border>()
                .ShouldAllBe(panel => panel.Background == panelBrush);
            GetSemanticElements(splitter, DraggerClass)
                .OfType<TemplatedControl>()
                .ShouldAllBe(dragger => dragger.Background == draggerBrush);
        });
    }

    private static AtomUISplitter CreateSplitter(params Control[] children)
    {
        var splitter = new AtomUISplitter
        {
            Orientation = Orientation.Vertical,
            Width       = 360,
            Height      = 180
        };

        foreach (var child in children)
        {
            splitter.Children.Add(child);
        }

        return splitter;
    }

    private static Border CreatePanel()
    {
        return new Border
        {
            MinWidth = 20,
            Child = new TextBlock
            {
                Text = "Panel"
            }
        };
    }

    private static void AssertMarkerCounts(
        AtomUISplitter splitter,
        int expectedPanels,
        int expectedDraggers)
    {
        GetSemanticElements(splitter, PanelClass).Length.ShouldBe(expectedPanels);
        GetSemanticElements(splitter, DraggerClass).Length.ShouldBe(expectedDraggers);
    }

    private static Visual[] GetSemanticElements(AtomUISplitter splitter, string semanticClass)
    {
        return splitter.GetVisualDescendants()
                       .Where(visual => visual.Classes.Contains(semanticClass))
                       .ToArray();
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 800,
            Height  = 600,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertRoot(SemanticPartDescriptor part, Type contractType)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
    }

    private static void AssertPart(
        SemanticPartDescriptor part,
        string selectorClass,
        Type contractType,
        string selectorRoute,
        SemanticPartCardinality cardinality)
    {
        part.Path.ShouldBe(part.Name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeTrue();
        part.Since.ShouldBe("6.2.0");
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
}
