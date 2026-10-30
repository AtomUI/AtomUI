using Avalonia.Controls;
namespace Fixture.BinaryAdapter;
public static class Adapter
{
    public static Control CreateControl() => new AtomUI.Desktop.Controls.SearchEdit { PlaceholderText = "ordinary binary adapter" };
}
