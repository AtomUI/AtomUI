using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.TokenRules;

/// <summary>The Generic view of <see cref="GalleryShowCaseHeaderToken"/>.</summary>
[ControlTokenRule]
internal sealed class GenericGalleryShowCaseHeaderTokenRule : IControlTokenRule<LanguageDesignToken, GalleryShowCaseHeaderToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<GalleryShowCaseHeaderToken> target)
    {
        var token  = target.Own;
        var global = target.Global;

        token.HeaderMargin              = new Thickness(global.SizeUnit * 7, global.SizeUnit * 6, global.SizeUnit * 7, global.SizeUnit * 4 + 2);
        token.HeaderSpacing             = global.SizeUnit * 4;
        token.SummarySpacing            = global.SizeUnit + 2;
        token.TitleFontSize             = global.FontSizeHeading2;
        token.TitleFontWeight           = global.FontWeightStrong;
        token.TagsMargin                = new Thickness(global.SizeUnit * 2 + 2, 0, 0, 0);
        token.TagItemSpacing            = global.SizeUnit * 2 + 2;
        token.TagLineSpacing            = global.SizeUnit + 2;
        token.SubtitleFontSize          = global.FontSizeLG;
        token.MetadataPadding           = new Thickness(global.SizeUnit * 4, global.SizeUnit * 2);
        token.MetadataCornerRadius      = global.BorderRadiusLG;
        token.MetadataMinHeight         = global.SizeUnit * 12;
        token.MetadataItemSpacing       = global.SizeUnit * 12;
        token.MetadataLineSpacing       = global.SizeUnit * 2;
        token.MetadataPairSpacing       = global.SizeUnit * 2 + 2;
        token.MetadataLabelWidth        = global.SizeUnit * 21;
        token.MetadataValueWidth        = global.SizeUnit * 50;
        token.MetadataLineHeight        = global.SizeUnit * 5 + 2;
        token.MetadataValueFontFamily   = global.FontFamily ?? FontFamily.Default;
    }
}
