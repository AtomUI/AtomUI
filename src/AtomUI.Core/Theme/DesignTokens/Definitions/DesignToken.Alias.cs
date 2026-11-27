using Avalonia;
using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// Background of a content region while it is hovered.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorFillContentHover { get; set; }

    /// <summary>
    /// Alternate background of an element, used in place of its default background to set it apart.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorFillAlter { get; set; }

    /// <summary>
    /// Background of a content region.
    /// Use <see cref="ColorFillContentHover" /> while the region is hovered.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorFillContent { get; set; }

    /// <summary>
    /// Background of a container while it is disabled.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorBgContainerDisabled { get; set; }

    /// <summary>
    /// Background painted behind text while it is hovered.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorBgTextHover { get; set; }

    /// <summary>
    /// Background painted behind text while it is in the active state.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorBgTextActive { get; set; }

    /// <summary>
    /// Background color that separates an element from what is behind it, for example the shadow of a count badge
    /// or dot badge and the border of an avatar in a group.
    /// Defaults to <see cref="ColorBgContainer" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorBorderBg { get; set; }

    /// <summary>
    /// Color of separator lines.
    /// It is the same color as <c>ColorBorderSecondary</c>, but defined with transparency.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorSplit { get; set; }

    /// <summary>
    /// Color of the placeholder text shown by an empty input.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorTextPlaceholder { get; set; }

    /// <summary>
    /// Text color while the control is disabled.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorTextDisabled { get; set; }

    /// <summary>
    /// Text color of headings.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorTextHeading { get; set; }

    /// <summary>
    /// Text color of labels.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorTextLabel { get; set; }

    /// <summary>
    /// Text color of descriptive text.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorTextDescription { get; set; }

    /// <summary>
    /// Fixed light text color for text painted on a coloured background,
    /// for example the label of a primary button.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorTextLightSolid { get; set; }

    /// <summary>
    /// Color of icons for secondary actions,
    /// such as the clear button of an input or the close button of an Alert.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorIcon { get; set; }

    /// <summary>
    /// Hover color of the icons that use <see cref="ColorIcon" />,
    /// such as the clear button of an input or the close button of an Alert.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorIconHover { get; set; }

    /// <summary>
    /// Color used to highlight an element on a page.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorHighlight { get; set; }

    /// <summary>
    /// Color of the outline drawn around an input control.
    /// The warning and error states use
    /// <see cref="ColorWarningOutline" /> and <see cref="ColorErrorOutline" /> instead.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorControlOutline { get; set; }

    /// <summary>
    /// Color of the outline drawn around an input control in the warning state.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorWarningOutline { get; set; }

    /// <summary>
    /// Color of the outline drawn around an input control in the error state.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorErrorOutline { get; set; }

    // Font
    /// <summary>
    /// Font size of operation icons inside select-like controls such as Select and Cascader.
    /// It normally equals <c>FontSizeSM</c>.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double FontSizeIcon { get; set; }

    /// <summary>
    /// Smallest size in the scale of inline icons.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double IconSizeXS { get; set; }

    /// <summary>
    /// Small size in the scale of inline icons.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double IconSizeSM { get; set; }

    /// <summary>
    /// Normal size in the scale of inline icons.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double IconSize { get; set; }

    /// <summary>
    /// Largest size in the scale of inline icons.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double IconSizeLG { get; set; }

    /// <summary>
    /// Font weight of headings and of selected items.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial FontWeight FontWeightStrong { get; set; }

    // Control
    /// <summary>
    /// Width of the outline drawn around an input control.
    /// <see cref="ColorControlOutline" /> supplies its color.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double ControlOutlineWidth { get; set; }

    /// <summary>
    /// Content padding of a medium-size input control.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness InputPadding { get; set; }

    /// <summary>
    /// Content padding of a small-size input control.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness InputPaddingSM { get; set; }

    /// <summary>
    /// Content padding of a large-size input control.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness InputPaddingLG { get; set; }

    /// <summary>
    /// Compensation margin between the content of an input control and its border.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness InputContentMargin { get; set; }

    /// <summary>
    /// Border color of a focused (active) input control; pairs with <see cref="InputActiveShadow" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color InputActiveBorderColor { get; set; }

    /// <summary>
    /// Box shadow that draws the outline around an input control in the focused (active) state.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial BoxShadows InputActiveShadow { get; set; }

    /// <summary>
    /// Box shadow that draws the outline around an input control
    /// that is focused (active) and in the error state.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial BoxShadows InputErrorActiveShadow { get; set; }

    /// <summary>
    /// Box shadow that draws the outline around an input control
    /// that is focused (active) and in the warning state.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial BoxShadows InputWarningActiveShadow { get; set; }

    /// <summary>
    /// Background of a control item while the pointer is over it.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ControlItemBgHover { get; set; }

    /// <summary>
    /// Background of a control item in the selected (active) state.
    /// The text on it uses <see cref="ControlItemColorActive" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ControlItemBgActive { get; set; }

    /// <summary>
    /// Background of a selected (active) control item while the pointer is over it.
    /// The text on it uses <see cref="ControlItemColorActive" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ControlItemBgActiveHover { get; set; }

    /// <summary>
    /// The text color of a selected (active) item drawn on <see cref="ControlItemBgActive" /> and
    /// <see cref="ControlItemBgActiveHover" />: the selected option, list item, tree node, data-grid row and range or
    /// time cell. Defaults to <see cref="ColorText" />; a theme whose selected background is a solid color sets the
    /// matching contrast color once here, and the controls' own selected-text tokens default to it.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ControlItemColorActive { get; set; }

    /// <summary>
    /// Size, in pixels, of the interactive indicator inside a control: the check indicator of a CheckBox uses it
    /// directly, and the expand icon and expand column of a data grid are sized from it.
    /// Defaults to half of <c>ControlHeight</c>.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double ControlInteractiveSize { get; set; }

    /// <summary>
    /// Background of a selected (active) control item while it is disabled.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ControlItemBgActiveDisabled { get; set; }

    /// <summary>
    /// Line width used in the focused state.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double LineWidthFocus { get; set; }
    
    /// <summary>
    /// Border thickness of the focus visual in the focused state.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness FocusVisualBorderThickness {  get; set; }
    
    /// <summary>
    /// Border color of the focus visual in the focused state;
    /// <see cref="FocusVisualBorderThickness" /> sets its thickness.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Color ColorFocusBorder { get; set; }

    /// <summary>
    /// Range over which the wave animation spreads.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double WaveAnimationRange { get; set; }

    /// <summary>
    /// Opacity at which the wave animation starts.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double WaveStartOpacity { get; set; }

    // Padding
    /// <summary>
    /// Extra-extra-small (XXS) step of the padding scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyPaddingXXS { get; set; }
    
    // Padding
    /// <summary>
    /// Extra-extra-small (XXS) step of the padding scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness PaddingXXS { get; set; }

    /// <summary>
    /// Extra-small (XS) step of the padding scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyPaddingXS { get; set; }
    
    /// <summary>
    /// Extra-small (XS) step of the padding scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness PaddingXS { get; set; }

    /// <summary>
    /// Small (SM) step of the padding scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyPaddingSM { get; set; }
    
    /// <summary>
    /// Small (SM) step of the padding scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness PaddingSM { get; set; }

    /// <summary>
    /// Base step of the padding scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyPadding { get; set; }
    
    /// <summary>
    /// Base step of the padding scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness Padding { get; set; }

    /// <summary>
    /// Medium (MD) step of the padding scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyPaddingMD { get; set; }
    
    /// <summary>
    /// Medium (MD) step of the padding scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness PaddingMD { get; set; }

    /// <summary>
    /// Large (LG) step of the padding scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyPaddingLG { get; set; }
    
    /// <summary>
    /// Large (LG) step of the padding scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness PaddingLG { get; set; }

    /// <summary>
    /// Extra-large (XL) step of the padding scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyPaddingXL { get; set; }
    
    /// <summary>
    /// Extra-large (XL) step of the padding scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness PaddingXL { get; set; }

    // Padding Content
    /// <summary>
    /// Large (LG) horizontal padding of content, for large screens.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double PaddingContentHorizontalLG { get; set; }

    /// <summary>
    /// Horizontal padding of content.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double PaddingContentHorizontal { get; set; }

    /// <summary>
    /// Small (SM) horizontal padding of content, for small screens.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double PaddingContentHorizontalSM { get; set; }

    /// <summary>
    /// Extra-small (XS) horizontal padding of content, for small screens.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double PaddingContentHorizontalXS { get; set; }

    /// <summary>
    /// Large (LG) vertical padding of content, for large screens.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double PaddingContentVerticalLG { get; set; }

    /// <summary>
    /// Vertical padding of content.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double PaddingContentVertical { get; set; }

    /// <summary>
    /// Small (SM) vertical padding of content, for small screens.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double PaddingContentVerticalSM { get; set; }

    /// <summary>
    /// Smallest (XXS) step of the margin scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyMarginXXS { get; set; }
    
    /// <summary>
    /// Smallest (XXS) step of the margin scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness MarginXXS { get; set; }

    /// <summary>
    /// Small (XS) step of the margin scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyMarginXS { get; set; }
    
    /// <summary>
    /// Small (XS) step of the margin scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness MarginXS { get; set; }

    /// <summary>
    /// Small-to-medium (SM) step of the margin scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyMarginSM { get; set; }
    
    /// <summary>
    /// Small-to-medium (SM) step of the margin scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness MarginSM { get; set; }

    /// <summary>
    /// Medium step of the margin scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyMargin { get; set; }
    
    /// <summary>
    /// Medium step of the margin scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness Margin { get; set; }

    /// <summary>
    /// Medium-to-large (MD) step of the margin scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyMarginMD { get; set; }
    
    /// <summary>
    /// Medium-to-large (MD) step of the margin scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness MarginMD { get; set; }

    /// <summary>
    /// Large (LG) step of the margin scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyMarginLG { get; set; }
    
    /// <summary>
    /// Large (LG) step of the margin scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness MarginLG { get; set; }

    /// <summary>
    /// Extra-large (XL) step of the margin scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyMarginXL { get; set; }

    /// <summary>
    /// Extra-large (XL) step of the margin scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness MarginXL { get; set; }
    
    /// <summary>
    /// Largest (XXL) step of the margin scale, as a single value for all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double UniformlyMarginXXL { get; set; }
    
    /// <summary>
    /// Largest (XXL) step of the margin scale, with a value for each of the four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial Thickness MarginXXL { get; set; }
    
    /// <summary>
    /// Smallest (XXS) gap between layout elements.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double SpacingXXS { get; set; }

    /// <summary>
    /// Small (XS) gap between layout elements.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double SpacingXS { get; set; }

    /// <summary>
    /// Small-to-medium (SM) gap between layout elements.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double SpacingSM { get; set; }

    /// <summary>
    /// Medium gap between layout elements.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double Spacing { get; set; }

    /// <summary>
    /// Medium-to-large (MD) gap between layout elements.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double SpacingMD { get; set; }

    /// <summary>
    /// Large (LG) gap between layout elements.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double SpacingLG { get; set; }
    
    /// <summary>
    /// Extra-large (XL) gap between layout elements.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double SpacingXL { get; set; }
    
    /// <summary>
    /// Largest (XXL) gap between layout elements.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double SpacingXXL { get; set; }

    // =============== Legacy: should be remove ===============
    /// <summary>
    /// Opacity applied in the loading state.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double OpacityLoading { get; set; }

    /// <summary>
    /// First-level box shadow of an element.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial BoxShadows BoxShadows { get; set; }

    /// <summary>
    /// Second-level box shadow of an element.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial BoxShadows BoxShadowsSecondary { get; set; }

    /// <summary>
    /// Third-level box shadow of an element.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial BoxShadows BoxShadowsTertiary { get; set; }

    /// <summary>
    /// Text decoration of a link.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial TextDecorationInfo? LinkDecoration { get; set; }

    /// <summary>
    /// Text decoration of a link while it is hovered.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial TextDecorationInfo? LinkHoverDecoration { get; set; }

    /// <summary>
    /// Text decoration of a link while it is focused.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial TextDecorationInfo? LinkFocusDecoration { get; set; }

    /// <summary>
    /// Horizontal padding of a control.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double ControlPaddingHorizontal { get; set; }

    /// <summary>
    /// Horizontal padding of a small (SM) control.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial double ControlPaddingHorizontalSM { get; set; }

    // Media queries breakpoints
    /// <summary>
    /// Screen width, in pixels, of an extra-small (XS) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenXS { get; set; }

    /// <summary>
    /// Minimum screen width, in pixels, of an extra-small (XS) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenXSMin { get; set; }

    /// <summary>
    /// Maximum screen width, in pixels, of an extra-small (XS) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenXSMax { get; set; }

    /// <summary>
    /// Screen width, in pixels, of a small (SM) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenSM { get; set; }

    /// <summary>
    /// Minimum screen width, in pixels, of a small (SM) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenSMMin { get; set; }

    /// <summary>
    /// Maximum screen width, in pixels, of a small (SM) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenSMMax { get; set; }

    /// <summary>
    /// Screen width, in pixels, of a medium (MD) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenMD { get; set; }

    /// <summary>
    /// Minimum screen width, in pixels, of a medium (MD) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenMDMin { get; set; }

    /// <summary>
    /// Maximum screen width, in pixels, of a medium (MD) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenMDMax { get; set; }

    /// <summary>
    /// Screen width, in pixels, of a large (LG) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenLG { get; set; }

    /// <summary>
    /// Minimum screen width, in pixels, of a large (LG) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenLGMin { get; set; }

    /// <summary>
    /// Maximum screen width, in pixels, of a large (LG) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenLGMax { get; set; }

    /// <summary>
    /// Screen width, in pixels, of an extra-large (XL) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenXL { get; set; }

    /// <summary>
    /// Minimum screen width, in pixels, of an extra-large (XL) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenXLMin { get; set; }

    /// <summary>
    /// Maximum screen width, in pixels, of an extra-large (XL) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenXLMax { get; set; }

    /// <summary>
    /// Screen width, in pixels, of an extra-extra-large (XXL) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenXXL { get; set; }

    /// <summary>
    /// Minimum screen width, in pixels, of an extra-extra-large (XXL) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenXXLMin { get; set; }

    /// <summary>
    /// Maximum screen width, in pixels, of an extra-extra-large (XXL) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenXXLMax { get; set; }

    /// <summary>
    /// Screen width, in pixels, of an extra-extra-extra-large (XXXL) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenXXXL { get; set; }

    /// <summary>
    /// Minimum screen width, in pixels, of an extra-extra-extra-large (XXXL) screen.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public partial int ScreenXXXLMin { get; set; }
}
