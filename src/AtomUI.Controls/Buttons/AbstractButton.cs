using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using AvaloniaButton = Avalonia.Controls.Button;

namespace AtomUI.Controls;

/// <summary>
/// Base class of buttons that show an icon next to their content and are painted by color and variant.
/// It raises the <c>icononly</c>, <c>loading</c> and <c>danger</c> pseudo-classes from the property values and
/// resolves the effective color and variant, asking the derived class for them when <see cref="Color" /> or
/// <see cref="Variant" /> is <see langword="null" />.
/// </summary>
[PseudoClasses(ButtonPseudoClass.IconOnly, ButtonPseudoClass.Loading, ButtonPseudoClass.IsDanger)]
public abstract class AbstractButton : AvaloniaButton
{
    /// <summary>
    /// Identifies the <see cref="Shape" /> property.
    /// </summary>
    public static readonly StyledProperty<ButtonShape> ShapeProperty =
        AvaloniaProperty.Register<AbstractButton, ButtonShape>(nameof(Shape));

    /// <summary>
    /// Identifies the <see cref="IsDanger" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDangerProperty =
        AvaloniaProperty.Register<AbstractButton, bool>(nameof(IsDanger));

    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<AbstractButton, bool>(nameof(IsLoading));

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractButton, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="IconPlacement" /> property.
    /// </summary>
    public static readonly StyledProperty<ButtonIconPlacement> IconPlacementProperty =
        AvaloniaProperty.Register<AbstractButton, ButtonIconPlacement>(nameof(IconPlacement), ButtonIconPlacement.Start);

    /// <summary>
    /// Identifies the <see cref="Color" /> property.
    /// </summary>
    public static readonly StyledProperty<ButtonColor?> ColorProperty =
        AvaloniaProperty.Register<AbstractButton, ButtonColor?>(nameof(Color));

    /// <summary>
    /// Identifies the <see cref="Variant" /> property.
    /// </summary>
    public static readonly StyledProperty<ButtonVariant?> VariantProperty =
        AvaloniaProperty.Register<AbstractButton, ButtonVariant?>(nameof(Variant));

    /// <summary>
    /// Identifies the <see cref="EffectiveColor" /> property.
    /// </summary>
    internal static readonly StyledProperty<ButtonColor> EffectiveColorProperty =
        AvaloniaProperty.Register<AbstractButton, ButtonColor>(nameof(EffectiveColor), ButtonColor.Default);

    /// <summary>
    /// Identifies the <see cref="EffectiveVariant" /> property.
    /// </summary>
    internal static readonly StyledProperty<ButtonVariant> EffectiveVariantProperty =
        AvaloniaProperty.Register<AbstractButton, ButtonVariant>(nameof(EffectiveVariant), ButtonVariant.Outlined);

    /// <summary>
    /// Shape of the button frame. <see cref="ButtonShape.Rectangular" /> gives square corners,
    /// <see cref="ButtonShape.Circle" /> measures square and <see cref="ButtonShape.Round" /> measures at least one and
    /// a half times as wide as high. Circle and Round take their corner radius from the measured height, and Circle
    /// pads with the circle padding of the theme instead of the content padding.
    /// </summary>
    public ButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    /// <summary>
    /// Marks the button as destructive. While it is set the button raises the <c>danger</c> pseudo-class, and every
    /// change re-resolves the effective color and variant, so <see cref="ResolveFallbackColorAndVariant" /> can answer
    /// <see cref="ButtonColor.Danger" /> for a danger button.
    /// </summary>
    public bool IsDanger
    {
        get => GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }

    /// <summary>
    /// Switches the button to its loading look. It raises the <c>loading</c> pseudo-class, with which the default
    /// themes replace the icon with a spinning loading icon and lower the opacity. The button stays enabled.
    /// </summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// Icon shown next to the content, or <see langword="null" /> for no icon. The theme sizes it with
    /// <c>IconWidth</c> and <c>IconHeight</c> and paints it with the foreground. An icon without content raises the
    /// <c>icononly</c> pseudo-class, with which the themes apply the icon-only padding.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Side of the content the icon is placed on. The default theme docks the icon to the left of the content for
    /// <see cref="ButtonIconPlacement.Start" /> and to the right for <see cref="ButtonIconPlacement.End" />, each with
    /// its own icon margin. When unset or cleared, the value falls back to <see cref="ButtonIconPlacement.Start" />.
    /// </summary>
    public ButtonIconPlacement IconPlacement
    {
        get => GetValue(IconPlacementProperty);
        set => SetValue(IconPlacementProperty, value);
    }

