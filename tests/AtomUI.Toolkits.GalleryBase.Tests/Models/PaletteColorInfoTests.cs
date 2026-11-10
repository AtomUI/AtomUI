using AtomUI.Toolkits.GalleryBase.Models;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Toolkits.GalleryBase.Tests.Models;

public class PaletteColorInfoTests
{
    [Theory]
    [InlineData(true, 4, "#FF000000")]
    [InlineData(true, 5, "#FFFFFFFF")]
    [InlineData(false, 4, "#FFFFFFFF")]
    [InlineData(false, 5, "#FF000000")]
    public void Reference_Swatch_Contrast_Changes_At_The_Middle_Of_The_Palette(bool light, int index, string expected)
    {
        var swatch = new PaletteColorInfo("Emerald", Brushes.Green, light, index);
        ((ISolidColorBrush)swatch.TextBrush).Color.ShouldBe(Color.Parse(expected));
        swatch.Brush.ShouldBeSameAs(Brushes.Green);
    }
}
