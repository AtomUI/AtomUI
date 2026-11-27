namespace AtomUI.Controls;

/// <summary>
/// Pseudo-class names for the size presets of an avatar.
/// Nothing in this library sets them; the avatar theme selects by the <c>SizeType</c> property instead.
/// </summary>
public static class AvatarPseudoClass
{
    /// <summary>
    /// Selector pseudo-class of an avatar with the large size preset.
    /// </summary>
    public const string Large = ":large";
    /// <summary>
    /// Selector pseudo-class of an avatar with the middle size preset.
    /// </summary>
    public const string Middle = ":middle";
    /// <summary>
    /// Selector pseudo-class of an avatar with the small size preset.
    /// </summary>
    public const string Small = ":small";
    /// <summary>
    /// Selector pseudo-class of an avatar whose size comes from its own width and height.
    /// </summary>
    public const string Custom = ":custom-size";
}
