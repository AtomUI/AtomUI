using System.Reflection;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

using AvaloniaVisualLayerManager = Avalonia.Controls.Primitives.VisualLayerManager;

internal static class VisualLayerManagerReflectionExtensions
{
    #region 反射信息定义
    private static readonly Lazy<MethodInfo> AddLayerMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(AvaloniaVisualLayerManager).GetMethod("AddLayer", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaVisualLayerManager),
            "AddLayer"));
    
    private static readonly Lazy<PropertyInfo> PopupOverlayLayerPropertyInfo = new(() =>
        FixedMemberReflection.RequireProperty(
            typeof(AvaloniaVisualLayerManager).GetProperty("PopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaVisualLayerManager),
            "PopupOverlayLayer"));

    #endregion
    
    internal static void AddLayer(this AvaloniaVisualLayerManager visualLayerManager, Control layer, int zindex)
    {
        AddLayerMethodInfo.Value.Invoke(visualLayerManager, [layer, zindex]);
    }

    internal static Control? GetPopupOverlayLayer(this AvaloniaVisualLayerManager visualLayerManager)
    {
        return PopupOverlayLayerPropertyInfo.Value.GetValue(visualLayerManager) as Control;
    }

    internal static Control? GetPopupOverlayLayer(this Visual visual)
    {
        foreach (var ancestor in visual.GetSelfAndVisualAncestors())
        {
            if (ancestor is AvaloniaVisualLayerManager visualLayerManager &&
                visualLayerManager.GetPopupOverlayLayer() is { } layer)
            {
                return layer;
            }
        }

        if (TopLevel.GetTopLevel(visual) is { } topLevel)
        {
            var visualLayerManager = FindFirstDescendantVisualLayerManager(topLevel);
            return visualLayerManager?.GetPopupOverlayLayer();
        }

        return null;
    }

    private static AvaloniaVisualLayerManager? FindFirstDescendantVisualLayerManager(Visual visual)
    {
        foreach (var descendant in visual.GetVisualDescendants())
        {
            if (descendant is AvaloniaVisualLayerManager visualLayerManager)
            {
                return visualLayerManager;
            }
        }

        return null;
    }
}
