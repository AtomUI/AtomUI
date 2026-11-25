using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class GalleryShowCaseHeaderToken : AbstractControlDesignToken
{

    public partial Thickness HeaderMargin { get; set; }
    public partial double HeaderSpacing { get; set; }
    public partial double SummarySpacing { get; set; }
    public partial double TitleFontSize { get; set; }
    public partial FontWeight TitleFontWeight { get; set; }
    public partial Thickness TagsMargin { get; set; }
    public partial double TagItemSpacing { get; set; }
    public partial double TagLineSpacing { get; set; }
    public partial double SubtitleFontSize { get; set; }
    public partial Thickness MetadataPadding { get; set; }
    public partial CornerRadius MetadataCornerRadius { get; set; }
    public partial double MetadataMinHeight { get; set; }
    [ReservedToken("Retained for compatibility with horizontal metadata layouts; the current vertical metadata layout has no consumer.")]
    public partial double MetadataItemSpacing { get; set; }
    public partial double MetadataLineSpacing { get; set; }
    public partial double MetadataPairSpacing { get; set; }
    public partial double MetadataLabelWidth { get; set; }
    public partial double MetadataValueWidth { get; set; }
    public partial double MetadataLineHeight { get; set; }
    public partial FontFamily? MetadataValueFontFamily { get; set; }

}
