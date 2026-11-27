using AtomUI.Localization;
using AtomUI.Theme;

namespace AtomUI;

/// <summary>
/// The theme and localization configuration that an application fills in while it is set up.
/// An instance is passed to the configuration callback of the startup and of <c>IAtomUI.LoadPackages</c>,
/// and what the callback wrote is consumed after the callback returns. The application writes through
/// <see cref="Theme" />, <see cref="Localization" /> and the extension methods of this type.
/// </summary>
public interface IAtomUIBuilder
{
    /// <summary>
    /// The theme configuration, consumed when the theme manager is built.
    /// <see cref="IThemeManagerBuilder" /> states what can be declared and until when.
    /// </summary>
    IThemeManagerBuilder Theme { get; }

    /// <summary>
    /// The localization configuration of <see cref="ILocalizationBuilder" />, consumed when the application's
    /// languages are registered and the localization host is built.
    /// </summary>
    ILocalizationBuilder Localization { get; }
}
