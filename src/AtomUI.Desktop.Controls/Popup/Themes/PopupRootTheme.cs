using AtomUI.Registration;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AtomUI.Desktop.Controls.Themes;

// The bootstrap menu uses this Avalonia-owned target; registration belongs to the local resource.
[BootstrapRegistration]
internal partial class PopupRootTheme : ResourceDictionary
{
    public PopupRootTheme() => AvaloniaXamlLoader.Load(this);
}
