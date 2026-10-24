using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Tests.DataGrid;

internal sealed partial class ComponentWindowTheme : ControlTheme
{
    public ComponentWindowTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
