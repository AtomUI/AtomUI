namespace AtomUI.Theme;

/// <summary>A receipt for a completed scope request, including no-op and superseded requests.</summary>
public sealed class ThemeTransitionCompletedEventArgs : EventArgs
{
    public ThemeTransitionCompletedEventArgs(ThemeRequest request, ThemeTransitionResult result)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    public ThemeRequest Request { get; }
    public ThemeTransitionResult Result { get; }
}
