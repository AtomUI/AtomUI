using AtomUI.Theme.TokenSystem;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public class EmptyToken : AbstractControlDesignToken
{
    public const string ID = "Empty";

    /// <summary>
    /// 空图片的高度
    /// </summary>
    public double EmptyImgHeight { get; set; }

    public double EmptyImgHeightSM { get; set; }
    public double EmptyImgHeightMD { get; set; }
    
    public Thickness DescriptionMargin { get; set; }
    public Thickness DescriptionMarginSM { get; set; }

    public EmptyToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var controlHeightLG = SharedToken.ControlHeightLG;
        EmptyImgHeight      = controlHeightLG * 2.5;
        EmptyImgHeightMD    = controlHeightLG * 1.85;
        EmptyImgHeightSM    = controlHeightLG * 0.875;
        DescriptionMargin   = new Thickness(0, SharedToken.UniformlyMarginSM, 0, 0);
        DescriptionMarginSM = new Thickness(0, SharedToken.UniformlyMarginXS, 0, 0);
    }
}