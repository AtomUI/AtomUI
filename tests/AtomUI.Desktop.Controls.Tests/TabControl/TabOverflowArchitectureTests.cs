using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabOverflowArchitectureTests
{
    private static readonly string[] OwnerThemePaths =
    [
        "src/AtomUI.Desktop.Controls/TabControl/Themes/TabControlTheme.axaml",
        "src/AtomUI.Desktop.Controls/TabControl/Themes/CardTabControlTheme.axaml",
        "src/AtomUI.Desktop.Controls/TabControl/Themes/TabStrip/TabStripTheme.axaml",
        "src/AtomUI.Desktop.Controls/TabControl/Themes/TabStrip/CardTabStripTheme.axaml"
    ];

    [Fact]
    public void Four_Owner_Themes_Use_One_Shared_Viewer_And_Template_Boundary()
    {
        foreach (var path in OwnerThemePaths)
        {
            var source = ReadRepositoryFile(path);
            source.ShouldContain("<atom:TabScrollViewer");
            source.ShouldContain("OverflowPopupTemplate=\"{TemplateBinding OverflowPopupTemplate}\"");
            source.ShouldContain("IsPopupPinnedOpen=\"{TemplateBinding IsPopupPinnedOpen}\"");
        }
    }

    [Fact]
    public void Overflow_Popup_Uses_Direct_State_Instead_Of_Relay_Or_String_Bindings()
    {
        var source = ReadRepositoryFile("src/AtomUI.Desktop.Controls/TabControl/TabScrollViewer.cs");
        var theme = ReadRepositoryFile("src/AtomUI.Desktop.Controls/TabControl/Themes/TabScrollViewerTheme.axaml");

        source.ShouldNotContain("RelayBind");
        theme.ShouldContain("RequestedPlacement=\"{TemplateBinding OverflowPopupPlacement}\"");
        theme.ShouldNotContain("RequestedPlacement=\"{Binding");
        theme.ShouldNotContain("IsOpen=\"{Binding");
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/CardTabControl.cs")]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/TabStrip/CardTabStrip.cs")]
    public void Card_Owners_Detach_The_Previous_Add_Button_Before_Retemplating(string path)
    {
        var source = ReadRepositoryFile(path);

        source.ShouldContain("_addTabButton.Click -= HandleAddButtonClicked");
        source.IndexOf("_addTabButton.Click -= HandleAddButtonClicked", StringComparison.Ordinal)
            .ShouldBeLessThan(source.IndexOf("e.NameScope.Find<IconButton>", StringComparison.Ordinal));
    }

    private static string ReadRepositoryFile(string relativePath)
    {
        return File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
               ?? throw new DirectoryNotFoundException("Unable to locate the AtomUI repository root.");
    }
}
