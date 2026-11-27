using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// A button that shows one icon of the icon gallery together with its name.
/// </summary>
public class IconInfoItem : Button, IMotionAwareControl
{
    /// <summary>
    /// Identifies the <see cref="IconName" /> property.
    /// </summary>
    public static readonly StyledProperty<string> IconNameProperty =
        AvaloniaProperty.Register<IconInfoItem, string>(nameof(IconName));

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<Icon?> IconProperty =
        AvaloniaProperty.Register<IconInfoItem, Icon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<IconInfoItem>();

    /// <summary>
    /// Name of the icon, shown as the caption of the item.
    /// </summary>
    public string IconName
    {
        get => GetValue(IconNameProperty);
        set => SetValue(IconNameProperty, value);
    }

    /// <summary>
    /// The icon that the item shows. The default is <see langword="null" />, which shows no icon.
    /// </summary>
    public Icon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Enables the transitions of the item when <see langword="true" />. The background color then changes through
    /// a transition. The theme sets the value from the <c>EnableMotion</c> token, so a value set on the item itself
    /// overrides it.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
}
