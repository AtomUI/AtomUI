using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal sealed class WindowFeedbackLayer : Panel
{
    private const int LayerZIndex = int.MaxValue - 50;

    internal static Panel? GetLayer(TopLevel topLevel)
    {
        var manager = FindFirstLayerManager(topLevel);
        if (manager is null)
        {
            return null;
        }

        var layer = VisualLayerManagerUtils.FindLayer<WindowFeedbackLayer>(manager);
        if (layer is not null)
        {
            return layer;
        }

        layer = new WindowFeedbackLayer
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment   = VerticalAlignment.Stretch,
            ZIndex              = LayerZIndex
        };
        manager.AddLayer(layer, LayerZIndex);
        return layer;
    }

    internal static void Activate(Panel? hostLayer, Control manager)
    {
        if (hostLayer is null)
        {
            return;
        }

        var children = hostLayer.Children;
        var lastIndex = children.Count - 1;
        if (lastIndex < 0 || ReferenceEquals(children[lastIndex], manager))
        {
            return;
        }

        var managerIndex = children.IndexOf(manager);
        if (managerIndex >= 0)
        {
            children.Move(managerIndex, lastIndex);
        }
    }

    private static VisualLayerManager? FindFirstLayerManager(TopLevel topLevel)
    {
        foreach (var descendant in topLevel.GetTemplateDescendants())
        {
            if (descendant is VisualLayerManager manager)
            {
                return manager;
            }
        }

        foreach (var descendant in topLevel.GetVisualDescendants())
        {
            if (descendant is VisualLayerManager manager)
            {
                return manager;
            }
        }

        return null;
    }
}
