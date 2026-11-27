using System.ComponentModel;

namespace AtomUI.Registration;

/// <summary>
/// Pure-data package contract consumed by the application startup-image linker.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class ControlPackageManifestAttribute(int version, string packageId, string data) : Attribute
{
    /// <summary>
    /// Format version of the manifest encoded in <see cref="Data" />.
    /// </summary>
    public int Version { get; } = version;
    /// <summary>
    /// Identity of the control package that the manifest describes.
    /// </summary>
    public string PackageId { get; } = packageId;
    /// <summary>
    /// The encoded startup manifest of the package, in the format that <see cref="Version" /> names.
    /// </summary>
    public string Data { get; } = data;
}
