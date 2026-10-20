using AtomUI.Controls.Primitives;
using AtomUI.MotionScene;
using Avalonia;
using Avalonia.Animation.Easings;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class FeedbackCardMotionTests
{
    static FeedbackCardMotionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(NotificationPosition.TopCenter, 0, -64)]
    [InlineData(NotificationPosition.BottomCenter, 0, 64)]
    [InlineData(NotificationPosition.TopLeft, -64, 0)]
    [InlineData(NotificationPosition.BottomLeft, -64, 0)]
    [InlineData(NotificationPosition.TopRight, 64, 0)]
    [InlineData(NotificationPosition.BottomRight, 64, 0)]
    public async Task Enter_Uses_Placement_Aware_Translate_Without_Scale(
        NotificationPosition position,
        double expectedX,
        double expectedY)
    {
        var actor = new MotionActor();
        var startOpacity = 1d;
        var start = Matrix.Identity;
        var capturedStart = false;
        actor.PropertyChanged += (_, args) =>
        {
            if (!capturedStart &&
                args.Property == BaseMotionActor.MotionTransformProperty &&
                actor.MotionTransform is { } transform &&
                (Math.Abs(transform.Value.M31) > 0.001 || Math.Abs(transform.Value.M32) > 0.001))
            {
                startOpacity = actor.Opacity;
                start = transform.Value;
                capturedStart = true;
            }
        };
        var motion = new FeedbackCardMotion(true, position, TimeSpan.Zero);

        await motion.RunAsync(actor, cancellationToken: TestContext.Current.CancellationToken);

        startOpacity.ShouldBe(0, 0.001);
        start.M11.ShouldBe(1, 0.001);
        start.M22.ShouldBe(1, 0.001);
        start.M31.ShouldBe(expectedX, 0.001);
        start.M32.ShouldBe(expectedY, 0.001);
        actor.Opacity.ShouldBe(1, 0.001);
        actor.MotionTransform.ShouldBeNull();
    }

    [Theory]
    [InlineData(NotificationPosition.TopCenter, 0, -64)]
    [InlineData(NotificationPosition.BottomCenter, 0, 64)]
    [InlineData(NotificationPosition.TopLeft, -64, 0)]
    [InlineData(NotificationPosition.BottomLeft, -64, 0)]
    [InlineData(NotificationPosition.TopRight, 64, 0)]
    [InlineData(NotificationPosition.BottomRight, 64, 0)]
    public async Task Exit_Uses_The_Same_Placement_Direction(
        NotificationPosition position,
        double expectedX,
        double expectedY)
    {
        var actor = new MotionActor
        {
            Opacity = 1
        };
        var target = Matrix.Identity;
        actor.PropertyChanged += (_, args) =>
        {
            if (args.Property == BaseMotionActor.MotionTransformProperty &&
                actor.MotionTransform is { } transform &&
                (Math.Abs(transform.Value.M31) > 0.001 || Math.Abs(transform.Value.M32) > 0.001))
            {
                target = transform.Value;
            }
        };
        var motion = new FeedbackCardMotion(false, position, TimeSpan.Zero);

        await motion.RunAsync(actor, cancellationToken: TestContext.Current.CancellationToken);

        target.M11.ShouldBe(1, 0.001);
        target.M22.ShouldBe(1, 0.001);
        target.M31.ShouldBe(expectedX, 0.001);
        target.M32.ShouldBe(expectedY, 0.001);
        actor.Opacity.ShouldBe(0, 0.001);
        actor.MotionTransform.ShouldBeNull();
    }

    [Fact]
    public void Motion_Uses_Ant_Ease_In_Out_For_The_Whole_Duration()
    {
        var duration = TimeSpan.FromMilliseconds(200);
        var motion = new FeedbackCardMotion(true, NotificationPosition.TopCenter, duration);

        motion.Duration.ShouldBe(duration);
        var easing = motion.Easing.ShouldBeOfType<SplineEasing>();
        easing.X1.ShouldBe(0.645, 0.0001);
        easing.Y1.ShouldBe(0.045, 0.0001);
        easing.X2.ShouldBe(0.355, 0.0001);
        easing.Y2.ShouldBe(1, 0.0001);
    }

    [Fact]
    public async Task Cancel_Releases_Transition_Completion_Subscriptions()
    {
        var actor = new MotionActor();
        var motion = new FeedbackCardMotion(
            true,
            NotificationPosition.TopCenter,
            TimeSpan.FromSeconds(1));
        using var cancellation = new CancellationTokenSource();

        var running = motion.RunAsync(actor, cancellationToken: cancellation.Token);
        cancellation.Cancel();
        await Should.ThrowAsync<OperationCanceledException>(async () => await running);

        motion.Transitions.Count.ShouldBe(2);
        foreach (var transition in motion.Transitions)
        {
            Should.Throw<ObjectDisposedException>(() => _ = transition.CompletedObservable);
        }
    }
}
