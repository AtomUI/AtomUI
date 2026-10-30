using System.Xml.Linq;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Card;

public class CardSemanticPartTests
{
    static CardSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_Only_The_Approved_Card_Family_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(Desktop.Controls.Card), out var cardDescriptor).ShouldBeTrue();
        cardDescriptor.ShouldNotBeNull();
        cardDescriptor.Parts.Select(static part => part.Name)
                      .ShouldBe(["root", "actions", "body", "cover", "extra", "header", "title"]);

        AssertRoot(cardDescriptor.Parts.Single(static part => part.Name == "root"), typeof(Desktop.Controls.Card));
        AssertPart(
            cardDescriptor.Parts.Single(static part => part.Name == "header"),
            "semantic-header",
            typeof(DashedBorder));
        AssertPart(
            cardDescriptor.Parts.Single(static part => part.Name == "title"),
            "semantic-title",
            typeof(ContentPresenter));
        AssertPart(
            cardDescriptor.Parts.Single(static part => part.Name == "extra"),
            "semantic-extra",
            typeof(ContentPresenter));
        AssertPart(
            cardDescriptor.Parts.Single(static part => part.Name == "cover"),
            "semantic-cover",
            typeof(Border));
        AssertPart(
            cardDescriptor.Parts.Single(static part => part.Name == "body"),
            "semantic-body",
            typeof(Border));
        AssertPart(
            cardDescriptor.Parts.Single(static part => part.Name == "actions"),
            "semantic-actions",
            typeof(TemplatedControl));

        registry.TryGetControl(typeof(Desktop.Controls.CardMetaContent), out var metaDescriptor).ShouldBeTrue();
        metaDescriptor.ShouldNotBeNull();
        metaDescriptor.Parts.Select(static part => part.Name)
                      .ShouldBe(["root", "avatar", "description", "section", "title"]);

        AssertRoot(
            metaDescriptor.Parts.Single(static part => part.Name == "root"),
            typeof(Desktop.Controls.CardMetaContent));
        AssertPart(
            metaDescriptor.Parts.Single(static part => part.Name == "section"),
            "semantic-section",
            typeof(Control));
        AssertPart(
            metaDescriptor.Parts.Single(static part => part.Name == "avatar"),
            "semantic-avatar",
            typeof(ContentPresenter));
        AssertPart(
            metaDescriptor.Parts.Single(static part => part.Name == "title"),
            "semantic-title",
            typeof(ContentPresenter));
        AssertPart(
            metaDescriptor.Parts.Single(static part => part.Name == "description"),
            "semantic-description",
            typeof(ContentPresenter));

        registry.TryGetControl(typeof(Desktop.Controls.CardActionButton), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(Desktop.Controls.CardGridContent), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(Desktop.Controls.CardGridItem), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(Desktop.Controls.CardTabsContent), out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData(
        "src/AtomUI.Desktop.Controls/Card/Themes/CardTheme.axaml",
        "semantic-header:PixelAlignedBorder,semantic-title:ContentPresenter,semantic-extra:ContentPresenter,semantic-cover:Border,semantic-body:Border,semantic-actions:CardActionPanel")]
    [InlineData(
        "src/AtomUI.Desktop.Controls/Card/Themes/CardMetaContentTheme.axaml",
        "semantic-section:DockPanel,semantic-avatar:ContentPresenter,semantic-title:ContentPresenter,semantic-description:ContentPresenter")]
    public void Built_In_Templates_Implement_The_Approved_Static_Marker_Contract(
        string relativePath,
        string expectedMarkers)
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
                            .ShouldBe(expectedMarkers.Split(',').OrderBy(static value => value, StringComparer.Ordinal));
        classPropertyMarkers.ShouldNotContain(static marker =>
            marker.Attribute.Name.LocalName == "Classes.semantic-root");
    }

    [Theory]
    [InlineData("default")]
    [InlineData("headerless")]
    [InlineData("loading")]
    [InlineData("meta")]
    [InlineData("grid")]
    [InlineData("tabs")]
    public void Card_Template_Preserves_One_Owner_Scoped_Marker_Per_Declared_Part(string scenario)
    {
        var card = CreateCardScenario(scenario);

        ShowInWindow(card, () =>
        {
            var markers = GetOwnerMarkers(card);

            markers.SelectMany(static control => control.Classes)
                   .Where(static className => className.StartsWith("semantic-", StringComparison.Ordinal))
                   .OrderBy(static className => className, StringComparer.Ordinal)
                   .ShouldBe([
                       "semantic-actions",
                       "semantic-body",
                       "semantic-cover",
                       "semantic-extra",
                       "semantic-header",
                       "semantic-title"
                   ]);
        });
    }

    [Fact]
    public void Actions_Collection_Changes_Do_Not_Recreate_The_Semantic_Actions_Part()
    {
        var card = new Desktop.Controls.Card
        {
            Header = "Actions",
            Content = "Body"
        };

        ShowInWindow(card, () =>
        {
            var actionsPart = FindOwnerMarker(card, "semantic-actions");

            card.Actions.Add(new Desktop.Controls.CardActionButton());
            Dispatcher.UIThread.RunJobs();
            FindOwnerMarker(card, "semantic-actions").ShouldBeSameAs(actionsPart);

            card.Actions.Clear();
            Dispatcher.UIThread.RunJobs();
            FindOwnerMarker(card, "semantic-actions").ShouldBeSameAs(actionsPart);
        });
    }

    [Fact]
    public void Card_And_Nested_Meta_Title_Selectors_Remain_Isolated_By_Their_Owner()
    {
        var meta = new Desktop.Controls.CardMetaContent
        {
            Header = "Meta title",
            Content = "Meta description"
        };
        meta.Classes.Add("meta-owner");

        var card = new Desktop.Controls.Card
        {
            Header = "Card title",
            Content = meta
        };
        card.Classes.Add("card-owner");
        card.Styles.Add(new Style(selector =>
            selector.OfType<Desktop.Controls.Card>().Class("card-owner").Template().Class("semantic-title"))
        {
            Setters =
            {
                new Setter(Control.TagProperty, "card-title")
            }
        });
        meta.Styles.Add(new Style(selector =>
            selector.OfType<Desktop.Controls.CardMetaContent>().Class("meta-owner").Template().Class("semantic-title"))
        {
            Setters =
            {
                new Setter(Control.TagProperty, "meta-title")
            }
        });

        ShowInWindow(card, () =>
        {
            FindOwnerMarker(card, "semantic-title").Tag.ShouldBe("card-title");
            FindOwnerMarker(meta, "semantic-title").Tag.ShouldBe("meta-title");
            GetOwnerMarkers(card).Length.ShouldBe(6);
            GetOwnerMarkers(meta).Length.ShouldBe(4);
        });
    }

    private static Desktop.Controls.Card CreateCardScenario(string scenario)
    {
        var card = new Desktop.Controls.Card
        {
            Header = "Title",
            Extra = "Extra",
            Cover = new Border { Height = 32 },
            Content = "Body"
        };
        card.Actions.Add(new Desktop.Controls.CardActionButton());

        switch (scenario)
        {
            case "default":
                break;
            case "headerless":
                card.Header = null;
                card.Extra = null;
                card.Cover = null;
                card.Actions.Clear();
                break;
            case "loading":
                card.IsLoading = true;
                break;
            case "meta":
                card.Content = new Desktop.Controls.CardMetaContent
                {
                    Header = "Meta",
                    Content = "Description"
                };
                break;
            case "grid":
                card.Content = new Desktop.Controls.CardGridContent
                {
                    Items =
                    {
                        new Desktop.Controls.CardGridItem { Content = "Grid" }
                    }
                };
                break;
            case "tabs":
                card.Content = new Desktop.Controls.CardTabsContent
                {
                    Items =
                    {
                        new Desktop.Controls.TabItem { Header = "Tab", Content = "Tab content" }
                    }
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null);
        }

        return card;
    }

    private static Control[] GetOwnerMarkers(TemplatedControl owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<Control>()
                    .Where(control => ReferenceEquals(control.TemplatedParent, owner))
                    .Where(static control => control.Classes.Any(
                        className => className.StartsWith("semantic-", StringComparison.Ordinal)))
                    .ToArray();
    }

    private static Control FindOwnerMarker(TemplatedControl owner, string marker)
    {
        return GetOwnerMarkers(owner).Single(control => control.Classes.Contains(marker));
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width = 640,
            Height = 480,
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

    private static void AssertRoot(SemanticPartDescriptor part, Type ownerType)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.ContractType.ShouldBe(ownerType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
    }

    private static void AssertPart(
        SemanticPartDescriptor part,
        string selectorClass,
        Type contractType)
    {
        part.Path.ShouldBe(part.Name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
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
