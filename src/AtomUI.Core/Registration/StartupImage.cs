using System.ComponentModel;
using AtomUI.Startup.Data;

namespace AtomUI.Registration;

/// <summary>Build-linked immutable startup contracts; carries no control CLR references.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class StartupImage
{
    private readonly Dictionary<string, string> _packages;
    private readonly Dictionary<string, string> _resourceOwners;

    public StartupImage(int version, ulong stamp, IReadOnlyList<KeyValuePair<string, string>> packages,
        IReadOnlyList<string> controlOrder, IEnumerable<KeyValuePair<string, string>>? resourceOwners = null,
        IEnumerable<string>? verifiedLanguages = null, IEnumerable<KeyValuePair<string, string>>? languageModules = null)
    {
        if (version != StartupManifestCodec.Version)
        {
            throw new NotSupportedException("Unsupported application startup image; rebuild the application.");
        }
        ArgumentNullException.ThrowIfNull(packages);
        ArgumentNullException.ThrowIfNull(controlOrder);
        Stamp = stamp;
        _packages = packages.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        ControlOrder = controlOrder.ToArray();
        _resourceOwners = resourceOwners?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal) ?? new(StringComparer.Ordinal);
        VerifiedLanguages = (verifiedLanguages ?? []).ToHashSet(StringComparer.Ordinal);
        LanguageModules = languageModules?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal) ?? new(StringComparer.Ordinal);
    }

    internal ulong Stamp { get; }
    internal IReadOnlyList<string> ControlOrder { get; }
    internal IReadOnlySet<string> VerifiedLanguages { get; }
    internal IReadOnlyDictionary<string, string> LanguageModules { get; }
    internal bool TryGetResourceOwner(string key, out string? packageId) => _resourceOwners.TryGetValue(key, out packageId);

    internal StartupPackageData GetPackage(ControlPackageDefinition definition)
    {
        if (!_packages.TryGetValue(definition.Id, out var data) ||
            !string.Equals(data, definition.EncodedManifest, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Control package '" + definition.Id +
                "' does not match the linked startup image; rebuild the application or use runtime package loading.");
        }
        // A generated package caches only immutable decoded metadata. No app resources, language
        // snapshots or callbacks are cached in the image or across application instances.
        return definition.Data;
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IGeneratedStartupImageHost
{
    StartupImage GetStartupImage();
}
