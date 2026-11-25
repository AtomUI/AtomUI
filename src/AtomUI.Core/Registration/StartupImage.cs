using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using AtomUI.Startup.Data;
using AtomUI.Theme.Languages;

namespace AtomUI.Registration;

/// <summary>Build-linked immutable startup metadata and static value package entries.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class StartupImage
{
    private readonly Dictionary<string, StartupPackageProof> _packages;
    private readonly Dictionary<string, string> _resourceOwners;
    // "s:" owners keyed by the bare resource name, so string key misses check ownership without concatenation.
    private readonly Dictionary<string, string> _stringResourceOwners = new(StringComparer.Ordinal);

    public StartupImage(uint formatId, ulong stamp, IReadOnlyList<StartupPackageProof> packages,
        IReadOnlyList<string> controlOrder, IEnumerable<KeyValuePair<string, string>>? resourceOwners = null,
        IEnumerable<string>? verifiedLanguages = null, IEnumerable<KeyValuePair<string, string>>? languageModules = null,
        IReadOnlyList<ControlValuePackage>? controlValuePackages = null)
    {
        if (formatId != StartupImageFormat.Id)
        {
            throw new NotSupportedException("Unsupported application startup image; rebuild the application.");
        }
        ArgumentNullException.ThrowIfNull(packages);
        ArgumentNullException.ThrowIfNull(controlOrder);
        Stamp = stamp;
        _packages = new Dictionary<string, StartupPackageProof>(packages.Count, StringComparer.Ordinal);
        foreach (var proof in packages)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(proof.PackageId);
            if (proof.ManifestFormatId != StartupManifestCodec.Version ||
                proof.Encoding != StartupPackageProof.DirectText ||
                proof.OriginalAsciiLength == 0 || proof.OriginalAsciiLength > StartupManifestCodec.MaximumBase64Length ||
                proof.Payload is null || proof.OriginalAsciiLength != proof.Payload.Length)
            {
                throw new InvalidDataException($"Invalid startup package proof for '{proof.PackageId}'; rebuild the application.");
            }
            _packages.Add(proof.PackageId, proof);
        }
        ControlOrder = controlOrder.ToArray();
        _resourceOwners = resourceOwners?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal) ?? new(StringComparer.Ordinal);
        foreach (var (key, packageId) in _resourceOwners)
        {
            if (key.StartsWith("s:", StringComparison.Ordinal))
            {
                _stringResourceOwners.Add(key.Substring(2), packageId);
            }
        }
        VerifiedLanguages = (verifiedLanguages ?? []).ToHashSet(StringComparer.Ordinal);
        LanguageModules = languageModules?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal) ?? new(StringComparer.Ordinal);
        ControlValuePackages = controlValuePackages?.ToArray() ?? [];
    }

    internal ulong Stamp { get; }
    internal IReadOnlyList<string> ControlOrder { get; }
    internal IReadOnlySet<string> VerifiedLanguages { get; }
    internal IReadOnlyDictionary<string, string> LanguageModules { get; }
    internal IReadOnlyList<ControlValuePackage> ControlValuePackages { get; }
    internal bool TryGetResourceOwner(string key, out string? packageId) => _resourceOwners.TryGetValue(key, out packageId);
    internal bool TryGetStringResourceOwner(string name, [NotNullWhen(true)] out string? packageId) =>
        _stringResourceOwners.TryGetValue(name, out packageId);

    internal StartupPackageData GetPackage(ControlPackageDefinition definition)
    {
        if (!_packages.TryGetValue(definition.Id, out var proof) ||
            !string.Equals(proof.Payload, definition.EncodedManifest, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Control package '" + definition.Id +
                "' does not match the linked startup image; rebuild the application or use runtime package loading.");
        }
        // A generated package caches only immutable decoded metadata. No app resources, language
        // snapshots or callbacks are cached in the image or across application instances.
        return definition.Data;
    }
}

/// <summary>Exact build-time package evidence for the generated startup image.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly record struct StartupPackageProof(
    string PackageId, uint ManifestFormatId, uint OriginalAsciiLength, byte Encoding, string Payload)
{
    internal const byte DirectText = 0;
}

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IGeneratedStartupImageHost
{
    StartupImage GetStartupImage();
}
