namespace AtomUI.Theme.Branding;

public sealed class ThemeBrandRecipeReadResult
{
    internal ThemeBrandRecipeReadResult(ThemeBrandRecipe? recipe, IEnumerable<ThemeDiagnostic> diagnostics)
    {
        Recipe = recipe;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
    }

    public ThemeBrandRecipe? Recipe { get; }
    public IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    public bool Success => Recipe is not null && Diagnostics.All(static item => item.Severity != ThemeDiagnosticSeverity.Error);
}
