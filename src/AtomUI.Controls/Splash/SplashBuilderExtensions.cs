namespace AtomUI.Controls;

/// <summary>
/// Extension methods that configure the application startup on an <see cref="IAtomUIBuilder" />.
/// </summary>
public static class SplashBuilderExtensions
{
    /// <summary>
    /// Registers the startup splash; without a call no splash is shown. The splash is not shown in the design mode or
    /// without a classic desktop application lifetime.
    /// </summary>
    /// <param name="builder">
    /// The builder of the AtomUI setup.
    /// </param>
    /// <param name="mode">
    /// Decides when the splash is shown: <see cref="SplashMode.Off" /> never, <see cref="SplashMode.Always" /> in
    /// every build, and <see cref="SplashMode.Auto" />, the default, only when the code is JIT compiled, ReadyToRun
    /// included, and not when the application is published as NativeAOT.
    /// </param>
    /// <param name="configure">
    /// Sets the content and the theme of the splash; <see langword="null" /> shows the default theme without logo,
    /// image and texts.
    /// </param>
    /// <returns>
    /// The same <paramref name="builder" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// A splash is already registered by an earlier call, or the builder no longer accepts changes to the initial
    /// application configuration.
    /// </exception>
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
