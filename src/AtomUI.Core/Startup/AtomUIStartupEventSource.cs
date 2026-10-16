using System.Diagnostics.Tracing;

namespace AtomUI;

/// <summary>启动分段打点。默认关闭；IsEnabled 之外零开销。NativeAOT 默认裁掉 EventSource。</summary>
[EventSource(Name = "AtomUI-Startup")]
internal sealed class AtomUIStartupEventSource : EventSource
{
    internal static readonly AtomUIStartupEventSource Log = new();

    [Event(1)]
    public void PhaseStart(string phase)
    {
        if (IsEnabled())
        {
            WriteEvent(1, phase);
        }
    }

    [Event(2)]
    public void PhaseStop(string phase)
    {
        if (IsEnabled())
        {
            WriteEvent(2, phase);
        }
    }

    [Event(3)]
    public void StateChanged(string state)
    {
        if (IsEnabled())
        {
            WriteEvent(3, state);
        }
    }

    [Event(4)]
    public void SplashFirstFrame()
    {
        if (IsEnabled())
        {
            WriteEvent(4);
        }
    }

    [Event(5)]
    public void MainWindowFirstFrame()
    {
        if (IsEnabled())
        {
            WriteEvent(5);
        }
    }

    [Event(6)]
    public void PackageTiming(string packageId, string phase, double elapsedMilliseconds)
    {
        if (IsEnabled())
        {
            WriteEvent(6, packageId, phase, elapsedMilliseconds);
        }
    }

    [Event(7)]
    public void FragmentMaterialized(string packageId, string controlType)
    {
        if (IsEnabled())
        {
            WriteEvent(7, packageId, controlType);
        }
    }

    [Event(8)]
    public void ThemeAssetMaterialized(string packageId, string assetId)
    {
        if (IsEnabled())
        {
            WriteEvent(8, packageId, assetId);
        }
    }
}
