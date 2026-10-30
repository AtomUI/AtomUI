using System.Collections.ObjectModel;
using System.Xml.Linq;
using AtomUI.Controls;
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
using AtomUISegmented = AtomUI.Desktop.Controls.Segmented;
using AtomUISegmentedItem = AtomUI.Desktop.Controls.SegmentedItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Segmented;

public class SegmentedSemanticPartTests
{
    private const string ItemClass = "semantic-item";
    private const string IconClass = "semantic-icon";
    private const string LabelClass = "semantic-label";
    private const string ItemRoute = "> .semantic-item";
    private const string RoutePrefix = "> .semantic-item /template/ .";

    static SegmentedSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUISegmented), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "icon", "item", "label"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(descriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass,
            typeof(AtomUISegmentedItem),
            ItemRoute);
        AssertPart(descriptor.Parts.Single(static part => part.Name == "icon"),
            IconClass,
            typeof(IconPresenter),
            RoutePrefix + IconClass);
        AssertPart(descriptor.Parts.Single(static part => part.Name == "label"),
            LabelClass,
            typeof(ContentPresenter),
            RoutePrefix + LabelClass);

        registry.TryGetControl(typeof(AtomUISegmentedItem), out _).ShouldBeFalse();
    }

    [Fact]
    public void Templates_Implement_Only_The_Approved_Static_Markers()
    {
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Segmented/Themes/SegmentedTheme.axaml",
            Array.Empty<string>());
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Segmented/Themes/SegmentedItemTheme.axaml",
            [
                "semantic-icon:IconPresenter",
                "semantic-label:ContentPresenter"
            ]);
    }

    [Fact]
    public void Generated_Items_Produce_One_Marker_Set_Per_Container()
    {
        var segmented = new AtomUISegmented
        {
            ItemsSource = new[] { "Daily", "Weekly", "Monthly" }
        };

        ShowInWindow(segmented, () =>
        {
            AssertMarkerCounts(segmented, 3);
        });
    }

    [Fact]
    public void Empty_Items_Produce_No_Markers()
    {
        var segmented = new AtomUISegmented
        {
            ItemsSource = Array.Empty<string>()
        };

        ShowInWindow(segmented, () =>
        {
            AssertMarkerCounts(segmented, 0);
        });
    }

    [Fact]
    public void Explicit_Items_Carry_The_Semantic_Item_Marker()
    {
        var segmented = new AtomUISegmented
        {
            Items =
            {
                new AtomUISegmentedItem { Content = "Daily" },
                new AtomUISegmentedItem { Content = "Weekly" }
            }
        };

        ShowInWindow(segmented, () =>
        {
            AssertMarkerCounts(segmented, 2);
        });
    }

    [Fact]
    public void Icon_Text_And_IconOnly_Variants_Keep_Template_Markers()
    {
        var segmented = new AtomUISegmented
        {
            Items =
            {
                new AtomUISegmentedItem
                {
                    Icon = CreateIcon(),
                    Content = "List"
                },
                new AtomUISegmentedItem { Content = "Kanban" },
                new AtomUISegmentedItem { Icon = CreateIcon() }
            }
        };

        ShowInWindow(segmented, () =>
        {
            AssertMarkerCounts(segmented, 3);

            var labels = GetSemanticLabels(segmented);
            labels[1].IsVisible.ShouldBeTrue();
            labels[2].IsVisible.ShouldBeFalse();

            var icons = GetSemanticIcons(segmented);
            icons[0].IsVisible.ShouldBeTrue();
            icons[1].IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Selection_Changes_Do_Not_Add_Or_Remove_Markers()
    {
        var segmented = new AtomUISegmented
        {
            ItemsSource = new[] { "Daily", "Weekly", "Monthly" }
        };

        ShowInWindow(segmented, () =>
        {
            AssertMarkerCounts(segmented, 3);

            segmented.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(segmented, 3);

            segmented.SelectedIndex = 2;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(segmented, 3);
        });
    }

    [Fact]
    public void Collection_Reset_Regenerates_The_Marker_Set()
    {
        var options = new ObservableCollection<string> { "Daily", "Weekly" };
        var segmented = new AtomUISegmented
        {
            ItemsSource = options
        };

        ShowInWindow(segmented, () =>
        {
            AssertMarkerCounts(segmented, 2);

            options.Clear();
            options.Add("Monthly");
            options.Add("Quarterly");
            options.Add("Yearly");
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(segmented, 3);
        });
    }

    [Fact]
    public void Block_Mode_And_Orientation_Keep_The_Marker_Set()
    {
        var segmented = new AtomUISegmented
        {
            ItemsSource = new[] { "Daily", "Weekly" }
        };

        ShowInWindow(segmented, () =>
        {
            AssertMarkerCounts(segmented, 2);

            segmented.IsExpanding = true;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(segmented, 2);

            segmented.Orientation = Orientation.Vertical;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(segmented, 2);
        });
    }

    [Fact]
    public void All_Size_Types_And_Round_Shape_Keep_The_Marker_Set()
    {
        var segmented = new AtomUISegmented
        {
            ItemsSource = new[] { "Daily", "Weekly" }
        };

        ShowInWindow(segmented, () =>
        {
            foreach (var sizeType in new[]
                     {
                         CustomizableSizeType.Large,
                         CustomizableSizeType.Middle,
                         CustomizableSizeType.Small,
                         CustomizableSizeType.Custom
                     })
            {
                segmented.SizeType = sizeType;
                Dispatcher.UIThread.RunJobs();
                AssertMarkerCounts(segmented, 2);
            }

            segmented.Shape = SegmentedShape.Round;
            segmented.SizeType = CustomizableSizeType.Large;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(segmented, 2);
        });
    }

    [Fact]
    public void Semantic_Selectors_Match_Their_Declared_Owner_Routes()
    {
        var segmented = new AtomUISegmented
        {
            ItemsSource = new[] { "Daily", "Weekly" }
        };
        segmented.Classes.Add("semantic-owner");
        segmented.Styles.Add(CreateItemRouteStyle(ItemClass, "item"));
        segmented.Styles.Add(CreateTemplateRouteStyle(IconClass, "icon"));
        segmented.Styles.Add(CreateTemplateRouteStyle(LabelClass, "label"));

        ShowInWindow(segmented, () =>
        {
            GetSemanticItems(segmented).ShouldAllBe(static item => Equals(item.Tag, "item"));
            GetSemanticIcons(segmented).ShouldAllBe(static icon => Equals(icon.Tag, "icon"));
            GetSemanticLabels(segmented).ShouldAllBe(static label => Equals(label.Tag, "label"));
        });
    }

    [Fact]
    public void Generated_Item_Style_Overrides_The_Default_Layout()
    {
        var baseline = new AtomUISegmented
        {
            ItemsSource = new[] { "Daily" }
        };
        var styled = new AtomUISegmented
        {
            ItemsSource = new[] { "Daily" }
        };
        styled.Classes.Add("semantic-owner");
        styled.Styles.Add(new Style(selector =>
            selector.OfType<AtomUISegmented>()
                    .Class("semantic-owner")
                    .Child()
                    .Class(ItemClass))
        {
            Setters = { new Setter(TemplatedControl.MinHeightProperty, 40d) }
        });

        var window = new AvaloniaWindow
        {
            Width = 800,
            Height = 600,
            Content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children = { baseline, styled }
            }
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var baselineItem = GetSemanticItems(baseline).Single();
            var styledItem = GetSemanticItems(styled).Single();
            styledItem.MinHeight.ShouldBe(40d);
            styledItem.MinHeight.ShouldNotBe(baselineItem.MinHeight);
            styledItem.Bounds.Height.ShouldBeGreaterThanOrEqualTo(40d);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Generated_Icon_And_Label_Styles_Apply_Through_The_Template_Route()
    {
        var segmented = new AtomUISegmented
        {
            Items =
            {
                new AtomUISegmentedItem
                {
                    Icon = CreateIcon(),
                    Content = "List"
                },
                new AtomUISegmentedItem { Content = "Kanban" }
            }
        };
        var iconBrush = new SolidColorBrush(Colors.DarkGreen);
        segmented.Classes.Add("semantic-owner");
        segmented.Styles.Add(new Style(selector =>
            selector.OfType<AtomUISegmented>()
                    .Class("semantic-owner")
                    .Child()
                    .Class(ItemClass)
                    .Template()
                    .Class(IconClass))
        {
            Setters = { new Setter(IconPresenter.IconBrushProperty, iconBrush) }
        });
        segmented.Styles.Add(new Style(selector =>
            selector.OfType<AtomUISegmented>()
                    .Class("semantic-owner")
                    .Child()
                    .Class(ItemClass)
                    .Template()
                    .Class(LabelClass))
        {
            Setters = { new Setter(Layoutable.MarginProperty, new Thickness(8, 0, 0, 0)) }
        });

        ShowInWindow(segmented, () =>
        {
            GetSemanticIcons(segmented)
                .ShouldAllBe(static icon => HasColor(icon.IconBrush, Colors.DarkGreen));
            GetSemanticLabels(segmented)
                .ShouldAllBe(static label => label.Margin == new Thickness(8, 0, 0, 0));
        });
    }

    [Fact]
    public void Root_Semantic_Part_Projects_The_Standard_Surface_Properties()
    {
        var segmented = new AtomUISegmented
        {
            ItemsSource = new[] { "Daily" }
        };
        segmented.CornerRadius = new CornerRadius(8);
        segmented.Padding = new Thickness(10);

        ShowInWindow(segmented, () =>
        {
            var frame = GetFrame(segmented);

            frame.CornerRadius.ShouldBe(new CornerRadius(8));
            frame.Padding.ShouldBe(new Thickness(10));
            frame.ClipToBounds.ShouldBeTrue();
        });
    }

    private static PathIcon CreateIcon()
    {
        return new PathIcon { Data = Geometry.Parse("M0,0 L10,0 L10,10 Z") };
    }

    private static bool HasColor(IBrush? brush, Color color)
    {
        return brush is SolidColorBrush solid && solid.Color == color;
    }

    private static Style CreateItemRouteStyle(string partClass, string tag)
    {
        return new Style(selector =>
            selector.OfType<AtomUISegmented>()
                    .Class("semantic-owner")
                    .Child()
                    .Class(partClass))
        {
            Setters = { new Setter(Control.TagProperty, tag) }
        };
    }

    private static Style CreateTemplateRouteStyle(string partClass, string tag)
    {
        return new Style(selector =>
            selector.OfType<AtomUISegmented>()
                    .Class("semantic-owner")
                    .Child()
                    .Class(ItemClass)
                    .Template()
                    .Class(partClass))
        {
            Setters = { new Setter(Control.TagProperty, tag) }
        };
    }

    private static void AssertMarkerCounts(AtomUISegmented segmented, int expectedItems)
    {
        GetSemanticItems(segmented).Length.ShouldBe(expectedItems);
        GetSemanticIcons(segmented).Length.ShouldBe(expectedItems);
        GetSemanticLabels(segmented).Length.ShouldBe(expectedItems);
    }

    private static AtomUISegmentedItem[] GetSemanticItems(AtomUISegmented segmented)
    {
        return segmented.GetVisualDescendants()
                        .OfType<AtomUISegmentedItem>()
                        .Where(static item => item.Classes.Contains(ItemClass))
                        .ToArray();
    }

    private static IconPresenter[] GetSemanticIcons(AtomUISegmented segmented)
    {
        return segmented.GetVisualDescendants()
                        .OfType<IconPresenter>()
                        .Where(static icon => icon.Classes.Contains(IconClass))
                        .ToArray();
    }

    private static ContentPresenter[] GetSemanticLabels(AtomUISegmented segmented)
    {
        return segmented.GetVisualDescendants()
                        .OfType<ContentPresenter>()
                        .Where(static label => label.Classes.Contains(LabelClass))
                        .ToArray();
    }

    private static Border GetFrame(AtomUISegmented segmented)
    {
        // The root Frame comes before the item Frame in depth-first visual order.
        return segmented.GetVisualDescendants()
                        .OfType<Border>()
                        .First(static border => border.Name == "Frame");
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width = 800,
            Height = 600,
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
        part.ContractType.ShouldBe(typeof(AtomUISegmented));
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
