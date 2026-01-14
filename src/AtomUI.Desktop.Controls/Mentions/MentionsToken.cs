using AtomUI.Theme.TokenSystem;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class MentionsToken : AbstractControlDesignToken
{
    public const string ID = "Mentions";
    
    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public Thickness PopupContentPadding { get; set; }
    
    /// <summary>
    /// 选项高度
    /// Height of option
    /// </summary>
    public double OptionHeight { get; set; }
    
    /// <summary>
    /// 候选列表弹窗最小宽度
    /// </summary>
    public double MinPopupWidth { get; set; }
    
    public MentionsToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        OptionHeight        = SharedToken.ControlHeight;
        PopupContentPadding = new Thickness(SharedToken.UniformlyPaddingXXS / 2);
        MinPopupWidth       = 120;
    }
}