using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AtomUI.Controls.Primitives;
using Shouldly;
using Xunit;
using AtomUICollapse = AtomUI.Desktop.Controls.Collapse;
using AtomUICollapseItem = AtomUI.Desktop.Controls.CollapseItem;
using IconButtonControl = AtomUI.Desktop.Controls.IconButton;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.CollapseControl;

public class CollapseSemanticPartTests
{
    private const string HeaderClass = "semantic-header";
    private const string IconClass = "semantic-icon";
    private const string TitleClass = "semantic-title";
    private const string BodyClass = "semantic-body";
    private const string ScopeItemsClass = "semantic-scope-items";
    private const string ScopePanelClass = "semantic-scope-panel";
    private const string ScopeItemClass = "semantic-scope-item";
    private const string RoutePrefix = "> .semantic-scope-item /template/ .";

    static CollapseSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUICollapse), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "body", "header", "icon", "title"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(descriptor.Parts.Single(static part => part.Name == "header"),
            HeaderClass,
            typeof(PixelAlignedBorder),
            RoutePrefix + HeaderClass);
        AssertPart(descriptor.Parts.Single(static part => part.Name == "icon"),
            IconClass,
            typeof(IconButtonControl),
            RoutePrefix + IconClass);
        AssertPart(descriptor.Parts.Single(static part => part.Name == "title"),
            TitleClass,
            typeof(ContentPresenter),
            RoutePrefix + TitleClass);
        AssertPart(descriptor.Parts.Single(static part => part.Name == "body"),
            BodyClass,
            typeof(PixelAlignedBorder),
            RoutePrefix + BodyClass);

        registry.TryGetControl(typeof(AtomUICollapseItem), out _).ShouldBeFalse();
    }

    [Fact]
    public void Templates_Implement_Only_The_Approved_Static_Markers()
    {
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Collapse/Themes/CollapseTheme.axaml",
            ["semantic-scope-items:ItemsPresenter"]);
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Collapse/Themes/CollapseItemTheme.axaml",
            [
                "semantic-body:PixelAlignedBorder",
                "semantic-header:PixelAlignedBorder",
                "semantic-icon:IconButton",
                "semantic-title:ContentPresenter"
            ]);
    }

    [Fact]
    public void Panels_Produce_One_Part_Marker_Set_Per_Item_And_A_Single_Scope_Chain()
    {
        var collapse = CreateCollapseWithThreePanels();

        ShowInWindow(collapse, () =>
        {
            // The body marker lives inside the content motion actor: it is materialized
            // only while the panel is expanded, so expand every panel before counting.
            ExpandAllPanels(collapse);
            GetSemanticHeaders(collapse).Length.ShouldBe(3);
            GetSemanticIcons(collapse).Length.ShouldBe(3);
            GetSemanticTitles(collapse).Length.ShouldBe(3);
            GetSemanticBodies(collapse).Length.ShouldBe(3);

            var presenter = GetItemsPresenter(collapse);
            presenter.Classes.ShouldContain(ScopeItemsClass);
            var panel = GetItemsPanel(collapse);
            panel.Classes.ShouldContain(ScopePanelClass);
            panel.ShouldBeSameAs(presenter.Panel);
            GetScopeItemContainers(collapse).Length.ShouldBe(3);
        });
    }

    [Fact]
    public void Body_Marker_Follows_Panel_Expansion_State()
    {
        var collapse = CreateCollapseWithThreePanels();

        ShowInWindow(collapse, () =>
        {
            GetSemanticHeaders(collapse).Length.ShouldBe(3);
            GetSemanticTitles(collapse).Length.ShouldBe(3);
            GetSemanticBodies(collapse).Length.ShouldBe(1);

            ExpandAllPanels(collapse);
            GetSemanticBodies(collapse).Length.ShouldBe(3);
        });
    }

    [Fact]
    public void Empty_Collection_Keeps_The_Scope_Chain_But_Has_No_Part_Markers()
    {
        var collapse = new AtomUICollapse();

        ShowInWindow(collapse, () =>
        {
            GetItemsPresenter(collapse).Classes.ShouldContain(ScopeItemsClass);
            GetItemsPanel(collapse).Classes.ShouldContain(ScopePanelClass);
            GetSemanticHeaders(collapse).ShouldBeEmpty();
            GetSemanticIcons(collapse).ShouldBeEmpty();
            GetSemanticTitles(collapse).ShouldBeEmpty();
            GetSemanticBodies(collapse).ShouldBeEmpty();
            GetScopeItemContainers(collapse).ShouldBeEmpty();
        });
    }

    [Fact]
    public void Items_Clear_And_ReAdd_Tracks_The_Part_Markers()
    {
        var collapse = CreateCollapseWithThreePanels();

        ShowInWindow(collapse, () =>
        {
            GetSemanticHeaders(collapse).Length.ShouldBe(3);

            collapse.Items.Clear();
            Dispatcher.UIThread.RunJobs();
            GetSemanticHeaders(collapse).ShouldBeEmpty();
            GetScopeItemContainers(collapse).ShouldBeEmpty();
            GetItemsPanel(collapse).Classes.ShouldContain(ScopePanelClass);

            collapse.Items.Add(CreateItem("1", true));
            collapse.Items.Add(CreateItem("2", true));
            Dispatcher.UIThread.RunJobs();
            GetSemanticHeaders(collapse).Length.ShouldBe(2);
            GetSemanticBodies(collapse).Length.ShouldBe(2);
            GetScopeItemContainers(collapse).Length.ShouldBe(2);
        });
    }

    [Fact]
    public void ShowExpandIcon_False_Hides_The_Icon_But_Preserves_The_Marker()
    {
        var collapse = new AtomUICollapse
        {
            Items =
            {
                CreateItem("1", true),
                new AtomUICollapseItem
                {
                    Header     = "Header 2",
                    Content    = "Content 2",
                    IsSelected = false,
                    IsShowExpandIcon = false
                }
            }
        };

        ShowInWindow(collapse, () =>
        {
            var icons = GetSemanticIcons(collapse);
            icons.Length.ShouldBe(2);
            icons.ShouldAllBe(static icon => icon.Classes.Contains(IconClass));
            icons.Single(static icon => !icon.IsVisible);
            icons.Single(static icon => icon.IsVisible);
        });
    }

    [Fact]
    public void State_And_Visual_Mode_Switches_Do_Not_Change_Marker_Counts()
    {
        var collapse = CreateCollapseWithThreePanels();

        ShowInWindow(collapse, () =>
        {
            ExpandAllPanels(collapse);
            AssertPanelMarkerCounts(collapse, 3, 3);

            collapse.SizeType          = CustomizableSizeType.Large;
            collapse.ExpandIconPosition = CollapseExpandIconPosition.End;
            collapse.IsBorderless      = true;
            collapse.IsGhostStyle      = true;
            collapse.IsAccordion       = true;
            collapse.TriggerType       = CollapseTriggerType.Icon;
            Dispatcher.UIThread.RunJobs();
            AssertPanelMarkerCounts(collapse, 3, 3);

            foreach (var item in GetScopeItemContainers(collapse))
            {
                item.IsSelected = !item.IsSelected;
            }
            Dispatcher.UIThread.RunJobs();
            ExpandAllPanels(collapse);
            AssertPanelMarkerCounts(collapse, 3, 3);

            collapse.IsGhostStyle = false;
            collapse.IsBorderless = false;
            collapse.IsAccordion  = false;
            Dispatcher.UIThread.RunJobs();
            AssertPanelMarkerCounts(collapse, 3, 3);
        });
    }

    [Fact]
    public void Detach_And_Reattach_Regenerates_The_Scope_Chain_And_Keeps_Every_Panel_Marked()
    {
        var collapse = CreateCollapseWithThreePanels();
        var window = new AvaloniaWindow { Width = 800, Height = 600 };

        try
        {
            window.Show();
            window.Content = collapse;
            Dispatcher.UIThread.RunJobs();
            ExpandAllPanels(collapse);
            AssertPanelMarkerCounts(collapse, 3, 3);

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            window.Content = collapse;
            Dispatcher.UIThread.RunJobs();

            ExpandAllPanels(collapse);
            AssertPanelMarkerCounts(collapse, 3, 3);
            GetItemsPresenter(collapse).Classes.ShouldContain(ScopeItemsClass);
            GetItemsPanel(collapse).Classes.ShouldContain(ScopePanelClass);
            GetScopeItemContainers(collapse).Length.ShouldBe(3);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Root_Semantic_Part_Projects_The_Standard_Surface_Properties()
    {
        var collapse = CreateCollapseWithThreePanels();
        collapse.Background     = new SolidColorBrush(Colors.AliceBlue);
        collapse.BorderBrush    = new SolidColorBrush(Color.Parse("#CDC1FF"));
        collapse.BorderThickness = new Thickness(1);
        collapse.CornerRadius   = new CornerRadius(8);
        collapse.Padding        = new Thickness(10);

        ShowInWindow(collapse, () =>
        {
            var frame = GetFrame(collapse);

            frame.Background.ShouldBeSameAs(collapse.Background);
            frame.BorderBrush.ShouldBeSameAs(collapse.BorderBrush);
            frame.BorderThickness.ShouldBe(new Thickness(1));
            frame.CornerRadius.ShouldBe(new CornerRadius(8));
            frame.Padding.ShouldBe(new Thickness(10));
        });
    }

    [Fact]
    public void Semantic_Selectors_Match_Their_Declared_Owner_Routes()
    {
        var collapse = CreateCollapseWithThreePanels();
        collapse.Classes.Add("semantic-owner");
        collapse.Styles.Add(CreateRouteStyle(HeaderClass, "header"));
        collapse.Styles.Add(CreateRouteStyle(IconClass, "icon"));
        collapse.Styles.Add(CreateRouteStyle(TitleClass, "title"));
        collapse.Styles.Add(CreateRouteStyle(BodyClass, "body"));

        ShowInWindow(collapse, () =>
        {
            GetSemanticHeaders(collapse).ShouldAllBe(static header => Equals(header.Tag, "header"));
            GetSemanticIcons(collapse).ShouldAllBe(static icon => Equals(icon.Tag, "icon"));
            GetSemanticTitles(collapse).ShouldAllBe(static title => Equals(title.Tag, "title"));
            GetSemanticBodies(collapse).ShouldAllBe(static body => Equals(body.Tag, "body"));
        });
    }

    [Fact]
    public void Semantic_Header_Padding_Overrides_The_Large_Size_Tier_And_Shrinks_The_Header()
    {
        var baseline = new AtomUICollapse
        {
            SizeType = CustomizableSizeType.Large,
            Items    = { CreateItem("1", true) }
        };
        var styled = new AtomUICollapse
        {
            SizeType = CustomizableSizeType.Large,
            Items    = { CreateItem("1", true) }
        };
        styled.Classes.Add("semantic-owner");
        styled.Styles.Add(new Style(selector =>
            selector.OfType<AtomUICollapse>()
                    .Class("semantic-owner")
                    .Child()
                    .Class(ScopeItemClass)
                    .Template()
                    .Class(HeaderClass))
        {
            Setters = { new Setter(Border.PaddingProperty, new Thickness(16, 12)) }
        });

        var window = new AvaloniaWindow
        {
            Width  = 800,
            Height = 600,
            Content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children    = { baseline, styled }
            }
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var baselineHeader = GetSemanticHeaders(baseline).Single();
            var styledHeader   = GetSemanticHeaders(styled).Single();
            styledHeader.Padding.ShouldBe(new Thickness(16, 12));
            styledHeader.Padding.ShouldNotBe(baselineHeader.Padding);
            styledHeader.Bounds.Height.ShouldBeLessThan(baselineHeader.Bounds.Height);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void SizeType_Tiers_Project_Distinct_Header_Paddings()
    {
        var collapse = new AtomUICollapse
        {
            Items = { CreateItem("1", true) }
        };

        ShowInWindow(collapse, () =>
        {
            collapse.SizeType = CustomizableSizeType.Small;
            Dispatcher.UIThread.RunJobs();
            var smallPadding = GetSemanticHeaders(collapse).Single().Padding;

            collapse.SizeType = CustomizableSizeType.Middle;
            Dispatcher.UIThread.RunJobs();
            var middlePadding = GetSemanticHeaders(collapse).Single().Padding;

            collapse.SizeType = CustomizableSizeType.Large;
            Dispatcher.UIThread.RunJobs();
            var largePadding = GetSemanticHeaders(collapse).Single().Padding;

            smallPadding.ShouldNotBe(largePadding);
            middlePadding.ShouldNotBe(largePadding);
        });
    }

    private static AtomUICollapse CreateCollapseWithThreePanels()
    {
        return new AtomUICollapse
        {
            IsMotionEnabled = false,
            Items =
            {
                CreateItem("1", true),
                CreateItem("2", false),
                CreateItem("3", false)
            }
        };
    }

    private static AtomUICollapseItem CreateItem(string header, bool isSelected)
    {
        return new AtomUICollapseItem
        {
            Header     = $"Header {header}",
            Content    = $"Content {header}",
            IsSelected = isSelected
        };
    }

    private static Style CreateRouteStyle(string partClass, string tag)
    {
        return new Style(selector =>
            selector.OfType<AtomUICollapse>()
                    .Class("semantic-owner")
                    .Child()
                    .Class(ScopeItemClass)
                    .Template()
                    .Class(partClass))
        {
            Setters = { new Setter(Control.TagProperty, tag) }
        };
    }

    private static void AssertPanelMarkerCounts(AtomUICollapse collapse, int expectedPanels, int expectedBodies)
    {
        GetSemanticHeaders(collapse).Length.ShouldBe(expectedPanels);
        GetSemanticIcons(collapse).Length.ShouldBe(expectedPanels);
        GetSemanticTitles(collapse).Length.ShouldBe(expectedPanels);
        GetSemanticBodies(collapse).Length.ShouldBe(expectedBodies);
        GetScopeItemContainers(collapse).Length.ShouldBe(expectedPanels);
    }

    private static void ExpandAllPanels(AtomUICollapse collapse)
    {
        foreach (var item in GetScopeItemContainers(collapse))
        {
            item.IsSelected = true;
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static PixelAlignedBorder GetFrame(AtomUICollapse collapse)
    {
        return collapse.GetVisualDescendants()
                       .OfType<PixelAlignedBorder>()
                       .Single(static border => border.Name == "PART_Frame");
    }

    private static ItemsPresenter GetItemsPresenter(AtomUICollapse collapse)
    {
        return collapse.GetVisualDescendants()
                       .OfType<ItemsPresenter>()
                       .Single(static presenter => presenter.Name == "PART_ItemsPresenter");
    }

    private static StackPanel GetItemsPanel(AtomUICollapse collapse)
    {
        return GetItemsPresenter(collapse).Panel.ShouldBeOfType<StackPanel>();
    }

    private static AtomUICollapseItem[] GetScopeItemContainers(AtomUICollapse collapse)
    {
        return collapse.GetVisualDescendants()
                       .OfType<AtomUICollapseItem>()
                       .Where(static item => item.Classes.Contains(ScopeItemClass))
                       .ToArray();
    }

    private static PixelAlignedBorder[] GetSemanticHeaders(AtomUICollapse collapse)
    {
        return collapse.GetVisualDescendants()
                       .OfType<PixelAlignedBorder>()
                       .Where(static border => border.Classes.Contains(HeaderClass))
                       .ToArray();
    }

    private static IconButtonControl[] GetSemanticIcons(AtomUICollapse collapse)
    {
        return collapse.GetVisualDescendants()
                       .OfType<IconButtonControl>()
                       .Where(static icon => icon.Classes.Contains(IconClass))
                       .ToArray();
    }

    private static ContentPresenter[] GetSemanticTitles(AtomUICollapse collapse)
    {
        return collapse.GetVisualDescendants()
                       .OfType<ContentPresenter>()
                       .Where(static title => title.Classes.Contains(TitleClass))
                       .ToArray();
    }

    private static PixelAlignedBorder[] GetSemanticBodies(AtomUICollapse collapse)
    {
        return collapse.GetVisualDescendants()
                       .OfType<PixelAlignedBorder>()
                       .Where(static border => border.Classes.Contains(BodyClass))
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

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUICollapse));
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
