using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomButton = AtomUI.Desktop.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Motion duration Tokens are TimeSpan values: their effect is the Duration of the transitions the theme installs, read back
/// from the Control's <see cref="Animatable.Transitions"/> (theme-effect-tests.md: TimeSpan Tokens are not compared by
/// pixels). The theme declares the durations with <c>atom:TokenTransitions</c>, so they follow the Control's effective
/// Token in its local <see cref="ThemeConfigProvider"/> scope.
/// </summary>
public partial class ButtonFamilyEffectTests
{
    private static readonly TimeSpan s_defaultMotionDurationMid = TimeSpan.FromMilliseconds(200);
    private const string OverrideDuration = "00:00:00.5000000";

    [Fact]
    public void Controls_Button_MotionDurationMid_Changes_Button_Transitions_But_Not_A_Tag()
    {
        var button = new AtomButton { Content = "Button" };
        var tag    = new CheckableTag { Content = "Tag" };
        WithMotionPair(button, tag, provider =>
        {
            BackgroundDuration(button).ShouldBe(s_defaultMotionDurationMid);
            BackgroundDuration(tag).ShouldBe(s_defaultMotionDurationMid);

            provider.Config = MotionConfig(control: ButtonTokens.Identity);
            WaitUntil(() => BackgroundDuration(button) != s_defaultMotionDurationMid);

            BackgroundDuration(button).ShouldBe(TimeSpan.Parse(OverrideDuration));
            BackgroundDuration(tag).ShouldBe(s_defaultMotionDurationMid);

            // Removing the override leaves no stale duration behind.
            provider.Config = MotionConfig();
            WaitUntil(() => BackgroundDuration(button) == s_defaultMotionDurationMid);

            BackgroundDuration(button).ShouldBe(s_defaultMotionDurationMid);
        });
    }

    [Fact]
    public void Root_MotionDurationMid_Changes_Both_The_Button_And_The_Tag()
    {
        var button = new AtomButton { Content = "Button" };
        var tag    = new CheckableTag { Content = "Tag" };
        WithMotionPair(button, tag, provider =>
        {
            provider.Config = MotionConfig(root: true);
            WaitUntil(() => BackgroundDuration(button) != s_defaultMotionDurationMid &&
                            BackgroundDuration(tag) != s_defaultMotionDurationMid);

            BackgroundDuration(button).ShouldBe(TimeSpan.Parse(OverrideDuration));
            BackgroundDuration(tag).ShouldBe(TimeSpan.Parse(OverrideDuration));
        });
    }

    [Fact]
    public void Root_EnableMotion_False_Zeroes_The_Button_Transition_Durations()
    {
        // The local value keeps the theme's [IsMotionEnabled=True] transitions installed, so the duration that
        // EnableMotion=false normalizes to zero is observable on them.
        var button = new AtomButton { Content = "Button", IsMotionEnabled = true };
        WithMotionPair(button, new Border(), provider =>
        {
            BackgroundDuration(button).ShouldBe(s_defaultMotionDurationMid);

            provider.Config = new ThemeConfigBuilder().WithToken(nameof(SharedTokenKind.EnableMotion), "false").Build();
            WaitUntil(() => BackgroundDuration(button) == TimeSpan.Zero);

            button.Transitions.ShouldNotBeNull().ShouldAllBe(transition =>
                ((TransitionBase)transition).Duration == TimeSpan.Zero);
        });
    }

    /// <summary>A config change is applied asynchronously; waits until it reached the Controls (or times out).</summary>
    private static void WaitUntil(Func<bool> condition)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(10))
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }
            Thread.Sleep(1);
        }
    }

    private static ThemeConfig MotionConfig(Theme.Schema.ControlTokenIdentity? control = null, bool root = false)
    {
        var builder = new ThemeConfigBuilder();
        if (root)
        {
            builder.WithToken(nameof(SharedTokenKind.MotionDurationMid), OverrideDuration);
        }
        if (control is { } identity)
        {
            builder.WithControl(identity,
                new ControlThemeConfigBuilder().WithToken(nameof(SharedTokenKind.MotionDurationMid), OverrideDuration)
                                               .Build());
        }

        return builder.Build();
    }

    private static TimeSpan BackgroundDuration(Animatable control)
    {
        var transitions = control.Transitions.ShouldNotBeNull($"{control.GetType().Name} has no transitions installed.");
        return transitions.OfType<TransitionBase>()
                          .Single(transition => transition.Property == TemplatedControlBackground(control))
                          .Duration;
    }

    private static AvaloniaProperty TemplatedControlBackground(Animatable control) =>
        control is AtomButton ? AtomButton.BackgroundProperty : CheckableTag.BackgroundProperty;

    private static void WithMotionPair(Control first, Control second, Action<ThemeConfigProvider> body)
    {
        var provider = new ThemeConfigProvider
        {
            Config = MotionConfig(),
            Child = new StackPanel
            {
                Orientation         = Orientation.Horizontal,
                Spacing             = 24,
                Margin              = new Thickness(24),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment   = VerticalAlignment.Top,
                Children            = { first, second }
            }
        };
        ThemeChangeFailedEventArgs? failure = null;
        provider.ThemeChangeFailed += (_, args) => failure = args;
        var window = new AvaloniaWindow { Width = 600, Height = 300, Content = provider };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            body(provider);
            failure.ShouldBeNull(failure?.Exception?.Message);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
