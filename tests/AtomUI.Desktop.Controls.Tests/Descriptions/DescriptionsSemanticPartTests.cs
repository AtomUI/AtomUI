using System.Xml.Linq;
using AtomUI.Controls.Primitives;
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
using AtomUISizeType = AtomUI.SizeType;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Descriptions;

public class DescriptionsSemanticPartTests
{
    private const string HeaderClass = "semantic-header";
    private const string TitleClass = "semantic-title";
    private const string ExtraClass = "semantic-extra";
    private const string LabelClass = "semantic-label";
    private const string ContentClass = "semantic-content";
    private const string LabelSelectorRoute =
        "/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-label";
    private const string ContentSelectorRoute =
        "/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-content";

    static DescriptionsSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(Desktop.Controls.Descriptions), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "content", "extra", "header", "label", "title"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(descriptor.Parts.Single(static part => part.Name == "header"),
            HeaderClass,
            typeof(DockPanel),
            SemanticPartCardinality.Single,
            false,
            "/template/ .semantic-header");
        AssertPart(descriptor.Parts.Single(static part => part.Name == "title"),
            TitleClass,
            typeof(ContentPresenter),
            SemanticPartCardinality.Single,
            false,
            "/template/ .semantic-title");
        AssertPart(descriptor.Parts.Single(static part => part.Name == "extra"),
            ExtraClass,
            typeof(ContentPresenter),
            SemanticPartCardinality.Single,
            false,
            "/template/ .semantic-extra");
        AssertPart(descriptor.Parts.Single(static part => part.Name == "label"),
            LabelClass,
            typeof(ContentPresenter),
            SemanticPartCardinality.Multiple,
            true,
            LabelSelectorRoute);
        AssertPart(descriptor.Parts.Single(static part => part.Name == "content"),
            ContentClass,
            typeof(ContentPresenter),
            SemanticPartCardinality.Multiple,
            true,
            ContentSelectorRoute);

