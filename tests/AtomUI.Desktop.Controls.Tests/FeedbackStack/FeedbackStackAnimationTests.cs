using System.Reflection;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class FeedbackStackAnimationTests
{
    static FeedbackStackAnimationTests() => AvaloniaTestApp.EnsureInitialized();

    [Theory]
    [InlineData(NotificationPosition.TopRight)]
    [InlineData(NotificationPosition.BottomRight)]
    public void Collapse_First_Frame_Preserves_The_Expanded_Card_And_Clip(NotificationPosition position)
    {
        using var scene = new AnimationScene(position);
        var card = scene.BackCard;
        var initialTransform = card.RenderTransform!.Value;
        var initialClip = ((RectangleGeometry)card.Clip!).Rect;

        scene.Collapse();

        card.RenderTransform!.Value.ShouldBe(initialTransform);
        card.GetValue(FeedbackStackPanel.StackClipProgressProperty).ShouldBe(0);
        ((RectangleGeometry)card.Clip!).Rect.ShouldBe(initialClip,
            "The transition's base target must not clip the still-expanded first frame.");
    }

    [Theory]
    [InlineData(NotificationPosition.TopRight)]
    [InlineData(NotificationPosition.BottomRight)]
    public void Collapse_Clip_Remains_At_Its_Target_After_The_Animation_Completes(NotificationPosition position)
    {
        using var scene = new AnimationScene(position);
        scene.Collapse();
        scene.Step(0);
        scene.Step(100);
        scene.Step(220);
        var targetTransform = scene.BackCard.RenderTransform!.Value;

        foreach (var time in new[] { 240, 320, 440, 600 })
        {
            scene.Step(time);

            scene.Panel.IsCollapsed.ShouldBeTrue();
            scene.BackCard.RenderTransform!.Value.ShouldBe(targetTransform);
            scene.BackCard.GetValue(FeedbackStackPanel.StackClipProgressProperty).ShouldBe(0.5,
                "Completing the clip transition must not restore the property's default and start a reverse transition.");
            scene.AssertClipMatchesProgress();
        }
    }

    [Theory]
    [InlineData(NotificationPosition.TopRight)]
    [InlineData(NotificationPosition.BottomRight)]
    public void Enabling_Motion_After_An_Immediate_Expansion_Keeps_The_Card_Fully_Visible(NotificationPosition position)
    {
        using var scene = new AnimationScene(position);
        scene.Collapse();
        scene.Step(0);
        scene.Step(220);
        scene.Step(440);
        scene.SetMotion(false);
        scene.Expand();
        var expandedClip = ((RectangleGeometry)scene.BackCard.Clip!).Rect;

        scene.SetMotion(true);

        foreach (var time in new[] { 440, 500, 660, 880 })
        {
            scene.Step(time);
            scene.Panel.IsCollapsed.ShouldBeFalse();
            ((RectangleGeometry)scene.BackCard.Clip!).Rect.ShouldBe(expandedClip,
                "Enabling motion must not replay a stale collapsed clip over an already-expanded card.");
        }
    }

    [Theory]
    [InlineData(NotificationPosition.TopRight)]
    [InlineData(NotificationPosition.BottomRight)]
    public void Reenabling_Stack_After_Disabling_Motion_Does_Not_Restore_A_Stale_Clip(NotificationPosition position)
    {
        using var scene = new AnimationScene(position);
        scene.Collapse();
        scene.Step(0);
        scene.Step(220);
        scene.Step(440);
        scene.SetMotion(false);
        scene.SetStack(false);
        scene.Expand();
        scene.SetMotion(true);

        scene.SetStack(true);

        foreach (var time in new[] { 440, 500, 660, 880 })
        {
            scene.Step(time);
            scene.Panel.IsCollapsed.ShouldBeFalse();
            var clip = ((RectangleGeometry)scene.BackCard.Clip!).Rect;
            clip.Top.ShouldBe(-48);
            clip.Bottom.ShouldBe(scene.BackCard.Bounds.Height + 48);
        }
    }

    private sealed class AnimationScene : IDisposable
    {
        private static readonly Type ClockType = typeof(Animation).Assembly.GetType("Avalonia.Animation.ClockBase", true)!;
        private static readonly MethodInfo Pulse = ClockType.GetMethod("Pulse", BindingFlags.Instance | BindingFlags.NonPublic)!;
        private readonly object _clock = Activator.CreateInstance(ClockType, nonPublic: true)!;
        private readonly WindowNotificationManager _manager;
        private readonly AvaloniaWindow _window;
        private readonly FeedbackStackPresenter _presenter;
        private readonly NotificationPosition _position;

        public FeedbackStackPanel Panel { get; }
        public NotificationCard BackCard => _manager.Cards[^2];

        public AnimationScene(NotificationPosition position)
        {
            _position = position;
            _manager = new WindowNotificationManager
            {
                IsStackEnabled = true,
                IsMotionEnabled = false,
                Position = position
            };
            _window = new AvaloniaWindow { Width = 1000, Height = 800, Content = _manager };
            typeof(Animatable).GetProperty("Clock", BindingFlags.Instance | BindingFlags.NonPublic)!
                              .SetValue(_window, _clock);
            _window.Show();
            Pump();
            for (var i = 1; i <= 4; i++)
            {
                _manager.Show(new Notification($"Notification {i}",
                    i % 2 == 0
                        ? $"Notification {i}: This is a deliberately longer stacked notification used to verify variable-height cards."
                        : $"Notification {i}: This is a stacked notification.",
                    expiration: TimeSpan.Zero));
            }
            Pump();
            _presenter = _manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            Panel = _presenter.GetVisualDescendants().OfType<FeedbackStackPanel>().Single();
            _presenter.SetPointerOverState(true);
            Pump();
            _manager.IsMotionEnabled = true;
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

        public void SetMotion(bool enabled)
        {
            _manager.IsMotionEnabled = enabled;
            Pump();
        }

        public void SetStack(bool enabled)
        {
            _manager.IsStackEnabled = enabled;
            Pump();
        }

        public void Step(int milliseconds)
        {
            Pulse.Invoke(_clock, [TimeSpan.FromMilliseconds(milliseconds)]);
            Pump();
        }

        public void AssertClipMatchesProgress()
        {
            var clip = ((RectangleGeometry)BackCard.Clip!).Rect;
            if (_position == NotificationPosition.TopRight)
                clip.Top.ShouldBe(BackCard.Bounds.Height / 2, 0.001);
            else
                clip.Bottom.ShouldBe(BackCard.Bounds.Height / 2, 0.001);
        }

        private void Pump()
        {
            Dispatcher.UIThread.RunJobs();
            _window.UpdateLayout();
        }

        public void Dispose()
        {
            _manager.Dispose();
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
