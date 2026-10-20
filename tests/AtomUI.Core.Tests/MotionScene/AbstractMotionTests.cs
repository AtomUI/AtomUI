using AtomUI.MotionScene;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.MotionScene;

public class AbstractMotionTests
{
    [Fact]
    public async Task Transition_Motion_Waits_For_All_Transitions_Before_Completing()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            var actor = new TestMotionActor
            {
                Content = new Border
                {
                    Width  = 100,
                    Height = 80
                }
            };
            var window = new Window
            {
                Width   = 240,
                Height  = 180,
                Content = actor
            };

            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();

                var motion = new UnevenTransitionMotion(
                    TimeSpan.FromMilliseconds(30),
                    TimeSpan.FromMilliseconds(150));
                var motionTask = motion.RunAsync(actor);
                await motion.FirstTransitionCompleted;
                motionTask.IsCompleted.ShouldBeFalse();
                await motionTask;

                actor.Opacity.ShouldBe(0);
                actor.Animating.ShouldBeFalse();
            }
            finally
            {
                window.Close();
            }
        });
    }

    private sealed class TestMotionActor : BaseMotionActor;

    private sealed class UnevenTransitionMotion : AbstractMotion
    {
        private readonly TimeSpan _transformDuration;
        private readonly TaskCompletionSource<bool> _firstTransitionCompleted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task FirstTransitionCompleted => _firstTransitionCompleted.Task;

        public UnevenTransitionMotion(TimeSpan duration, TimeSpan transformDuration)
            : base(duration)
        {
            _transformDuration = transformDuration;
        }

        protected override void ConfigureTransitions()
        {
            base.ConfigureTransitions();
            Transitions[0].TransitionCompleted += (_, _) =>
                _firstTransitionCompleted.TrySetResult(true);
            ((TransitionBase)Transitions[1]).Duration = _transformDuration;
        }

        protected override void ConfigureMotionStartValue(BaseMotionActor actor)
        {
            actor.Opacity         = 1;
            actor.MotionTransform = BuildScaleYTransform(1);
        }

        protected override void ConfigureMotionEndValue(BaseMotionActor actor)
        {
            actor.Opacity         = 0;
            actor.MotionTransform = BuildScaleYTransform(0.8);
        }
    }
}
