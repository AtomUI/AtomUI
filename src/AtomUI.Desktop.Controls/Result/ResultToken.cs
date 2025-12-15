using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ResultToken : AbstractControlDesignToken
{
    public const string ID = "Result";

    /// <summary>
    /// 标题字体大小
    /// </summary>
    public double HeaderFontSize { get; set; }

    /// <summary>
    /// 副标题字体大小
    /// </summary>
    public double SubHeaderFontSize { get; set; }

    /// <summary>
    /// 图标大小
    /// </summary>
    public double IconSize { get; set; }

    /// <summary>
    /// 额外区域外间距
    /// </summary>
    public Thickness ExtraMargin { get; set; }

    public double ImageWidth { get; set; }
    public double ImageHeight { get; set; }

    public Color ResultInfoIconColor { get; set; }
    public Color ResultSuccessIconColor { get; set; }
    public Color ResultWarningIconColor { get; set; }
    public Color ResultErrorIconColor { get; set; }
    public Thickness FramePadding { get; set; }
    public Thickness ContentPadding { get; set; }
    public Thickness ContentMargin { get; set; }
    public Thickness HeaderMargin { get; set; }
    public Thickness StatusImageMargin { get; set; }

    public ResultToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        HeaderFontSize    = SharedToken.FontSizeHeading3;
        SubHeaderFontSize = SharedToken.FontSize;
        IconSize          = SharedToken.FontSizeHeading3 * 3;
        ExtraMargin       = new Thickness(0, SharedToken.UniformlyMargin, 0, 0);

        ImageWidth  = 250;
        ImageHeight = 295;

        ResultInfoIconColor    = SharedToken.ColorInfo;
        ResultSuccessIconColor = SharedToken.ColorSuccess;
        ResultWarningIconColor = SharedToken.ColorWarning;
        ResultErrorIconColor   = SharedToken.ColorError;

        ContentPadding    = new Thickness(SharedToken.UniformlyPadding * 2.5, SharedToken.UniformlyPaddingLG);
        ContentMargin     = new Thickness(0, SharedToken.UniformlyPaddingLG, 0, 0); 
        StatusImageMargin = new Thickness(0, 0, 0, SharedToken.UniformlyMargin);
        HeaderMargin      = new Thickness(0, SharedToken.UniformlyMarginXS);
        FramePadding      = new Thickness(SharedToken.UniformlyPaddingLG * 2, SharedToken.UniformlyMarginXL);
    }
}