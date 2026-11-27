using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="Select" /> dropdown that the other select-like inputs do not share: the
/// look of an option in the dropdown list, the padding of the popup frame, the border style of the selector
/// frame and the tint of its indicator.
/// </summary>
[ControlDesignToken]
public sealed partial class SelectToken : AbstractSelectInputToken
{
    /// <summary>
    /// Text foreground of a selected dropdown option. Pair with <see cref="OptionSelectedBg" />.
    /// </summary>
    public partial Color OptionSelectedColor { get; set; }

    /// <summary>
    /// Foreground of the check mark on a selected dropdown option.
    /// </summary>
    public partial Color OptionSelectedIndicatorColor { get; set; }

    /// <summary>
    /// Weight of the selected option label.
    /// </summary>
    public partial FontWeight OptionSelectedFontWeight { get; set; }

    /// <summary>
    /// Fill of a selected dropdown option. Pair with <see cref="OptionSelectedColor" />.
    /// </summary>
    public partial Color OptionSelectedBg { get; set; }

    /// <summary>
    /// Fill of the active option in the dropdown, for example during keyboard navigation.
    /// </summary>
    public partial Color OptionActiveBg { get; set; }

    /// <summary>
    /// Distance between the painted option background and the edge of its interactive row.
    /// </summary>
    public partial Thickness OptionBackgroundInset { get; set; }

    /// <summary>
    /// Corner radius of the option background painted inside <see cref="OptionBackgroundInset" />.
    /// </summary>
    public partial CornerRadius OptionBorderRadius { get; set; }

    /// <summary>
    /// Space between the option row edge and its label and selection mark.
    /// </summary>
    public partial Thickness OptionPadding { get; set; }

    /// <summary>
    /// Height of an option row in the dropdown list.
    /// </summary>
    public partial double OptionHeight { get; set; }

    /// <summary>
    /// Padding of the prefix and suffix areas of the select. Reserved: the value is kept but not applied yet.
    /// </summary>
    [ReservedToken("antd Select selectAffixPadding. The prefix is aligned by MultiModePrefixIndent* and the suffix sits " +
                   "inside the input frame padding, so no separate affix padding is applied yet.")]
    public partial Thickness SelectAffixPadding { get; set; }

    /// <summary>
    /// Fixed margin of the tags in the multiple-selection mode. Reserved: the value is kept but not applied yet.
    /// </summary>
    [ReservedToken("antd Select INTERNAL_FIXED_ITEM_MARGIN (floor(paddingXXS / 2)). Multiple-mode tags are spaced by " +
                   "the tag panels (SpacingXXS) and sized by MultipleItemHeight*, so no tag margin is applied yet.")]
    public partial Thickness FixedItemMargin { get; set; }

    /// <summary>
    /// Space inside the popup frame around the dropdown list.
    /// </summary>
    public partial Thickness PopupContentPadding { get; set; }

    /// <summary>
    /// Whether the input frame keeps a transparent border at rest and on hover.
    /// </summary>
    public partial bool TransparentOutlineBorder { get; set; }

    /// <summary>
    /// Whether the input frame draws only its bottom border.
    /// </summary>
    public partial bool BottomOutlineBorder { get; set; }

    /// <summary>
    /// Whether the select indicator uses the muted description color.
    /// </summary>
    public partial bool MutedSelectIndicator { get; set; }
}
