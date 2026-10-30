using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Aot;

public class FixedAvaloniaMemberRetentionTests
{
    [Fact]
    public void Fixed_Avalonia_Adapters_Do_Not_Request_Whole_Member_Categories()
    {
        var files = new[]
        {
            "src/AtomUI.Desktop.Controls/Input/Utils/TextBoxReflectionExtensions.cs",
            "src/AtomUI.Desktop.Controls/Popup/PopupReflectionExtensions.cs",
            "src/AtomUI.Desktop.Controls/Flyouts/PopupFlyoutBaseReflectionExtensions.cs",
            "src/AtomUI.Desktop.Controls/ComboBox/ComboBoxReflectionExtensions.cs",
            "src/AtomUI.Desktop.Controls/Menu/ContextMenuReflectionExtensions.cs",
            "src/AtomUI.Desktop.Controls/TabControl/ScrollContentPresenterReflectionExtensions.cs"
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

        var windowSource = File.ReadAllText(Path.Combine(
            GetRepoRoot(),
            "src/AtomUI.Desktop.Controls/Window/Utils/WindowDrawnDecorationsReflectionExtensions.cs"));
        windowSource.ShouldContain(
            "DynamicallyAccessedMemberTypes.NonPublicFields,\n        \"Avalonia.Controls.TopLevelHost\"");
        windowSource.ShouldContain(
            "DynamicallyAccessedMemberTypes.NonPublicProperties,\n        \"Avalonia.Controls.Chrome.ResizeGripLayer\"");
    }

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src/AtomUI.Desktop.Controls")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Unable to locate the AtomUI repository root.");
    }
}
