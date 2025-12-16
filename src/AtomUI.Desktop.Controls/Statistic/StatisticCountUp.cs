using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class StatisticCountUp : TemplatedControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<double> EndValueProperty =
        AvaloniaProperty.Register<StatisticCountUp, double>(nameof(EndValue));
    
    public static readonly StyledProperty<string> DecimalSeparatorProperty =
        AvaloniaProperty.Register<StatisticCountUp, string>(nameof(DecimalSeparator), ".");
    
    public static readonly StyledProperty<string> GroupSeparatorProperty =
        AvaloniaProperty.Register<StatisticCountUp, string>(nameof(GroupSeparator), ",");
    
    public static readonly StyledProperty<int> PrecisionProperty =
        AvaloniaProperty.Register<StatisticCountUp, int>(nameof(Precision), 0);
    
    public double EndValue
    {
        get => GetValue(EndValueProperty);
        set => SetValue(EndValueProperty, value);
    }

    public string DecimalSeparator
    {
        get => GetValue(DecimalSeparatorProperty);
        set => SetValue(DecimalSeparatorProperty, value);
    }
    
    public string GroupSeparator
    {
        get => GetValue(GroupSeparatorProperty);
        set => SetValue(GroupSeparatorProperty, value);
    }
    
    public int Precision
    {
        get => GetValue(PrecisionProperty);
        set => SetValue(PrecisionProperty, value);
    }

    #endregion

    #region 内部属性定义
    
    public static readonly StyledProperty<double> AnimatingValueProperty =
        AvaloniaProperty.Register<StatisticCountUp, double>(nameof(AnimatingValue), 0);
    
    internal static readonly DirectProperty<StatisticCountUp, string?> FormattedValueProperty =
        AvaloniaProperty.RegisterDirect<StatisticCountUp, string?>(nameof(FormattedValue),
            o => o.FormattedValue,
            (o, v) => o.FormattedValue = v);
    
    public double AnimatingValue
    {
        get => GetValue(AnimatingValueProperty);
        set => SetValue(AnimatingValueProperty, value);
    }
    
    private string? _formattedValue;

    internal string? FormattedValue
    {
        get => _formattedValue;
        set => SetAndRaise(FormattedValueProperty, ref _formattedValue, value);
    }

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => StatisticToken.ID;

    #endregion

    static StatisticCountUp()
    {
        AffectsMeasure<StatisticCountUp>(FormattedValueProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == EndValueProperty)
            {
                SetCurrentValue(AnimatingValueProperty, EndValue);
            }
        }
        
        if (change.Property == AnimatingValueProperty ||
            change.Property == GroupSeparatorProperty ||
            change.Property == PrecisionProperty ||
            change.Property == DecimalSeparatorProperty)
        {
            var effectiveValue = StatisticUtils.FormatNumber(AnimatingValue, GroupSeparator, DecimalSeparator, Precision);
            SetCurrentValue(FormattedValueProperty, effectiveValue);
        }
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (force || Transitions == null)
        {
            Transitions =
            [
                TransitionUtils.CreateTransition<DoubleTransition>(AnimatingValueProperty, SharedTokenKey.MotionDurationVerySlow, new ExponentialEaseOut())
            ];
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
        AnimatingValue = EndValue;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }
}