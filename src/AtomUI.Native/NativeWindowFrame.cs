using Avalonia.Controls;

namespace AtomUI.Native;

internal static class NativeWindowFrame
{
    /// <summary>
    /// 仅 Windows 10 返回 hook：修正扩展客户区窗口在 RedirectionSurface 下激活态 DWM 边框被覆盖的问题。
    /// Windows 11 及其他平台返回 <c>null</c>。
    /// </summary>
    public static IDisposable? TryAttachLegacyFrameMarginHook(Window window)
    {
        return OperatingSystem.IsWindows()
            ? Windows.WindowsLegacyFrameMarginHook.TryCreate(window)
            : null;
    }
}
