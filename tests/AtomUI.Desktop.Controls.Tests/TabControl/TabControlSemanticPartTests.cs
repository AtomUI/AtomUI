using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUICardTabControl = AtomUI.Desktop.Controls.CardTabControl;
using AtomUITabControl = AtomUI.Desktop.Controls.TabControl;
using AtomUITabItem = AtomUI.Desktop.Controls.TabItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabControlSemanticPartTests
{
    private const string ItemClass      = "semantic-item";
    private const string HeaderClass    = "semantic-header";
    private const string ContentClass   = "semantic-content";
    private const string AddClass       = "semantic-add";
    private const string IndicatorClass = "semantic-indicator";
    private const string IconClass      = "semantic-icon";
    private const string LabelClass     = "semantic-label";
    private const string CloseClass     = "semantic-close";

    static TabControlSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_The_Expected_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUITabControl), out var tabControlDescriptor).ShouldBeTrue();
        tabControlDescriptor.ShouldNotBeNull();
        tabControlDescriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "content", "header", "indicator", "item"]);

        AssertRoot(tabControlDescriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomUITabControl));
        AssertPart(tabControlDescriptor.Parts.Single(static part => part.Name == "header"),
            HeaderClass, "/template/ .semantic-header", typeof(Border),
            SemanticPartCardinality.Single, typeof(TabControlHeaderStyle), runtimeCreated: false,
            since: "6.2.0");
        AssertPart(tabControlDescriptor.Parts.Single(static part => part.Name == "indicator"),
            IndicatorClass, "/template/ .semantic-indicator", typeof(Border),
            SemanticPartCardinality.Single, typeof(TabControlIndicatorStyle), runtimeCreated: false,
            since: "6.2.0");
        AssertPart(tabControlDescriptor.Parts.Single(static part => part.Name == "content"),
            ContentClass, "/template/ .semantic-content", typeof(ContentPresenter),
            SemanticPartCardinality.Single, typeof(TabControlContentStyle), runtimeCreated: false);
        AssertPart(tabControlDescriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass, "> .semantic-item", typeof(AtomUITabItem),
            SemanticPartCardinality.Multiple, typeof(TabControlItemStyle), runtimeCreated: true);

        registry.TryGetControl(typeof(AtomUICardTabControl), out var cardDescriptor).ShouldBeTrue();
        cardDescriptor.ShouldNotBeNull();
        cardDescriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "add", "content", "header", "item"]);

        AssertRoot(cardDescriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomUICardTabControl));
        AssertPart(cardDescriptor.Parts.Single(static part => part.Name == "header"),
            HeaderClass, "/template/ .semantic-header", typeof(Border),
            SemanticPartCardinality.Single, typeof(CardTabControlHeaderStyle), runtimeCreated: false,
            since: "6.2.0");
        AssertPart(cardDescriptor.Parts.Single(static part => part.Name == "add"),
            AddClass, "/template/ .semantic-add", typeof(IconButton),
            SemanticPartCardinality.Single, typeof(CardTabControlAddStyle), runtimeCreated: false);
        AssertPart(cardDescriptor.Parts.Single(static part => part.Name == "content"),
            ContentClass, "/template/ .semantic-content", typeof(ContentPresenter),
            SemanticPartCardinality.Single, typeof(CardTabControlContentStyle), runtimeCreated: false);
        AssertPart(cardDescriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass, "> .semantic-item", typeof(AtomUITabItem),
            SemanticPartCardinality.Multiple, typeof(CardTabControlItemStyle), runtimeCreated: true);

        registry.TryGetControl(typeof(AtomUITabItem), out var tabItemDescriptor).ShouldBeTrue();
        tabItemDescriptor.ShouldNotBeNull();
        tabItemDescriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "close", "icon", "label"]);

        AssertRoot(tabItemDescriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomUITabItem));
        AssertPart(tabItemDescriptor.Parts.Single(static part => part.Name == "close"),
            CloseClass, "/template/ .semantic-close", typeof(IconButton),
            SemanticPartCardinality.Single, typeof(TabItemCloseStyle), runtimeCreated: false);
        AssertPart(tabItemDescriptor.Parts.Single(static part => part.Name == "icon"),
            IconClass, "/template/ .semantic-icon", typeof(AtomUI.Controls.IconPresenter),
            SemanticPartCardinality.Single, typeof(TabItemIconStyle), runtimeCreated: false);
        AssertPart(tabItemDescriptor.Parts.Single(static part => part.Name == "label"),
            LabelClass, "/template/ .semantic-label", typeof(ContentPresenter),
            SemanticPartCardinality.Single, typeof(TabItemLabelStyle), runtimeCreated: false);

        registry.TryGetControl(typeof(BaseTabControl), out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/Themes/TabControlTheme.axaml",
        new[] { "semantic-content:ContentPresenter", "semantic-header:Border", "semantic-indicator:Border" })]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/Themes/CardTabControlTheme.axaml",
        new[] { "semantic-add:IconButton", "semantic-content:ContentPresenter", "semantic-header:Border" })]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/Themes/BaseTabItemTheme.axaml",
        new[] { "semantic-close:IconButton", "semantic-icon:IconPresenter", "semantic-label:ContentPresenter" })]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/Themes/CardTabItemTheme.axaml",
        new[] { "semantic-close:IconButton", "semantic-icon:IconPresenter", "semantic-label:ContentPresenter" })]
    public void Built_In_Themes_Declare_Expected_Static_Semantic_Markers(string relativePath, string[] expectedMarkers)
    {
        AssertThemeMarkers(relativePath, expectedMarkers);
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/Themes/BaseTabControlTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/Themes/TabItemTheme.axaml")]
    public void Built_In_Themes_Declare_No_Static_Semantic_Markers(string relativePath)
    {
        AssertThemeMarkers(relativePath, Array.Empty<string>());
    }

    [Fact]
    public void Realized_Tab_Items_Are_Item_Parts()
    {
        var tabControl = new AtomUITabControl();
        tabControl.ItemsSource = new[] { "First", "Second", "Third" };

        using var window = Show(tabControl);

        var items = GetSemanticElements(tabControl, ItemClass).OfType<AtomUITabItem>().ToArray();
        items.Length.ShouldBe(3);
        tabControl.Classes.ShouldNotContain("semantic-root");
    }

    [Fact]
    public void User_Provided_TabItem_Instances_Receive_The_Item_Marker()
    {
        var tabControl = new AtomUITabControl();
        tabControl.Items.Add(new AtomUITabItem { Header = "A" });
        tabControl.Items.Add(new AtomUITabItem { Header = "B" });

        using var window = Show(tabControl);

        GetSemanticElements(tabControl, ItemClass).OfType<AtomUITabItem>().ToArray().Length.ShouldBe(2);
    }

    [Fact]
    public void Generated_Item_And_Content_Styles_Apply_To_Expected_Elements()
    {
        var tabControl = new AtomUITabControl();
        tabControl.ItemsSource = new[] { "First", "Second" };
        tabControl.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUITabControl>().Class("semantic-owner"));
        ownerStyle.Children.Add(new TabControlItemStyle
        {
            Setters = { new Setter(Control.TagProperty, "item") }
        });
        ownerStyle.Children.Add(new TabControlContentStyle
        {
            Setters = { new Setter(Control.TagProperty, "content") }
        });
        tabControl.Styles.Add(ownerStyle);

        using var window = Show(tabControl);

        GetSemanticElements(tabControl, ItemClass).OfType<Control>()
                                                 .ShouldAllBe(static control => Equals(control.Tag, "item"));
        GetSemanticElements(tabControl, ContentClass).OfType<Control>()
                                                     .ShouldAllBe(static control => Equals(control.Tag, "content"));
    }

    [Fact]
    public void Generated_Add_Style_Applies_To_The_Add_Button_Only()
    {
        var cardTabControl = new AtomUICardTabControl();
        cardTabControl.ItemsSource = new[] { "First" };
        cardTabControl.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUICardTabControl>().Class("semantic-owner"));
        ownerStyle.Children.Add(new CardTabControlAddStyle
        {
            Setters = { new Setter(Control.TagProperty, "add") }
        });
        cardTabControl.Styles.Add(ownerStyle);

        using var window = Show(cardTabControl);

        var addButtons = GetSemanticElements(cardTabControl, AddClass).OfType<Control>().ToArray();
        addButtons.Length.ShouldBe(1);
        addButtons[0].Tag.ShouldBe("add");
        GetSemanticElements(cardTabControl, ItemClass).OfType<Control>()
                                                     .ShouldAllBe(static control => control.Tag == null);
    }

    [Fact]
    public void Tab_Items_Carry_Icon_Label_And_Close_Markers_In_Line_And_Card_Templates()
    {
        var tabControl = new AtomUITabControl();
        tabControl.ItemsSource = new[] { "A" };

        using (var window = Show(tabControl))
        {
            AssertItemSubPartMarkers(tabControl);
        }

        var cardTabControl = new AtomUICardTabControl();
        cardTabControl.ItemsSource = new[] { "A" };

        using (var window = Show(cardTabControl))
        {
            AssertItemSubPartMarkers(cardTabControl);
        }
    }

    [Fact]
    public void Generated_TabItem_SubPart_Styles_Apply_Within_Owner_Scope()
    {
        var tabControl = new AtomUITabControl();
        var styledItem = new AtomUITabItem { Header = "A" };
        styledItem.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUITabItem>().Class("semantic-owner"));
        ownerStyle.Children.Add(new TabItemIconStyle
        {
            Setters = { new Setter(Control.TagProperty, "icon") }
        });
        ownerStyle.Children.Add(new TabItemLabelStyle
        {
            Setters = { new Setter(Control.TagProperty, "label") }
        });
        ownerStyle.Children.Add(new TabItemCloseStyle
        {
            Setters = { new Setter(Control.TagProperty, "close") }
        });
        styledItem.Styles.Add(ownerStyle);

        tabControl.Items.Add(styledItem);
        tabControl.Items.Add(new AtomUITabItem { Header = "B" });

        using var window = Show(tabControl);

        styledItem.GetVisualDescendants().OfType<Control>().Single(
                      static control => control.Classes.Contains(IconClass)).Tag.ShouldBe("icon");
        styledItem.GetVisualDescendants().OfType<Control>().Single(
                      static control => control.Classes.Contains(LabelClass)).Tag.ShouldBe("label");
        styledItem.GetVisualDescendants().OfType<Control>().Single(
                      static control => control.Classes.Contains(CloseClass)).Tag.ShouldBe("close");

        var unstyledItem = tabControl.GetVisualDescendants().OfType<AtomUITabItem>()
                                     .Single(item => item != styledItem);
        unstyledItem.GetVisualDescendants().OfType<Control>().ShouldAllBe(static control =>
            !Equals(control.Tag, "icon") && !Equals(control.Tag, "label") && !Equals(control.Tag, "close"));
    }

    [Fact]
    public void SizeType_Baseline_Is_Preserved_When_Semantic_Styles_Override_Item_Padding()
    {
        AtomUITabControl CreateControl(AtomUI.SizeType sizeType)
        {
            var tabControl = new AtomUITabControl
            {
                SizeType = sizeType
            };
            tabControl.ItemsSource = new[] { "First" };
            tabControl.Classes.Add("semantic-owner");

            var ownerStyle = new Style(selector => selector.OfType<AtomUITabControl>().Class("semantic-owner"));
            ownerStyle.Children.Add(new TabControlItemStyle
            {
                Setters = { new Setter(AtomUITabItem.PaddingProperty, new Thickness(0)) }
            });
            tabControl.Styles.Add(ownerStyle);
            return tabControl;
        }

        var large = CreateControl(AtomUI.SizeType.Large);
        var small = CreateControl(AtomUI.SizeType.Small);

        using var window = Show(new StackPanel
        {
            Children = { large, small }
        });

        var largeItem = large.GetVisualDescendants().OfType<AtomUITabItem>().Single();
        var smallItem = small.GetVisualDescendants().OfType<AtomUITabItem>().Single();

        largeItem.Padding.ShouldBe(new Thickness(0));
        smallItem.Padding.ShouldBe(new Thickness(0));
        largeItem.FontSize.ShouldBeGreaterThan(smallItem.FontSize);
    }

    [Fact]
    public void Generated_Header_Styles_Apply_To_The_Header_Container()
    {
        var tabControl = new AtomUITabControl();
        tabControl.ItemsSource = new[] { "First" };
        tabControl.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUITabControl>().Class("semantic-owner"));
        ownerStyle.Children.Add(new TabControlHeaderStyle
        {
            Setters = { new Setter(Control.TagProperty, "header") }
        });
        tabControl.Styles.Add(ownerStyle);

        using (Show(tabControl))
        {
            GetSemanticElements(tabControl, HeaderClass).OfType<Control>()
                                                       .Single().Tag.ShouldBe("header");
        }

        var cardTabControl = new AtomUICardTabControl();
        cardTabControl.ItemsSource = new[] { "First" };
        cardTabControl.Classes.Add("semantic-owner");

        var cardOwnerStyle = new Style(selector => selector.OfType<AtomUICardTabControl>().Class("semantic-owner"));
        cardOwnerStyle.Children.Add(new CardTabControlHeaderStyle
        {
            Setters = { new Setter(Control.TagProperty, "header") }
        });
        cardTabControl.Styles.Add(cardOwnerStyle);

        using (Show(cardTabControl))
        {
            GetSemanticElements(cardTabControl, HeaderClass).OfType<Control>()
                                                           .Single().Tag.ShouldBe("header");
        }
    }

    [Fact]
    public void Generated_Indicator_Style_Applies_To_The_Indicator_Border()
    {
        var tabControl = new AtomUITabControl();
        tabControl.ItemsSource = new[] { "First" };
        tabControl.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUITabControl>().Class("semantic-owner"));
        ownerStyle.Children.Add(new TabControlIndicatorStyle
        {
            Setters = { new Setter(Control.TagProperty, "indicator") }
        });
        tabControl.Styles.Add(ownerStyle);

        using (Show(tabControl))
        {
            GetSemanticElements(tabControl, IndicatorClass).OfType<Control>()
                                                          .Single().Tag.ShouldBe("indicator");
        }
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

    private static void AssertPart(
        SemanticPartDescriptor part,
        string selectorClass,
        string selectorRoute,
        Type contractType,
        SemanticPartCardinality cardinality,
        Type styleType,
        bool runtimeCreated,
        string since = "6.2.0")
    {
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBe(runtimeCreated);
        part.StyleType.ShouldBe(styleType);
        part.Since.ShouldBe(since);
    }

    private static void AssertItemSubPartMarkers(Control owner)
    {
        var item = owner.GetVisualDescendants().OfType<AtomUITabItem>().Single();
        item.GetVisualDescendants().OfType<Control>().Count(
                static control => control.Classes.Contains(IconClass)).ShouldBe(1);
        item.GetVisualDescendants().OfType<Control>().Count(
                static control => control.Classes.Contains(LabelClass)).ShouldBe(1);
        item.GetVisualDescendants().OfType<Control>().Count(
                static control => control.Classes.Contains(CloseClass)).ShouldBe(1);
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
            Width   = 640,
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

    private sealed class WindowLifetime(AvaloniaWindow window) : IDisposable
    {
        public void Dispose()
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
