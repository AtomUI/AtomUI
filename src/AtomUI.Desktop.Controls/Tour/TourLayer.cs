using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class TourLayer : Control, IMotionAwareControl
{
    private readonly RectangleGeometry _layerGeometry = new();
    private readonly RectangleGeometry _targetRegionGeometry = new();
    private CombinedGeometry? _combinedGeometry;
    private Rect _combinedLayerRect;
    private Rect _combinedTargetRegion;
    private double _combinedTargetRegionCornerRadius = double.NaN;
    
    #region 公共属性定义
    public static readonly StyledProperty<Rect> TargetRegionProperty = 
        AvaloniaProperty.Register<TourLayer, Rect>(nameof(TargetRegion));
    
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<TourLayer, IBrush?>(nameof(Background));
    
    public static readonly StyledProperty<double> TargetRegionCornerRadiusProperty =
        AvaloniaProperty.Register<TourLayer, double>(nameof(TargetRegionCornerRadius));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TourLayer>();
    
    public Rect TargetRegion
    {
        get => GetValue(TargetRegionProperty);
        set => SetValue(TargetRegionProperty, value);
    }
    
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }
    
    public double TargetRegionCornerRadius
    {
        get => GetValue(TargetRegionCornerRadiusProperty);
        set => SetValue(TargetRegionCornerRadiusProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    #endregion

    static TourLayer()
    {
        AffectsRender<TourLayer>(BackgroundProperty, TargetRegionProperty, TargetRegionCornerRadiusProperty);
    }

    public sealed override void Render(DrawingContext context)
    {
        context.DrawGeometry(Background, null, GetCombinedGeometry());
    }

    private Geometry GetCombinedGeometry()
    {
        var layerRect                = new Rect(Bounds.Size);
        var targetRegion            = TargetRegion;
        var targetRegionCornerRadius = TargetRegionCornerRadius;

        if (_combinedGeometry is not null &&
            _combinedLayerRect == layerRect &&
            _combinedTargetRegion == targetRegion &&
            MathUtils.AreClose(_combinedTargetRegionCornerRadius, targetRegionCornerRadius))
        {
            return _combinedGeometry;
        }

        _combinedLayerRect                = layerRect;
        _combinedTargetRegion            = targetRegion;
        _combinedTargetRegionCornerRadius = targetRegionCornerRadius;
        _layerGeometry.Rect               = layerRect;
        _targetRegionGeometry.Rect        = targetRegion;
        _targetRegionGeometry.RadiusX     = targetRegionCornerRadius;
        _targetRegionGeometry.RadiusY     = targetRegionCornerRadius;
        _combinedGeometry                 = new CombinedGeometry(GeometryCombineMode.Exclude,
            _layerGeometry,
            _targetRegionGeometry);
        return _combinedGeometry;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        e.Handled = true;
    }
    
    public static TourLayer? GetTourLayer(Visual visual)
    {
        var manager = LayerLadder.FindTopLevelLayerManager(visual);
        if (manager == null)
        {
            return null;
        }

        var tourLayer = VisualLayerManagerUtils.FindLayer<TourLayer>(manager);
        if (tourLayer == null)
        {
            tourLayer = new TourLayer()
            {
                IsVisible = false
            };

            LayerLadder.AddLayer(manager, tourLayer, LayerLadder.Tour);
        }
        return tourLayer;
    }
}
