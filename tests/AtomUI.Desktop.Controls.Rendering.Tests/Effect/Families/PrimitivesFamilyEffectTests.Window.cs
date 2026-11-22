using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class PrimitivesFamilyEffectTests
{
    // Native drawing is unavailable in the headless host. Project the real decorations template's visuals as
    // TopLevelHost does, and assert its final binding endpoints under a real scoped Window (controller ruling).
    private static void AssertWindowEndpoint(string token)
    {
        FeedbackFamilyEffectTests.WithProvider(new Border(), provider =>
        {
            var window = new Window { Width = 500, Height = 320, Title = "Native consumer contract", [Window.OsTypeProperty] = OsType.Linux };
            using var lease = ThemeContextLease.Attach(window, ThemeScope.ResolveContext(provider)!);
            var theme = (ControlTheme)new WindowDrawnDecorationsTheme()[typeof(WindowDrawnDecorations)]!;
            var template = (IWindowDrawnDecorationsTemplate)theme.Setters.OfType<Setter>()
                .Single(s => s.Property == WindowDrawnDecorations.TemplateProperty).Value!;
            var content = template.Build().Result;
            var projection = new Panel();
            var visuals = new[] { content.Underlay, content.Overlay };
            content.Underlay = null;
            content.Overlay = null;
            foreach (var visual in visuals)
            {
                if (visual is Control control) { projection.Children.Add(control); }
            }
            var fullScreenTitleBarHost = new FullScreenTitleBarHost();
            projection.Children.Add(fullScreenTitleBarHost);
            window.Content = projection;
            try
            {
                window.Show();
                fullScreenTitleBarHost.ApplyTemplate();
                window.UpdateLayout();
                object? Read() => token switch
                {
                    "SystemBarColor" => TopLevel.GetSystemBarColor(window)?.Color,
                    "TitleBarHeight" => window.TitleBarHeight,
                    "CornerRadius" => window.CornerRadius,
                    "FrameBorderActive" => Frame("PART_LegacyFrameTopBorderActive").Background,
                    "FrameBorderInactive" => Frame("PART_LegacyFrameTopBorderInactive").Background,
                    "FullscreenPopoverShadows" => Frame("PART_Surface").BoxShadow,
                    _ => throw new ArgumentOutOfRangeException(nameof(token))
                };
                Border Frame(string name) => projection.GetVisualDescendants().OfType<Border>().First(b => b.Name == name);
                var before = Read();
                var identity = WindowTokens.Identity;
                var value = token switch
                {
                    "SystemBarColor" or "FrameBorderActive" or "FrameBorderInactive" => "#ff00b96b",
                    "CornerRadius" => "24",
                    "FullscreenPopoverShadows" => "0 12 30 4 #9900b96b",
                    _ => "64"
                };
                provider.Config = FeedbackFamilyEffectTests.WithControlToken(identity, token, value);
                FeedbackFamilyEffectTests.WaitFor(() => !Equals(Read(), before));
                var changed = Read();
                changed.ShouldNotBe(before);
                provider.Config = new ThemeConfigBuilder().Build();
                FeedbackFamilyEffectTests.WaitFor(() => Equals(Read(), before));
                Read().ShouldBe(before);
            }
            finally
            {
                window.Close();
                Dispatcher.UIThread.RunJobs();
            }
        });
    }
}
