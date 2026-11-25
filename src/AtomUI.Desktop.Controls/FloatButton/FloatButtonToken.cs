using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class FloatButtonToken : AbstractControlDesignToken
{
    
    /// <summary>Side length of a floating button before any description changes its content layout.</summary>
    public partial double FloatButtonSize { get; set; }
    
    /// <summary>Size of the glyph in a floating button.</summary>
    public partial double FloatButtonIconSize { get; set; }
    
    /// <summary>Icon and description foreground over a primary floating button's fill.</summary>
    public partial Color PrimaryColor { get; set; }
    
    /// <summary>Line height of the floating button label without an outline.</summary>
    public partial double DescriptionLineHeight { get; set; }
    
    /// <summary>Horizontal inset of the floating button in its host.</summary>
    public partial double FloatOffsetX { get; set; }
    
    /// <summary>Vertical inset of the floating button in its host.</summary>
    public partial double FloatOffsetY { get; set; }

    /// <summary>Elevation shadow underneath a floating button.</summary>
    public partial BoxShadows FloatButtonShadows { get; set; }

    /// <summary>Resting fill of the default floating button, paired with <see cref="DefaultIconColor"/>.</summary>
    public partial Color DefaultBg { get; set; }

    /// <summary>Fill of a default floating button under the pointer.</summary>
    public partial Color DefaultHoverBg { get; set; }

    /// <summary>Fill of a default floating button while pressed.</summary>
    public partial Color DefaultActiveBg { get; set; }

    /// <summary>Icon foreground on the default floating button's fill.</summary>
    public partial Color DefaultIconColor { get; set; }

    /// <summary>
    /// Whether float buttons draw an outline frame: outlined default and square buttons, disabled buttons at reduced
    /// opacity, and the description styling Tokens below.
    /// </summary>
    public partial bool HasOutline { get; set; }

    /// <summary>Whether an outlined primary button uses <see cref="FloatButtonShadows"/> instead of its preset shadow.</summary>
    public partial bool OutlineOnlyShadow { get; set; }

    /// <summary>Whether an outlined button with a description rounds into a capsule.</summary>
    public partial bool CapsuleDescription { get; set; }

    /// <summary>Whether an outlined button shows its plain description in upper case.</summary>
    public partial bool UppercaseDescription { get; set; }

    /// <summary>The description font weight of an outlined button.</summary>
    public partial FontWeight DescriptionWeight { get; set; }

    /// <summary>
    /// The description line height of an outlined button, used unless the application sets
    /// <see cref="DescriptionLineHeight"/>.
    /// </summary>
    public partial double OutlineDescriptionLineHeight { get; set; }
    
}
