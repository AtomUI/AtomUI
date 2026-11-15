using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.DesignTokens;

namespace AtomUIGallery.ShowCases.ThemePresets;

// The Gallery is an internal friend of Core solely for this adapter. It reads the committed
// compiled catalog; preview never publishes a root request or discovers packages by reflection.
internal sealed class ThemePresetCatalog(ThemeManager manager)
{
    private CompiledThemeCatalog? _catalog;
    private readonly Dictionary<string, ThemeConfig> _configs = new(StringComparer.Ordinal);

    internal IReadOnlyList<(ThemeInfo Info, string Package)> Read()
    {
        var catalog = manager.CompiledThemeCatalog;
        if (!ReferenceEquals(_catalog, catalog))
        {
            _catalog = catalog;
            _configs.Clear();
        }
        var compiled = catalog.ResolverSlices
            .SelectMany(slice => slice.Definitions
                .Where(entry => entry.Revision.ProgramDigest is not null)
                .Select(entry => (entry.Info, Package: slice.ResolverId))).ToArray();
        var packages = compiled.Where(item => item.Package != "AtomUIGallery" && item.Package != "AtomUI.Core").ToArray();
        return packages.Length == 0 ? compiled : packages;
    }

    // A null chain keeps the definition's own chain: a mode profile may run a package algorithm (for example
    // PrimeNG:Aura.Dark) that the built-in Default/Dark/Compact toggles cannot stand in for.
    // A non-null primary replaces the definition's ColorPrimary seed; null leaves the declarations untouched.
    internal ThemeConfig CreateConfig(string themeId, IReadOnlyList<ThemeAlgorithm>? algorithms, string? primary = null)
    {
        _ = Read();
        var definition = _catalog!.Get(themeId).Definition;
        algorithms ??= definition.Algorithms.Select(descriptor => descriptor.Algorithm).ToArray();
        var key = themeId + ":" + string.Join(",", algorithms) + ":" + primary;
        if (_configs.TryGetValue(key, out var cached))
        {
            return cached;
        }
        // Inherit=false starts from library defaults, so unrelated root aliases and Own
        // overrides cannot leak into this preview. Keep every declaration, including Own tokens.
        var builder = new ThemeConfigBuilder().WithInherit(false).WithAlgorithms(algorithms.ToArray());
        foreach (var token in definition.Tokens)
        {
            builder.WithToken(token.Descriptor.Name, token.CanonicalValue);
        }
        if (primary is not null)
        {
            builder.WithToken(nameof(DesignToken.ColorPrimary), primary);
        }
        foreach (var control in definition.Controls)
        {
            var mode = control.AlgorithmMode == ControlAlgorithmMode.Unspecified
                ? ControlAlgorithmMode.Disabled : control.AlgorithmMode;
            var local = new ControlThemeConfigBuilder().WithAlgorithm(mode);
            if (mode == ControlAlgorithmMode.Custom)
            {
                local.WithAlgorithms(control.Algorithms.Select(value => value.Algorithm).ToArray());
            }
            foreach (var token in control.GlobalTokens.Concat(control.OwnTokens))
            {
                local.WithToken(token.Descriptor.Name, token.CanonicalValue);
            }
            builder.WithControl(control.Descriptor.Identity, local.Build());
        }
        var config = builder.Build();
        _configs.Add(key, config);
        return config;
    }

    // Tokens derived from the brand color; a theme declaring BrandColor="Seed" must not pin them (rule ATMTHM3101).
    private static readonly HashSet<string> s_brandDerivedTokens = new(StringComparer.Ordinal)
    {
        nameof(DesignToken.ColorPrimaryHover), nameof(DesignToken.ColorPrimaryActive),
        nameof(DesignToken.ColorPrimaryBg), nameof(DesignToken.ColorPrimaryBgHover),
        nameof(DesignToken.ColorPrimaryBorder), nameof(DesignToken.ColorPrimaryBorderHover),
        nameof(DesignToken.ColorPrimaryText), nameof(DesignToken.ColorPrimaryTextHover),
        nameof(DesignToken.ColorPrimaryTextActive), nameof(DesignToken.ColorControlOutline)
    };

    internal static string Export(string id, string name, ThemeConfig config, ThemeAppearance? appearance = null,
        string? designLanguage = null, ThemeBrandColorSupport brandColor = ThemeBrandColorSupport.Fixed,
        ThemeDensity density = ThemeDensity.Normal)
    {
        // A Seed theme that pins a brand-derived token would be rejected on read, so export it as Fixed.
        if (brandColor == ThemeBrandColorSupport.Seed &&
            (config.Tokens.Keys.Any(s_brandDerivedTokens.Contains) ||
             config.Controls.Values.Any(control => control.Tokens.Keys.Any(s_brandDerivedTokens.Contains))))
        {
            brandColor = ThemeBrandColorSupport.Fixed;
        }
        XNamespace ns = "https://atomui.net/schemas/theme/v1";
        XElement Algorithms(IEnumerable<ThemeAlgorithm> values) => new(ns + "Algorithms",
            values.Select(value => new XElement(ns + "Algorithm",
                value.Catalog == ThemeAlgorithm.BuiltInCatalog ? null : new XAttribute("Catalog", value.Catalog),
                new XAttribute("Id", value.Id))));
        XElement Tokens(IReadOnlyDictionary<string, string> values) => new(ns + "Tokens",
            values.Select(value => new XElement(ns + "Token", new XAttribute("Name", value.Key), new XAttribute("Value", value.Value))));
        var root = new XElement(ns + "Theme", new XAttribute("Id", id), new XAttribute("Name", name),
            new XAttribute("Appearance",
                (appearance ?? (config.Algorithms!.Contains(ThemeAlgorithm.Dark) ? ThemeAppearance.Dark : ThemeAppearance.Light)).ToString()),
            designLanguage is null ? null : new XAttribute("DesignLanguage", designLanguage),
            new XAttribute("BrandColor", brandColor.ToString()), new XAttribute("Density", density.ToString()),
            new XAttribute("IsDefault", false), Algorithms(config.Algorithms!), Tokens(config.Tokens),
            // The schema requires at least one Control inside Controls, so a profile without control values omits it.
            config.Controls.Count == 0 ? null : new XElement(ns + "Controls", config.Controls.Select(pair =>
            {
                var control = new XElement(ns + "Control", new XAttribute("Catalog", pair.Key.Catalog),
                    new XAttribute("Id", pair.Key.Id));
                if (pair.Value.Algorithm is ControlAlgorithmMode.Disabled or ControlAlgorithmMode.Global)
                {
                    control.Add(new XAttribute("Algorithm", pair.Value.Algorithm));
                }
                if (pair.Value.Algorithms is { } algorithms)
                {
                    control.Add(Algorithms(algorithms));
                }
                control.Add(Tokens(pair.Value.Tokens));
                return control;
            })));
        return new XDocument(new XDeclaration("1.0", "utf-8", null), root).ToString();
    }
}
