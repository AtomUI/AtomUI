using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed class GalleryShowCaseHeaderToken : AbstractControlDesignToken
{

    public Thickness HeaderMargin { get; set; }
    public double HeaderSpacing { get; set; }
    public double SummarySpacing { get; set; }
    public double TitleFontSize { get; set; }
    public FontWeight TitleFontWeight { get; set; }
    public Thickness TagsMargin { get; set; }
    public double TagItemSpacing { get; set; }
    public double TagLineSpacing { get; set; }
    public double SubtitleFontSize { get; set; }
    public Thickness MetadataPadding { get; set; }
    public CornerRadius MetadataCornerRadius { get; set; }
    public double MetadataMinHeight { get; set; }
    [ReservedToken("Retained for compatibility with horizontal metadata layouts; the current vertical metadata layout has no consumer.")]
    public double MetadataItemSpacing { get; set; }
    public double MetadataLineSpacing { get; set; }
    public double MetadataPairSpacing { get; set; }
    public double MetadataLabelWidth { get; set; }
    public double MetadataValueWidth { get; set; }
    public double MetadataLineHeight { get; set; }
    public FontFamily MetadataValueFontFamily { get; set; } = FontFamily.Default;

    public GalleryShowCaseHeaderToken()

    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        HeaderMargin              = new Thickness(EffectiveGlobalToken.SizeUnit * 7, EffectiveGlobalToken.SizeUnit * 6, EffectiveGlobalToken.SizeUnit * 7, EffectiveGlobalToken.SizeUnit * 4 + 2);
        HeaderSpacing             = EffectiveGlobalToken.SizeUnit * 4;
        SummarySpacing            = EffectiveGlobalToken.SizeUnit + 2;
        TitleFontSize             = EffectiveGlobalToken.FontSizeHeading2;
        TitleFontWeight           = EffectiveGlobalToken.FontWeightStrong;
        TagsMargin                = new Thickness(EffectiveGlobalToken.SizeUnit * 2 + 2, 0, 0, 0);
        TagItemSpacing            = EffectiveGlobalToken.SizeUnit * 2 + 2;
        TagLineSpacing            = EffectiveGlobalToken.SizeUnit + 2;
        SubtitleFontSize          = EffectiveGlobalToken.FontSizeLG;
        MetadataPadding           = new Thickness(EffectiveGlobalToken.SizeUnit * 4, EffectiveGlobalToken.SizeUnit * 2);
        MetadataCornerRadius      = EffectiveGlobalToken.BorderRadiusLG;
        MetadataMinHeight         = EffectiveGlobalToken.SizeUnit * 12;
        MetadataItemSpacing       = EffectiveGlobalToken.SizeUnit * 12;
        MetadataLineSpacing       = EffectiveGlobalToken.SizeUnit * 2;
        MetadataPairSpacing       = EffectiveGlobalToken.SizeUnit * 2 + 2;
        MetadataLabelWidth        = EffectiveGlobalToken.SizeUnit * 21;
        MetadataValueWidth        = EffectiveGlobalToken.SizeUnit * 50;
        MetadataLineHeight        = EffectiveGlobalToken.SizeUnit * 5 + 2;
        MetadataValueFontFamily   = EffectiveGlobalToken.FontFamily ?? FontFamily.Default;
    }

}
