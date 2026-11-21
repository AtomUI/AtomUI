using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

internal class NumericUpDownInputPanel : Decorator
{
    #region 内部属性定义

    internal static readonly StyledProperty<double> LeftInsetProperty =
        AvaloniaProperty.Register<NumericUpDownInputPanel, double>(nameof(LeftInset));

    internal static readonly StyledProperty<double> RightInsetProperty =
        AvaloniaProperty.Register<NumericUpDownInputPanel, double>(nameof(RightInset));

    internal double LeftInset
    {
        get => GetValue(LeftInsetProperty);
        set => SetValue(LeftInsetProperty, value);
    }

    internal double RightInset
    {
        get => GetValue(RightInsetProperty);
        set => SetValue(RightInsetProperty, value);
    }

    #endregion

    static NumericUpDownInputPanel()
    {
        AffectsArrange<NumericUpDownInputPanel>(LeftInsetProperty, RightInsetProperty);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Arrange the dock panel within the floating handle's animated boundaries.
        var left  = Math.Clamp(LeftInset, 0d, finalSize.Width);
        var right = Math.Clamp(finalSize.Width - RightInset, left, finalSize.Width);
        if (UseLayoutRounding)
        {
            var scale = LayoutHelper.GetLayoutScale(this);
            left      = LayoutHelper.RoundLayoutValue(left, scale);
            right     = LayoutHelper.RoundLayoutValue(right, scale);
        }
        Child?.Arrange(new Rect(left, 0, right - left, finalSize.Height));
        return finalSize;
    }
}
