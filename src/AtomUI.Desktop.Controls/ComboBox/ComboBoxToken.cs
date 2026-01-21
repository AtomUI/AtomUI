using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ComboBoxToken : ButtonSpinnerToken
{
    public new const string ID = "ComboBox";

    public ComboBoxToken()
        : base(ID)
    {
    }

    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public Thickness PopupContentPadding { get; set; }

    /// <summary>
    /// 列表项文字颜色
    /// </summary>
    public Color ItemColor { get; set; }

    /// <summary>
    /// 列表项文字悬浮颜色
    /// </summary>
    public Color ItemHoverColor { get; set; }

    /// <summary>
    /// 列表项文字选中颜色
    /// </summary>
    public Color ItemSelectedColor { get; set; }

    /// <summary>
    /// 列表项文字禁用颜色
    /// </summary>
    public Color ItemDisabledColor { get; set; }

    /// <summary>
    /// 列表项背景色
    /// </summary>
    public Color ItemBgColor { get; set; }

    /// <summary>
    /// 列表项悬浮态背景色
    /// </summary>
    public Color ItemHoverBgColor { get; set; }

    /// <summary>
    /// 列表项选中背景色
    /// </summary>
    public Color ItemSelectedBgColor { get; set; }

    /// <summary>
    /// 列表项内间距
    /// </summary>
    public Thickness ItemPadding { get; set; }

    /// <summary>
    /// 列表项外边距
    /// </summary>
    public Thickness ItemMargin { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        PopupContentPadding  = new Thickness(SharedToken.UniformlyPaddingXXS, SharedToken.BorderRadiusLG.TopLeft / 2);
        
        var colorTextDisabled  = SharedToken.ColorTextDisabled;
        var colorTextSecondary = SharedToken.ColorTextSecondary;
        var colorBgContainer   = SharedToken.ColorBgElevated;
        var colorBgTextHover   = SharedToken.ColorBgTextHover;

        ItemColor         = colorTextSecondary;
        ItemHoverColor    = colorTextSecondary;
        ItemSelectedColor = SharedToken.ColorText;

        ItemBgColor         = colorBgContainer;
        ItemHoverBgColor    = colorBgTextHover;
        ItemSelectedBgColor = SharedToken.ControlItemBgActive;
        ItemDisabledColor = colorTextDisabled;

        ItemPadding = new Thickness(SharedToken.UniformlyPaddingSM, 0);
        ItemMargin  = new Thickness(0, 0.5);
    }
}