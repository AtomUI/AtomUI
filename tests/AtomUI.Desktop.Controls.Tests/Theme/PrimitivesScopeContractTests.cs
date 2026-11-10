using System.Diagnostics;
using System.Reflection;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Embedding;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomWindow = AtomUI.Desktop.Controls.Window;
using HostWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Theme;

public class PrimitivesScopeContractTests
{
    public PrimitivesScopeContractTests() { AvaloniaTestApp.EnsureInitialized(); }

    [Fact]
    public void Wave_Uses_Owner_Painter_And_Actual_Animation_Endpoints_With_Publish_Restore_And_Detach()
    {
        var wave = new WaveSpiritDecorator
        {
            Width = 100, Height = 40, IsMotionEnabled = true, IsWaveSpiritEnabled = true, WaveType = WaveSpiritType.RoundRectWave
        };
        var owner = new ArrowDecoratedBox { Content = wave };
        var provider = new ThemeConfigProvider { Config = new ThemeConfigBuilder().Build(), Child = owner };
        var window = new HostWindow { Content = provider, Width = 300, Height = 200 };
        try
        {
            window.Show();
            window.UpdateLayout();
            ThemeScope.GetTokenOwner(wave).ShouldBe(ArrowDecoratedBoxTokens.Identity);
            wave.Play();
            wave.IsPlaying.ShouldBeTrue();
            var painter = typeof(WaveSpiritDecorator).GetField("_wavePainter", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(wave).ShouldBeOfType<RoundRectWavePainter>();
            var baselineBrush = ((ISolidColorBrush)painter.WaveBrush!).Color;
            var baselineRange = painter.WaveRange;
            var baselineOpacity = painter.OriginOpacity;
            var baselineDuration = painter.SizeMotionDuration;
            provider.Config = new ThemeConfigBuilder().WithControl(ArrowDecoratedBoxTokens.Identity, new ControlThemeConfigBuilder()
                .WithToken("ColorPrimary", "#ff00b96b").WithToken("WaveAnimationRange", "7")
                .WithToken("WaveStartOpacity", "0.6").WithToken("MotionDurationSlow", "00:00:01").Build()).Build();
            Wait(() => painter.WaveRange == 7 && painter.OriginOpacity == .6 && painter.SizeMotionDuration == TimeSpan.FromSeconds(1));
            ((ISolidColorBrush)painter.WaveBrush!).Color.ShouldBe(Color.Parse("#ff00b96b"));
            wave.IsPlaying.ShouldBeFalse();
            var sizeAnimation = new Animation();
            painter.NotifyBuildSizeAnimation(sizeAnimation, WaveSpiritDecorator.WidthProperty);
            sizeAnimation.Duration.ShouldBe(TimeSpan.FromSeconds(1));
            sizeAnimation.Children.Last().Setters.Single().Value.ShouldBe(painter.OriginSize + new Size(14, 14));
            var opacityAnimation = new Animation();
            painter.NotifyBuildOpacityAnimation(opacityAnimation, WaveSpiritDecorator.OpacityProperty);
            opacityAnimation.Duration.ShouldBe(TimeSpan.FromMilliseconds(1050));
            opacityAnimation.Children.First().Setters.Single().Value.ShouldBe(.6);
            opacityAnimation.Children.Last().Setters.Single().Value.ShouldBe(0d);
            provider.Config = new ThemeConfigBuilder().Build();
            Wait(() => painter.WaveRange == baselineRange && painter.OriginOpacity == baselineOpacity);
            ((ISolidColorBrush)painter.WaveBrush!).Color.ShouldBe(baselineBrush);
            painter.SizeMotionDuration.ShouldBe(baselineDuration);
            wave.Play();
            wave.IsPlaying.ShouldBeTrue();
            owner.Content = null;
            wave.IsPlaying.ShouldBeFalse();
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Window_Opening_Uses_Scoped_Own_Background_And_Publish_Then_Releases_Its_Lease()
    {
        var anchor = new Border();
        var provider = new ThemeConfigProvider { Config = new ThemeConfigBuilder().Build(), Child = anchor };
        var host = new HostWindow { Content = provider };
        var window = new AtomWindow();
        try
        {
            host.Show();
            provider.Config = new ThemeConfigBuilder().WithControl(WindowTokens.Identity,
                new ControlThemeConfigBuilder().WithToken("DefaultBackground", "#ff00b96b").Build()).Build();
            Wait(() => ControlTokenAccessor.Capture<AtomWindow>(anchor).GetOwn<Color>(WindowTokenKind.DefaultBackground) == Color.Parse("#ff00b96b"));
            using var lease = ThemeContextLease.Attach(window, ThemeScope.ResolveContext(provider)!);
            Color? opening = null;
            window.Opened += (_, _) => opening = ((ISolidColorBrush)window.Background!).Color;
            window.Show();
            opening.ShouldBe(Color.Parse("#ff00b96b"));
            provider.Config = new ThemeConfigBuilder().WithControl(WindowTokens.Identity,
                new ControlThemeConfigBuilder().WithToken("DefaultBackground", "#ff102030").Build()).Build();
            Wait(() => ((ISolidColorBrush)window.Background!).Color == Color.Parse("#ff102030"));
            ((ISolidColorBrush)window.TransparencyBackgroundFallback!).Color.ShouldBe(Color.Parse("#ff102030"));
        }
        finally { window.Close(); host.Close(); Dispatcher.UIThread.RunJobs(); }
        window.Resources.MergedDictionaries.OfType<ThemeContextResourceBridge>().ShouldBeEmpty();
    }

    [Fact]
    public void Actual_Embedding_Root_Uses_Scoped_Global_Endpoints_And_Publish_Restore()
    {
        var provider = new ThemeConfigProvider { Config = new ThemeConfigBuilder().Build(), Child = new Border() };
        var host = new HostWindow { Content = provider };
        // Use the real injectable TopLevel constructor: headless has no native embedding factory.
        using var root = new EmbeddableControlRoot(new HostWindow().PlatformImpl!);
        try
        {
            host.Show();
            using var lease = ThemeContextLease.Attach(root, ThemeScope.ResolveContext(provider)!);
            root.TryFindResource(typeof(EmbeddableControlRoot), out var resource).ShouldBeTrue();
            root.Theme = resource.ShouldBeOfType<ControlTheme>();
            root.Prepare();
            var before = ((ISolidColorBrush)root.Background!).Color;
            provider.Config = new ThemeConfigBuilder().WithToken("ColorBgContainer", "#ff00b96b")
                .WithToken("ColorText", "#ff112233").WithToken("FontSize", "22").WithToken("FontFamily", "monospace").Build();
            Wait(() => ((ISolidColorBrush)root.Background!).Color == Color.Parse("#ff00b96b"));
            ((ISolidColorBrush)root.TransparencyBackgroundFallback!).Color.ShouldBe(Color.Parse("#ff00b96b"));
            TopLevel.GetSystemBarColor(root)!.Color.ShouldBe(Color.Parse("#ff00b96b"));
            ((ISolidColorBrush)root.Foreground!).Color.ShouldBe(Color.Parse("#ff112233"));
            root.FontSize.ShouldBe(22);
            root.FontFamily.Name.ShouldBe("monospace");
            provider.Config = new ThemeConfigBuilder().Build();
            Wait(() => ((ISolidColorBrush)root.Background!).Color == before);
        }
        finally { host.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private static void Wait(Func<bool> condition)
    {
        var watch = Stopwatch.StartNew();
        while (!condition() && watch.Elapsed < TimeSpan.FromSeconds(10))
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }
        condition().ShouldBeTrue();
    }
}
