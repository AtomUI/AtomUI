using System.Reflection;
using System.Xml.Linq;
using AtomUIGallery.Workspace.Views;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.Workspace;

public class GalleryVersionInfoTests
{
    [Fact]
    public void Gallery_AtomUI_Version_Metadata_Comes_From_Version_Props()
    {
        var expectedVersion = ReadAtomUIVersionFromProps();
        var assembly        = typeof(WorkspaceWindow).Assembly;

        var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                               .SingleOrDefault(attribute => attribute.Key == "AtomUIVersion");

        metadata.ShouldNotBeNull();
        metadata.Value.ShouldBe(expectedVersion);
    }

    private static string ReadAtomUIVersionFromProps()
    {
        var path     = GetRepoFile("build/Versions.props");
        var document = XDocument.Load(path);
        var version  = document.Descendants("AtomUIVersion").SingleOrDefault()?.Value;

        version.ShouldNotBeNullOrWhiteSpace();
        return version;
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }
}
