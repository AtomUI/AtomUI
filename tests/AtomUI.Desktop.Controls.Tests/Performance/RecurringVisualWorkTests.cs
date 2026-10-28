using System.Reflection;
using System.Reactive.Subjects;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.Tests.Motion;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomUICarousel = AtomUI.Desktop.Controls.Carousel;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Performance;

public class RecurringVisualWorkTests
{
    static RecurringVisualWorkTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Infinite_Avalonia_Animations_Pause_When_Effectively_Invisible()
    {
        using var clock = new ManualMotionClock();
        var child = new Border { Width = 20, Height = 20, Opacity = .4 };
        var target = new Border { Width = 80, Height = 60, Child = child };
        using var source = new BehaviorSubject<double>(.8);
        using var binding = target.Bind(Visual.OpacityProperty, source);
        var host = new Border { Child = target };
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(100),
            Children =
            {
                new KeyFrame { Cue = new Cue(0), Setters = { new Setter(Visual.OpacityProperty, 0d) } },
                new KeyFrame { Cue = new Cue(1), Setters = { new Setter(Visual.OpacityProperty, 1d) } }
            }
        };
        ShowInWindow(host, () =>
        {
            using var loop = animation.PlayLoop(target, Visual.OpacityProperty);
            Dispatcher.UIThread.RunJobs();
            clock.Step(0);
            clock.Step(350);
            target.Opacity.ShouldBe(.5, .001, "Skipped frames must preserve the native infinite timeline's phase.");
            child.Opacity.ShouldBe(.4, "An instance loop style must affect only its own host.");
            host.IsVisible = false;
            Dispatcher.UIThread.RunJobs();
            var hidden = target.Opacity;
            clock.Step(550);
            target.Opacity.ShouldBe(hidden);
            host.IsVisible = true;
            Dispatcher.UIThread.RunJobs();
            clock.Step(550);
            clock.Step(575);
            var resumed = target.Opacity;
            clock.Step(625);
            target.Opacity.ShouldNotBe(resumed);
            source.OnNext(.6);
            target.GetBaseValue(Visual.OpacityProperty).Value.ShouldBe(.6);
            loop.Dispose();
            target.Opacity.ShouldBe(.6);
            clock.Step(800);
            target.Opacity.ShouldBe(.6);

            var knob = new SwitchKnob { Width = 24, Height = 24, LoadingAnimationDuration = TimeSpan.FromMilliseconds(100) };
            target.Child = knob;
            Dispatcher.UIThread.RunJobs();
            knob.NotifyStartLoading();
            Dispatcher.UIThread.RunJobs();
            clock.Step(800);
            clock.Step(850);
            knob.LoadingAnimationDuration = TimeSpan.FromMilliseconds(200);
            Dispatcher.UIThread.RunJobs();
            clock.Step(850);
            clock.Step(900);
            knob.Rotation.ShouldBe(90);
            knob.NotifyStopLoading();
            var stopped = knob.Rotation;
            clock.Step(1100);
            knob.Rotation.ShouldBe(stopped);
        });
    }

    [Fact]
    public void Marquee_Animation_Pauses_When_Effectively_Invisible()
    {
        var marquee = new MarqueeLabel();
        SetPrivateField(typeof(AbstractMarqueeLabel), marquee, "_lastTextWidth", 240d);
        InvokePrivateMethod(typeof(AbstractMarqueeLabel), marquee, "ReConfigureAnimation");

        var animation = GetPrivateField<Animation>(typeof(AbstractMarqueeLabel), marquee, "_animation");

        animation.PlaybackBehavior.ShouldBe(PlaybackBehavior.OnlyIfVisible);
    }

    [Fact]
    public void Loading_Icon_Does_Not_Run_When_An_Ancestor_Is_Hidden()
    {
        var icon = new TestIcon
        {
            Width            = 24,
            Height           = 24,
            LoadingAnimation = IconAnimation.Spin
        };
        var host = new Border { Child = icon };

        ShowInWindow(host, () =>
        {
            InvokePrivateMethod<bool>(typeof(Icon), icon, "CanRunLoadingAnimation").ShouldBeTrue();

            host.IsVisible = false;
            Dispatcher.UIThread.RunJobs();

            InvokePrivateMethod<bool>(typeof(Icon), icon, "CanRunLoadingAnimation").ShouldBeFalse();

            host.IsVisible = true;
            Dispatcher.UIThread.RunJobs();

            InvokePrivateMethod<bool>(typeof(Icon), icon, "CanRunLoadingAnimation").ShouldBeTrue();
        });
    }

    [Fact]
    public void Spin_Stops_Compositor_Animations_When_An_Ancestor_Is_Hidden()
    {
        var indicator = new SpinIndicator();
        var host = new Border { Child = indicator };

        ShowInWindow(host, () =>
        {
            GetPrivateFieldValue(typeof(AbstractSpinIndicator), indicator, "_animatedIndicatorTarget")
                .ShouldNotBeNull();

            host.IsVisible = false;
            Dispatcher.UIThread.RunJobs();

            GetPrivateFieldValue(typeof(AbstractSpinIndicator), indicator, "_animatedIndicatorTarget")
                .ShouldBeNull();

            host.IsVisible = true;
            Dispatcher.UIThread.RunJobs();

            GetPrivateFieldValue(typeof(AbstractSpinIndicator), indicator, "_animatedIndicatorTarget")
                .ShouldNotBeNull();
        });
    }

    [Fact]
    public void Carousel_AutoPlay_Timer_Pauses_When_An_Ancestor_Is_Hidden()
    {
        var carousel = new AtomUICarousel
        {
            IsAutoPlay = true,
            ItemsSource = new[] { "First", "Second" }
        };
        var host = new Border { Child = carousel };

        ShowInWindow(host, () =>
        {
            var timer = GetPrivateField<DispatcherTimer>(typeof(AtomUICarousel), carousel, "_autoPlayTimer");
            timer.IsEnabled.ShouldBeTrue();

            host.IsVisible = false;
            Dispatcher.UIThread.RunJobs();

            timer.IsEnabled.ShouldBeFalse();

            host.IsVisible = true;
            Dispatcher.UIThread.RunJobs();

            timer.IsEnabled.ShouldBeTrue();
        });
    }

    [Fact]
    public void Carousel_Progress_Animation_Pauses_When_Effectively_Invisible()
    {
        var indicator = new CarouselPageIndicator
        {
            IsSelected               = true,
            IsShowTransitionProgress = true
        };
        var host = new Border { Child = indicator };

        InvokePrivateMethod(
            typeof(CarouselPageIndicator),
            indicator,
            "BuildProgressAnimation",
            true);

        var animation = GetPrivateField<Animation>(typeof(CarouselPageIndicator), indicator, "_animation");
        animation.PlaybackBehavior.ShouldBe(PlaybackBehavior.OnlyIfVisible);

        ShowInWindow(host, () =>
        {
            GetPrivateFieldValue(typeof(CarouselPageIndicator), indicator, "_cancellationTokenSource")
                .ShouldNotBeNull();

            host.IsVisible = false;
            Dispatcher.UIThread.RunJobs();

            GetPrivateFieldValue(typeof(CarouselPageIndicator), indicator, "_cancellationTokenSource")
                .ShouldBeNull();
            indicator.ProgressValue.ShouldBe(0d);

            host.IsVisible = true;
            Dispatcher.UIThread.RunJobs();

            GetPrivateFieldValue(typeof(CarouselPageIndicator), indicator, "_cancellationTokenSource")
                .ShouldNotBeNull();
        });
    }

    [Fact]
    public void TimerStatistic_Timer_Pauses_When_An_Ancestor_Is_Hidden()
    {
        var statistic = new TimerStatistic
        {
            Value           = DateTime.Now.AddMinutes(1),
            RefreshDuration = TimeSpan.FromSeconds(1)
        };
        var host = new Border { Child = statistic };

        ShowInWindow(host, () =>
        {
            var timer = GetPrivateField<DispatcherTimer>(typeof(TimerStatistic), statistic, "_timer");
            timer.IsEnabled.ShouldBeTrue();

            host.IsVisible = false;
            Dispatcher.UIThread.RunJobs();

            timer.IsEnabled.ShouldBeFalse();

            host.IsVisible = true;
            Dispatcher.UIThread.RunJobs();

            timer.IsEnabled.ShouldBeTrue();
        });
    }

    private static T GetPrivateField<T>(Type ownerType, object instance, string fieldName)
    {
        return GetPrivateFieldValue(ownerType, instance, fieldName).ShouldBeOfType<T>();
    }

    private static object? GetPrivateFieldValue(Type ownerType, object instance, string fieldName)
    {
        var field = ownerType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return field.GetValue(instance);
    }

    private static void SetPrivateField(Type ownerType, object instance, string fieldName, object value)
    {
        var field = ownerType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        field.SetValue(instance, value);
    }

    private static void InvokePrivateMethod(
        Type ownerType,
        object instance,
        string methodName,
        params object?[]? parameters)
    {
        var method = ownerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        method.Invoke(instance, parameters);
    }

    private static T InvokePrivateMethod<T>(Type ownerType, object instance, string methodName)
    {
        var method = ownerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        return method.Invoke(instance, null).ShouldBeOfType<T>();
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 320,
            Height  = 220,
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

    private sealed class TestIcon : Icon
    {
    }
}
