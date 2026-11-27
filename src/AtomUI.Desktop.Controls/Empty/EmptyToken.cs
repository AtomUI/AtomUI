using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the empty-state placeholder: the height of its illustration per size and the spacing of
/// the description and the footer.
/// </summary>
[ControlDesignToken]
public sealed partial class EmptyToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Default height of the empty-state illustration.
    /// </summary>
    public partial double EmptyImgHeight { get; set; }

    /// <summary>
    /// Height of the empty-state illustration in the small variant.
    /// </summary>
    public partial double EmptyImgHeightSM { get; set; }
    /// <summary>
    /// Height of the empty-state illustration in the medium variant.
    /// </summary>
    public partial double EmptyImgHeightMD { get; set; }
    
    /// <summary>
    /// Space around the empty-state description.
    /// </summary>
    public partial Thickness DescriptionMargin { get; set; }
    /// <summary>
    /// Space around the description in the small empty state.
    /// </summary>
    public partial Thickness DescriptionMarginSM { get; set; }
    /// <summary>
    /// Space around the optional footer below the description.
    /// </summary>
    public partial Thickness FooterMargin { get; set; }
    
}
