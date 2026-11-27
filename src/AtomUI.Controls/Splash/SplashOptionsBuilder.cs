using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AtomUI.Controls;

/// <summary>
/// Collects the content and the theme of the <see cref="Splash" /> in the callback passed to
/// <c>UseSplash</c>. Images are stored as factories, because bitmaps cannot be created at that time: the platform
/// is not initialized yet.
/// </summary>
public sealed class SplashOptionsBuilder
{
    internal Func<IImage>? Logo { get; private set; }
    internal Func<IImage>? BackgroundImage { get; private set; }
    internal string? Title { get; private set; }
    internal string? Message { get; private set; }
    internal Func<ResourceDictionary>? CreateControlTheme { get; private set; }

    /// <summary>
    /// Sets the logo, the image shown above the title of the splash card. The bitmap is loaded on the UI thread
    /// when the splash is shown.
    /// </summary>
    /// <param name="source">
    /// An <c>avares://</c> URI or a file URI.
    /// </param>
    public SplashOptionsBuilder WithLogo(Uri source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Logo = () => LoadBitmap(source);
        return this;
    }

    /// <summary>
    /// Sets the logo, the image shown above the title of the splash card, from a factory for custom sources such
    /// as vector drawings. The factory is invoked on the UI thread when the splash is shown.
    /// </summary>
    public SplashOptionsBuilder WithLogo(Func<IImage> factory)
    {
        Logo = factory ?? throw new ArgumentNullException(nameof(factory));
        return this;
    }

    /// <summary>
    /// Sets the background image, the image painted behind the content of the splash card. The bitmap is loaded
    /// on the UI thread when the splash is shown.
    /// </summary>
    /// <param name="source">
    /// An <c>avares://</c> URI or a file URI.
    /// </param>
    public SplashOptionsBuilder WithBackgroundImage(Uri source)
    {
        ArgumentNullException.ThrowIfNull(source);
        BackgroundImage = () => LoadBitmap(source);
        return this;
    }

    /// <summary>
    /// Sets the background image, the image painted behind the content of the splash card, from a factory for
    /// custom sources. The factory is invoked on the UI thread when the splash is shown.
    /// </summary>
    public SplashOptionsBuilder WithBackgroundImage(Func<IImage> factory)
    {
        BackgroundImage = factory ?? throw new ArgumentNullException(nameof(factory));
        return this;
    }

    private static Bitmap LoadBitmap(Uri source) =>
        source.Scheme == "avares"
            ? new Bitmap(AssetLoader.Open(source))
            : new Bitmap(source.IsFile ? source.LocalPath : source.ToString());

    /// <summary>
    /// Sets the heading shown below the logo of the splash card.
    /// </summary>
    public SplashOptionsBuilder WithTitle(string title)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        return this;
    }

    /// <summary>
    /// Sets the initial status text shown below the title of the splash card. The message of a progress report
    /// replaces it, and so does the error message when the startup fails.
    /// </summary>
    public SplashOptionsBuilder WithMessage(string message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        return this;
    }

    /// <summary>
    /// Sets a custom theme for the splash. The dictionary is merged after the default theme, so its implicit
    /// ControlTheme for <see cref="Splash" /> and, optionally, for <see cref="SplashWindow" /> replaces the default
    /// one. It may only use the <see cref="Splash" /> and <see cref="SplashProgressBar" /> controls, Avalonia base
    /// elements and global Tokens: it must not contain other AtomUI controls or depend on custom fonts.
    /// </summary>
    /// <typeparam name="TTheme">
    /// An x:Class resource dictionary that holds the custom themes.
    /// </typeparam>
    public SplashOptionsBuilder WithControlTheme<TTheme>() where TTheme : ResourceDictionary, new()
    {
        CreateControlTheme = static () => new TTheme();
        return this;
    }

    internal SplashContent Build() => new(Logo, BackgroundImage, Title, Message, CreateControlTheme);
}
