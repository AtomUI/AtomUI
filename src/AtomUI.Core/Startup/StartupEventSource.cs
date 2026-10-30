using System.Diagnostics.Tracing;

namespace AtomUI;

/// <summary>启动分段打点。关闭时不映射枚举或构造事件负载。NativeAOT 默认裁掉 EventSource。</summary>
[EventSource(Name = "AtomUI-Startup")]
internal sealed class StartupEventSource : EventSource
{
    internal static readonly StartupEventSource Log = new();

    [NonEvent]
    internal void PhaseStart(StartupPhase phase)
    {
        if (IsEnabled())
        {
            PhaseStart(GetPhaseName(phase));
        }
    }

    [NonEvent]
    internal void PhaseStop(StartupPhase phase)
    {
        if (IsEnabled())
        {
            PhaseStop(GetPhaseName(phase));
        }
    }

    [NonEvent]
    internal void StateChanged(StartupState state)
    {
        if (IsEnabled())
        {
            StateChanged(GetStateName(state));
        }
    }

    // Keep the existing event methods, IDs and string payloads as the diagnostics wire contract.
    // Only the typed NonEvent wrappers can invoke these writers.
    [Event(1)]
    private void PhaseStart(string phase) => WriteEvent(1, phase);

    [Event(2)]
    private void PhaseStop(string phase) => WriteEvent(2, phase);

    [Event(3)]
    private void StateChanged(string state) => WriteEvent(3, state);

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

    [NonEvent]
    internal void PackageTiming(string packageId, PackagePhase phase, double elapsedMilliseconds)
    {
        if (IsEnabled())
        {
            PackageTiming(packageId, GetPhaseName(phase), elapsedMilliseconds);
        }
    }

    [Event(6)]
    private void PackageTiming(string packageId, string phase, double elapsedMilliseconds) =>
        WriteEvent(6, packageId, phase, elapsedMilliseconds);

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

    private static string GetPhaseName(StartupPhase phase) => phase switch
    {
        StartupPhase.Configure => "Configure",
        StartupPhase.Prepare => "Prepare",
        StartupPhase.PrepareWait => "PrepareWait",
        StartupPhase.Splash => "Splash",
        StartupPhase.Commit => "Commit",
        StartupPhase.CommitLocalization => "Commit.Localization",
        StartupPhase.CommitTheme => "Commit.Theme",
        StartupPhase.CommitOwnedServices => "Commit.OwnedServices",
        StartupPhase.CommitInitializers => "Commit.Initializers",
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null)
    };

    private static string GetPhaseName(PackagePhase phase) => phase switch
    {
        PackagePhase.Enable => "Enable",
        PackagePhase.Commit => "Commit",
        PackagePhase.FirstUse => "FirstUse",
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null)
    };

    private static string GetStateName(StartupState state) => state switch
    {
        StartupState.Preparing => "Preparing",
        StartupState.Committed => "Committed",
        StartupState.Presenting => "Presenting",
        StartupState.Ready => "Ready",
        StartupState.Failed => "Failed",
        StartupState.Canceled => "Canceled",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };
}
