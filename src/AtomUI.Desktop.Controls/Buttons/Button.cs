using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public enum ButtonType
{
    Default,
    Dashed,
    Primary,
    Link,
    Text
}

[PseudoClasses(ButtonPseudoClass.DefaultType,
    ButtonPseudoClass.DashedType,
    ButtonPseudoClass.PrimaryType,
    ButtonPseudoClass.LinkType,
    ButtonPseudoClass.TextType)]
public partial class Button : AbstractButton,
                      ICustomizableSizeTypeAware,
                      IWaveSpiritAwareControl,
                      ICompactSpaceAware,
                      IFormItemAware,
                      IFocusOutlineShapeProvider
{
    #region 公共属性定义

    public static readonly StyledProperty<ButtonType> ButtonTypeProperty =
        AvaloniaProperty.Register<Button, ButtonType>(nameof(ButtonType));

    public new static readonly StyledProperty<ButtonShape> ShapeProperty =
        AbstractButton.ShapeProperty.AddOwner<Button>();

    public new static readonly StyledProperty<bool> IsDangerProperty =
        AbstractButton.IsDangerProperty.AddOwner<Button>();

    public static readonly StyledProperty<bool> IsGhostProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(IsGhost));

    public new static readonly StyledProperty<bool> IsLoadingProperty =
        AbstractButton.IsLoadingProperty.AddOwner<Button>();

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Button>();

    public new static readonly StyledProperty<PathIcon?> IconProperty =
        AbstractButton.IconProperty.AddOwner<Button>();

    public static readonly StyledProperty<double> IconWidthProperty =
        AvaloniaProperty.Register<Button, double>(nameof(IconWidth));

    public static readonly StyledProperty<double> IconHeightProperty =
        AvaloniaProperty.Register<Button, double>(nameof(IconHeight));

    public new static readonly StyledProperty<ButtonIconPlacement> IconPlacementProperty =
        AbstractButton.IconPlacementProperty.AddOwner<Button>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Button>();

    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<Button>();

    public new static readonly StyledProperty<ButtonColor?> ColorProperty =
        AbstractButton.ColorProperty.AddOwner<Button>();

    public new static readonly StyledProperty<ButtonVariant?> VariantProperty =
        AbstractButton.VariantProperty.AddOwner<Button>();

    public ButtonType ButtonType
    {
        get => GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }

    public new ButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public new bool IsDanger
    {
        get => GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }

    public bool IsGhost
    {
        get => GetValue(IsGhostProperty);
        set => SetValue(IsGhostProperty, value);
    }

    public new bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public new PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public double IconWidth
    {
        get => GetValue(IconWidthProperty);
        set => SetValue(IconWidthProperty, value);
    }

    public double IconHeight
    {
        get => GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }

    public new ButtonIconPlacement IconPlacement
    {
        get => GetValue(IconPlacementProperty);
        set => SetValue(IconPlacementProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }

    public new ButtonColor? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

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

    internal static readonly StyledProperty<WaveSpiritType> WaveSpiritTypeProperty =
        WaveSpiritAwareControlProperty.WaveSpiritTypeProperty.AddOwner<Button>();
    
    internal static readonly DirectProperty<Button, CornerRadius> EffectiveCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<Button, CornerRadius>(nameof(EffectiveCornerRadius),
            o => o.EffectiveCornerRadius,
            (o, v) => o.EffectiveCornerRadius = v);
    
    /// <summary>
    /// Corner radius imposed by a hosting control that splits one visual frame over several Buttons (SplitButton). It
    /// replaces <see cref="TemplatedControl.CornerRadius"/> as the base of <see cref="EffectiveCornerRadius"/> so the
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
        AvaloniaProperty.RegisterDirect<Button, bool>(nameof(DisablesWithOpacity), button => button.DisablesWithOpacity);

    internal static readonly DirectProperty<Button, double> ThemeDisabledOpacityProperty =
        AvaloniaProperty.RegisterDirect<Button, double>(nameof(ThemeDisabledOpacity), button => button.ThemeDisabledOpacity);

    internal static readonly DirectProperty<Button, bool> PressMovesProperty =
        AvaloniaProperty.RegisterDirect<Button, bool>(nameof(PressMoves), button => button.PressMoves);

    internal static readonly DirectProperty<Button, bool> UnderlinesLinkOnHoverProperty =
        AvaloniaProperty.RegisterDirect<Button, bool>(nameof(UnderlinesLinkOnHover), button => button.UnderlinesLinkOnHover);

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
    /// Whether the icon slot is shown: an AtomUI <see cref="AtomUI.Controls.Icon"/> is set, the icon is allowed
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

    internal WaveSpiritType WaveSpiritType
    {
        get => GetValue(WaveSpiritTypeProperty);
        set => SetValue(WaveSpiritTypeProperty, value);
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

    // The recipe's colour model; see ButtonThemeRecipe.UsesRecipeVariants.
    internal bool UsesThemeRecipe
    {
        get => _usesThemeRecipe;
        private set => SetAndRaise(UsesThemeRecipeProperty, ref _usesThemeRecipe, value);
    }

    private bool _disablesWithOpacity;

    // Disabled buttons keep their colours at ThemeDisabledOpacity instead of the grey disabled paint.
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

    // Pressing moves the button instead of switching to the pressed colours.
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
    private WaveSpiritDecorator? _waveSpiritDecorator;
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

    protected virtual void NotifySetFormValue(object? value)
    {
    }

    protected virtual object? NotifyGetFormValue()
    {
        return null;
    }

    protected virtual void NotifyClearFormValue()
    {
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }

    #endregion

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _themeScopeSubscription?.Dispose();
        _themeScopeSubscription = s_themeTokenResolver.Subscribe(
            this,
            _ => ConfigureThemeAppearance());
        ConfigureThemeAppearance();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _themeScopeSubscription?.Dispose();
        _themeScopeSubscription = null;
        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _waveSpiritDecorator = e.NameScope.Find<WaveSpiritDecorator>("PART_WaveSpirit");
        UpdateButtonTypePseudoClasses();
        ConfigureWaveSpiritType();
        ConfigureEffectiveCornerRadius();
    }

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

        if (change.Property == IsPressedProperty)
        {
            HandlePressedChanged(change);
        }

        if (change.Property == FlyoutProperty)
        {
            PseudoClasses.Set(":popup-trigger", Flyout is not null);
        }

        if (change.Property == ButtonTypeProperty ||
            change.Property == ShapeProperty)
        {
            ConfigureWaveSpiritType();
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
        var recipe = ButtonThemeRecipe.Get(this);
        _themeRecipe = recipe;
        UsesThemeRecipe = recipe is { UsesRecipeVariants: true };
        DisablesWithOpacity   = recipe?.DisabledOpacity is not null;
        ThemeDisabledOpacity  = recipe?.DisabledOpacity ?? 1;
        PressMoves            = recipe is { PressMoves: true };
        UnderlinesLinkOnHover = recipe is { UnderlineLinkOnHover: true };
        PseudoClasses.Set(":link-underlined", recipe?.UnderlineLinkAtRest == true);
        ConfigureVariantThemeVariables();
    }

    private void HandlePressedChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!CanPlayWaveSpirit(change))
        {
            return;
        }

        Debug.Assert(_waveSpiritDecorator != null);
        Dispatcher.Post(() =>
        {
            ConfigureWaveSpiritBrush();
            _waveSpiritDecorator?.Play();
        });
    }

    private bool CanPlayWaveSpirit(AvaloniaPropertyChangedEventArgs change)
    {
        return !IsLoading &&
               IsMotionEnabled &&
               IsWaveSpiritEnabled &&
               (change.OldValue as bool? == true) &&
               IsWaveSpiritSupportedButtonType();
    }

    private bool IsWaveSpiritSupportedButtonType()
    {
        return EffectiveVariant == ButtonVariant.Solid ||
               EffectiveVariant == ButtonVariant.Outlined ||
               EffectiveVariant == ButtonVariant.Dashed;
    }

    private void ConfigureWaveSpiritBrush()
    {
        if (_waveSpiritDecorator is null)
        {
            return;
        }

        var waveBrush = ResolveWaveSpiritBrush();
        if (waveBrush is not null)
        {
            _waveSpiritDecorator.WaveBrush = waveBrush;
        }
        else
        {
            _waveSpiritDecorator.ClearValue(WaveSpiritDecorator.WaveBrushProperty);
        }
    }

    private IBrush? ResolveWaveSpiritBrush()
    {
        return WaveSpiritDecorator.ResolveWaveSpiritBrush(BorderBrush, Background);
    }

    private void ConfigureWaveSpiritType()
    {
        WaveSpiritType = Shape switch
        {
            ButtonShape.Round  => WaveSpiritType.PillWave,
            ButtonShape.Circle => WaveSpiritType.CircleWave,
            _                  => WaveSpiritType.RoundRectWave
        };
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

    protected override void OnEffectiveButtonStateChanged()
    {
        EffectiveIsDanger   = EffectiveColor == ButtonColor.Danger;
        EffectiveIsGhost    = IsGhost;
        EffectiveIsBordered = IsBorderedVariant(EffectiveVariant);

        ConfigureEffectiveBorderThickness();
        ConfigureVariantThemeVariables();
    }

    protected override ButtonVariant NormalizeVariant(ButtonVariant variant) =>
        IsGhost && variant == ButtonVariant.Solid ? ButtonVariant.Outlined : variant;

    protected override (ButtonColor Color, ButtonVariant Variant) ResolveFallbackColorAndVariant()
    {
        if (ButtonType != ButtonType.Default || IsDanger)
        {
            var variant = ButtonType switch
            {
                ButtonType.Primary => ButtonVariant.Solid,
                ButtonType.Dashed  => ButtonVariant.Dashed,
                ButtonType.Link    => ButtonVariant.Link,
                ButtonType.Text    => ButtonVariant.Text,
                _                  => ButtonVariant.Outlined
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
            _                  => ButtonColor.Default
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
