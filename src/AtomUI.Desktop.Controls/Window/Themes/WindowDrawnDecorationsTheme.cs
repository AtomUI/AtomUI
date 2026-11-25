using System.Runtime.Versioning;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes;

[UnsupportedOSPlatform("browser")]
internal partial class WindowDrawnDecorationsTheme : ResourceDictionary
{
    public static readonly IValueConverter CachedFrameShadow =
        new FuncValueConverter<BoxShadows, BoxShadows>(static shadows => shadows.HasInsetShadows ? default : shadows);

    // An inset shadow must be drawn above the fill in the original frame drawing operation.
    public static readonly IValueConverter UncachedFrameShadow =
        new FuncValueConverter<BoxShadows, BoxShadows>(static shadows => shadows.HasInsetShadows ? shadows : default);

    public WindowDrawnDecorationsTheme() => AvaloniaXamlLoader.Load(this);
}
