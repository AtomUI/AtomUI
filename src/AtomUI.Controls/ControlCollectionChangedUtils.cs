using System.Collections;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace AtomUI.Controls;

internal static class ControlCollectionChangedUtils
{
    public static void InsertControls(IAvaloniaList<Control> target, int index, IList source)
    {
        var controls = CollectControls(source);
        if (controls is not null)
        {
            target.InsertRange(index, controls);
        }
    }

    public static void InsertLogicalControls(IAvaloniaList<ILogical> target, int index, IList source)
    {
        var controls = CollectLogicalControls(source);
        if (controls is not null)
        {
            target.InsertRange(index, controls);
        }
    }

    public static void InsertVisuals(IAvaloniaList<Visual> target, int index, IList source)
    {
        var visuals = CollectVisuals(source);
        if (visuals is not null)
        {
            target.InsertRange(index, visuals);
        }
    }

    public static List<Control>? CollectControls(IList source)
    {
        List<Control>? controls = null;

        for (var i = 0; i < source.Count; i++)
        {
            if (source[i] is Control control)
            {
                controls ??= new List<Control>(source.Count - i);
                controls.Add(control);
            }
        }

        return controls;
    }

    private static List<ILogical>? CollectLogicalControls(IList source)
    {
        List<ILogical>? controls = null;

        for (var i = 0; i < source.Count; i++)
        {
            if (source[i] is Control control)
            {
                controls ??= new List<ILogical>(source.Count - i);
                controls.Add(control);
            }
        }

        return controls;
    }

    private static List<Visual>? CollectVisuals(IList source)
    {
        List<Visual>? visuals = null;

        for (var i = 0; i < source.Count; i++)
        {
            if (source[i] is Visual visual)
            {
                visuals ??= new List<Visual>(source.Count - i);
                visuals.Add(visual);
            }
        }

        return visuals;
    }
}
