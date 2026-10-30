using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class FeedbackCardTransitionThemeTests
{
    static FeedbackCardTransitionThemeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Message_Queue_Transitions_Use_The_Shared_Duration_And_Ant_Easing()
    {
        AssertQueueTransition(new MessageCard
        {
            Message = "Message",
            IsMotionEnabled = true
        }, expectsStackClipTransition: false);
    }

    [Fact]
    public void Notification_Queue_Transitions_Use_The_Shared_Duration_And_Ant_Easing()
    {
        using var manager = new WindowNotificationManager();
        AssertQueueTransition(new NotificationCard(manager)
        {
            Title = "Notification",
            Content = "Content",
            IsMotionEnabled = true
        }, expectsStackClipTransition: true);
    }

    private static void AssertQueueTransition(Control card, bool expectsStackClipTransition)
    {
        var window = new AvaloniaWindow
        {
            Width = 500,
            Height = 300,
            Content = card
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            card.ApplyTemplate();
            window.UpdateLayout();

            var expectedDuration = GetThemeResource<TimeSpan>(SharedTokenKind.MotionDurationMid);
            var transitions = card.Transitions.ShouldNotBeNull();
            var transform = transitions.OfType<TransformOperationsTransition>().Single();
            var opacity = transitions.OfType<DoubleTransition>()
                                     .Single(transition => transition.Property == Visual.OpacityProperty);

            transform.Duration.ShouldBe(expectedDuration);
            opacity.Duration.ShouldBe(expectedDuration);
            AssertAntEasing(transform.Easing);
            AssertAntEasing(opacity.Easing);

            var clipTransitions = transitions.OfType<DoubleTransition>()
                                             .Where(transition =>
                                                 transition.Property == FeedbackStackPanel.StackClipProgressProperty)
                                             .ToArray();
            clipTransitions.Length.ShouldBe(expectsStackClipTransition ? 1 : 0);
            if (expectsStackClipTransition)
            {
                clipTransitions[0].Property.ShouldBe(FeedbackStackPanel.StackClipProgressProperty);
                clipTransitions[0].Duration.ShouldBe(expectedDuration);
                AssertAntEasing(clipTransitions[0].Easing);
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void AssertAntEasing(Easing easing)
    {
        var spline = easing.ShouldBeOfType<SplineEasing>();
        spline.X1.ShouldBe(0.645, 0.0001);
        spline.Y1.ShouldBe(0.045, 0.0001);
        spline.X2.ShouldBe(0.355, 0.0001);
        spline.Y2.ShouldBe(1, 0.0001);
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current;
        application.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }
}
