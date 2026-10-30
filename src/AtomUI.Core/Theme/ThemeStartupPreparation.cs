using AtomUI.Theme.Compilation;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Theme;

internal sealed record ThemeStartupInputs(
    ThemeRequest InitialRequest,
    ThemeRequest? FollowSystemLightRequest,
    ThemeRequest? FollowSystemDarkRequest,
    IReadOnlyList<ThemeDefinitionRegistration> Resolvers,
    string ApplicationId,
    string ApplicationDataRoot,
    FontFamily? FontFamily);

internal sealed class PreparedThemeStartup
{
    internal PreparedThemeStartup(ThemeSnapshot light, ThemeSnapshot? dark)
    {
        Light = light;
        Dark = dark;
    }

    internal ThemeSnapshot Light { get; }

    internal ThemeSnapshot? Dark { get; }

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
        var registry = ThemeSchemaRegistry.CreateGlobalOnly();
        var catalogResult = CompiledThemeCatalog.LoadInitial(
            registry,
            inputs.Resolvers,
            new ThemeDefinitionResolveContext(inputs.ApplicationId, inputs.ApplicationDataRoot, false, 0));
        if (!catalogResult.Success)
        {
            throw new ThemeLoadException(CompiledThemeCatalog.Describe(catalogResult.Diagnostics), catalogResult.Exception);
        }

        var compiler = new ThemeCompiler();
        var light = Compile(inputs.FollowSystemLightRequest ?? inputs.InitialRequest, catalogResult.Catalog!, registry, inputs, compiler);
        var dark = inputs.FollowSystemDarkRequest is { } darkRequest
            ? Compile(darkRequest, catalogResult.Catalog!, registry, inputs, compiler)
            : null;
        return new PreparedThemeStartup(light, dark);
    }

    private static ThemeSnapshot Compile(ThemeRequest request, CompiledThemeCatalog catalog,
        ThemeSchemaRegistry registry, ThemeStartupInputs inputs, ThemeCompiler compiler)
    {
        var result = ThemeStartupCompilation.CompileInitial(
            request, inputs.InitialRequest, catalog, registry, inputs.FontFamily, compiler);
        if (!result.Success)
        {
            throw new ThemeLoadException(
                $"Startup global theme '{request.ThemeId}' failed to compile.", result.Exception);
        }
        return result.Snapshot!;
    }
}
