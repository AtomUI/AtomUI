using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Badge;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class BadgeShowCasePageTests
{

    [Fact]
    public void Badge_ShowCase_Declares_Three_Deferred_Semantic_Part_Previews()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataDisplay/Badge/Views/BadgeShowCase.axaml");
        var codeBehind = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Badge/Views/BadgeShowCase.axaml.cs");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        CountOccurrences(source, "<gallery:SemanticPartPreview\n").ShouldBe(3);
        source.ShouldContain("Name=\"CountBadgeSemanticPreview\"");
        source.ShouldContain("Name=\"DotBadgeSemanticPreview\"");
        source.ShouldContain("Name=\"RibbonBadgeSemanticPreview\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:CountBadge}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:DotBadge}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:RibbonBadge}\"");
        source.ShouldContain("Name=\"CountBadgeSemanticOwner\"");
        source.ShouldContain("Name=\"DotBadgeSemanticOwner\"");
        source.ShouldContain("Name=\"RibbonBadgeSemanticOwner\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(7);
        CountOccurrences(source, "Path=\"root\"").ShouldBe(3);
        CountOccurrences(source, "Path=\"indicator\"").ShouldBe(3);
        CountOccurrences(source, "Path=\"content\"").ShouldBe(1);
        source.ShouldContain("Loaded=\"HandleCrossRootSemanticPreviewLoaded\"");
        source.ShouldContain("Unloaded=\"HandleCrossRootSemanticPreviewUnloaded\"");

        codeBehind.ShouldContain("CountBadgeIndicatorStyle");
        codeBehind.ShouldContain("DotBadgeIndicatorStyle");
        codeBehind.ShouldContain("RibbonBadgeIndicatorStyle");
        codeBehind.ShouldContain("RibbonBadgeContentStyle");
        codeBehind.ShouldNotContain("/template/");
    }

    [Fact]
    public void Badge_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new BadgeShowCase
        {
            DataContext = new BadgeViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            GetSemanticDemoBadges(page).ShouldBeEmpty();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().Count().ShouldBe(3);
            GetSemanticDemoBadges(page).Count.ShouldBe(3);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));

            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            page.GetVisualDescendants().OfType<SemanticPartPreview>().Count().ShouldBe(3);
            GetSemanticDemoBadges(page).Count.ShouldBe(3);
        });
    }

    [Fact]
    public void Badge_Count_And_Dot_Previews_Register_Only_Their_Runtime_Adorner()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new BadgeShowCase
        {
            DataContext = new BadgeViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var previews = page.GetVisualDescendants().OfType<SemanticPartPreview>().ToArray();
            AssertPreviewOwnsRuntimeAdorner(
                previews.Single(static preview => preview.Name == "CountBadgeSemanticPreview"),
                "CountBadgeSemanticOwner");
            AssertPreviewOwnsRuntimeAdorner(
                previews.Single(static preview => preview.Name == "DotBadgeSemanticPreview"),
                "DotBadgeSemanticOwner");

            previews.Single(static preview => preview.Name == "RibbonBadgeSemanticPreview")
                    .AdditionalRoots.ShouldBeEmpty();
        });
    }

    [Fact]
    public void Badge_ShowCase_Reserves_Top_Space_For_Adorner_Badges()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataDisplay/Badge/Views/BadgeShowCase.axaml");

        source.ShouldContain("ContentMargin=\"28,28,28,28\"");
        source.ShouldNotContain("ContentMargin=\"28,10,28,28\"");
    }

    [Fact]
    public void Badge_Semantic_Part_Example_Is_Deferred_Scoped_And_Versioned()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataDisplay/Badge/Views/BadgeShowCase.axaml");
        var localization = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataDisplay/Badge/Localization/en-US.xlf");

        CountOccurrences(source, "IsDeferredContentEnabled=\"True\"").ShouldBe(11);
        source.ShouldContain("SourceKey=\"badge-semantic-part\"");
        source.ShouldContain("BadgeShowCaseLangResource SemanticPartStyleTitle");
        source.ShouldContain("BadgeShowCaseLangResource SemanticPartStyleDescription");
        source.ShouldContain("<StackPanel.Styles>");
        source.ShouldContain("<StackPanel Spacing=\"24\"\n                            HorizontalAlignment=\"Left\"");
        source.ShouldContain("<atom:CountBadgeIndicatorStyle x:SetterTargetType=\"Control\">");
        source.ShouldContain("Selector=\"atom|CountBadge.semantic-custom[Size=Default]\"");
        source.ShouldContain("<atom:CountBadgeIndicatorStyle x:SetterTargetType=\"Control\">");
        source.ShouldContain("Selector=\"atom|RibbonBadge.semantic-demo\"");
        source.ShouldContain("<atom:RibbonBadgeIndicatorStyle x:SetterTargetType=\"Control\">");
        source.ShouldContain("<atom:RibbonBadgeContentStyle x:SetterTargetType=\"TextBlock\">");
        source.ShouldContain("<StackPanel Orientation=\"Horizontal\" Spacing=\"16\"");
        source.ShouldContain("<StackPanel Spacing=\"16\"");
        CountOccurrences(source, "Width=\"40\"").ShouldBeGreaterThanOrEqualTo(2);
        CountOccurrences(source, "Height=\"40\"").ShouldBeGreaterThanOrEqualTo(2);
        CountOccurrences(source, "Classes=\"semantic-demo\"").ShouldBe(2);
        CountOccurrences(source, "Classes=\"semantic-demo semantic-custom\"").ShouldBe(2);
        source.ShouldNotContain("<Setter Property=\"Width\" Value=\"28\"");
        source.ShouldNotContain("<Setter Property=\"Height\" Value=\"28\"");
        source.ShouldNotContain("Selector=\".semantic-indicator\"");
        source.ShouldNotContain("Selector=\".semantic-content\"");
        source.ShouldNotContain("/template/ .semantic-");
        localization.ShouldContain("<source>Custom Semantic Part styling</source>");
        localization.ShouldContain("<source>Use owner-scoped style selectors to customize Badge's published Semantic Parts.</source>");
        localization.ShouldContain("<source>This card customizes its ribbon with Semantic Part style selectors.</source>");
        localization.ShouldNotContain("semantic dom");
        localization.ShouldNotContain("classNames");
    }

    [Fact]
    public void Badge_Semantic_Part_Example_Applies_Styles_To_Runtime_Parts()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new BadgeShowCase
        {
            DataContext = new BadgeViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 1800, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "badge-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var countBadges = page.GetVisualDescendants()
                                  .OfType<AtomUI.Desktop.Controls.CountBadge>()
                                  .Where(static badge => badge.Classes.Contains("semantic-demo"))
                                  .ToArray();
            countBadges.Length.ShouldBe(2);
            var smallCountBadge = countBadges.Single(static badge =>
                badge.Size == AtomUI.Controls.Commons.CountBadgeSize.Small);
            var customCountBadge = countBadges.Single(static badge =>
                badge.Size == AtomUI.Controls.Commons.CountBadgeSize.Default);
            smallCountBadge.DecoratedTarget.ShouldBeOfType<Border>().Width.ShouldBe(40);
            smallCountBadge.DecoratedTarget.ShouldBeOfType<Border>().Height.ShouldBe(40);
            customCountBadge.DecoratedTarget.ShouldBeOfType<Border>().Width.ShouldBe(40);
            customCountBadge.DecoratedTarget.ShouldBeOfType<Border>().Height.ShouldBe(40);
            customCountBadge.BadgeColor.ShouldBe("#696FC7");

            var smallCountIndicator  = FindSemanticIndicator(FindNativeAdorner(smallCountBadge));
            var customCountIndicator = FindSemanticIndicator(FindNativeAdorner(customCountBadge));
            smallCountIndicator.GetValue(TemplatedControl.FontSizeProperty).ShouldBe(10);
            customCountIndicator.GetValue(TemplatedControl.FontSizeProperty).ShouldBe(14);
            smallCountIndicator.Effect.ShouldBeNull();
            customCountIndicator.Effect.ShouldBeNull();

            var ribbonBadges = page.GetVisualDescendants()
                                   .OfType<AtomUI.Desktop.Controls.RibbonBadge>()
                                   .Where(static badge => badge.Classes.Contains("semantic-demo"))
                                   .ToArray();
            ribbonBadges.Length.ShouldBe(2);
            foreach (var ribbonBadge in ribbonBadges)
            {
                ribbonBadge.Width.ShouldBe(400);
                ribbonBadge.HorizontalAlignment.ShouldBe(Avalonia.Layout.HorizontalAlignment.Left);
                ribbonBadge.DecoratedTarget.ShouldBeOfType<AtomUI.Desktop.Controls.Card>()
                           .SizeType.ShouldBe(SizeType.Middle);
                ribbonBadge.DecoratedTarget.ShouldBeOfType<AtomUI.Desktop.Controls.Card>()
                           .BorderBrush.ShouldNotBeNull()
                           .ShouldBeAssignableTo<ISolidColorBrush>()
                           .Color.ShouldBe(Color.Parse("#d9d9d9"));
                ribbonBadge.DecoratedTarget.ShouldBeOfType<AtomUI.Desktop.Controls.Card>()
                           .CornerRadius.ShouldBe(new CornerRadius(10));
                FindSemanticIndicator(ribbonBadge)
                    .Effect.ShouldBeOfType<DropShadowEffect>().BlurRadius.ShouldBe(4);
            }

            var customRibbonBadge = ribbonBadges.Single(static badge => badge.Classes.Contains("semantic-custom"));
            customRibbonBadge.RibbonColor.ShouldBe("#696FC7");
            FindSemanticIndicator(customRibbonBadge)
                .GetVisualDescendants()
                .OfType<TextBlock>()
                .Single(static control => control.Classes.Contains("semantic-content"))
                .FontWeight.ShouldBe(FontWeight.Bold);
        });
    }

    [Fact]
    public void Badge_Examples_Layer_Adorners_Are_Hidden_While_Semantic_Tab_Is_Active()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new BadgeShowCase
        {
            DataContext = new BadgeViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            var examplesBadges = page.GetVisualDescendants()
                                     .OfType<AtomUI.Desktop.Controls.CountBadge>()
                                     .Where(static badge => badge.IsEffectivelyVisible)
                                     .ToArray();
            examplesBadges.ShouldNotBeEmpty("the deferred Examples items materialize CountBadge instances.");

            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var adornerLayer = AdornerLayer.GetAdornerLayer(page).ShouldNotBeNull();
            var staleAdorners = adornerLayer.Children
                                            .Where(static child =>
                                                AdornerLayer.GetAdornedElement(child) is { } adorned &&
                                                !adorned.IsEffectivelyVisible &&
                                                child.IsVisible)
                                            .ToArray();
            staleAdorners.ShouldBeEmpty(
                "hidden Examples badges must not paint indicators through the window adorner layer.");

            var semanticOwner = page.GetVisualDescendants()
                                    .OfType<AtomUI.Desktop.Controls.CountBadge>()
                                    .Single(static badge => badge.Name == "CountBadgeSemanticOwner");
            var semanticAdorner = adornerLayer.Children.Single(child =>
                ReferenceEquals(AdornerLayer.GetAdornedElement(child), semanticOwner));
            semanticAdorner.IsVisible.ShouldBeTrue(
                "the visible semantic preview owner must keep its indicator.");

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();

            var hiddenWhileExamplesActive = adornerLayer.Children
                                                       .Where(static child =>
                                                           AdornerLayer.GetAdornedElement(child) is { } adorned &&
                                                           !adorned.IsEffectivelyVisible &&
                                                           child.IsVisible)
                                                       .ToArray();
            hiddenWhileExamplesActive.ShouldBeEmpty();
            foreach (var badge in examplesBadges)
            {
                var adorner = adornerLayer.Children.SingleOrDefault(child =>
                    ReferenceEquals(AdornerLayer.GetAdornedElement(child), badge));
                if (adorner is not null && badge.BadgeIsVisible)
                {
                    adorner.IsVisible.ShouldBeTrue(
                        "switching back to Examples must restore the layer indicators.");
                }
            }
        });
    }

    private static string ExtractBadgeExampleItems(string source)
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
        return ShowCaseSnapshotMarkup.Normalize(source);
    }

    private static IReadOnlyList<Control> GetSemanticDemoBadges(Control page)
    {
        return page.GetVisualDescendants()
                   .OfType<Control>()
                   .Where(static control => control.Name is
                       "CountBadgeSemanticOwner" or
                       "DotBadgeSemanticOwner" or
                       "RibbonBadgeSemanticOwner")
                   .ToArray();
    }

    private static void AssertPreviewOwnsRuntimeAdorner(SemanticPartPreview preview, string ownerName)
    {
        var owner = preview.PreviewContent.ShouldNotBeNull();
        owner.Name.ShouldBe(ownerName);

        var adornerLayer = AdornerLayer.GetAdornerLayer(owner).ShouldNotBeNull();
        var runtimeAdorners = adornerLayer.Children
                                          .Where(child => ReferenceEquals(
                                              AdornerLayer.GetAdornedElement(child),
                                              owner))
                                          .ToArray();
        runtimeAdorners.Length.ShouldBe(
            1,
            $"owner loaded={owner.IsLoaded}, owner attached={owner.IsAttachedToVisualTree()}, " +
            $"preview loaded={preview.IsLoaded}, preview attached={preview.IsAttachedToVisualTree()}, " +
            $"layer children={adornerLayer.Children.Count}");
        preview.AdditionalRoots.Count.ShouldBe(
            1,
            $"runtime adorner exists but preview registration count is {preview.AdditionalRoots.Count}");
        var additionalRoot = preview.AdditionalRoots.Single();
        AdornerLayer.GetAdornedElement(additionalRoot).ShouldBeSameAs(owner);
    }

    private static Control FindNativeAdorner(Control badge)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(badge).ShouldNotBeNull();
        return adornerLayer.Children.Single(child =>
            ReferenceEquals(AdornerLayer.GetAdornedElement(child), badge));
    }

    private static Control FindSemanticIndicator(Control root)
    {
        return root.GetVisualDescendants()
                   .OfType<Control>()
                   .Single(static control => control.Classes.Contains("semantic-indicator"));
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
            Child              = content
        };
        var window = new AvaloniaWindow
        {
            Content = visualLayerManager,
            Width   = width,
            Height  = height
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
