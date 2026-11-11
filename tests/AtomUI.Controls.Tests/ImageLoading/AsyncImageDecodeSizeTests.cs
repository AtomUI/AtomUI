using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests.ImageLoading;

public class AsyncImageDecodeSizeTests
{
    static AsyncImageDecodeSizeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private const double PageWidth = 390;
    private const double BoxHeight = 200;

    // A full-width image on a phone-sized page with a bounded height.
    private static (AsyncImage Image, ImageControlTestHost Host, Func<ImageOpenedEventArgs?> Opened) CreateBoundedBox(
        Stretch stretch,
        int sourceWidth,
        int sourceHeight,
        string identity)
    {
        ImageOpenedEventArgs? opened = null;
        var image = new AsyncImage
        {
            Height = BoxHeight,
            Stretch = stretch,
            Source = new BytesImageSource(ImageControlTestHost.CreatePng(sourceWidth, sourceHeight), identity)
        };
        image.ImageOpened += (_, args) => opened = args;
        var host = new ImageControlTestHost(image, width: PageWidth, height: BoxHeight);
        ImageControlTestHost.WaitUntil(
            () => image.LoadState is ImageLoadState.Loaded or ImageLoadState.Failed,
            $"{stretch} image terminal state");
        image.LoadError.ShouldBeNull();
        return (image, host, () => opened);
    }

    private static double RenderScaling(Visual visual) => TopLevel.GetTopLevel(visual)?.RenderScaling ?? 1;

    [Fact]
    public void Fill_with_bounded_height_decodes_to_cover_width()
    {
        var (image, host, opened) = CreateBoundedBox(Stretch.Fill, 1024, 1024, "fill-square");
        using (host)
        {
            var scaling = RenderScaling(image);
            var decoded = opened().ShouldNotBeNull();
            image.Bounds.Width.ShouldBe(PageWidth);
            decoded.PixelWidth.ShouldBeGreaterThanOrEqualTo((int)Math.Ceiling(image.Bounds.Width * scaling));
            decoded.PixelHeight.ShouldBeGreaterThanOrEqualTo((int)Math.Ceiling(image.Bounds.Height * scaling));
            // Covering the 390-wide box only needs the 400 px width bucket, not the 1024 px original.
            decoded.PixelWidth.ShouldBe(400);
            decoded.PixelHeight.ShouldBe(400);
        }
    }

    [Fact]
    public void UniformToFill_with_bounded_height_decodes_to_cover_width()
    {
        var (image, host, opened) = CreateBoundedBox(Stretch.UniformToFill, 1024, 1024, "cover-square");
        using (host)
        {
            var scaling = RenderScaling(image);
            var decoded = opened().ShouldNotBeNull();
            decoded.PixelWidth.ShouldBeGreaterThanOrEqualTo((int)Math.Ceiling(image.Bounds.Width * scaling));
            decoded.PixelHeight.ShouldBeGreaterThanOrEqualTo((int)Math.Ceiling(image.Bounds.Height * scaling));
            decoded.PixelWidth.ShouldBe(400);
            decoded.PixelHeight.ShouldBe(400);
        }
    }

    [Fact]
    public void UniformToFill_with_wide_source_decodes_to_cover_height()
    {
        // A 4:1 source is wider than the box, so the height decides the cover scale; a width-only cap would under-decode.
        var (image, host, opened) = CreateBoundedBox(Stretch.UniformToFill, 2048, 512, "cover-wide");
        using (host)
        {
            var scaling = RenderScaling(image);
            var decoded = opened().ShouldNotBeNull();
            decoded.PixelWidth.ShouldBeGreaterThanOrEqualTo((int)Math.Ceiling(image.Bounds.Width * scaling));
            // 2048 x 512 scaled to cover 400 x 208 is 832 x 208. The headless bitmap stub reports a square size, so the
            // decoded height is asserted against the codec directly below.
            decoded.PixelWidth.ShouldBe(832);
        }
    }

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

    [Fact]
    public void Uniform_with_bounded_height_still_fits_inside()
    {
        var (_, host, opened) = CreateBoundedBox(Stretch.Uniform, 1024, 1024, "uniform-square");
        using (host)
        {
            var decoded = opened().ShouldNotBeNull();
            decoded.PixelWidth.ShouldBe(208);
            decoded.PixelHeight.ShouldBe(208);
        }
    }

    [Fact]
    public void Changing_stretch_to_fill_redecodes_to_cover()
    {
        var (image, host, opened) = CreateBoundedBox(Stretch.Uniform, 1024, 1024, "uniform-then-fill");
        using (host)
        {
            opened().ShouldNotBeNull().PixelWidth.ShouldBe(208);

            image.Stretch = Stretch.Fill;
            ImageControlTestHost.WaitUntil(() => opened()?.PixelWidth == 400, "cover decode after Stretch change");
            opened().ShouldNotBeNull().PixelHeight.ShouldBe(400);
        }
    }

