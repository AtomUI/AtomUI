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
using AtomUICardTabStrip = AtomUI.Desktop.Controls.CardTabStrip;
using AtomUITabStrip = AtomUI.Desktop.Controls.TabStrip;
using AtomUITabStripItem = AtomUI.Desktop.Controls.TabStripItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabStripSemanticPartTests
{
    private const string ItemClass  = "semantic-item";
    private const string AddClass   = "semantic-add";
    private const string IconClass  = "semantic-icon";
    private const string LabelClass = "semantic-label";
    private const string CloseClass = "semantic-close";

    static TabStripSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_The_Expected_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUITabStrip), out var tabStripDescriptor).ShouldBeTrue();
        tabStripDescriptor.ShouldNotBeNull();
        tabStripDescriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "item"]);

        AssertRoot(tabStripDescriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomUITabStrip));
        AssertPart(tabStripDescriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass, "> .semantic-item", typeof(AtomUITabStripItem),
            SemanticPartCardinality.Multiple, typeof(TabStripItemStyle), runtimeCreated: true);

        registry.TryGetControl(typeof(AtomUICardTabStrip), out var cardDescriptor).ShouldBeTrue();
        cardDescriptor.ShouldNotBeNull();
        cardDescriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "add", "item"]);

        AssertRoot(cardDescriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomUICardTabStrip));
        AssertPart(cardDescriptor.Parts.Single(static part => part.Name == "add"),
            AddClass, "/template/ .semantic-add", typeof(IconButton),
            SemanticPartCardinality.Single, typeof(CardTabStripAddStyle), runtimeCreated: false);
        AssertPart(cardDescriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass, "> .semantic-item", typeof(AtomUITabStripItem),
            SemanticPartCardinality.Multiple, typeof(CardTabStripItemStyle), runtimeCreated: true);

        registry.TryGetControl(typeof(AtomUITabStripItem), out var stripItemDescriptor).ShouldBeTrue();
        stripItemDescriptor.ShouldNotBeNull();
        stripItemDescriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "close", "icon", "label"]);

        AssertRoot(stripItemDescriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomUITabStripItem));
        AssertPart(stripItemDescriptor.Parts.Single(static part => part.Name == "close"),
            CloseClass, "/template/ .semantic-close", typeof(IconButton),
            SemanticPartCardinality.Single, typeof(TabStripItemCloseStyle), runtimeCreated: false);
        AssertPart(stripItemDescriptor.Parts.Single(static part => part.Name == "icon"),
            IconClass, "/template/ .semantic-icon", typeof(AtomUI.Controls.IconPresenter),
            SemanticPartCardinality.Single, typeof(TabStripItemIconStyle), runtimeCreated: false);
        AssertPart(stripItemDescriptor.Parts.Single(static part => part.Name == "label"),
            LabelClass, "/template/ .semantic-label", typeof(ContentPresenter),
            SemanticPartCardinality.Single, typeof(TabStripItemLabelStyle), runtimeCreated: false);

        registry.TryGetControl(typeof(BaseTabStrip), out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/Themes/TabStrip/CardTabStripTheme.axaml",
        new[] { "semantic-add:IconButton" })]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/Themes/TabStrip/BaseTabStripItemTheme.axaml",
        new[] { "semantic-close:IconButton", "semantic-icon:IconPresenter", "semantic-label:ContentPresenter" })]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/Themes/TabStrip/CardTabStripItemTheme.axaml",
        new[] { "semantic-close:IconButton", "semantic-icon:IconPresenter", "semantic-label:ContentPresenter" })]
    public void Built_In_Themes_Declare_Expected_Static_Semantic_Markers(string relativePath, string[] expectedMarkers)
    {
        AssertThemeMarkers(relativePath, expectedMarkers);
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/Themes/TabStrip/TabStripTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/Themes/TabStrip/BaseTabStripTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/Themes/TabStrip/TabStripItemTheme.axaml")]
    public void Built_In_Themes_Declare_No_Static_Semantic_Markers(string relativePath)
    {
        AssertThemeMarkers(relativePath, Array.Empty<string>());
    }

    [Fact]
    public void Realized_Strip_Items_Are_Item_Parts()
    {
        var tabStrip = new AtomUITabStrip();
        tabStrip.ItemsSource = new[] { "First", "Second" };

        using var window = Show(tabStrip);

        var items = GetSemanticElements(tabStrip, ItemClass).OfType<AtomUITabStripItem>().ToArray();
        items.Length.ShouldBe(2);
        tabStrip.Classes.ShouldNotContain("semantic-root");
    }

    [Fact]
    public void Generated_Item_Style_Applies_To_Strip_Items_Only()
    {
        var tabStrip = new AtomUITabStrip();
        tabStrip.ItemsSource = new[] { "First", "Second" };
        tabStrip.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUITabStrip>().Class("semantic-owner"));
        ownerStyle.Children.Add(new TabStripItemStyle
        {
            Setters = { new Setter(Control.TagProperty, "item") }
        });
        tabStrip.Styles.Add(ownerStyle);

        using var window = Show(tabStrip);

        GetSemanticElements(tabStrip, ItemClass).OfType<Control>()
                                               .ShouldAllBe(static control => Equals(control.Tag, "item"));
    }

    [Fact]
    public void Generated_Add_Style_Applies_To_The_Add_Button_Only()
    {
        var cardTabStrip = new AtomUICardTabStrip();
        cardTabStrip.ItemsSource = new[] { "First" };
        cardTabStrip.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUICardTabStrip>().Class("semantic-owner"));
        ownerStyle.Children.Add(new CardTabStripAddStyle
        {
            Setters = { new Setter(Control.TagProperty, "add") }
        });
        cardTabStrip.Styles.Add(ownerStyle);

        using var window = Show(cardTabStrip);

        var addButtons = GetSemanticElements(cardTabStrip, AddClass).OfType<Control>().ToArray();
        addButtons.Length.ShouldBe(1);
        addButtons[0].Tag.ShouldBe("add");
        GetSemanticElements(cardTabStrip, ItemClass).OfType<Control>()
                                                   .ShouldAllBe(static control => control.Tag == null);
    }

    [Fact]
    public void Strip_Items_Carry_Icon_Label_And_Close_Markers_In_Line_And_Card_Templates()
    {
        var tabStrip = new AtomUITabStrip();
        tabStrip.ItemsSource = new[] { "A" };

        using (var window = Show(tabStrip))
        {
            AssertItemSubPartMarkers(tabStrip);
        }

        var cardTabStrip = new AtomUICardTabStrip();
        cardTabStrip.ItemsSource = new[] { "A" };

        using (var window = Show(cardTabStrip))
        {
            AssertItemSubPartMarkers(cardTabStrip);
        }
    }

    [Fact]
    public void Generated_StripItem_SubPart_Styles_Apply_Within_Owner_Scope()
    {
        var tabStrip = new AtomUITabStrip();
        var styledItem = new AtomUITabStripItem { Content = "A" };
        styledItem.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUITabStripItem>().Class("semantic-owner"));
        ownerStyle.Children.Add(new TabStripItemIconStyle
        {
            Setters = { new Setter(Control.TagProperty, "icon") }
        });
        ownerStyle.Children.Add(new TabStripItemLabelStyle
        {
            Setters = { new Setter(Control.TagProperty, "label") }
        });
        ownerStyle.Children.Add(new TabStripItemCloseStyle
        {
            Setters = { new Setter(Control.TagProperty, "close") }
        });
        styledItem.Styles.Add(ownerStyle);

        tabStrip.Items.Add(styledItem);
        tabStrip.Items.Add(new AtomUITabStripItem { Content = "B" });

        using var window = Show(tabStrip);

        styledItem.GetVisualDescendants().OfType<Control>().Single(
                      static control => control.Classes.Contains(IconClass)).Tag.ShouldBe("icon");
        styledItem.GetVisualDescendants().OfType<Control>().Single(
                      static control => control.Classes.Contains(LabelClass)).Tag.ShouldBe("label");
        styledItem.GetVisualDescendants().OfType<Control>().Single(
                      static control => control.Classes.Contains(CloseClass)).Tag.ShouldBe("close");

        var unstyledItem = tabStrip.GetVisualDescendants().OfType<AtomUITabStripItem>()
                                   .Single(item => item != styledItem);
        unstyledItem.GetVisualDescendants().OfType<Control>().ShouldAllBe(static control =>
            !Equals(control.Tag, "icon") && !Equals(control.Tag, "label") && !Equals(control.Tag, "close"));
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
        bool runtimeCreated)
    {
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBe(runtimeCreated);
        part.StyleType.ShouldBe(styleType);
        part.Since.ShouldBe("6.2.0");
    }

    private static void AssertItemSubPartMarkers(Control owner)
    {
        var item = owner.GetVisualDescendants().OfType<AtomUITabStripItem>().Single();
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
