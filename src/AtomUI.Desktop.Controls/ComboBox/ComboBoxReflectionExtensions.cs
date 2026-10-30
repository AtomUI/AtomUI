using System.Reflection;
using AtomUI.Reflection;

using AvaloniaComboBox = Avalonia.Controls.ComboBox;
using AvaloniaTextBox = Avalonia.Controls.TextBox;

namespace AtomUI.Desktop.Controls;

internal static class ComboBoxReflectionExtensions
{
    #region 反射信息定义
    private static readonly Lazy<FieldInfo> PopupFieldInfo = new(() =>
        FixedMemberReflection.RequireField(
            typeof(AvaloniaComboBox).GetField("_popup", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaComboBox),
            "_popup"));

    private static readonly Lazy<FieldInfo> InputTextBoxFieldInfo = new(() =>
        FixedMemberReflection.RequireField(
            typeof(AvaloniaComboBox).GetField("_inputTextBox", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaComboBox),
            "_inputTextBox"));
    #endregion
    
    public static void SetPopup(this AvaloniaComboBox comboBox, Popup? popup)
    {
        PopupFieldInfo.Value.SetValue(comboBox, popup);
    }

    public static void SetInputTextBox(this AvaloniaComboBox comboBox, AvaloniaTextBox? textBox)
    {
        InputTextBoxFieldInfo.Value.SetValue(comboBox, textBox);
    }
}
