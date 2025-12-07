using AtomUI.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Utilities;

namespace AtomUI.Desktop.Controls.Primitives;

public class DashedBorder : Decorator
{
    #region 公共属性定义
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
            AvaloniaProperty.Register<DashedBorder, IBrush?>(nameof(Background));
    
    public static readonly StyledProperty<BackgroundSizing> BackgroundSizingProperty =
        AvaloniaProperty.Register<DashedBorder, BackgroundSizing>(
            nameof(BackgroundSizing),
            BackgroundSizing.CenterBorder);
    
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<DashedBorder, IBrush?>(nameof(BorderBrush));
        
    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<DashedBorder, Thickness>(nameof(BorderThickness));
        
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<DashedBorder, CornerRadius>(nameof(CornerRadius));
        
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        AvaloniaProperty.Register<DashedBorder, BoxShadows>(nameof(BoxShadow));
    
    public static readonly StyledProperty<IList<double>?> StrokeDashArrayProperty =
        AvaloniaProperty.Register<DashedBorder, IList<double>?>(nameof(StrokeDashArray));
    
    public static readonly StyledProperty<double> StrokeDaskOffsetProperty =
        AvaloniaProperty.Register<DashedBorder, double>(nameof(StrokeDaskOffset), 0.0);
    
    /// <summary>
    /// Gets or sets a brush with which to paint the background.
    /// </summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets how the background is drawn relative to the border.
    /// </summary>
    public BackgroundSizing BackgroundSizing
    {
        get => GetValue(BackgroundSizingProperty);
        set => SetValue(BackgroundSizingProperty, value);
    }

    /// <summary>
    /// Gets or sets a brush with which to paint the border.
    /// </summary>
    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of the border.
    /// </summary>
    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the radius of the border rounded corners.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the box shadow effect parameters
    /// </summary>
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    public IList<double>? StrokeDashArray
    {
        get => GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }

    public double StrokeDaskOffset
    {
        get => GetValue(StrokeDaskOffsetProperty);
        set => SetValue(StrokeDaskOffsetProperty, value);
    }
    
    #endregion
    
    private readonly BorderRenderHelper _borderRenderHelper = new BorderRenderHelper();
    private Thickness? _layoutThickness;
    private double _scale;
        
    static DashedBorder()
    {
        AffectsRender<DashedBorder>(
            BackgroundProperty,
            BackgroundSizingProperty,
            BorderBrushProperty,
            CornerRadiusProperty,
            BoxShadowProperty,
            StrokeDashArrayProperty,
            StrokeDaskOffsetProperty);
        AffectsMeasure<DashedBorder>(BorderThicknessProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        switch (change.Property.Name)
        {
            case nameof(UseLayoutRounding):
            case nameof(BorderThickness):
                _layoutThickness = null;
                break;
        }
    }

    private Thickness LayoutThickness
    {
        get
        {
            VerifyScale();
            if (_layoutThickness == null)
            {
                var borderThickness = BorderThickness;

                if (UseLayoutRounding)
                {
                    borderThickness = LayoutHelper.RoundLayoutThickness(borderThickness, _scale, _scale);
                }
                _layoutThickness = borderThickness;
            }
            return _layoutThickness.Value;
        }
    }

    private void VerifyScale()
    {
        var currentScale = LayoutHelper.GetLayoutScale(this);
        if (MathUtilities.AreClose(currentScale, _scale))
        {
            return;
        }

        _scale           = currentScale;
        _layoutThickness = null;
    }

    /// <summary>
    /// Renders the control.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    public sealed override void Render(DrawingContext context)
    {
        _borderRenderHelper.Render(
            context,
            Bounds.Size,
            BorderThickness,
            CornerRadius,
            BackgroundSizing,
            Background,
            BorderBrush,
            StrokeDashArray,
            StrokeDaskOffset,
            BoxShadow);
    }

    /// <summary>
    /// Measures the control.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        return LayoutHelper.MeasureChild(Child, availableSize, Padding, BorderThickness);
    }

    /// <summary>
    /// Arranges the control's child.
    /// </summary>
    /// <param name="finalSize">The size allocated to the control.</param>
    /// <returns>The space taken.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        return LayoutHelper.ArrangeChild(Child, finalSize, Padding, BorderThickness);
    }
}