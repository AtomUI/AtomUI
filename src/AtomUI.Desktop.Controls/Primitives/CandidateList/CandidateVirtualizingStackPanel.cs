using Avalonia.Controls;

namespace AtomUI.Desktop.Controls.Primitives;

internal class CandidateVirtualizingStackPanel : VirtualizingStackPanel
{
    public Control? ScrollCandidateItemIntoView(int index)
    {
        return ScrollIntoView(index);
    }
}