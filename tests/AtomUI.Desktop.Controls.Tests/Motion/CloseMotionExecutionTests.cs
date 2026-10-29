using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Motion;

public class CloseMotionExecutionTests
{
    static CloseMotionExecutionTests() => AvaloniaTestApp.EnsureInitialized();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Retemplate_During_Pending_Close_Completes_Exactly_Once(bool notification)
    {
        using var scene = new Scene(notification, TimeSpan.Zero, show: false);
        scene.Card.Close();
        scene.Show();
        scene.Card.IsClosed.ShouldBeTrue();
        scene.Closed.ShouldBe(1);
        scene.Surface.IsVisible.ShouldBeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Retemplate_During_Playing_Close_Uses_The_Current_Surface_And_Closes_Once(bool notification)
    {
        using var scene = new Scene(notification, TimeSpan.FromSeconds(1));
        scene.Step(0);
        scene.Step(250);
        scene.Card.Close();
        scene.Pump();
        var previous = scene.Surface;
        previous.IsAnimating(Visual.OpacityProperty).ShouldBeTrue();
        scene.Card.Duration = TimeSpan.Zero;
        scene.Surface = new Border { Width = 320, Height = 96 };
        scene.Card.ApplySurface(scene.Surface);
        scene.Pump();
        scene.Card.IsClosed.ShouldBeTrue();
        scene.Closed.ShouldBe(1);
        scene.Surface.IsVisible.ShouldBeFalse();
        previous.IsAttachedToVisualTree().ShouldBeFalse();
        previous.IsAnimating(Visual.OpacityProperty).ShouldBeFalse();
        MotionTransformProbe.Find(previous).ShouldBeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Disabling_Motion_During_Close_Converges_And_Closes_Once(bool notification)
    {
        using var scene = new Scene(notification, TimeSpan.FromSeconds(1));
        scene.Step(0);
        scene.Step(250);
        scene.Card.Close();
        scene.Pump();
        scene.Surface.IsAnimating(Visual.OpacityProperty).ShouldBeTrue();
        scene.Card.IsMotionEnabled = false;
        scene.Pump();
        scene.Card.IsClosed.ShouldBeTrue();
        scene.Closed.ShouldBe(1);
        scene.Surface.IsAnimating(Visual.OpacityProperty).ShouldBeFalse();
        scene.Surface.IsVisible.ShouldBeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void External_Closed_State_Cancels_Entry_And_Converges_Hidden(bool notification)
    {
        using var scene = new Scene(notification, TimeSpan.FromMinutes(1));
        scene.Surface.IsAnimating(Visual.OpacityProperty).ShouldBeTrue();
        scene.Card.IsClosed = true;
        scene.Surface.IsAnimating(Visual.OpacityProperty).ShouldBeFalse();
        MotionTransformProbe.Find(scene.Surface).ShouldBeNull();
        scene.Surface.IsVisible.ShouldBeFalse();
        scene.Surface.Opacity.ShouldBe(1);
    }

    private interface ITestCard : IDisposable
    {
        TemplatedControl Control { get; }
        bool IsClosed { get; set; }
        bool IsMotionEnabled { get; set; }
        TimeSpan Duration { get; set; }
        event EventHandler<RoutedEventArgs> Closed;
        void Close();
        void ApplySurface(Border surface);
    }

    private sealed class TestMessageCard : MessageCard, ITestCard
    {
        public TemplatedControl Control => this;
        public TimeSpan Duration { get => OpenCloseMotionDuration; set => OpenCloseMotionDuration = value; }
        public event EventHandler<RoutedEventArgs> Closed { add => MessageClosed += value; remove => MessageClosed -= value; }
        public void ApplySurface(Border surface) => SetSurfaceTemplate(this, surface, "PART_Frame");
        public void Dispose() => ReleaseOwner();
    }

    private sealed class TestNotificationCard(WindowNotificationManager manager) : NotificationCard(manager), ITestCard
    {
        public TemplatedControl Control => this;
        public TimeSpan Duration { get => OpenCloseMotionDuration; set => OpenCloseMotionDuration = value; }
        public event EventHandler<RoutedEventArgs> Closed { add => NotificationClosed += value; remove => NotificationClosed -= value; }
        public void ApplySurface(Border surface) => SetSurfaceTemplate(this, surface, "Frame");
        public void Dispose() => ReleaseOwner();
    }

    private static void SetSurfaceTemplate(TemplatedControl card, Border surface, string part)
    {
        surface.Name = part;
        card.Template = new FuncControlTemplate((_, scope) =>
        {
            scope.Register(part, surface);
            return surface;
        });
        card.ApplyTemplate();
    }

    private sealed class Scene : IDisposable
    {
        private readonly ManualMotionClock _clock = new();
        private readonly WindowNotificationManager? _manager;
        internal readonly ITestCard Card;
        internal readonly AvaloniaWindow Window;
        internal Border Surface = new() { Width = 320, Height = 96 };
        internal int Closed;

        internal Scene(bool notification, TimeSpan duration, bool show = true)
        {
            if (notification)
            {
                _manager = new WindowNotificationManager();
                Card = new TestNotificationCard(_manager);
            }
            else
            {
                Card = new TestMessageCard();
            }
            Card.IsMotionEnabled = true;
            Card.Duration = duration;
            Card.Closed += (_, _) => Closed++;
            Window = new AvaloniaWindow { Width = 600, Height = 400, Content = Card.Control };
            if (show)
            {
                Show();
            }
        }

        internal void Show()
        {
            Card.ApplySurface(Surface);
            Window.Show();
            Pump();
        }

        internal void Pump()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
        }

        internal void Step(int milliseconds)
        {
            _clock.Step(milliseconds);
            Pump();
        }

        public void Dispose()
        {
            Card.Dispose();
            Window.Close();
            Pump();
            _manager?.Dispose();
            _clock.Dispose();
        }
    }
}
