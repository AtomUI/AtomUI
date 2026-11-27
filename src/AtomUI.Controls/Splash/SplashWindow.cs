using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Controls;

/// <summary>
/// Host window of the splash. It derives from the Avalonia <c>Window</c> and does not depend on an AtomUI
/// theme; it carries all of its resources itself: the global Tokens, the default theme and an optional custom
/// theme.
/// </summary>
public class SplashWindow : Window
{
    internal Splash? Splash { get; private set; }

    /// <summary>
    /// Keys the splash window as <see cref="SplashWindow" /> for theme lookup. The base <c>Window</c> returns its
    /// own type here, so without this override the splash window would neither find the implicit theme keyed by
    /// <see cref="SplashWindow" /> nor apply a theme assigned through the <c>Theme</c> property whose TargetType is
    /// <see cref="SplashWindow" />. Overrides must keep returning <see cref="SplashWindow" />.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(SplashWindow);

    internal void Initialize(IReadOnlyDictionary<object, object?> globalResources, SplashContent content)
    {
        var tokens = new ResourceDictionary();
        foreach (var (key, value) in globalResources)
        {
            tokens[key] = value;
        }
        Resources.MergedDictionaries.Add(tokens);
        Resources.MergedDictionaries.Add(new SplashDefaultTheme());
        if (content.CreateControlTheme is { } createTheme)
        {
            // The dictionary merged last wins: a ControlTheme with the same key in the custom theme
            // overrides the one in the default theme.
            Resources.MergedDictionaries.Add(createTheme());
        }
        Splash = new Splash
        {
            // The image factories are invoked here, on the UI thread with the platform initialized: bitmaps
            // cannot be created while the configuration callback runs.
            Logo = content.Logo?.Invoke(),
            BackgroundImage = content.BackgroundImage?.Invoke(),
            Title = content.Title,
            Message = content.Message
        };
        Content = Splash;
    }

    /// <summary>
    /// Switches the splash to its opaque presentation when the window is not transparent
    /// (<see cref="TopLevel.ActualTransparencyLevel" /> is <see cref="WindowTransparencyLevel.None" />): the default
    /// theme then drops the corner radius and the shadow of the splash card. Overrides must call the base member,
    /// because the presenter uses the <c>Opened</c> event raised by the base implementation to detect the first
    /// rendered frame.
    /// </summary>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        Splash?.SetOpaque(ActualTransparencyLevel == WindowTransparencyLevel.None);
    }

    /// <summary>
    /// Raises <see cref="Splash.CloseRequested" />, which makes the presenter close the splash window, and marks
    /// the key event as handled when Escape is pressed while the splash is in the <see cref="SplashStatus.Error" />
    /// state. The base member is called first for every key event. Overrides must call the base member to keep
    /// the Escape shortcut.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape && Splash is { Status: SplashStatus.Error } splash)
        {
            splash.RaiseEvent(new RoutedEventArgs(Splash.CloseRequestedEvent));
            e.Handled = true;
        }
    }
}
