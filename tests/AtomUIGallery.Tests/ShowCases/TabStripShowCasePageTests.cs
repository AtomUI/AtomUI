using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.TabStrip;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUICardTabStrip = AtomUI.Desktop.Controls.CardTabStrip;
using AtomUITabStrip = AtomUI.Desktop.Controls.TabStrip;
using AtomUITabStripItem = AtomUI.Desktop.Controls.TabStripItem;

namespace AtomUIGallery.Tests.ShowCases;

public class TabStripShowCasePageTests
{

    [Fact]
    public void TabStrip_Semantic_Previews_Are_Materialized_On_Tab_Selection()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new TabStripShowCase
        {
            DataContext = new TabStripViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().Count().ShouldBe(3);

            var tabStripPreview = page.GetVisualDescendants()
                                      .OfType<SemanticPartPreview>()
                                      .Single(static candidate => candidate.Name == "TabStripSemanticPreview");
            var semanticTabStrip = tabStripPreview.SemanticOwner.ShouldBeOfType<AtomUITabStrip>();
            semanticTabStrip.Name.ShouldBe("TabStripSemanticOwner");
            semanticTabStrip.GetVisualDescendants()
                            .Count(static item => item is AtomUITabStripItem && item.Classes.Contains("semantic-item"))
                            .ShouldBe(3);

            var cardPreview = page.GetVisualDescendants()
                                  .OfType<SemanticPartPreview>()
                                  .Single(static candidate => candidate.Name == "CardTabStripSemanticPreview");
            var semanticCardTabStrip = cardPreview.SemanticOwner.ShouldBeOfType<AtomUICardTabStrip>();
            semanticCardTabStrip.Name.ShouldBe("CardTabStripSemanticOwner");
            semanticCardTabStrip.GetVisualDescendants()
                                .Count(static item => item is AtomUITabStripItem && item.Classes.Contains("semantic-item"))
                                .ShouldBe(3);
            semanticCardTabStrip.GetVisualDescendants()
                                .Count(static item => item.Classes.Contains("semantic-add"))
                                .ShouldBe(1);

            var itemPreview = page.GetVisualDescendants()
                                  .OfType<SemanticPartPreview>()
                                  .Single(static candidate => candidate.Name == "TabStripItemSemanticPreview");
            var semanticTabStripItem = itemPreview.SemanticOwner.ShouldBeOfType<AtomUITabStripItem>();
            semanticTabStripItem.Name.ShouldBe("TabStripItemSemanticOwner");
            semanticTabStripItem.GetVisualDescendants()
                                .Count(static item => item.Classes.Contains("semantic-icon"))
                                .ShouldBe(1);
            semanticTabStripItem.GetVisualDescendants()
                                .Count(static item => item.Classes.Contains("semantic-label"))
                                .ShouldBe(1);
            semanticTabStripItem.GetVisualDescendants()
                                .Count(static item => item.Classes.Contains("semantic-close"))
                                .ShouldBe(1);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void TabStrip_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Navigation/TabStrip/Views/TabStripShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Navigation/TabStrip/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItemBySourceKey(source, "tabstrip-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"TabStripSemanticPreview\"");
        source.ShouldContain("Name=\"CardTabStripSemanticPreview\"");
        source.ShouldContain("Name=\"TabStripItemSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #TabStripSemanticOwner}\"");
        source.ShouldContain("SemanticOwner=\"{Binding #CardTabStripSemanticOwner}\"");
        source.ShouldContain("SemanticOwner=\"{Binding #TabStripItemSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:TabStrip}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:CardTabStrip}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:TabStripItem}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(9);
        foreach (var path in new[] { "root", "item", "add", "close", "icon", "label" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"tabstrip-semantic-part\"");
        semanticSource.ShouldContain("TabStripShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("TabStripShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|TabStrip.semantic-style-demo\"");
        semanticSource.ShouldContain("Selector=\"atom|TabStripItem.semantic-style-demo\"");
        semanticSource.ShouldContain("Selector=\"atom|CardTabStrip.semantic-style-demo\"");
        semanticSource.ShouldContain("Selector=\"^:selected\"");
        semanticSource.ShouldContain("Property=\"Margin\"");
        semanticSource.ShouldContain("Value=\"0,0,4,0\"");
        semanticSource.ShouldContain("Property=\"Background\"");
        semanticSource.ShouldContain("Value=\"#1A1677FF\"");
        semanticSource.ShouldContain("Property=\"FontWeight\"");
        semanticSource.ShouldContain("Value=\"SemiBold\"");
        semanticSource.ShouldContain("Property=\"CornerRadius\"");
        semanticSource.ShouldContain("Value=\"4\"");
        CountOccurrences(semanticSource, "<atom:TabStripItemStyle ").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:TabStripItemIconStyle ").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:TabStripItemLabelStyle ").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:CardTabStripAddStyle ").ShouldBe(1);
        CountOccurrences(semanticSource, "x:SetterTargetType=\"atom:TabStripItem\"").ShouldBe(1);
        CountOccurrences(semanticSource, "x:SetterTargetType=\"ContentPresenter\"").ShouldBe(1);
        CountOccurrences(semanticSource, "x:SetterTargetType=\"atom:IconPresenter\"").ShouldBe(1);
        CountOccurrences(semanticSource, "x:SetterTargetType=\"atom:IconButton\"").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:TabStrip ").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:CardTabStrip ").ShouldBe(1);
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
    }

