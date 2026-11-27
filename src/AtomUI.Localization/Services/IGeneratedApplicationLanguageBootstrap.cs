using System.ComponentModel;

namespace AtomUI.Localization;

/// <summary>
/// Implemented by the generated partial declaration of an application's class. It lets startup reach the
/// application instance and register the language catalogs and translation bundles that the application
/// assembly owns. The source generator writes the implementation; application code does not implement it.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IGeneratedApplicationLanguageBootstrap
{
    /// <summary>
    /// Registers the language catalogs and translation bundles that the application assembly owns with
    /// <paramref name="builder" />. Startup calls it once while it commits startup, before the localization host
    /// is built.
    /// </summary>
    /// <param name="builder">
    /// The localization builder that receives the registrations.
    /// </param>
    void RegisterApplicationLanguages(ILocalizationBuilder builder);
}
