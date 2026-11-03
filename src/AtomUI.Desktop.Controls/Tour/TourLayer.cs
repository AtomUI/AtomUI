using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class TourLayer : Control, IMotionAwareControl
{
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

    #region 运行时字段

    private const int TourLayerZIndex = int.MaxValue - 98;

    private readonly RectangleGeometry _layerGeometry = new();

    private readonly RectangleGeometry _targetRegionGeometry = new();

    private CombinedGeometry? _combinedGeometry;

    private Rect _combinedLayerRect;

    private Rect _combinedTargetRegion;

    private double _combinedTargetRegionCornerRadius = double.NaN;

    #endregion

    #region 构造与初始化

    static TourLayer()
    {
        AffectsRender<TourLayer>(BackgroundProperty, TargetRegionProperty, TargetRegionCornerRadiusProperty);
    }

    #endregion

    #region 公共与受保护 API

    public sealed override void Render(DrawingContext context)
    {
        context.DrawGeometry(Background, null, GetCombinedGeometry());
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        e.Handled = true;
    }

    public static TourLayer? GetTourLayer(Visual visual)
    {
        VisualLayerManager? manager;
        if (visual is TopLevel topLevel)
        {
            manager = FindFirstTemplateLayerManager(topLevel);
        }
        else
        {
            manager = visual.FindAncestorOfType<VisualLayerManager>();
        }

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

            manager.AddLayer(tourLayer, TourLayerZIndex);
        }
        return tourLayer;
    }

    #endregion

    #region 私有实现

    private Geometry GetCombinedGeometry()
    {
        var layerRect                = new Rect(Bounds.Size);
        var targetRegion            = TargetRegion;
        var targetRegionCornerRadius = TargetRegionCornerRadius;

        if (_combinedGeometry is not null &&
            _combinedLayerRect == layerRect &&
            _combinedTargetRegion == targetRegion &&
            _combinedTargetRegionCornerRadius == targetRegionCornerRadius)
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

    private static VisualLayerManager? FindFirstTemplateLayerManager(TopLevel topLevel)
    {
        foreach (var descendant in topLevel.GetTemplateDescendants())
        {
            if (descendant is VisualLayerManager manager)
            {
                return manager;
            }
        }

        return null;
    }

    #endregion

}
