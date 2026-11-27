namespace AtomUI.Controls;

/// <summary>
/// The resource limits and the conformance mode for SVG documents, reached through
/// <see cref="ImageLoadingOptionsBuilder.Svg" />.
/// </summary>
/// <remarks>
/// Every limit must be greater than zero. A document that exceeds <see cref="MaxDocumentBytes" /> fails with
/// <see cref="ImageLoadErrorCode.ResponseTooLarge" />, one that exceeds an embedded image limit fails with
/// <see cref="ImageLoadErrorCode.EmbeddedResourceLimitExceeded" />, and one that exceeds any other limit
/// fails with <see cref="ImageLoadErrorCode.VectorComplexityLimitExceeded" />.
/// </remarks>
public sealed class SvgImageLoadingOptionsBuilder
{
    /// <summary>
    /// Whether an SVG document with duplicate <c>id</c> attributes is accepted.
    /// </summary>
    public SvgConformanceMode ConformanceMode { get; set; } = SvgConformanceMode.Compatible;

    /// <summary>
    /// The largest size in bytes of an SVG document.
    /// </summary>
    public long MaxDocumentBytes { get; set; } = 4L * 1024 * 1024;

    /// <summary>
    /// The largest number of characters an SVG document may contain.
    /// </summary>
    public long MaxXmlCharacters { get; set; } = 8_000_000;

    /// <summary>
    /// The largest number of elements an SVG document may contain.
    /// </summary>
    public int MaxElementCount { get; set; } = 20_000;

    /// <summary>
    /// The largest total number of attributes an SVG document may contain.
    /// </summary>
    public int MaxAttributeCount { get; set; } = 100_000;

    /// <summary>
    /// The largest nesting depth of elements an SVG document may contain.
    /// </summary>
    public int MaxElementDepth { get; set; } = 256;

    /// <summary>
    /// The largest number of hops a chain of local <c>#id</c> references may contain.
    /// </summary>
    public int MaxReferenceDepth { get; set; } = 64;

    /// <summary>
    /// The largest total number of characters of all <c>d</c> attributes, which hold path data.
    /// </summary>
    public long MaxPathDataCharacters { get; set; } = 2_000_000;

    /// <summary>
    /// The largest number of images an SVG document may embed as <c>data:</c> URIs.
    /// </summary>
    public int MaxEmbeddedImageCount { get; set; } = 16;

    /// <summary>
    /// The largest total size in bytes of the images an SVG document embeds as <c>data:</c> URIs.
    /// </summary>
    public long MaxEmbeddedImageBytes { get; set; } = 8L * 1024 * 1024;

    internal SvgImageLoadingOptions Build()
    {
        if (!Enum.IsDefined(ConformanceMode))
        {
            throw new InvalidOperationException($"{nameof(ConformanceMode)} must be a defined value.");
        }
        ValidatePositive(MaxDocumentBytes, nameof(MaxDocumentBytes));
        ValidatePositive(MaxXmlCharacters, nameof(MaxXmlCharacters));
        ValidatePositive(MaxElementCount, nameof(MaxElementCount));
        ValidatePositive(MaxAttributeCount, nameof(MaxAttributeCount));
        ValidatePositive(MaxElementDepth, nameof(MaxElementDepth));
        ValidatePositive(MaxReferenceDepth, nameof(MaxReferenceDepth));
        ValidatePositive(MaxPathDataCharacters, nameof(MaxPathDataCharacters));
        ValidatePositive(MaxEmbeddedImageCount, nameof(MaxEmbeddedImageCount));
        ValidatePositive(MaxEmbeddedImageBytes, nameof(MaxEmbeddedImageBytes));

        return new SvgImageLoadingOptions(
            ConformanceMode,
            MaxDocumentBytes,
            MaxXmlCharacters,
            MaxElementCount,
            MaxAttributeCount,
            MaxElementDepth,
            MaxReferenceDepth,
            MaxPathDataCharacters,
            MaxEmbeddedImageCount,
            MaxEmbeddedImageBytes);
    }

    private static void ValidatePositive(long value, string name)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException($"{name} must be greater than zero.");
        }
    }
}
