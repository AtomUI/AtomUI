using AtomUI.Utils;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.ImagePreviewer;

public class AssetsLoaderTests
{
    public AssetsLoaderTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void OpenStream_Loads_Local_Absolute_File_Path()
    {
        var path = CreateSampleFile();

        using var stream = AssetsLoader.OpenStream(path);
        using var reader = new StreamReader(stream);

        reader.ReadToEnd().ShouldBe("asset-loader-local-file");
    }

    [Fact]
    public void OpenStream_Loads_Local_Relative_File_Path()
    {
        var originalDirectory = Directory.GetCurrentDirectory();
        var directory         = CreateTempDirectory();
        try
        {
            Directory.SetCurrentDirectory(directory);
            File.WriteAllText("sample.txt", "asset-loader-relative-file");

            using var stream = AssetsLoader.OpenStream("./sample.txt");
            using var reader = new StreamReader(stream);

            reader.ReadToEnd().ShouldBe("asset-loader-relative-file");
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void OpenStream_Loads_Local_File_Uri()
    {
        var path = CreateSampleFile();
        var uri  = new Uri(path);

        using var stream = AssetsLoader.OpenStream(uri.AbsoluteUri);
        using var reader = new StreamReader(stream);

        reader.ReadToEnd().ShouldBe("asset-loader-local-file");
    }

    [Fact]
    public void OpenStream_Missing_Local_File_Throws_FileNotFoundException()
    {
        var missingPath = Path.Combine(CreateTempDirectory(), "missing.txt");

        Should.Throw<FileNotFoundException>(() => AssetsLoader.OpenStream(missingPath));
    }

    [Fact]
    public void OpenStream_Keeps_Avalonia_Asset_Uri_Support()
    {
        // The built-in theme XML is compiled into its language package now, so a test-owned resource stands in.
        using var stream = AssetsLoader.OpenStream(
            "avares://AtomUI.Desktop.Controls.Tests/ImagePreviewer/Assets/asset-loader-sample.xml");
        using var reader = new StreamReader(stream);

        reader.ReadLine().ShouldNotBeNull().ShouldContain("xml");
    }

    private static string CreateSampleFile()
    {
        var directory = CreateTempDirectory();
        var path      = Path.Combine(directory, "sample.txt");
        File.WriteAllText(path, "asset-loader-local-file");
        return path;
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"atomui-assets-loader-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }
}
