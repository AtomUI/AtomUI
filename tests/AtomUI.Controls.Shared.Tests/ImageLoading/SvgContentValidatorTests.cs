using System.Text;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.ImageLoading;

public class SvgContentValidatorTests
{
    [Fact]
    public void Same_Document_References_Inline_Css_And_Data_Raster_Are_Allowed()
    {
        var pngData = Convert.ToBase64String(ImageLoadingTestSupport.CreatePngHeader(2, 3));
        var probe = ValidateSvg($$"""
            <svg xmlns="http://www.w3.org/2000/svg"
                 xmlns:xlink="http://www.w3.org/1999/xlink"
                 width="64" height="64" viewBox="0 0 64 64">
              <defs>
                <linearGradient id="paint"><stop offset="0"/></linearGradient>
                <path id="face" d="M0 0 L64 0 L64 64 Z"/>
              </defs>
              <style>.face { fill: url(#paint); }</style>
              <use class="face" href="#face" xlink:href="#face"/>
              <image href="data:image/png;base64,{{pngData}}" width="2" height="3"/>
            </svg>
            """);

        probe.Format.ShouldBe(ImageContentFormat.Svg);
        probe.MediaType.ShouldBe("image/svg+xml");
        probe.SvgMetadata.ShouldNotBeNull();
        probe.SvgMetadata.ElementCount.ShouldBeGreaterThan(5);
        probe.SvgMetadata.EmbeddedImageCount.ShouldBe(1);
        probe.SvgMetadata.EmbeddedImageDecodedBytes.ShouldBe(24);
        probe.SvgMetadata.EstimatedDecodedCost.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("image/svg+xml")]
    [InlineData("application/xml")]
    [InlineData("text/xml")]
    [InlineData("application/octet-stream")]
    [InlineData(null)]
    public void Approved_Svg_Media_Types_Are_Accepted(string? mediaType)
    {
        ValidateSvg(
                "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 8 8\"/>",
                mediaType)
            .Format.ShouldBe(ImageContentFormat.Svg);
    }

    [Fact]
    public void Compatible_Mode_Accepts_Duplicate_Ids()
    {
        var probe = ValidateSvg("""
            <svg xmlns="http://www.w3.org/2000/svg">
              <path id="shape" d="M0 0"/>
              <path id="shape" d="M1 1"/>
            </svg>
            """);

        probe.Format.ShouldBe(ImageContentFormat.Svg);
    }

    [Fact]
    public void Strict_Mode_Rejects_Duplicate_Ids()
    {
        AssertSvgFailure(
            ImageLoadErrorCode.InvalidImageData,
            """
            <svg xmlns="http://www.w3.org/2000/svg">
              <path id="shape" d="M0 0"/>
              <path id="shape" d="M1 1"/>
            </svg>
            """,
            configure: builder => builder.Svg.ConformanceMode = SvgConformanceMode.Strict);
    }

    [Fact]
    public void Strict_Duplicate_Id_Does_Not_Mask_Unsafe_Content()
    {
        AssertSvgFailure(
            ImageLoadErrorCode.UnsafeVectorContent,
            """
            <svg xmlns="http://www.w3.org/2000/svg">
              <g id="same"/>
              <g id="same" onclick="run()"/>
            </svg>
            """,
            configure: builder => builder.Svg.ConformanceMode = SvgConformanceMode.Strict);
    }

    [Fact]
    public void Strict_Duplicate_Id_Does_Not_Mask_Reference_Complexity()
    {
        AssertSvgFailure(
            ImageLoadErrorCode.VectorComplexityLimitExceeded,
            """
            <svg xmlns="http://www.w3.org/2000/svg">
              <g id="same"><use href="#same"/></g>
              <g id="same"/>
            </svg>
            """,
            configure: builder => builder.Svg.ConformanceMode = SvgConformanceMode.Strict);
    }

