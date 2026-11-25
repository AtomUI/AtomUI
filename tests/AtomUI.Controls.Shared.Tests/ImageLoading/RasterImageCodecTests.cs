using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.ImageLoading;

public class RasterImageCodecTests
{
    [Theory]
    [InlineData(1024, 1024, 400, 208, false, 208, 208)]
    [InlineData(1024, 1024, 400, 208, true, 400, 400)]
    [InlineData(2048, 512, 400, 208, false, 400, 100)]
    [InlineData(2048, 512, 400, 208, true, 832, 208)]
    [InlineData(512, 2048, 400, 208, true, 400, 1600)]
    [InlineData(1024, 1024, 400, 0, true, 400, 400)]
    [InlineData(100, 100, 400, 208, true, 100, 100)]
    [InlineData(4000, 3000, 2147483632, 2147483632, true, 4000, 3000)]
    public void Raster_target_size_contains_or_covers_the_box_without_upscaling(
        int originalWidth,
        int originalHeight,
        int boxWidth,
        int boxHeight,
        bool cover,
        int expectedWidth,
        int expectedHeight)
    {
        RasterImageCodec.CalculateTargetSize(originalWidth, originalHeight, boxWidth, boxHeight, cover)
                        .ShouldBe((expectedWidth, expectedHeight));
    }
}
