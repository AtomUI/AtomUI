using System.ComponentModel;

namespace AtomUI.Registration;

[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class ControlPackageMarkerAttribute : Attribute
{
    public ControlPackageMarkerAttribute(string packageId, Type mapGroup, int abiVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentNullException.ThrowIfNull(mapGroup);
        if (abiVersion != ControlRegistrationRuntime.AbiVersion)
        {
            throw new ArgumentOutOfRangeException(nameof(abiVersion), "Unsupported control registration ABI.");
        }
        if (!mapGroup.IsVisible || mapGroup.ContainsGenericParameters)
        {
            throw new ArgumentException("A package Group must be public and closed.", nameof(mapGroup));
        }
        PackageId = packageId;
        MapGroup = mapGroup;
        AbiVersion = abiVersion;
    }

    public string PackageId { get; }
    public Type MapGroup { get; }
    public int AbiVersion { get; }
}
