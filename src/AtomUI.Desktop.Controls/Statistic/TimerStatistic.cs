using Avalonia;

namespace AtomUI.Desktop.Controls;

public class TimerStatistic : AbstractStatistic
{
    #region 公共属性定义

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<AbstractStatistic, double>(nameof(Value));
    
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    #endregion
}