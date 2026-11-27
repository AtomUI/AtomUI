using Avalonia;

namespace AtomUI.Controls;

/// <summary>
/// Provides the extension method that registers image loading on an <see cref="IAtomUIBuilder" />.
/// </summary>
public static class ImageLoadingBuilderExtensions
{
    private const string StateId = "AtomUI.Controls.ImageLoading";
    private const string ServiceId = "AtomUI.Controls.ImageLoader";

    /// <summary>
    /// Registers the image loader of the application, which image controls use and
    /// <see cref="ImageLoadingApplicationExtensions.GetImageLoader" /> returns.
    /// </summary>
    /// <param name="builder">
    /// The builder that <c>UseAtomUI()</c> passes to its configuration callback.
    /// </param>
    /// <param name="configure">
    /// Changes the options of the loader. <see langword="null" /> keeps every default.
    /// </param>
    /// <returns>
    /// The same <paramref name="builder" />, for chaining.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="builder" /> is not the builder created by <c>UseAtomUI()</c>.
    /// </exception>
    /// <remarks>
    /// Calling it again, as <c>UseCommonControls()</c> does, joins the same registration: every callback receives
    /// the same options and one loader is created. The options are validated when the loader is created at
    /// application startup; an invalid value throws <see cref="InvalidOperationException" /> at that point.
    /// </remarks>
    public static IAtomUIBuilder UseImageLoading(
        this IAtomUIBuilder builder,
        Action<ImageLoadingOptionsBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var atomBuilder = builder as AtomUIBuilder ??
            throw new InvalidOperationException("Image loading requires the AtomUI application builder.");
        var state = atomBuilder.GetOrAddExtensionState(StateId, static () => new ImageLoadingRegistrationState());
        configure?.Invoke(state.Options);
        atomBuilder.AddOwnedService(ServiceId, state.BuildLoader);
        return builder;
    }

    internal static IAtomUIBuilder AddImageCodec<TCodec>(
        this IAtomUIBuilder builder,
        Func<TCodec> factory)
        where TCodec : ImageCodec
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(factory);
        builder.UseImageLoading();
        var atomBuilder = (AtomUIBuilder)builder;
        var state = atomBuilder.GetOrAddExtensionState(StateId, static () => new ImageLoadingRegistrationState());
        state.AddCodec(factory);
        return builder;
    }

    private sealed class ImageLoadingRegistrationState
    {
        private readonly List<Func<ImageCodec>> _codecFactories = [static () => new RasterImageCodec()];
        private bool _built;

        internal ImageLoadingOptionsBuilder Options { get; } = new();

        internal void AddCodec(Func<ImageCodec> factory)
        {
            if (_built)
            {
                throw new InvalidOperationException("Image codec registration is frozen.");
            }
            _codecFactories.Add(factory);
        }

        internal ImageLoader BuildLoader(Application application)
        {
            if (_built)
            {
                throw new InvalidOperationException("Image loader has already been built.");
            }
            _built = true;
            var applicationName = application.GetType().Assembly.GetName().Name;
            return new ImageLoader(
                Options.Build(applicationName),
                _codecFactories.Select(factory => factory()));
        }
    }
}

/// <summary>
/// Provides the extension methods that look up the image loader of an <see cref="Application" />.
/// </summary>
public static class ImageLoadingApplicationExtensions
{
    /// <summary>
    /// Returns the image loader registered for <paramref name="application" />.
    /// </summary>
    /// <param name="application">
    /// The application that owns the loader.
    /// </param>
    /// <returns>
    /// The loader of the application.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Image loading is not registered for the application or its loader has been disposed. Register it with
    /// <see cref="ImageLoadingBuilderExtensions.UseImageLoading" />, which <c>UseCommonControls()</c> and
    /// <c>UseDesktopControls()</c> also do.
    /// </exception>
    public static IImageLoader GetImageLoader(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ImageLoaderStore.Get(application) ?? throw new InvalidOperationException(
            "Image loading is not registered for this Application. Call UseImageLoading(), UseCommonControls(), or UseDesktopControls() inside UseAtomUI().");
    }

    /// <summary>
    /// Returns the image loader registered for <paramref name="application" />, or <see langword="null" /> when
    /// image loading is not registered for it or its loader has been disposed.
    /// </summary>
    public static IImageLoader? TryGetImageLoader(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ImageLoaderStore.Get(application);
    }
}
