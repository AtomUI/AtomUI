using AtomUI.Theme.TokenSystem;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public class MessageBoxToken : AbstractControlDesignToken
{
    public const string ID = "MessageBox";
    
    /// <summary>
    /// Style Icon 的大小
    /// </summary>
    public double StyleIconSize { get; set; }
    
    public MessageBoxToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        StyleIconSize = SharedToken.SizeLG * 1.2;
    }
}