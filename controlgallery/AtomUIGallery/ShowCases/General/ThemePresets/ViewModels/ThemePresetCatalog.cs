using System.Globalization;
using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Shell;

namespace AtomUIGallery.ShowCases.ThemePresets;

internal sealed record ThemePresetChoice(ThemeInfo Info, DesignStyle? Style = null, bool ShowInBar = true)
{
    // The catalog reserves the implicit entry's dollar prefix outside the XML theme Id syntax.
    internal string SelectionId => Style is { } style ? $"{Info.Id}.{style.Id}" : Info.Id;
    internal string PreviewId => Style is { } style ? Info.Language + style.Id : Info.Id;
    internal string Name => Style is { } style ? $"{Info.Language} {style.Id}" : Info.Name;
}

// The Gallery is an internal friend of Core solely for this adapter. It reads the committed
// compiled catalog; preview never publishes a root request or discovers packages by reflection.
internal sealed class ThemePresetCatalog(ThemeManager manager)
{
    private CompiledThemeCatalog? _catalog;
    private readonly Dictionary<(string ThemeId, DesignStyle Style, ThemeAppearance Appearance,
        DesignDensity Density, string? Primary, string? Neutral), ThemeConfig> _configs = new();

    /// <summary>The Gallery's language style previews, followed by named themes compiled into packages.</summary>
    internal IReadOnlyList<ThemePresetChoice> Read()
    {
        var catalog = GetCatalog();
        var choices = new List<ThemePresetChoice>();
        foreach (var language in catalog.Languages.Where(language => language.Id is "PrimeNG" or "Shadcn")
                                        .OrderBy(language => language.Id, StringComparer.Ordinal))
        {
            var definition = catalog.Get(ThemeInfo.LanguageDefaultId(language.Id)).Info;
            foreach (var style in language.Styles.OrderBy(style => style.Id, StringComparer.Ordinal))
            {
                choices.Add(new ThemePresetChoice(definition, style));
            }
        }
        var hasStylePreviews = choices.Count > 0;
        var compiled = catalog.ResolverSlices
            .SelectMany(slice => slice.Definitions
                .Where(entry => entry.Revision.ProgramDigest is not null && !entry.Info.IsLanguageDefault)
                .Select(entry => (catalog.Get(entry.Info.Id).Info, Package: slice.ResolverId))).ToArray();
        var packages = compiled.Where(item => item.Package != "AtomUIGallery").ToArray();
        foreach (var item in packages.Length == 0 ? compiled : packages)
        {
            // Daybreak Blue is the built-in fallback when the language previews are unavailable.
            choices.Add(new ThemePresetChoice(item.Info, ShowInBar: !hasStylePreviews || item.Info.Id != "DaybreakBlue"));
        }
        return choices;
    }

    private CompiledThemeCatalog GetCatalog()
    {
        var catalog = manager.CompiledThemeCatalog;
        if (!ReferenceEquals(_catalog, catalog))
        {
            _catalog = catalog;
            _configs.Clear();
        }
        return catalog;
    }

    /// <summary>The language of a theme, as the catalog describes it.</summary>
    internal DesignLanguageInfo? Language(ThemeInfo info) =>
        GetCatalog().Languages.FirstOrDefault(language => language.Id == info.Language);

    internal ThemeInfo LanguageDefaults(ThemeInfo info) =>
        GetCatalog().Get(ThemeInfo.LanguageDefaultId(info.Language)).Info;

