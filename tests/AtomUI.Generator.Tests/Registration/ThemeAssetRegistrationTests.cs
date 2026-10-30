using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.Registration.TypeMapRegistrationGeneratorTests;

namespace AtomUI.Generator.Tests.Registration;

public class ThemeAssetRegistrationTests
{
    private const string Controls = "namespace Demo { public class Button : Avalonia.Controls.Control { } public class Alert : Avalonia.Controls.Control { } }";

    [Theory]
    [InlineData("Themes/RatingTheme.axaml")]
    [InlineData("Rating/Themes/RatingTheme.axaml")]
    public void Explicit_Export_Generates_Shared_Token_Surface_Without_An_Own_Token(string path)
    {
        var asset = new TextFile(path, "<ResourceDictionary xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:local=\"using:Demo\" xmlns:tokens=\"using:Demo.DesignTokens\"><ControlTheme x:Key=\"{x:Type local:Rating}\" TargetType=\"local:Rating\"><Setter Property=\"MinHeight\" Value=\"{tokens:RatingTokenResource ControlHeight}\" /></ControlTheme></ResourceDictionary>");
        var result = Run("namespace Demo { public class Rating : Avalonia.Controls.Control { } }", asset);
        result.Diagnostics.ShouldBeEmpty();
        var tokens = result.GeneratedTrees.Single(tree => tree.FilePath.EndsWith("TokenResourceConst.g.cs")).ToString();
        tokens.ShouldContain("public enum RatingTokenKey");
        tokens.ShouldContain("public static class RatingTokens");
        tokens.ShouldContain("public class RatingTokenResourceExtension");
        tokens.ShouldNotContain("RatingTokenKind");
        var schema = result.GeneratedTrees.Single(tree => tree.FilePath.EndsWith("GeneratedThemeSchema.g.cs")).ToString();
        schema.ShouldContain("typeof(global::Demo.Rating)");
        schema.ShouldNotContain("new global::Demo.RatingToken()");
    }

    [Fact]
    public void Typed_Export_Uses_The_Authors_Explicit_Control_Catalog()
    {
        var result = Run("namespace Demo { public class Rating : Avalonia.Controls.Control { } }",
            new Options(globalOptions: new() { ["build_property.AtomUIThemeControlCatalog"] = "Acme.Controls" }),
            Asset("Rating", "<ControlTheme x:Key=\"{x:Type local:Rating}\" TargetType=\"local:Rating\" />"));
        result.Diagnostics.ShouldBeEmpty();
        foreach (var name in new[] { "TokenResourceConst.g.cs", "GeneratedThemeSchema.g.cs" })
            result.GeneratedTrees.Single(tree => tree.FilePath.EndsWith(name)).ToString()
                .ShouldContain("ControlTokenIdentity.ForControl(typeof(global::Demo.Rating), \"Acme.Controls\", \"Rating\")");
    }

    [Fact]
    public void Explicit_Export_Target_Is_Not_Rejected_By_Unrelated_Same_Name_Controls()
    {
        var result = Run("namespace Demo { public class Button : Avalonia.Controls.Control { } } namespace Other { public class Button : Avalonia.Controls.Control { } }",
            Asset("Button", "<ControlTheme x:Key=\"{x:Type local:Button}\" TargetType=\"local:Button\" />"));
        result.Diagnostics.ShouldBeEmpty();
        var source = result.GeneratedTrees.Single(tree => tree.FilePath.EndsWith("GeneratedTypeMapRegistration.g.cs")).ToString();
        source.ShouldContain("typeof(global::Demo.Button)");
        source.ShouldNotContain("typeof(global::Other.Button)");
        source.ShouldContain("avares://TypeMapFixture/Button/Themes/ButtonTheme.axaml");
    }

    [Fact]
    public void File_And_Directory_Names_Do_Not_Override_Declared_Export_Targets()
    {
        var result = Run(Controls, Asset("Button", "<ControlTheme x:Key=\"{x:Type local:Alert}\" TargetType=\"local:Alert\" />"));
        result.Diagnostics.ShouldBeEmpty();
        var factories = result.GeneratedTrees.Single(tree => tree.FilePath.EndsWith("GeneratedTypeMapRegistration.g.cs")).ToString();
        factories.ShouldContain("typeof(global::Demo.Alert)");
        factories.ShouldNotContain("typeof(global::Demo.Button)");
    }

    [Fact]
    public void Aggregated_Dictionary_Preserves_Each_Explicit_Export_And_Uses_A_Resource_Factory()
    {
        var result = Run(Controls, Asset("AllThemes", "<ControlTheme x:Key=\"{x:Type local:Button}\" TargetType=\"local:Button\" /><ControlTheme x:Key=\"NamedAlert\" TargetType=\"local:Alert\" />"));
        result.Diagnostics.ShouldBeEmpty();
        var source = result.GeneratedTrees.Single(tree => tree.FilePath.EndsWith("GeneratedTypeMapRegistration.g.cs")).ToString();
        source.ShouldContain("typeof(global::Demo.Button)");
        source.ShouldContain("typeof(global::Demo.Alert)");
        source.ShouldContain("\"NamedAlert\"");
        source.ShouldContain("() => new global::AtomUI.Generated.TypeMapFixture.GeneratedThemeAssetResource_");
        source.ShouldNotContain("Activator.CreateInstance");
    }

    [Fact]
    public void Resource_Only_Include_Does_Not_Generate_An_Independent_Control_Contract()
    {
        var resources = Asset("Resources", "<SolidColorBrush x:Key=\"Accent\">Red</SolidColorBrush>");
        var theme = Asset("Button", "<ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"avares://TypeMapFixture/Resources/Themes/ResourcesTheme.axaml\" /></ResourceDictionary.MergedDictionaries><ControlTheme x:Key=\"{x:Type local:Button}\" TargetType=\"local:Button\"><Setter Property=\"Background\" Value=\"{StaticResource Accent}\" /></ControlTheme>");
        var result = Run(Controls, resources, theme);
        result.Diagnostics.ShouldBeEmpty();
        var source = result.GeneratedTrees.Single(tree => tree.FilePath.EndsWith("GeneratedTypeMapRegistration.g.cs")).ToString();
        source.ShouldNotContain("avares://TypeMapFixture/Resources/Themes/ResourcesTheme.axaml");
        source.ShouldContain("avares://TypeMapFixture/Button/Themes/ButtonTheme.axaml");
    }

    [Fact]
    public void Duplicate_Logical_Asset_Paths_Fail_Instead_Of_Overwriting_An_Export()
    {
        var result = Run(Controls, Asset("Button", "<ControlTheme x:Key=\"One\" TargetType=\"local:Button\" />"),
            Asset("Button", "<ControlTheme x:Key=\"Two\" TargetType=\"local:Button\" />"));
        result.Diagnostics.ShouldContain(d => d.Id == "ATOMUIREG005");
    }
}
