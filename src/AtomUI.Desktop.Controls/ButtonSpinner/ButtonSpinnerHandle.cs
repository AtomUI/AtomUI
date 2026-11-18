using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Media;
using AtomUI.Utils;
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

    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<ButtonSpinnerHandle>();

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

    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    private Thickness _spinnerBorderThickness;

    internal Thickness SpinnerBorderThickness
    {
        get => _spinnerBorderThickness;
        set => SetAndRaise(SpinnerBorderThicknessProperty, ref _spinnerBorderThickness, value);
    }

    public static readonly DirectProperty<ButtonSpinnerHandle, Thickness> HandleContentMarginProperty =
        AvaloniaProperty.RegisterDirect<ButtonSpinnerHandle, Thickness>(nameof(HandleContentMargin),
            o => o.HandleContentMargin,
            (o, v) => o.HandleContentMargin = v);

    private Thickness _handleContentMargin;

    /// <summary>
    /// The inset of the two half-height buttons: the frame's border width on the outer edges, so their hover/pressed
    /// fill stays inside the input frame's outline (PrimeNG insets its stacked button group by 1px the same way).
    /// </summary>
    internal Thickness HandleContentMargin
    {
        get => _handleContentMargin;
        private set => SetAndRaise(HandleContentMarginProperty, ref _handleContentMargin, value);
    }
    
    public IconButton? IncreaseButton { get; private set; }
    public IconButton? DecreaseButton { get; private set; }
    
    public event EventHandler? ButtonsCreated;
    
    private BorderRenderHelper _borderRenderHelper;
    private IPen? _handlePen;

    static ButtonSpinnerHandle()
    {
        AffectsRender<ButtonSpinnerHandle>(
            ButtonSpinnerLocationProperty,
            CornerRadiusProperty,
            BackgroundProperty,
            BorderBrushProperty,
            SpinnerBorderThicknessProperty,
            UseLayoutRoundingProperty);
    }
    
    public ButtonSpinnerHandle()
    {
        _borderRenderHelper = new BorderRenderHelper();
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        IncreaseButton = e.NameScope.Find<IconButton>("PART_IncreaseButton");
        DecreaseButton = e.NameScope.Find<IconButton>("PART_DecreaseButton");
        ConfigureHandleButtonRadii();
        ConfigureHandleContentMargin();
        ButtonsCreated?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CornerRadiusProperty ||
            change.Property == ButtonSpinnerLocationProperty ||
            change.Property == SpinnerBorderThicknessProperty ||
            change.Property == BoundsProperty)
        {
            ConfigureHandleButtonRadii();
            ConfigureHandleContentMargin();
        }
    }

    // The outer edges inset by the frame's border width; the inner edge (the side facing the text) stays flush so the
    // buttons keep meeting the divider.
    private void ConfigureHandleContentMargin()
    {
        var border = SpinnerBorderThickness;
        HandleContentMargin = ButtonSpinnerLocation == ButtonSpinnerLocation.Left
            ? new Thickness(border.Left, border.Top, 0, border.Bottom)
            : new Thickness(0, border.Top, border.Right, border.Bottom);
    }

    // The pressed/hover background of each half-height button draws with the IconButton's own CornerRadius, which
    // falls back to the (theme-wide, e.g. 26) global BorderRadius. Oversized for a half-height cell, Avalonia draws
    // it asymmetrically and rounds the middle divider edge; split the handle radius per side and clamp it to the
    // half cell (CSS border-radius semantics) so the outer corners follow the frame and the divider edge stays flat.
    private void ConfigureHandleButtonRadii()
    {
        if (IncreaseButton is null || DecreaseButton is null)
        {
            return;
        }
        var isRight = ButtonSpinnerLocation != ButtonSpinnerLocation.Left;
        var cellHeight = Math.Max(0d, Bounds.Height / 2 - 1); // UniformGrid RowSpacing of 1
        var cellWidth = Bounds.Width;
        CornerRadius increase;
        CornerRadius decrease;
        if (isRight)
        {
            increase = new CornerRadius(0, CornerRadius.TopRight, 0, 0);
            decrease = new CornerRadius(0, 0, CornerRadius.BottomRight, 0);
        }
        else
        {
            increase = new CornerRadius(CornerRadius.TopLeft, 0, 0, 0);
            decrease = new CornerRadius(0, 0, 0, CornerRadius.BottomLeft);
        }
        // Template parts keep their LocalValue free; the split radius is a runtime composition (ATOMUIGEN047).
        IncreaseButton.SetCurrentValue(CornerRadiusProperty,
            AtomUI.Media.CornerRadiusUtils.ClampToSize(increase, cellWidth, cellHeight));
        DecreaseButton.SetCurrentValue(CornerRadiusProperty,
            AtomUI.Media.CornerRadiusUtils.ClampToSize(decrease, cellWidth, cellHeight));
    }
    
    public override void Render(DrawingContext context)
    {
        var          spinnerBorderThickness = BorderUtils.BuildRenderScaleAwareThickness(this, SpinnerBorderThickness);
        var          lineWidth = spinnerBorderThickness.Left;
        var          lineCenterOffset = lineWidth / 2;
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
                var startPoint = new Point(lineCenterOffset, lineWidth);
                var endPoint   = new Point(lineCenterOffset, Bounds.Height - lineWidth);
                PenUtils.TryModifyOrCreate(ref _handlePen, BorderBrush, lineWidth);
                if (_handlePen is not null)
                {
                    context.DrawLine(_handlePen, startPoint, endPoint);
                }
            }
            {
                // 画横线
                var startPoint = new Point(lineWidth, handleOffsetY); 
                var endPoint   = new Point(Bounds.Width - lineWidth, handleOffsetY);
                PenUtils.TryModifyOrCreate(ref _handlePen, BorderBrush, lineWidth);
                if (_handlePen is not null)
                {
                    context.DrawLine(_handlePen, startPoint, endPoint);
                }
            }
        }
    }
}
