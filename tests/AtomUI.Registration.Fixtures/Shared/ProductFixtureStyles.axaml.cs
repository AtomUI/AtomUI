using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace AtomUI.Registration.Fixtures;

internal partial class ProductFixtureStyles : Styles
{
    public ProductFixtureStyles()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
