using Avalonia.Controls;

namespace AtomUI.Theme.Schema;

/// <summary>
/// One semantic theme binding of a control theme asset: it names the Token owner, the property that holds
/// the ControlTheme of one of its semantic parts, and the type that ControlTheme targets.
/// </summary>
/// <remarks>
/// <para>
/// The binding is valid only when the owner has a semantic part whose
/// <see cref="ControlThemeSemanticPartDescriptor" /> has the same property name and target type.
/// </para>
/// </remarks>
public sealed record ControlThemeBindingDescriptor
{
    /// <summary>
    /// Creates a semantic theme binding.
    /// </summary>
    /// <param name="ownerIdentity">
    /// The Token owner that has the semantic part. It must carry its Control type.
    /// </param>
    /// <param name="propertyName">
    /// The name of the owner's property that holds the ControlTheme of the semantic part.
    /// </param>
    /// <param name="targetType">
    /// The type the ControlTheme targets. It must derive from <see cref="Control" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName" /> or <paramref name="targetType" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="propertyName" /> is white space, the owner identity has no Control type, or the
    /// target type does not derive from <see cref="Control" />.
    /// </exception>
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

    /// <summary>
    /// The <see cref="ControlTokenIdentity" /> of the Token owner that has the semantic part.
    /// </summary>
    public ControlTokenIdentity OwnerIdentity { get; }
    /// <summary>
    /// The name of the owner's property that holds the ControlTheme of the semantic part.
    /// </summary>
    public string PropertyName { get; }
    /// <summary>
    /// The type the ControlTheme targets.
    /// </summary>
    public Type TargetType { get; }
}
