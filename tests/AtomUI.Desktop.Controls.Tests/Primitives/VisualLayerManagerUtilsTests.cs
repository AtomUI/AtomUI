using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Primitives;

public class VisualLayerManagerUtilsTests
{
    [Fact]
    public void FindLayer_Returns_First_Registered_Layer_And_Ignores_Ordinary_Child()
    {
        var child = new MarkerLayer();
        var first = new MarkerLayer();
        var second = new MarkerLayer();
        var manager = new VisualLayerManager { Child = child };
        manager.AddLayer(first, 1);
        manager.AddLayer(second, 2);

        VisualLayerManagerUtils.FindLayer<MarkerLayer>(manager).ShouldBeSameAs(first);
    }

    [Fact]
    public void FindLayer_Remains_Accurate_After_Child_Is_Rehosted()
    {
        var registered = new MarkerLayer();
        var manager = new VisualLayerManager { Child = new MarkerLayer() };
        manager.AddLayer(registered, 1);

        manager.Child = new Border();

        VisualLayerManagerUtils.FindLayer<MarkerLayer>(manager).ShouldBeSameAs(registered);
    }

    [Fact]
    public void Reflection_Extension_Does_Not_Retain_Private_Layer_List()
    {
        var source = File.ReadAllText(Path.Combine(
            GetRepoRoot(),
            "src/AtomUI.Controls/Primitives/VisualLayers/VisualLayerManagerReflectionExtensions.cs"));

        source.ShouldNotContain("LayersFieldInfo");
        source.ShouldNotContain("\"_layers\"");
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

    private sealed class MarkerLayer : Border;
}
