using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.Tests.Motion;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomDialog = AtomUI.Desktop.Controls.Dialog;
using AtomWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Dialog;

[Collection(DialogLifecycleTestCollection.Name)]
public class OverlayDialogPresenterMotionTests
{
    static OverlayDialogPresenterMotionTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Opening_Holds_The_First_Frame_And_Completes_On_The_Framework_Clock()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            using var scene = new Scene();
            var opening = scene.BeginShow();
            scene.Target.Opacity.ShouldBe(0);
            MotionTransformProbe.Matrix(scene.Target).M11.ShouldBe(.2, .001);
            scene.Step(0);
            scene.Target.Opacity.ShouldBe(0);
            scene.Step(500);
            opening.IsCompleted.ShouldBeFalse();
            scene.Target.Opacity.ShouldBeGreaterThan(0);
            scene.Target.Opacity.ShouldBeLessThan(1);
            scene.Step(1100);
            opening.IsCompletedSuccessfully.ShouldBeTrue();
            scene.Target.Opacity.ShouldBe(1);
            MotionTransformProbe.Matrix(scene.Target).M11.ShouldBe(1);
            scene.Mask.Opacity.ShouldBe(1);
        });
    }

    [Fact]
    public void Closing_Holds_The_Host_Until_The_Last_Frame_Then_Releases_Overrides()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            using var scene = new Scene();
            scene.BeginShow();
            scene.Step(0);
            scene.Step(1100);
            var target = scene.Target;
            var closing = scene.Presenter.CloseAsync().AsTask();
            scene.Pump();
            scene.Step(1100);
            scene.Step(1600);
            closing.IsCompleted.ShouldBeFalse();
            scene.Presenter.Parent.ShouldNotBeNull();
            target.Opacity.ShouldBeGreaterThan(0);
            target.Opacity.ShouldBeLessThan(1);
            scene.Step(2200);
            closing.IsCompletedSuccessfully.ShouldBeTrue();
            scene.Presenter.Parent.ShouldBeNull();
            MotionTransformProbe.Find(target).ShouldBeNull();
            target.Opacity.ShouldBe(1);
        });
    }

    [Fact]
    public void Unanchored_Entry_Zooms_From_The_Actual_Surface_Center()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            using var scene = new Scene(anchored: false);
            scene.BeginShow();
            var surface = scene.Presenter.Surface;
            var center = surface.TranslatePoint(new Point(surface.Bounds.Width / 2, surface.Bounds.Height / 2), scene.Target)!.Value;
            MotionTransformProbe.Origin(scene.Target).X.ShouldBe(center.X, .001);
            MotionTransformProbe.Origin(scene.Target).Y.ShouldBe(center.Y, .001);
            MotionTransformProbe.Matrix(scene.Target).M11.ShouldBe(.2, .001);
            scene.Step(0);
            scene.Step(1100);
            scene.Target.Opacity.ShouldBe(1);
        });
    }

    [Fact]
    public void Closing_Preserves_The_Opening_Origin_When_The_Trigger_Moves()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            using var scene = new Scene();
            scene.BeginShow();
            scene.Step(0);
            scene.Step(1100);
            var target = scene.Target;
            var origin = MotionTransformProbe.Origin(target);
            scene.Placement.Margin = new Thickness(40, 20, 0, 0);
            scene.Pump();
            scene.Presenter.CloseAsync();
            MotionTransformProbe.Origin(target).ShouldBe(origin);
            scene.Step(1100);
            scene.Step(2200);
        });
    }

    [Fact]
    public void Closing_During_Entry_Preserves_The_Presented_Scale_And_Opacity()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            using var scene = new Scene();
            var opening = scene.BeginShow();
            scene.Step(0);
            scene.Step(250);
            var target = scene.Target;
            var matrix = MotionTransformProbe.Matrix(target);
            var opacity = target.Opacity;
            var closing = scene.Presenter.CloseAsync().AsTask();
            scene.Pump();
            MotionTransformProbe.Matrix(target).ShouldBe(matrix);
            target.Opacity.ShouldBe(opacity, .001);
            scene.Step(250);
            scene.Step(1350);
            opening.IsCompletedSuccessfully.ShouldBeTrue();
            closing.IsCompletedSuccessfully.ShouldBeTrue();
            scene.Presenter.Parent.ShouldBeNull();
            MotionTransformProbe.Find(target).ShouldBeNull();
        });
    }

    private sealed class Scene : IDisposable
    {
        private readonly ManualMotionClock _clock = new();
        internal readonly Border Placement = new() { Width = 80, Height = 32 };
        internal readonly AtomWindow Window;
        internal readonly OverlayDialogPresenter Presenter;
        internal Border Target => Presenter.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_SurfaceMotionRoot");
        internal OverlayDialogMask Mask => Presenter.GetVisualDescendants().OfType<OverlayDialogMask>().Single();

        internal Scene(bool anchored = true)
        {
            Window = new AtomWindow
            {
                Width = 480, Height = 360,
                Content = new ScopeAwareOverlayLayerPanel { Children = { Placement } }
            };
            var dialog = new AtomDialog
            {
                Content = "Dialog", HostWidth = 240, HostHeight = 160,
                IsMotionEnabled = true, IsModal = true, PlacementTarget = Placement,
                MotionAnchorMode = anchored ? DialogMotionAnchorMode.ExplicitPlacementTarget : DialogMotionAnchorMode.FallbackPlacementTarget
            };
            Presenter = new OverlayDialogPresenter(dialog, Placement)
            {
                OpeningMotionDuration = TimeSpan.FromSeconds(1), MotionDuration = TimeSpan.FromSeconds(1)
            };
            Window.Show();
            Pump();
        }

        internal Task BeginShow()
        {
            var task = Presenter.ShowAsync(TestContext.Current.CancellationToken).AsTask();
            Pump();
            return task;
        }

        internal void Step(int milliseconds)
        {
            _clock.Step(milliseconds);
            Pump();
        }

        internal void Pump()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
        }

        public void Dispose()
        {
            Presenter.IsMotionEnabled = false;
            var disposal = Presenter.DisposeAsync().AsTask();
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);
            while (!disposal.IsCompleted && DateTime.UtcNow < deadline)
            {
                Pump();
                Thread.Sleep(1);
            }
            disposal.IsCompletedSuccessfully.ShouldBeTrue(disposal.Exception?.ToString());
            Window.Close();
            Pump();
            _clock.Dispose();
        }
    }
}
