using System.Runtime.CompilerServices;
using AtomUI.Desktop.Controls.Tests.Motion;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class FeedbackCardMotionCoordinatorTests
{
    static FeedbackCardMotionCoordinatorTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Dispose_Releases_The_Current_Target_Overrides_Immediately()
    {
        using var scene = new Scene();
        scene.Enter();
        scene.Target.IsAnimating(Visual.OpacityProperty).ShouldBeTrue();
        scene.Coordinator.Dispose();
        scene.Target.IsAnimating(Visual.OpacityProperty).ShouldBeFalse();
        scene.Target.RenderTransform.ShouldBeNull();
        scene.Target.Opacity.ShouldBe(1);
    }

    [Fact]
    public void Attached_Entry_Keeps_The_Prepare_State_Until_The_First_Clock_Tick()
    {
        using var scene = new Scene();
        scene.Enter(NotificationPosition.TopRight);
        scene.Target.Opacity.ShouldBe(0);
        scene.Target.RenderTransform!.Value.M31.ShouldBe(64, .001);
        scene.Step(0);
        scene.Target.Opacity.ShouldBe(0);
        scene.Step(1000);
        scene.Target.Opacity.ShouldBeGreaterThan(0);
        scene.Target.Opacity.ShouldBeLessThan(1);
    }

    [Theory]
    [InlineData(NotificationPosition.TopCenter, 0, -64)]
    [InlineData(NotificationPosition.BottomCenter, 0, 64)]
    [InlineData(NotificationPosition.TopLeft, -64, 0)]
    [InlineData(NotificationPosition.BottomLeft, -64, 0)]
    [InlineData(NotificationPosition.TopRight, 64, 0)]
    [InlineData(NotificationPosition.BottomRight, 64, 0)]
    public void Closing_During_Entry_Is_Continuous_And_Completes_Exactly_Once(
        NotificationPosition position, double x, double y)
    {
        using var scene = new Scene();
        scene.Enter(position);
        scene.Step(0);
        scene.Step(2500);
        var opacity = scene.Target.Opacity;
        var matrix = scene.Target.RenderTransform!.Value;
        scene.Coordinator.StartClose(false, true, position, scene.Duration);
        scene.Pump();
        scene.Target.Opacity.ShouldBe(opacity, .001);
        scene.Target.RenderTransform!.Value.ShouldBe(matrix);
        scene.Step(2500);
        scene.Step(12400);
        scene.Closed.ShouldBe(0);
        scene.Clock.Step(12600);
        scene.Window.UpdateLayout();
        scene.Target.Opacity.ShouldBe(0);
        var endpoint = scene.Target.RenderTransform!.Value;
        endpoint.M31.ShouldBe(x, .001);
        endpoint.M32.ShouldBe(y, .001);
        scene.Pump();
        scene.Closed.ShouldBe(1);
        scene.Target.IsVisible.ShouldBeFalse();
        scene.Target.RenderTransform.ShouldBeNull();
        scene.Target.IsAnimating(Visual.OpacityProperty).ShouldBeFalse();
        scene.Step(15000);
        scene.Closed.ShouldBe(1);
    }

    [Fact]
    public void DetachTarget_Preserves_The_Hidden_State_For_A_Closed_Card()
    {
        var target = new Border();
        using var coordinator = new FeedbackCardMotionCoordinator(() => { });
        coordinator.ApplyTarget(target, false, false, false, NotificationPosition.TopCenter, TimeSpan.Zero);
        coordinator.DetachTarget(false, true);
        target.IsVisible.ShouldBeFalse();
        target.Opacity.ShouldBe(1);
        target.RenderTransform.ShouldBeNull();
    }

    [Fact]
    public void Detach_Before_Entry_Completes_Does_Not_Suppress_The_Next_Attached_Entry()
    {
        using var scene = new Scene();
        scene.Enter();
        scene.Coordinator.DetachTarget(false, false);
        scene.Enter(NotificationPosition.TopRight);
        scene.Target.IsAnimating(Visual.OpacityProperty).ShouldBeTrue();
        scene.Target.Opacity.ShouldBe(0);
        scene.Target.RenderTransform!.Value.M31.ShouldBe(64, .001);
    }

    [Fact]
    public void Dispose_During_Entry_Releases_The_Coordinator_And_Target_Graph()
    {
        var references = CreateDisposedMotionGraph(false);
        ForceFullCollection();
        references.Coordinator.IsAlive.ShouldBeFalse();
        references.Target.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Dispose_Before_The_First_Clock_Tick_Releases_The_Owner_Graph()
    {
        var references = CreateDisposedMotionGraph(true);
        ForceFullCollection();
        references.Coordinator.IsAlive.ShouldBeFalse();
        references.Target.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Configuration_Change_During_Entry_Does_Not_Restart_The_Current_Target()
    {
        using var scene = new Scene();
        scene.Enter();
        scene.Step(0);
        scene.Step(1000);
        var transform = scene.Target.RenderTransform;
        var opacity = scene.Target.Opacity;
        scene.Coordinator.UpdateConfiguration(false, false, true, NotificationPosition.BottomCenter, TimeSpan.FromSeconds(2));
        scene.Pump();
        scene.Target.RenderTransform.ShouldBeSameAs(transform);
        scene.Target.Opacity.ShouldBe(opacity);
    }

    [Fact]
    public void Configuration_Change_During_Exit_Does_Not_Restart_The_Current_Target()
    {
        using var scene = new Scene();
        scene.Coordinator.ApplyTarget(scene.Target, false, false, false, NotificationPosition.TopCenter, scene.Duration);
        scene.Coordinator.StartClose(false, true, NotificationPosition.TopCenter, scene.Duration);
        scene.Pump();
        scene.Step(0);
        scene.Step(1000);
        var transform = scene.Target.RenderTransform;
        var opacity = scene.Target.Opacity;
        scene.Coordinator.UpdateConfiguration(true, false, true, NotificationPosition.BottomCenter, TimeSpan.FromSeconds(2));
        scene.Pump();
        scene.Target.RenderTransform.ShouldBeSameAs(transform);
        scene.Target.Opacity.ShouldBe(opacity);
        scene.Step(11000);
        scene.Closed.ShouldBe(1);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Coordinator, WeakReference Target) CreateDisposedMotionGraph(bool attached)
    {
        using var clock = new ManualMotionClock();
        var target = new Border { Width = 320, Height = 96 };
        var coordinator = new FeedbackCardMotionCoordinator(() => { });
        AvaloniaWindow? window = null;
        if (attached)
        {
            window = new AvaloniaWindow { Width = 600, Height = 400, Content = target };
            window.Show();
            window.UpdateLayout();
        }
        coordinator.ApplyTarget(target, false, false, true, NotificationPosition.TopRight, TimeSpan.FromMinutes(1));
        Dispatcher.UIThread.RunJobs();
        var references = (new WeakReference(coordinator), new WeakReference(target));
        coordinator.Dispose();
        if (window is not null)
        {
            window.Content = null;
            window.Close();
        }
        Dispatcher.UIThread.RunJobs();
        return references;
    }

    private static void ForceFullCollection()
    {
        for (var i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Yield();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private sealed class Scene : IDisposable
    {
        internal readonly ManualMotionClock Clock = new();
        internal readonly Border Target = new() { Width = 320, Height = 96 };
        internal readonly AvaloniaWindow Window;
        internal readonly FeedbackCardMotionCoordinator Coordinator;
        internal readonly TimeSpan Duration = TimeSpan.FromSeconds(10);
        internal int Closed;

        internal Scene()
        {
            Window = new AvaloniaWindow { Width = 600, Height = 400, Content = Target };
            Coordinator = new FeedbackCardMotionCoordinator(() => Closed++);
            Window.Show();
            Pump();
        }

        internal void Enter(NotificationPosition position = NotificationPosition.TopCenter)
        {
            Coordinator.ApplyTarget(Target, false, false, true, position, Duration);
            Pump();
        }

        internal void Pump()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
        }

        internal void Step(int milliseconds)
        {
            Clock.Step(milliseconds);
            Pump();
        }

        public void Dispose()
        {
            Coordinator.Dispose();
            Window.Close();
            Pump();
            Clock.Dispose();
        }
    }
}
