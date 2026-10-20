using System.Collections.ObjectModel;
using System.Xml.Linq;
using AtomUI.Controls.Commons;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUITextBlock = AtomUI.Desktop.Controls.TextBlock;
using AtomUITimeline = AtomUI.Desktop.Controls.Timeline;
using AtomUITimelineItem = AtomUI.Desktop.Controls.TimelineItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Timeline;

public class TimelineSemanticPartTests
{
    private const string ItemClass = "semantic-item";
    private const string WrapperClass = "semantic-item-wrapper";
    private const string IconClass = "semantic-item-icon";
    private const string SectionClass = "semantic-item-section";
    private const string HeaderClass = "semantic-item-header";
    private const string TitleClass = "semantic-item-title";
    private const string ContentClass = "semantic-item-content";
    private const string RailClass = "semantic-item-rail";
    private const string IndicatorClass = "semantic-indicator";
    private const string OwnerClass = "semantic-owner";
    private const string IconPresentPseudoClass = ":icon-present";

    static TimelineSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUITimeline), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(
                      [
                          "root",
                          "item",
                          "itemContent",
                          "itemHeader",
                          "itemIcon",
                          "itemRail",
                          "itemSection",
                          "itemTitle",
                          "itemWrapper"
                      ]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(descriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass,
            typeof(AtomUITimelineItem),
            "> .semantic-item");
        AssertPart(descriptor.Parts.Single(static part => part.Name == "itemWrapper"),
            WrapperClass,
            typeof(Panel),
            "> .semantic-item /template/ .semantic-item-wrapper");
        AssertPart(descriptor.Parts.Single(static part => part.Name == "itemIcon"),
            IconClass,
            typeof(Border),
            "> .semantic-item /template/ .semantic-indicator /template/ .semantic-item-icon");
        AssertPart(descriptor.Parts.Single(static part => part.Name == "itemSection"),
            SectionClass,
            typeof(Panel),
            "> .semantic-item /template/ .semantic-item-section");
        AssertPart(descriptor.Parts.Single(static part => part.Name == "itemHeader"),
            HeaderClass,
            typeof(StackPanel),
            "> .semantic-item /template/ .semantic-item-header");
        AssertPart(descriptor.Parts.Single(static part => part.Name == "itemTitle"),
            TitleClass,
            typeof(AtomUITextBlock),
            "> .semantic-item /template/ .semantic-item-title");
        AssertPart(descriptor.Parts.Single(static part => part.Name == "itemContent"),
            ContentClass,
            typeof(ContentPresenter),
            "> .semantic-item /template/ .semantic-item-content");
        AssertPart(descriptor.Parts.Single(static part => part.Name == "itemRail"),
            RailClass,
            typeof(Border),
            "> .semantic-item /template/ .semantic-indicator /template/ .semantic-item-rail");

        registry.TryGetControl(typeof(AtomUITimelineItem), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(AbstractTimeline), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(AbstractTimelineItem), out _).ShouldBeFalse();
    }

    [Fact]
    public void Templates_Implement_Only_The_Approved_Static_Markers()
    {
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Timeline/Themes/TimelineTheme.axaml",
            Array.Empty<string>());
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Timeline/Themes/TimelineItemTheme.axaml",
            [
                "semantic-indicator:TimelineIndicator",
                "semantic-item-content:ContentPresenter",
                "semantic-item-header:StackPanel",
                "semantic-item-section:TimelineSectionPanel",
                "semantic-item-title:TextBlock",
                "semantic-item-wrapper:TimelineItemPanel"
            ]);
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Timeline/Themes/TimelineIndicatorTheme.axaml",
            [
                "semantic-item-icon:Border",
                "semantic-item-icon:Border",
                "semantic-item-rail:Border"
            ]);
    }

    [Fact]
    public void Basic_Item_Produces_The_Full_Marker_Set()
    {
        var timeline = new AtomUITimeline
        {
            Width = 640,
            Items =
            {
                new AtomUITimelineItem
                {
                    Label   = "2015-09-01",
                    Content = "Create a services site"
                }
            }
        };

        ShowInWindow(timeline, () =>
        {
            AssertMarkerCounts(timeline, 1);

            var dot  = GetNamedPart(timeline, "PART_Dot");
            var host = GetNamedPart(timeline, "PART_IconHost");
            dot.IsVisible.ShouldBeTrue();
            host.IsVisible.ShouldBeFalse();
            GetIndicatorHops(timeline)
                .ShouldAllBe(static hop => !hop.Classes.Contains(IconPresentPseudoClass));
        });
    }

    [Fact]
    public void Three_Items_Produce_One_Marker_Set_Per_Item()
    {
        var timeline = new AtomUITimeline
        {
            Width = 640,
            Items =
            {
                new AtomUITimelineItem { Content = "Create a services site" },
                new AtomUITimelineItem
                {
                    IndicatorIcon = CreateIcon(),
                    Content       = "Solve initial network problems"
                },
                new AtomUITimelineItem
                {
                    Label   = "2015-09-01",
                    Content = "Technical testing"
                }
            }
        };

        ShowInWindow(timeline, () =>
        {
            AssertMarkerCounts(timeline, 3);
        });
    }

    [Fact]
    public void Empty_Items_Produce_No_Markers()
    {
        var timeline = new AtomUITimeline
        {
            Width = 640
        };

        ShowInWindow(timeline, () =>
        {
            AssertMarkerCounts(timeline, 0);
        });
    }

    [Fact]
    public void Pending_Reverse_And_Orientation_Keep_Marker_Sets()
    {
        var timeline = new AtomUITimeline
        {
            Width   = 640,
            Pending = "Loading",
            Items =
            {
                new AtomUITimelineItem { Content = "Create a services site" }
            }
        };

        ShowInWindow(timeline, () =>
        {
            AssertMarkerCounts(timeline, 2);

            timeline.IsReverse = true;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(timeline, 2);

            timeline.Orientation = Orientation.Horizontal;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(timeline, 2);

            timeline.Pending = null;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(timeline, 1);
        });
    }

    [Fact]
    public void Collection_Reset_Regenerates_The_Marker_Set()
    {
        var contents = new ObservableCollection<string> { "A", "B" };
        var timeline = new AtomUITimeline
        {
            Width       = 640,
            ItemsSource = contents
        };

        ShowInWindow(timeline, () =>
        {
            AssertMarkerCounts(timeline, 2);

            contents.Clear();
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(timeline, 0);

            contents.Add("C");
            contents.Add("D");
            contents.Add("E");
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(timeline, 3);
        });
    }

    [Fact]
    public void Icon_Item_Masks_The_Rail_And_Flips_The_Icon_Present_Pseudo_Class()
    {
        var timeline = new AtomUITimeline
        {
            Width = 640,
            Items =
            {
                new AtomUITimelineItem { Content = "Plain item" },
                new AtomUITimelineItem
                {
                    IndicatorIcon = CreateIcon(),
                    Content       = "Icon item"
                }
            }
        };

        ShowInWindow(timeline, () =>
        {
            AssertMarkerCounts(timeline, 2);

            var hops = GetIndicatorHops(timeline);
            hops[0].Classes.Contains(IconPresentPseudoClass).ShouldBeFalse();
            hops[1].Classes.Contains(IconPresentPseudoClass).ShouldBeTrue();

            var items = GetSemanticItems(timeline);
            var plainDot  = GetNamedPart(items[0], "PART_Dot");
            var iconDot   = GetNamedPart(items[1], "PART_Dot");
            var iconHost  = GetNamedPart(items[1], "PART_IconHost");
            plainDot.IsVisible.ShouldBeTrue();
            iconDot.IsVisible.ShouldBeFalse();
            iconHost.IsVisible.ShouldBeTrue();
            iconHost.GetValue(Border.BackgroundProperty).ShouldNotBeNull();
        });
    }

    [Fact]
    public void Single_Item_Rail_Collapses_To_Zero_Extent()
    {
        var timeline = new AtomUITimeline
        {
            Width = 640,
            Items =
            {
                new AtomUITimelineItem { Content = "The only item" }
            }
        };

        ShowInWindow(timeline, () =>
        {
            var rail = (Border)GetSemanticRails(timeline).Single();

            rail.Bounds.Width.ShouldBeGreaterThan(0);
            rail.Bounds.Height.ShouldBe(0);
        });
    }

    [Fact]
    public void Semantic_Selectors_Match_Their_Declared_Owner_Routes()
    {
        var timeline = new AtomUITimeline
        {
            Width = 640,
            Items =
            {
                new AtomUITimelineItem
                {
                    IndicatorIcon = CreateIcon(),
                    Label         = "2015-09-01",
                    Content       = "Create a services site"
                },
                new AtomUITimelineItem { Content = "Solve initial network problems" }
            }
        };
        timeline.Classes.Add(OwnerClass);
        timeline.Styles.Add(CreateItemRouteStyle(ItemClass, "item"));
        timeline.Styles.Add(CreateTemplateRouteStyle(WrapperClass, "wrapper"));
        timeline.Styles.Add(CreateTemplateRouteStyle(SectionClass, "section"));
        timeline.Styles.Add(CreateTemplateRouteStyle(HeaderClass, "header"));
        timeline.Styles.Add(CreateTemplateRouteStyle(TitleClass, "title"));
        timeline.Styles.Add(CreateTemplateRouteStyle(ContentClass, "content"));
        timeline.Styles.Add(CreateIndicatorRouteStyle(IconClass, "icon"));
        timeline.Styles.Add(CreateIndicatorRouteStyle(RailClass, "rail"));

        ShowInWindow(timeline, () =>
        {
            GetSemanticItems(timeline).ShouldAllBe(static item => Equals(item.Tag, "item"));
            GetSemanticWrappers(timeline).ShouldAllBe(static wrapper => Equals(wrapper.Tag, "wrapper"));
            GetSemanticSections(timeline).ShouldAllBe(static section => Equals(section.Tag, "section"));
            GetSemanticHeaders(timeline).ShouldAllBe(static header => Equals(header.Tag, "header"));
            GetSemanticTitles(timeline).ShouldAllBe(static title => Equals(title.Tag, "title"));
            GetSemanticContents(timeline).ShouldAllBe(static content => Equals(content.Tag, "content"));
            GetSemanticIcons(timeline).ShouldAllBe(static icon => Equals(icon.Tag, "icon"));
            GetSemanticRails(timeline).ShouldAllBe(static rail => Equals(rail.Tag, "rail"));
        });
    }

    [Fact]
    public void Generated_Route_Styles_Apply_Real_Setters()
    {
        var timeline = new AtomUITimeline
        {
            Width = 640,
            Items =
            {
                new AtomUITimelineItem
                {
                    IndicatorIcon = CreateIcon(),
                    Label         = "2015-09-01",
                    Content       = "Create a services site"
                },
                new AtomUITimelineItem { Content = "Solve initial network problems" }
            }
        };
        var wrapperBrush = new SolidColorBrush(Colors.LightYellow);
        var iconBrush    = new SolidColorBrush(Colors.DarkGreen);
        timeline.Classes.Add(OwnerClass);
        timeline.Styles.Add(new Style(selector =>
            selector.OfType<AtomUITimeline>()
                    .Class(OwnerClass)
                    .Child()
                    .Class(ItemClass))
        {
            Setters = { new Setter(TemplatedControl.MinHeightProperty, 40d) }
        });
        timeline.Styles.Add(new Style(selector =>
            selector.OfType<AtomUITimeline>()
                    .Class(OwnerClass)
                    .Child()
                    .Class(ItemClass)
                    .Template()
                    .Class(WrapperClass))
        {
            Setters = { new Setter(Panel.BackgroundProperty, wrapperBrush) }
        });
        timeline.Styles.Add(new Style(selector =>
            selector.OfType<AtomUITimeline>()
                    .Class(OwnerClass)
                    .Child()
                    .Class(ItemClass)
                    .Template()
                    .Class(SectionClass))
        {
            Setters = { new Setter(Panel.ClipToBoundsProperty, true) }
        });
        timeline.Styles.Add(new Style(selector =>
            selector.OfType<AtomUITimeline>()
                    .Class(OwnerClass)
                    .Child()
                    .Class(ItemClass)
                    .Template()
                    .Class(HeaderClass))
        {
            Setters = { new Setter(StackPanel.OrientationProperty, Orientation.Horizontal) }
        });
        timeline.Styles.Add(new Style(selector =>
            selector.OfType<AtomUITimeline>()
                    .Class(OwnerClass)
                    .Child()
                    .Class(ItemClass)
                    .Template()
                    .Class(TitleClass))
        {
            Setters = { new Setter(TextBlock.FontWeightProperty, FontWeight.SemiBold) }
        });
        timeline.Styles.Add(new Style(selector =>
            selector.OfType<AtomUITimeline>()
                    .Class(OwnerClass)
                    .Child()
                    .Class(ItemClass)
                    .Template()
                    .Class(ContentClass))
        {
            Setters = { new Setter(Layoutable.MarginProperty, new Thickness(8, 0, 0, 0)) }
        });
        timeline.Styles.Add(new Style(selector =>
            selector.OfType<AtomUITimeline>()
                    .Class(OwnerClass)
                    .Child()
                    .Class(ItemClass)
                    .Template()
                    .Class(IndicatorClass)
                    .Template()
                    .Class(IconClass))
        {
            Setters = { new Setter(Border.BorderBrushProperty, iconBrush) }
        });
        timeline.Styles.Add(new Style(selector =>
            selector.OfType<AtomUITimeline>()
                    .Class(OwnerClass)
                    .Child()
                    .Class(ItemClass)
                    .Template()
                    .Class(IndicatorClass)
                    .Template()
                    .Class(RailClass))
        {
            Setters = { new Setter(Border.CornerRadiusProperty, new CornerRadius(4)) }
        });

        ShowInWindow(timeline, () =>
        {
            GetSemanticItems(timeline).ShouldAllBe(static item => Equals(item.MinHeight, 40d));
            GetSemanticWrappers(timeline)
                .ShouldAllBe(static wrapper => HasColor(wrapper.Background, Colors.LightYellow));
            GetSemanticSections(timeline).ShouldAllBe(static section => section.ClipToBounds);
            GetSemanticHeaders(timeline)
                .ShouldAllBe(static header => header.Orientation == Orientation.Horizontal);
            GetSemanticTitles(timeline)
                .ShouldAllBe(static title => title.FontWeight == FontWeight.SemiBold);
            GetSemanticContents(timeline)
                .ShouldAllBe(static content => content.Margin == new Thickness(8, 0, 0, 0));
            GetSemanticIcons(timeline)
                .ShouldAllBe(static icon => HasColor(icon.BorderBrush, Colors.DarkGreen));
            GetSemanticRails(timeline)
                .ShouldAllBe(static rail => Equals(rail.CornerRadius, new CornerRadius(4)));
        });
    }

    [Fact]
    public void Root_Style_Setter_Applies_To_The_Timeline_Owner()
    {
        var timeline = new AtomUITimeline
        {
            Width = 640,
            Items =
            {
                new AtomUITimelineItem { Content = "Create a services site" }
            }
        };
        timeline.Classes.Add(OwnerClass);
        timeline.Styles.Add(new Style(selector =>
            selector.OfType<AtomUITimeline>().Class(OwnerClass))
        {
            Setters = { new Setter(TemplatedControl.BackgroundProperty, Brushes.Yellow) }
        });

        ShowInWindow(timeline, () =>
        {
            GetBrushColor(timeline.Background).ShouldBe(Colors.Yellow);
        });
    }

    private static PathIcon CreateIcon()
    {
        return new PathIcon { Data = Geometry.Parse("M0,0 L10,0 L10,10 Z") };
    }

    private static Color GetBrushColor(IBrush? brush)
    {
        return ((ISolidColorBrush)brush!).Color;
    }

    private static bool HasColor(IBrush? brush, Color color)
    {
        return brush is SolidColorBrush solid && solid.Color == color;
    }

    private static Style CreateItemRouteStyle(string partClass, string tag)
    {
        return new Style(selector =>
            selector.OfType<AtomUITimeline>()
                    .Class(OwnerClass)
                    .Child()
                    .Class(partClass))
        {
            Setters = { new Setter(Control.TagProperty, tag) }
        };
    }

    private static Style CreateTemplateRouteStyle(string partClass, string tag)
    {
        return new Style(selector =>
            selector.OfType<AtomUITimeline>()
                    .Class(OwnerClass)
                    .Child()
                    .Class(ItemClass)
                    .Template()
                    .Class(partClass))
        {
            Setters = { new Setter(Control.TagProperty, tag) }
        };
    }

    private static Style CreateIndicatorRouteStyle(string partClass, string tag)
    {
        return new Style(selector =>
            selector.OfType<AtomUITimeline>()
                    .Class(OwnerClass)
                    .Child()
                    .Class(ItemClass)
                    .Template()
                    .Class(IndicatorClass)
                    .Template()
                    .Class(partClass))
        {
            Setters = { new Setter(Control.TagProperty, tag) }
        };
    }

    private static void AssertMarkerCounts(AtomUITimeline timeline, int expectedItems)
    {
        GetSemanticItems(timeline).Length.ShouldBe(expectedItems);
        GetSemanticWrappers(timeline).Length.ShouldBe(expectedItems);
        GetSemanticSections(timeline).Length.ShouldBe(expectedItems);
        GetSemanticHeaders(timeline).Length.ShouldBe(expectedItems);
        GetSemanticTitles(timeline).Length.ShouldBe(expectedItems);
        GetSemanticContents(timeline).Length.ShouldBe(expectedItems);
        GetSemanticIcons(timeline).Length.ShouldBe(expectedItems * 2);
        GetSemanticRails(timeline).Length.ShouldBe(expectedItems);
        GetIndicatorHops(timeline).Length.ShouldBe(expectedItems);
    }

    private static AtomUITimelineItem[] GetSemanticItems(AtomUITimeline timeline)
    {
        return timeline.GetVisualDescendants()
                       .OfType<AtomUITimelineItem>()
                       .Where(static item => item.Classes.Contains(ItemClass))
                       .ToArray();
    }

    private static Panel[] GetSemanticWrappers(AtomUITimeline timeline)
    {
        return timeline.GetVisualDescendants()
                       .OfType<Panel>()
                       .Where(static wrapper => wrapper.Classes.Contains(WrapperClass))
                       .ToArray();
    }

    private static Panel[] GetSemanticSections(AtomUITimeline timeline)
    {
        return timeline.GetVisualDescendants()
                       .OfType<Panel>()
                       .Where(static section => section.Classes.Contains(SectionClass))
                       .ToArray();
    }

    private static StackPanel[] GetSemanticHeaders(AtomUITimeline timeline)
    {
        return timeline.GetVisualDescendants()
                       .OfType<StackPanel>()
                       .Where(static header => header.Classes.Contains(HeaderClass))
                       .ToArray();
    }

    private static AtomUITextBlock[] GetSemanticTitles(AtomUITimeline timeline)
    {
        return timeline.GetVisualDescendants()
                       .OfType<AtomUITextBlock>()
                       .Where(static title => title.Classes.Contains(TitleClass))
                       .ToArray();
    }

    private static ContentPresenter[] GetSemanticContents(AtomUITimeline timeline)
    {
        return timeline.GetVisualDescendants()
                       .OfType<ContentPresenter>()
                       .Where(static content => content.Classes.Contains(ContentClass))
                       .ToArray();
    }

    private static Border[] GetSemanticIcons(AtomUITimeline timeline)
    {
        return timeline.GetVisualDescendants()
                       .OfType<Border>()
                       .Where(static icon => icon.Classes.Contains(IconClass))
                       .ToArray();
    }

    private static Border[] GetSemanticRails(AtomUITimeline timeline)
    {
        return timeline.GetVisualDescendants()
                       .OfType<Border>()
                       .Where(static rail => rail.Classes.Contains(RailClass))
                       .ToArray();
    }

    private static Visual[] GetIndicatorHops(AtomUITimeline timeline)
    {
        return timeline.GetVisualDescendants()
                       .Where(static visual => visual.Classes.Contains(IndicatorClass))
                       .ToArray();
    }

    private static Control GetNamedPart(Visual root, string name)
    {
        return root.GetVisualDescendants()
                   .OfType<Control>()
                   .Single(part => part.Name == name);
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

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUITimeline));
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
