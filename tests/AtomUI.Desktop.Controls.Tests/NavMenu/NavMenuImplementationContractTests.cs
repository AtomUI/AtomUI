using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuImplementationContractTests
{
    [Fact]
    public void Inline_Keyboard_Navigation_Does_Not_Flatten_Items_On_Each_Key()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/NavMenuInteractionHandlerBase.cs");

        source.ShouldNotContain("new List<NavMenuItem>");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Unable to locate repository file '{relativePath}'.");
    }
}
