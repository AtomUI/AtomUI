namespace AtomUI.Theme.Schema;

/// <summary>
/// Theme side of a semantic part: the name of the property that holds the ControlTheme of the part, and
/// the type that ControlTheme targets.
/// </summary>
public sealed record ControlThemeSemanticPartDescriptor
{
    /// <summary>
    /// Creates the theme descriptor of a semantic part.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the owner's property that holds the ControlTheme of the part.
    /// </param>
    /// <param name="targetType">
    /// The type the ControlTheme targets. It must derive from <see cref="Avalonia.Controls.Control" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName" /> or <paramref name="targetType" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="propertyName" /> is white space, or the target type does not derive from
    /// <see cref="Avalonia.Controls.Control" />.
    /// </exception>
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

    /// <summary>
    /// The type the ControlTheme of the part targets.
    /// </summary>
    public Type TargetType { get; }
    /// <summary>
    /// The name of the owner's property that holds the ControlTheme of the part.
    /// </summary>
    public string PropertyName { get; }
}
