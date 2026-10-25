using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Steps;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUISteps = AtomUI.Desktop.Controls.Steps;
using AtomUIStepsItem = AtomUI.Desktop.Controls.StepsItem;

namespace AtomUIGallery.Tests.ShowCases;

public class StepsShowCasePageTests
{
    static StepsShowCasePageTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Steps_ShowCase_Uses_Redesigned_Control_Contract()
    {
        var pageSource       = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Steps/Views/StepsShowCase.axaml");
        var codeBehindSource = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Steps/Views/StepsShowCase.axaml.cs");
        var viewModelSource  = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Steps/ViewModels/StepsViewModel.cs");
        var combinedSource   = string.Join('\n', pageSource, codeBehindSource, viewModelSource);

        pageSource.ShouldContain("BaseClass=\"ItemsControl\"");
        pageSource.ShouldContain("Current=\"{Binding Current}\"");
        pageSource.ShouldContain("CurrentChangeRequested=\"HandleCurrentChangeRequested\"");
        pageSource.ShouldContain("Text=\"{Binding InteractivePageContent}\"");
        pageSource.ShouldContain("Type=\"Dot\"");
        pageSource.ShouldContain("Type=\"OutlineDot\"");
        pageSource.ShouldContain("Type=\"Navigation\"");
        pageSource.ShouldContain("Type=\"Inline\"");
        pageSource.ShouldContain("TitlePlacement=\"Vertical\"");
        pageSource.ShouldContain("Percent=\"60\"");
        pageSource.ShouldContain("Offset=\"2\"");
        pageSource.ShouldContain("ItemHeaderForeground=\"{atom:SharedTokenResource ColorPrimaryText}\"");
        pageSource.ShouldContain("ItemSubHeaderForeground=\"{atom:SharedTokenResource ColorPrimaryTextActive}\"");
        pageSource.ShouldContain("ItemRailBackground=\"{atom:SharedTokenResource ColorTextDisabled}\"");
        pageSource.ShouldNotContain("inline-primary");
        pageSource.ShouldNotContain("/template/");
        CountOccurrences(pageSource, "SourceKey=\"").ShouldBe(17);

        codeBehindSource.ShouldContain("HandleCurrentChangeRequested");
        codeBehindSource.ShouldContain("viewModel.Current = args.Current");
        codeBehindSource.ShouldNotContain("CurrentContentProperty");
        codeBehindSource.ShouldNotContain("CurrentContentTemplateProperty");
        codeBehindSource.ShouldNotContain("FindDescendantByName");
        codeBehindSource.ShouldNotContain("HandleInteractiveStepsLoaded");

        viewModelSource.ShouldContain("InteractivePageContent");

        foreach (var removedApi in new[]
                 {
                     "CurrentStep", "InitialStep", "CurrentStepStatus", "ProgressValue",
                     "IsShowItemProgress", "ItemIndicatorType", "StepsStyle", "LabelPlacement",
                     "CurrentContent", "StepsItem.Description", "SelectingItemsControl"
                 })
        {
            combinedSource.ShouldNotContain(removedApi);
        }

        Regex.IsMatch(pageSource, @"<atom:StepsItem\b[^>]*\bDescription=").ShouldBeFalse();
        pageSource.ShouldNotContain("Style=\"Navigation\"");
        pageSource.ShouldNotContain("Style=\"Inline\"");
    }

    [Fact]
    public void Steps_ShowCase_Interactive_Content_Is_Not_Empty_For_Each_Controlled_Step()
    {
        AvaloniaTestApp.EnsureInitialized();
        var viewModel = new StepsViewModel(null!);

        viewModel.Current = 0;
        viewModel.InteractivePageContent.ShouldBe("First-content");

        viewModel.Current = 1;
        viewModel.InteractivePageContent.ShouldBe("Second-content");

        viewModel.Current = 2;
        viewModel.InteractivePageContent.ShouldBe("Last-content");
    }

