using System.Reflection;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
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
        token.AssignEffectiveGlobalToken(new DesignToken
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

        token.CalculateTokenValues(isDarkMode: false);

        AssertLoadingBrush(token.LoadingBackgroundStart, new Point(-3.0, 0.5), new Point(1.0, 0.5));
        AssertLoadingBrush(token.LoadingBackgroundMiddle, new Point(-1.5, 0.5), new Point(2.5, 0.5));
        AssertLoadingBrush(token.LoadingBackgroundEnd, new Point(0.0, 0.5), new Point(4.0, 0.5));
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

        imageTheme.ShouldContain("Border Name=\"PART_ContentLayer\"");
        imageTheme.ShouldContain("IsVisible=\"{TemplateBinding IsActive, Converter={x:Static BoolConverters.Not}}\"");
        imageTheme.ShouldContain("Border Name=\"PART_ActiveAnimationLayer\"");
        imageTheme.IndexOf("Border Name=\"PART_ActiveAnimationLayer\"", StringComparison.Ordinal)
                  .ShouldBeGreaterThan(imageTheme.IndexOf("Border Name=\"PART_ContentLayer\"", StringComparison.Ordinal));
        imageTheme.IndexOf("antdicons:ImageFilled", StringComparison.Ordinal)
                  .ShouldBeGreaterThan(imageTheme.IndexOf("Border Name=\"PART_ActiveAnimationLayer\"", StringComparison.Ordinal));

        nodeTheme.ShouldContain("Border Name=\"PART_ContentLayer\"");
        nodeTheme.ShouldContain("IsVisible=\"{TemplateBinding IsActive, Converter={x:Static BoolConverters.Not}}\"");
        nodeTheme.ShouldContain("Border Name=\"PART_ActiveAnimationLayer\"");
        nodeTheme.IndexOf("Border Name=\"PART_ActiveAnimationLayer\"", StringComparison.Ordinal)
                 .ShouldBeGreaterThan(nodeTheme.IndexOf("Border Name=\"PART_ContentLayer\"", StringComparison.Ordinal));
        nodeTheme.IndexOf("ContentPresenter", StringComparison.Ordinal)
                 .ShouldBeGreaterThan(nodeTheme.IndexOf("Border Name=\"PART_ActiveAnimationLayer\"", StringComparison.Ordinal));
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
            var activeLayer = skeleton.GetVisualDescendants()
                                      .OfType<Border>()
                                      .Single(border => border.Name == "PART_ActiveAnimationLayer");

            activeLayer.GetValue(Border.BackgroundProperty).ShouldBeNull();

            skeleton.IsActive = true;
            Dispatcher.UIThread.RunJobs();
            activeLayer.GetValue(Border.BackgroundProperty).ShouldNotBeNull();

            skeleton.IsActive = false;
            Dispatcher.UIThread.RunJobs();
            activeLayer.GetValue(Border.BackgroundProperty).ShouldBeNull();
        });
    }

    [Fact]
    public void Active_Animation_Pauses_When_Skeleton_Is_Effectively_Invisible()
    {
        var skeleton = new SkeletonLine();
        var buildAnimation = typeof(AbstractSkeleton).GetMethod(
            "BuildActiveAnimation",
            BindingFlags.Instance | BindingFlags.NonPublic);

        buildAnimation.ShouldNotBeNull();
        var animation = buildAnimation.Invoke(skeleton, null).ShouldBeOfType<Animation>();

        animation.PlaybackBehavior.ShouldBe(PlaybackBehavior.OnlyIfVisible);
    }

    private static void AssertLoadingBrush(IBrush? brush, Point expectedStartPoint, Point expectedEndPoint)
    {
        var gradientBrush = brush.ShouldBeOfType<LinearGradientBrush>();
        gradientBrush.StartPoint.Unit.ShouldBe(RelativeUnit.Relative);
        gradientBrush.StartPoint.Point.ShouldBe(expectedStartPoint);
        gradientBrush.EndPoint.Unit.ShouldBe(RelativeUnit.Relative);
        gradientBrush.EndPoint.Point.ShouldBe(expectedEndPoint);

        gradientBrush.GradientStops.Count.ShouldBe(3);
        gradientBrush.GradientStops[0].Offset.ShouldBe(0.25d);
        gradientBrush.GradientStops[1].Offset.ShouldBe(0.37d);
        gradientBrush.GradientStops[2].Offset.ShouldBe(0.63d);
        gradientBrush.GradientStops[0].Color.ShouldBe(Colors.LightGray);
        gradientBrush.GradientStops[1].Color.ShouldBe(Colors.WhiteSmoke);
        gradientBrush.GradientStops[2].Color.ShouldBe(Colors.LightGray);
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
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
