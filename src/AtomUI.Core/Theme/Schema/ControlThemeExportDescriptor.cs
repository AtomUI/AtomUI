using Avalonia;

namespace AtomUI.Theme.Schema;

/// <summary>
/// One ControlTheme that a control theme asset exports: the key it is stored under and the type it targets.
/// </summary>
public sealed record ControlThemeExportDescriptor
{
    /// <summary>
    /// Creates an export descriptor.
    /// </summary>
    /// <param name="targetType">
    /// The type the ControlTheme targets. It must derive from <see cref="StyledElement" />.
    /// </param>
    /// <param name="resourceKey">
    /// The key the ControlTheme is stored under in the asset: a <see cref="Type" /> or a <see cref="string" />.
    /// It must not be used by another export of the same registration.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// An argument is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The target type does not derive from <see cref="StyledElement" />, or the resource key is neither a
    /// <see cref="Type" /> nor a <see cref="string" />.
    /// </exception>
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

    /// <summary>
    /// The type the ControlTheme targets.
    /// </summary>
    public Type TargetType { get; }
    /// <summary>
    /// The key the ControlTheme is stored under in the asset: a <see cref="Type" /> or a <see cref="string" />.
    /// </summary>
    public object ResourceKey { get; }
}
