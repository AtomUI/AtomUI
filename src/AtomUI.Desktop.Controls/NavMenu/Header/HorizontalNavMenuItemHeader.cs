using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public class HorizontalNavMenuItemHeader : BaseNavMenuItemHeader
{
    #region 公共属性定义
    
    public static readonly StyledProperty<double> ActiveBarScaleXProperty =
        AvaloniaProperty.Register<HorizontalNavMenuItemHeader, double>(nameof(ActiveBarScaleX), 0.5d,
            coerce: (o, v) => Math.Max(Math.Min(v, 1.0), 0.0));

    public static readonly StyledProperty<double> ActiveBarHeightProperty =
        AvaloniaProperty.Register<HorizontalNavMenuItemHeader, double>(nameof(ActiveBarHeight));
    
    public static readonly StyledProperty<IBrush?> ActiveBarColorProperty =
        AvaloniaProperty.Register<HorizontalNavMenuItemHeader, IBrush?>(nameof(ActiveBarColor));
    
    public static readonly DirectProperty<HorizontalNavMenuItemHeader, bool> IsTopLevelProperty =
        AvaloniaProperty.RegisterDirect<HorizontalNavMenuItemHeader, bool>(
            nameof(IsTopLevel), o => o.IsTopLevel,
            (o, v) => o.IsTopLevel = v);
    
    public double ActiveBarScaleX
    {
        get => GetValue(ActiveBarScaleXProperty);
        set => SetValue(ActiveBarScaleXProperty, value);
    }

    public double ActiveBarHeight
    {
        get => GetValue(ActiveBarHeightProperty);
        set => SetValue(ActiveBarHeightProperty, value);
    }
    
    private bool _isTopLevel;
    public bool IsTopLevel
    {
        get => _isTopLevel;
        set => SetAndRaise(IsTopLevelProperty, ref _isTopLevel, value);
    }
    
    public IBrush? ActiveBarColor
    {
        get => GetValue(ActiveBarColorProperty);
        set => SetValue(ActiveBarColorProperty, value);
    }

    #endregion

    private Rectangle? _activeIndicator;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ActiveBarScaleXProperty)
        {
            ConfigureActiveIndicator();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _activeIndicator =  e.NameScope.Find<Rectangle>("PART_ActiveIndicator");
        ConfigureActiveIndicator();
    }

    private void ConfigureActiveIndicator()
    {
        if (_activeIndicator != null)
        {
            if (_activeIndicator.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.ScaleX = ActiveBarScaleX;
                scaleTransform.ScaleY = 1.0;
            }
            else
            {
                _activeIndicator.RenderTransform = new ScaleTransform(ActiveBarScaleX, 1.0);
            }

            if (_activeIndicator.RenderTransformOrigin != RelativePoint.Center)
            {
                _activeIndicator.RenderTransformOrigin = RelativePoint.Center;
            }
        }
    }

}
