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
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

using AvaloniaRadioButton = Avalonia.Controls.RadioButton;

/// <summary>
/// Base class of one option of an <see cref="AbstractOptionButtonGroup" />, which must be its parent.
/// The button paints its own background and border with the corner radius that its position in the
/// group allows; the group supplies the size, the style, the orientation and the motion settings.
/// </summary>
public abstract class AbstractOptionButton : AvaloniaRadioButton
{
    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty = AvaloniaProperty.Register<AbstractOptionButton, PathIcon?>(nameof (Icon));
    
    /// <summary>
    /// Icon shown to the left of the option text, tinted with <see cref="TemplatedControl.Foreground" />.
    /// <see langword="null" />, the default, shows the text only. Set it on the option button, or let an
    /// <see cref="OptionButtonData" /> item provide it.
    /// </summary>
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

    /// <summary>
    /// Identifies the <see cref="OutlineCheckedFill" /> property.
    /// </summary>
    internal static readonly StyledProperty<IBrush?> OutlineCheckedFillProperty =
        AvaloniaProperty.Register<AbstractOptionButton, IBrush?>(nameof(OutlineCheckedFill));

    /// <summary>
    /// Size step of the option, <c>Middle</c> by default. The group binds it one-way to its own
    /// <see cref="AbstractOptionButtonGroup.SizeType" />, so the value of the group applies to every option. The
    /// theme selects on it to take the font size, padding, corner radius and icon size from the Tokens of that
    /// step, and in a vertical group the height of the option as well. A change invalidates the measure of the
    /// option.
    /// </summary>
    internal CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Visual style of the option, <c>Outline</c> by default. The group binds it one-way to its own
    /// <see cref="AbstractOptionButtonGroup.ButtonStyle" />, so the value of the group applies to every option. For
    /// <c>Solid</c> the theme fills a checked option with its own colors and animates their changes while
    /// <see cref="IsMotionEnabled" /> is <see langword="true" />; for <c>Outline</c> the group marks the checked
    /// option with a border of its own and with <see cref="OutlineCheckedFill" />. A change invalidates the measure
    /// of the option.
    /// </summary>
    internal OptionButtonStyle ButtonStyle
    {
        get => GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }
    
