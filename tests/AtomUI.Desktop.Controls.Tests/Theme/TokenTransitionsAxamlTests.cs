using System.Diagnostics;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Theme;

[Collection(ControlEffectiveGlobalBindingTestCollection.Name)]
public class TokenTransitionsAxamlTests
{
    private static readonly TimeSpan s_mid = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan s_slow = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan s_override = TimeSpan.FromMilliseconds(500);

    static TokenTransitionsAxamlTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Plain_Transitions_Setter_Value_Is_One_Instance_Shared_By_All_Targets()
    {
        // Design evidence: the compiled Setter holds one Transitions object and hands it to every styled control,
        // so a duration written on it (or bound on it) cannot differ per control.
        WithView(null, view =>
        {
            var first = Frame(view, "PlainFirst").Transitions.ShouldNotBeNull();
            Frame(view, "PlainSecond").Transitions.ShouldBeSameAs(first);
        });
    }

    [Fact]
    public void Compiled_Axaml_Resolves_Declared_Durations_In_Setter_And_Template()
    {
        WithView(null, view =>
        {
            var owned = Frame(view, "OwnedFrame").Transitions.ShouldNotBeNull();
            Duration(owned, Border.BackgroundProperty).ShouldBe(s_mid);
            Duration(owned, Visual.OpacityProperty).ShouldBe(s_slow);
            Duration(owned, Decorator.PaddingProperty).ShouldBe(TimeSpan.FromMilliseconds(70),
                "a transition without a declared Token keeps its written duration");

            Duration(TemplateFrame(view).Transitions.ShouldNotBeNull(), Visual.RenderTransformProperty).ShouldBe(s_mid);

            // Targets that resolve the same durations share one built instance.
            Frame(view, "UnownedFrame").Transitions.ShouldBeSameAs(owned);
            Frame(view, "SecondUnownedFrame").Transitions.ShouldBeSameAs(owned);
        });
    }

    [Fact]
    public void Controls_Button_Override_Reaches_Only_Transitions_Owned_By_The_Button()
    {
        var provider = new ThemeConfigProvider();
        WithView(provider, view =>
        {
            provider.Config = ButtonMotionConfig();
            WaitUntil(() => Duration(Frame(view, "OwnedFrame").Transitions!, Border.BackgroundProperty) == s_override);

            var owned = Frame(view, "OwnedFrame").Transitions.ShouldNotBeNull();
            Duration(owned, Border.BackgroundProperty).ShouldBe(s_override);
            Duration(owned, Visual.OpacityProperty).ShouldBe(s_slow);
            Duration(TemplateFrame(view).Transitions.ShouldNotBeNull(), Visual.RenderTransformProperty)
                .ShouldBe(s_override);
            var unowned = Frame(view, "UnownedFrame").Transitions.ShouldNotBeNull();
            unowned.ShouldNotBeSameAs(owned);
            Duration(unowned, Border.BackgroundProperty).ShouldBe(s_mid);

            provider.Config = new ThemeConfigBuilder().Build();
            WaitUntil(() => Duration(Frame(view, "OwnedFrame").Transitions!, Border.BackgroundProperty) == s_mid);

            Duration(Frame(view, "OwnedFrame").Transitions!, Border.BackgroundProperty).ShouldBe(s_mid);
            Frame(view, "OwnedFrame").Transitions.ShouldBeSameAs(Frame(view, "UnownedFrame").Transitions);
        });
    }

    private static ThemeConfig ButtonMotionConfig() =>
        new ThemeConfigBuilder()
            .WithControl(
                ButtonTokens.Identity,
                new ControlThemeConfigBuilder()
                    .WithToken(nameof(SharedTokenKind.MotionDurationMid), "00:00:00.5000000")
                    .Build())
            .Build();

    private static void WithView(ThemeConfigProvider? provider, Action<TokenTransitionsView> body)
    {
        var view = new TokenTransitionsView();
        Control content = view;
        if (provider is not null)
        {
            provider.Child = view;
            provider.ThemeChangeFailed += static (_, args) =>
                throw new InvalidOperationException(args.Exception?.Message ?? "Theme compilation failed.");
            content = provider;
        }

        var window = new AvaloniaWindow { Width = 240, Height = 240, Content = content };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            body(view);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void WaitUntil(Func<bool> condition)
    {
        var stopwatch = Stopwatch.StartNew();
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

    private static Border Frame(Control view, string name) => view.FindControl<Border>(name).ShouldNotBeNull();

    private static Border TemplateFrame(Control view) =>
        view.FindControl<ContentControl>("TemplatedHost")
            .ShouldNotBeNull()
            .GetVisualDescendants()
            .OfType<Border>()
            .Single(border => border.Name == "PART_Frame");

    private static TimeSpan Duration(Transitions transitions, AvaloniaProperty property) =>
        transitions.OfType<TransitionBase>().Single(transition => transition.Property == property).Duration;
}
