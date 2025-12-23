using AtomUI.Theme.TokenSystem;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ArrowDecoratedBoxToken : AbstractControlDesignToken
{
    public const string ID = "ArrowDecoratedBox";

    /// <summary>
    /// 箭头三角形大小
    /// </summary>
    public double ArrowSize { get; set; }

    /// <summary>
    /// 默认的内边距
    /// </summary>
    public Thickness Padding { get; set; }

    public ArrowDecoratedBoxToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ArrowSize = SharedToken.SizePopupArrow / 1.3;
        Padding   = SharedToken.PaddingXS;
    }
}