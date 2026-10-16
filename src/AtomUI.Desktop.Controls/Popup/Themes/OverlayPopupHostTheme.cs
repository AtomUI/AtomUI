using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AtomUI.Desktop.Controls.Themes;

// The bootstrap menu uses this Avalonia-owned target; registration belongs to the local resource.
internal partial class OverlayPopupHostTheme : ResourceDictionary
{
    public OverlayPopupHostTheme() => AvaloniaXamlLoader.Load(this);
}