    [Fact]
    public void Steps_Semantic_Previews_Are_Materialized_On_Tab_Selection()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new StepsShowCase
        {
            DataContext = new StepsViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var previews = page.GetVisualDescendants().OfType<SemanticPartPreview>().ToArray();
            previews.Length.ShouldBe(1);
            var preview = previews.Single(static candidate => candidate.Name == "StepsSemanticPreview");
            var semanticSteps = preview.SemanticOwner.ShouldBeOfType<AtomUISteps>();
            semanticSteps.Name.ShouldBe("StepsSemanticOwner");
            semanticSteps.HorizontalAlignment.ShouldBe(HorizontalAlignment.Center);
            semanticSteps.VerticalAlignment.ShouldBe(VerticalAlignment.Center);
            semanticSteps.TitlePlacement.ShouldBe(Orientation.Vertical);
            semanticSteps.GetVisualDescendants()
                         .Count(static item => item.Classes.Contains("semantic-item") && item.IsVisible)
                         .ShouldBe(3);
            semanticSteps.GetVisualDescendants()
                         .Count(static item => item.Classes.Contains("semantic-item-icon") && item.IsVisible)
                         .ShouldBe(3);
            semanticSteps.GetVisualDescendants()
                         .Count(static item => item.Classes.Contains("semantic-item-title") && item.IsVisible)
                         .ShouldBe(3);
            semanticSteps.GetVisualDescendants()
                         .Count(static item => item.Classes.Contains("semantic-item-subtitle") && item.IsVisible)
                         .ShouldBe(3);
            semanticSteps.GetVisualDescendants()
                         .Count(static item => item.Classes.Contains("semantic-item-section") && item.IsVisible)
                         .ShouldBe(3);
            semanticSteps.GetVisualDescendants()
                         .Count(static item => item.Classes.Contains("semantic-item-content") && item.IsVisible)
                         .ShouldBe(3);

            var semanticItems = semanticSteps.Items.OfType<AtomUIStepsItem>().ToArray();
            semanticItems.Length.ShouldBe(3);
            semanticItems.Select(static item => item.Header)
                         .ShouldBe(new object?[] { "Step 1", "Step 2", "Step 3" });
            semanticItems.Select(static item => item.SubHeader)
                         .ShouldBe(new object?[] { "00:00", "00:01", "00:02" });
            semanticItems.Select(static item => item.Content)
                         .ShouldBe(new object?[]
                         {
                             "This is a content.",
                             "This is a content.",
                             "This is a content."
                         });

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void Steps_Semantic_Preview_Shows_Panel_Demo_Below_The_Default_Steps()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new StepsShowCase
        {
            DataContext = new StepsViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "StepsSemanticPreview");
            var owner = preview.SemanticOwner.ShouldBeOfType<AtomUISteps>();
            var panelSteps = preview.GetVisualDescendants()
                                    .OfType<AtomUISteps>()
                                    .Single(static steps => steps.Type == StepsType.Panel);

            panelSteps.Current.ShouldBe(1);
            panelSteps.Bounds.Top.ShouldBeGreaterThanOrEqualTo(owner.Bounds.Bottom);

            var panelItems = panelSteps.Items.OfType<AtomUIStepsItem>().ToArray();
            panelItems.Length.ShouldBe(3);
            panelItems.Select(static item => item.Header)
                      .ShouldBe(new object?[] { "Step 1", "Step 2", "Step 3" });
            panelItems.Select(static item => item.SubHeader)
                      .ShouldBe(new object?[] { "00:00", "00:01", "00:02" });
            panelItems.Select(static item => item.Content)
                      .ShouldBe(new object?[]
                      {
                          "This is a content.",
                          "This is a content.",
                          "This is a content."
                      });
        });
    }

    [Fact]
    public void Steps_Semantic_Parts_Pane_Fills_The_Remaining_Height_And_Scrolls_Internally()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new StepsShowCase
        {
            DataContext = new StepsViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            // The filling contract applies when the card list overflows the available height.
            var semanticPreview = page.GetVisualDescendants().OfType<SemanticPartPreview>().Single();
            var descriptions = semanticPreview.PartDescriptions.ToArray();
            semanticPreview.PartDescriptions.Clear();
            foreach (var description in descriptions)
            {
                semanticPreview.PartDescriptions.Add(new SemanticPartDescription
                {
                    Path = description.Path,
                    OwnerType = description.OwnerType,
                    Description = description.Description + "\nFirst detail line.\nSecond detail line.\nThird detail line."
                });
            }
            Relayout();

            var preview = page.GetVisualDescendants().OfType<SemanticPartPreview>().Single();
            var pageScrollViewer = page.GetVisualDescendants()
                                       .OfType<Avalonia.Controls.ScrollViewer>()
                                       .Single(static viewer => viewer.Name == "PART_ScrollViewer");
            var inspectionPanel = preview.GetVisualDescendants()
                                         .OfType<Border>()
                                         .Single(static border => border.Name == "PART_InspectionPanel");
            var inspectionBottom = inspectionPanel
                .TranslatePoint(
                    new Point(0, inspectionPanel.Bounds.Height + inspectionPanel.Margin.Bottom),
                    pageScrollViewer)!
                .Value.Y;

            inspectionBottom.ShouldBe(pageScrollViewer.Viewport.Height, 2);
        });

        var smallerPage = new StepsShowCase
        {
            DataContext = new StepsViewModel(new TestScreen())
        };

        ShowInWindow(smallerPage, 1280, 700, () =>
        {
            var host = smallerPage.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            Relayout();

            var preview = smallerPage.GetVisualDescendants().OfType<SemanticPartPreview>().Single();
            var paneScrollViewer = preview.GetVisualDescendants()
                                          .OfType<Border>()
                                          .Single(static border => border.Name == "PART_PartsPane")
                                          .GetVisualDescendants()
                                          .OfType<Avalonia.Controls.ScrollViewer>()
                                          .Single();

            paneScrollViewer.Viewport.Height.ShouldBeLessThan(400);
            paneScrollViewer.Extent.Height.ShouldBeGreaterThan(paneScrollViewer.Viewport.Height);

            paneScrollViewer.ScrollToEnd();
            Dispatcher.UIThread.RunJobs();

            paneScrollViewer.Offset.Y.ShouldBeGreaterThan(0);
            var lastCard = preview.GetVisualDescendants()
                                  .OfType<UserControl>()
                                  .Single(candidate =>
                                      (string?)candidate.DataContext
                                                        ?.GetType()
                                                        .GetProperty("Path")!
                                                        .GetValue(candidate.DataContext) == "itemRail");
            var lastCardTop = lastCard.TranslatePoint(default, paneScrollViewer)!.Value.Y;
            lastCardTop.ShouldBeGreaterThanOrEqualTo(-1);
            lastCardTop.ShouldBeLessThan(paneScrollViewer.Viewport.Height);
        });
    }

    [Fact]
    public void Steps_ShowCase_Declares_The_Semantic_Preview_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Navigation/Steps/Views/StepsShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Navigation/Steps/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItemBySourceKey(source, "steps-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"StepsSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #StepsSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Steps}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(9);
        foreach (var path in new[]
                 {
                     "root", "item", "itemWrapper", "itemIcon",
                     "itemTitle", "itemSubtitle", "itemSection", "itemContent", "itemRail"
                 })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        source.ShouldContain(
            "Header=\"{gallery:StepsShowCaseLangResource P2HeaderStepN1}\" SubHeader=\"{gallery:StepsShowCaseLangResource P2SubHeaderTimeN1}\" Content=\"{gallery:StepsShowCaseLangResource P2ContentThisIsAContent}\"");
        source.ShouldContain(
            "Header=\"{gallery:StepsShowCaseLangResource P2HeaderStepN2}\" SubHeader=\"{gallery:StepsShowCaseLangResource P2SubHeaderTimeN2}\" Content=\"{gallery:StepsShowCaseLangResource P2ContentThisIsAContent}\"");
        source.ShouldContain(
            "Header=\"{gallery:StepsShowCaseLangResource P2HeaderStepN3}\" SubHeader=\"{gallery:StepsShowCaseLangResource P2SubHeaderTimeN3}\" Content=\"{gallery:StepsShowCaseLangResource P2ContentThisIsAContent}\"");
        source.ShouldContain("Current=\"1\" Type=\"Panel\"");
        english.ShouldContain("<unit id=\"P2SubHeaderTimeN1\">");
        english.ShouldContain("<unit id=\"P2SubHeaderTimeN2\">");
        english.ShouldContain("<unit id=\"P2SubHeaderTimeN3\">");
        english.ShouldContain("<source>00:00</source>");
        english.ShouldContain("<source>00:01</source>");
        english.ShouldContain("<source>00:02</source>");

        semanticSource.ShouldContain("SourceKey=\"steps-semantic-part\"");
        semanticSource.ShouldContain("StepsShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("StepsShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|Steps.semantic-style-demo-root\"");
        semanticSource.ShouldContain("Selector=\"atom|Steps.semantic-style-demo-object\"");
        semanticSource.ShouldContain("Selector=\"atom|Steps[Type=Navigation].semantic-style-demo-root\"");
        semanticSource.ShouldContain("Property=\"BorderBrush\"");
        semanticSource.ShouldContain("Property=\"BorderThickness\"");
        semanticSource.ShouldContain("Property=\"BorderDashArray\"");
        semanticSource.ShouldContain("Value=\"4,2\"");
        semanticSource.ShouldContain("Property=\"CornerRadius\"");
        semanticSource.ShouldContain("Property=\"Padding\"");
        semanticSource.ShouldContain("Property=\"FontStyle\"");
        semanticSource.ShouldContain("Value=\"Italic\"");
        semanticSource.ShouldContain("Value=\"10\"");
        semanticSource.ShouldContain("Value=\"#1890FF\"");
        CountOccurrences(semanticSource, "<atom:StepsItemIconStyle ").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:StepsItemContentStyle ").ShouldBe(1);
        CountOccurrences(semanticSource, "x:SetterTargetType=\"TemplatedControl\"").ShouldBe(1);
        CountOccurrences(semanticSource, "x:SetterTargetType=\"ContentPresenter\"").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:Steps ").ShouldBe(2);
        CountOccurrences(semanticSource, "Type=\"Navigation\"").ShouldBe(1);
        CountOccurrences(semanticSource, "Current=\"1\"").ShouldBe(2);
        CountOccurrences(semanticSource, "P2ContentThisIsAContent").ShouldBe(6);
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain("Style the root and item parts through generated Semantic Part styles");
        english.ShouldNotContain("Mirrors the Ant Design style-class demo");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
    }

    [Fact]
    public void Steps_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new StepsShowCase
        {
            DataContext = new StepsViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "steps-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demoRows = page.GetVisualDescendants()
                               .OfType<AtomUISteps>()
                               .Where(static steps => steps.Classes.Contains("semantic-style-demo-root"))
                               .ToArray();
            demoRows.Length.ShouldBe(2);

            foreach (var row in demoRows)
            {
                row.BorderThickness.ShouldBe(new Thickness(2));
                row.BorderDashArray.ShouldBe(new[] { 4d, 2d });
                row.CornerRadius.TopLeft.ShouldBeGreaterThan(0);
                row.Padding.Top.ShouldBeGreaterThan(0);
            }

            var objectDemo = demoRows.Single(
                static steps => steps.Classes.Contains("semantic-style-demo-object"));
            AssertSolidColor(objectDemo.BorderBrush, "#D9D9D9");
            var objectItems = GetSemanticItemParts(objectDemo, "semantic-item");
            objectItems.Length.ShouldBe(3);

            var objectIcons = GetSemanticItemParts(objectDemo, "semantic-item-icon")
                .OfType<TemplatedControl>()
                .ToArray();
            objectIcons.Length.ShouldBe(3);
            foreach (var icon in objectIcons)
            {
                icon.CornerRadius.ShouldBe(new CornerRadius(10));
            }

            var objectContents = GetSemanticItemParts(objectDemo, "semantic-item-content")
                .OfType<ContentPresenter>()
                .ToArray();
            objectContents.Length.ShouldBe(3);
            foreach (var content in objectContents)
            {
                content.FontStyle.ShouldBe(FontStyle.Italic);
            }

            var functionDemo = demoRows.Single(
                static steps => !steps.Classes.Contains("semantic-style-demo-object"));
            functionDemo.Type.ShouldBe(StepsType.Navigation);
            AssertSolidColor(functionDemo.BorderBrush, "#1890FF");
            functionDemo.BorderThickness.ShouldBe(new Thickness(2));
            functionDemo.BorderDashArray.ShouldBe(new[] { 4d, 2d });

            var functionIcons = GetSemanticItemParts(functionDemo, "semantic-item-icon")
                .OfType<TemplatedControl>()
                .ToArray();
            functionIcons.Length.ShouldBe(3);
            foreach (var icon in functionIcons)
            {
                icon.CornerRadius.ShouldNotBe(new CornerRadius(10));
            }

            var functionContents = GetSemanticItemParts(functionDemo, "semantic-item-content")
                .OfType<ContentPresenter>()
                .ToArray();
            functionContents.Length.ShouldBe(3);
            foreach (var content in functionContents)
            {
                content.FontStyle.ShouldBe(FontStyle.Normal);
            }
        });
    }

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        actual.ShouldNotBeNull()
              .ShouldBeAssignableTo<ISolidColorBrush>()
              .Color.ShouldBe(Color.Parse(expected));
    }

    private static Control[] GetSemanticItemParts(AtomUISteps steps, string semanticClass)
    {
        return steps.GetVisualDescendants()
                    .OfType<Control>()
                    .Where(control => control.Classes.Contains(semanticClass))
                    .ToArray();
    }

    private static string ExtractShowCaseItemBySourceKey(string source, string sourceKey)
    {
        var sourceKeyIndex = source.IndexOf($"SourceKey=\"{sourceKey}\"", StringComparison.Ordinal);
        sourceKeyIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", sourceKeyIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string itemEndMarker = "</gallery:ShowCaseItem>";
        var itemEnd = source.IndexOf(itemEndMarker, sourceKeyIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(itemStart);

        return source[itemStart..(itemEnd + itemEndMarker.Length)];
    }

    private static void Relayout()
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        Dispatcher.UIThread.RunJobs();
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

    private static string ExtractStepsExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(StripStepsBehaviorMarkup(source));
    }

    private static string StripStepsBehaviorMarkup(string source)
    {
        var normalized = Regex.Replace(
            source,
            @"\s+Click=""Handle(Next|Previous)ButtonClick""",
            string.Empty,
            RegexOptions.CultureInvariant);

        return Regex.Replace(
            normalized,
            @"\s+Content=""\{Binding NextButtonText\}""",
            " Content=\"{gallery:StepsShowCaseLangResource P2ContentNext}\"",
            RegexOptions.CultureInvariant);
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
}
