using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ListToken : AbstractControlDesignToken
{
    public const string ID = "List";

    public ListToken()
        : this(ID)
    {
    }

    protected ListToken(string id)
        : base(id)
    {
    }

    /// <summary>
    /// List 内边距
    /// </summary>
    public Thickness ContentPadding { get; set; }

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
    /// 列表项小号内间距
    /// </summary>
    public Thickness ItemPaddingSM { get; set; }

    /// <summary>
    /// 列表项内间距
    /// </summary>
    public Thickness ItemPadding { get; set; }

    /// <summary>
    /// 列表项大号内间距
    /// </summary>
    public Thickness ItemPaddingLG { get; set; }

    /// <summary>
    /// 列表项外边距
    /// </summary>
    public Thickness ItemMargin { get; set; }

    /// <summary>
    /// 分页器的外边距
    /// </summary>
    public Thickness PaginationMargin { get; set; }
    
    /// <summary>
    /// 分组标题的颜色
    /// </summary>
    public Color GroupHeaderColor { get; set; }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var colorTextDisabled  = SharedToken.ColorTextDisabled;
        var colorTextSecondary = SharedToken.ColorTextSecondary;
        var colorBgTextHover   = SharedToken.ColorBgTextHover;

        ItemColor         = colorTextSecondary;
        ItemHoverColor    = colorTextSecondary;
        ItemSelectedColor = SharedToken.ColorText;

        ItemBgColor         = SharedToken.ColorTransparent;
        ItemHoverBgColor    = colorBgTextHover;
        ItemSelectedBgColor = SharedToken.ControlItemBgActive;

        ItemDisabledColor = colorTextDisabled;

        ItemPaddingLG = new Thickness(SharedToken.UniformlyPadding, 0);
        ItemPaddingSM = new Thickness(SharedToken.UniformlyPaddingXS, 0);
        ItemPadding   = new Thickness(SharedToken.UniformlyPaddingSM, 0);

        ContentPadding   = new Thickness(SharedToken.UniformlyPaddingXXS / 2);
        ItemMargin       = new Thickness(0, 0.5);
        PaginationMargin = new Thickness(0, SharedToken.UniformlyMargin);
        GroupHeaderColor = SharedToken.ColorTextDescription;
    }
}