using System.ComponentModel;

namespace AtomUI.Registration;

[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Method)]
public sealed class GeneratedTypeMapAccessorAttribute : Attribute
{
    public GeneratedTypeMapAccessorAttribute(Type group)
    {
        ArgumentNullException.ThrowIfNull(group);
        Group = group;
    }

    public Type Group { get; }
}
