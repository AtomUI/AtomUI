using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.Toolkits;

public class GalleryBaseExtractionTests
{
    [Fact]
    public void GalleryBase_Exposes_Neutral_And_Legacy_Xaml_Namespaces_Without_Product_ShowCases()
    {
        var assemblyInfo = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Properties/AssemblyInfo.cs");

        assemblyInfo.ShouldContain("https://atomui.net/toolkits/gallery-base");
        assemblyInfo.ShouldContain("https://atomui.net/oss-controls/gallery");
        assemblyInfo.ShouldContain("AtomUI.Toolkits.GalleryBase.Controls");
        assemblyInfo.ShouldContain("AtomUI.Toolkits.GalleryBase.Models");
        assemblyInfo.ShouldNotContain("AtomUIGallery.ShowCases");
        assemblyInfo.ShouldNotContain("AtomUIGallery.Localization");
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
