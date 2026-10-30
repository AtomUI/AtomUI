using System.Runtime.Versioning;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AtomUI.Desktop.Controls.Themes;

[UnsupportedOSPlatform("browser")]
internal partial class OtpTextBoxTheme : ResourceDictionary
{
    public OtpTextBoxTheme() => AvaloniaXamlLoader.Load(this);
}
