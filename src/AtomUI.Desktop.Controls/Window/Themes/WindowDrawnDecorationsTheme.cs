using System.Runtime.Versioning;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AtomUI.Desktop.Controls.Themes;

[UnsupportedOSPlatform("browser")]
internal partial class WindowDrawnDecorationsTheme : ResourceDictionary
{
    public WindowDrawnDecorationsTheme() => AvaloniaXamlLoader.Load(this);
}
