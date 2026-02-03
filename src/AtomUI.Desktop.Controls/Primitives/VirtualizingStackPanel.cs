using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls.Primitives;

using AvaloniaVirtualizingStackPanel = Avalonia.Controls.VirtualizingStackPanel;

internal class VirtualizingStackPanel : AvaloniaVirtualizingStackPanel
{
    
    public void ScrollItemIntoView(int index)
    {
        ScrollIntoView(index);
    }
}
