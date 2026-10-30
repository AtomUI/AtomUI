using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIBreadcrumb = AtomUI.Desktop.Controls.Breadcrumb;
using AtomUIBreadcrumbItem = AtomUI.Desktop.Controls.BreadcrumbItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Breadcrumb;

public class BreadcrumbSemanticPartTests
{
    private const string ItemClass      = "semantic-item";
    private const string SeparatorClass = "semantic-separator";
    private const string ItemRoute      = "> .semantic-item";
    private const string SeparatorRoute = "> .semantic-separator";

    static BreadcrumbSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_Ant_Design_Aligned_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIBreadcrumb), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "item", "separator"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass,
            typeof(AtomUIBreadcrumbItem),
            typeof(BreadcrumbItemStyle),
            ItemRoute,
            runtimeCreated: true);
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "separator"),
            SeparatorClass,
            typeof(ContentPresenter),
            typeof(BreadcrumbSeparatorStyle),
            SeparatorRoute,
            runtimeCreated: true);

        registry.TryGetControl(typeof(AtomUIBreadcrumbItem), out _).ShouldBeFalse();
    }

    [Fact]
    public void Templates_Implement_Only_The_Approved_Static_Markers()
    {
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Breadcrumb/Themes/BreadcrumbTheme.axaml",
            []);
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Breadcrumb/Themes/BreadcrumbItemTheme.axaml",
            []);
    }

    [Fact]
    public void Default_Themes_Do_Not_Consume_Semantic_Selectors()
    {
        AssertNoSemanticSelectors("src/AtomUI.Desktop.Controls/Breadcrumb/Themes/BreadcrumbTheme.axaml");
        AssertNoSemanticSelectors("src/AtomUI.Desktop.Controls/Breadcrumb/Themes/BreadcrumbItemTheme.axaml");
    }

    [Fact]
    public void Separator_Margin_Is_Counted_Exactly_Once_In_Flow_Layout()
    {
        var view = new SemanticParts.SemanticRepairStylesView();
        var breadcrumb = view.FindControl<AtomUIBreadcrumb>("Breadcrumb").ShouldNotBeNull();
        var style = view.Styles.OfType<Style>().SelectMany(owner => owner.Children)
            .OfType<BreadcrumbSeparatorStyle>().ShouldHaveSingleItem();
        style.Setters.OfType<Setter>().Single(setter => setter.Property == Layoutable.MarginProperty)
            .Value.ShouldBe(new Thickness(10, 0));
        var codeBehind = File.ReadAllText(GetRepoFile(
            "tests/AtomUI.Desktop.Controls.Tests/SemanticParts/SemanticRepairStylesView.axaml.cs"));
        codeBehind.ShouldNotContain("Loaded");
        codeBehind.ShouldNotContain("Unloaded");
        codeBehind.ShouldNotContain("FindControl");
        codeBehind.ShouldNotContain("NameScope");
        ShowInWindow(view, () =>
        {
            var separator = GetSemanticSeparators(breadcrumb).Single();
            breadcrumb.DesiredSize.Width.ShouldBe(110d);
            separator.Bounds.Left.ShouldBe(50d);
            separator.Bounds.Width.ShouldBe(10d);
        });
    }

    [Fact]
    public void Explicit_Items_Produce_Item_And_Separator_Markers()
    {
        var breadcrumb = new AtomUIBreadcrumb
        {
            Items =
            {
                new AtomUIBreadcrumbItem { Content = "Home" },
                new AtomUIBreadcrumbItem { Content = "Application", NavigateContext = "#" },
                new AtomUIBreadcrumbItem { Content = "Detail" }
            }
        };

        ShowInWindow(breadcrumb, () =>
        {
            GetSemanticItems(breadcrumb).Length.ShouldBe(3);
            GetSemanticSeparators(breadcrumb).Length.ShouldBe(2);
        });
    }

    [Fact]
    public void ItemsSource_Containers_Receive_The_Item_Marker()
    {
        var breadcrumb = new AtomUIBreadcrumb
        {
            ItemsSource = new[]
            {
                new BreadcrumbItemData { Content = "Home" },
                new BreadcrumbItemData { Content = "Application", NavigateContext = "#" },
                new BreadcrumbItemData { Content = "Detail" }
            }
        };

        ShowInWindow(breadcrumb, () =>
        {
            GetSemanticItems(breadcrumb).Length.ShouldBe(3);
            GetSemanticSeparators(breadcrumb).Length.ShouldBe(2);
        });
    }

    [Fact]
    public void Generated_Route_Styles_Apply_To_Item_And_Separator_Parts()
    {
        var breadcrumb = new AtomUIBreadcrumb
        {
            Items =
            {
                new AtomUIBreadcrumbItem { Content = "Home" },
                new AtomUIBreadcrumbItem { Content = "Application", NavigateContext = "#" },
                new AtomUIBreadcrumbItem { Content = "Detail" }
            }
        };
        breadcrumb.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUIBreadcrumb>().Class("semantic-owner"));
        ownerStyle.Children.Add(new BreadcrumbItemStyle
        {
            Setters = { new Setter(Control.TagProperty, "item") }
        });
        ownerStyle.Children.Add(new BreadcrumbSeparatorStyle
        {
            Setters =
            {
                new Setter(Control.TagProperty, "separator"),
                new Setter(ContentPresenter.ForegroundProperty, Brushes.Red)
            }
        });
        breadcrumb.Styles.Add(ownerStyle);

        ShowInWindow(breadcrumb, () =>
        {
            GetSemanticItems(breadcrumb).ShouldAllBe(static item => Equals(item.Tag, "item"));
            GetSemanticSeparators(breadcrumb).ShouldAllBe(static separator =>
                Equals(separator.Tag, "separator") && Equals(separator.Foreground, Brushes.Red));
        });
    }

    [Fact]
    public void Linked_Items_Keep_The_Token_Link_Color_On_The_Content_Presenter_Despite_Item_Styles()
    {
        var breadcrumb = new AtomUIBreadcrumb
        {
            Items =
            {
                new AtomUIBreadcrumbItem { Content = "Home" },
                new AtomUIBreadcrumbItem { Content = "Application", NavigateContext = "#" }
            }
        };
        breadcrumb.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUIBreadcrumb>().Class("semantic-owner"));
        ownerStyle.Children.Add(new BreadcrumbItemStyle
        {
            Setters = { new Setter(TemplatedControl.ForegroundProperty, Brushes.Red) }
        });
        breadcrumb.Styles.Add(ownerStyle);

        ShowInWindow(breadcrumb, () =>
        {
            var items = GetSemanticItems(breadcrumb);
            items.Length.ShouldBe(2);

            // Plain items inherit the item style color through the content presenter.
            var plainForeground = FindContentPresenter(items[0]).Foreground
                .ShouldNotBeNull()
                .ShouldBeAssignableTo<ISolidColorBrush>();
            plainForeground.Color.ShouldBe(Colors.Red);

            // Linked items carry the token link color directly on the content presenter
            // (Ant Design `.ant-breadcrumb-item a` semantics), so item styles cannot recolor them.
            // LinkColor = ColorTextDescription = AlphaColor(black, 0.45).
            var linkedForeground = FindContentPresenter(items[1]).Foreground
                .ShouldNotBeNull()
                .ShouldBeAssignableTo<ISolidColorBrush>();
            linkedForeground.Color.ShouldBe(Color.FromArgb(0x72, 0x00, 0x00, 0x00));
        });
    }

    [Fact]
    public void Clearing_And_Readding_Items_Tracks_The_Semantic_Markers()
    {
        var breadcrumb = new AtomUIBreadcrumb
        {
            Items =
            {
                new AtomUIBreadcrumbItem { Content = "Home" },
                new AtomUIBreadcrumbItem { Content = "Application" }
            }
        };

        ShowInWindow(breadcrumb, () =>
        {
            GetSemanticItems(breadcrumb).Length.ShouldBe(2);

            breadcrumb.Items.Clear();
            Dispatcher.UIThread.RunJobs();
            GetSemanticItems(breadcrumb).ShouldBeEmpty();
            GetSemanticSeparators(breadcrumb).ShouldBeEmpty();

            breadcrumb.Items.Add(new AtomUIBreadcrumbItem { Content = "Home" });
            breadcrumb.Items.Add(new AtomUIBreadcrumbItem { Content = "Application" });
            breadcrumb.Items.Add(new AtomUIBreadcrumbItem { Content = "Detail" });
            Dispatcher.UIThread.RunJobs();
            GetSemanticItems(breadcrumb).Length.ShouldBe(3);
            GetSemanticSeparators(breadcrumb).Length.ShouldBe(2);
        });
    }

    [Fact]
    public void Adding_Item_Keeps_Existing_Separator_Attached_And_Reuses_It()
    {
        var breadcrumb = new AtomUIBreadcrumb
        {
            Items =
            {
                new AtomUIBreadcrumbItem { Content = "Home" },
                new AtomUIBreadcrumbItem { Content = "Application" }
            }
        };

        ShowInWindow(breadcrumb, () =>
        {
            var separator = GetSemanticSeparators(breadcrumb).Single();
            var attachedCount = 0;
            var detachedCount = 0;
            separator.AttachedToVisualTree += (_, _) => attachedCount++;
            separator.DetachedFromVisualTree += (_, _) => detachedCount++;

            breadcrumb.Items.Add(new AtomUIBreadcrumbItem { Content = "Detail" });
            Dispatcher.UIThread.RunJobs();

            var separators = GetSemanticSeparators(breadcrumb);
            separators.Length.ShouldBe(2);
            separators[0].ShouldBeSameAs(separator);
            attachedCount.ShouldBe(0);
            detachedCount.ShouldBe(0);
        });
    }

    [Fact]
    public void Pending_Separator_Update_Is_Cancelled_On_Detach_And_Rebuilt_On_Reattach()
    {
        var breadcrumb = new AtomUIBreadcrumb
        {
            Items = { new AtomUIBreadcrumbItem { Content = "A" }, new AtomUIBreadcrumbItem { Content = "B" } }
        };
        var window = new AvaloniaWindow { Width = 500, Height = 200, Content = breadcrumb };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var oldSeparator = GetSemanticSeparators(breadcrumb).Single();
            breadcrumb.Items.Add(new AtomUIBreadcrumbItem { Content = "C" });
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            oldSeparator.GetVisualParent().ShouldBeNull();
            oldSeparator.Parent.ShouldBeNull();
            breadcrumb.Items.Clear();
            breadcrumb.Items.Add(new AtomUIBreadcrumbItem { Content = "New", Separator = ":" });
            breadcrumb.Items.Add(new AtomUIBreadcrumbItem { Content = "Tail" });
            window.Content = breadcrumb;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            GetSemanticSeparators(breadcrumb).Single().Content.ShouldBe(":");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Separators_Are_Siblings_Interleaved_After_Their_Owning_Item()
    {
        var breadcrumb = new AtomUIBreadcrumb
        {
            Items =
            {
                new AtomUIBreadcrumbItem { Content = "Home", Separator = ":" },
                new AtomUIBreadcrumbItem { Content = "Application", NavigateContext = "#" },
                new AtomUIBreadcrumbItem { Content = "Detail" }
            }
        };

        ShowInWindow(breadcrumb, () =>
        {
            var items      = GetSemanticItems(breadcrumb);
            var separators = GetSemanticSeparators(breadcrumb);

            items.Length.ShouldBe(3);
            // N items produce N - 1 separators; the last item has no trailing separator.
            separators.Length.ShouldBe(2);

            // A per-item override flows to the trailing sibling separator.
            separators[0].Content.ShouldBe(":");
            separators[1].Content.ShouldBe("/");

            // Separators are visual siblings of the item containers inside the items panel,
            // but the panel's generator-owned Children collection keeps containers only.
            var panel    = breadcrumb.GetVisualDescendants().OfType<Panel>().First();
            var children = panel.Children.ToArray();

            children.Length.ShouldBe(3);
            children[0].ShouldBeSameAs(items[0]);
            children[1].ShouldBeSameAs(items[1]);
            children[2].ShouldBeSameAs(items[2]);
            separators[0].GetVisualParent().ShouldBeSameAs(panel);
            separators[1].GetVisualParent().ShouldBeSameAs(panel);

            // Spatial order: container 0, separator 0, container 1, separator 1, container 2.
            separators[0].Bounds.Left.ShouldBeGreaterThanOrEqualTo(items[0].Bounds.Right);
            separators[0].Bounds.Right.ShouldBeLessThanOrEqualTo(items[1].Bounds.Left);
            separators[1].Bounds.Left.ShouldBeGreaterThanOrEqualTo(items[1].Bounds.Right);
            separators[1].Bounds.Right.ShouldBeLessThanOrEqualTo(items[2].Bounds.Left);
        });
    }

    private static AtomUIBreadcrumbItem[] GetSemanticItems(AtomUIBreadcrumb breadcrumb)
    {
        return breadcrumb.GetVisualDescendants()
                         .OfType<AtomUIBreadcrumbItem>()
                         .Where(static item => item.Classes.Contains(ItemClass))
                         .ToArray();
    }

    private static ContentPresenter[] GetSemanticSeparators(AtomUIBreadcrumb breadcrumb)
    {
        return breadcrumb.GetVisualDescendants()
                         .OfType<ContentPresenter>()
                         .Where(static presenter => presenter.Classes.Contains(SeparatorClass))
                         .ToArray();
    }

    private static ContentPresenter FindContentPresenter(AtomUIBreadcrumbItem item)
    {
        return item.GetVisualDescendants()
                   .OfType<ContentPresenter>()
                   .Single(static presenter => presenter.Name == "Content");
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
        part.ContractType.ShouldBe(typeof(AtomUIBreadcrumb));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
    }

    private static void AssertPart(
        SemanticPartDescriptor part,
        string selectorClass,
        Type contractType,
        Type styleType,
        string selectorRoute,
        bool runtimeCreated)
    {
        part.Path.ShouldBe(part.Name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.StyleType.ShouldBe(styleType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Multiple);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBe(runtimeCreated);
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

    private static void AssertNoSemanticSelectors(string relativePath)
    {
        var document = XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);
        var selectors = document.Descendants()
                                .Where(static element => element.Name.LocalName == "Style")
                                .Attributes("Selector")
                                .Select(static attribute => attribute.Value)
                                .ToArray();
        selectors.ShouldAllBe(static selector => !selector.Contains("semantic-", StringComparison.Ordinal));
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
