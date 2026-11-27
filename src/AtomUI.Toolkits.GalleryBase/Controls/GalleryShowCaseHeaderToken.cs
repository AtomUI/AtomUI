using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class GalleryShowCaseHeaderToken : AbstractControlDesignToken
{

    /// <summary>
    /// Outer margin of the showcase header, around the summary block and the metadata card.
    /// </summary>
    public partial Thickness HeaderMargin { get; set; }
    /// <summary>
    /// Vertical gap between the summary block of the showcase header and its metadata card.
    /// </summary>
    public partial double HeaderSpacing { get; set; }
    /// <summary>
    /// Vertical gap between the title row, the subtitle and the description in the summary block of the header.
    /// </summary>
    public partial double SummarySpacing { get; set; }
    /// <summary>
    /// Font size of the title of a showcase in its header.
    /// </summary>
    public partial double TitleFontSize { get; set; }
    /// <summary>
    /// Font weight of the title of a showcase in its header.
    /// </summary>
    public partial FontWeight TitleFontWeight { get; set; }
    /// <summary>
    /// Outer margin of the tag panel that follows the title in the showcase header.
    /// </summary>
    public partial Thickness TagsMargin { get; set; }
    /// <summary>
    /// Horizontal gap between adjacent tags in the tag panel of the showcase header.
    /// </summary>
    public partial double TagItemSpacing { get; set; }
    /// <summary>
    /// Vertical gap between the lines of the tag panel of the showcase header, when the tags wrap.
    /// </summary>
    public partial double TagLineSpacing { get; set; }
    /// <summary>
    /// Font size of the subtitle shown under the title in the showcase header.
    /// </summary>
    public partial double SubtitleFontSize { get; set; }
    /// <summary>
    /// Padding inside the metadata card of the showcase header, around the metadata rows.
    /// </summary>
    public partial Thickness MetadataPadding { get; set; }
    /// <summary>
    /// Corner radius of the metadata card of the showcase header.
    /// </summary>
    public partial CornerRadius MetadataCornerRadius { get; set; }
    /// <summary>
    /// Minimum height of the metadata card of the showcase header.
    /// </summary>
    public partial double MetadataMinHeight { get; set; }
    /// <summary>
    /// Reserved for the horizontal metadata layout. The current vertical layout has no consumer for the value, so
    /// it has no visible effect.
    /// </summary>
    [ReservedToken("Retained for compatibility with horizontal metadata layouts; the current vertical metadata layout has no consumer.")]
    public partial double MetadataItemSpacing { get; set; }
    /// <summary>
    /// Vertical gap between the metadata rows in the metadata card of the showcase header.
    /// </summary>
    public partial double MetadataLineSpacing { get; set; }
    /// <summary>
    /// Gap between the label and the value of one metadata row of the showcase header.
    /// </summary>
    public partial double MetadataPairSpacing { get; set; }
    /// <summary>
    /// Width reserved for the label of each metadata row of the showcase header. It feeds the
    /// <see cref="GalleryShowCaseHeader.MetadataLabelWidth" /> property of the header.
    /// </summary>
    public partial double MetadataLabelWidth { get; set; }
    /// <summary>
    /// Width reserved for the value of each metadata row of the showcase header. It feeds the
    /// <see cref="GalleryShowCaseHeader.MetadataValueWidth" /> property of the header.
    /// </summary>
    public partial double MetadataValueWidth { get; set; }
    /// <summary>
    /// Line height of the value text of a metadata row of the showcase header.
    /// </summary>
    public partial double MetadataLineHeight { get; set; }
    /// <summary>
    /// Font family of the value text of a metadata row of the showcase header.
    /// </summary>
    public partial FontFamily? MetadataValueFontFamily { get; set; }

}