    /// <summary>
    /// Resolves a language style or named theme in an isolated local scope, with the selected variant and seeds.
    /// </summary>
    internal ThemeConfig CreateConfig(ThemePresetChoice choice, bool dark, bool compact, string? primary, string? neutral)
    {
        var definition = GetCatalog().Get(choice.Info.Id).Definition;
        var language = definition.Language;
        var appearance = definition.Appearance ?? (dark ? ThemeAppearance.Dark : ThemeAppearance.Light);
        var density = definition.Density ??
                      (compact && language.Axes.Densities.Contains(DesignDensity.Compact) ? DesignDensity.Compact : DesignDensity.Default);
        var style = definition.Style ?? choice.Style ?? language.Axes.DefaultStyle;
        var key = (choice.Info.Id, style, appearance, density, primary, neutral);
        if (_configs.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var builder = new ThemeConfigBuilder().WithInherit(false).WithLanguage(language.Id).WithStyle(style)
                                              .WithAppearance(appearance).WithDensity(density);
        var layer = definition.ValuesFor(new DesignVariant(style, appearance, density, DesignIdiom.Desktop));
        foreach (var token in layer.GlobalTokens)
        {
            builder.WithToken(token.Descriptor.Name, token.CanonicalValue);
        }
        foreach (var token in layer.LanguageTokens)
        {
            builder.WithLanguageToken(token.Descriptor.Name, token.CanonicalValue);
        }
        if (primary is not null && !definition.LockedSeeds.Contains(nameof(DesignToken.ColorPrimary)))
        {
            builder.WithToken(nameof(DesignToken.ColorPrimary), primary);
        }
        if (neutral is not null && !definition.LockedSeeds.Contains(GalleryWorkspaceViewModel.NeutralSeedName))
        {
            builder.WithLanguageToken(GalleryWorkspaceViewModel.NeutralSeedName, neutral);
        }
        foreach (var (identity, control) in layer.Controls)
        {
            var local = new ControlThemeConfigBuilder();
            if (control.Appearance is { } controlAppearance)
            {
                local.WithAppearance(controlAppearance);
            }
            if (control.Density is { } controlDensity)
            {
                local.WithDensity(controlDensity);
            }
            foreach (var token in control.GlobalTokens.Concat(control.OwnTokens))
            {
                local.WithToken(token.Descriptor.Name, token.CanonicalValue);
            }
            builder.WithControl(identity, local.Build());
        }
        var config = builder.Build();
        _configs.Add(key, config);
        return config;
    }

    internal string Export(ThemePresetChoice choice, ThemeConfig config)
    {
        var id = choice.PreviewId;
        var catalog = GetCatalog();
        if (choice.Style is not null && catalog.Contains(id))
        {
            var previewId = id + ".Preview";
            id = previewId;
            for (var suffix = 2; catalog.Contains(id); suffix++)
            {
                id = previewId + "." + suffix.ToString(CultureInfo.InvariantCulture);
            }
        }
        return Export(id, choice.Name, config, choice.Info.LockedSeeds);
    }

    /// <summary>
    /// The previewed configuration as a v2 theme definition (spec 7.2): the language, the style and the variant's axes
    /// fixed, the global and language tokens, and the control entries.
    /// </summary>
    internal static string Export(string id, string name, ThemeConfig config, IReadOnlyCollection<string>? lockedSeeds = null)
    {
        XNamespace ns = "https://atomui.net/schemas/theme/v2";
        XElement? Tokens(string element, IReadOnlyDictionary<string, string> values,
            IReadOnlyCollection<string>? locks = null) => values.Count == 0
            ? null
            : new XElement(ns + element, values.OrderBy(static value => value.Key, StringComparer.Ordinal).Select(value =>
                new XElement(ns + "Token", new XAttribute("Name", value.Key), new XAttribute("Value", value.Value),
                    locks?.Contains(value.Key) == true ? new XAttribute("Locked", true) : null)));
        var root = new XElement(ns + "Theme", new XAttribute("Id", id), new XAttribute("Name", name),
            new XAttribute("Language", config.Language ?? "AntDesign"),
            config.Style is { } style ? new XAttribute("Style", style.Id) : null,
            config.Appearance is { } appearance ? new XAttribute("Appearance", appearance.ToString()) : null,
            config.Density is { } density ? new XAttribute("Density", density.Id) : null,
            Tokens("Tokens", config.Tokens, lockedSeeds), Tokens("LanguageTokens", config.LanguageTokens, lockedSeeds),
            // The schema requires at least one Control inside Controls, so a profile without control values omits it.
            config.Controls.Count == 0 ? null : new XElement(ns + "Controls", config.Controls.Select(pair =>
                new XElement(ns + "Control", new XAttribute("Catalog", pair.Key.Catalog), new XAttribute("Id", pair.Key.Id),
                    pair.Value.Appearance is { } controlAppearance ? new XAttribute("Appearance", controlAppearance.ToString()) : null,
                    pair.Value.Density is { } controlDensity ? new XAttribute("Density", controlDensity.Id) : null,
                    Tokens("Tokens", pair.Value.Tokens)))));
        return new XDocument(new XDeclaration("1.0", "utf-8", null), root).ToString();
    }
}
