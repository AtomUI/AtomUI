using System.ComponentModel;

namespace AtomUI.Registration;

/// <summary>Pure-data package contract consumed by the application startup-image linker.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class ControlPackageManifestAttribute(int version, string packageId, string data) : Attribute
{
    public int Version { get; } = version;
    public string PackageId { get; } = packageId;
    public string Data { get; } = data;
}
