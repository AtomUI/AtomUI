using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Where <see cref="MotionGate" /> reads activity from: the nearest injected source, else the application.
/// </summary>
public static class MotionActivity
{
    /// <summary>
    /// Activity source for a visual subtree. Inherited, so a host such as a mobile runtime root sets it once for
    /// everything below it.
    /// </summary>
    public static readonly AttachedProperty<IMotionActivitySource?> SourceProperty =
        AvaloniaProperty.RegisterAttached<Visual, IMotionActivitySource?>("Source", typeof(MotionActivity), inherits: true);

    private static ApplicationActivitySource? s_application;

    /// <summary>
    /// Activity of the whole application, from <see cref="IActivatableLifetime" />; always active without one.
    /// </summary>
    public static IMotionActivitySource Application => s_application ??= new ApplicationActivitySource();

    /// <summary>
    /// Reads the activity source in effect for <paramref name="visual" />.
    /// </summary>
    /// <returns>
    /// The source set on <paramref name="visual" /> or inherited from an ancestor, or <see langword="null" /> when
    /// there is none; a <see cref="MotionGate" /> then falls back to <see cref="Application" />.
    /// </returns>
    public static IMotionActivitySource? GetSource(Visual visual) => visual.GetValue(SourceProperty);

    /// <summary>
    /// Sets the activity source on <paramref name="visual" />; every visual below it inherits the value.
    /// </summary>
    public static void SetSource(Visual visual, IMotionActivitySource? source) => visual.SetValue(SourceProperty, source);

    private sealed class ApplicationActivitySource : IMotionActivitySource
    {
        private bool _isActive = true;

        public ApplicationActivitySource()
        {
            if (Avalonia.Application.Current?.TryGetFeature<IActivatableLifetime>() is { } lifetime)
            {
                lifetime.Activated   += HandleActivated;
                lifetime.Deactivated += HandleDeactivated;
            }
        }

        public bool IsActive => _isActive;

        public event EventHandler? IsActiveChanged;

        private void HandleActivated(object? sender, ActivatedEventArgs e)
        {
            if (e.Kind == ActivationKind.Background)
            {
                SetActive(true);
            }
        }

        private void HandleDeactivated(object? sender, ActivatedEventArgs e)
        {
            if (e.Kind == ActivationKind.Background)
            {
                SetActive(false);
            }
        }

        private void SetActive(bool isActive)
        {
            if (_isActive == isActive)
            {
                return;
            }
            _isActive = isActive;
            IsActiveChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
