namespace AtomUI.Controls;

public static class SplashBuilderExtensions
{
    /// <summary>启用启动 Splash。不调用即不显示。</summary>
    public static IAtomUIBuilder UseSplash(this IAtomUIBuilder builder, SplashMode mode = SplashMode.Auto,
                                          Action<SplashOptionsBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var options = new SplashOptionsBuilder();
        configure?.Invoke(options);
        ((AtomUIBuilder)builder).SetSplash(
            new SplashRegistration(mode, static () => new SplashPresenter(), options.Build()));
        return builder;
    }
}
