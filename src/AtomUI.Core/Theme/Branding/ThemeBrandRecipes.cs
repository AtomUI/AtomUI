using AtomUI.Theme.Configuration;

namespace AtomUI.Theme.Branding;

/// <summary>Versioned data recipes bound to explicitly registered assets in the existing theme runtime.</summary>
public static class ThemeBrandRecipes
{
    public static ThemeBrandRecipeReadResult Read(IThemeManager runtime, string json)
    {
        var manager = Runtime(runtime);
        manager.VerifyBrandReadAccess();
        var parsed = ThemeBrandRecipeJson.Read(json);
        return parsed.Success ? manager.ValidateBrandRecipe(parsed.Recipe!) : parsed;
    }

    public static string Write(ThemeBrandRecipe recipe) => ThemeBrandRecipeJson.Write(recipe);

    public static ThemeConfig CreateConfig(IThemeManager runtime, ThemeBrandRecipe recipe) =>
        Runtime(runtime).BindBrandRecipe(recipe);

    public static Task<ThemeTransitionResult> ApplyAsync(IThemeManager runtime, ThemeBrandRecipe recipe,
        CancellationToken cancellationToken = default) => Runtime(runtime).ApplyBrandRecipeAsync(recipe, cancellationToken);

    public static ThemeBrandRecipe ExportCurrent(IThemeManager runtime, string id, string revision,
        ThemeBrandRecipeKind kind = ThemeBrandRecipeKind.Complete) => Runtime(runtime).ExportBrandRecipe(id, revision, kind);

    public static ThemeBrandSelection? GetSelection(IThemeManager runtime) => Runtime(runtime).GetBrandSelection();

    private static ThemeManager Runtime(IThemeManager runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        return runtime as ThemeManager ?? throw new NotSupportedException("This runtime does not support version 1 brand recipes.");
    }
}
