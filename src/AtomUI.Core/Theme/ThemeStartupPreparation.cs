using AtomUI.Theme.Compilation;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Theme;

internal sealed record ThemeStartupInputs(
    ThemeRequest? InitialRequest,
    ThemeSelection? InitialSelection,
    ThemeRequest? FollowSystemLightRequest,
    ThemeRequest? FollowSystemDarkRequest,
    IReadOnlyList<ThemeDefinitionRegistration> Resolvers,
    string ApplicationId,
    string ApplicationDataRoot,
    FontFamily? FontFamily,
    IReadOnlyList<DesignLanguageDescriptor> Languages,
    IReadOnlyDictionary<string, string>? DefaultThemes);

internal sealed class PreparedThemeStartup
{
    private readonly ThemeSnapshot? _light;
    private readonly ThemeSnapshot? _dark;
    private readonly Lazy<ThemeSnapshot>? _deferredLight;
    private readonly Lazy<ThemeSnapshot>? _deferredDark;

    internal PreparedThemeStartup(ThemeSnapshot light, ThemeSnapshot? dark)
    {
        _light = light;
        _dark = dark;
    }

    internal PreparedThemeStartup(Func<ThemeAppearance, ThemeSnapshot> compile)
    {
        _deferredLight = new Lazy<ThemeSnapshot>(() => compile(ThemeAppearance.Light));
        _deferredDark = new Lazy<ThemeSnapshot>(() => compile(ThemeAppearance.Dark));
    }

    internal ThemeSnapshot Light => _light ?? _deferredLight!.Value;

    internal ThemeSnapshot? Dark => _dark ?? _deferredDark?.Value;

    internal ThemeSnapshot Select(ThemeAppearance appearance) =>
        appearance == ThemeAppearance.Dark && Dark is { } dark ? dark : Light;

    internal IReadOnlyDictionary<object, object?> GlobalResources(ThemeAppearance appearance) =>
        Select(appearance).GlobalResources;
}

/// <summary>
/// 后台 Prepare：只做纯数据工作（加载主题目录、编译全局层），可在任意线程执行。
/// 使用"仅全局"registry，结果作为提交时初始主题编译的 ReusableParent。
/// </summary>
internal static class ThemeStartupPreparation
{
    internal static PreparedThemeStartup Prepare(ThemeStartupInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        var registry = ThemeSchemaRegistry.CreateGlobalOnly(inputs.Languages);
        var catalogResult = CompiledThemeCatalog.LoadInitial(
            registry,
            inputs.Resolvers,
            new ThemeDefinitionResolveContext(inputs.ApplicationId, inputs.ApplicationDataRoot, false, 0),
            defaultThemes: inputs.DefaultThemes);
        if (!catalogResult.Success)
        {
            throw new ThemeLoadException(CompiledThemeCatalog.Describe(catalogResult.Diagnostics), catalogResult.Exception);
        }

        var catalog = catalogResult.Catalog!;
        var compiler = new ThemeCompiler();
        var (initial, lightRequest, darkRequest) = ThemeStartupCompilation.ResolveStartupRequests(inputs.InitialRequest,
            inputs.InitialSelection, inputs.FollowSystemLightRequest, inputs.FollowSystemDarkRequest, catalog);
        var lightTarget = lightRequest ?? initial;
        if (darkRequest is not null && catalog.Get(lightTarget.ThemeId).Info.Language != catalog.Get(darkRequest.ThemeId).Info.Language)
        {
            // The system appearance is known at commit. Preparing both here would evaluate an unused language.
            return new PreparedThemeStartup(appearance => Compile(
                appearance == ThemeAppearance.Dark ? darkRequest : lightTarget, initial, catalog, registry, inputs, compiler));
        }
        var light = Compile(lightTarget, initial, catalog, registry, inputs, compiler);
        var dark = darkRequest is not null
            ? Compile(darkRequest, initial, catalog, registry, inputs, compiler)
            : null;
        return new PreparedThemeStartup(light, dark);
    }

    private static ThemeSnapshot Compile(ThemeRequest request, ThemeRequest initial, CompiledThemeCatalog catalog,
        ThemeSchemaRegistry registry, ThemeStartupInputs inputs, ThemeCompiler compiler)
    {
        var result = ThemeStartupCompilation.CompileInitial(
            request, inputs.InitialRequest is null ? null : initial, catalog, registry, inputs.FontFamily, compiler);
        if (!result.Success)
        {
            throw new ThemeLoadException(
                $"Startup global theme '{request.ThemeId}' failed to compile.", result.Exception);
        }
        return result.Snapshot!;
    }
}
