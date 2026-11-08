using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
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
using Avalonia.Media.Immutable;

namespace AtomUI.Desktop.Controls;

using AvaloniaButton = Avalonia.Controls.Button;

public enum ButtonType
{
    Default,
    Dashed,
    Primary,
    Link,
    Text
}

public enum ButtonShape
{
    Default,
    Circle,
    Round
}

public enum ButtonIconPlacement
{
    Start,
    End
}

public enum ButtonColor
{
    Default,
    Primary,
    Danger,
    Red,
    Volcano,
    Orange,
    Gold,
    Yellow,
    Lime,
    Green,
    Cyan,
    Blue,
    GeekBlue,
    Purple,
    Pink,
    Magenta,
    Grey
}

public enum ButtonVariant
{
    Outlined,
    Dashed,
    Solid,
    Filled,
    Text,
    Link
}

[PseudoClasses(ButtonPseudoClass.IconOnly,
    ButtonPseudoClass.Loading,
    ButtonPseudoClass.IsDanger,
    ButtonPseudoClass.DefaultType,
    ButtonPseudoClass.DashedType,
    ButtonPseudoClass.PrimaryType,
    ButtonPseudoClass.LinkType,
    ButtonPseudoClass.TextType)]
public partial class Button : AvaloniaButton,
                      ICustomizableSizeTypeAware,
                      IWaveSpiritAwareControl,
                      ICompactSpaceAware,
                      IFormItemAware,
                      IFocusOutlineShapeProvider
{
    #region 公共属性定义

    public static readonly StyledProperty<ButtonType> ButtonTypeProperty =
        AvaloniaProperty.Register<Button, ButtonType>(nameof(ButtonType));

    public static readonly StyledProperty<ButtonShape> ShapeProperty =
        AvaloniaProperty.Register<Button, ButtonShape>(nameof(Shape));

    public static readonly StyledProperty<bool> IsDangerProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(IsDanger));

    public static readonly StyledProperty<bool> IsGhostProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(IsGhost));

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(IsLoading));

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Button>();

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<Button, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<double> IconWidthProperty =
        AvaloniaProperty.Register<Button, double>(nameof(IconWidth));

    public static readonly StyledProperty<double> IconHeightProperty =
        AvaloniaProperty.Register<Button, double>(nameof(IconHeight));

    public static readonly StyledProperty<ButtonIconPlacement> IconPlacementProperty =
        AvaloniaProperty.Register<Button, ButtonIconPlacement>(
            nameof(IconPlacement),
            ButtonIconPlacement.Start);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Button>();

    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<Button>();

    public static readonly StyledProperty<ButtonColor?> ColorProperty =
        AvaloniaProperty.Register<Button, ButtonColor?>(nameof(Color));

    public static readonly StyledProperty<ButtonVariant?> VariantProperty =
        AvaloniaProperty.Register<Button, ButtonVariant?>(nameof(Variant));

    public ButtonType ButtonType
    {
        get => GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }

    public ButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public bool IsDanger
    {
        get => GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }

    public bool IsGhost
    {
        get => GetValue(IsGhostProperty);
        set => SetValue(IsGhostProperty, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public PathIcon? Icon
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

    public ButtonIconPlacement IconPlacement
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

    public ButtonColor? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public ButtonVariant? Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsIconVisibleProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(IsIconVisible), true);

    internal static readonly StyledProperty<object?> RightExtraContentProperty =
        AvaloniaProperty.Register<Button, object?>(nameof(RightExtraContent));

    internal static readonly StyledProperty<IDataTemplate?> RightExtraContentTemplateProperty =
        AvaloniaProperty.Register<Button, IDataTemplate?>(nameof(RightExtraContentTemplate));

    internal static readonly StyledProperty<Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.Register<Button, Thickness>(
            nameof(EffectiveBorderThickness));

    internal static readonly StyledProperty<ButtonColor> EffectiveColorProperty =
        AvaloniaProperty.Register<Button, ButtonColor>(
            nameof(EffectiveColor),
            ButtonColor.Default);

    internal static readonly StyledProperty<ButtonVariant> EffectiveVariantProperty =
        AvaloniaProperty.Register<Button, ButtonVariant>(
            nameof(EffectiveVariant),
            ButtonVariant.Outlined);

    internal static readonly StyledProperty<bool> EffectiveIsDangerProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(EffectiveIsDanger));

    internal static readonly StyledProperty<bool> EffectiveIsGhostProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(EffectiveIsGhost));

    internal static readonly StyledProperty<bool> EffectiveIsBorderedProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(EffectiveIsBordered), true);

    internal static readonly StyledProperty<IBrush?> VariantTextBrushProperty =
        AvaloniaProperty.Register<Button, IBrush?>(nameof(VariantTextBrush));

    internal static readonly StyledProperty<IBrush?> VariantTextHoverBrushProperty =
        AvaloniaProperty.Register<Button, IBrush?>(nameof(VariantTextHoverBrush));

    internal static readonly StyledProperty<IBrush?> VariantTextPressedBrushProperty =
        AvaloniaProperty.Register<Button, IBrush?>(nameof(VariantTextPressedBrush));

    internal static readonly StyledProperty<IBrush?> VariantBackgroundBrushProperty =
        AvaloniaProperty.Register<Button, IBrush?>(nameof(VariantBackgroundBrush));

    internal static readonly StyledProperty<IBrush?> VariantBackgroundHoverBrushProperty =
        AvaloniaProperty.Register<Button, IBrush?>(nameof(VariantBackgroundHoverBrush));

    internal static readonly StyledProperty<IBrush?> VariantBackgroundPressedBrushProperty =
        AvaloniaProperty.Register<Button, IBrush?>(nameof(VariantBackgroundPressedBrush));

    internal static readonly StyledProperty<IBrush?> VariantBorderBrushProperty =
        AvaloniaProperty.Register<Button, IBrush?>(nameof(VariantBorderBrush));

    internal static readonly StyledProperty<IBrush?> VariantBorderHoverBrushProperty =
        AvaloniaProperty.Register<Button, IBrush?>(nameof(VariantBorderHoverBrush));

    internal static readonly StyledProperty<IBrush?> VariantBorderPressedBrushProperty =
        AvaloniaProperty.Register<Button, IBrush?>(nameof(VariantBorderPressedBrush));

    internal static readonly StyledProperty<BoxShadows> VariantShadowProperty =
        AvaloniaProperty.Register<Button, BoxShadows>(nameof(VariantShadow));
    
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
    
    internal bool IsIconVisible
    {
        get => GetValue(IsIconVisibleProperty);
        set => SetValue(IsIconVisibleProperty, value);
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

    internal ButtonColor EffectiveColor
    {
        get => GetValue(EffectiveColorProperty);
        set => SetValue(EffectiveColorProperty, value);
    }

    internal ButtonVariant EffectiveVariant
    {
        get => GetValue(EffectiveVariantProperty);
        set => SetValue(EffectiveVariantProperty, value);
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

    internal IBrush? VariantTextBrush
    {
        get => GetValue(VariantTextBrushProperty);
        set => SetValue(VariantTextBrushProperty, value);
    }

    internal IBrush? VariantTextHoverBrush
    {
        get => GetValue(VariantTextHoverBrushProperty);
        set => SetValue(VariantTextHoverBrushProperty, value);
    }

    internal IBrush? VariantTextPressedBrush
    {
        get => GetValue(VariantTextPressedBrushProperty);
        set => SetValue(VariantTextPressedBrushProperty, value);
    }

    internal IBrush? VariantBackgroundBrush
    {
        get => GetValue(VariantBackgroundBrushProperty);
        set => SetValue(VariantBackgroundBrushProperty, value);
    }

    internal IBrush? VariantBackgroundHoverBrush
    {
        get => GetValue(VariantBackgroundHoverBrushProperty);
        set => SetValue(VariantBackgroundHoverBrushProperty, value);
    }

    internal IBrush? VariantBackgroundPressedBrush
    {
        get => GetValue(VariantBackgroundPressedBrushProperty);
        set => SetValue(VariantBackgroundPressedBrushProperty, value);
    }

    internal IBrush? VariantBorderBrush
    {
        get => GetValue(VariantBorderBrushProperty);
        set => SetValue(VariantBorderBrushProperty, value);
    }

    internal IBrush? VariantBorderHoverBrush
    {
        get => GetValue(VariantBorderHoverBrushProperty);
        set => SetValue(VariantBorderHoverBrushProperty, value);
    }

    internal IBrush? VariantBorderPressedBrush
    {
        get => GetValue(VariantBorderPressedBrushProperty);
        set => SetValue(VariantBorderPressedBrushProperty, value);
    }

    internal BoxShadows VariantShadow
    {
        get => GetValue(VariantShadowProperty);
        set => SetValue(VariantShadowProperty, value);
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
            _ => ConfigureVariantThemeVariables());
        ConfigureVariantThemeVariables();
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
        ConfigureEffectiveButtonState();
        UpdatePseudoClasses();
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
        if (change.Property == IsPressedProperty)
        {
            HandlePressedChanged(change);
        }

        if (change.Property == ButtonTypeProperty ||
            change.Property == ShapeProperty)
        {
            ConfigureWaveSpiritType();
        }

        if (ShouldConfigureEffectiveButtonState(change.Property))
        {
            ConfigureEffectiveButtonState();
        }

        if (ShouldUpdatePseudoClasses(change.Property))
        {
            UpdatePseudoClasses();
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
            ButtonShape.Default => WaveSpiritType.RoundRectWave,
            ButtonShape.Round   => WaveSpiritType.PillWave,
            ButtonShape.Circle  => WaveSpiritType.CircleWave,
            _                   => default
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

    private void ConfigureEffectiveButtonState()
    {
        var (color, variant) = ResolveEffectiveColorAndVariant();

        if (IsGhost && variant == ButtonVariant.Solid)
        {
            variant = ButtonVariant.Outlined;
        }

        EffectiveColor      = color;
        EffectiveVariant    = variant;
        EffectiveIsDanger   = color == ButtonColor.Danger;
        EffectiveIsGhost    = IsGhost;
        EffectiveIsBordered = IsBorderedVariant(variant);

        ConfigureEffectiveBorderThickness();
        ConfigureVariantThemeVariables();
    }

    private (ButtonColor Color, ButtonVariant Variant) ResolveEffectiveColorAndVariant()
    {
        if (Color is not null && Variant is not null)
        {
            return (Color.Value, Variant.Value);
        }

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
            _shapeCornerRadius ?? HostCornerRadius ?? CornerRadius,
            IsUsedInCompactSpace, 
            CompactSpaceItemPosition,
            CompactSpaceOrientation);
    }

    private bool ShouldUpdatePseudoClasses(AvaloniaProperty property)
    {
        return property == ContentProperty ||
               property == IsLoadingProperty ||
               property == ButtonTypeProperty ||
               property == IsDangerProperty;
    }

    private bool ShouldConfigureEffectiveButtonState(AvaloniaProperty property)
    {
        return property == ButtonTypeProperty ||
               property == IsDangerProperty ||
               property == IsGhostProperty ||
               property == ColorProperty ||
               property == VariantProperty;
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
               property == CompactSpaceItemPositionProperty ||
               property == CompactSpaceOrientationProperty;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ButtonPseudoClass.IconOnly, Icon is not null && Content is null);
        PseudoClasses.Set(ButtonPseudoClass.Loading, IsLoading);
        PseudoClasses.Set(ButtonPseudoClass.DefaultType, ButtonType == ButtonType.Default);
        PseudoClasses.Set(ButtonPseudoClass.DashedType, ButtonType == ButtonType.Dashed);
        PseudoClasses.Set(ButtonPseudoClass.PrimaryType, ButtonType == ButtonType.Primary);
        PseudoClasses.Set(ButtonPseudoClass.LinkType, ButtonType == ButtonType.Link);
        PseudoClasses.Set(ButtonPseudoClass.TextType, ButtonType == ButtonType.Text);
        PseudoClasses.Set(ButtonPseudoClass.IsDanger, IsDanger);
    }

}
