using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests;

public class FixedAvaloniaMemberRetentionTests
{
    [Fact]
    public void Fixed_Avalonia_Adapters_Do_Not_Request_Whole_Member_Categories()
    {
        var files = new[]
        {
            "src/AtomUI.Controls/Buttons/AbstractIconButton.cs",
            "src/AtomUI.Controls/ItemsControl/ItemCollectionReflectionExtensions.cs",
            "src/AtomUI.Controls/ItemsControl/ItemsControlReflectionExtensions.cs",
            "src/AtomUI.Controls/Primitives/TopLevelReflectionExtensions.cs",
            "src/AtomUI.Controls/Primitives/VisualLayers/VisualLayerManagerReflectionExtensions.cs",
            "src/AtomUI.Controls/ScrollViewer/ScrollBarReflectionExtensions.cs"
        };

        foreach (var file in files)
        {
            var source = File.ReadAllText(Path.Combine(GetRepoRoot(), file));
            source.ShouldNotContain("DynamicallyAccessedMemberTypes.NonPublic", customMessage: file);
            source.ShouldNotContain("GetMethodInfoOrThrow", customMessage: file);
            source.ShouldNotContain("GetFieldInfoOrThrow", customMessage: file);
            source.ShouldNotContain("GetPropertyInfoOrThrow", customMessage: file);
            source.ShouldNotContain("GetEventInfoOrThrow", customMessage: file);
        }
    }

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src/AtomUI.Controls")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Unable to locate the AtomUI repository root.");
    }
}