    /// <summary>
    /// Whether the option animates. The group binds it one-way to
    /// <see cref="AbstractOptionButtonGroup.IsMotionEnabled" />, so the value of the group applies to every option.
    /// While it is <see langword="true" />, a press that ends plays the click wave of <c>PART_WaveSpirit</c> when
    /// <see cref="IsWaveSpiritEnabled" /> is <see langword="true" /> as well, and the theme of the <c>Solid</c>
    /// style animates the color changes of the option. The template passes the value to <c>PART_WaveSpirit</c>.
    /// </summary>
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Whether the option may play the click wave. The group binds it one-way to
    /// <see cref="AbstractOptionButtonGroup.IsWaveSpiritEnabled" />, so the value of the group applies to every
    /// option. The wave plays when a press ends while <see cref="IsMotionEnabled" /> is <see langword="true" />.
    /// The template passes the value to <c>PART_WaveSpirit</c>.
    /// </summary>
    internal bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }

    /// <summary>
    /// Fill of the checked option in the <see cref="OptionButtonStyle.Outline" /> style. The option button
    /// does not paint it: the group draws the fill inside a border of its own over the checked option and
    /// the separators next to it, and only while the group and the option are enabled. The theme takes the
    /// value from the <c>ButtonOutlineCheckedBackground</c> token, which is transparent by default;
    /// <see langword="null" /> paints no fill.
    /// </summary>
    internal IBrush? OutlineCheckedFill
    {
        get => GetValue(OutlineCheckedFillProperty);
        set => SetValue(OutlineCheckedFillProperty, value);
    }

    private OptionButtonPositionTrait _groupPositionTrait = OptionButtonPositionTrait.OnlyOne;

    internal OptionButtonPositionTrait GroupPositionTrait
    {
        get => _groupPositionTrait;
        set => SetAndRaise(GroupPositionTraitProperty, ref _groupPositionTrait, value);
    }

    private Orientation _groupOrientation;

    /// <summary>
    /// Orientation of the group the option belongs to, <see cref="Orientation.Horizontal" /> by default; the group
    /// assigns its own <c>Orientation</c> to every option. Together with <see cref="GroupPositionTrait" /> it decides
    /// which corners keep the radius of <see cref="TemplatedControl.CornerRadius" /> in
    /// <see cref="EffectiveCornerRadius" />. The theme selects on a vertical group to give the option a fixed height
    /// and to stretch its content layout.
    /// </summary>
    internal Orientation GroupOrientation
    {
        get => _groupOrientation;
        set => SetAndRaise(GroupOrientationProperty, ref _groupOrientation, value);
    }

    private CornerRadius _effectiveCornerRadius;

    /// <summary>
    /// Corner radius the option paints its background and border with: the corners on the outside of the group keep
    /// the radius of <see cref="TemplatedControl.CornerRadius" /> and the corners that touch a neighbor are square.
    /// The option rebuilds it when <see cref="TemplatedControl.CornerRadius" />,
    /// <see cref="GroupPositionTrait" /> or <see cref="GroupOrientation" /> changes. The template passes it to
    /// <c>PART_WaveSpirit</c> through its <c>CornerRadius</c>.
    /// </summary>
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

    /// <summary>
    /// Turns the transitions off again when the template is replaced, so that the initial state of the new
    /// template is shown without an animation. Overrides must call the base method.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Turns the transitions off when the button leaves the visual tree, so that attaching it again does
    /// not animate. Overrides must call the base method.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Turns the transitions back on once the button is loaded and its initial state is in place.
    /// Overrides must call the base method.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// Checks in debug builds that the parent is an <see cref="AbstractOptionButtonGroup" />.
    /// Overrides must call the base method.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        Debug.Assert(Parent is AbstractOptionButtonGroup, "AbstractOptionButton parent must be type of OptionButtonGroup");
    }

    /// <summary>
    /// Rebuilds <c>EffectiveCornerRadius</c> when <c>CornerRadius</c>, the group position or the group
    /// orientation changes, plays the click wave when a press ends while <c>IsMotionEnabled</c> and
    /// <c>IsWaveSpiritEnabled</c> are <see langword="true" />, and asks the group to repaint when
    /// <c>OutlineCheckedFill</c> changes. Overrides must call the base method.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OutlineCheckedFillProperty)
        {
            // The group paints the fill.
            this.FindAncestorOfType<AbstractOptionButtonGroup>()?.InvalidateVisual();
        }
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

    /// <summary>
    /// Reports the press through <c>OptionButtonPointerEvent</c> with <c>IsPressed</c> and
    /// <c>IsHovering</c> set to <see langword="true" />. Overrides must call the base method.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        OptionButtonPointerEvent?.Invoke(this, new OptionButtonPointerEventArgs(this)
        {
            IsHovering = true,
            IsPressed  = true
        });
    }

    /// <summary>
    /// Reports the release through <c>OptionButtonPointerEvent</c> with <c>IsPressed</c> set to
    /// <see langword="false" /> and <c>IsHovering</c> to <see langword="true" />. Overrides must call the
    /// base method.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        OptionButtonPointerEvent?.Invoke(this, new OptionButtonPointerEventArgs(this)
        {
            IsHovering = true,
            IsPressed  = false
        });
    }

    /// <summary>
    /// Reports the pointer entry through <c>OptionButtonPointerEvent</c> with <c>IsHovering</c> set to
    /// <see langword="true" /> and <c>IsPressed</c> to <see langword="false" />. Overrides must call the
    /// base method.
    /// </summary>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        OptionButtonPointerEvent?.Invoke(this, new OptionButtonPointerEventArgs(this)
        {
            IsHovering = true,
            IsPressed  = false
        });
    }

    /// <summary>
    /// Reports the pointer exit through <c>OptionButtonPointerEvent</c> with <c>IsHovering</c> and
    /// <c>IsPressed</c> set to <see langword="false" />. Overrides must call the base method.
    /// </summary>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        OptionButtonPointerEvent?.Invoke(this, new OptionButtonPointerEventArgs(this)
        {
            IsHovering = false,
            IsPressed  = false
        });
    }

    /// <summary>
    /// Paints <c>Background</c> and <c>BorderBrush</c> within <c>EffectiveCornerRadius</c>; no template part
    /// draws them. Overrides must call the base method.
    /// </summary>
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

    /// <summary>
    /// Looks up the <c>WaveSpiritDecorator</c> named <c>PART_WaveSpirit</c> that plays the click wave, and
    /// turns the transitions back on once the button is loaded. Overrides must call the base method.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _waveSpiritDecorator = e.NameScope.Find<WaveSpiritDecorator>("PART_WaveSpirit");
        _transitionInitialization.CompleteInitialization();
    }

}
