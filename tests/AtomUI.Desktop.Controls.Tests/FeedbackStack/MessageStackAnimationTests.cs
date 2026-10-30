using System.Reflection;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class MessageStackAnimationTests
{
    static MessageStackAnimationTests() => AvaloniaTestApp.EnsureInitialized();

    [Theory]
    [InlineData(NotificationPosition.TopCenter)]
    [InlineData(NotificationPosition.BottomCenter)]
    public void Expanded_Backplates_Keep_The_Latest_Card_Width(NotificationPosition position)
    {
        using var scene = new AnimationScene(position);

        scene.AssertBackplateWidths();
        scene.Collapse();
        foreach (var time in new[] { 0, 40, 100, 220, 400 })
        {
            scene.Step(time);
            scene.AssertBackplateWidths();
        }
    }

    [Theory]
    [InlineData(NotificationPosition.TopCenter)]
    [InlineData(NotificationPosition.BottomCenter)]
    public void Backplates_Fade_And_Slide_In_While_The_Older_Messages_Are_Still_Collapsing(
        NotificationPosition position)
    {
        using var scene = new AnimationScene(position);
        scene.Collapse();
        scene.Step(0);
        scene.Backplates.ShouldAllBe(plate => plate.Opacity == 0);
        scene.Step(50);

        foreach (var plate in scene.Backplates)
        {
            plate.Opacity.ShouldBeInRange(0.01, 0.99);
            Math.Abs(plate.RenderTransform!.Value.M32).ShouldBeInRange(0.01, 15.99);
            plate.IsHitTestVisible.ShouldBeFalse();
        }
        scene.Step(110);
        scene.Backplates.ShouldAllBe(plate => plate.Opacity == 1);
        scene.Backplates.ShouldAllBe(plate => plate.RenderTransform!.Value.M32 == 0);
        scene.Manager.Cards[^2].Opacity.ShouldBeGreaterThan(0);
        scene.AssertBackplatePositions();

        scene.Step(220);
        scene.Manager.Cards[^2].Opacity.ShouldBe(0);
        scene.AssertBackplateWidths();
        scene.AssertBackplatePositions();
    }

    [Theory]
    [InlineData(NotificationPosition.TopCenter)]
    [InlineData(NotificationPosition.BottomCenter)]
    public void Quick_Hover_Reversal_Continues_From_The_Current_Backplate_Frame(NotificationPosition position)
    {
        using var scene = new AnimationScene(position);
        scene.Collapse();
        scene.Step(0);
        scene.Step(40);
        var plate = scene.Backplates[0];
        var opacity = plate.Opacity;
        var transform = plate.RenderTransform!.Value;

        scene.Expand();
        plate.Opacity.ShouldBe(opacity);
        plate.RenderTransform!.Value.ShouldBe(transform);
        scene.Step(40);
        scene.Step(70);
        plate.Opacity.ShouldBeLessThan(opacity);
        opacity = plate.Opacity;
        transform = plate.RenderTransform!.Value;

        scene.Collapse();
        plate.Opacity.ShouldBe(opacity);
        plate.RenderTransform!.Value.ShouldBe(transform);
        scene.Step(70);
        scene.Step(180);
        plate.Opacity.ShouldBe(1);
        scene.AssertBackplateWidths();
        scene.AssertBackplatePositions();
    }

    [Theory]
    [InlineData(NotificationPosition.TopCenter)]
    [InlineData(NotificationPosition.BottomCenter)]
    public void Changing_Expanded_Message_Width_Prepares_Backplates_Before_The_Next_Collapse(
        NotificationPosition position)
    {
        using var scene = new AnimationScene(position);
        var latest = scene.Manager.Cards[^1];
        foreach (var (message, time) in new[] { ("Short message", 0), ("A much longer message replacing the short content while expanded", 600) })
        {
            latest.Message = message;
            scene.Expand();
            scene.Step(time);
            scene.Step(time + 320);
            scene.AssertBackplateWidths();
            scene.Collapse();
            scene.Step(time + 320);
            scene.Step(time + 370);
            scene.AssertBackplateWidths();
            scene.Step(time + 540);
            scene.AssertBackplatePositions();
        }
    }

    [Theory]
    [InlineData(NotificationPosition.TopCenter)]
    [InlineData(NotificationPosition.BottomCenter)]
    public void Motion_And_Stack_Toggles_Apply_The_Backplate_Target_Immediately(NotificationPosition position)
    {
        using var scene = new AnimationScene(position);
        scene.Collapse();
        scene.Step(0);
        scene.Step(40);
        scene.Manager.IsMotionEnabled = false;
        scene.Step(40);
        scene.Backplates.ShouldAllBe(plate => plate.Opacity == 1 && plate.Transitions == null);
        scene.AssertBackplatePositions();

        scene.Manager.IsStackEnabled = false;
        scene.Step(40);
        scene.Backplates.ShouldAllBe(plate => plate.Opacity == 0);
        scene.Manager.IsStackEnabled = true;
        scene.Step(40);
        scene.Backplates.ShouldAllBe(plate => plate.Opacity == 1);
        scene.Expand();
        scene.Manager.IsMotionEnabled = true;
        scene.Step(40);
        scene.Step(400);
        scene.Backplates.ShouldAllBe(plate => plate.Opacity == 0);
        scene.AssertBackplateWidths();
    }

    [Theory]
    [InlineData(NotificationPosition.TopCenter)]
    [InlineData(NotificationPosition.BottomCenter)]
    public void Hidden_Backplates_Do_Not_Enlarge_Empty_Or_Expanded_Queues(NotificationPosition position)
    {
        using var scene = new AnimationScene(position);
        scene.Manager.IsMotionEnabled = false;
        scene.Manager.DestroyAll();
        scene.Step(0);
        scene.AssertQueueExtent(0);
        for (var count = 1; count <= 4; count++)
        {
            scene.Manager.Show(new Message($"Message {count}", expiration: TimeSpan.Zero));
            scene.Expand();
            scene.Step(0);
            scene.AssertQueueExtent(scene.Manager.Cards.Sum(card => card.Bounds.Height) + (count - 1) * 16);
            scene.Backplates.ShouldAllBe(plate => plate.Opacity == 0 && !plate.IsHitTestVisible);
        }
    }

    private sealed class AnimationScene : IDisposable
    {
        private static readonly Type ClockType = typeof(Animation).Assembly.GetType("Avalonia.Animation.ClockBase", true)!;
        private static readonly MethodInfo Pulse = ClockType.GetMethod("Pulse", BindingFlags.Instance | BindingFlags.NonPublic)!;
        private readonly object _clock = Activator.CreateInstance(ClockType, nonPublic: true)!;
        private readonly AvaloniaWindow _window;
        private readonly FeedbackStackPresenter _presenter;
        private readonly NotificationPosition _position;

        public WindowMessageManager Manager { get; }
        public Border[] Backplates { get; }

        public AnimationScene(NotificationPosition position)
        {
            _position = position;
            Manager = new WindowMessageManager(null)
            {
                IsStackEnabled = true,
                IsMotionEnabled = false,
                Position = position
            };
            _window = new AvaloniaWindow { Width = 1000, Height = 800, Content = Manager };
            typeof(Animatable).GetProperty("Clock", BindingFlags.Instance | BindingFlags.NonPublic)!
                              .SetValue(_window, _clock);
            _window.Show();
            Pump();
            for (var i = 1; i <= 4; i++)
            {
                Manager.Show(new Message(i % 2 == 0
                    ? $"Message {i}: This is a slightly longer stacked message."
                    : $"Message {i}: This is a stacked message.", expiration: TimeSpan.Zero));
            }
            Pump();
            _presenter = Manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            Backplates = Manager.GetVisualDescendants().OfType<Border>()
                                .Where(plate => plate.Name is "PART_FirstBackplate" or "PART_SecondBackplate")
                                .OrderBy(plate => plate.Name).ToArray();
            _presenter.SetPointerOverState(true);
            Pump();
            Manager.IsMotionEnabled = true;
            Pump();
            Step(0);
        }

        public void Collapse()
        {
            _presenter.SetPointerOverState(false);
            Pump();
        }

        public void Expand()
        {
            _presenter.SetPointerOverState(true);
            Pump();
        }

        public void Step(int milliseconds)
        {
            Pulse.Invoke(_clock, [TimeSpan.FromMilliseconds(milliseconds)]);
            Pump();
        }

        public void AssertBackplateWidths()
        {
            var latestWidth = Manager.Cards[^1].Bounds.Width;
            latestWidth.ShouldBeGreaterThan(100);
            Backplates[0].Width.ShouldBe(latestWidth - 16, 0.001,
                "Hover must not reset the backplate width and restart a 300 ms expansion on collapse.");
            Backplates[1].Width.ShouldBe(latestWidth - 32, 0.001);
        }

        public void AssertQueueExtent(double height)
        {
            var host = Manager.GetVisualDescendants().OfType<Avalonia.Controls.Grid>()
                              .Single(grid => grid.Name == "PART_StackHost");
            host.Bounds.Height.ShouldBe(height, 0.01);
            _presenter.Bounds.Height.ShouldBe(height, 0.01);
        }

        public void AssertBackplatePositions()
        {
            var latest = Manager.Cards[^1];
            var cardOrigin = latest.TranslatePoint(default, _window)!.Value;
            for (var index = 0; index < Backplates.Length; index++)
            {
                var plate = Backplates[index];
                var origin = plate.TranslatePoint(default, _window)!.Value;
                (origin.X + plate.Bounds.Width / 2).ShouldBe(cardOrigin.X + latest.Bounds.Width / 2, 0.01);
                var expectedY = _position == NotificationPosition.TopCenter
                    ? cardOrigin.Y + latest.Bounds.Height - 8 + index * 8
                    : cardOrigin.Y - 8 - index * 8;
                origin.Y.ShouldBe(expectedY, 0.01);
                plate.Bounds.Height.ShouldBe(16);
            }
        }

        private void Pump()
        {
            Dispatcher.UIThread.RunJobs();
            _window.UpdateLayout();
        }

        public void Dispose()
        {
            Manager.Dispose();
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
