using System.Diagnostics;
using AtomUI.Animations;
using AtomUI.Controls.Primitives;
using AtomUI.Controls.Utils;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

using AvaloniaRadioButton = Avalonia.Controls.RadioButton;

public abstract class AbstractOptionButton : AvaloniaRadioButton
{
    public static readonly StyledProperty<PathIcon?> IconProperty = AvaloniaProperty.Register<AbstractOptionButton, PathIcon?>(nameof (Icon));
    
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    #region 内部属性定义

    internal static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractOptionButton>();

    internal static readonly StyledProperty<OptionButtonStyle> ButtonStyleProperty =
        AvaloniaProperty.Register<AbstractOptionButton, OptionButtonStyle>(nameof(ButtonStyle));
    
    internal static readonly DirectProperty<AbstractOptionButton, OptionButtonPositionTrait> GroupPositionTraitProperty =
        AvaloniaProperty.RegisterDirect<AbstractOptionButton, OptionButtonPositionTrait>(
            nameof(GroupPositionTrait),
            o => o.GroupPositionTrait,
            (o, v) => o.GroupPositionTrait = v,
            OptionButtonPositionTrait.OnlyOne);

    internal static readonly DirectProperty<AbstractOptionButton, Orientation> GroupOrientationProperty =
        AvaloniaProperty.RegisterDirect<AbstractOptionButton, Orientation>(
            nameof(GroupOrientation),
            o => o.GroupOrientation,
            (o, v) => o.GroupOrientation = v,
            Orientation.Horizontal);

    internal static readonly DirectProperty<AbstractOptionButton, CornerRadius> EffectiveCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<AbstractOptionButton, CornerRadius>(
            nameof(EffectiveCornerRadius),
            o => o.EffectiveCornerRadius,
            (o, v) => o.EffectiveCornerRadius = v);

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractOptionButton>();

    internal static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<AbstractOptionButton>();

    internal CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    internal OptionButtonStyle ButtonStyle
    {
        get => GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }

    private OptionButtonPositionTrait _groupPositionTrait = OptionButtonPositionTrait.OnlyOne;

    internal OptionButtonPositionTrait GroupPositionTrait
    {
        get => _groupPositionTrait;
        set => SetAndRaise(GroupPositionTraitProperty, ref _groupPositionTrait, value);
    }

    private Orientation _groupOrientation;

    internal Orientation GroupOrientation
    {
        get => _groupOrientation;
        set => SetAndRaise(GroupOrientationProperty, ref _groupOrientation, value);
    }

    private CornerRadius _effectiveCornerRadius;

    internal CornerRadius EffectiveCornerRadius
    {
        get => _effectiveCornerRadius;
        private set => SetAndRaise(EffectiveCornerRadiusProperty, ref _effectiveCornerRadius, value);
    }

    internal event EventHandler<OptionButtonPointerEventArgs>? OptionButtonPointerEvent;

    #endregion

    private readonly BorderRenderHelper _borderRenderHelper;
    private WaveSpiritDecorator? _waveSpiritDecorator;

    private readonly TransitionInitializationSession _transitionInitialization;

    static AbstractOptionButton()
    {
        AffectsMeasure<AbstractOptionButton>(SizeTypeProperty, ButtonStyleProperty);
        AffectsRender<AbstractOptionButton>(IsCheckedProperty,
            EffectiveCornerRadiusProperty,
            ForegroundProperty,
            BackgroundProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            UseLayoutRoundingProperty);
    }

    public AbstractOptionButton()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
        _borderRenderHelper = new BorderRenderHelper();
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        Debug.Assert(Parent is AbstractOptionButtonGroup, "AbstractOptionButton parent must be type of OptionButtonGroup");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsPointerOverProperty ||
            change.Property == IsPressedProperty ||
            change.Property == IsCheckedProperty)
        {
            if (change.Property == IsPressedProperty &&
                change.OldValue as bool? == true &&
                IsMotionEnabled &&
                IsWaveSpiritEnabled)
            {
                _waveSpiritDecorator?.Play();
            }
        }

        if (change.Property == CornerRadiusProperty ||
            change.Property == GroupPositionTraitProperty ||
            change.Property == GroupOrientationProperty)
        {
            EffectiveCornerRadius = BuildEffectiveCornerRadius(
                GroupPositionTrait,
                GroupOrientation,
                CornerRadius);
        }
    }

    private static CornerRadius BuildEffectiveCornerRadius(
        OptionButtonPositionTrait positionTrait,
        Orientation orientation,
        CornerRadius cornerRadius)
    {
        if (positionTrait == OptionButtonPositionTrait.First)
        {
            return orientation == Orientation.Horizontal
                ? new CornerRadius(cornerRadius.TopLeft, 0, 0, cornerRadius.BottomLeft)
                : new CornerRadius(cornerRadius.TopLeft, cornerRadius.TopRight, 0, 0);
        }

        if (positionTrait == OptionButtonPositionTrait.Last)
        {
            return orientation == Orientation.Horizontal
                ? new CornerRadius(0, cornerRadius.TopRight, cornerRadius.BottomRight, 0)
                : new CornerRadius(0, 0, cornerRadius.BottomRight, cornerRadius.BottomLeft);
        }

        if (positionTrait == OptionButtonPositionTrait.Middle)
        {
            return new CornerRadius(0);
        }

        return cornerRadius;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        OptionButtonPointerEvent?.Invoke(this, new OptionButtonPointerEventArgs(this)
        {
            IsHovering = true,
            IsPressed  = true
        });
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        OptionButtonPointerEvent?.Invoke(this, new OptionButtonPointerEventArgs(this)
        {
            IsHovering = true,
            IsPressed  = false
        });
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        OptionButtonPointerEvent?.Invoke(this, new OptionButtonPointerEventArgs(this)
        {
            IsHovering = true,
            IsPressed  = false
        });
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        OptionButtonPointerEvent?.Invoke(this, new OptionButtonPointerEventArgs(this)
        {
            IsHovering = false,
            IsPressed  = false
        });
    }

    public override void Render(DrawingContext context)
    {
        _borderRenderHelper.Render(context,
            Bounds.Size,
            BorderUtils.BuildRenderScaleAwareThickness(this, BorderThickness),
            EffectiveCornerRadius,
            BackgroundSizing.InnerBorderEdge,
            Background,
            BorderBrush);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _waveSpiritDecorator = e.NameScope.Find<WaveSpiritDecorator>("PART_WaveSpirit");
        _transitionInitialization.CompleteInitialization();
    }

}
