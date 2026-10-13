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

    public ThemeAlgorithmDescriptor(
        ThemeAlgorithm algorithm,
        int revision,
        ThemeAppearanceEffect appearanceEffect,
        Func<IThemeAlgorithm> factory)
    {
        if (!Enum.IsDefined(algorithm))
        {
            throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, "Theme algorithm must be defined.");
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
