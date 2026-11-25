using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

internal sealed class WindowFeedbackLayer : Panel
{
    internal static Panel? GetLayer(TopLevel topLevel)
    {
        var manager = LayerLadder.FindTopLevelLayerManager(topLevel);
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
            ZIndex              = LayerLadder.WindowFeedback
        };
        LayerLadder.AddLayer(manager, layer, LayerLadder.WindowFeedback);
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
}
