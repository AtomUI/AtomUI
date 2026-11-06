using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Branding;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Registration;
using System.Globalization;
namespace AtomUI.Registration.Fixtures;
public partial class ProductApp
{
    public void PrepareRetainedState()
    {
        _textArea.Text = "retained Windows input";
        _textArea.SelectionStart = 2;
        _textArea.SelectionEnd = 8;
    }
    public async Task VerifyExtraAsync()
    {
        Require(ControlRegistrationRuntime.IsTrimmed, "selected registration runtime IsTrimmed true");
        var manager = (ThemeManager)this.GetThemeManager()!;
        var snapshot = manager.RootContext.Snapshot;
        // A complete recipe for this finite selected scope. ExportCurrent intentionally
        // enumerates the entire enabled catalog and would request unrelated control schemas.
        var config = new ThemeConfigBuilder().WithInherit(false)
            .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark)
            .WithToken(nameof(DesignToken.FontSize), "21")
            .WithControl(ButtonTokens.Identity, new ControlThemeConfigBuilder()
                .WithToken("ContentFontSize", "27").Build()).Build();
        var recipe = new ThemeBrandRecipe("windows-cold", "1", snapshot.ThemeId, config,
            Array.Empty<ThemeStyleAssetReference>(),
            new ThemeBrandRequirements(snapshot.RegistryRevision.Value.ToString("X16", CultureInfo.InvariantCulture),
                snapshot.DefinitionRevision.ContentDigest, snapshot.DefinitionRevision.ProgramDigest,
                (ThemeStyleCapabilities)0), ThemeBrandRecipeKind.Complete);
        var json = ThemeBrandRecipes.Write(recipe);
        Require(json.Contains("windows-cold"), "complete selected JSON writes identity");
        var parsed = ThemeBrandRecipes.Read(manager, json);
        Require(parsed.Success && parsed.Recipe is not null, "complete selected JSON reads against actual schema requirements");
        Require(parsed.Recipe!.Kind == ThemeBrandRecipeKind.Complete && parsed.Recipe.Config.Controls.Count == 1,
            "complete selected JSON retains selected Button override");
        Require(ThemeBrandRecipes.Write(parsed.Recipe) == json, "complete selected JSON write read roundtrip");
        var applied = await ThemeBrandRecipes.ApplyAsync(manager, parsed.Recipe);
        Require(applied.Status == ThemeTransitionStatus.Committed, "complete selected JSON applies");
        Require(ThemeBrandRecipes.GetSelection(manager)?.Id == "windows-cold", "complete JSON selected recipe commits identity");
        Require(_textArea.Text == "retained Windows input", "theme update retains input text");
        Require(_textArea.SelectionStart == 2 && _textArea.SelectionEnd == 8, "theme update retains input selection");
    }
}
