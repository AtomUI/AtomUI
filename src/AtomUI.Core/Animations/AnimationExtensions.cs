using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia.Threading;

namespace AtomUI.Animations;

internal static class AnimationExtensions
{
    internal static IDisposable PlayLoop<T>(this Animation animation, StyledElement target, AvaloniaProperty<T> property)
    {
        Dispatcher.UIThread.VerifyAccess();
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(property);
        if (animation.Duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(animation.Duration));
        }
        if (animation.Duration == TimeSpan.Zero)
        {
            return Disposable.Empty;
        }
        foreach (var frame in animation.Children)
        {
            foreach (var setter in frame.Setters)
            {
                if (setter.Property != property)
                {
                    throw new ArgumentException("A feedback loop must animate only its declared property.", nameof(animation));
                }
            }
        }
        animation.IterationCount = IterationCount.Infinite;
        animation.PlaybackBehavior = PlaybackBehavior.OnlyIfVisible;
        // A selector-less instance style matches its own host only. The native
        // infinite timeline keeps elapsed phase and owns all frame subscriptions.
        var style = new Style { Animations = { animation } };
        var subscription = Disposable.Create(() =>
        {
            Dispatcher.UIThread.VerifyAccess();
            // Release the native style's binding inside a separate owned cover,
            // then leave its styling batch before restoring the latest base value.
            using var cover = property is StyledProperty<T> styledProperty
                ? target.SetValue(styledProperty, target.GetValue(styledProperty), BindingPriority.Animation)
                : null;
            target.Styles.Remove(style);
        });
        try
        {
            target.Styles.Add(style);
            return subscription;
        }
        catch (Exception startFailure)
        {
            try
            {
                subscription.Dispose();
            }
            catch (Exception cleanupFailure)
            {
                throw new AggregateException(startFailure, cleanupFailure);
            }
            throw;
        }
    }
}
