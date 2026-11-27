namespace AtomUI.Controls;

/// <summary>
/// The encoded bytes of an image together with the metadata that the image caches need to store, share and
/// revalidate them; it is what <c>ImageCodec.DecodeAsync</c> decodes, for example in <c>SvgImageCodec</c> of
/// AtomUI.Controls.
/// </summary>
/// <remarks>
/// <c>IsFresh</c> is <see langword="true" /> while <c>FreshUntil</c> lies in the future and the content is not
/// marked <c>NoCache</c>. <c>ForContentStore</c> removes the response and lifetime metadata, which belong to a
/// single request, so that the content can be stored by its content id.
/// </remarks>
internal sealed record ImageEncodedContent(
    byte[] Bytes,
    string? MediaType,
    ImageLoadOrigin Origin,
    DateTimeOffset StoredAt,
    DateTimeOffset? FreshUntil = null,
    string? ETag = null,
    DateTimeOffset? LastModified = null,
    bool NoStore = false,
    bool NoCache = false,
    bool MustRevalidate = false,
    bool IsRemote = false,
    bool IsTrustedAsset = false,
    ImageSourceVersion? SourceVersion = null,
    string[]? VaryHeaders = null,
    string? VaryDigest = null,
    int SecurityPolicyVersion = 0,
    DateTimeOffset? ResponseDate = null,
    TimeSpan? ResponseAge = null,
    DateTimeOffset? Expires = null,
    TimeSpan? MaxAge = null,
    bool IsPrivate = false,
    ImageContentId? ContentId = null,
    ImageSourceValidation SourceValidation = ImageSourceValidation.Current,
    ImageProbeResult? Probe = null)
{
    internal long Size => Bytes.LongLength;

    internal bool IsFresh(DateTimeOffset now) =>
        !NoCache && FreshUntil is not null && FreshUntil.Value > now;

    internal ImageEncodedContent WithOrigin(ImageLoadOrigin source) => this with { Origin = source };

    internal ImageEncodedContent MarkValidated(ImageProbeResult? probe = null) => this with
    {
        SecurityPolicyVersion = ImageSecurityPolicy.Version,
        ContentId = ContentId ?? ImageContentId.Create(Bytes),
        Probe = probe ?? Probe
    };

    internal ImageEncodedContent ForContentStore() => this with
    {
        FreshUntil = null,
        ETag = null,
        LastModified = null,
        NoStore = false,
        NoCache = false,
        MustRevalidate = false,
        IsRemote = false,
        IsTrustedAsset = false,
        SourceVersion = null,
        VaryHeaders = null,
        VaryDigest = null,
        ResponseDate = null,
        ResponseAge = null,
        Expires = null,
        MaxAge = null,
        IsPrivate = false,
        SourceValidation = ImageSourceValidation.Current
    };
}