    [Theory]
    [InlineData(SvgConformanceMode.Compatible)]
    [InlineData(SvgConformanceMode.Strict)]
    public void Duplicate_Id_Nested_References_Cannot_Bypass_Reference_Depth(SvgConformanceMode mode)
    {
        AssertSvgFailure(
            ImageLoadErrorCode.VectorComplexityLimitExceeded,
            """
            <svg xmlns="http://www.w3.org/2000/svg">
              <g id="same"><g id="inner"><use href="#leaf"/></g></g>
              <g id="same"><use href="#same"/></g>
              <path id="leaf" d="M0 0"/>
            </svg>
            """,
            configure: builder =>
            {
                builder.Svg.ConformanceMode = mode;
                builder.Svg.MaxReferenceDepth = 1;
            });
    }

    [Fact]
    public void Compatible_Mode_Keeps_Duplicate_Id_Occurrences_As_Separate_Reference_Nodes()
    {
        var probe = ValidateSvg("""
            <svg xmlns="http://www.w3.org/2000/svg">
              <g id="same"><use href="#leaf"/></g>
              <g id="same"><use href="#same"/></g>
              <path id="leaf" d="M0 0"/>
            </svg>
            """);

        probe.SvgMetadata.ShouldNotBeNull();
        probe.SvgMetadata.MaxReferenceDepth.ShouldBe(2);
    }

    [Fact]
    public void Compatible_Mode_Counts_References_From_Every_Duplicate_Id_Occurrence()
    {
        AssertSvgFailure(
            ImageLoadErrorCode.VectorComplexityLimitExceeded,
            """
            <svg xmlns="http://www.w3.org/2000/svg">
              <g id="same"/>
              <g id="same"><use href="#middle"/></g>
              <g id="middle"><use href="#leaf"/></g>
              <path id="leaf" d="M0 0"/>
            </svg>
            """,
            configure: builder => builder.Svg.MaxReferenceDepth = 1);
    }

    [Theory]
    [InlineData(SvgConformanceMode.Compatible)]
    [InlineData(SvgConformanceMode.Strict)]
    public void Conformance_Mode_Does_Not_Change_Security_Rejection(SvgConformanceMode mode)
    {
        AssertSvgFailure(
            ImageLoadErrorCode.UnsafeVectorContent,
            "<svg xmlns='http://www.w3.org/2000/svg'><script>throw 1</script></svg>",
            configure: builder => builder.Svg.ConformanceMode = mode);
    }

    [Theory]
    [InlineData("<!DOCTYPE svg [<!ENTITY xxe SYSTEM 'file:///etc/passwd'>]><svg xmlns='http://www.w3.org/2000/svg'>&xxe;</svg>")]
    [InlineData("<svg xmlns='http://www.w3.org/2000/svg'><script>throw 1</script></svg>")]
    [InlineData("<svg xmlns='http://www.w3.org/2000/svg' onload='run()'/>")]
    [InlineData("<svg xmlns='http://www.w3.org/2000/svg'><foreignObject/></svg>")]
    [InlineData("<svg xmlns='http://www.w3.org/2000/svg' xml:base='https://example.com/'/>")]
    [InlineData("<svg xmlns='http://www.w3.org/2000/svg'><image href='https://example.com/a.png'/></svg>")]
    [InlineData("<svg xmlns='http://www.w3.org/2000/svg'><use href='../icons.svg#user'/></svg>")]
    [InlineData("<svg xmlns='http://www.w3.org/2000/svg'><style>@import url('https://example.com/a.css');</style></svg>")]
    [InlineData("<svg xmlns='http://www.w3.org/2000/svg'><path style='fill:url(https://example.com/p.svg#p)'/></svg>")]
    [InlineData("<svg xmlns='http://www.w3.org/2000/svg'><image href='data:image/svg+xml,%3Csvg/%3E'/></svg>")]
    public void Unsafe_Vector_Content_Is_Rejected(string svg)
    {
        AssertSvgFailure(ImageLoadErrorCode.UnsafeVectorContent, svg);
    }

    [Theory]
    [InlineData("<html><svg xmlns='http://www.w3.org/2000/svg'/></html>")]
    [InlineData("<svg xmlns='urn:not-svg'/>")]
    [InlineData("<svg xmlns='http://www.w3.org/2000/svg'>")]
    public void Invalid_Xml_Or_Root_Is_Rejected(string svg)
    {
        AssertSvgFailure(ImageLoadErrorCode.InvalidImageData, svg);
    }

