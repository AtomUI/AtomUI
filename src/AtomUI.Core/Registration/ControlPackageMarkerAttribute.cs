using System.ComponentModel;

namespace AtomUI.Registration;

/// <summary>
/// Marks an assembly as a control package whose registration is generated at build time.
/// The source generator and the build tools read the marker to locate the registration of the package.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class ControlPackageMarkerAttribute : Attribute
{
    /// <summary>
    /// Creates the marker of one control package.
    /// </summary>
    /// <param name="packageId">
    /// Identifier of the package; must not be null, empty or white space.
    /// </param>
    /// <param name="mapGroup">
    /// The group type. It must be public and closed, that is, not an open generic type.
    /// </param>
    /// <param name="abiVersion">
    /// The registration ABI the package was generated for. Any other value than
    /// <see cref="ControlRegistrationRuntime.AbiVersion" /> throws an <see cref="ArgumentOutOfRangeException" />.
    /// </param>
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

    /// <summary>
    /// The identifier of the control package carried by the marked assembly.
    /// </summary>
    public string PackageId { get; }
    /// <summary>
    /// The public closed type that identifies the generated type map of the package. Build tools resolve the
    /// type map accessor through it instead of searching by name.
    /// </summary>
    public Type MapGroup { get; }
    /// <summary>
    /// The registration ABI the package was generated for. It always equals
    /// <see cref="ControlRegistrationRuntime.AbiVersion" />, because the constructor rejects any other value.
    /// </summary>
    public int AbiVersion { get; }
}
