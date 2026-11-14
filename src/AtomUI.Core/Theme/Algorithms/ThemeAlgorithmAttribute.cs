using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Algorithms;

/// <summary>
/// Declares a theme algorithm class to the AtomUI generator under the identity <c>Catalog:Id</c>.
/// </summary>
/// <remarks>
/// The generator registers the class; it must be a non-abstract, non-generic class that implements
/// <see cref="IThemeAlgorithm"/> and has a public parameterless constructor. A declaration that cannot be registered
/// is reported as the build error ATOMUITOKEN002 at this attribute.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ThemeAlgorithmAttribute : Attribute
{
    /// <summary>
    /// Declares the algorithm <c>catalog:id</c>.
    /// </summary>
    /// <param name="catalog">
    /// The catalog that owns the algorithm, usually the package's own name (for example <c>PrimeNG</c>); a third-party
    /// algorithm should not use the built-in algorithms' catalog <c>AtomUI</c>.
    /// </param>
    /// <param name="id">The algorithm's id within its catalog (for example <c>Aura.Dark</c>).</param>
    /// <param name="revision">
    /// A positive number that the author increases whenever the algorithm's output changes; it is part of every
    /// compiled-theme fingerprint and cache key, so cached results of the old revision are not reused.
    /// </param>
    /// <param name="appearanceEffect">
    /// How the algorithm sets the appearance of the chain it runs in: keeps the current one, or forces light or dark.
    /// </param>
    public ThemeAlgorithmAttribute(
        string catalog,
        string id,
        int revision,
        ThemeAppearanceEffect appearanceEffect)
    {
        Algorithm        = new ThemeAlgorithm(catalog, id);
        Revision         = revision;
        AppearanceEffect = appearanceEffect;
    }

    /// <summary>
    /// The catalog part of the algorithm identity.
    /// </summary>
    public string Catalog => Algorithm.Catalog;

    /// <summary>
    /// The id part of the algorithm identity.
    /// </summary>
    public string Id => Algorithm.Id;

    /// <summary>
    /// The algorithm identity <c>Catalog:Id</c>.
    /// </summary>
    public ThemeAlgorithm Algorithm { get; }

    /// <summary>
    /// The algorithm revision; increased whenever the algorithm's output changes.
    /// </summary>
    public int Revision { get; }

    /// <summary>
    /// How the algorithm sets the appearance of the chain it runs in.
    /// </summary>
    public ThemeAppearanceEffect AppearanceEffect { get; }
}
