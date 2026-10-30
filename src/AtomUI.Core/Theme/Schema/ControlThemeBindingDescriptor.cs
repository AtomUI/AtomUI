using Avalonia.Controls;

namespace AtomUI.Theme.Schema;

public sealed record ControlThemeBindingDescriptor
{
    public ControlThemeBindingDescriptor(ControlTokenIdentity ownerIdentity, string propertyName, Type targetType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        ArgumentNullException.ThrowIfNull(targetType);
        if (ownerIdentity.OwnerType is null)
        {
            throw new ArgumentException("A semantic theme binding requires a typed owner identity.", nameof(ownerIdentity));
        }
        if (!typeof(Control).IsAssignableFrom(targetType))
        {
            throw new ArgumentException("Theme target must derive from Control.", nameof(targetType));
        }
        OwnerIdentity = ownerIdentity;
        PropertyName = propertyName;
        TargetType = targetType;
    }

    public ControlTokenIdentity OwnerIdentity { get; }
    public string PropertyName { get; }
    public Type TargetType { get; }
}
