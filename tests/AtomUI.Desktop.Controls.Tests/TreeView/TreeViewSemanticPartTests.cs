using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomTreeView = AtomUI.Desktop.Controls.TreeView;
using AtomTreeViewItem = AtomUI.Desktop.Controls.TreeViewItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TreeView;

public class TreeViewSemanticPartTests
{
    private const string ItemClass = "semantic-item";
    private const string ScopeHeaderClass = "semantic-scope-header";
    private const string SwitcherClass = "semantic-item-switcher";
    private const string IconClass = "semantic-item-icon";
    private const string TitleClass = "semantic-item-title";
    private const string IndicatorClass = "semantic-item-indicator";
    private const string OwnerClass = "semantic-owner";

    static TreeViewSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_Only_The_Approved_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        // TreeView owns the root and its direct (top-level) node containers.
        registry.TryGetControl(typeof(AtomTreeView), out var treeViewDescriptor).ShouldBeTrue();
        treeViewDescriptor.ShouldNotBeNull();
        treeViewDescriptor.Parts.Select(static part => part.Name)
                        .ShouldBe(["root", "item"]);
        AssertRoot(treeViewDescriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomTreeView));
        AssertPart(treeViewDescriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass,
            typeof(AtomTreeViewItem),
            "> .semantic-item");

        // TreeViewItem is a recursive nested owner: its own child containers and the
        // switcher / icon / title content regions of each node.
        registry.TryGetControl(typeof(AtomTreeViewItem), out var itemDescriptor).ShouldBeTrue();
        itemDescriptor.ShouldNotBeNull();
        itemDescriptor.Parts.Select(static part => part.Name)
                    .ShouldBe(
                        [
                            "root",
                            "item",
                            "itemIcon",
                            "itemIndicator",
                            "itemSwitcher",
                            "itemTitle"
                        ]);
        AssertRoot(itemDescriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomTreeViewItem));
        AssertPart(itemDescriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass,
            typeof(AtomTreeViewItem),
            "> .semantic-item");
        AssertPart(itemDescriptor.Parts.Single(static part => part.Name == "itemSwitcher"),
            SwitcherClass,
            typeof(ToggleButton),
            "/template/ .semantic-scope-header /template/ .semantic-item-switcher");
        AssertPart(itemDescriptor.Parts.Single(static part => part.Name == "itemIcon"),
            IconClass,
            typeof(IconPresenter),
            "/template/ .semantic-scope-header /template/ .semantic-item-icon");
        AssertPart(itemDescriptor.Parts.Single(static part => part.Name == "itemTitle"),
            TitleClass,
            typeof(ContentPresenter),
            "/template/ .semantic-scope-header /template/ .semantic-item-title");
        AssertPart(itemDescriptor.Parts.Single(static part => part.Name == "itemIndicator"),
            IndicatorClass,
            typeof(ToggleButton),
            "/template/ .semantic-scope-header /template/ .semantic-item-indicator");

        registry.TryGetControl(typeof(AtomUI.Desktop.Controls.FloatableTreeView), out _).ShouldBeFalse();
    }

    [Fact]
    public void Templates_Implement_Only_The_Approved_Static_Markers()
    {
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/TreeView/Themes/TreeViewTheme.axaml",
            Array.Empty<string>());
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/TreeView/Themes/TreeViewItemTheme.axaml",
            [
                "semantic-scope-header:TreeViewItemHeader"
            ]);
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/TreeView/Themes/TreeViewItemHeaderTheme.axaml",
            [
                "semantic-item-icon:IconPresenter",
                "semantic-item-indicator:CheckBox",
                "semantic-item-indicator:RadioButton",
                "semantic-item-switcher:NodeSwitcherButton",
                "semantic-item-title:ContentPresenter"
            ]);
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/TreeView/Themes/NodeSwitcherButtonTheme.axaml",
            Array.Empty<string>());
    }

    [Fact]
    public void Expanded_Nodes_Produce_The_Full_Marker_Set_Per_Container()
    {
        var treeView = CreateThreeNodeTree();

        ShowInWindow(treeView, () =>
        {
            AssertMarkerCounts(treeView, 3);
        });
    }

    [Fact]
    public void Empty_Items_Produce_No_Item_Markers()
    {
        var treeView = new AtomTreeView
        {
            IsShowEmptyIndicator = false
        };

        ShowInWindow(treeView, () =>
        {
            AssertMarkerCounts(treeView, 0);
        });
    }

    [Fact]
    public void TreeView_Owner_Item_Route_Reaches_Top_Level_Only()
    {
        var treeView = CreateThreeNodeTree();
        treeView.Classes.Add(OwnerClass);
        treeView.Styles.Add(new Style(selector =>
            selector.OfType<AtomTreeView>().Class(OwnerClass).Child().Class(ItemClass))
        {
            Setters = { new Setter(Control.TagProperty, "top-item") }
        });

        ShowInWindow(treeView, () =>
        {
            GetTopLevelItems(treeView).ShouldAllBe(static item => Equals(item.Tag, "top-item"));
            GetNestedItems(treeView).ShouldAllBe(static item => item.Tag == null);
        });
    }

    [Fact]
    public void TreeViewItem_Owner_Routes_Reach_Nested_Containers_And_All_Node_Content()
    {
        var treeView = CreateThreeNodeTree();
        treeView.Styles.Add(new Style(selector =>
            selector.OfType<AtomTreeViewItem>().Child().Class(ItemClass))
        {
            Setters = { new Setter(Control.TagProperty, "nested-item") }
        });
        treeView.Styles.Add(new Style(selector =>
            selector.OfType<AtomTreeViewItem>()
                    .Template()
                    .Class(ScopeHeaderClass)
                    .Template()
                    .Class(SwitcherClass))
        {
            Setters = { new Setter(Control.TagProperty, "switcher") }
        });
        treeView.Styles.Add(new Style(selector =>
            selector.OfType<AtomTreeViewItem>()
                    .Template()
                    .Class(ScopeHeaderClass)
                    .Template()
                    .Class(IconClass))
        {
            Setters = { new Setter(Control.TagProperty, "icon") }
        });
        treeView.Styles.Add(new Style(selector =>
            selector.OfType<AtomTreeViewItem>()
                    .Template()
                    .Class(ScopeHeaderClass)
                    .Template()
                    .Class(TitleClass))
        {
            Setters = { new Setter(Control.TagProperty, "title") }
        });
        treeView.Styles.Add(new Style(selector =>
            selector.OfType<AtomTreeViewItem>()
                    .Template()
                    .Class(ScopeHeaderClass)
                    .Template()
                    .Class(IndicatorClass))
        {
            Setters = { new Setter(Control.TagProperty, "indicator") }
        });

        ShowInWindow(treeView, () =>
        {
            // Nested containers are the child items of the top-level TreeViewItem.
            GetNestedItems(treeView).ShouldAllBe(static item => Equals(item.Tag, "nested-item"));
            GetTopLevelItems(treeView).ShouldAllBe(static item => item.Tag == null);

            // Content parts are owner-scoped to TreeViewItem, so every node's
            // switcher / icon / title / indicator is reached regardless of depth.
            GetSemanticSwitchers(treeView).ShouldAllBe(static switcher => Equals(switcher.Tag, "switcher"));
            GetSemanticIcons(treeView).ShouldAllBe(static icon => Equals(icon.Tag, "icon"));
            GetSemanticTitles(treeView).ShouldAllBe(static title => Equals(title.Tag, "title"));
            GetSemanticIndicators(treeView).ShouldAllBe(static indicator => Equals(indicator.Tag, "indicator"));
        });
    }

    [Fact]
    public void Generated_Route_Styles_Apply_Real_Setters()
    {
        var treeView = CreateThreeNodeTree();
        var switcherBrush = new SolidColorBrush(Colors.LightYellow);
        var iconBrush = new SolidColorBrush(Colors.DarkGreen);
        treeView.Classes.Add(OwnerClass);
        treeView.Styles.Add(new Style(selector =>
            selector.OfType<AtomTreeView>().Class(OwnerClass).Child().Class(ItemClass))
        {
            Setters = { new Setter(TemplatedControl.MinHeightProperty, 40d) }
        });
        treeView.Styles.Add(new Style(selector =>
            selector.OfType<AtomTreeViewItem>()
                    .Template()
                    .Class(ScopeHeaderClass)
                    .Template()
                    .Class(SwitcherClass))
        {
            Setters = { new Setter(TemplatedControl.BackgroundProperty, switcherBrush) }
        });
        treeView.Styles.Add(new Style(selector =>
            selector.OfType<AtomTreeViewItem>()
                    .Template()
                    .Class(ScopeHeaderClass)
                    .Template()
                    .Class(IconClass))
        {
            Setters = { new Setter(IconPresenter.IconBrushProperty, iconBrush) }
        });
        treeView.Styles.Add(new Style(selector =>
            selector.OfType<AtomTreeViewItem>()
                    .Template()
                    .Class(ScopeHeaderClass)
                    .Template()
                    .Class(TitleClass))
        {
            Setters = { new Setter(ContentPresenter.MarginProperty, new Thickness(8, 0, 0, 0)) }
        });
        treeView.Styles.Add(new Style(selector =>
            selector.OfType<AtomTreeViewItem>()
                    .Template()
                    .Class(ScopeHeaderClass)
                    .Template()
                    .Class(IndicatorClass))
        {
            Setters = { new Setter(TemplatedControl.MarginProperty, new Thickness(0, 0, 8, 0)) }
        });

        ShowInWindow(treeView, () =>
        {
            // TreeView's item route styles only the direct (top-level) containers.
            GetTopLevelItems(treeView).ShouldAllBe(static item => Equals(item.MinHeight, 40d));
            GetNestedItems(treeView).ShouldAllBe(static item => !Equals(item.MinHeight, 40d));

            // TreeViewItem content routes reach every node's content.
            GetSemanticSwitchers(treeView)
                .ShouldAllBe(static switcher => HasColor(switcher.Background, Colors.LightYellow));
            GetSemanticIcons(treeView)
                .ShouldAllBe(static icon => HasColor(icon.IconBrush, Colors.DarkGreen));
            GetSemanticTitles(treeView)
                .ShouldAllBe(static title => title.Margin == new Thickness(8, 0, 0, 0));
            GetSemanticIndicators(treeView)
                .ShouldAllBe(static indicator => indicator.Margin == new Thickness(0, 0, 8, 0));
        });
    }

    private static AtomTreeView CreateThreeNodeTree()
    {
        var childA = new TreeItemNode { Header = "child-a", ItemKey = "0-0" };
        var childB = new TreeItemNode { Header = "child-b", ItemKey = "0-1" };
        var root = new TreeItemNode
        {
            Header     = "root",
            ItemKey    = "0",
            IsExpanded = true,
            Children   = [childA, childB]
        };
        return new AtomTreeView
        {
            IsShowEmptyIndicator = false,
            ItemsSource          = new[] { root }
        };
    }

    private static void AssertMarkerCounts(AtomTreeView treeView, int expectedItems)
    {
        GetSemanticItems(treeView).Length.ShouldBe(expectedItems);
        GetScopeHeaders(treeView).Length.ShouldBe(expectedItems);
        GetSemanticSwitchers(treeView).Length.ShouldBe(expectedItems);
        GetSemanticIcons(treeView).Length.ShouldBe(expectedItems);
        GetSemanticTitles(treeView).Length.ShouldBe(expectedItems);
        // Each container hosts a checkbox and a radio alternative, only one of
        // which is visible depending on ToggleType.
        GetSemanticIndicators(treeView).Length.ShouldBe(expectedItems * 2);
    }

    private static AtomTreeViewItem[] GetSemanticItems(AtomTreeView treeView)
    {
        return treeView.GetVisualDescendants()
                       .OfType<AtomTreeViewItem>()
                       .Where(static item => item.Classes.Contains(ItemClass))
                       .ToArray();
    }

    private static AtomTreeViewItem[] GetTopLevelItems(AtomTreeView treeView)
    {
        return GetSemanticItems(treeView)
               .Where(item => ReferenceEquals(item.Parent, treeView))
               .ToArray();
    }

    private static AtomTreeViewItem[] GetNestedItems(AtomTreeView treeView)
    {
        return GetSemanticItems(treeView)
               .Where(static item => item.Parent is AtomTreeViewItem)
               .ToArray();
    }

    private static Visual[] GetScopeHeaders(AtomTreeView treeView)
    {
        return treeView.GetVisualDescendants()
                       .Where(static visual => visual.Classes.Contains(ScopeHeaderClass))
                       .ToArray();
    }

    private static ToggleButton[] GetSemanticSwitchers(AtomTreeView treeView)
    {
        return treeView.GetVisualDescendants()
                       .OfType<ToggleButton>()
                       .Where(static switcher => switcher.Classes.Contains(SwitcherClass))
                       .ToArray();
    }

    private static IconPresenter[] GetSemanticIcons(AtomTreeView treeView)
    {
        return treeView.GetVisualDescendants()
                       .OfType<IconPresenter>()
                       .Where(static icon => icon.Classes.Contains(IconClass))
                       .ToArray();
    }

    private static ContentPresenter[] GetSemanticTitles(AtomTreeView treeView)
    {
        return treeView.GetVisualDescendants()
                       .OfType<ContentPresenter>()
                       .Where(static title => title.Classes.Contains(TitleClass))
                       .ToArray();
    }

    private static ToggleButton[] GetSemanticIndicators(AtomTreeView treeView)
    {
        return treeView.GetVisualDescendants()
                       .OfType<ToggleButton>()
                       .Where(static indicator => indicator.Classes.Contains(IndicatorClass))
                       .ToArray();
    }

    private static bool HasColor(IBrush? brush, Color color)
    {
        return brush is SolidColorBrush solid && solid.Color == color;
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
