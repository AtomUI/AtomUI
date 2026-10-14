using System.Runtime.Versioning;
using AtomUI.Registration;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AtomUI.Desktop.Controls.Themes;

[UnsupportedOSPlatform("browser")]
[BootstrapRegistration]
internal partial class WindowDrawnDecorationsTheme : ResourceDictionary
{
    public WindowDrawnDecorationsTheme() => AvaloniaXamlLoader.Load(this);
}
