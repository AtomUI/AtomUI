using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class ComboBoxVirtualizingStackPanel : VirtualizingStackPanel
{
    protected override Type StyleKeyOverride => typeof(VirtualizingStackPanel);

    protected override Control? ScrollIntoView(int index)
    {
        // Avalonia's selected-item handler requests this before filling Text.
        // A selection being committed must not recycle or scroll the leaving view.
        if (ItemsControl is ComboBox { ShouldDeferFilterProjection: true })
        {
            return ContainerFromIndex(index);
        }

        return base.ScrollIntoView(index);
    }
}
