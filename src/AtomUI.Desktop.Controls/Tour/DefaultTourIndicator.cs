using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// 默认步骤指示器：圆点由代码按 StepCount 物化为 Ellipse 并只挂 marker 与激活状态类，
/// 视觉（尺寸/颜色/间距）全部由 DefaultTourIndicatorTheme 的选择器与绑定表达，
/// 语义 popup.indicator 专用 Style 因此可跨根命中每个圆点。
/// </summary>
public class DefaultTourIndicator : TourIndicator
{
    #region 公共属性定义

    public static readonly StyledProperty<double> IndicatorSizeProperty = 
        AvaloniaProperty.Register<DefaultTourIndicator, double>(nameof(IndicatorSize));
    
    public static readonly StyledProperty<IBrush?> IndicatorColorProperty = 
        AvaloniaProperty.Register<DefaultTourIndicator, IBrush?>(nameof(IndicatorColor));
    
    public static readonly StyledProperty<IBrush?> IndicatorActiveColorProperty = 
        AvaloniaProperty.Register<DefaultTourIndicator, IBrush?>(nameof(IndicatorActiveColor));
    
    public static readonly StyledProperty<double> ItemSpacingProperty = 
        AvaloniaProperty.Register<DefaultTourIndicator, double>(nameof(ItemSpacing));

    public double IndicatorSize
    {
        get => GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }
        
    public IBrush? IndicatorColor
    {
        get => GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }
    
    public IBrush? IndicatorActiveColor
    {
        get => GetValue(IndicatorActiveColorProperty);
        set => SetValue(IndicatorActiveColorProperty, value);
    }
    
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }
    #endregion

    private Panel? _dotsLayout;

    static DefaultTourIndicator()
    {
        AffectsMeasure<DefaultTourIndicator>(ItemSpacingProperty, IndicatorSizeProperty, StepCountProperty);
    }

    /// <summary>
    /// 保持与旧自绘版一致的布局契约：N*size + (N+1)*spacing
    /// （左右各留一份间距，圆点组在 Footer 中垂直居中）。
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var height = IndicatorSize;
        var width  = StepCount * IndicatorSize + ItemSpacing * (StepCount + 1);
        return new Size(width, height);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _dotsLayout = e.NameScope.Find<Panel>("DotsLayout");
        SyncDots();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StepCountProperty || change.Property == ActiveIndexProperty)
        {
            SyncDots();
        }
    }

    private void SyncDots()
    {
        if (_dotsLayout is null)
        {
            return;
        }

        while (_dotsLayout.Children.Count > StepCount)
        {
            _dotsLayout.Children.RemoveAt(_dotsLayout.Children.Count - 1);
        }

        while (_dotsLayout.Children.Count < StepCount)
        {
            var dot = new Ellipse();
            dot.Classes.Add(TourSemanticParts.PopupIndicatorClass);
            _dotsLayout.Children.Add(dot);
        }

        for (var i = 0; i < _dotsLayout.Children.Count; i++)
        {
            var dot = (Ellipse)_dotsLayout.Children[i];
            var isActive = i == ActiveIndex;
            if (isActive && !dot.Classes.Contains(ActiveClass))
            {
                dot.Classes.Add(ActiveClass);
            }
            else if (!isActive && dot.Classes.Contains(ActiveClass))
            {
                dot.Classes.Remove(ActiveClass);
            }
        }
    }

    private const string ActiveClass = "active";
}
