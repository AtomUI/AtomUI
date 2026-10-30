using System.Runtime.Versioning;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AtomUI.Desktop.Controls.Themes;

[UnsupportedOSPlatform("browser")]
internal partial class AdornerLayerTheme : ResourceDictionary
{
    public AdornerLayerTheme() => AvaloniaXamlLoader.Load(this);
}
