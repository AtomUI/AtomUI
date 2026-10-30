using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public class SkeletonLine : AbstractSkeleton
{
    #region 公共属性定义

    public static readonly StyledProperty<Dimension> LineWidthProperty =
        AvaloniaProperty.Register<SkeletonLine, Dimension>(nameof(LineWidth), new Dimension(100.0, DimensionUnitType.Percentage));
    
    public static readonly StyledProperty<bool> IsRoundProperty =
        AvaloniaProperty.Register<SkeletonLine, bool>(nameof(IsRound));
    
    public Dimension LineWidth
    {
        get => GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }
    
    public bool IsRound
    {
        get => GetValue(IsRoundProperty);
        set => SetValue(IsRoundProperty, value);
    }

    #endregion

    private Panel? _rootLayout;

    static SkeletonLine()
    {
        AffectsMeasure<SkeletonLine>(LineWidthProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var actualWidth = availableSize.Width;
        if (LineWidth.IsAbsolute)
        {
            actualWidth = LineWidth.Value;
        }
        var actualSize = new Size(actualWidth, availableSize.Height);
        return base.MeasureOverride(actualSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size        = base.ArrangeOverride(finalSize);
        var actualWidth = Math.Min(finalSize.Width, LineWidth.Resolve(finalSize.Width));
        
        if (_rootLayout != null)
        {
            _rootLayout.Arrange(new Rect(0, 0, actualWidth, finalSize.Height));
        }
        return size;
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _rootLayout = e.NameScope.Find<Panel>("PART_RootLayout");
    }
}
