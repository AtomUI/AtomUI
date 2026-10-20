using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.ImageLoading;

public class SvgImageLoadingOptionsTests
{
    [Fact]
    public void Svg_Options_Use_The_Approved_Defaults()
    {
        var options = ImageLoadingTestSupport.CreateOptions();

        options.Svg.ConformanceMode.ShouldBe(SvgConformanceMode.Compatible);
        options.Svg.MaxDocumentBytes.ShouldBe(4L * 1024 * 1024);
        options.Svg.MaxXmlCharacters.ShouldBe(8_000_000);
        options.Svg.MaxElementCount.ShouldBe(20_000);
        options.Svg.MaxAttributeCount.ShouldBe(100_000);
        options.Svg.MaxElementDepth.ShouldBe(256);
        options.Svg.MaxReferenceDepth.ShouldBe(64);
        options.Svg.MaxPathDataCharacters.ShouldBe(2_000_000);
        options.Svg.MaxEmbeddedImageCount.ShouldBe(16);
        options.Svg.MaxEmbeddedImageBytes.ShouldBe(8L * 1024 * 1024);
    }

    [Fact]
    public void Svg_Conformance_Mode_Is_Frozen_When_Image_Options_Are_Built()
    {
        var builder = new ImageLoadingOptionsBuilder();
        builder.Svg.ConformanceMode = SvgConformanceMode.Strict;

        var options = builder.Build("svg-conformance-options-test");
        builder.Svg.ConformanceMode = SvgConformanceMode.Compatible;

        options.Svg.ConformanceMode.ShouldBe(SvgConformanceMode.Strict);
    }

    [Fact]
    public void Svg_Conformance_Mode_Must_Be_Defined()
    {
        Should.Throw<InvalidOperationException>(() =>
            ImageLoadingTestSupport.CreateOptions(builder =>
                builder.Svg.ConformanceMode = (SvgConformanceMode)int.MaxValue));
    }

    [Fact]
    public void Svg_Options_Are_Frozen_When_Image_Options_Are_Built()
    {
        var builder = new ImageLoadingOptionsBuilder();
        builder.Svg.MaxElementCount = 123;

        var options = builder.Build("svg-options-test");
        builder.Svg.MaxElementCount = 456;

        options.Svg.MaxElementCount.ShouldBe(123);
    }

    [Fact]
    public void Every_Svg_Resource_Limit_Must_Be_Positive()
    {
        Action<ImageLoadingOptionsBuilder>[] invalidConfigurations =
        [
            builder => builder.Svg.MaxDocumentBytes = 0,
            builder => builder.Svg.MaxXmlCharacters = 0,
            builder => builder.Svg.MaxElementCount = 0,
            builder => builder.Svg.MaxAttributeCount = 0,
            builder => builder.Svg.MaxElementDepth = 0,
            builder => builder.Svg.MaxReferenceDepth = 0,
            builder => builder.Svg.MaxPathDataCharacters = 0,
            builder => builder.Svg.MaxEmbeddedImageCount = 0,
            builder => builder.Svg.MaxEmbeddedImageBytes = 0
        ];

        foreach (var configure in invalidConfigurations)
        {
            Should.Throw<InvalidOperationException>(() =>
                ImageLoadingTestSupport.CreateOptions(configure));
        }
    }

    [Fact]
    public void Svg_Security_Error_Codes_Are_Part_Of_The_Public_Error_Contract()
    {
        Enum.IsDefined(ImageLoadErrorCode.VectorComplexityLimitExceeded).ShouldBeTrue();
        Enum.IsDefined(ImageLoadErrorCode.EmbeddedResourceLimitExceeded).ShouldBeTrue();
    }
}
