using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using AvaloniaButton = Avalonia.Controls.Button;

namespace AtomUI.Controls;

[PseudoClasses(ButtonPseudoClass.IconOnly, ButtonPseudoClass.Loading, ButtonPseudoClass.IsDanger)]
public abstract class AbstractButton : AvaloniaButton
{
    public static readonly StyledProperty<ButtonShape> ShapeProperty =
        AvaloniaProperty.Register<AbstractButton, ButtonShape>(nameof(Shape));

    public static readonly StyledProperty<bool> IsDangerProperty =
        AvaloniaProperty.Register<AbstractButton, bool>(nameof(IsDanger));

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<AbstractButton, bool>(nameof(IsLoading));

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractButton, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<ButtonIconPlacement> IconPlacementProperty =
        AvaloniaProperty.Register<AbstractButton, ButtonIconPlacement>(nameof(IconPlacement), ButtonIconPlacement.Start);

    public static readonly StyledProperty<ButtonColor?> ColorProperty =
        AvaloniaProperty.Register<AbstractButton, ButtonColor?>(nameof(Color));

    public static readonly StyledProperty<ButtonVariant?> VariantProperty =
        AvaloniaProperty.Register<AbstractButton, ButtonVariant?>(nameof(Variant));

    internal static readonly StyledProperty<ButtonColor> EffectiveColorProperty =
        AvaloniaProperty.Register<AbstractButton, ButtonColor>(nameof(EffectiveColor), ButtonColor.Default);

    internal static readonly StyledProperty<ButtonVariant> EffectiveVariantProperty =
        AvaloniaProperty.Register<AbstractButton, ButtonVariant>(nameof(EffectiveVariant), ButtonVariant.Outlined);

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

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public ButtonIconPlacement IconPlacement
    {
        get => GetValue(IconPlacementProperty);
        set => SetValue(IconPlacementProperty, value);
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

    internal ButtonColor EffectiveColor => GetValue(EffectiveColorProperty);
    internal ButtonVariant EffectiveVariant => GetValue(EffectiveVariantProperty);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        PseudoClasses.Set(ButtonPseudoClass.IconOnly, Icon is not null && Content is null);
        PseudoClasses.Set(ButtonPseudoClass.Loading, IsLoading);
        PseudoClasses.Set(ButtonPseudoClass.IsDanger, IsDanger);
        RefreshEffectiveButtonState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty || change.Property == ContentProperty)
        {
            PseudoClasses.Set(ButtonPseudoClass.IconOnly, Icon is not null && Content is null);
        }
        else if (change.Property == IsLoadingProperty)
        {
            PseudoClasses.Set(ButtonPseudoClass.Loading, IsLoading);
        }

        if (change.Property == IsDangerProperty)
        {
            PseudoClasses.Set(ButtonPseudoClass.IsDanger, IsDanger);
        }

        if (change.Property == ColorProperty || change.Property == VariantProperty || change.Property == IsDangerProperty)
        {
            RefreshEffectiveButtonState();
        }
    }

    protected void RefreshEffectiveButtonState()
    {
        var (color, variant) = Color is { } explicitColor && Variant is { } explicitVariant
            ? (explicitColor, explicitVariant)
            : ResolveFallbackColorAndVariant();

        SetValue(EffectiveColorProperty, color);
        SetValue(EffectiveVariantProperty, NormalizeVariant(variant));
        OnEffectiveButtonStateChanged();
    }

    protected abstract (ButtonColor Color, ButtonVariant Variant) ResolveFallbackColorAndVariant();

    protected virtual ButtonVariant NormalizeVariant(ButtonVariant variant) => variant;

    // The product can project effective state into its own token resources and template.
    protected virtual void OnEffectiveButtonStateChanged()
    {
    }
}