    [Fact]
    public void Raster_Declared_Media_Type_Cannot_Contain_Svg()
    {
        AssertSvgFailure(
            ImageLoadErrorCode.ContentTypeMismatch,
            "<svg xmlns='http://www.w3.org/2000/svg'/>",
            "image/png");
    }

    [Fact]
    public void Element_Attribute_Depth_Path_And_Reference_Limits_Are_Enforced()
    {
        AssertSvgFailure(
            ImageLoadErrorCode.VectorComplexityLimitExceeded,
            "<svg xmlns='http://www.w3.org/2000/svg'><g/><g/></svg>",
            configure: builder => builder.Svg.MaxElementCount = 2);
        AssertSvgFailure(
            ImageLoadErrorCode.VectorComplexityLimitExceeded,
            "<svg xmlns='http://www.w3.org/2000/svg' width='1' height='1'/>",
            configure: builder => builder.Svg.MaxAttributeCount = 2);
        AssertSvgFailure(
            ImageLoadErrorCode.VectorComplexityLimitExceeded,
            "<svg xmlns='http://www.w3.org/2000/svg'><g><g/></g></svg>",
            configure: builder => builder.Svg.MaxElementDepth = 2);
        AssertSvgFailure(
            ImageLoadErrorCode.VectorComplexityLimitExceeded,
            "<svg xmlns='http://www.w3.org/2000/svg'><path d='M0 0 L1 1'/></svg>",
            configure: builder => builder.Svg.MaxPathDataCharacters = 5);
        AssertSvgFailure(
            ImageLoadErrorCode.VectorComplexityLimitExceeded,
            """
            <svg xmlns="http://www.w3.org/2000/svg">
              <g id="a"><use href="#b"/></g>
              <g id="b"><use href="#c"/></g>
              <g id="c"/>
            </svg>
            """,
            configure: builder => builder.Svg.MaxReferenceDepth = 1);
    }

    [Fact]
    public void Embedded_Image_Count_And_Byte_Limits_Are_Enforced()
    {
        var pngData = Convert.ToBase64String(ImageLoadingTestSupport.CreatePngHeader(1, 1));
        var svg = $$"""
            <svg xmlns="http://www.w3.org/2000/svg">
              <image href="data:image/png;base64,{{pngData}}"/>
              <image href="data:image/png;base64,{{pngData}}"/>
            </svg>
            """;

        AssertSvgFailure(
            ImageLoadErrorCode.EmbeddedResourceLimitExceeded,
            svg,
            configure: builder => builder.Svg.MaxEmbeddedImageCount = 1);
        AssertSvgFailure(
            ImageLoadErrorCode.EmbeddedResourceLimitExceeded,
            svg,
            configure: builder => builder.Svg.MaxEmbeddedImageBytes = 1);
    }

    [Fact]
    public void Cancellation_Is_Observed_During_Validation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var validator = new ImageContentValidator(ImageLoadingTestSupport.CreateOptions());

        Should.Throw<OperationCanceledException>(() => validator.Validate(
            CreateContent("<svg xmlns='http://www.w3.org/2000/svg'/>", "image/svg+xml"),
            ImageSource.Parse("https://example.com/avatar.svg"),
            cancellation.Token));
    }

    private static ImageProbeResult ValidateSvg(
        string svg,
        string? mediaType = "image/svg+xml",
        Action<ImageLoadingOptionsBuilder>? configure = null)
    {
        var validator = new ImageContentValidator(ImageLoadingTestSupport.CreateOptions(configure));
        return validator.Validate(
            CreateContent(svg, mediaType),
            ImageSource.Parse("https://example.com/avatar.svg"));
    }

    private static void AssertSvgFailure(
        ImageLoadErrorCode expected,
        string svg,
        string? mediaType = "image/svg+xml",
        Action<ImageLoadingOptionsBuilder>? configure = null)
    {
        var exception = Should.Throw<ImageLoadFailureException>(() =>
            ValidateSvg(svg, mediaType, configure));
        exception.Error.Code.ShouldBe(expected);
    }

    private static ImageEncodedContent CreateContent(string svg, string? mediaType)
    {
        return ImageLoadingTestSupport.CreateContent(Encoding.UTF8.GetBytes(svg), mediaType);
    }
}
