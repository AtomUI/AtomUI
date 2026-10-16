using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public class ScrollViewer : AbstractScrollViewer
{
    #region 公共属性定义

    public static readonly AttachedProperty<bool> IsLiteModeProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, Control, bool>(
            nameof(IsLiteMode));

    public bool IsLiteMode
    {
        get => GetValue(IsLiteModeProperty);
        set => SetValue(IsLiteModeProperty, value);
    }
    
    #endregion
    
    public ScrollViewer()
    {
    }
    
    public static bool GetIsLiteMode(Control control)
    {
        return control.GetValue(IsLiteModeProperty);
    }
    
    public static void SetIsLiteMode(Control control, bool value)
    {
        control.SetValue(IsLiteModeProperty, value);
    }
}