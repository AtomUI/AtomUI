using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class StatisticToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 标题字体大小
    /// Title font size
    /// </summary>
    public double TitleFontSize { get; set; }
    
    /// <summary>
    /// 内容字体大小
    /// Content font size
    /// </summary>
    public double ContentFontSize { get; set; }
    
    [ControlTokenDefault(nameof(TitleFontSize))]
    private static double DefaultForTitleFontSize(in ControlTokenDefaultContext context) => context.Global.FontSize;

    [ControlTokenDefault(nameof(ContentFontSize))]
    private static double DefaultForContentFontSize(in ControlTokenDefaultContext context) => context.Global.FontSizeHeading3;
    
}
