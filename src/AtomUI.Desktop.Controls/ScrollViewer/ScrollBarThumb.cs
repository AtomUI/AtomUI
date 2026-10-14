using AtomUI.Controls.Commons;
using AtomUI.Registration;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[BootstrapRegistration]
public class ScrollBarThumb : AbstractScrollBarThumb
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsLiteModeProperty =
        ScrollViewer.IsLiteModeProperty.AddOwner<ScrollBarThumb>();

    public bool IsLiteMode
    {
        get => GetValue(IsLiteModeProperty);
        set => SetValue(IsLiteModeProperty, value);
    }
    #endregion
}