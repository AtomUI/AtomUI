using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Controls.Primitives;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.TabControl;
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
using AtomUICardTabControl = AtomUI.Desktop.Controls.CardTabControl;
using AtomUITabControl = AtomUI.Desktop.Controls.TabControl;
using AtomUITabItem = AtomUI.Desktop.Controls.TabItem;

namespace AtomUIGallery.Tests.ShowCases;

public class TabControlShowCasePageTests
{

    [Fact]
    public void TabControl_Semantic_Previews_Are_Materialized_On_Tab_Selection()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new TabControlShowCase
        {
            DataContext = new TabControlViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().Count().ShouldBe(3);

            var pageScroller = page.GetVisualDescendants()
                                   .OfType<AtomUI.Desktop.Controls.ScrollViewer>()
                                   .Single(static candidate => candidate.Name == "PART_ScrollViewer");
            pageScroller.Extent.Height.ShouldBeGreaterThan(pageScroller.Viewport.Height);
            foreach (var preview in page.GetVisualDescendants().OfType<SemanticPartPreview>())
            {
                var partsPane = preview.GetVisualDescendants()
                                       .OfType<Border>()
                                       .Single(static candidate => candidate.Name == "PART_PartsPane");
                var partsScroller = partsPane.GetVisualDescendants()
                                             .OfType<AtomUI.Desktop.Controls.ScrollViewer>()
                                             .Single();
                partsPane.ClipToBounds.ShouldBeTrue();
                partsScroller.Viewport.Height.ShouldBeLessThanOrEqualTo(400.5);
                var overflows = partsScroller.Extent.Height > partsScroller.Viewport.Height + 0.5;
                partsScroller.GetVisualDescendants()
                             .OfType<AtomUI.Desktop.Controls.ScrollBar>()
                             .Single(static bar => bar.Name == "PART_VerticalScrollBar")
                             .IsVisible.ShouldBe(overflows);
                if (overflows)
                {
                    partsScroller.Viewport.Height.ShouldBe(400, 1);
                }
                else
                {
                    partsScroller.Extent.Height.ShouldBe(partsScroller.Viewport.Height, 0.5);
                }
            }

            var tabControlPreview = page.GetVisualDescendants()
                                        .OfType<SemanticPartPreview>()
                                        .Single(static candidate => candidate.Name == "TabControlSemanticPreview");
            var semanticTabControl = tabControlPreview.SemanticOwner.ShouldBeOfType<AtomUITabControl>();
            semanticTabControl.Name.ShouldBe("TabControlSemanticOwner");
            semanticTabControl.GetVisualDescendants()
                              .Count(static item => item is AtomUITabItem && item.Classes.Contains("semantic-item"))
                              .ShouldBe(3);
            semanticTabControl.GetVisualDescendants()
                              .Count(static item => item is ContentPresenter && item.Classes.Contains("semantic-content"))
                              .ShouldBe(1);
            semanticTabControl.GetVisualDescendants()
                              .Count(static item => item.Classes.Contains("semantic-indicator"))
                              .ShouldBe(1);

            var cardPreview = page.GetVisualDescendants()
                                  .OfType<SemanticPartPreview>()
                                  .Single(static candidate => candidate.Name == "CardTabControlSemanticPreview");
            var semanticCardTabControl = cardPreview.SemanticOwner.ShouldBeOfType<AtomUICardTabControl>();
            semanticCardTabControl.Name.ShouldBe("CardTabControlSemanticOwner");
            semanticCardTabControl.GetVisualDescendants()
                                  .Count(static item => item is AtomUITabItem && item.Classes.Contains("semantic-item"))
                                  .ShouldBe(3);
            semanticCardTabControl.GetVisualDescendants()
                                  .Count(static item => item.Classes.Contains("semantic-add"))
                                  .ShouldBe(1);
            semanticCardTabControl.GetVisualDescendants()
                                  .Count(static item => item is ContentPresenter && item.Classes.Contains("semantic-content"))
                                  .ShouldBe(1);

            var itemPreview = page.GetVisualDescendants()
                                  .OfType<SemanticPartPreview>()
                                  .Single(static candidate => candidate.Name == "TabItemSemanticPreview");
            var semanticTabItem = itemPreview.SemanticOwner.ShouldBeOfType<AtomUITabItem>();
            semanticTabItem.Name.ShouldBe("TabItemSemanticOwner");
            semanticTabItem.GetVisualDescendants()
                           .Count(static item => item.Classes.Contains("semantic-icon"))
                           .ShouldBe(1);
            semanticTabItem.GetVisualDescendants()
                           .Count(static item => item.Classes.Contains("semantic-close"))
                           .ShouldBe(1);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void TabControl_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Navigation/TabControl/Views/TabControlShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Navigation/TabControl/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItemBySourceKey(source, "tabcontrol-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"TabControlSemanticPreview\"");
        source.ShouldContain("Name=\"CardTabControlSemanticPreview\"");
        source.ShouldContain("Name=\"TabItemSemanticPreview\"");
        source.ShouldContain("Title=\"TabItem\"");
        source.ShouldContain("SemanticOwner=\"{Binding #TabControlSemanticOwner}\"");
        source.ShouldContain("SemanticOwner=\"{Binding #CardTabControlSemanticOwner}\"");
        source.ShouldContain("SemanticOwner=\"{Binding #TabItemSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:TabControl}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:CardTabControl}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:TabItem}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(12);
        foreach (var path in new[] { "root", "header", "add", "content", "item", "indicator", "icon", "close" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }
        CountOccurrences(source, "Path=\"indicator\"").ShouldBe(1);

        semanticSource.ShouldContain("SourceKey=\"tabcontrol-semantic-part\"");
        semanticSource.ShouldContain("TabControlShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("TabControlShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|TabControl.semantic-style-demo\"");
        semanticSource.ShouldContain("Selector=\"atom|TabItem.semantic-style-demo\"");
        semanticSource.ShouldContain("Property=\"Padding\"");
        semanticSource.ShouldContain("Value=\"16\"");
        semanticSource.ShouldContain("Value=\"6,10\"");
        semanticSource.ShouldContain("Property=\"Background\"");
        semanticSource.ShouldContain("Value=\"#CCE6F7FF\"");
        semanticSource.ShouldContain("Property=\"FontWeight\"");
        semanticSource.ShouldContain("Value=\"Bold\"");
        semanticSource.ShouldContain("Property=\"Foreground\"");
        semanticSource.ShouldContain("Value=\"#1890FF\"");
        semanticSource.ShouldContain("TabStripPlacement=\"Left\"");
        semanticSource.ShouldContain("Initialized=\"HandleSemanticStyleDemoTabControlInitialized\"");
        semanticSource.ShouldContain("BorderThickness=\"2\"");
        semanticSource.ShouldNotContain("<atom:PixelAlignedBorder ");
        CountOccurrences(semanticSource, "BorderDashArray=\"4,2\"").ShouldBe(2);
        semanticSource.ShouldContain("BorderBrush=\"#E0000000\"");
        semanticSource.ShouldContain("Background=\"#CCFAFAFA\"");
        semanticSource.ShouldContain("BorderBrush=\"#D9D9D9\"");
        semanticSource.ShouldContain("Padding=\"16\"");
        CountOccurrences(semanticSource, "<atom:TabControlContentStyle ").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:TabControlItemStyle ").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:TabItemLabelStyle ").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:TabControlHeaderStyle ").ShouldBe(1);
        semanticSource.ShouldContain("Value=\"#80F5F5F5\"");
        CountOccurrences(semanticSource, "x:SetterTargetType=\"atom:TabItem\"").ShouldBe(1);
        CountOccurrences(semanticSource, "x:SetterTargetType=\"ContentPresenter\"").ShouldBe(2);
        CountOccurrences(semanticSource, "x:SetterTargetType=\"Border\"").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:TabControl ").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:CardTabControl ").ShouldBe(1);
        semanticSource.ShouldNotContain("Selector=\"atom|CardTabControl.semantic-style-demo\"");
        semanticSource.ShouldNotContain("Selector=\"^:selected\"");
        semanticSource.ShouldNotContain("<atom:TabItemIconStyle ");
        semanticSource.ShouldNotContain("<atom:CardTabControlAddStyle ");
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
    }

