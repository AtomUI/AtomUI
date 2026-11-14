using AtomUI.Theme.Algorithms;

namespace AtomUI.Theme.Schema;

public enum ThemeAppearanceEffect : byte
{
    Preserve,
    Light,
    Dark
}

public sealed class ThemeAlgorithmDescriptor
{
    private readonly Func<IThemeAlgorithm> _factory;
    internal object CompilationIdentity { get; } = new();

    /// <summary>
    /// Describes one theme algorithm for registration. The generator creates these for <c>[ThemeAlgorithm]</c>
    /// classes; a package can also create one by hand and pass it to
    /// <c>IThemeManagerBuilder.AddThemeAlgorithms</c>.
    /// </summary>
    /// <param name="algorithm">The algorithm identity <c>Catalog:Id</c>; must not be uninitialized.</param>
    /// <param name="revision">
    /// A positive number increased whenever the algorithm's output changes; it is part of every compiled-theme
    /// fingerprint and cache key.
    /// </param>
    /// <param name="appearanceEffect">How the algorithm sets the appearance of the chain it runs in.</param>
    /// <param name="factory">
    /// Creates an algorithm instance; called every time the compiler evaluates a chain that contains the algorithm,
    /// possibly from several threads at once, and must not return <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="algorithm"/> is uninitialized.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="revision"/> is zero or negative.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <see langword="null"/>.</exception>
    public ThemeAlgorithmDescriptor(
        ThemeAlgorithm algorithm,
        int revision,
        ThemeAppearanceEffect appearanceEffect,
        Func<IThemeAlgorithm> factory)
    {
        if (algorithm.IsUninitialized)
        {
            throw new ArgumentException("Theme algorithm identity is uninitialized.", nameof(algorithm));
        }
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(revision);
        ArgumentNullException.ThrowIfNull(factory);
        Algorithm        = algorithm;
        Revision         = revision;
        AppearanceEffect = appearanceEffect;
        _factory         = factory;
    }

    public ThemeAlgorithm Algorithm { get; }
    public int Revision { get; }
    public ThemeAppearanceEffect AppearanceEffect { get; }

    public IThemeAlgorithm Create()
    {
        return _factory() ??
               throw new InvalidOperationException($"Theme algorithm '{Algorithm}' factory returned null.");
    }
}
