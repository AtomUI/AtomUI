using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.TreeSelectControl;

public class TreeSelectThemeContractTests
{
    [Fact]
    public void TreeSelect_Multiple_Empty_Small_Uses_Small_Content_Padding()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectAddOnDecoratedBoxTheme.axaml");
        var owner  = ReadRepoFile("src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectTheme.axaml");

        // The shared decorated box reads the Small single-mode padding through the SelectTag theme input,
        // which the TreeSelect terminal maps from its own Token.
        Regex.IsMatch(
                owner,
                """
                <Style Selector="\^\[SizeType=Small\]">(?:(?!</Style>)[\s\S])*?<Setter Property="atom:SelectTag.ThemeSingleModePadding" Value="\{atom:TreeSelectTokenResource SingleModePaddingSM\}" />
                """,
                RegexOptions.CultureInvariant)
            .ShouldBeTrue("TreeSelect must choose its Small padding Token in the Small style.");
        Regex.IsMatch(
                source,
                """
                <Style Selector="\^\[IsSelectionEmpty=True\]">[\s\S]*?<Style Selector="\^\[SizeType=Small\]">[\s\S]*?<Setter Property="Padding"\s+Value="\{Binding \$self\.\(atom:SelectTag\.ThemeSingleModePadding\)\}" />
                """,
                RegexOptions.CultureInvariant)
            .ShouldBeTrue("empty multiple TreeSelect should use small select padding when SizeType is Small.");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate) || Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }
}
