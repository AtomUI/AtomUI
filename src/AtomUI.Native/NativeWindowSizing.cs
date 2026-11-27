using Avalonia.Controls;

namespace AtomUI.Native;

/// <summary>
/// Keeps the size limits of a window with client-side decorations in effect while the user resizes it, and
/// reports when such a resize ends. <see cref="NativeWindowSizing.TryAttachCsdSizingHook" /> returns the
/// Windows implementation and <see langword="null" /> on other platforms. AtomUI.Desktop.Controls uses it in
/// <c>DialogWindow</c>.
/// </summary>
/// <remarks>
/// The hook applies <c>MinWidth</c>, <c>MinHeight</c>, <c>MaxWidth</c> and <c>MaxHeight</c> of the window only
/// while the <c>isEnabled</c> callback returns <see langword="true" />, the window is visible, is extended into
/// its decorations and cannot be maximized. A resize is in progress from <see cref="BeginUserResize" /> until
/// <see cref="CompleteUserResize" />, which raises <see cref="UserResizeCompleted" />, or
/// <see cref="CancelUserResize" />, which ends it without raising the event. The Windows implementation begins
/// the resize when Windows starts a size or move loop and completes it after the loop has ended. After the hook
/// is disposed, the three methods do nothing.
/// </remarks>
internal interface INativeWindowSizingHook : IDisposable
{
    event EventHandler? UserResizeCompleted;

    bool IsUserResizeInProgress { get; }

    void BeginUserResize();

    void CompleteUserResize();

    void CancelUserResize();
}

internal static class NativeWindowSizing
{
    public static INativeWindowSizingHook? TryAttachCsdSizingHook(Window window, Func<bool> isEnabled)
    {
        return OperatingSystem.IsWindows()
            ? new WindowsCsdSizingHook(window, isEnabled)
            : null;
    }
}
