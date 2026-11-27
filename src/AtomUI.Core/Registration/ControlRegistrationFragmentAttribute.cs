using System.ComponentModel;

namespace AtomUI.Registration;

/// <summary>
/// Base class of a generated proxy that contributes one control to the registration of its package.
/// The proxy class is sealed and non-generic, and it carries an instance of itself as its class attribute;
/// <see cref="ControlRegistrationRuntime.ResolveFragment" /> and
/// <see cref="ControlRegistrationRuntime.CollectFragments" /> reject any other shape.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public abstract class ControlRegistrationFragmentAttribute : Attribute
{
    /// <summary>
    /// Identifies the fragment inside a package. Generated fragments use the package id, a colon and the
    /// assembly-qualified name of the control.
    /// </summary>
    public abstract string FragmentId { get; }
    /// <summary>
    /// Writes the registration of the control, such as its descriptors, theme assets and resource providers,
    /// into <paramref name="builder" />.
    /// </summary>
    /// <remarks>
    /// <see cref="ControlPackageRegistrationBuilder.AddFragment" /> calls it once per fragment. An exception
    /// thrown here fails the whole package collection, which then refuses every further change.
    /// </remarks>
    public abstract void Add(ControlPackageRegistrationBuilder builder);
}
