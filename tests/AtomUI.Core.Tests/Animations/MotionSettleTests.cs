using AtomUI.Animations;
using AtomUI.Core.Tests.Theme;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Animations;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class MotionSettleTests
{
    private static readonly TimeSpan LongMotion = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task Theme_Commit_Lands_Every_Animation_It_Starts_Before_The_First_Frame_Is_Committed()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            var manager = new ThemeManagerBuilder().Build();
            manager.InitializeApplication(Application.Current!);
            var probe = new ArrangeProbe();
            var target = new TintedBorder { Child = probe };
            var window = new Window { Width = 400, Height = 300, Content = target };
            try
            {
                window.Show();
                Frame();
                var initialTint = target.Tint.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;
                probe.Opacity.ShouldBe(1);

                var keyframeRuns = new List<Task>();
                manager.ThemeChanged += (_, _) =>
                {
                    // ThemeChanged handlers run inside the commit: a local value and a keyframe animation.
                    target.Opacity = 0.5;
                    keyframeRuns.Add(CreateWidthAnimation().RunAsync(target));
                };

                var result = await manager.ApplyThemeAsync(
                    new ThemeRequest(
                        IThemeManager.DEFAULT_THEME_ID,
                        new ThemeConfigBuilder()
                            .WithToken(nameof(DesignToken.ColorPrimary), "#00b96b")
                            .Build(),
                        ThemeTransitionReason.UserRequest),
                    TestContext.Current.CancellationToken);
                result.Status.ShouldBe(ThemeTransitionStatus.Committed);

                // Before any frame: the resource-driven transition (style priority), the local-value transition
                // and the keyframe animation are already on their final values.
                var tint = target.Tint.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;
                tint.ShouldBe(Color.Parse("#00b96b"));
                tint.ShouldNotBe(initialTint);
                target.Opacity.ShouldBe(0.5);
                target.Width.ShouldBe(200);
                await Task.WhenAll(keyframeRuns).WaitAsync(TimeSpan.FromSeconds(1));

                // The first frame lays the new width out, and the transition that layout starts lands in it too.
                Frame();
                target.Bounds.Width.ShouldBe(200);
                probe.Opacity.ShouldBe(0.25);

                // Once that frame is done, animations keep their duration again.
                Frame();
                target.Opacity = 1;
                Frame();
                Frame();
                target.Opacity.ShouldBeLessThan(0.6);
            }
            finally
            {
                window.Close();
                manager.Dispose();
            }
        });
    }

    [Fact]
    public async Task Settle_Completes_Only_Animations_Started_Inside_The_Window()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            var running = new TintedBorder();
            var started = new TintedBorder();
            var faded = new TintedBorder();
            var window = new Window
            {
                Width = 400,
                Height = 300,
                Content = new StackPanel { Children = { running, started, faded } }
            };
            try
            {
                window.Show();
                Frame();
                running.Opacity = 0.5;
                Frame();

                var settle = new MotionSettle();
                var scope = settle.Begin();
                started.Opacity = 0.2;
                var fading = new FadeOutMotion(LongMotion).PlayAsync(faded, TestContext.Current.CancellationToken);
                settle.SettleStarted();

                started.Opacity.ShouldBe(0.2);
                running.Opacity.ShouldBeGreaterThan(0.6);
                for (var frame = 0; frame < 5 && !fading.IsCompleted; frame++)
                {
                    Frame();
                    await Task.Yield();
                }
                (await fading.WaitAsync(TimeSpan.FromSeconds(1))).IsCompleted.ShouldBeTrue();
                faded.Opacity.ShouldBe(0);

                settle.EndAfterNextFrame(scope);
                Dispatcher.UIThread.RunJobs();
                settle.IsSettling.ShouldBeTrue();
                Frame();
                settle.IsSettling.ShouldBeFalse();

                started.Opacity = 1;
                Frame();
                Frame();
                started.Opacity.ShouldBeLessThan(0.6);
                running.Opacity.ShouldBeGreaterThan(0.5);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Transition_Started_By_Layout_Lands_In_The_Same_Frame()
    {
        HeadlessTestApp.Run(() =>
        {
            var probe = new ArrangeProbe { HorizontalAlignment = HorizontalAlignment.Left, Width = 100 };
            var window = new Window { Width = 400, Height = 300, Content = probe };
            try
            {
                window.Show();
                Frame();
                probe.Opacity.ShouldBe(1);

                var settle = new MotionSettle();
                var scope = settle.Begin();
                probe.Width = 200;
                settle.SettleStarted();
                settle.EndAfterNextFrame(scope);
                probe.Opacity.ShouldBe(1);

                Frame();
                probe.Bounds.Width.ShouldBe(200);
                probe.Opacity.ShouldBe(0.25);

                // Without a window the same layout change animates.
                Frame();
                settle.IsSettling.ShouldBeFalse();
                probe.Width = 100;
                Frame();
                probe.Opacity.ShouldBeLessThan(0.4);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void Frame()
    {
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
    }

    private static Animation CreateWidthAnimation() => new()
    {
        Duration = LongMotion,
        FillMode = FillMode.Forward,
        Children =
        {
            new KeyFrame
            {
                Cue = new Cue(1),
                Setters = { new Setter(Layoutable.WidthProperty, 200d) }
            }
        }
    };

    private sealed class TintedBorder : Border
    {
        public static readonly StyledProperty<IBrush?> TintProperty =
            AvaloniaProperty.Register<TintedBorder, IBrush?>(nameof(Tint));

        public IBrush? Tint
        {
            get => GetValue(TintProperty);
            set => SetValue(TintProperty, value);
        }

        public TintedBorder()
        {
            Width = 100;
            Height = 100;
            Transitions =
            [
                new SolidColorBrushTransition { Property = TintProperty, Duration = LongMotion },
                new DoubleTransition { Property = OpacityProperty, Duration = LongMotion }
            ];
            // Mirrors a theme style setter bound to a token.
            TokenResourceBinder.CreateGlobalTokenBinding(this, TintProperty, SharedTokenKind.ColorPrimary, BindingPriority.Style);
        }
    }

    private sealed class ArrangeProbe : Control
    {
        public ArrangeProbe()
        {
            Transitions = [new DoubleTransition { Property = OpacityProperty, Duration = LongMotion }];
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Opacity = finalSize.Width >= 200 ? 0.25 : 1;
            return base.ArrangeOverride(finalSize);
        }
    }
}
