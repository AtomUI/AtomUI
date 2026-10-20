using System.Reactive.Subjects;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Animations;

public class StateMotionContractTests
{
    [Fact]
    public void Suppression_Restores_Live_Bindings_With_Interleaved_And_Repeated_Disposal()
    {
        HeadlessTestApp.Run(() =>
        {
            var target = new Border();
            var original = CreateTransitions();
            var latest = CreateTransitions();
            using var source = new BehaviorSubject<Transitions?>(original);
            using var binding = target.Bind(Animatable.TransitionsProperty, source);
            var outer = new TransitionSuppressionScope(target);
            var inner = new TransitionSuppressionScope(target);
            source.OnNext(latest);
            outer.Dispose();
            target.Transitions.ShouldBeNull();
            inner.Dispose();
            target.Transitions.ShouldBeSameAs(latest);
            using (var current = new TransitionSuppressionScope(target))
            {
                outer.Dispose();
                inner.Dispose();
                target.Transitions.ShouldBeNull();
            }
            source.OnNext(original);
            target.Transitions.ShouldBeSameAs(original);
        });
    }

    [Fact]
    public void Initialization_And_Remount_Snap_While_The_First_Subsequent_Change_Animates()
    {
        HeadlessTestApp.Run(() =>
        {
            var target = new MotionTarget();
            target.BeginInit();
            target.EndInit();
            target.Motion.CompleteInitialization();
            target.Transitions.ShouldBeNull();
            var window = new Window { Content = target };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                target.Opacity.ShouldBe(0.2);
                target.Opacity = 0.6;
                target.Opacity.ShouldBe(0.2);
                window.Content = null;
                target.InitialOpacity = 0.8;
                window.Content = target;
                Dispatcher.UIThread.RunJobs();
                target.Opacity.ShouldBe(0.8);
                target.Opacity = 0.4;
                target.Opacity.ShouldBe(0.8);
                target.GetBaseValue(Visual.OpacityProperty).Value.ShouldBe(0.4);
            }
            finally
            {
                window.Close();
                target.Motion.Dispose();
            }
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reset_Owns_Its_Suppression_Across_Reentrant_Property_Notifications(bool release)
    {
        HeadlessTestApp.Run(() =>
        {
            var target = new Border { Transitions = CreateTransitions() };
            using var motion = new StateMotionSession(target);
            var window = new Window { Content = target };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                motion.CompleteInitialization();
                target.PropertyChanged += (_, change) =>
                {
                    if (change.Property == Animatable.TransitionsProperty && target.Transitions is null)
                    {
                        if (release)
                        {
                            motion.Dispose();
                        }
                        else
                        {
                            motion.CompleteInitialization();
                        }
                    }
                };
                motion.Reset();
                motion.CompleteInitialization();
                target.Transitions.ShouldNotBeNull();
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static Transitions CreateTransitions()
    {
        return new Transitions
        {
            new DoubleTransition { Property = Visual.OpacityProperty, Duration = TimeSpan.FromSeconds(1) }
        };
    }

    private sealed class MotionTarget : Border
    {
        internal StateMotionSession Motion { get; }
        internal double InitialOpacity { get; set; } = 0.2;

        public MotionTarget()
        {
            Motion = new StateMotionSession(this);
            Width = 20;
            Height = 20;
            Transitions = CreateTransitions();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            Motion.Reset();
            base.OnDetachedFromVisualTree(e);
        }

        protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
        {
            base.OnLoaded(e);
            Opacity = InitialOpacity;
            Motion.CompleteInitialization();
        }
    }
}
