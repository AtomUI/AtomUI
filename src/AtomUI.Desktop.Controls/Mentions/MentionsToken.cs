using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class MentionsToken : AbstractControlDesignToken
{
    
    /// <summary>Space around the candidate list inside the suggestion popup.</summary>
    public partial Thickness PopupContentPadding { get; set; }
    
    /// <summary>
    /// 选项高度
    /// Height of option
    /// </summary>
    public partial double OptionHeight { get; set; }
    
    /// <summary>
    /// 候选项文字颜色
    /// Text colour of a candidate
    /// </summary>
    public partial Color OptionColor { get; set; }

    /// <summary>
    /// 选中候选项文字颜色
    /// Text colour of the selected candidate
    /// </summary>
    public partial Color OptionSelectedColor { get; set; }

    /// <summary>Minimum width of the mention suggestion popup.</summary>
    public partial double MinPopupWidth { get; set; }
    
}
