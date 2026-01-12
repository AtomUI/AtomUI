using Avalonia.Animation;

namespace AtomUI.Animations;

internal static class AnimationExtensions
{
    public static async Task RunInfiniteAsync(this Animation animation, Animatable control,
                                              CancellationToken cancellationToken = default)
    {
        animation.IterationCount = new IterationCount(1);
        try 
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await animation.RunAsync(control, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
        }
    }
}