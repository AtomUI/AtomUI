using System.ComponentModel;

namespace AtomUI.Registration;

[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public abstract class ControlRegistrationFragmentAttribute : Attribute
{
    public virtual bool IsBootstrap => true;
    public abstract string FragmentId { get; }
    public abstract void Add(ControlPackageRegistrationBuilder builder);
}
