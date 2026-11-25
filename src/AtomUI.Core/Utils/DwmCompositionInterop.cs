using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AtomUI.Utils;

[SupportedOSPlatform("windows")]
internal static partial class DwmCompositionInterop
{
    [LibraryImport("dwmapi.dll", EntryPoint = "DwmFlush")]
    internal static partial int Flush();

    internal static unsafe bool TryGetSingleMonitorTiming(out long refreshPeriod, out long verticalBlank)
    {
        refreshPeriod = 0;
        verticalBlank = 0;
        // DwmGetCompositionTimingInfo requires a null HWND. The desktop-wide timing
        // sample can only be associated with a specific monitor on a single-monitor desktop.
        if (GetSystemMetrics(SystemMetric.SM_CMONITORS) != 1)
        {
            return false;
        }

        var timing = new DwmTimingInfo { Size = (uint)sizeof(DwmTimingInfo) };
        if (DwmGetCompositionTimingInfo(IntPtr.Zero, ref timing) < 0)
        {
            return false;
        }

        refreshPeriod = (long)timing.RefreshPeriod;
        verticalBlank = (long)timing.VerticalBlank;
        return true;
    }

    [LibraryImport("user32.dll")]
    private static partial int GetSystemMetrics(SystemMetric index);

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmGetCompositionTimingInfo(IntPtr hwnd, ref DwmTimingInfo timing);

    // winuser.h defines the metric index passed to GetSystemMetrics.
    private enum SystemMetric
    {
        SM_CMONITORS = 80
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct UnsignedRatio
    {
        public uint Numerator;
        public uint Denominator;
    }

    // Keep the complete layout, including fields unused by the clock, as defined by dwmapi.h.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DwmTimingInfo
    {
        public uint Size;
        public UnsignedRatio RefreshRate;
        public ulong RefreshPeriod;
        public UnsignedRatio CompositionRate;
        public ulong VerticalBlank;
        public ulong RefreshCount;
        public uint DirectXRefreshCount;
        public ulong CompositionTime;
        public ulong Frame;
        public uint DirectXPresent;
        public ulong RefreshFrame;
        public ulong FrameSubmitted;
        public uint DirectXPresentSubmitted;
        public ulong FrameConfirmed;
        public uint DirectXPresentConfirmed;
        public ulong RefreshConfirmed;
        public uint DirectXRefreshConfirmed;
        public ulong FramesLate;
        public uint FramesOutstanding;
        public ulong FrameDisplayed;
        public ulong FrameDisplayedTime;
        public ulong RefreshFrameDisplayed;
        public ulong FrameComplete;
        public ulong FrameCompleteTime;
        public ulong FramePending;
        public ulong FramePendingTime;
        public ulong FramesDisplayed;
        public ulong FramesComplete;
        public ulong FramesPending;
        public ulong FramesAvailable;
        public ulong FramesDropped;
        public ulong FramesMissed;
        public ulong RefreshNextDisplayed;
        public ulong RefreshNextPresented;
        public ulong RefreshesDisplayed;
        public ulong RefreshesPresented;
        public ulong RefreshStarted;
        public ulong PixelsReceived;
        public ulong PixelsDrawn;
        public ulong BuffersEmpty;
    }
}
