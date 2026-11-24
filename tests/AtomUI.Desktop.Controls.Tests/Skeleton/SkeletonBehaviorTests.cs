using AtomUI.Desktop.Controls.Tests.Theme;
using AtomUI.Controls;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Skeleton;

public class SkeletonBehaviorTests
{
    static SkeletonBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }
    
    [Fact]
    public void Loading_Background_Uses_AntDesign_Shimmer_Geometry()
    {
        var token = new SkeletonToken();
        AntDesignTokenRuleRunner.Apply(token, new DesignToken
        {
            ColorFillContent   = Colors.LightGray,
            ColorFill          = Colors.WhiteSmoke,
            ControlHeight      = 32,
            BorderRadiusSM     = new CornerRadius(4),
            UniformlyMargin    = 16,
            UniformlyMarginLG  = 24,
            UniformlyMarginXXS = 4,
            ControlHeightXS    = 16
        });

        // The band: base colour, highlight at 0.316 of 1.52W, base colour; absolute units so the host width rules.
        var width = 300d;
        var brush = SkeletonShimmerVisualHandler.CreateBandBrush(token.GradientFromColor, token.GradientToColor, width);
        brush.StartPoint.ShouldBe(new RelativePoint(0, 0, RelativeUnit.Absolute));
        brush.EndPoint.Point.X.ShouldBe(1.52 * width, 1e-9);
        brush.GradientStops.Select(stop => stop.Color)
             .ShouldBe([Colors.LightGray, Colors.WhiteSmoke, Colors.LightGray]);

        // Same trajectory as the Ant Design 400% background the token brushes still describe: the highlight
        // (0.37 of a 4W gradient) starts at -1.52W and ends at +1.48W.
        var linear = new LinearEasing();
        var duration = token.LoadingMotionDuration;
        duration.ShouldBe(TimeSpan.FromSeconds(1.4));
        AssertPeak(token.LoadingBackgroundStart, 0);
        AssertPeak(token.LoadingBackgroundEnd, duration.TotalSeconds - 1e-9);

        void AssertPeak(IBrush? legacyBrush, double seconds)
        {
            var legacy = legacyBrush.ShouldBeOfType<LinearGradientBrush>();
            var legacyPeak = (legacy.StartPoint.Point.X +
                              (legacy.EndPoint.Point.X - legacy.StartPoint.Point.X) * legacy.GradientStops[1].Offset) * width;
            var peak = SkeletonShimmerVisualHandler.SampleBandLeft(seconds, width, duration, linear) +
                       SkeletonShimmerVisualHandler.PeakOffset * SkeletonShimmerVisualHandler.BandWidthFactor * width;
            peak.ShouldBe(legacyPeak, 0.001 * width);
        }
    }

    [Fact]
    public void SkeletonInput_Accepts_Custom_SizeType()
    {
        var input = new SkeletonInput
        {
            SizeType = CustomizableSizeType.Small
        };

        input.SizeType.ShouldBe(CustomizableSizeType.Small);
    }

    [Fact]
    public void Image_And_Node_Templates_Do_Not_Cover_Active_Background_With_Content_Background()
    {
        var imageTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonImageTheme.axaml");
        var nodeTheme  = ReadRepoFile("src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonNodeTheme.axaml");

        foreach (var theme in new[] { imageTheme, nodeTheme })
        {
            theme.ShouldContain("Border Name=\"PART_ContentLayer\"");
            theme.ShouldContain("atom:AbstractSkeleton.IsShimmerBlock=\"True\"");
            theme.ShouldNotContain("PART_ActiveAnimationLayer");
            theme.ShouldNotContain("IsVisible=\"{TemplateBinding IsActive");
        }
        imageTheme.IndexOf("antdicons:ImageFilled", StringComparison.Ordinal)
                  .ShouldBeGreaterThan(imageTheme.IndexOf("Border Name=\"PART_ContentLayer\"", StringComparison.Ordinal));
        nodeTheme.IndexOf("ContentPresenter", StringComparison.Ordinal)
                 .ShouldBeGreaterThan(nodeTheme.IndexOf("Border Name=\"PART_ContentLayer\"", StringComparison.Ordinal));

        // At runtime the shimmer hangs on the content layer, so the icon, a later sibling, stays above it.
        var image = new SkeletonImage { IsActive = true };
        ShowInWindow(image, () =>
        {
            var layer = image.ShimmerLayer.ShouldBeOfType<Border>();
            layer.Name.ShouldBe("PART_ContentLayer");
            var layout = (Panel)layer.GetVisualParent()!;
            layout.Children.IndexOf(layer).ShouldBeLessThan(layout.Children.Count - 1);
            layout.Children[^1].ShouldBeAssignableTo<Icon>();
            ElementComposition.GetElementChildVisual(layer).ShouldBeSameAs(image.ShimmerVisual);
        });
    }

    [Fact]
    public void Paragraph_Lines_Follow_Active_And_Round_State()
    {
        var paragraph = new SkeletonParagraph
        {
            Rows    = 2,
            IsRound = false
        };

        ShowInWindow(paragraph, () =>
        {
            var lines = GetParagraphLines(paragraph);
            lines.Length.ShouldBe(2);
            lines.All(line => !line.IsActive).ShouldBeTrue();
            lines.All(line => !line.IsRound).ShouldBeTrue();

            paragraph.IsActive = true;
            paragraph.IsRound  = true;
            Dispatcher.UIThread.RunJobs();

            lines = GetParagraphLines(paragraph);
            lines.All(line => line.IsActive).ShouldBeTrue();
            lines.All(line => line.IsRound).ShouldBeTrue();

            paragraph.Rows = 3;
            Dispatcher.UIThread.RunJobs();

            lines = GetParagraphLines(paragraph);
            lines.Length.ShouldBe(3);
            lines.All(line => line.IsActive).ShouldBeTrue();
            lines.All(line => line.IsRound).ShouldBeTrue();
        });
    }
    
    [Fact]
    public void Active_Animation_Is_Applied_To_Template_Layer_And_Released_When_Inactive()
    {
        var skeleton = new SkeletonLine();

        ShowInWindow(skeleton, () =>
        {
            var contentLayer = skeleton.GetVisualDescendants()
                                       .OfType<Border>()
                                       .Single(border => border.Name == "PART_ContentLayer");
            var baseBackground = contentLayer.Background.ShouldNotBeNull();
            skeleton.ShimmerVisual.ShouldBeNull();

            skeleton.IsActive = true;
            Dispatcher.UIThread.RunJobs();
            skeleton.IsShimmerRunning.ShouldBeTrue();
            contentLayer.Background.ShouldBeNull();
            contentLayer.IsAnimating(Border.BackgroundProperty).ShouldBeTrue();
            ElementComposition.GetElementChildVisual(contentLayer).ShouldBeSameAs(skeleton.ShimmerVisual);

            skeleton.IsActive = false;
            Dispatcher.UIThread.RunJobs();
            skeleton.IsShimmerRunning.ShouldBeFalse();
            contentLayer.Background.ShouldBeSameAs(baseBackground);
            contentLayer.IsAnimating(Border.BackgroundProperty).ShouldBeFalse();
        });
    }

    [Fact]
    public void Active_Animation_Pauses_When_Skeleton_Is_Effectively_Invisible()
    {
        var skeleton = new SkeletonLine { IsActive = true };
        var parent   = new Panel { Children = { skeleton } };

        ShowInWindow(parent, () =>
        {
            skeleton.IsShimmerRunning.ShouldBeTrue();

            parent.IsVisible = false;
            Dispatcher.UIThread.RunJobs();
            skeleton.IsShimmerRunning.ShouldBeFalse();

            parent.IsVisible = true;
            Dispatcher.UIThread.RunJobs();
            skeleton.IsShimmerRunning.ShouldBeTrue();

            skeleton.IsMotionEnabled = false;
            Dispatcher.UIThread.RunJobs();
            skeleton.IsShimmerRunning.ShouldBeFalse();
        });
    }

    private static string ReadRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Unable to locate repository file '{relativePath}'.");
    }
    
    private static SkeletonLine[] GetParagraphLines(SkeletonParagraph paragraph)
    {
        return paragraph.GetVisualDescendants().OfType<SkeletonLine>().ToArray();
    }
    
    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 160,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
