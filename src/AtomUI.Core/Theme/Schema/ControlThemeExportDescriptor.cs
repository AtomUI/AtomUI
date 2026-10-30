using Avalonia;

namespace AtomUI.Theme.Schema;

public sealed record ControlThemeExportDescriptor
{
    public ControlThemeExportDescriptor(Type targetType, object resourceKey)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentNullException.ThrowIfNull(resourceKey);
        if (!typeof(StyledElement).IsAssignableFrom(targetType))
        {
            throw new ArgumentException("Theme target must derive from StyledElement.", nameof(targetType));
        }
        if (resourceKey is not (Type or string))
        {
            throw new ArgumentException("Theme export keys must be Types or strings.", nameof(resourceKey));
        }
        TargetType = targetType;
        ResourceKey = resourceKey;
    }

    public Type TargetType { get; }
    public object ResourceKey { get; }
}
