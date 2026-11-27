using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Visual kind of a <see cref="Button" />. The value decides the color and variant a button falls back to
/// while <see cref="Button.Color" /> or <see cref="Button.Variant" /> is <see langword="null" />, and
/// <see cref="Dashed" /> selects the template with the dashed frame.
/// </summary>
public enum ButtonType
{
    /// <summary>
    /// Standard button. It is outlined and neutral, or solid and primary while <see cref="Button.Variant" />
    /// is <see cref="ButtonVariant.Solid" />, and it takes the danger color while
    /// <see cref="Button.IsDanger" /> is set.
    /// </summary>
    Default,
    /// <summary>
    /// Button with a dashed border, drawn in the <see cref="ButtonVariant.Dashed" /> style.
    /// </summary>
    Dashed,
    /// <summary>
    /// Button in the <see cref="ButtonVariant.Solid" /> style with the primary color, which is the danger
    /// color while <see cref="Button.IsDanger" /> is set.
    /// </summary>
    Primary,
    /// <summary>
    /// Button in the <see cref="ButtonVariant.Link" /> style: text without border or background, in the
    /// link color of the theme.
    /// </summary>
    Link,
    /// <summary>
    /// Button in the <see cref="ButtonVariant.Text" /> style: text without border; the background appears
    /// only while the button is hovered or pressed.
    /// </summary>
    Text
}

/// <summary>
/// The button of the desktop library. On top of <see cref="AbstractButton" /> it adds the visual kind
/// (<see cref="ButtonType" />), the shape of the frame (<see cref="Shape" />), the ghost, danger and loading looks,
/// a size step (<see cref="SizeType" />) and the placement, size and padding of the icon and of the content. An
/// explicit <see cref="Color" /> and <see cref="Variant" /> replace what the kind resolves to. The button plays a
/// wave animation when it is released while <see cref="IsWaveSpiritEnabled" /> allows it, and it takes part in a
/// compact <c>Space</c>, which adjusts its corner radius to its place in the group.
/// </summary>
[PseudoClasses(ButtonPseudoClass.DefaultType,
    ButtonPseudoClass.DashedType,
    ButtonPseudoClass.PrimaryType,
    ButtonPseudoClass.LinkType,
    ButtonPseudoClass.TextType)]