    [Fact]
    public void TabControl_Semantic_Style_Example_Applies_The_Declared_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new TabControlShowCase
        {
            DataContext = new TabControlViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "tabcontrol-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demoControls = page.GetVisualDescendants()
                                   .Where(static control =>
                                       (control is AtomUITabControl or AtomUICardTabControl) &&
                                       control.Classes.Contains("semantic-style-demo"))
                                   .ToArray();
            demoControls.Length.ShouldBe(2);

            var lineDemo = demoControls.OfType<AtomUITabControl>().Single();
            var cardDemo = demoControls.OfType<AtomUICardTabControl>().Single();

            var lineFrame = lineDemo.GetVisualChildren().OfType<PixelAlignedBorder>()
                                    .Single(static candidate => candidate.Name == "Frame");
            lineFrame.BorderThickness.ShouldBe(new Thickness(2));
            AssertSolidColor(lineFrame.BorderBrush, "#E0000000");
            lineFrame.StrokeDashArray.ShouldNotBeNull().Count.ShouldBe(2);
            lineFrame.StrokeDashArray[0].ShouldBe(4d);
            lineFrame.StrokeDashArray[1].ShouldBe(2d);
            lineFrame.Padding.ShouldBe(new Thickness(16));

            var lineHeader = lineDemo.GetVisualDescendants().OfType<Border>()
                                     .Single(static candidate => candidate.Classes.Contains("semantic-header"));
            AssertSolidColor(lineHeader.Background, "#80F5F5F5");

            var cardFrame = cardDemo.GetVisualChildren().OfType<PixelAlignedBorder>()
                                    .Single(static candidate => candidate.Name == "Frame");
            AssertSolidColor(cardFrame.Background, "#CCFAFAFA");
            AssertSolidColor(cardFrame.BorderBrush, "#D9D9D9");
            cardFrame.BorderThickness.ShouldBe(new Thickness(2));
            cardFrame.StrokeDashArray.ShouldNotBeNull().Count.ShouldBe(2);
            cardFrame.StrokeDashArray[0].ShouldBe(4d);
            cardFrame.StrokeDashArray[1].ShouldBe(2d);
            cardFrame.Padding.ShouldBe(new Thickness(16));
            cardDemo.TabStripPlacement.ShouldBe(Dock.Left);

            var contentPresenter = lineDemo.GetVisualDescendants()
                                           .OfType<ContentPresenter>()
                                           .Single(static candidate => candidate.Classes.Contains("semantic-content"));
            contentPresenter.Padding.ShouldBe(new Thickness(16));
            AssertSolidColor(contentPresenter.Background, "#CCE6F7FF");

            var lineItems = lineDemo.GetVisualDescendants()
                                    .OfType<AtomUITabItem>()
                                    .Where(static item => item.Classes.Contains("semantic-item"))
                                    .ToArray();
            lineItems.Length.ShouldBe(3);
            foreach (var lineItem in lineItems)
            {
                lineItem.Padding.ShouldBe(new Thickness(6, 10));
                lineItem.GetVisualDescendants()
                        .OfType<ContentPresenter>()
                        .Single(static candidate => candidate.Classes.Contains("semantic-label"))
                        .ShouldSatisfyAllConditions(
                            label =>
                            {
                                label.FontWeight.ShouldBe(FontWeight.Bold);
                                AssertSolidColor(label.Foreground, "#1890FF");
                            });
            }

            var indicator = lineDemo.GetVisualDescendants()
                                    .OfType<Border>()
                                    .Single(static candidate => candidate.Name == "PART_SelectedItemIndicator");
            AssertSolidColor(indicator.Background, "#4DFF4D4F");
            indicator.Height.ShouldBe(4d);

            var cardItems = cardDemo.GetVisualDescendants()
                                    .OfType<AtomUITabItem>()
                                    .Where(static item => item.Classes.Contains("semantic-item"))
                                    .ToArray();
            cardItems.Length.ShouldBe(3);
        });
    }

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        actual.ShouldNotBeNull()
              .ShouldBeAssignableTo<ISolidColorBrush>()
              .Color.ShouldBe(Color.Parse(expected));
    }

    private static string ExtractTabControlExampleItems(string source)
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
        return ShowCaseSnapshotMarkup.Normalize(StripTabControlBehaviorMarkup(source));
    }

    private static string StripTabControlBehaviorMarkup(string source)
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
