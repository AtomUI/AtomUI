using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests;

public class FixedAvaloniaMemberRetentionTests
{
    [Fact]
    public void Fixed_Avalonia_Adapters_Do_Not_Request_Whole_Member_Categories()
    {
        var files = new[]
        {
            "src/AtomUI.Core/Reflection/AnimatableReflectionExtensions.cs",
            "src/AtomUI.Core/Controls/FocusManagerReflectionExtensions.cs",
            "src/AtomUI.Core/Controls/ItemCollectionReflectionExtensions.cs",
            "src/AtomUI.Core/Controls/RawPointerEventTypeReflectionExtensions.cs",
            "src/AtomUI.Core/Controls/VisualReflectionExtensions.cs",
            "src/AtomUI.Core/Reflection/StyledElementReflectionExtensions.cs",
            "src/AtomUI.Core/Utils/AvaloniaPropertyReflectionExtensions.cs"
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
            if (Directory.Exists(Path.Combine(directory.FullName, "src/AtomUI.Core")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Unable to locate the AtomUI repository root.");
    }
}
