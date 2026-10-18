using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

internal class NumericUpDownInputPanel : DockPanel
{
    #region 内部属性定义

    internal static readonly StyledProperty<double> RightInsetProperty =
        AvaloniaProperty.Register<NumericUpDownInputPanel, double>(nameof(RightInset));

    internal double RightInset
    {
        get => GetValue(RightInsetProperty);
        set => SetValue(RightInsetProperty, value);
    }

    #endregion

    static NumericUpDownInputPanel()
    {
        AffectsArrange<NumericUpDownInputPanel>(RightInsetProperty);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Reserve the floating handle's current animated inset before docking the suffix.
        // Both the editor's hit area and the suffix then follow the same layout boundary.
        var inset = Math.Clamp(RightInset, 0d, finalSize.Width);
        var contentWidth = finalSize.Width - inset;
        if (UseLayoutRounding)
        {
            // Round the shared boundary once, before DockPanel rounds each child's slot.
            // Otherwise a fractional animation frame can round the editor and suffix apart.
            contentWidth = LayoutHelper.RoundLayoutValue(contentWidth, LayoutHelper.GetLayoutScale(this));
        }
        base.ArrangeOverride(new Size(contentWidth, finalSize.Height));
        return finalSize;
    }
}