public partial class Button : AbstractButton,
                              ICustomizableSizeTypeAware,
                              IInteractionEffectAwareControl,
                              ICompactSpaceAware,
                              IFormItemAware,
                              IFocusOutlineShapeProvider
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="ButtonType" /> property.
    /// </summary>
    public static readonly StyledProperty<ButtonType> ButtonTypeProperty =
        AvaloniaProperty.Register<Button, ButtonType>(nameof(ButtonType));

    /// <summary>
    /// Identifies the <see cref="Shape" /> property.
    /// </summary>
    public new static readonly StyledProperty<ButtonShape> ShapeProperty =
        AbstractButton.ShapeProperty.AddOwner<Button>();

    /// <summary>
    /// Identifies the <see cref="IsDanger" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsDangerProperty =
        AbstractButton.IsDangerProperty.AddOwner<Button>();

    /// <summary>
    /// Identifies the <see cref="IsGhost" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsGhostProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(IsGhost));

    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsLoadingProperty =
        AbstractButton.IsLoadingProperty.AddOwner<Button>();

    /// <summary>
    /// Identifies the <see cref="LoadingIndicatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LoadingIndicatorTemplateProperty =
        AvaloniaProperty.Register<Button, IDataTemplate?>(nameof(LoadingIndicatorTemplate));

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Button>();

    /// <summary>
    /// Identifies the <see cref="ContentPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> ContentPaddingProperty =
        AvaloniaProperty.Register<Button, Thickness>(nameof(ContentPadding));

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public new static readonly StyledProperty<PathIcon?> IconProperty =
        AbstractButton.IconProperty.AddOwner<Button>();

    /// <summary>
    /// Identifies the <see cref="IconWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> IconWidthProperty =
        AvaloniaProperty.Register<Button, double>(nameof(IconWidth));

    /// <summary>
    /// Identifies the <see cref="IconHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> IconHeightProperty =
        AvaloniaProperty.Register<Button, double>(nameof(IconHeight));

    /// <summary>
    /// Identifies the <see cref="IconPlacement" /> property.
    /// </summary>
    public new static readonly StyledProperty<ButtonIconPlacement> IconPlacementProperty =
        AbstractButton.IconPlacementProperty.AddOwner<Button>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Button>();

    /// <summary>
    /// Identifies the <see cref="IsInteractionEffectEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsInteractionEffectEnabledProperty =
        InteractionEffectAwareControlProperty.IsInteractionEffectEnabledProperty.AddOwner<Button>();

    /// <summary>
    /// Identifies the <see cref="Color" /> property.
    /// </summary>
    public new static readonly StyledProperty<ButtonColor?> ColorProperty =
        AbstractButton.ColorProperty.AddOwner<Button>();

    /// <summary>
    /// Identifies the <see cref="Variant" /> property.
    /// </summary>
    public new static readonly StyledProperty<ButtonVariant?> VariantProperty =
        AbstractButton.VariantProperty.AddOwner<Button>();

    /// <summary>
    /// Visual kind of the button, one of the <c>ButtonType</c> values. While <see cref="Color" /> or
    /// <see cref="Variant" /> is <see langword="null" />, the value decides both through
    /// <see cref="ResolveFallbackColorAndVariant" />; a button that sets both is painted with those instead.
    /// </summary>
    public ButtonType ButtonType
    {
        get => GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }

    /// <summary>
    /// Shape of the button frame. The button never measures taller than wide. <see cref="ButtonShape.Circle" />
    /// measures square and <see cref="ButtonShape.Round" /> at least one and a half times as wide as high, and
    /// both take their corner radius from the measured height; <see cref="ButtonShape.Rectangular" /> forces
    /// square corners. The default theme pads a circle with its circle padding instead of
    /// <see cref="ContentPadding" />.
    /// </summary>
    public new ButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    /// <summary>
    /// Marks the button as destructive: it raises the <c>danger</c> pseudo-class, and while <see cref="Color" />
    /// or <see cref="Variant" /> is <see langword="null" /> the button is painted in the
    /// <see cref="ButtonColor.Danger" /> color whatever its <see cref="ButtonType" /> is.
    /// </summary>
    public new bool IsDanger
    {
        get => GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }

    /// <summary>
    /// Makes the button a ghost button: its background stays transparent and it casts no shadow, whatever its
    /// color and variant. A ghost button is painted in the <see cref="ButtonVariant.Outlined" /> style even when
    /// its type or <see cref="Variant" /> resolves to <see cref="ButtonVariant.Solid" />.
    /// </summary>
    public bool IsGhost
    {
        get => GetValue(IsGhostProperty);
        set => SetValue(IsGhostProperty, value);
    }

    /// <summary>
    /// Switches the button to its loading look: it raises the <c>loading</c> pseudo-class, with which the
    /// themes replace <see cref="Icon" /> with a spinning loading icon, and the wave animation is not played.
    /// The button stays enabled.
    /// </summary>
    public new bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// Size step of the button, <c>Middle</c> by default. The default theme takes the minimum height, font
    /// size, <see cref="ContentPadding" />, corner radius and icon size from it; <c>Custom</c> sets no minimum
    /// height. The value is set on the button itself and does not inherit.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Padding between the frame and the content of a button that shows content. The default theme sets it
    /// from the content padding of the <see cref="SizeType" /> and copies it into
    /// <see cref="TemplatedControl.Padding" />; a value set here applies to every size. It is ignored for an
    /// icon-only button and for a <see cref="ButtonShape.Circle" /> button, which pad with the icon-only
    /// padding and the circle padding of the theme.
    /// </summary>
    public Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    /// <summary>
    /// Icon shown next to the content, or <see langword="null" /> for no icon. The theme shows it only while
    /// it is an AtomUI <see cref="AtomUI.Controls.Icon" /> and the button is not loading; it is sized with
    /// <see cref="IconWidth" /> and <see cref="IconHeight" /> and painted with the foreground. An icon without
    /// content raises the <c>icononly</c> pseudo-class, with which the themes apply the icon-only padding.
    /// </summary>
    public new PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Width of <see cref="Icon" /> and of the loading icon, in device-independent pixels. The default theme
    /// sets it from the icon size of the <see cref="SizeType" />, or from the icon-only size while an icon-only
    /// button is loading. The registered value is 0, so a theme must set the property for the icon to be seen.
    /// </summary>
    public double IconWidth
    {
        get => GetValue(IconWidthProperty);
        set => SetValue(IconWidthProperty, value);
    }

    /// <summary>
    /// Height of <see cref="Icon" /> and of the loading icon, in device-independent pixels. The default theme
    /// sets it from the icon size of the <see cref="SizeType" />, or from the icon-only size while an icon-only
    /// button is loading. The registered value is 0, so a theme must set the property for the icon to be seen.
    /// </summary>
    public double IconHeight
    {
        get => GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }

    /// <summary>
    /// Side of the content the icon is placed on, <see cref="ButtonIconPlacement.Start" /> by default. The
    /// default theme docks <see cref="Icon" /> to the left of the content for
    /// <see cref="ButtonIconPlacement.Start" /> and to the right for <see cref="ButtonIconPlacement.End" />,
    /// each with its own icon margin; it has no effect on an icon-only button.
    /// </summary>
    public new ButtonIconPlacement IconPlacement
    {
        get => GetValue(IconPlacementProperty);
        set => SetValue(IconPlacementProperty, value);
    }

    /// <summary>
    /// Enables the motion of the button while <see langword="true" />: the color transitions of the default
    /// theme and the wave animation that runs when the button is released. The default theme sets it from the
    /// <c>EnableMotion</c> token; a value set on the button replaces that.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// The template used to create the indicator while the button is loading.
    /// </summary>
    public IDataTemplate? LoadingIndicatorTemplate
    {
        get => GetValue(LoadingIndicatorTemplateProperty);
        set => SetValue(LoadingIndicatorTemplateProperty, value);
    }

    /// <summary>
    /// Enables interaction feedback while <see langword="true" />. It runs when a pressed button is interacted with,
    /// and only while <see cref="IsMotionEnabled" /> is also <see langword="true" />, the button is not loading
    /// and its effective variant is <see cref="ButtonVariant.Solid" />,
    /// <see cref="ButtonVariant.Outlined" /> or <see cref="ButtonVariant.Dashed" />. The default theme sets it
    /// from the <c>EnableInteractionEffect</c> token; a value set on the button replaces that.
    /// </summary>
    public bool IsInteractionEffectEnabled
    {
        get => GetValue(IsInteractionEffectEnabledProperty);
        set => SetValue(IsInteractionEffectEnabledProperty, value);
    }

    /// <summary>
    /// Color scheme of the button, or <see langword="null" /> to derive it. The value is used only while
    /// <see cref="Variant" /> is also set; otherwise <see cref="ResolveFallbackColorAndVariant" /> decides the
    /// color from <see cref="ButtonType" /> and <see cref="IsDanger" />.
    /// </summary>
    public new ButtonColor? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>
    /// Painting style of the button, or <see langword="null" /> to derive it. With <see cref="Color" /> also
    /// set the pair is used as it is; otherwise <see cref="ResolveFallbackColorAndVariant" /> decides both, and
    /// a ghost button never shows <see cref="ButtonVariant.Solid" /> because
    /// <see cref="NormalizeVariant" /> turns it into <see cref="ButtonVariant.Outlined" />.
    /// </summary>
    public new ButtonVariant? Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsIconVisibleProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(IsIconVisible), true);

    internal static readonly DirectProperty<Button, bool> EffectiveIconVisibleProperty =
        AvaloniaProperty.RegisterDirect<Button, bool>(
            nameof(EffectiveIconVisible),
            o => o.EffectiveIconVisible);

    internal static readonly StyledProperty<object?> RightExtraContentProperty =
        AvaloniaProperty.Register<Button, object?>(nameof(RightExtraContent));

    internal static readonly StyledProperty<IDataTemplate?> RightExtraContentTemplateProperty =
        AvaloniaProperty.Register<Button, IDataTemplate?>(nameof(RightExtraContentTemplate));

    internal static readonly StyledProperty<Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.Register<Button, Thickness>(
            nameof(EffectiveBorderThickness));

    internal static readonly StyledProperty<bool> EffectiveIsDangerProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(EffectiveIsDanger));

    internal static readonly StyledProperty<bool> EffectiveIsGhostProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(EffectiveIsGhost));

    internal static readonly StyledProperty<bool> EffectiveIsBorderedProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(EffectiveIsBordered), true);

    internal static readonly DirectProperty<Button, ButtonVariantAppearance> VariantAppearanceProperty =
        AvaloniaProperty.RegisterDirect<Button, ButtonVariantAppearance>(
            nameof(VariantAppearance), button => button.VariantAppearance);

    internal static readonly DirectProperty<Button, CornerRadius> EffectiveCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<Button, CornerRadius>(nameof(EffectiveCornerRadius),
            o => o.EffectiveCornerRadius,
            (o, v) => o.EffectiveCornerRadius = v);

    /// <summary>
    /// Corner radius imposed by a hosting control that splits one visual frame over several Buttons (SplitButton). It
    /// replaces <see cref="TemplatedControl.CornerRadius" /> as the base of <see cref="EffectiveCornerRadius" /> so the
    /// public property keeps the theme and user values.
    /// </summary>
    internal static readonly StyledProperty<CornerRadius?> HostCornerRadiusProperty =
        AvaloniaProperty.Register<Button, CornerRadius?>(nameof(HostCornerRadius));

    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty =
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<Button>();

    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty =
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<Button>();

    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty =
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<Button>();

    internal static readonly DirectProperty<Button, bool> UsesThemeRecipeProperty =
        AvaloniaProperty.RegisterDirect<Button, bool>(nameof(UsesThemeRecipe), button => button.UsesThemeRecipe);

    internal static readonly DirectProperty<Button, bool> DisablesWithOpacityProperty =
        AvaloniaProperty.RegisterDirect<Button, bool>(nameof(DisablesWithOpacity),
            button => button.DisablesWithOpacity);

    internal static readonly DirectProperty<Button, double> ThemeDisabledOpacityProperty =
        AvaloniaProperty.RegisterDirect<Button, double>(nameof(ThemeDisabledOpacity),
            button => button.ThemeDisabledOpacity);

    internal static readonly DirectProperty<Button, bool> PressMovesProperty =
        AvaloniaProperty.RegisterDirect<Button, bool>(nameof(PressMoves), button => button.PressMoves);

    internal static readonly DirectProperty<Button, bool> UnderlinesLinkOnHoverProperty =
        AvaloniaProperty.RegisterDirect<Button, bool>(nameof(UnderlinesLinkOnHover),
            button => button.UnderlinesLinkOnHover);

    internal static readonly StyledProperty<bool> IsExternalPopupTriggerProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(IsExternalPopupTrigger));

    internal static readonly StyledProperty<bool> IsExternalPopupOpenProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(IsExternalPopupOpen));

    internal bool IsIconVisible
    {
        get => GetValue(IsIconVisibleProperty);
        set => SetValue(IsIconVisibleProperty, value);
    }

    private bool _effectiveIconVisible;

    /// <summary>
    /// Whether the icon slot is shown: an AtomUI <see cref="AtomUI.Controls.Icon" /> is set, the icon is allowed
    /// and the button is not loading. Templates bind to it instead of a three-way MultiBinding.
    /// </summary>
    internal bool EffectiveIconVisible
    {
        get => _effectiveIconVisible;
        private set => SetAndRaise(EffectiveIconVisibleProperty, ref _effectiveIconVisible, value);
    }

    internal object? RightExtraContent
    {
        get => GetValue(RightExtraContentProperty);
        set => SetValue(RightExtraContentProperty, value);
    }

    internal IDataTemplate? RightExtraContentTemplate
    {
        get => GetValue(RightExtraContentTemplateProperty);
        set => SetValue(RightExtraContentTemplateProperty, value);
    }

    internal Thickness EffectiveBorderThickness
    {
        get => GetValue(EffectiveBorderThicknessProperty);
        set => SetValue(EffectiveBorderThicknessProperty, value);
    }

    internal bool EffectiveIsDanger
    {
        get => GetValue(EffectiveIsDangerProperty);
        set => SetValue(EffectiveIsDangerProperty, value);
    }

    internal bool EffectiveIsGhost
    {
        get => GetValue(EffectiveIsGhostProperty);
        set => SetValue(EffectiveIsGhostProperty, value);
    }

    internal bool EffectiveIsBordered
    {
        get => GetValue(EffectiveIsBorderedProperty);
        set => SetValue(EffectiveIsBorderedProperty, value);
    }

    private ButtonVariantAppearance _variantAppearance;

    internal ButtonVariantAppearance VariantAppearance
    {
        get => _variantAppearance;

        private set
        {
            SetAndRaise(VariantAppearanceProperty, ref _variantAppearance, value);
            if (PressMoves)
            {
                PseudoClasses.Set(":pressed-text", value.TextPressed is not null);
                PseudoClasses.Set(":pressed-background", value.BackgroundPressed is not null);
                PseudoClasses.Set(":pressed-border", value.BorderPressed is not null);
            }
        }
    }

    private CornerRadius _effectiveCornerRadius;

    internal CornerRadius EffectiveCornerRadius
    {
        get => _effectiveCornerRadius;
        set => SetAndRaise(EffectiveCornerRadiusProperty, ref _effectiveCornerRadius, value);
    }

    // Round and circle shapes are expressed by the effective radius, not by CornerRadius.
    CornerRadius IFocusOutlineShapeProvider.FocusOutlineCornerRadius => EffectiveCornerRadius;

    internal CornerRadius? HostCornerRadius
    {
        get => GetValue(HostCornerRadiusProperty);
        set => SetValue(HostCornerRadiusProperty, value);
    }

    internal SpaceItemPosition? CompactSpaceItemPosition
    {
        get => GetValue(CompactSpaceItemPositionProperty);
        set => SetValue(CompactSpaceItemPositionProperty, value);
    }

    internal Orientation CompactSpaceOrientation
    {
        get => GetValue(CompactSpaceOrientationProperty);
        set => SetValue(CompactSpaceOrientationProperty, value);
    }

    internal bool IsUsedInCompactSpace
    {
        get => GetValue(IsUsedInCompactSpaceProperty);
        set => SetValue(IsUsedInCompactSpaceProperty, value);
    }

    private bool _usesThemeRecipe;

    // The recipe color model; see AbstractButtonGeometryToken.RecipeVariants.
    internal bool UsesThemeRecipe
    {
        get => _usesThemeRecipe;
        private set => SetAndRaise(UsesThemeRecipeProperty, ref _usesThemeRecipe, value);
    }

    private bool _disablesWithOpacity;

    // Disabled buttons keep their colors at ThemeDisabledOpacity instead of the gray disabled paint.
    internal bool DisablesWithOpacity
    {
        get => _disablesWithOpacity;
        private set => SetAndRaise(DisablesWithOpacityProperty, ref _disablesWithOpacity, value);
    }

    private double _themeDisabledOpacity = 1;

    internal double ThemeDisabledOpacity
    {
        get => _themeDisabledOpacity;
        private set => SetAndRaise(ThemeDisabledOpacityProperty, ref _themeDisabledOpacity, value);
    }

    private bool _pressMoves;

    // Pressing moves the button instead of switching to the pressed colors.
    internal bool PressMoves
    {
        get => _pressMoves;
        private set => SetAndRaise(PressMovesProperty, ref _pressMoves, value);
    }

    private bool _underlinesLinkOnHover;

    internal bool UnderlinesLinkOnHover
    {
        get => _underlinesLinkOnHover;
        private set => SetAndRaise(UnderlinesLinkOnHoverProperty, ref _underlinesLinkOnHover, value);
    }

    internal bool IsExternalPopupTrigger
    {
        get => GetValue(IsExternalPopupTriggerProperty);
        set => SetValue(IsExternalPopupTriggerProperty, value);
    }

    internal bool IsExternalPopupOpen
    {
        get => GetValue(IsExternalPopupOpenProperty);
        set => SetValue(IsExternalPopupOpenProperty, value);
    }

    #endregion

    private static readonly ThemeTokenResolver s_themeTokenResolver = new();
    private InteractionEffectHost? _pressFeedback;
    private bool _feedbackGesture;
    private bool _feedbackActivated;
    private InteractionEffectInputSource _activationSource = InteractionEffectInputSource.Automation;
    private IDisposable? _themeScopeSubscription;
    private CornerRadius? _shapeCornerRadius;

    static Button()
    {
        AffectsMeasure<Button>(SizeTypeProperty,
            ShapeProperty,
            IconProperty,
            IconWidthProperty,
            IconHeightProperty,
            IconPlacementProperty,
            CompactSpaceItemPositionProperty,
            CompactSpaceOrientationProperty);
        AffectsRender<Button>(ButtonTypeProperty,
            IsDangerProperty,
            IsGhostProperty,
            ColorProperty,
            VariantProperty);
    }

    #region 实现 CompactSpace 接口

    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        IsUsedInCompactSpace     = position != null;
        CompactSpaceItemPosition = position;
    }

    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        CompactSpaceOrientation = orientation;
    }

    bool ICompactSpaceAware.IsAlwaysActiveZIndex()
    {
        return ButtonType == ButtonType.Primary;
    }

    double ICompactSpaceAware.GetBorderThickness() => GetBorderThicknessForCompactSpace();

    /// <summary>
    /// Border width this button reports to a compact <c>Space</c>, which offsets each item of the group by a
    /// multiple of it: the left border in a horizontal space, the top border in a vertical one, and 0 while the
    /// button is not used in compact space. It is called through
    /// <c>ICompactSpaceAware.GetBorderThickness</c>; override it to report a different width.
    /// </summary>
    /// <returns>
    /// The border width in device-independent pixels.
    /// </returns>
    protected virtual double GetBorderThicknessForCompactSpace()
    {
        if (!IsUsedInCompactSpace)
        {
            return 0.0;
        }

        return CompactSpaceOrientation == Orientation.Horizontal ? BorderThickness.Left : BorderThickness.Top;
    }

    #endregion

    #region 实现 FormItem 接口

    private EventHandler? _formValueChanged;

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);

    /// <summary>
    /// Called when a form writes a value into this control through <c>IFormItemAware.SetFormValue</c>, which
    /// a form item does for values that do not come from the user. A button keeps no form value, so the
    /// default does nothing; override it to react to the value.
    /// </summary>
    /// <param name="value">
    /// The value the form writes, of the type that <see cref="NotifyGetFormValue" /> returns.
    /// </param>
    protected virtual void NotifySetFormValue(object? value)
    {
    }

    /// <summary>
    /// Returns the value a form reads from this control through <c>IFormItemAware.GetFormValue</c>, which
    /// becomes the value of the form item and of its field. A button keeps no form value, so the default
    /// returns <see langword="null" />; override it to report one.
    /// </summary>
    /// <returns>
    /// <see langword="null" /> in the default implementation.
    /// </returns>
    protected virtual object? NotifyGetFormValue()
    {
        return null;
    }

    /// <summary>
    /// Called when the form item that holds this control is reset and has no initial value to restore. A
    /// button keeps no form value, so the default does nothing; override it to reset state kept for the form.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
    }

    /// <summary>
    /// Called when the form item that holds this control updates its validation state, also with
    /// <c>FormValidateStatus.Validating</c> while a validation runs. A button shows no validation feedback, so
    /// the default does nothing; override it to react to the status.
    /// </summary>
    /// <param name="status">
    /// The new validation status of the form item.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }

    #endregion

    /// <summary>
    /// Calls the base member, then subscribes to the theme token resolver and applies the theme appearance.
    /// The subscription replaces one left by an earlier attachment and is disposed in
    /// <see cref="OnDetachedFromLogicalTree" />. Derived overrides must call the base member.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _themeScopeSubscription?.Dispose();
        _themeScopeSubscription = s_themeTokenResolver.Subscribe(
            this,
            _ => ConfigureThemeAppearance());
        ConfigureThemeAppearance();
    }

    /// <summary>
    /// Disposes the theme token subscription, then calls the base member. Derived overrides must call the base
    /// member.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _themeScopeSubscription?.Dispose();
        _themeScopeSubscription = null;
        base.OnDetachedFromLogicalTree(e);
    }

    /// <summary>
    /// Calls the base member, then resolves the wave spirit decorator of the template and refreshes the type
    /// pseudo-classes, the wave type and the effective corner radius from the current property values. Derived
    /// overrides must call the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        CancelFeedback();
        InteractionEffectHost.CancelFor(this);
        base.OnApplyTemplate(e);
        UpdateButtonTypePseudoClasses();
        ConfigureEffectiveCornerRadius();
    }

    /// <summary>
    /// Measures the content and applies the shape to the result: the width is never smaller than the height,
    /// a circle measures square and a round button at least one and a half times as wide as high. For those two
    /// shapes the measured height also becomes the effective corner radius, without changing
    /// <see cref="TemplatedControl.CornerRadius" />.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size         = LayoutHelper.ApplyLayoutConstraints(this, base.MeasureOverride(availableSize));
        var targetWidth  = size.Width;
        var targetHeight = size.Height;

        targetWidth = Math.Max(targetWidth, targetHeight);

        if (Shape == ButtonShape.Circle)
        {
            targetWidth = targetHeight;
        }
        else if (Shape == ButtonShape.Round)
        {
            targetWidth = Math.Max(targetWidth, targetHeight + targetHeight / 2);
        }

        // Circle and Round radii follow the measured height. They feed the internal EffectiveCornerRadius only, so
        // the public CornerRadius keeps the theme value and any user value.
        CornerRadius? shapeCornerRadius = Shape is ButtonShape.Circle or ButtonShape.Round
            ? new CornerRadius(targetHeight)
            : null;
        if (shapeCornerRadius != _shapeCornerRadius)
        {
            _shapeCornerRadius = shapeCornerRadius;
            ConfigureEffectiveCornerRadius();
        }

        return new Size(targetWidth, targetHeight);
    }

    /// <summary>
    /// Calls the base member, then keeps the state derived from the properties in step with them: the icon
    /// slot follows <see cref="Icon" /> and <see cref="IsLoading" />, releasing a pressed button starts the
    /// wave animation, the <c>popup-trigger</c> pseudo-class follows whether a <c>Flyout</c> is set, the type
    /// pseudo-classes follow <see cref="ButtonType" />, the wave type follows <see cref="Shape" />, the
    /// effective color and variant follow <see cref="ButtonType" /> and <see cref="IsGhost" />, and the
    /// effective border thickness and corner radius follow the properties they are computed from. Derived
    /// overrides must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty ||
            change.Property == IsIconVisibleProperty ||
            change.Property == IsLoadingProperty)
        {
            // Same rule as ButtonIconVisibleConverter, which the templates used before.
            EffectiveIconVisible = Icon is AtomUI.Controls.Icon && IsIconVisible && !IsLoading;
        }

        if (change.Property == FlyoutProperty)
        {
            PseudoClasses.Set(":popup-trigger", Flyout is not null);
        }

        if ((change.Property == IsInteractionEffectEnabledProperty && !IsInteractionEffectEnabled) ||
            (change.Property == IsLoadingProperty && IsLoading))
        {
            CancelFeedback();
            InteractionEffectHost.CancelFor(this);
        }

        if (change.Property == ButtonTypeProperty || change.Property == IsGhostProperty)
        {
            RefreshEffectiveButtonState();
        }

        if (change.Property == ButtonTypeProperty)
        {
            UpdateButtonTypePseudoClasses();
        }

        if (ShouldConfigureEffectiveBorderThickness(change.Property))
        {
            ConfigureEffectiveBorderThickness();
        }

        if (ShouldConfigureEffectiveCornerRadius(change.Property))
        {
            ConfigureEffectiveCornerRadius();
        }
    }

    private void ConfigureThemeAppearance()
    {
        var behavior = ButtonBehavior.Get(this);
        _behavior             = behavior;
        UsesThemeRecipe       = behavior.RecipeVariants;
        DisablesWithOpacity   = behavior.DisabledByOpacity;
        ThemeDisabledOpacity  = behavior.DisabledByOpacity ? behavior.DisabledOpacity : 1;
        PressMoves            = behavior.PressMoves;
        UnderlinesLinkOnHover = behavior.UnderlineLinkOnHover;
        PseudoClasses.Set(":link-underlined", behavior.UnderlineLinkAtRest);
        ConfigureVariantThemeVariables();
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && IsEffectivelyEnabled)
        {
            BeginFeedback(InteractionEffectInputSource.Pointer, e.GetPosition(this));
        }
        base.OnPointerPressed(e);
        if (!IsPressed)
        {
            CompleteFeedback();
        }
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            CompleteFeedback();
        }
    }

    /// <inheritdoc />
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        CancelFeedback();
        base.OnPointerCaptureLost(e);
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Space && IsFocused && !_feedbackGesture)
        {
            BeginFeedback(InteractionEffectInputSource.Keyboard, null);
        }
        _activationSource = InteractionEffectInputSource.Keyboard;
        try
        {
            base.OnKeyDown(e);
        }
        finally
        {
            _activationSource = InteractionEffectInputSource.Automation;
        }
    }

    /// <inheritdoc />
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (e.Key == Key.Space)
        {
            CompleteFeedback();
        }
    }

    /// <inheritdoc />
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        CancelFeedback();
        base.OnLostFocus(e);
    }

    /// <inheritdoc />
    protected override void OnClick()
    {
        if (IsEffectivelyEnabled && CanPlayFeedback())
        {
            if (_feedbackGesture)
            {
                _feedbackActivated = true;
                _pressFeedback?.Accept();
            }
            else
            {
                InteractionEffectHost.Activate(this, DefaultFeedback, _activationSource, null,
                    EffectiveCornerRadius, AntDesignWaveEffect.ResolveBrush(BorderBrush, Background));
            }
        }
        base.OnClick();
    }

    private InteractionEffect DefaultFeedback => EffectiveVariant is ButtonVariant.Solid or ButtonVariant.Outlined or ButtonVariant.Dashed
        ? AntDesignWaveEffect.Default : InteractionEffect.None;

    private bool CanPlayFeedback() => !IsLoading && InteractionEffectAwareControlProperty.IsEnabled(this);

    private void BeginFeedback(InteractionEffectInputSource source, Point? position)
    {
        CancelFeedback();
        _feedbackGesture = true;
        if (CanPlayFeedback())
        {
            _pressFeedback = InteractionEffectHost.Begin(this, DefaultFeedback, source, position,
                EffectiveCornerRadius, AntDesignWaveEffect.ResolveBrush(BorderBrush, Background));
        }
    }

    private void CompleteFeedback()
    {
        var press = _pressFeedback;
        var activated = _feedbackActivated;
        _pressFeedback = null;
        _feedbackGesture = false;
        _feedbackActivated = false;
        press?.Complete(activated);
    }

    private void CancelFeedback()
    {
        var press = _pressFeedback;
        _pressFeedback = null;
        _feedbackGesture = false;
        _feedbackActivated = false;
        press?.Cancel();
    }

    private void ConfigureEffectiveBorderThickness()
    {
        if (EffectiveIsBordered)
        {
            EffectiveBorderThickness = BorderThickness;
        }
        else
        {
            EffectiveBorderThickness = new Thickness(0);
        }
    }

    /// <summary>
    /// Projects a new effective color and variant into the template state: <c>EffectiveIsDanger</c> follows
    /// the effective color, <c>EffectiveIsGhost</c> follows <see cref="IsGhost" />, the frame gets a border for
    /// the <see cref="ButtonVariant.Outlined" />, <see cref="ButtonVariant.Dashed" /> and
    /// <see cref="ButtonVariant.Solid" /> styles and none for the others, and the variant theme variables are
    /// refreshed. It runs after every change of the effective state; derived overrides must call the base
    /// member.
    /// </summary>
    protected override void OnEffectiveButtonStateChanged()
    {
        EffectiveIsDanger   = EffectiveColor == ButtonColor.Danger;
        EffectiveIsGhost    = IsGhost;
        EffectiveIsBordered = IsBorderedVariant(EffectiveVariant);

        ConfigureEffectiveBorderThickness();
        ConfigureVariantThemeVariables();
    }

    /// <summary>
    /// Turns <see cref="ButtonVariant.Solid" /> into <see cref="ButtonVariant.Outlined" /> for a ghost button
    /// and returns every other variant unchanged. It runs before a variant is stored as the effective variant,
    /// for a set <see cref="Variant" /> as well as for the fallback.
    /// </summary>
    /// <param name="variant">
    /// The variant resolved from <see cref="Variant" /> or from the fallback.
    /// </param>
    protected override ButtonVariant NormalizeVariant(ButtonVariant variant) =>
        IsGhost && variant == ButtonVariant.Solid ? ButtonVariant.Outlined : variant;

    /// <summary>
    /// Returns the color and variant for a button whose <see cref="Color" /> or <see cref="Variant" /> is
    /// <see langword="null" />. A button of a type other than <c>Default</c>, or a danger button, takes the
    /// style of its type and the <see cref="ButtonColor.Danger" /> color, or <see cref="ButtonColor.Primary" />
    /// for <c>Primary</c> and <see cref="ButtonColor.Default" /> for every other type. A default button that
    /// is not a danger button takes primary and solid while <see cref="Variant" /> is
    /// <see cref="ButtonVariant.Solid" />, and default and outlined otherwise.
    /// </summary>
    /// <returns>
    /// The fallback color in <c>Color</c> and the fallback variant in <c>Variant</c>.
    /// </returns>
    protected override (ButtonColor Color, ButtonVariant Variant) ResolveFallbackColorAndVariant()
    {
        if (ButtonType != ButtonType.Default || IsDanger)
        {
            var variant = ButtonType switch
            {
                ButtonType.Primary => ButtonVariant.Solid,
                ButtonType.Dashed => ButtonVariant.Dashed,
                ButtonType.Link => ButtonVariant.Link,
                ButtonType.Text => ButtonVariant.Text,
                _ => ButtonVariant.Outlined
            };
            return (IsDanger ? ButtonColor.Danger : ResolveCompatibilityColor(ButtonType), variant);
        }

        if (Variant == ButtonVariant.Solid)
        {
            return (ButtonColor.Primary, ButtonVariant.Solid);
        }

        return (ButtonColor.Default, ButtonVariant.Outlined);
    }

    private static ButtonColor ResolveCompatibilityColor(ButtonType buttonType)
    {
        return buttonType switch
        {
            ButtonType.Primary => ButtonColor.Primary,
            _ => ButtonColor.Default
        };
    }

    private static bool IsBorderedVariant(ButtonVariant variant)
    {
        return variant == ButtonVariant.Outlined ||
               variant == ButtonVariant.Dashed ||
               variant == ButtonVariant.Solid;
    }

    private static bool TryGetPresetPrimaryColor(ButtonColor color, out PresetPrimaryColor presetColor)
    {
        switch (color)
        {
            case ButtonColor.Red:
                presetColor = PresetPrimaryColor.Red;
                return true;
            case ButtonColor.Volcano:
                presetColor = PresetPrimaryColor.Volcano;
                return true;
            case ButtonColor.Orange:
                presetColor = PresetPrimaryColor.Orange;
                return true;
            case ButtonColor.Gold:
                presetColor = PresetPrimaryColor.Gold;
                return true;
            case ButtonColor.Yellow:
                presetColor = PresetPrimaryColor.Yellow;
                return true;
            case ButtonColor.Lime:
                presetColor = PresetPrimaryColor.Lime;
                return true;
            case ButtonColor.Green:
                presetColor = PresetPrimaryColor.Green;
                return true;
            case ButtonColor.Cyan:
                presetColor = PresetPrimaryColor.Cyan;
                return true;
            case ButtonColor.Blue:
                presetColor = PresetPrimaryColor.Blue;
                return true;
            case ButtonColor.GeekBlue:
                presetColor = PresetPrimaryColor.GeekBlue;
                return true;
            case ButtonColor.Purple:
                presetColor = PresetPrimaryColor.Purple;
                return true;
            case ButtonColor.Pink:
                presetColor = PresetPrimaryColor.Pink;
                return true;
            case ButtonColor.Magenta:
                presetColor = PresetPrimaryColor.Magenta;
                return true;
            case ButtonColor.Grey:
                presetColor = PresetPrimaryColor.Grey;
                return true;
            default:
                presetColor = PresetPrimaryColor.Grey;
                return false;
        }
    }

    private void ConfigureEffectiveCornerRadius()
    {
        EffectiveCornerRadius = CompactSpace.CalculateEffectiveCornerRadius(
            Shape == ButtonShape.Rectangular ? default : _shapeCornerRadius ?? HostCornerRadius ?? CornerRadius,
            IsUsedInCompactSpace,
            CompactSpaceItemPosition,
            CompactSpaceOrientation);
    }

    private bool ShouldConfigureEffectiveBorderThickness(AvaloniaProperty property)
    {
        return property == BorderBrushProperty ||
               property == IsEnabledProperty ||
               property == BorderThicknessProperty;
    }

    private bool ShouldConfigureEffectiveCornerRadius(AvaloniaProperty property)
    {
        return property == CornerRadiusProperty ||
               property == HostCornerRadiusProperty ||
               property == ShapeProperty ||
               property == CompactSpaceItemPositionProperty ||
               property == CompactSpaceOrientationProperty;
    }

    private void UpdateButtonTypePseudoClasses()
    {
        PseudoClasses.Set(ButtonPseudoClass.DefaultType, ButtonType == ButtonType.Default);
        PseudoClasses.Set(ButtonPseudoClass.DashedType, ButtonType == ButtonType.Dashed);
        PseudoClasses.Set(ButtonPseudoClass.PrimaryType, ButtonType == ButtonType.Primary);
        PseudoClasses.Set(ButtonPseudoClass.LinkType, ButtonType == ButtonType.Link);
        PseudoClasses.Set(ButtonPseudoClass.TextType, ButtonType == ButtonType.Text);
    }
}