        registry.TryGetControl(typeof(DescriptionItem), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(DescriptionDefaultItem), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(DescriptionBorderedItemLabel), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(DescriptionBorderedItemContent), out _).ShouldBeFalse();
    }

    [Fact]
    public void Descriptions_Template_Implements_Only_The_Approved_Static_Markers()
    {
        var document = XDocument.Load(
            GetRepoFile("src/AtomUI.Desktop.Controls/Descriptions/Themes/DescriptionsTheme.axaml"),
            LoadOptions.SetLineInfo);
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
                            .ShouldBe([
                                "semantic-extra:ContentPresenter",
                                "semantic-header:DockPanel",
                                "semantic-scope-items:Grid",
                                "semantic-title:ContentPresenter"
                            ]);
    }

    [Theory]
    [InlineData(Orientation.Horizontal, false)]
    [InlineData(Orientation.Horizontal, true)]
    [InlineData(Orientation.Vertical, false)]
    [InlineData(Orientation.Vertical, true)]
    public void Every_Layout_Produces_One_Label_And_Content_Target_Per_Item(
        Orientation layout,
        bool isBordered)
    {
        var descriptions = CreateDescriptions(layout, isBordered, 3);

        ShowInWindow(descriptions, () =>
        {
            GetSemanticPresenters(descriptions, LabelClass).Length.ShouldBe(3);
            GetSemanticPresenters(descriptions, ContentClass).Length.ShouldBe(3);
            GetSemanticControls(descriptions).ShouldAllBe(static control =>
                control.Classes.Contains(LabelClass) || control.Classes.Contains(ContentClass) ||
                control.Classes.Contains(HeaderClass) || control.Classes.Contains(TitleClass) ||
                control.Classes.Contains(ExtraClass) ||
                control.Classes.Contains("semantic-scope-items") ||
                control.Classes.Contains("semantic-scope-item"));
        });
    }

    [Fact]
    public void Empty_Items_Produce_No_Item_Scoped_Targets()
    {
        var descriptions = CreateDescriptions(Orientation.Horizontal, false, 0);

        ShowInWindow(descriptions, () =>
        {
            GetSemanticPresenters(descriptions, LabelClass).ShouldBeEmpty();
            GetSemanticPresenters(descriptions, ContentClass).ShouldBeEmpty();
            GetStaticOwnerMarkers(descriptions).Length.ShouldBe(4);
        });
    }

    [Fact]
    public void Collection_Changes_Preserve_Item_Target_Cardinality()
    {
        var descriptions = CreateDescriptions(Orientation.Horizontal, false, 1);

        ShowInWindow(descriptions, () =>
        {
            AssertItemTargetCount(descriptions, 1);

            descriptions.Items.Add(new DescriptionItem { Label = "Second", Content = "Second content" });
            Dispatcher.UIThread.RunJobs();
            AssertItemTargetCount(descriptions, 2);

            descriptions.Items.RemoveAt(0);
            Dispatcher.UIThread.RunJobs();
            AssertItemTargetCount(descriptions, 1);

            descriptions.Items.Clear();
            Dispatcher.UIThread.RunJobs();
            AssertItemTargetCount(descriptions, 0);

            descriptions.Items.AddRange([
                new DescriptionItem { Label = "Reset A", Content = "A" },
                new DescriptionItem { Label = "Reset B", Content = "B" }
            ]);
            Dispatcher.UIThread.RunJobs();
            AssertItemTargetCount(descriptions, 2);
        });
    }

    [Fact]
    public void Layout_And_Bordered_Rebuilds_Replace_Targets_Without_Changing_Cardinality()
    {
        var descriptions = CreateDescriptions(Orientation.Horizontal, false, 2);

        ShowInWindow(descriptions, () =>
        {
            var originalTargets = GetItemTargets(descriptions);

            descriptions.IsBordered = true;
            Dispatcher.UIThread.RunJobs();
            AssertRebuiltTargets(descriptions, originalTargets, 2);
            var horizontalBorderedTargets = GetItemTargets(descriptions);

            descriptions.Layout = Orientation.Vertical;
            Dispatcher.UIThread.RunJobs();
            AssertRebuiltTargets(descriptions, horizontalBorderedTargets, 2);
            var verticalBorderedTargets = GetItemTargets(descriptions);

            descriptions.IsBordered = false;
            Dispatcher.UIThread.RunJobs();
            AssertRebuiltTargets(descriptions, verticalBorderedTargets, 2);
        });
    }

    [Fact]
    public void Static_And_Item_Selectors_Match_Their_Declared_Owner_Routes()
    {
        var descriptions = CreateDescriptions(Orientation.Horizontal, true, 2);
        descriptions.Classes.Add("semantic-owner");
        descriptions.Styles.Add(new Style(selector =>
            selector.OfType<Desktop.Controls.Descriptions>()
                    .Class("semantic-owner")
                    .Template()
                    .Class(TitleClass))
        {
            Setters =
            {
                new Setter(Control.TagProperty, "title")
            }
        });
        descriptions.Styles.Add(new Style(selector =>
            selector.OfType<Desktop.Controls.Descriptions>()
                    .Class("semantic-owner")
                    .Template()
                    .Class("semantic-scope-items")
                    .Child()
                    .Class("semantic-scope-item")
                    .Template()
                    .Class(LabelClass))
        {
            Setters =
            {
                new Setter(Control.TagProperty, "label")
            }
        });
        descriptions.Styles.Add(new Style(selector =>
            selector.OfType<Desktop.Controls.Descriptions>()
                    .Class("semantic-owner")
                    .Template()
                    .Class("semantic-scope-items")
                    .Child()
                    .Class("semantic-scope-item")
                    .Template()
                    .Class(ContentClass))
        {
            Setters =
            {
                new Setter(Control.TagProperty, "content")
            }
        });

        ShowInWindow(descriptions, () =>
        {
            GetStaticOwnerMarkers(descriptions).Single(static control => control.Classes.Contains(TitleClass))
                                                .Tag.ShouldBe("title");
            GetSemanticPresenters(descriptions, LabelClass)
                .ShouldAllBe(static presenter => Equals(presenter.Tag, "label"));
            GetSemanticPresenters(descriptions, ContentClass)
                .ShouldAllBe(static presenter => Equals(presenter.Tag, "content"));
        });
    }

    [Fact]
    public void Root_Semantic_Part_Projects_The_Standard_Surface_Properties()
    {
        var descriptions = CreateDescriptions(Orientation.Horizontal, true, 3);
        descriptions.Padding = new Thickness(10);
        descriptions.Background = new SolidColorBrush(Colors.AliceBlue);
        descriptions.BorderBrush = new SolidColorBrush(Color.Parse("#CDC1FF"));
        descriptions.BorderThickness = new Thickness(1);
        descriptions.CornerRadius = new CornerRadius(8);

        ShowInWindow(descriptions, () =>
        {
            var rootFrame = descriptions.GetVisualDescendants()
                                        .OfType<PixelAlignedBorder>()
                                        .Single(static border => border.Name == "RootFrame");

            rootFrame.Padding.ShouldBe(new Thickness(10));
            rootFrame.Background.ShouldBeSameAs(descriptions.Background);
            rootFrame.BorderBrush.ShouldBeSameAs(descriptions.BorderBrush);
            rootFrame.BorderThickness.ShouldBe(new Thickness(1));
            rootFrame.CornerRadius.ShouldBe(new CornerRadius(8));
        });
    }

    [Fact]
    public void Runtime_Item_Selector_Route_Excludes_A_Nested_Button_Owner()
    {
        var descriptions = CreateDescriptions(Orientation.Horizontal, true, 1);
        descriptions.Extra = new Desktop.Controls.Button { Content = "Edit" };
        descriptions.Classes.Add("semantic-owner");
        descriptions.Styles.Add(new Style(selector =>
            selector.OfType<Desktop.Controls.Descriptions>()
                    .Class("semantic-owner")
                    .Template()
                    .Class("semantic-scope-items")
                    .Child()
                    .Class("semantic-scope-item")
                    .Template()
                    .Class(ContentClass))
        {
            Setters =
            {
                new Setter(Control.TagProperty, "descriptions-content")
            }
        });

        ShowInWindow(descriptions, () =>
        {
            GetSemanticPresenters(descriptions, ContentClass)
                .Single(static presenter => presenter.TemplatedParent is DescriptionBorderedItemContent)
                .Tag.ShouldBe("descriptions-content");
            descriptions.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Single(static presenter =>
                    presenter.Classes.Contains(ContentClass) &&
                    presenter.TemplatedParent is Desktop.Controls.Button)
                .Tag.ShouldBeNull();
        });
    }

    [Fact]
    public void Runtime_Item_Selector_Route_Excludes_A_Nested_Descriptions_Owner()
    {
        var nested = CreateDescriptions(Orientation.Horizontal, true, 1);
        var descriptions = CreateDescriptions(Orientation.Horizontal, true, 1);
        descriptions.Items[0].Content = nested;
        descriptions.Classes.Add("semantic-owner");
        descriptions.Styles.Add(new Style(selector =>
            selector.OfType<Desktop.Controls.Descriptions>()
                    .Class("semantic-owner")
                    .Template()
                    .Class("semantic-scope-items")
                    .Child()
                    .Class("semantic-scope-item")
                    .Template()
                    .Class(ContentClass))
        {
            Setters =
            {
                new Setter(Control.TagProperty, "outer-content")
            }
        });

        ShowInWindow(descriptions, () =>
        {
            GetSemanticPresenters(descriptions, ContentClass)
                .Single(static presenter => presenter.TemplatedParent is DescriptionBorderedItemContent)
                .Tag.ShouldBe("outer-content");
            GetSemanticPresenters(nested, ContentClass)
                .ShouldAllBe(static presenter => presenter.Tag == null);
        });
    }

    [Theory]
    [InlineData(AtomUISizeType.Large)]
    [InlineData(AtomUISizeType.Middle)]
    [InlineData(AtomUISizeType.Small)]
    public void Vertical_Bordered_Items_Receive_The_Owner_SizeType_Baseline(AtomUISizeType sizeType)
    {
        var descriptions = CreateDescriptions(Orientation.Vertical, true, 1);
        descriptions.SizeType = sizeType;

        ShowInWindow(descriptions, () =>
        {
            var item = descriptions.GetVisualDescendants().OfType<DescriptionDefaultItem>().Single();
            item.SizeType.ShouldBe(sizeType);

            var label = GetSemanticPresenters(descriptions, LabelClass).Single();
            var content = GetSemanticPresenters(descriptions, ContentClass).Single();
            label.Padding.ShouldBe(item.Padding);
            content.Padding.ShouldBe(item.Padding);
            item.Padding.ShouldNotBe(new Thickness());
        });
    }

    [Theory]
    [InlineData(AtomUISizeType.Large)]
    [InlineData(AtomUISizeType.Middle)]
    [InlineData(AtomUISizeType.Small)]
    public void Horizontal_Bordered_Baseline_Is_Projected_To_The_Semantic_Presenters(AtomUISizeType sizeType)
    {
        var descriptions = CreateDescriptions(Orientation.Horizontal, true, 1);
        descriptions.SizeType = sizeType;

        ShowInWindow(descriptions, () =>
        {
            var labelCell = descriptions.GetVisualDescendants().OfType<DescriptionBorderedItemLabel>().Single();
            var contentCell = descriptions.GetVisualDescendants().OfType<DescriptionBorderedItemContent>().Single();
            var label = GetSemanticPresenters(descriptions, LabelClass).Single();
            var content = GetSemanticPresenters(descriptions, ContentClass).Single();

            label.Padding.ShouldBe(labelCell.Padding);
            content.Padding.ShouldBe(contentCell.Padding);
            label.Background.ShouldBe(labelCell.Background);
            label.Foreground.ShouldBe(labelCell.Foreground);
            content.Foreground.ShouldBe(contentCell.Foreground);
            labelCell.Padding.ShouldNotBe(new Thickness());
            contentCell.Padding.ShouldNotBe(new Thickness());

            var labelFrame = labelCell.GetVisualChildren().OfType<PixelAlignedBorder>().Single();
            var contentFrame = contentCell.GetVisualChildren().OfType<PixelAlignedBorder>().Single();
            labelFrame.Padding.ShouldBe(new Thickness());
            contentFrame.Padding.ShouldBe(new Thickness());
            labelFrame.Background.ShouldBeNull();
        });
    }

    [Fact]
    public void Horizontal_Bordered_Semantic_Setters_Override_The_Presenter_Baseline()
    {
        var semanticPadding = new Thickness(13, 7);
        var semanticLabelBackground = Brushes.Gold;
        var semanticLabelForeground = Brushes.DarkRed;
        var semanticContentForeground = Brushes.DarkGreen;
        var descriptions = CreateDescriptions(Orientation.Horizontal, true, 1);
        descriptions.Styles.Add(new Style(selector =>
            selector.OfType<Desktop.Controls.Descriptions>().Descendant().Class(LabelClass))
        {
            Setters =
            {
                new Setter(Decorator.PaddingProperty, semanticPadding),
                new Setter(TemplatedControl.BackgroundProperty, semanticLabelBackground),
                new Setter(TemplatedControl.ForegroundProperty, semanticLabelForeground)
            }
        });
        descriptions.Styles.Add(new Style(selector =>
            selector.OfType<Desktop.Controls.Descriptions>().Descendant().Class(ContentClass))
        {
            Setters =
            {
                new Setter(Decorator.PaddingProperty, semanticPadding),
                new Setter(TemplatedControl.ForegroundProperty, semanticContentForeground)
            }
        });

        ShowInWindow(descriptions, () =>
        {
            var label = GetSemanticPresenters(descriptions, LabelClass).Single();
            var content = GetSemanticPresenters(descriptions, ContentClass).Single();

            label.Padding.ShouldBe(semanticPadding);
            label.Background.ShouldBeSameAs(semanticLabelBackground);
            label.Foreground.ShouldBeSameAs(semanticLabelForeground);
            content.Padding.ShouldBe(semanticPadding);
            content.Foreground.ShouldBeSameAs(semanticContentForeground);
        });
    }

    private static Desktop.Controls.Descriptions CreateDescriptions(
        Orientation layout,
        bool isBordered,
        int itemCount)
    {
        var descriptions = new Desktop.Controls.Descriptions
        {
            Header = "Title",
            Extra = "Extra",
            Layout = layout,
            IsBordered = isBordered,
            ColumnInfo = 1
        };
        for (var i = 0; i < itemCount; i++)
        {
            descriptions.Items.Add(new DescriptionItem
            {
                Label = $"Label {i}",
                Content = $"Content {i}"
            });
        }

        return descriptions;
    }

    private static Control[] GetSemanticControls(Desktop.Controls.Descriptions descriptions)
    {
        return descriptions.GetVisualDescendants()
                           .OfType<Control>()
                           .Where(static control => control.Classes.Any(
                               className => className.StartsWith("semantic-", StringComparison.Ordinal)))
                           .ToArray();
    }

    private static Control[] GetStaticOwnerMarkers(Desktop.Controls.Descriptions descriptions)
    {
        return GetSemanticControls(descriptions)
            .Where(control => ReferenceEquals(control.TemplatedParent, descriptions))
            .ToArray();
    }

    private static ContentPresenter[] GetSemanticPresenters(
        Desktop.Controls.Descriptions descriptions,
        string marker)
    {
        var itemsScope = descriptions.GetVisualDescendants()
                                     .OfType<Avalonia.Controls.Grid>()
                                     .Single(control =>
                                         ReferenceEquals(control.TemplatedParent, descriptions) &&
                                         control.Classes.Contains("semantic-scope-items"));
        return itemsScope.GetVisualChildren()
                         .OfType<Control>()
                         .Where(static control => control.Classes.Contains("semantic-scope-item"))
                         .SelectMany(control => control.GetVisualDescendants()
                                                   .OfType<ContentPresenter>()
                                                   .Where(presenter =>
                                                       ReferenceEquals(presenter.TemplatedParent, control) &&
                                                       presenter.Classes.Contains(marker)))
                         .ToArray();
    }

    private static ContentPresenter[] GetItemTargets(Desktop.Controls.Descriptions descriptions)
    {
        return GetSemanticPresenters(descriptions, LabelClass)
            .Concat(GetSemanticPresenters(descriptions, ContentClass))
            .ToArray();
    }

    private static void AssertItemTargetCount(Desktop.Controls.Descriptions descriptions, int itemCount)
    {
        GetSemanticPresenters(descriptions, LabelClass).Length.ShouldBe(itemCount);
        GetSemanticPresenters(descriptions, ContentClass).Length.ShouldBe(itemCount);
    }

    private static void AssertRebuiltTargets(
        Desktop.Controls.Descriptions descriptions,
        IReadOnlyCollection<ContentPresenter> oldTargets,
        int itemCount)
    {
        AssertItemTargetCount(descriptions, itemCount);
        var currentTargets = GetItemTargets(descriptions);
        currentTargets.ShouldAllBe(current => oldTargets.All(old => !ReferenceEquals(old, current)));
        foreach (var oldTarget in oldTargets)
        {
            oldTarget.GetVisualAncestors().ShouldNotContain(descriptions);
        }
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

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(Desktop.Controls.Descriptions));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
    }

    private static void AssertPart(
        SemanticPartDescriptor part,
        string selectorClass,
        Type contractType,
        SemanticPartCardinality cardinality,
        bool runtimeCreated,
        string selectorRoute)
    {
        part.Path.ShouldBe(part.Name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBe(runtimeCreated);
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
