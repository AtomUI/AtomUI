using AtomUI.Animations;
using AtomUI.Desktop.Controls.Tests.Motion;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class FeedbackCardMotionTests
{
    static FeedbackCardMotionTests() => AvaloniaTestApp.EnsureInitialized();

    [Theory]
    [InlineData(NotificationPosition.TopCenter, 0, -64)]
    [InlineData(NotificationPosition.BottomCenter, 0, 64)]
    [InlineData(NotificationPosition.TopLeft, -64, 0)]
    [InlineData(NotificationPosition.BottomLeft, -64, 0)]
    [InlineData(NotificationPosition.TopRight, 64, 0)]
    [InlineData(NotificationPosition.BottomRight, 64, 0)]
    public void Enter_Uses_Placement_Aware_Translate_Without_Scale(NotificationPosition position, double x, double y)
    {
        using var clock = new ManualMotionClock();
        var target = new Border { Width = 320, Height = 96 };
        var window = Show(target);
        try
        {
            var playback = new FeedbackCardMotion(true, position, TimeSpan.FromSeconds(1)).PlayAsync(target, TestContext.Current.CancellationToken);
            target.Opacity.ShouldBe(0);
            AssertMatrix(target, x, y);
            clock.Step(0);
            Pump(window);
            clock.Step(1100);
            Pump(window);
            playback.IsCompletedSuccessfully.ShouldBeTrue();
            target.Opacity.ShouldBe(1);
            AssertMatrix(target, 0, 0);
            AtomUI.Animations.Motion.Reset(target);
            target.RenderTransform.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(NotificationPosition.TopCenter, 0, -64)]
    [InlineData(NotificationPosition.BottomCenter, 0, 64)]
    [InlineData(NotificationPosition.TopLeft, -64, 0)]
    [InlineData(NotificationPosition.BottomLeft, -64, 0)]
    [InlineData(NotificationPosition.TopRight, 64, 0)]
    [InlineData(NotificationPosition.BottomRight, 64, 0)]
    public async Task Exit_Uses_The_Same_Placement_Direction(NotificationPosition position, double x, double y)
    {
        var target = new Border { Width = 320, Height = 96 };
        var window = Show(target);
        try
        {
            var result = await new FeedbackCardMotion(false, position, TimeSpan.Zero)
                .PlayAsync(target, TestContext.Current.CancellationToken);
            result.IsCompleted.ShouldBeTrue();
            AssertMatrix(target, x, y);
            target.Opacity.ShouldBe(0);
            AtomUI.Animations.Motion.Reset(target);
            target.RenderTransform.ShouldBeNull();
            target.Opacity.ShouldBe(1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Motion_Uses_Ant_Ease_In_Out_For_The_Whole_Duration()
    {
        var duration = TimeSpan.FromMilliseconds(200);
        var motion = new FeedbackCardMotion(true, NotificationPosition.TopCenter, duration);
        motion.Duration.ShouldBe(duration);
        var easing = motion.Easing.ShouldNotBeNull();
        easing.Ease(0).ShouldBe(0);
        easing.Ease(.5).ShouldBe(.516875, .0001);
        easing.Ease(1).ShouldBe(1);
    }

    [Fact]
    public async Task Cancel_Releases_Animation_Overrides_And_Restores_The_Underlying_Properties()
    {
        using var clock = new ManualMotionClock();
        var target = new Border { Width = 320, Height = 96 };
        var window = Show(target);
        using var cancellation = new CancellationTokenSource();
        try
        {
            var running = new FeedbackCardMotion(true, NotificationPosition.TopCenter, TimeSpan.FromSeconds(1))
                .PlayAsync(target, cancellation.Token);
            cancellation.Cancel();
            (await running).Outcome.ShouldBe(MotionOutcome.Cancelled);
            target.IsAnimating(Visual.OpacityProperty).ShouldBeFalse();
            target.RenderTransform.ShouldBeNull();
            target.Opacity.ShouldBe(1);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertMatrix(Control target, double x, double y)
    {
        var matrix = target.RenderTransform!.Value;
        matrix.M11.ShouldBe(1, .001);
        matrix.M22.ShouldBe(1, .001);
        matrix.M31.ShouldBe(x, .001);
        matrix.M32.ShouldBe(y, .001);
    }

    private static AvaloniaWindow Show(Control target)
    {
        var window = new AvaloniaWindow { Width = 600, Height = 400, Content = target };
        window.Show();
        Pump(window);
        return window;
    }

    private static void Pump(AvaloniaWindow window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }
}
