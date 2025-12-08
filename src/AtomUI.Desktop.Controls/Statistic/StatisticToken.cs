using AtomUI.Theme.TokenSystem;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class StatisticToken : AbstractControlDesignToken
{
    public const string ID = "Statistic";
    
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
    
    public StatisticToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues()
    {
        base.CalculateTokenValues();
        TitleFontSize = SharedToken.FontSize;
        ContentFontSize = SharedToken.FontSizeHeading3;
    }
}