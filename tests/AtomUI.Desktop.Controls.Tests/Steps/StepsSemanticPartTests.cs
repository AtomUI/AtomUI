using System.Xml.Linq;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISteps = AtomUI.Desktop.Controls.Steps;
using AtomUIStepsItem = AtomUI.Desktop.Controls.StepsItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Steps;

public class StepsSemanticPartTests
{
    private const string ItemClass         = "semantic-item";
    private const string ItemWrapperClass  = "semantic-item-wrapper";
    private const string ItemIconClass     = "semantic-item-icon";
    private const string ItemTitleClass    = "semantic-item-title";
    private const string ItemSubtitleClass = "semantic-item-subtitle";
    private const string ItemSectionClass  = "semantic-item-section";
    private const string ItemContentClass  = "semantic-item-content";
    private const string ItemRailClass     = "semantic-item-rail";

    static StepsSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_The_Ant_Design_Aligned_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUISteps), out var stepsDescriptor).ShouldBeTrue();
        stepsDescriptor.ShouldNotBeNull();
        stepsDescriptor.Parts.Select(static part => part.Name).ShouldBe(
        [
            "root",
            "item",
            "itemContent",
            "itemIcon",
            "itemRail",
            "itemSection",
            "itemSubtitle",
            "itemTitle",
            "itemWrapper"
        ]);

        AssertRoot(stepsDescriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomUISteps));

        AssertItemPart(stepsDescriptor.Parts.Single(static part => part.Name == "item"),
            "semantic-item",
            "> .semantic-item",
            typeof(AtomUIStepsItem),
            typeof(StepsItemStyle));
        AssertItemPart(stepsDescriptor.Parts.Single(static part => part.Name == "itemWrapper"),
            ItemWrapperClass,
            "> .semantic-item /template/ .semantic-item-wrapper",
            typeof(Border),
            typeof(StepsItemWrapperStyle));
        AssertItemPart(stepsDescriptor.Parts.Single(static part => part.Name == "itemIcon"),
            ItemIconClass,
            "> .semantic-item /template/ .semantic-item-icon",
            typeof(TemplatedControl),
            typeof(StepsItemIconStyle));
        AssertItemPart(stepsDescriptor.Parts.Single(static part => part.Name == "itemTitle"),
            ItemTitleClass,
            "> .semantic-item /template/ .semantic-item-title",
            typeof(ContentPresenter),
            typeof(StepsItemTitleStyle));
        AssertItemPart(stepsDescriptor.Parts.Single(static part => part.Name == "itemSubtitle"),
            ItemSubtitleClass,
            "> .semantic-item /template/ .semantic-item-subtitle",
            typeof(ContentPresenter),
            typeof(StepsItemSubtitleStyle));
        AssertItemPart(stepsDescriptor.Parts.Single(static part => part.Name == "itemSection"),
            ItemSectionClass,
            "> .semantic-item /template/ .semantic-item-section",
            typeof(Panel),
            typeof(StepsItemSectionStyle));
        AssertItemPart(stepsDescriptor.Parts.Single(static part => part.Name == "itemContent"),
            ItemContentClass,
            "> .semantic-item /template/ .semantic-item-content",
            typeof(ContentPresenter),
            typeof(StepsItemContentStyle));
        AssertItemPart(stepsDescriptor.Parts.Single(static part => part.Name == "itemRail"),
            ItemRailClass,
            "> .semantic-item /template/ .semantic-item-rail",
            typeof(DashedBorder),
            typeof(StepsItemRailStyle));

        registry.TryGetControl(typeof(AtomUIStepsItem), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(StepsPanel), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(StepsItemIndicator), out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/Steps/Themes/StepsItemTheme.axaml",
        new[]
        {
            "semantic-item-content:ContentPresenter",
            "semantic-item-icon:StepsItemIndicator",
            "semantic-item-rail:PixelAlignedBorder",
            "semantic-item-section:StepsItemSectionPanel",
            "semantic-item-subtitle:ContentPresenter",
            "semantic-item-title:ContentPresenter",
            "semantic-item-wrapper:StepsPanelItemFrame"
        })]
    public void Built_In_Themes_Declare_Expected_Static_Semantic_Markers(string relativePath, string[] expectedMarkers)
    {
        AssertThemeMarkers(relativePath, expectedMarkers);
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/Steps/Themes/StepsTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/Steps/Themes/StepsItemIndicatorTheme.axaml")]
    public void Built_In_Themes_Declare_No_Static_Semantic_Markers(string relativePath)
    {
        AssertThemeMarkers(relativePath, Array.Empty<string>());
    }

    [Fact]
    public void Realized_Items_Are_Item_Parts_With_All_Sub_Part_Markers()
    {
        var steps = new AtomUISteps
        {
            Current = 1
        };
        steps.Items.Add(new AtomUIStepsItem
        {
            Header    = "First",
            SubHeader = "First description",
            Content   = "First content"
        });
        steps.Items.Add(new AtomUIStepsItem
        {
            Header    = "Second",
            SubHeader = "Second description",
            Content   = "Second content"
        });

        using var window = Show(steps);

        var items = GetSemanticElements(steps, ItemClass).OfType<AtomUIStepsItem>().ToArray();
        items.Length.ShouldBe(2);
        steps.Classes.ShouldNotContain("semantic-root");

        foreach (var item in items)
        {
            GetSemanticElements(item, ItemWrapperClass).Length.ShouldBe(1);
            GetSemanticElements(item, ItemIconClass).Length.ShouldBe(1);
            GetSemanticElements(item, ItemTitleClass).Length.ShouldBe(1);
            GetSemanticElements(item, ItemSubtitleClass).Length.ShouldBe(1);
            GetSemanticElements(item, ItemSectionClass).Length.ShouldBe(1);
            GetSemanticElements(item, ItemContentClass).Length.ShouldBe(1);
            GetSemanticElements(item, ItemRailClass).Length.ShouldBe(1);
        }
    }

    [Fact]
    public void Generated_Item_Part_Styles_Apply_To_The_Expected_Template_Nodes()
    {
        var steps = new AtomUISteps
        {
            Current = 1
        };
        steps.Items.Add(new AtomUIStepsItem
        {
            Header    = "First",
            SubHeader = "First description",
            Content   = "First content"
        });
        steps.Items.Add(new AtomUIStepsItem
        {
            Header    = "Second",
            SubHeader = "Second description",
            Content   = "Second content"
        });
        steps.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUISteps>().Class("semantic-owner"));
        ownerStyle.Children.Add(new StepsItemStyle
        {
            Setters = { new Setter(Control.TagProperty, "item") }
        });
        ownerStyle.Children.Add(new StepsItemWrapperStyle
        {
            Setters = { new Setter(Control.TagProperty, "wrapper") }
        });
        ownerStyle.Children.Add(new StepsItemIconStyle
        {
            Setters = { new Setter(Control.TagProperty, "icon") }
        });
        ownerStyle.Children.Add(new StepsItemTitleStyle
        {
            Setters = { new Setter(Control.TagProperty, "title") }
        });
        ownerStyle.Children.Add(new StepsItemSubtitleStyle
        {
            Setters = { new Setter(Control.TagProperty, "subtitle") }
        });
        ownerStyle.Children.Add(new StepsItemSectionStyle
        {
            Setters = { new Setter(Control.TagProperty, "section") }
        });
        ownerStyle.Children.Add(new StepsItemContentStyle
        {
            Setters = { new Setter(Control.TagProperty, "content") }
        });
        ownerStyle.Children.Add(new StepsItemRailStyle
        {
            Setters = { new Setter(Control.TagProperty, "rail") }
        });
        steps.Styles.Add(ownerStyle);

        using var window = Show(steps);

        var items = GetSemanticElements(steps, ItemClass).OfType<AtomUIStepsItem>().ToArray();
        items.Length.ShouldBe(2);
        items.ShouldAllBe(static item => Equals(item.Tag, "item"));

        foreach (var item in items)
        {
            GetSemanticElements(item, ItemWrapperClass).OfType<Control>().Single().Tag.ShouldBe("wrapper");
            GetSemanticElements(item, ItemIconClass).OfType<Control>().Single().Tag.ShouldBe("icon");
            GetSemanticElements(item, ItemTitleClass).OfType<Control>().Single().Tag.ShouldBe("title");
            GetSemanticElements(item, ItemSubtitleClass).OfType<Control>().Single().Tag.ShouldBe("subtitle");
            GetSemanticElements(item, ItemSectionClass).OfType<Control>().Single().Tag.ShouldBe("section");
            GetSemanticElements(item, ItemContentClass).OfType<Control>().Single().Tag.ShouldBe("content");
            GetSemanticElements(item, ItemRailClass).OfType<Control>().Single().Tag.ShouldBe("rail");
        }
    }

    [Fact]
    public void Item_Icon_Corner_Radius_Defaults_To_The_Circular_Frame()
    {
        var steps = new AtomUISteps
        {
            Current = 1
        };
        steps.Items.Add(new AtomUIStepsItem
        {
            Header    = "First",
            SubHeader = "First description",
            Content   = "First content"
        });

        using var window = Show(steps);

        var icon = GetSemanticElements(steps, ItemIconClass).OfType<TemplatedControl>().Single();
        icon.CornerRadius.ShouldBe(new CornerRadius(32));
    }

    [Fact]
    public void Generated_ItemIcon_Style_Overrides_The_Default_Circular_Corner_Radius()
    {
        var steps = new AtomUISteps
        {
            Current = 1
        };
        steps.Items.Add(new AtomUIStepsItem
        {
            Header    = "First",
            SubHeader = "First description",
            Content   = "First content"
        });
        steps.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUISteps>().Class("semantic-owner"));
        ownerStyle.Children.Add(new StepsItemIconStyle
        {
            Setters = { new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(10)) }
        });
        steps.Styles.Add(ownerStyle);

        using var window = Show(steps);

        var icon = GetSemanticElements(steps, ItemIconClass).OfType<TemplatedControl>().Single();
        icon.CornerRadius.ShouldBe(new CornerRadius(10));
    }

    private static void AssertRoot(SemanticPartDescriptor part, Type ownerType)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.SelectorRoute.ShouldBeNull();
        part.ContractType.ShouldBe(ownerType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.StyleType.ShouldBeNull();
        part.Since.ShouldBe("6.2.0");
    }

    private static void AssertItemPart(
        SemanticPartDescriptor part,
        string selectorClass,
        string selectorRoute,
        Type contractType,
        Type styleType)
    {
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Multiple);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeTrue();
        part.StyleType.ShouldBe(styleType);
        part.Since.ShouldBe("6.2.0");
    }

    private static Control[] GetSemanticElements(Control owner, string semanticClass)
    {
        return owner.GetVisualDescendants()
                    .OfType<Control>()
                    .Where(control => control.Classes.Contains(semanticClass))
                    .ToArray();
    }

    private static IDisposable Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 900,
            Height  = 400,
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

    private sealed class WindowLifetime(AvaloniaWindow window) : IDisposable
    {
        public void Dispose()
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
