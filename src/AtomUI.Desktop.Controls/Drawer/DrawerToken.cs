using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class DrawerToken : AbstractControlDesignToken
{

    /// <summary>Elevation shadow when the drawer opens from the left edge.</summary>
    public partial BoxShadows BoxShadowDrawerLeft { get; set; }

    /// <summary>Elevation shadow when the drawer opens from the right edge.</summary>
    public partial BoxShadows BoxShadowDrawerRight { get; set; }

    /// <summary>Elevation shadow when the drawer opens from the top edge.</summary>
    public partial BoxShadows BoxShadowDrawerUp { get; set; }

    /// <summary>Elevation shadow when the drawer opens from the bottom edge.</summary>
    public partial BoxShadows BoxShadowDrawerDown { get; set; }
    
    /// <summary>Space around the drawer header content.</summary>
    public partial Thickness HeaderMargin { get; set; }
    
    /// <summary>Inset of the footer content and actions.</summary>
    public partial Thickness FooterPadding { get; set; }
    
    /// <summary>Extent of a medium drawer measured perpendicular to its attachment edge.</summary>
    public partial Dimension MiddleSize { get; set; }
    
    /// <summary>Extent of a large drawer measured perpendicular to its attachment edge.</summary>
    public partial Dimension LargeSize { get; set; }
    
    /// <summary>Extent of a small drawer measured perpendicular to its attachment edge.</summary>
    public partial Dimension SmallSize { get; set; }
    
    /// <summary>Space inside the close button around its glyph.</summary>
    public partial Thickness CloseIconPadding { get; set; }
    
    /// <summary>Space around the close button in the header.</summary>
    public partial Thickness CloseIconMargin { get; set; }
    
    /// <summary>Inset of the drawer body content.</summary>
    public partial Thickness ContentPadding { get; set; }
    
    /// <summary>
    /// 子 Drawer push 的 Offset 比率
    /// </summary>
    public partial double PushOffsetPercent { get; set; }

    /// <summary>
    /// The border colour drawn on the drawer's anchored side (the sheet border of shadcn). Defaults to
    /// transparent, so the Ant Design rendering draws nothing.
    /// </summary>
    public partial Color EdgeBorderColor { get; set; }

    /// <summary>
    /// The thickness of the anchored-side border. Defaults to 0.
    /// </summary>
    public partial double EdgeBorderThickness { get; set; }
    
}
