using AtomUI.Controls;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public class InlineNavMenuItemHeader : BaseNavMenuItemHeader
{
    #region 公共属性定义
    public static readonly DirectProperty<InlineNavMenuItemHeader, int> LevelProperty =
        AvaloniaProperty.RegisterDirect<InlineNavMenuItemHeader, int>(
            nameof(Level), 
            o => o.Level,
            (o, v) => o.Level = v);
    
    public static readonly DirectProperty<InlineNavMenuItemHeader, double> InlineItemIndentUnitProperty =
        AvaloniaProperty.RegisterDirect<InlineNavMenuItemHeader, double>(nameof(InlineItemIndentUnit),
            o => o.InlineItemIndentUnit,
            (o, v) => o.InlineItemIndentUnit = v);
    
    public static readonly StyledProperty<ITransform?> MenuIndicatorRenderTransformProperty =
        AvaloniaProperty.Register<InlineNavMenuItemHeader, ITransform?>(nameof(MenuIndicatorRenderTransform));
    
    private int _level;
    
    public int Level
    {
        get => _level;
        set => SetAndRaise(LevelProperty, ref _level, value);
    }
    
    private double _inlineItemIndentUnit;

    public double InlineItemIndentUnit
    {
        get => _inlineItemIndentUnit;
        set => SetAndRaise(InlineItemIndentUnitProperty, ref _inlineItemIndentUnit, value);
    }
    
    public ITransform? MenuIndicatorRenderTransform
    {
        get => GetValue(MenuIndicatorRenderTransformProperty);
        set => SetValue(MenuIndicatorRenderTransformProperty, value);
    }
    #endregion

    protected override void NotifyConfigureTransitions(Transitions transitions)
    {
        base.NotifyConfigureTransitions(transitions);
        transitions.Add(TransitionUtils.CreateTransition<TransformOperationsTransition>(MenuIndicatorRenderTransformProperty));
    }
}