    // Vertical StackPanel (unbounded slot height), width-stretched, height bounded only by MaxHeight.
    private static (AsyncImage Image, ImageControlTestHost Host, List<ImageOpenedEventArgs> Opened) CreateContentSizedFill(
        Stretch stretch,
        int sourceWidth,
        int sourceHeight,
        string identity)
    {
        var opened = new List<ImageOpenedEventArgs>();
        var image = new AsyncImage
        {
            MaxHeight = 400,
            Stretch = stretch,
            Source = new BytesImageSource(ImageControlTestHost.CreatePng(sourceWidth, sourceHeight), identity)
        };
        image.ImageOpened += (_, args) => opened.Add(args);
        var panel = new StackPanel { Children = { image } };
        var host = new ImageControlTestHost(panel, width: PageWidth, height: 844);
        ImageControlTestHost.WaitUntil(() => opened.Count > 0, $"{stretch} content-sized image load");
        // Let the loaded image re-measure and re-arrange, then give any restart time to finish.
        var settle = System.Diagnostics.Stopwatch.StartNew();
        while (settle.Elapsed < TimeSpan.FromMilliseconds(300))
        {
            panel.UpdateLayout();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Thread.Yield();
        }
        return (image, host, opened);
    }

    [Fact]
    public void Content_sized_fill_image_with_max_height_decodes_once()
    {
        var (image, host, opened) = CreateContentSizedFill(Stretch.Fill, 1024, 1024, "content-sized-fill");
        using (host)
        {
            opened.Count.ShouldBe(1);
            image.IsLoaded.ShouldBeTrue();
            // Fill expands to the MaxHeight bound once loaded, so the box is known before the load.
            image.Bounds.Height.ShouldBe(400);
            opened[0].PixelWidth.ShouldBe(400);
        }
    }

    [Fact]
    public void Content_sized_uniform_to_fill_wide_image_with_max_height_decodes_once_to_cover()
    {
        var (image, host, opened) = CreateContentSizedFill(Stretch.UniformToFill, 2048, 512, "content-sized-cover-wide");
        using (host)
        {
            opened.Count.ShouldBe(1);
            image.Bounds.Height.ShouldBe(400);
            // Covering 400 x 400 from a 4:1 source needs 1600 x 400; a width-only decode (400 x 100) would be 4x too small.
            opened[0].PixelWidth.ShouldBe(1600);
        }
    }

    [Fact]
    public void Avatar_uniform_to_fill_decodes_to_cover_box()
    {
        // A 4:1 source in a 48 px avatar: fitting inside decodes 48 px wide, covering needs 192 x 48.
        ImageOpenedEventArgs? opened = null;
        var avatar = new global::AtomUI.Controls.Avatar
        {
            Size = 48,
            Stretch = Stretch.UniformToFill,
            Source = new BytesImageSource(ImageControlTestHost.CreatePng(2048, 512), "avatar-cover-wide")
        };
        avatar.ImageOpened += (_, args) => opened = args;
        using var host = new ImageControlTestHost(avatar);
        ImageControlTestHost.WaitUntil(() => opened is not null, "avatar cover decode");
        opened.ShouldNotBeNull().PixelWidth.ShouldBe(192);

        avatar.Stretch = Stretch.Uniform;
        ImageControlTestHost.WaitUntil(() => opened?.PixelWidth == 48, "avatar contain decode after Stretch change");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Huge_height_does_not_throw(bool useMaxHeight)
    {
        // 1e10 DIP overflows a 32-bit pixel size even at render scaling 1.
        var image = new AsyncImage
        {
            Width = 32,
            Source = new BorrowedImageSource(new TestBorrowedImage(64, 64))
        };
        if (useMaxHeight)
        {
            image.MaxHeight = 1e10;
        }
        else
        {
            image.Height = 1e10;
        }

        ImageControlTestHost? host = null;
        try
        {
            Should.NotThrow(() => host = new ImageControlTestHost(image, width: 32, height: 120));
            ImageControlTestHost.WaitUntil(
                () => image.LoadState is ImageLoadState.Loaded or ImageLoadState.Failed,
                "huge height image terminal state");
            image.LoadError.ShouldBeNull();
            image.IsLoaded.ShouldBeTrue();
        }
        finally
        {
            host?.Dispose();
        }
    }

    private sealed class TestBorrowedImage(double width, double height) : IImage
    {
        public Size Size { get; } = new(width, height);

        public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
        {
        }
    }
}
