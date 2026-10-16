namespace AtomUI;

/// <summary>Splash 显示策略。Auto：动态代码编译（JIT，含 ReadyToRun）时显示，NativeAOT 时不显示。</summary>
public enum SplashMode
{
    Off,
    Always,
    Auto
}
