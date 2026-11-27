namespace AtomUI;

/// <summary>
/// Decides when a splash is shown. The splash is not shown in the design mode or without a classic desktop
/// application lifetime.
/// </summary>
public enum SplashMode
{
    /// <summary>
    /// Never shows the splash.
    /// </summary>
    Off,
    /// <summary>
    /// Shows the splash in every build, NativeAOT included.
    /// </summary>
    Always,
    /// <summary>
    /// Shows the splash only when the code is JIT compiled, ReadyToRun included, and not when the application is
    /// published as NativeAOT.
    /// </summary>
    Auto
}
