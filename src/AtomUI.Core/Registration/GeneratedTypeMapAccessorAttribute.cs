using System.ComponentModel;

namespace AtomUI.Registration;

/// <summary>
/// Marks the generated method that gives access to the type map of a control package. Build tools resolve
/// the method through <see cref="Group" /> instead of searching it by name.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Method)]
public sealed class GeneratedTypeMapAccessorAttribute : Attribute
{
    /// <summary>
    /// Creates the marker of the type map accessor that belongs to <paramref name="group" />.
    /// </summary>
    /// <param name="group">
    /// The group that owns the accessor; must not be null.
    /// </param>
    public GeneratedTypeMapAccessorAttribute(Type group)
    {
        ArgumentNullException.ThrowIfNull(group);
        Group = group;
    }

    /// <summary>
    /// The group that owns the type map accessor, the group that
    /// <see cref="ControlPackageMarkerAttribute.MapGroup" /> names for the package. It must be a public closed
    /// type owned by the package.
    /// </summary>
    public Type Group { get; }
}
