namespace AtomUI.Theme.Schema;

public sealed record ControlThemeSemanticPartDescriptor
{
    public ControlThemeSemanticPartDescriptor(string propertyName, Type targetType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        ArgumentNullException.ThrowIfNull(targetType);
        if (!typeof(Avalonia.Controls.Control).IsAssignableFrom(targetType))
        {
            throw new ArgumentException("Theme target must derive from Control.", nameof(targetType));
        }
        PropertyName = propertyName;
        TargetType = targetType;
    }

    public Type TargetType { get; }
    public string PropertyName { get; }
}
