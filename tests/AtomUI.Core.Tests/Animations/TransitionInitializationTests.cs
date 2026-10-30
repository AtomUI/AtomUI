using System.Reactive.Subjects;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Animations;

public class TransitionInitializationTests
{
    [Fact]
    public void Suppression_And_Registration_Restore_Live_Bindings_Across_Reentry()
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

            void RevokeDuringConstruction(object? sender, AvaloniaPropertyChangedEventArgs change)
            {
                if (change.Property == Animatable.TransitionsProperty && target.Transitions is null)
                {
                    TransitionInitialization.SetMode(target, TransitionInitializationMode.None);
                }
            }
            target.PropertyChanged += RevokeDuringConstruction;
            TransitionInitialization.SetMode(target, TransitionInitializationMode.Owner);
            target.PropertyChanged -= RevokeDuringConstruction;
            TransitionInitialization.GetMode(target).ShouldBe(TransitionInitializationMode.None);
            target.Transitions.ShouldBeSameAs(original);

            TransitionInitialization.SetMode(target, TransitionInitializationMode.Owner);
            source.OnNext(latest);
            void ReplaceDuringDisposal(object? sender, AvaloniaPropertyChangedEventArgs change)
            {
                if (change.Property == Animatable.TransitionsProperty && target.Transitions is not null)
                {
                    target.PropertyChanged -= ReplaceDuringDisposal;
                    TransitionInitialization.SetMode(target, TransitionInitializationMode.Owner);
                }
            }
            target.PropertyChanged += ReplaceDuringDisposal;
            TransitionInitialization.SetMode(target, TransitionInitializationMode.None);
            TransitionInitialization.GetMode(target).ShouldBe(TransitionInitializationMode.Owner);
            TransitionInitialization.SetMode(target, TransitionInitializationMode.None);
            target.Transitions.ShouldBeSameAs(latest);
        });
    }

    [Fact]
    public void Initialization_And_Remount_Snap_While_The_First_Subsequent_Change_Animates()
    {
        HeadlessTestApp.Run(() =>
        {
            var target = new TransitionTarget();
            target.BeginInit();
            target.EndInit();
            target.Initialization.CompleteInitialization();
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
                target.Initialization.Dispose();
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
            using var motion = new TransitionInitializationSession(target);
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
                if (release)
                {
                    AssertReleasedOwnerCannotCaptureTarget();
                }
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

    private static void AssertReleasedOwnerCannotCaptureTarget()
    {
        var ownerTarget = new Border();
        var transitions = CreateTransitions();
        var target = new Border { Transitions = transitions };
        using var owner = new TransitionInitializationSession(ownerTarget);
        using var child = new TransitionInitializationSession(target);
        var window = new Window { Content = new StackPanel { Children = { ownerTarget, target } } };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            owner.CompleteInitialization();
            child.CompleteInitialization();
            target.PropertyChanged += (_, change) =>
            {
                if (change.Property == Animatable.TransitionsProperty && target.Transitions is null)
                {
                    owner.Dispose();
                }
            };
            owner.RegisterTarget(child);
            child.CompleteInitialization();
            target.Transitions.ShouldBeSameAs(transitions);
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class TransitionTarget : Border
    {
        internal TransitionInitializationSession Initialization { get; }
        internal double InitialOpacity { get; set; } = 0.2;

        public TransitionTarget()
        {
            Initialization = new TransitionInitializationSession(this);
            Width = 20;
            Height = 20;
            Transitions = CreateTransitions();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            Initialization.Reset();
            base.OnDetachedFromVisualTree(e);
        }

        protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
        {
            base.OnLoaded(e);
            Opacity = InitialOpacity;
            Initialization.CompleteInitialization();
        }
    }
}
