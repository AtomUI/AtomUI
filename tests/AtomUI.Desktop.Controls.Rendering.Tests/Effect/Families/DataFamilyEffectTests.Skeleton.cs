using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Skeleton and its placeholder controls. Skeleton is Token-owner transparent (the content it wraps keeps the host's owner)
/// and draws only through its registered parts, which read the Skeleton Own Tokens through <c>SkeletonTokenResource</c> and
/// their own globals. SkeletonElement (abstract) and SkeletonImage share their themes with SkeletonButton / SkeletonInput and
/// SkeletonNode, which run the base theme's reads as their own rows.
/// </summary>
public partial class DataFamilyEffectTests
{
    /// <summary>
    /// Tokens only visible in the running shimmer (the highlight colour and the global it derives from); asserted by
    /// <see cref="Skeleton_Shimmer_Follows_The_Gradient_Tokens"/>. The LoadingBackground* keyframe brushes are Reserved:
    /// the shimmer is drawn from ShimmerFromColor / ShimmerToColor.
    /// </summary>
    private static readonly HashSet<string> s_shimmerTokens = new(StringComparer.Ordinal)
    {
        "GradientToColor", "ColorFill"
    };

    internal static Skeleton CreateSkeleton(bool avatar = false, bool round = false) => new()
    {
        Width        = 360,
        IsLoading    = true,
        IsShowAvatar = avatar,
        IsRound      = round
    };

    public static IEnumerable<object[]> SkeletonRows() =>
        MapRows("Skeleton").Where(static row => !s_shimmerTokens.Contains((string)row[0]));

    [Theory]
    [MemberData(nameof(SkeletonRows))]
    public void Skeleton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = SkeletonScenario(token);
        Run(scenario.Create, scenario.Arrange, SkeletonTokens.Identity, token, scenario.Options);
    }

    private static Scenario SkeletonScenario(string token)
    {
        switch (token)
        {
            case "AvatarMarginRight" or "UniformlyMargin":
                return new Scenario(static () => CreateSkeleton(avatar: true));
            // A round paragraph line is a capsule: a larger radius is clamped, so shrink it to show the corners (compared on
            // the title line, where the corners are a visible share of the bounds).
            case "ParagraphLineRoundCornerRadius":
                return new Scenario(static () => CreateSkeleton(round: true), null,
                    new EffectOptions { Perturb = static _ => new CornerRadius(1), Region = TitleOf });
            case "BlockRadius" or "BorderRadiusSM":
                return new Scenario(static () => CreateSkeleton(), null, new EffectOptions { Region = TitleOf });
            // The image placeholder Tokens are Skeleton Own Tokens drawn by a SkeletonImage: read them on a Skeleton
            // next to it and compare the image.
            case "ImageColor" or "ImageSize" or "ImageContainerSize":
                return new Scenario(static () => SkeletonWithImage(), null, ImageOptions());
            // The image container is far below its maximum: only a smaller maximum shows.
            case "ImageContainerMaxSize":
                return new Scenario(static () => SkeletonWithImage(), null,
                    ImageOptions() with { Perturb = static _ => 40d });
            default:
                // GradientFromColor / ColorFillContent, ParagraphLineHeight / TitleHeight / ControlHeight, BlockRadius /
                // BorderRadiusSM, ParagraphMarginTop / UniformlyMarginLG / UniformlyMarginXXS, LoadingMotionDuration.
                return new Scenario(static () => CreateSkeleton());
        }

        static Control TitleOf(Control skeleton) => Descendant<SkeletonTitle>(skeleton);

        static Control SkeletonWithImage() =>
            new StackPanel { Spacing = 8, Children = { CreateSkeleton(), new SkeletonImage() } };

        static EffectOptions ImageOptions() => new()
        {
            Target = static root => Descendant<Skeleton>(root),
            Region = static root => Descendant<SkeletonImage>(TopLevel.GetTopLevel(root)!)
        };
    }

    public static IEnumerable<object[]> SkeletonAvatarRows() => MapRows("SkeletonAvatar");

    [Theory]
    [MemberData(nameof(SkeletonAvatarRows))]
    public void SkeletonAvatar_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // Square avatars use the size's radius; a custom-size square the middle BorderRadius.
        Func<Control> create = token switch
        {
            "ControlHeightLG" => static () => new SkeletonAvatar { SizeType = CustomizableSizeType.Large },
            "ControlHeightSM" => static () => new SkeletonAvatar { SizeType = CustomizableSizeType.Small },
            "BorderRadiusLG" => static () => new SkeletonAvatar { SizeType = CustomizableSizeType.Large, Shape = AvatarShape.Square },
            "BorderRadiusSM" => static () => new SkeletonAvatar { SizeType = CustomizableSizeType.Small, Shape = AvatarShape.Square },
            "BorderRadius" => static () => new SkeletonAvatar { Shape = AvatarShape.Square },
            _ => static () => new SkeletonAvatar()
        };
        Run(create, null, SkeletonAvatarTokens.Identity, token);
    }

    // SkeletonElement is abstract: its theme reads run as rows of SkeletonButton and SkeletonInput, which share the theme.
    public static IEnumerable<object[]> SkeletonButtonRows() => HostRows("SkeletonButton", ["SkeletonElement"], []);

    [Theory]
    [MemberData(nameof(SkeletonButtonRows))]
    public void SkeletonButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(ElementCreate<SkeletonButton>(token), null, SkeletonButtonTokens.Identity, token);
    }

    public static IEnumerable<object[]> SkeletonInputRows() => HostRows("SkeletonInput", ["SkeletonElement"], []);

    [Theory]
    [MemberData(nameof(SkeletonInputRows))]
    public void SkeletonInput_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(ElementCreate<SkeletonInput>(token), null, SkeletonInputTokens.Identity, token);
    }

    private static Func<Control> ElementCreate<T>(string token)
        where T : SkeletonElement, new()
    {
        var size = token switch
        {
            "ControlHeightLG" => CustomizableSizeType.Large,
            "ControlHeightSM" => CustomizableSizeType.Small,
            _ => CustomizableSizeType.Middle
        };
        return () => new T { SizeType = size, Width = 160 };
    }

    public static IEnumerable<object[]> SkeletonImageRows() => MapRows("SkeletonImage");

    [Theory]
    [MemberData(nameof(SkeletonImageRows))]
    public void SkeletonImage_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // The only global is the corner radius of the 96 px container: perturb to a radius that clearly shows.
        Run(static () => new SkeletonImage(), null, SkeletonImageTokens.Identity, token,
            new EffectOptions { Perturb = static value => value is CornerRadius ? new CornerRadius(24) : TokenPerturbation.Perturb(value) });
    }

    public static IEnumerable<object[]> SkeletonNodeRows() => HostRows("SkeletonNode", ["SkeletonImage"], []);

    [Theory]
    [MemberData(nameof(SkeletonNodeRows))]
    public void SkeletonNode_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(static () => new SkeletonNode { Content = "Node" }, null, SkeletonNodeTokens.Identity, token,
            new EffectOptions { Perturb = static value => value is CornerRadius ? new CornerRadius(24) : TokenPerturbation.Perturb(value) });
    }

    public static IEnumerable<object[]> SkeletonParagraphRows() => MapRows("SkeletonParagraph");

    [Theory]
    [MemberData(nameof(SkeletonParagraphRows))]
    public void SkeletonParagraph_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(static () => new SkeletonParagraph { Width = 300, Rows = 3 }, null, SkeletonParagraphTokens.Identity, token);
    }

    [Fact]
    public void Skeleton_Shimmer_Follows_The_Gradient_Tokens()
    {
        // The active shimmer sweeps a highlight band over the placeholder background (GradientToColor is the moving
        // highlight, derived from ColorFill). Each override must show up in the running animation: sample a whole cycle and
        // look for the override colour.
        var marker = Color.FromRgb(0xFF, 0x00, 0x00);
        ShimmerShows(null, marker).ShouldBeFalse("The default shimmer must not contain the marker colour.");
        foreach (var token in s_shimmerTokens.Order(StringComparer.Ordinal))
        {
            ShimmerShows(token, marker).ShouldBeTrue($"Controls[Skeleton].{token} did not reach the running shimmer.");
        }
    }

    private static bool ShimmerShows(string? token, Color marker)
    {
        var builder = new ThemeConfigBuilder();
        if (token is not null)
        {
            builder.WithControl(SkeletonTokens.Identity,
                new ControlThemeConfigBuilder().WithToken(token, "#ffff0000").Build());
        }

        var skeleton = new Skeleton { Width = 300, IsLoading = true, IsActive = true, IsShowParagraph = false };
        var provider = new ThemeConfigProvider { Config = builder.Build(), Child = skeleton };
        var window   = new AvaloniaWindow { Width = 400, Height = 200, Content = provider };
        try
        {
            window.Show();
            FrameCapture.Refresh(window);
            var title = Descendant<SkeletonTitle>(skeleton);
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(1700))
            {
                Thread.Sleep(40);
                AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
                Dispatcher.UIThread.RunJobs();
                using var frame = FrameCapture.Capture(window);
                if (ContainsColor(frame, title, marker))
                {
                    return true;
                }
            }

            return false;
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static bool ContainsColor(SKBitmap frame, Control control, Color color)
    {
        var origin = control.TranslatePoint(new Point(), TopLevel.GetTopLevel(control)!).ShouldNotBeNull();
        for (var y = (int)origin.Y + 2; y < (int)(origin.Y + control.Bounds.Height) - 2; y += 2)
        {
            for (var x = (int)origin.X + 1; x < (int)(origin.X + control.Bounds.Width) - 1; x += 2)
            {
                var pixel = frame.GetPixel(x, y);
                if (Math.Abs(pixel.Red - color.R) < 48 && Math.Abs(pixel.Green - color.G) < 48 &&
                    Math.Abs(pixel.Blue - color.B) < 48)
                {
                    return true;
                }
            }
        }

        return false;
    }

    [Fact]
    public void Skeleton_Loading_Motion_Duration_Drives_The_Shimmer()
    {
        // LoadingMotionDuration is a read-back row; the placeholders take it as their animation duration.
        var skeleton = new Skeleton { Width = 300, IsLoading = true, IsActive = true };
        WithProvider(skeleton, provider =>
        {
            var title = Descendant<SkeletonTitle>(skeleton);
            title.MotionDuration.ShouldBe(TimeSpan.FromSeconds(1.4));
            provider.Config = ControlConfig(SkeletonTokens.Identity, "LoadingMotionDuration", "00:00:02");
            WaitFor(() => title.MotionDuration == TimeSpan.FromSeconds(2));
            title.MotionDuration.ShouldBe(TimeSpan.FromSeconds(2));
            skeleton.MotionDuration.ShouldBe(TimeSpan.FromSeconds(2));
        });
    }
}