    [Fact]
    public void TabStrip_Semantic_Style_Example_Applies_The_Declared_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new TabStripShowCase
        {
            DataContext = new TabStripViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "tabstrip-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demoControls = page.GetVisualDescendants()
                                   .Where(static control =>
                                       (control is AtomUITabStrip or AtomUICardTabStrip) &&
                                       control.Classes.Contains("semantic-style-demo"))
                                   .ToArray();
            demoControls.Length.ShouldBe(2);

            var lineDemo = demoControls.OfType<AtomUITabStrip>().Single();
            var cardDemo = demoControls.OfType<AtomUICardTabStrip>().Single();
            lineDemo.GetVisualParent().ShouldBeSameAs(cardDemo.GetVisualParent());

            var lineItems = lineDemo.GetVisualDescendants()
                                    .OfType<AtomUITabStripItem>()
                                    .Where(static item => item.Classes.Contains("semantic-item"))
                                    .ToArray();
            lineItems.Length.ShouldBe(3);
            var lineSelected = lineItems.Single(static item => item is ISelectable { IsSelected: true });
            AssertSolidColor(lineSelected.Background, "#1A1677FF");
            foreach (var lineItem in lineItems.Where(static item => item is not ISelectable { IsSelected: true }))
            {
                lineItem.Margin.ShouldBe(new Thickness(0, 0, 4, 0));
            }

            var cardItems = cardDemo.GetVisualDescendants()
                                    .OfType<AtomUITabStripItem>()
                                    .Where(static item => item.Classes.Contains("semantic-item"))
                                    .ToArray();
            cardItems.Length.ShouldBe(2);
            cardItems[0].GetVisualDescendants()
                        .Count(static candidate => candidate.Classes.Contains("semantic-icon"))
                        .ShouldBe(1);
            cardItems[0].GetVisualDescendants()
                        .OfType<ContentPresenter>()
                        .Single(static candidate => candidate.Classes.Contains("semantic-label"))
                        .FontWeight.ShouldBe(FontWeight.SemiBold);

            var addButton = cardDemo.GetVisualDescendants()
                                    .OfType<IconButton>()
                                    .Single(static candidate => candidate.Classes.Contains("semantic-add"));
            addButton.CornerRadius.ShouldBe(new CornerRadius(4));
        });
    }

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        actual.ShouldNotBeNull()
              .ShouldBeAssignableTo<ISolidColorBrush>()
              .Color.ShouldBe(Color.Parse(expected));
    }

    private static string ExtractTabStripExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string ExtractShowCaseItemBySourceKey(string source, string sourceKey)
    {
        var sourceKeyIndex = source.IndexOf($"SourceKey=\"{sourceKey}\"", StringComparison.Ordinal);
        sourceKeyIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", sourceKeyIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string itemEndMarker = "</gallery:ShowCaseItem>";
        var itemEnd = source.IndexOf(itemEndMarker, sourceKeyIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(sourceKeyIndex);

        return source[itemStart..(itemEnd + itemEndMarker.Length)];
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(StripTabStripBehaviorMarkup(source));
    }

    private static string StripTabStripBehaviorMarkup(string source)
    {
        var normalized = Regex.Replace(
            source,
            "\\s*OptionCheckedChanged=\"Handle(?:Card)?(?:ReorderPlacement|Placement|SizeType)OptionCheckedChanged\"",
            string.Empty,
            RegexOptions.CultureInvariant);

        normalized = Regex.Replace(
            normalized,
            "\\s*AddTabRequest=\"HandleAddTabRequest\"",
            string.Empty,
            RegexOptions.CultureInvariant);

        return normalized;
    }

    private static string ComputeSha256(string source)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadSnapshotHash(string source)
    {
        return source
            .Split('\n')
            .First(line => line.StartsWith("sha256:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim();
    }

    private static int ReadSnapshotCount(string source)
    {
        return int.Parse(source
            .Split('\n')
            .First(line => line.StartsWith("count:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim());
    }

    private static int CountShowCaseItemElements(string source)
    {
        return Regex.Matches(source, @"<gallery:ShowCaseItem(\s|>)", RegexOptions.CultureInvariant).Count;
    }

    private static int CountOccurrences(string source, string value)
    {
        var count      = 0;
        var startIndex = 0;
        while (true)
        {
            var matchIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return count;
            }

            count++;
            startIndex = matchIndex + value.Length;
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
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

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }

    private static void ShowInWindow(Control content, double width, double height, Action assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child = content
        };

        var window = new AvaloniaWindow
        {
            Content = visualLayerManager,
            Width = width,
            Height = height
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
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
