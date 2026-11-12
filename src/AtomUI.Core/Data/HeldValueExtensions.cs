using Avalonia;
using Avalonia.Data;

namespace AtomUI.Data;

/// <summary>
/// Writes a control's derived value at a non-local priority through a held handle, so repeated writes of the same
/// property replace one entry instead of stacking a new value frame each time.
/// </summary>
internal static class HeldValueExtensions
{
    /// <summary>
    /// Sets <paramref name="value"/> at <paramref name="priority"/> and releases the entry previously held in
    /// <paramref name="handle"/>. The new entry goes in before the old one is released, so the effective value never
    /// falls back in between.
    /// </summary>
    public static void SetHeldValue<T>(
        this AvaloniaObject target,
        StyledProperty<T> property,
        T value,
        ref IDisposable? handle,
        BindingPriority priority = BindingPriority.Template)
    {
        var previous = handle;
        handle = target.SetValue(property, value, priority);
        previous?.Dispose();
    }

    /// <summary>Releases the entry held in <paramref name="handle"/>, if any.</summary>
    public static void ReleaseHeldValue(ref IDisposable? handle)
    {
        var previous = handle;
        handle = null;
        previous?.Dispose();
    }
}
