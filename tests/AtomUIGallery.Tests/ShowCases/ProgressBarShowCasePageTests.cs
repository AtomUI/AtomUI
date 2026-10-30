using System.Security.Cryptography;
using System.Text;
using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.ProgressBar;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AtomUICircleProgress = AtomUI.Desktop.Controls.CircleProgress;
using AtomUIDashboardProgress = AtomUI.Desktop.Controls.DashboardProgress;
using AtomUIProgressBar = AtomUI.Desktop.Controls.ProgressBar;
using AtomUIStepsProgressBar = AtomUI.Desktop.Controls.StepsProgressBar;
using Rectangle = Avalonia.Controls.Shapes.Rectangle;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class ProgressBarShowCasePageTests
{

    [Fact]
    public void ProgressBar_ShowCase_Declares_The_Official_Semantic_Preview_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Feedback/ProgressBar/Views/ProgressBarShowCase.axaml");
        var viewModelSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Feedback/ProgressBar/ViewModels/ProgressBarViewModel.cs");
        var semanticSource = ExtractShowCaseItem(source, "progress-bar-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        CountOccurrences(source, "<gallery:SemanticPartPreview Name=").ShouldBe(4);
        foreach (var previewName in new[]
                 {
                     "ProgressBarSemanticPreview",
                     "StepsProgressBarSemanticPreview",
                     "CircleProgressSemanticPreview",
                     "DashboardProgressSemanticPreview"
                 })
        {
            source.ShouldContain($"Name=\"{previewName}\"");
        }

        foreach (var ownerName in new[]
                 {
                     "ProgressBarSemanticOwner",
                     "StepsProgressBarSemanticOwner",
                     "CircleProgressSemanticOwner",
                     "DashboardProgressSemanticOwner"
                 })
        {
            source.ShouldContain($"Name=\"{ownerName}\"");
        }

        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:ProgressBar}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:StepsProgressBar}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:CircleProgress}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:DashboardProgress}\"");
        CountOccurrences(source, "<atom:SegmentedItem Content=\"line\" />").ShouldBe(1);
        CountOccurrences(source, "<atom:SegmentedItem Content=\"steps\" />").ShouldBe(1);
        CountOccurrences(source, "<atom:SegmentedItem Content=\"circle\" />").ShouldBe(1);
        CountOccurrences(source, "<atom:SegmentedItem Content=\"dashboard\" />").ShouldBe(1);
        CountOccurrences(source, "OnContent=\"Gradient\"").ShouldBe(1);
        CountOccurrences(source, "OffContent=\"Gradient\"").ShouldBe(1);
        CountOccurrences(
            source,
            "ContentTemplate=\"{StaticResource SemanticProgressToolbarTemplate}\"").ShouldBe(4);
        CountOccurrences(source, "<Grid Height=\"200\" HorizontalAlignment=\"Stretch\">").ShouldBe(4);
        CountOccurrences(source, "VerticalAlignment=\"Top\"").ShouldBeGreaterThanOrEqualTo(4);
        CountOccurrences(source, "Value=\"80\"").ShouldBeGreaterThanOrEqualTo(4);
        source.ShouldContain("Steps=\"5\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(19);
        CountOccurrences(source, "Path=\"root\"").ShouldBe(4);
        CountOccurrences(source, "Path=\"body\"").ShouldBe(4);
        CountOccurrences(source, "Path=\"rail\"").ShouldBe(3);
        CountOccurrences(source, "Path=\"track\"").ShouldBe(4);
        CountOccurrences(source, "Path=\"indicator\"").ShouldBe(4);

        semanticSource.ShouldContain("SourceKey=\"progress-bar-semantic-part\"");
        semanticSource.ShouldContain("ProgressBarShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("ProgressBarShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Spacing=\"0\"");
        semanticSource.ShouldNotContain("Spacing=\"{atom:SharedTokenResource SpacingLG}\"");
        semanticSource.ShouldContain("<Setter Property=\"Padding\" Value=\"2\" />");
        semanticSource.ShouldContain("<Setter Property=\"CornerRadius\" Value=\"8\" />");
        semanticSource.ShouldContain("<Setter Property=\"Background\" Value=\"#1A000000\" />");
        semanticSource.ShouldContain("<Setter Property=\"CornerRadius\" Value=\"8\" />");
        semanticSource.ShouldContain("<BrushTransition Property=\"Background\" Duration=\"0:0:0.3\">");
        semanticSource.ShouldContain("<DoubleTransition Property=\"Width\" Duration=\"0:0:0.3\">");
        CountOccurrences(semanticSource, "<SplineEasing X1=\"0.25\" Y1=\"0.1\" X2=\"0.25\" Y2=\"1\" />")
            .ShouldBe(2);
        foreach (var percent in new[] { 10, 20, 40, 60, 80, 99 })
        {
            semanticSource.ShouldContain($"Selector=\"atom|ProgressBar.semantic-style-demo[Value={percent}]\"");
            semanticSource.ShouldContain($"Classes=\"semantic-style-demo\" Value=\"{percent}\"");
            viewModelSource.ShouldContain($"SemanticTrackBrush{percent} = CreateSemanticTrackBrush({percent});");
        }

        semanticSource.ShouldNotContain("/template/ .semantic-");
        viewModelSource.ShouldContain("Color.Parse(\"#108ee9\")");
        viewModelSource.ShouldContain("Color.Parse(\"#87d068\")");
        viewModelSource.ShouldContain("Math.Round(200 - percent * 2)");
        viewModelSource.ShouldContain("HslColor.FromAhsl(1, hue, 0.85, 0.65)");
        viewModelSource.ShouldContain("HslColor.FromAhsl(0.95, hue + 30, 0.9, 0.55)");
        foreach (var language in new[] { "en-US", "zh-CN", "zh-TW", "pt-BR" })
        {
            var catalog = ReadRepoFile(
                $"controlgallery/AtomUIGallery/ShowCases/Feedback/ProgressBar/Localization/{language}.xlf");
            catalog.ShouldContain("Custom Semantic Part styling");
            catalog.ShouldContain(
                "Use owner-scoped style selectors to customize ProgressBar's published Semantic Parts.");
            catalog.ShouldNotContain("semantic dom");
            catalog.ShouldNotContain("classNames");
            catalog.ShouldContain("Root element, set relative positioning and basic container styles");
            catalog.ShouldContain("Body element, set progress bar layout and size styles");
            catalog.ShouldContain(
                "Rail element, set background track color and border radius styles. Not exist in steps mode");
            catalog.ShouldContain("Track element, set progress fill color and transition animation styles");
            catalog.ShouldContain(
                "Indicator element, set percentage text or icon position and font styles");
        }
    }

    [Fact]
    public void ProgressBar_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ProgressBarShowCase
        {
            DataContext = new ProgressBarViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            GetSemanticPreviewOwners(page).ShouldBeEmpty();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().Count().ShouldBe(4);
            GetSemanticPreviewOwners(page).ShouldHaveSingleItem()
                .Name.ShouldBe("ProgressBarSemanticOwner");

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
            Assert.All(GetSemanticPreviewOwners(page), owner => Assert.False(owner.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void ProgressBar_Semantic_Preview_Matches_The_Official_Type_And_Gradient_State()
    {
        AvaloniaTestApp.EnsureInitialized();

        var viewModel = new ProgressBarViewModel(new TestScreen());
        var page = new ProgressBarShowCase
        {
            DataContext = viewModel
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            viewModel.IsSemanticGradientEnabled = true;
            Dispatcher.UIThread.RunJobs();

            var line = page.GetVisualDescendants()
                           .OfType<AtomUIProgressBar>()
                           .Single(static progress => progress.Name == "ProgressBarSemanticOwner");
            line.Value.ShouldBe(80);
            line.StrokeBrush.ShouldNotBeNull().ShouldBeAssignableTo<IGradientBrush>();
            AssertVisiblePreview(page, "ProgressBarSemanticPreview");
            AssertOfficialSemanticPreviewLayout(
                page,
                line,
                HorizontalAlignment.Stretch);

            viewModel.SemanticPreviewIndex = 1;
            Dispatcher.UIThread.RunJobs();
            var steps = page.GetVisualDescendants()
                            .OfType<AtomUIStepsProgressBar>()
                            .Single(static progress => progress.Name == "StepsProgressBarSemanticOwner");
            steps.Value.ShouldBe(80);
            steps.Steps.ShouldBe(5);
            steps.StrokeBrush.ShouldNotBeNull().ShouldBeAssignableTo<IGradientBrush>();
            AssertVisiblePreview(page, "StepsProgressBarSemanticPreview");
            AssertOfficialSemanticPreviewLayout(
                page,
                steps,
                HorizontalAlignment.Center);
            AssertStepsIndicatorContent(steps, "80%");

            viewModel.SemanticPreviewIndex = 2;
            Dispatcher.UIThread.RunJobs();
            var circle = page.GetVisualDescendants()
                             .OfType<AtomUICircleProgress>()
                             .Single(static progress => progress.Name == "CircleProgressSemanticOwner");
            circle.Value.ShouldBe(80);
            circle.StrokeBrush.ShouldNotBeNull().ShouldBeAssignableTo<IGradientBrush>();
            AssertVisiblePreview(page, "CircleProgressSemanticPreview");
            AssertOfficialSemanticPreviewLayout(
                page,
                circle,
                HorizontalAlignment.Center);

            viewModel.SemanticPreviewIndex = 3;
            Dispatcher.UIThread.RunJobs();
            var dashboard = page.GetVisualDescendants()
                                .OfType<AtomUIDashboardProgress>()
                                .Single(static progress => progress.Name == "DashboardProgressSemanticOwner");
            dashboard.Value.ShouldBe(80);
            dashboard.StrokeBrush.ShouldNotBeNull().ShouldBeAssignableTo<IGradientBrush>();
            AssertVisiblePreview(page, "DashboardProgressSemanticPreview");
            AssertOfficialSemanticPreviewLayout(
                page,
                dashboard,
                HorizontalAlignment.Center);

            var gradient = viewModel.SemanticPreviewGradientBrush;
            gradient.StartPoint.ShouldBe(new RelativePoint(0, 0.5, RelativeUnit.Relative));
            gradient.EndPoint.ShouldBe(new RelativePoint(1, 0.5, RelativeUnit.Relative));
            gradient.GradientStops[0].Color.ShouldBe(Color.Parse("#108ee9"));
            gradient.GradientStops[1].Color.ShouldBe(Color.Parse("#87d068"));
        });
    }

    [Fact]
    public void ProgressBar_Semantic_Style_Example_Applies_The_Official_Root_Rail_And_Track_Styles()
    {
        AvaloniaTestApp.EnsureInitialized();

        var viewModel = new ProgressBarViewModel(new TestScreen());
        var page = new ProgressBarShowCase
        {
            DataContext = viewModel
        };

        ShowInWindow(page, 1280, 1400, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "progress-bar-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var progressBars = page.GetVisualDescendants()
                                   .OfType<AtomUIProgressBar>()
                                   .Where(static progress => progress.Classes.Contains("semantic-style-demo"))
                                   .OrderBy(static progress => progress.Value)
                                   .ToArray();
            progressBars.Select(static progress => progress.Value)
                        .ShouldBe([10d, 20d, 40d, 60d, 80d, 99d]);
            for (var index = 0; index < progressBars.Length - 1; index++)
            {
                var gap = progressBars[index + 1].Bounds.Top - progressBars[index].Bounds.Bottom;
                gap.ShouldBe(0);
            }

            var expectedBrushes = new Dictionary<double, LinearGradientBrush>
            {
                [10] = viewModel.SemanticTrackBrush10,
                [20] = viewModel.SemanticTrackBrush20,
                [40] = viewModel.SemanticTrackBrush40,
                [60] = viewModel.SemanticTrackBrush60,
                [80] = viewModel.SemanticTrackBrush80,
                [99] = viewModel.SemanticTrackBrush99
            };

            foreach (var progress in progressBars)
            {
                progress.Padding.ShouldBe(new Thickness(2));
                progress.CornerRadius.ShouldBe(new CornerRadius(8));

                var rail = FindSemanticBorder(progress, "semantic-rail");
                rail.Background.ShouldNotBeNull()
                    .ShouldBeAssignableTo<ISolidColorBrush>()
                    .Color.ShouldBe(Color.Parse("#1A000000"));
                rail.CornerRadius.ShouldBe(new CornerRadius(8));

                var track = FindSemanticBorder(progress, "semantic-track");
                AssertGradientBrush(track.Background, expectedBrushes[progress.Value]);
                track.CornerRadius.ShouldBe(new CornerRadius(8));
                var transitions = track.Transitions.ShouldNotBeNull();
                var brushTransition = transitions.OfType<BrushTransition>().ShouldHaveSingleItem();
                brushTransition.Duration.ShouldBe(TimeSpan.FromSeconds(0.3));
                AssertCssEase(brushTransition.Easing);
                var doubleTransition = transitions.OfType<DoubleTransition>().ShouldHaveSingleItem();
                doubleTransition.Duration.ShouldBe(TimeSpan.FromSeconds(0.3));
                AssertCssEase(doubleTransition.Easing);

                progress.GetVisualDescendants()
                        .OfType<Panel>()
                        .Single(static panel => panel.Classes.Contains("semantic-indicator"))
                        .IsVisible.ShouldBeTrue();
            }
        });
    }

    [Fact]
    public void ProgressBar_Examples_Steps_Indicators_Are_Correct_On_The_First_Deferred_Layout()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ProgressBarShowCase
        {
            DataContext = new ProgressBarViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 1400, () =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab.ShouldBe(GalleryShowCaseTab.Examples);

            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            foreach (var item in panel.Children.OfType<ShowCaseItem>())
            {
                item.MaterializeDeferredContent();
            }
            Dispatcher.UIThread.RunJobs();

            var positionExamples = page.GetVisualDescendants()
                                       .OfType<StackPanel>()
                                       .Single(static candidate =>
                                           candidate.Children.OfType<AtomUIStepsProgressBar>().Count() == 8);
            foreach (var progress in positionExamples.Children.OfType<AtomUIStepsProgressBar>())
            {
                AssertStepsIndicatorPlacement(progress);
            }
        });
    }

    private static string ExtractProgressBarExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"progress-bar-semantic-part\"";
        var semanticItemStart = source.IndexOf(semanticItemMarker, firstItemStart, StringComparison.Ordinal);
        semanticItemStart.ShouldBeGreaterThan(firstItemStart);
        var panelCloseStart = source.LastIndexOf(firstItemMarker, semanticItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string ExtractShowCaseItem(string source, string sourceKey)
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
        return ShowCaseSnapshotMarkup.Normalize(source);
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

    private static IReadOnlyList<Control> GetSemanticPreviewOwners(Control page)
    {
        return page.GetVisualDescendants()
                   .OfType<Control>()
                   .Where(static control => control.Name is
                       "ProgressBarSemanticOwner" or
                       "StepsProgressBarSemanticOwner" or
                       "CircleProgressSemanticOwner" or
                       "DashboardProgressSemanticOwner")
                   .ToArray();
    }

    private static Border FindSemanticBorder(Control owner, string semanticClass)
    {
        return owner.GetVisualDescendants()
                    .OfType<Border>()
                    .Single(border => border.Classes.Contains(semanticClass));
    }

    private static void AssertVisiblePreview(Control page, string expectedName)
    {
        var visiblePreviews = page.GetVisualDescendants()
                                  .OfType<SemanticPartPreview>()
                                  .Where(static preview => preview.IsVisible)
                                  .ToArray();
        visiblePreviews.ShouldHaveSingleItem().Name.ShouldBe(expectedName);
    }

    private static void AssertOfficialSemanticPreviewLayout(
        Control page,
        Control owner,
        HorizontalAlignment expectedHorizontalAlignment)
    {
        var preview = page.GetVisualDescendants()
                          .OfType<SemanticPartPreview>()
                          .Single(static candidate => candidate.IsVisible);
        var block = preview.PreviewContent.ShouldBeOfType<StackPanel>();
        block.Children.Count.ShouldBe(2);

        var toolbarHost = block.Children[0].ShouldBeOfType<ContentControl>();
        var toolbar = toolbarHost.GetVisualDescendants()
                                 .OfType<StackPanel>()
                                 .Single(static panel =>
                                     panel.Orientation == Orientation.Horizontal &&
                                     panel.Children.OfType<AtomUI.Desktop.Controls.Segmented>().Any() &&
                                     panel.Children.OfType<AtomUI.Desktop.Controls.ToggleSwitch>().Any());
        toolbar.Orientation.ShouldBe(Orientation.Horizontal);
        var segmented = toolbar.Children.OfType<AtomUI.Desktop.Controls.Segmented>().ShouldHaveSingleItem();
        var toggle = toolbar.Children.OfType<AtomUI.Desktop.Controls.ToggleSwitch>().ShouldHaveSingleItem();
        segmented.Bounds.Center.Y.ShouldBe(toggle.Bounds.Center.Y, 0.01);

        var progressStage = block.Children[1].ShouldBeOfType<Grid>();
        progressStage.Height.ShouldBe(200);
        progressStage.Bounds.Top.ShouldBe(toolbarHost.Bounds.Bottom + block.Spacing, 0.01);

        owner.Parent.ShouldBeSameAs(progressStage);
        owner.VerticalAlignment.ShouldBe(VerticalAlignment.Top);
        owner.HorizontalAlignment.ShouldBe(expectedHorizontalAlignment);
        owner.Bounds.Top.ShouldBe(0, 0.01);
        if (expectedHorizontalAlignment == HorizontalAlignment.Stretch)
        {
            owner.Bounds.Left.ShouldBe(0, 0.01);
            owner.Bounds.Width.ShouldBe(progressStage.Bounds.Width, 0.01);
        }
        else
        {
            owner.Bounds.Center.X.ShouldBe(progressStage.Bounds.Width / 2, 0.51);
        }
    }

    private static void AssertCssEase(Easing easing)
    {
        easing.ShouldBeOfType<SplineEasing>().ShouldSatisfyAllConditions(
            spline => spline.X1.ShouldBe(0.25),
            spline => spline.Y1.ShouldBe(0.1),
            spline => spline.X2.ShouldBe(0.25),
            spline => spline.Y2.ShouldBe(1));
    }

    private static void AssertGradientBrush(IBrush? actualBrush, ILinearGradientBrush expectedBrush)
    {
        var actual = actualBrush.ShouldNotBeNull().ShouldBeAssignableTo<ILinearGradientBrush>();
        actual.StartPoint.ShouldBe(expectedBrush.StartPoint);
        actual.EndPoint.ShouldBe(expectedBrush.EndPoint);
        actual.GradientStops.Count.ShouldBe(expectedBrush.GradientStops.Count);
        for (var index = 0; index < expectedBrush.GradientStops.Count; index++)
        {
            actual.GradientStops[index].Color.ShouldBe(expectedBrush.GradientStops[index].Color);
            actual.GradientStops[index].Offset.ShouldBe(expectedBrush.GradientStops[index].Offset);
        }
    }

    private static void AssertStepsIndicatorContent(AtomUIStepsProgressBar progress, string expectedText)
    {
        var indicator = progress.GetVisualDescendants()
                                .OfType<Panel>()
                                .Single(static candidate => candidate.Classes.Contains("semantic-indicator"));
        indicator.IsVisible.ShouldBeTrue();
        indicator.Bounds.Width.ShouldBeGreaterThan(0);
        indicator.Bounds.Height.ShouldBeGreaterThan(0);
        indicator.Bounds.Right.ShouldBeLessThanOrEqualTo(progress.Bounds.Width + 0.001);
        indicator.Bounds.Bottom.ShouldBeLessThanOrEqualTo(progress.Bounds.Height + 0.001);

        var label = indicator.GetVisualDescendants()
                             .OfType<Label>()
                             .Single(static candidate => candidate.Name == "PART_PercentageLabel");
        label.Content.ShouldBe(expectedText);
        label.IsVisible.ShouldBeTrue();
        label.Bounds.Width.ShouldBeGreaterThan(0);
        label.Bounds.Height.ShouldBeGreaterThan(0);
        var labelForeground = label.Foreground.ShouldNotBeNull().ShouldBeAssignableTo<ISolidColorBrush>();
        var ownerForeground = progress.Foreground.ShouldNotBeNull().ShouldBeAssignableTo<ISolidColorBrush>();
        labelForeground.Color.ShouldBe(ownerForeground.Color);
        labelForeground.Color.A.ShouldBeGreaterThan((byte)0);

        var labelOrigin = label.TranslatePoint(default, progress).ShouldNotBeNull();
        var labelRect = new Rect(labelOrigin, label.Bounds.Size);
        labelRect.Left.ShouldBeGreaterThanOrEqualTo(-0.001);
        labelRect.Top.ShouldBeGreaterThanOrEqualTo(-0.001);
        labelRect.Right.ShouldBeLessThanOrEqualTo(progress.Bounds.Width + 0.001);
        labelRect.Bottom.ShouldBeLessThanOrEqualTo(progress.Bounds.Height + 0.001);
    }

    private static void AssertStepsIndicatorPlacement(AtomUIStepsProgressBar progress)
    {
        const double tolerance = 0.001;
        var tracks = progress.GetVisualDescendants()
                             .OfType<Rectangle>()
                             .Where(static track => track.Classes.Contains("semantic-track"))
                             .ToArray();
        var indicator = progress.GetVisualDescendants()
                                .OfType<Panel>()
                                .Single(static candidate => candidate.Classes.Contains("semantic-indicator"));
        var trackLeft = tracks.Min(static track => track.Bounds.Left);
        var trackRight = tracks.Max(static track => track.Bounds.Right);
        var trackBottom = tracks.Max(static track => track.Bounds.Bottom);

        if (progress.PercentPosition == LinePercentAlignment.Start)
        {
            indicator.Bounds.Right.ShouldBeLessThanOrEqualTo(trackLeft + tolerance);
        }
        else if (progress.PercentPosition == LinePercentAlignment.Center)
        {
            indicator.Bounds.Top.ShouldBeGreaterThanOrEqualTo(trackBottom - tolerance);
        }
        else
        {
            indicator.Bounds.Left.ShouldBeGreaterThanOrEqualTo(trackRight - tolerance);
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
