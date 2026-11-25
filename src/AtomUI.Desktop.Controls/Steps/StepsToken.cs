using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class StepsToken : AbstractControlDesignToken
{

    /// <summary>Maximum width of the step description presenter.</summary>
    public partial double DescriptionMaxWidth { get; set; }

    /// <summary>Minimum width and height of the container when the step supplies a custom icon.</summary>
    public partial double CustomIconSize { get; set; }

    /// <summary>Width and height of the custom icon inside its container.</summary>
    public partial double CustomIconFontSize { get; set; }

    /// <summary>Width and height of the standard step indicator container.</summary>
    public partial double IconContainerSize { get; set; }

    /// <summary>Corner radius of the standard step indicator container.</summary>
    public partial CornerRadius IconContainerCornerRadius { get; set; }

    /// <summary>
    /// Size of the number and the built-in completion and error marks in the standard step indicator.
    /// </summary>
    public partial double IconFontSize { get; set; }

    /// <summary>Diameter of the filled or outlined dot on a non-current step.</summary>
    public partial double DotSize { get; set; }

    /// <summary>Diameter of the filled or outlined dot on the current step.</summary>
    public partial double DotCurrentSize { get; set; }

    /// <summary>Foreground of the arrow separating steps in navigation mode.</summary>
    public partial Color NavArrowColor { get; set; }

    /// <summary>Width and height of the standard step indicator container at the small size.</summary>
    public partial double IconContainerSizeSM { get; set; }

    /// <summary>Margin around the title when the step label is arranged horizontally.</summary>
    public partial Thickness HorizontalHeaderMargin { get; set; }

    /// <summary>Margin around the step subtitle.</summary>
    public partial Thickness SubHeaderMargin { get; set; }

    /// <summary>Space between consecutive steps in a vertical layout.</summary>
    public partial double VerticalItemSpacing { get; set; }

    /// <summary>Padding inside the description presenter in a vertical layout.</summary>
    public partial Thickness VerticalDescriptionPadding { get; set; }

    /// <summary>
    /// Margin between the indicator and the content when the step label is stacked beneath the indicator.
    /// </summary>
    public partial Thickness VerticalLabelContentMargin { get; set; }

    /// <summary>Margin around the separator arrow in vertical navigation mode.</summary>
    public partial Thickness VerticalNavArrowMargin { get; set; }

    /// <summary>
    /// Margin around the separator arrow in vertical navigation mode at the small size.
    /// </summary>
    public partial Thickness VerticalNavArrowMarginSM { get; set; }

    /// <summary>Padding inside a step in vertical navigation mode.</summary>
    public partial Thickness VerticalNavItemPadding { get; set; }

    /// <summary>Gap between a navigation step's content and its active indicator bar.</summary>
    public partial double NavItemGutter { get; set; }

    #region 内部 Token

    /// <summary>
    /// Foreground of the number or custom icon on a waiting step. Pairs with <see cref="WaitIconBgColor"/>.
    /// </summary>
    public partial Color WaitIconColor { get; set; }
    /// <summary>
    /// Fill inside the standard indicator of a waiting step. Pairs with <see cref="WaitIconColor"/>.
    /// </summary>
    public partial Color WaitIconBgColor { get; set; }
    /// <summary>Outline of the standard indicator of a waiting step.</summary>
    public partial Color WaitIconBorderColor { get; set; }
    /// <summary>
    /// Fill inside the standard indicator of a completed step. Pairs with <see cref="FinishIconColor"/>.
    /// </summary>
    public partial Color FinishIconBgColor { get; set; }
    /// <summary>Outline of the standard indicator of a completed step.</summary>
    public partial Color FinishIconBorderColor { get; set; }

    /// <summary>Colour of the connector extending from a step in progress.</summary>
    public partial Color ProcessTailColor { get; set; }
    /// <summary>
    /// Foreground of the number in the standard indicator of a step in progress. Pairs with
    /// <see cref="ProcessIconBgColor"/>.
    /// </summary>
    public partial Color ProcessIconColor { get; set; }
    /// <summary>Foreground of the title of a step in progress.</summary>
    public partial Color ProcessTitleColor { get; set; }

    /// <summary>The font weight of the step titles (inline steps keep the normal weight).</summary>
    public partial FontWeight TitleFontWeight { get; set; }
    /// <summary>Foreground of the subtitle and description of a step in progress.</summary>
    public partial Color ProcessDescriptionColor { get; set; }
    /// <summary>
    /// Fill inside the standard indicator of a step in progress. Pairs with <see cref="ProcessIconColor"/>.
    /// </summary>
    public partial Color ProcessIconBgColor { get; set; }
    /// <summary>
    /// Outline of the standard indicator and foreground of a custom icon on a step in progress. Also accentuates an
    /// invocable step on hover.
    /// </summary>
    public partial Color ProcessIconBorderColor { get; set; }
    /// <summary>
    /// Fill for solid and inline dots and outline for hollow dots on a step in progress. Also highlights a clickable
    /// dot on hover.
    /// </summary>
    public partial Color ProcessDotColor { get; set; }
    /// <summary>Foreground of the title of a waiting step.</summary>
    public partial Color WaitTitleColor { get; set; }
    /// <summary>Foreground of the subtitle and description of a waiting step.</summary>
    public partial Color WaitDescriptionColor { get; set; }
    /// <summary>Colour of the connector extending from a waiting step.</summary>
    public partial Color WaitTailColor { get; set; }
    /// <summary>Fill for solid and inline dots and outline for hollow dots on a waiting step.</summary>
    public partial Color WaitDotColor { get; set; }
    /// <summary>
    /// Foreground of the completion mark or custom icon on a completed step. Pairs with
    /// <see cref="FinishIconBgColor"/>.
    /// </summary>
    public partial Color FinishIconColor { get; set; }
    /// <summary>Foreground of the title of a completed step.</summary>
    public partial Color FinishTitleColor { get; set; }
    /// <summary>Foreground of the subtitle and description of a completed step.</summary>
    public partial Color FinishDescriptionColor { get; set; }
    /// <summary>Colour of the connector extending from a completed step.</summary>
    public partial Color FinishTailColor { get; set; }
    /// <summary>Fill for solid and inline dots and outline for hollow dots on a completed step.</summary>
    public partial Color FinishDotColor { get; set; }
    /// <summary>
    /// Foreground of the icon in the standard indicator of a step in error. Pairs with <see cref="ErrorIconBgColor"/>.
    /// </summary>
    public partial Color ErrorIconColor { get; set; }
    /// <summary>Foreground of the title of a step in error.</summary>
    public partial Color ErrorTitleColor { get; set; }
    /// <summary>Foreground of the subtitle and description of a step in error.</summary>
    public partial Color ErrorDescriptionColor { get; set; }
    /// <summary>Colour of the connector extending from a step in error.</summary>
    public partial Color ErrorTailColor { get; set; }
    /// <summary>
    /// Fill inside the standard indicator of a step in error. Pairs with <see cref="ErrorIconColor"/>.
    /// </summary>
    public partial Color ErrorIconBgColor { get; set; }
    /// <summary>
    /// Outline of the standard indicator and foreground of a custom icon on a step in error.
    /// </summary>
    public partial Color ErrorIconBorderColor { get; set; }
    /// <summary>Fill for solid and inline dots and outline for hollow dots on a step in error.</summary>
    public partial Color ErrorDotColor { get; set; }
    /// <summary>Fill of the active indicator bar in navigation mode.</summary>
    public partial Color StepsNavActiveColor { get; set; }
    // Steps inline variable
    /// <summary>Diameter of the indicator dot in inline mode.</summary>
    public partial double InlineDotSize { get; set; }
    /// <summary>Margin around the title presenter in inline mode.</summary>
    public partial Thickness InlineHeaderMargin { get; set; }
    /// <summary>Padding inside the title presenter in inline mode.</summary>
    public partial Thickness InlineHeaderPadding { get; set; }
    /// <summary>Padding inside the step item in inline mode.</summary>
    public partial Thickness InlineItemPadding { get; set; }
    /// <summary>Foreground of the title in inline mode.</summary>
    public partial Color InlineTitleColor { get; set; }
    /// <summary>Colour of the connector between steps in inline mode.</summary>
    public partial Color InlineTailColor { get; set; }
    /// <summary>Thickness of the connector in the filled-dot and outlined-dot variants.</summary>
    public partial double DotLineThickness { get; set; }

    /// <summary>Outline thickness of a hollow step indicator dot.</summary>
    public partial Thickness OutlineDotBorderThickness { get; set; }
    /// <summary>
    /// Padding inside the standard-size indicator's frame when it shows circular progress.
    /// </summary>
    public partial Thickness ProgressFramePadding { get; set; }
    /// <summary>Padding inside the small indicator's frame when it shows circular progress.</summary>
    public partial Thickness ProgressFramePaddingSM { get; set; }
    /// <summary>
    /// Colour of the unfilled circular progress track. Pairs with <see cref="ProgressColor"/>.
    /// </summary>
    public partial Color ProgressGrooveColor { get; set; }
    /// <summary>
    /// Colour of the filled arc of the circular progress track. Pairs with <see cref="ProgressGrooveColor"/>.
    /// </summary>
    public partial Color ProgressColor { get; set; }
    /// <summary>Width of the chevron joining adjacent panel steps at the standard size.</summary>
    public partial double PanelArrowWidth { get; set; }
    /// <summary>Width of the chevron joining adjacent panel steps at the small size.</summary>
    public partial double PanelArrowWidthSM { get; set; }
    /// <summary>
    /// Fill of a waiting panel in the filled variant on a non-current step. Pairs with
    /// <see cref="PanelWaitTextColor"/>.
    /// </summary>
    public partial Color PanelWaitBackground { get; set; }
    /// <summary>
    /// Fill of a panel in progress in the filled variant on a non-current step. Pairs with
    /// <see cref="PanelProcessTextColor"/>.
    /// </summary>
    public partial Color PanelProcessBackground { get; set; }
    /// <summary>
    /// Fill of a completed panel in the filled variant on a non-current step. Pairs with
    /// <see cref="PanelFinishTextColor"/>.
    /// </summary>
    public partial Color PanelFinishBackground { get; set; }
    /// <summary>
    /// Fill of a panel in error in the filled variant on a non-current step. Pairs with
    /// <see cref="PanelErrorTextColor"/>.
    /// </summary>
    public partial Color PanelErrorBackground { get; set; }
    /// <summary>
    /// Fill of the current waiting panel in the filled variant. Pairs with <see cref="PanelActiveTextColor"/>.
    /// </summary>
    public partial Color PanelWaitActiveBackground { get; set; }
    /// <summary>
    /// Fill of the current panel in progress in the filled variant. Pairs with <see cref="PanelActiveTextColor"/>.
    /// </summary>
    public partial Color PanelProcessActiveBackground { get; set; }
    /// <summary>
    /// Fill of the current completed panel in the filled variant. Pairs with <see cref="PanelActiveTextColor"/>.
    /// </summary>
    public partial Color PanelFinishActiveBackground { get; set; }
    /// <summary>
    /// Fill of the current panel in error in the filled variant. Pairs with <see cref="PanelErrorActiveTextColor"/>.
    /// </summary>
    public partial Color PanelErrorActiveBackground { get; set; }
    /// <summary>
    /// Foreground of the title, subtitle and description in a waiting panel. Pairs with
    /// <see cref="PanelWaitBackground"/> in the filled variant on a non-current step.
    /// </summary>
    public partial Color PanelWaitTextColor { get; set; }
    /// <summary>
    /// Foreground of the title, subtitle and description in a panel in progress. Pairs with
    /// <see cref="PanelProcessBackground"/> in the filled variant on a non-current step.
    /// </summary>
    public partial Color PanelProcessTextColor { get; set; }
    /// <summary>
    /// Foreground of the title, subtitle and description in a completed panel. Pairs with
    /// <see cref="PanelFinishBackground"/> in the filled variant on a non-current step.
    /// </summary>
    public partial Color PanelFinishTextColor { get; set; }
    /// <summary>
    /// Foreground of the title, subtitle and description in a panel in error. Pairs with
    /// <see cref="PanelErrorBackground"/> in the filled variant on a non-current step.
    /// </summary>
    public partial Color PanelErrorTextColor { get; set; }
    /// <summary>
    /// Foreground of the title, subtitle and description in the current filled panel when it is waiting, in progress or
    /// completed. Pairs with <see cref="PanelWaitActiveBackground"/>, <see cref="PanelProcessActiveBackground"/> and
    /// <see cref="PanelFinishActiveBackground"/>.
    /// </summary>
    public partial Color PanelActiveTextColor { get; set; }
    /// <summary>
    /// Foreground of the title, subtitle and description in the current filled panel in error. Pairs with
    /// <see cref="PanelErrorActiveBackground"/>.
    /// </summary>
    public partial Color PanelErrorActiveTextColor { get; set; }
    /// <summary>Thickness of each border edge on the first panel step.</summary>
    public partial Thickness PanelFirstBorderThickness { get; set; }
    /// <summary>Thickness of each border edge on a panel step between the first and last steps.</summary>
    public partial Thickness PanelMiddleBorderThickness { get; set; }
    /// <summary>Thickness of each border edge on the last panel step.</summary>
    public partial Thickness PanelLastBorderThickness { get; set; }
    /// <summary>Corner radii of the first panel step at the standard size.</summary>
    public partial CornerRadius PanelFirstCornerRadius { get; set; }
    /// <summary>Corner radii of the last panel step at the standard size.</summary>
    public partial CornerRadius PanelLastCornerRadius { get; set; }
    /// <summary>Corner radii of the first panel step at the small size.</summary>
    public partial CornerRadius PanelFirstCornerRadiusSM { get; set; }
    /// <summary>Corner radii of the last panel step at the small size.</summary>
    public partial CornerRadius PanelLastCornerRadiusSM { get; set; }

    #endregion
}
