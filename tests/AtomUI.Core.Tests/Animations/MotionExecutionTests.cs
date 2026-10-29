using AtomUI.Animations;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Rendering.Composition;
using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Animations;

public class MotionExecutionTests
{
    [Fact]
    public async Task Parallel_Motion_Waits_For_The_Longest_Child_Before_Completing()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            var target = new Button { Width = 100, Height = 80, Padding = new Thickness(4) };
            var window = new Window { Width = 240, Height = 180, Content = target };
            var firstCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            target.PropertyChanged += (_, change) =>
            {
                if (change.Property == Visual.OpacityProperty && target.Opacity == 0)
                {
                    firstCompleted.TrySetResult();
                }
            };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                var motion = new ParallelMotion
                {
                    Children =
                    {
                        new FadeOutMotion(TimeSpan.FromMilliseconds(30)),
                        new ScaleMotion(TimeSpan.FromMilliseconds(150))
                    }
                };
                var playing = motion.PlayAsync(target, TestContext.Current.CancellationToken);
                await firstCompleted.Task;
                playing.IsCompleted.ShouldBeFalse();
                (await playing).IsCompleted.ShouldBeTrue();
                target.Opacity.ShouldBe(0);
                ElementComposition.GetElementVisual(target)!.Scale.Y.ShouldBe(.8, .001);
                target.Padding.ShouldBe(new Thickness(8));
                Motion.Reset(target);
                ElementComposition.GetElementVisual(target)!.Scale.Y.ShouldBe(1);
                target.Opacity.ShouldBe(1);
                target.Padding.ShouldBe(new Thickness(4));

                using var source = new BehaviorSubject<double>(.8);
                using var binding = target.Bind(Visual.OpacityProperty, source);
                var instant = await new FadeOutMotion(TimeSpan.Zero).PlayAsync(target, TestContext.Current.CancellationToken);
                instant.IsCompleted.ShouldBeTrue();
                instant.WasAnimated.ShouldBeFalse();
                source.OnNext(.6);
                Motion.Reset(target);
                target.Opacity.ShouldBe(.6);

                using var cancellation = new CancellationTokenSource();
                void CancelAtTheEnd(object? sender, AvaloniaPropertyChangedEventArgs change)
                {
                    if (change.Property == Visual.OpacityProperty && target.Opacity == 0)
                    {
                        cancellation.Cancel();
                    }
                }
                target.PropertyChanged += CancelAtTheEnd;
                try
                {
                    var cancelled = await new FadeOutMotion(TimeSpan.Zero).PlayAsync(target, cancellation.Token);
                    cancelled.Outcome.ShouldBe(MotionOutcome.Cancelled);
                    target.Opacity.ShouldBe(.6);
                }
                finally
                {
                    target.PropertyChanged -= CancelAtTheEnd;
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    private sealed class ScaleMotion(TimeSpan duration) : AbstractMotion(duration)
    {
        protected override void Build(MotionBuilder builder, in MotionContext context)
        {
            builder.ScaleY.From(1).To(.8);
            builder.Property(Avalonia.Controls.Primitives.TemplatedControl.PaddingProperty,
                static (Thickness from, Thickness to, double progress) =>
                    new Thickness(from.Left + (to.Left - from.Left) * progress))
                .FromCurrent().To(new Thickness(8));
        }
    }
}
