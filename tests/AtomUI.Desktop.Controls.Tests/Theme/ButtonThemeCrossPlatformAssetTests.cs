using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

public class ButtonThemeCrossPlatformAssetTests
{
    [Fact]
    public void Button_Family_Does_Not_Define_Browser_Theme_Overrides()
    {
        var repositoryRoot = GetRepositoryRoot();

        File.Exists(Path.Combine(
            repositoryRoot,
            "src/AtomUI.Desktop.Controls/Buttons/Themes/BrowserButtonThemes.axaml")).ShouldBeFalse();
        Directory.Exists(Path.Combine(
            repositoryRoot,
            "src/AtomUI.Desktop.Controls/Buttons/Themes/Browser")).ShouldBeFalse();
    }

    [Fact]
    public void Generated_Manifest_Uses_Common_Button_Family_Theme_Assets()
    {
        var manifest = File.ReadAllText(Path.Combine(
            GetRepositoryRoot(),
            "src/AtomUI.Desktop.Controls/GeneratedFiles/AtomUI.Generator/AtomUI.Generator.TokenResourceKeyGenerator/GeneratedTypeMapRegistration.g.cs"));

        manifest.ShouldContain("Buttons/Themes/ButtonTheme.axaml");
        manifest.ShouldContain("Buttons/Themes/DropdownButtonTheme.axaml");
        manifest.ShouldContain("Buttons/Themes/IconButtonTheme.axaml");
        manifest.ShouldContain("Buttons/Themes/SplitButtonTheme.axaml");
        manifest.ShouldContain("Buttons/Themes/HyperLinkButtonTheme.axaml");
        manifest.ShouldNotContain("Buttons/Themes/Browser/");
        manifest.ShouldNotContain("Buttons/Themes/BrowserButtonThemes.axaml");
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate repository root.");
    }
}
