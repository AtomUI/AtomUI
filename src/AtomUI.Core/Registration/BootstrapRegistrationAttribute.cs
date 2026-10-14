namespace AtomUI.Registration;

/// <summary>
/// Marks a control or theme resource class whose registration is required before the startup surface is shown.
/// Packages without this marker register all their fragments during initial configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class BootstrapRegistrationAttribute : Attribute
{
}
