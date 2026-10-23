using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Theme;

/// <summary>
/// 启动期主题编译的纯函数：不读写 ThemeManager 状态、不接触 Avalonia 线程对象，
/// 供 ThemeManager 与后台 Prepare 共用，保证两边的有效配置一致（全局层才能复用）。
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
        return new ThemeConfig(
            config?.Inherit ?? true,
            config?.Algorithms,
            tokens,
            config?.Controls ?? new Dictionary<ControlTokenIdentity, ControlThemeConfig>(),
            config?.StyleAssets, config?.BrandSelection, config?.BrandRequirements);
    }

    internal static ThemeConfig? StripControls(ThemeConfig? config)
    {
        if (config is null || config.Controls.Count == 0)
        {
            return config;
        }
        return new ThemeConfig(config.Inherit, config.Algorithms, config.Tokens,
            new Dictionary<ControlTokenIdentity, ControlThemeConfig>(), config.StyleAssets, config.BrandSelection,
            config.BrandRequirements);
    }

    /// <summary>主题定义默认值之上依次叠加应用层（初始请求）与运行时层（本次请求）。</summary>
    internal static NormalizedThemeConfig MergeEffectiveConfig(
        NormalizedThemeConfig defaults,
        NormalizedThemeConfig initial,
        NormalizedThemeConfig runtime)
    {
        var applicationConfig = ThemeConfigMerger.Merge(defaults, null, initial).EffectiveConfig;
        return ThemeConfigMerger.Merge(defaults, applicationConfig, runtime).EffectiveConfig;
    }

    /// <summary>
    /// 与 ThemeManager.PrepareCompiledTheme 的配置分层一一对应：运行时层取本次请求的配置，
    /// 应用层取初始请求的配置，二者为空时都退回仅含默认字体的配置。"仅全局"registry 下先去掉控件段。
    /// </summary>
    internal static ThemeCompileResult CompileInitial(
        ThemeRequest request,
        ThemeRequest initialRequest,
        CompiledThemeCatalog catalog,
        ThemeSchemaRegistry registry,
        FontFamily? fontFamily,
        ThemeCompiler compiler)
    {
        var runtimeDefault = ThemeConfigNormalizer.Normalize(AddDefaultFont(null, fontFamily) ?? ThemeConfig.Empty, registry);
        var runtime = Normalize(request.Config, runtimeDefault, registry, fontFamily);
        var initial = Normalize(initialRequest.Config, runtimeDefault, registry, fontFamily);
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
