using AtomUI.Icons.AntDesign;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls.Themes;

internal class RevealButton : ToggleIconButton
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (CheckedIcon == null)
        {
            SetCurrentValue(CheckedIconProperty, new EyeOutlined());
        }

        if (UnCheckedIcon == null)
        {
            SetCurrentValue(UnCheckedIconProperty, new EyeInvisibleOutlined());
        }
    }
}