    /// <summary>
    /// Color scheme of the button, or <see langword="null" /> to let the derived control choose. It is used only when
    /// <see cref="Variant" /> is also set; if either is <see langword="null" />, the derived control resolves both
    /// through <see cref="ResolveFallbackColorAndVariant" />.
    /// </summary>
    public ButtonColor? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>
    /// Painting style of the button, or <see langword="null" /> to let the derived control choose. It is used only when
    /// <see cref="Color" /> is also set; if either is <see langword="null" />, the derived control resolves both
    /// through <see cref="ResolveFallbackColorAndVariant" />. <see cref="NormalizeVariant" /> is applied to the variant
    /// in both cases.
    /// </summary>
    public ButtonVariant? Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>
    /// The color the themes paint with: <see cref="Color" /> when it and <see cref="Variant" /> are set, otherwise the
    /// fallback from <see cref="ResolveFallbackColorAndVariant" />. Written by
    /// <see cref="RefreshEffectiveButtonState" />.
    /// </summary>
    internal ButtonColor EffectiveColor => GetValue(EffectiveColorProperty);
    /// <summary>
    /// The variant the themes paint with: <see cref="Variant" /> or the fallback, after <see cref="NormalizeVariant" />
    /// was applied. Written by <see cref="RefreshEffectiveButtonState" />.
    /// </summary>
    internal ButtonVariant EffectiveVariant => GetValue(EffectiveVariantProperty);

    /// <summary>
    /// Applies the <c>icononly</c>, <c>loading</c> and <c>danger</c> pseudo-classes from the current property values
    /// and refreshes the effective color and variant. Derived overrides must call the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        PseudoClasses.Set(ButtonPseudoClass.IconOnly, Icon is not null && Content is null);
        PseudoClasses.Set(ButtonPseudoClass.Loading, IsLoading);
        PseudoClasses.Set(ButtonPseudoClass.IsDanger, IsDanger);
        RefreshEffectiveButtonState();
    }

    /// <summary>
    /// Updates the pseudo-classes when <see cref="Icon" />, <see cref="ContentControl.Content" />, <see
    /// cref="IsLoading" /> or <see cref="IsDanger" /> changes, and refreshes the effective color and variant when <see
    /// cref="Color" />, <see cref="Variant" /> or <see cref="IsDanger" /> changes. Derived overrides must call the base
    /// member.
    /// </summary>
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

    /// <summary>
    /// Stores the effective color and variant: <see cref="Color" /> and <see cref="Variant" /> when both are set,
    /// otherwise the result of <see cref="ResolveFallbackColorAndVariant" />, with <see cref="NormalizeVariant" />
    /// applied to the variant. Then calls <see cref="OnEffectiveButtonStateChanged" />. Call it again when the state
    /// that the fallback depends on changes.
    /// </summary>
    protected void RefreshEffectiveButtonState()
    {
        var (color, variant) = Color is { } explicitColor && Variant is { } explicitVariant
            ? (explicitColor, explicitVariant)
            : ResolveFallbackColorAndVariant();

        SetValue(EffectiveColorProperty, color);
        SetValue(EffectiveVariantProperty, NormalizeVariant(variant));
        OnEffectiveButtonStateChanged();
    }

    /// <summary>
    /// Returns the color and variant to use while <see cref="Color" /> or <see cref="Variant" /> is
    /// <see langword="null" />. It is called by <see cref="RefreshEffectiveButtonState" />.
    /// </summary>
    /// <returns>
    /// The fallback color in <c>Color</c> and the fallback variant in <c>Variant</c>.
    /// </returns>
    protected abstract (ButtonColor Color, ButtonVariant Variant) ResolveFallbackColorAndVariant();

    /// <summary>
    /// Adjusts a resolved variant before it is stored as the effective variant. The base member returns
    /// <paramref name="variant" /> unchanged; override it to replace a variant that the derived button shows
    /// differently.
    /// </summary>
    /// <param name="variant">
    /// The variant that was resolved from <see cref="Variant" /> or from the fallback.
    /// </param>
    protected virtual ButtonVariant NormalizeVariant(ButtonVariant variant) => variant;

    /// <summary>
    /// Called at the end of <see cref="RefreshEffectiveButtonState" />, after it stored the effective color and
    /// variant, whether or not they differ from the previous values. Override it to project the effective state into
    /// the control's own resources and template; the base member does nothing.
    /// </summary>
    protected virtual void OnEffectiveButtonStateChanged()
    {
    }
}
