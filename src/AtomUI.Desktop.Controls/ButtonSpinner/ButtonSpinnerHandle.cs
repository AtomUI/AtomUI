using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class ButtonSpinnerHandle : TemplatedControl
{
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ButtonSpinnerHandle>();
    
    public static readonly StyledProperty<ButtonSpinnerLocation> ButtonSpinnerLocationProperty =
        ButtonSpinner.ButtonSpinnerLocationProperty.AddOwner<ButtonSpinnerHandle>();

    public static readonly DirectProperty<ButtonSpinnerHandle, Thickness> SpinnerBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<ButtonSpinnerHandle, Thickness>(nameof(SpinnerBorderThickness),
            o => o.SpinnerBorderThickness,
            (o, v) => o.SpinnerBorderThickness = v);
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public ButtonSpinnerLocation ButtonSpinnerLocation
    {
        get => GetValue(ButtonSpinnerLocationProperty);
        set => SetValue(ButtonSpinnerLocationProperty, value);
    }

    private Thickness _spinnerBorderThickness;

    internal Thickness SpinnerBorderThickness
    {
        get => _spinnerBorderThickness;
        set => SetAndRaise(SpinnerBorderThicknessProperty, ref _spinnerBorderThickness, value);
    }
    
    public IconButton? IncreaseButton { get; private set; }
    public IconButton? DecreaseButton { get; private set; }
    
    public event EventHandler? ButtonsCreated;
    
    private BorderRenderHelper _borderRenderHelper;

    static ButtonSpinnerHandle()
    {
        AffectsRender<ButtonSpinnerHandle>(ButtonSpinnerLocationProperty, CornerRadiusProperty, BackgroundProperty, BorderBrushProperty);
    }
    
    public ButtonSpinnerHandle()
    {
        _borderRenderHelper = new BorderRenderHelper();
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        IncreaseButton = e.NameScope.Find<IconButton>(ButtonSpinnerThemeConstants.IncreaseButtonPart);
        DecreaseButton = e.NameScope.Find<IconButton>(ButtonSpinnerThemeConstants.DecreaseButtonPart);
        ButtonsCreated?.Invoke(this, EventArgs.Empty);
    }
    
    public override void Render(DrawingContext context)
    {
        var          lineWidth = SpinnerBorderThickness.Left;
        CornerRadius cornerRadius;
        if (ButtonSpinnerLocation == ButtonSpinnerLocation.Left)
        {
            cornerRadius = new CornerRadius(CornerRadius.TopLeft,
                0,
                0,
                CornerRadius.BottomLeft);
        }
        else
        {
            cornerRadius = new CornerRadius(0,
                CornerRadius.TopRight,
                CornerRadius.BottomRight,
                0);
        }
     
        {
            using var optionState = context.PushTransform(Matrix.CreateTranslation(lineWidth, lineWidth));
            _borderRenderHelper.Render(context, Bounds.Size.Deflate(new Thickness(lineWidth)),
                new Thickness(0), 
                cornerRadius, BackgroundSizing.OuterBorderEdge,
                Background, null);
        }
        
        {
            var handleOffsetY = Bounds.Height / 2;
            using var optionState = context.PushRenderOptions(new RenderOptions
            {
                EdgeMode = EdgeMode.Antialias
            });
            
            {
                // 画竖线
                var startPoint = new Point(lineWidth, lineWidth);
                var endPoint   = new Point(lineWidth, Bounds.Height - lineWidth);
                context.DrawLine(new Pen(BorderBrush, lineWidth), startPoint, endPoint);
            }
            {
                // 画横线
                var startPoint = new Point(lineWidth, handleOffsetY); 
                var endPoint   = new Point(Bounds.Width - lineWidth, handleOffsetY);
                context.DrawLine(new Pen(BorderBrush, lineWidth), startPoint, endPoint);
            }
        }
    }
}
