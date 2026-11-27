using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Theme;

/// <summary>
/// Pure functions of the startup theme compilation: they neither read nor write <c>ThemeManager</c> state and
/// touch no Avalonia object bound to the UI thread. <c>ThemeManager</c> and the background preparation share
/// them, so both derive the same effective config and the global layer compiled in the background can be reused.
/// </summary>
internal static class ThemeStartupCompilation
{
    private const string FontFamilyToken = "FontFamily";

    internal static ThemeConfig? AddDefaultFont(ThemeConfig? config, FontFamily? fontFamily)
    {
        if (fontFamily is null || config?.Tokens.ContainsKey(FontFamilyToken) == true)
        {
            return config;
        }

        var tokens = config is null
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(config.Tokens, StringComparer.Ordinal);
        tokens[FontFamilyToken] = fontFamily.ToString();
        return (config ?? ThemeConfig.Empty).WithTokens(tokens);
    }

    internal static ThemeConfig? StripControls(ThemeConfig? config)
    {
        if (config is null || config.Controls.Count == 0)
        {
            return config;
        }
        return config.WithoutControls();
    }

    /// <summary>
    /// The startup requests: the explicit initial request; else the initial selection resolved against the catalog (both
    /// appearances when it follows the system); else, with exactly one enabled language, that language's default variant.
    /// </summary>
    /// <exception cref="ThemeLoadException">
    /// Neither an initial request nor an initial selection is given, and the number of enabled languages is not
    /// exactly one.
    /// </exception>
    internal static (ThemeRequest Initial, ThemeRequest? Light, ThemeRequest? Dark) ResolveStartupRequests(
        ThemeRequest? initialRequest,
        ThemeSelection? initialSelection,
        ThemeRequest? followSystemLight,
        ThemeRequest? followSystemDark,
        CompiledThemeCatalog catalog)
    {
        if (initialRequest is not null)
        {
            return (initialRequest, followSystemLight, followSystemDark);
        }

        if (initialSelection is not null)
        {
            if (initialSelection.FollowSystem)
            {
                var light = catalog.Resolve(initialSelection, ThemeAppearance.Light)
                    .Request with { Reason = ThemeTransitionReason.FollowSystem };
                var dark = catalog.Resolve(initialSelection, ThemeAppearance.Dark)
                    .Request with { Reason = ThemeTransitionReason.FollowSystem };
                return (light, light, dark);
            }

            var resolved = catalog.Resolve(initialSelection)
                .Request with { Reason = ThemeTransitionReason.Startup };
            return (resolved, null, null);
        }

        if (catalog.Languages.Count == 1)
        {
            return (new ThemeRequest(catalog.DefaultThemeId(catalog.Languages[0].Id), null,
                ThemeTransitionReason.Startup), null, null);
        }

        throw new ThemeLoadException(catalog.Languages.Count == 0
            ? "No design language is enabled; enable one (for example builder.UseAntDesign(...))."
            : "Several design languages are enabled; call WithInitialDesignLanguage<TLanguage>() or WithInitialTheme(ThemeSelection).");
    }

    /// <summary>
    /// Layers the application config (<paramref name="initial" />) and then the runtime config
    /// (<paramref name="runtime" />) over the theme definition defaults (<paramref name="defaults" />).
    /// </summary>
    internal static NormalizedThemeConfig MergeEffectiveConfig(
        NormalizedThemeConfig defaults,
        NormalizedThemeConfig initial,
        NormalizedThemeConfig runtime)
    {
        var applicationConfig = ThemeConfigMerger.Merge(defaults, null, initial).EffectiveConfig;
        return ThemeConfigMerger.Merge(defaults, applicationConfig, runtime).EffectiveConfig;
    }

    /// <summary>
    /// Compiles <paramref name="request" /> with the config layers of <c>ThemeManager.PrepareCompiledTheme</c>: the
    /// runtime layer is the config of <paramref name="request" />, the application layer is the config of
    /// <paramref name="initialRequest" />, and a missing config is replaced by one that holds only the default font.
    /// With a global-only registry, the controls are removed from the given configs before they are normalized.
    /// </summary>
    /// <returns>
    /// The compile result; a failed compilation is reported through <c>Success</c> and not thrown.
    /// </returns>
    /// <exception cref="ThemeLoadException">
    /// The config of <paramref name="request" /> or of <paramref name="initialRequest" /> is invalid.
    /// </exception>
    internal static ThemeCompileResult CompileInitial(
        ThemeRequest request,
        ThemeRequest? initialRequest,
        CompiledThemeCatalog catalog,
        ThemeSchemaRegistry registry,
        FontFamily? fontFamily,
        ThemeCompiler compiler)
    {
        var runtimeDefault = ThemeConfigNormalizer.Normalize(AddDefaultFont(null, fontFamily) ?? ThemeConfig.Empty, registry);
        var runtime = Normalize(request.Config, runtimeDefault, registry, fontFamily);
        var initial = Normalize(initialRequest?.Config, runtimeDefault, registry, fontFamily);
        if (!runtime.Success || !initial.Success)
        {
            throw new ThemeLoadException(
                $"Initial theme config for '{request.ThemeId}' is invalid.");
        }

        var entry = catalog.Get(request.ThemeId);
        var defaults = ThemeCompiler.CreateDefinitionDefaults(entry.Definition, registry);
        var effective = MergeEffectiveConfig(defaults, initial.Config!, runtime.Config!);
        return compiler.Compile(new ThemeCompileInput(entry.Definition, entry.Revision, effective, registry, null, null));
    }

    private static ThemeConfigNormalizeResult Normalize(
        ThemeConfig? config,
        ThemeConfigNormalizeResult runtimeDefault,
        ThemeSchemaRegistry registry,
        FontFamily? fontFamily)
    {
        if (config is null)
        {
            return runtimeDefault;
        }
        var scoped = registry.IsGlobalOnly ? StripControls(config)! : config;
        return ThemeConfigNormalizer.Normalize(AddDefaultFont(scoped, fontFamily)!, registry);
    }
}
