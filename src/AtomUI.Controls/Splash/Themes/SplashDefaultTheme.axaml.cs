using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AtomUI.Controls;

/// <summary>
/// Default theme of the <see cref="Splash" /> and the <see cref="SplashWindow" />. A custom theme can replace
/// it as a whole, or merge this dictionary and use <c>BasedOn</c> to override parts of it.
/// </summary>
public partial class SplashDefaultTheme : ResourceDictionary
{
    public SplashDefaultTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
