using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Algorithms;

/// <summary>
/// Declares a theme algorithm class to the AtomUI generator under the identity <c>Catalog:Id</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ThemeAlgorithmAttribute : Attribute
{
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

    public string Catalog => Algorithm.Catalog;

    public string Id => Algorithm.Id;

    public ThemeAlgorithm Algorithm { get; }

    public int Revision { get; }

    public ThemeAppearanceEffect AppearanceEffect { get; }
}
