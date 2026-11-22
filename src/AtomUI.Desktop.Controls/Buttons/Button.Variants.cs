using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Variant theme variables: the text, background, border and shadow values a Button shows per state, chosen from
/// <see cref="EffectiveColor"/> and <see cref="EffectiveVariant"/>.
/// </summary>
/// <remarks>
/// Token-backed variables are read from the same resource keys the ControlTheme's <c>ButtonTokenResource</c> setters use
/// (boxed <see cref="ButtonTokenKind"/> for Own Tokens, the owner's Effective Global key for global Tokens). Application,
/// scoped and <c>ThemeConfigProvider</c> resources therefore reach the variant colours exactly as they reach every other
/// Button Token. One set observes the active resource slots and refreshes only the values that change. Global keys use the registered
/// identity of the instance (DropdownButton has its own), Own Tokens use that same terminal identity. Preset colours are not Tokens: they
/// come from the captured snapshot's palettes and are recomputed when the theme publishes.
/// </remarks>
public partial class Button
{
    private const int VariantSlotCount = 10;
    private const int VariantShadowSlot = 9;
    private static readonly IBrush TransparentBrush = new ImmutableSolidColorBrush(Colors.Transparent);
    private static readonly VariantValue TransparentVariant = new(TransparentBrush, false);
    private static readonly VariantValue NoShadowVariant = new(new BoxShadows(), false);
    private static readonly VariantValue NoPressedPaint = new(AvaloniaProperty.UnsetValue, false);

    // Theme reads happen on the UI thread only.
    private static readonly Dictionary<(ControlTokenIdentity Identity, ButtonTokenKind Kind), object> s_ownVariantKeys = new();
    private static readonly Dictionary<(ControlTokenIdentity Identity, ButtonTokenKind Kind, SharedTokenKind? Fallback), object> s_variantOverrideKeys = new();
    private static readonly Dictionary<(ControlTokenIdentity Identity, SharedTokenKind Kind), object> s_globalVariantKeys = new();
    private static readonly Dictionary<object, object> s_explicitPressedKeys = new();

    private readonly VariantValue?[] _variantSlots = new VariantValue?[VariantSlotCount];
    private OwnerTokenSet? _variantTokens;
    private int _configuringVariantSlot = -1;

    // The recipe only changes with the theme snapshot, so ConfigureThemeAppearance resolves it and every
    // variant pass reads the cached value.
    private ButtonThemeRecipe? _themeRecipe;

    private void ConfigureVariantThemeVariables()
    {
        var tokens = new VariantTokens(this, _themeRecipe);
        if (EffectiveColor == ButtonColor.Default ||
            (EffectiveColor == ButtonColor.Primary && EffectiveVariant == ButtonVariant.Link &&
             tokens.Recipe is { UsesRecipeVariants: true }))
        {
            ConfigureDefaultVariantThemeVariables(tokens);
        }
        else if (EffectiveColor == ButtonColor.Primary)
        {
            ConfigureSemanticVariantThemeVariables(
                tokens,
                tokens.Global(SharedTokenKind.ColorPrimary),
                tokens.Global(SharedTokenKind.ColorPrimaryHover),
                tokens.Global(SharedTokenKind.ColorPrimaryActive),
                tokens.Global(SharedTokenKind.ColorPrimaryBg),
                tokens.Global(SharedTokenKind.ColorPrimaryBgHover),
                tokens.Global(SharedTokenKind.ColorPrimaryBorder),
                tokens.Own(ButtonTokenKind.PrimaryColor),
                tokens.Own(ButtonTokenKind.PrimaryShadow));
        }
        else if (EffectiveColor == ButtonColor.Danger)
        {
            ConfigureDangerVariantThemeVariables(tokens);
        }
        else if (EffectiveColor == ButtonColor.Success)
        {
            ConfigureStatusVariantThemeVariables(
                tokens,
                SharedTokenKind.ColorSuccess,
                SharedTokenKind.ColorSuccessHover,
                SharedTokenKind.ColorSuccessActive,
                SharedTokenKind.ColorSuccessBg,
                SharedTokenKind.ColorSuccessBgHover,
                SharedTokenKind.ColorSuccessBorder);
        }
        else if (EffectiveColor == ButtonColor.Warning)
        {
            ConfigureStatusVariantThemeVariables(
                tokens,
                SharedTokenKind.ColorWarning,
                SharedTokenKind.ColorWarningHover,
                SharedTokenKind.ColorWarningActive,
                SharedTokenKind.ColorWarningBg,
                SharedTokenKind.ColorWarningBgHover,
                SharedTokenKind.ColorWarningBorder);
        }
        else if (TryGetPresetPrimaryColor(EffectiveColor, out var presetColor) &&
                 TryCaptureSnapshotTokens(out var snapshotTokens))
        {
            var colors = snapshotTokens.GetPresetPalette(presetColor).ColorSequence;
            ConfigureSemanticVariantThemeVariables(
                tokens,
                VariantValue.Of(colors[5]),
                VariantValue.Of(colors[4]),
                VariantValue.Of(colors[6]),
                VariantValue.Of(colors[0]),
                VariantValue.Of(colors[1]),
                VariantValue.Of(colors[2]),
                tokens.Own(ButtonTokenKind.SolidTextColor),
                tokens.Recipe is not { UsesRecipeVariants: true }
                    ? VariantValue.Of(CreatePresetShadow(snapshotTokens, colors[0]))
                    : NoShadowVariant);
        }
    }

    /// <summary>
    /// Success and Warning follow the semantic variant layout with Global status Tokens. Their solid shadow is derived
    /// from the light status colour, so it is a snapshot value recomputed when the theme publishes.
    /// </summary>
    private void ConfigureStatusVariantThemeVariables(
        VariantTokens tokens,
        SharedTokenKind color,
        SharedTokenKind hoverColor,
        SharedTokenKind activeColor,
        SharedTokenKind bgColor,
        SharedTokenKind bgHoverColor,
        SharedTokenKind borderColor)
    {
        var shadow = tokens.Recipe is not { UsesRecipeVariants: true } && TryCaptureSnapshotTokens(out var snapshotTokens)
            ? VariantValue.Of(CreatePresetShadow(snapshotTokens, snapshotTokens.GetEffectiveGlobal<Color>(bgColor)))
            : NoShadowVariant;
        ConfigureSemanticVariantThemeVariables(
            tokens,
            tokens.Global(color),
            tokens.Global(hoverColor),
            tokens.Global(activeColor),
            tokens.Global(bgColor),
            tokens.Global(bgHoverColor),
            tokens.Global(borderColor),
            tokens.Own(ButtonTokenKind.SolidTextColor),
            shadow);
    }

    private void ConfigureDangerVariantThemeVariables(VariantTokens tokens)
    {
        var dangerColor      = tokens.Own(ButtonTokenKind.DangerColor);
        var dangerShadow     = EffectiveIsGhost ? NoShadowVariant : tokens.Own(ButtonTokenKind.DangerShadow);
        var colorError       = tokens.Global(SharedTokenKind.ColorError);
        var colorErrorHover  = tokens.Global(SharedTokenKind.ColorErrorHover);
        var colorErrorActive = tokens.Global(SharedTokenKind.ColorErrorActive);
        switch (EffectiveVariant)
        {
            case ButtonVariant.Solid when tokens.Recipe is { UsesRecipeVariants: true }:
                SetVariantThemeVariables(
                    dangerColor,
                    dangerColor,
                    dangerColor,
                    tokens.Global(SharedTokenKind.ColorErrorBg),
                    tokens.Global(SharedTokenKind.ColorErrorBgHover),
                    colorErrorActive,
                    TransparentVariant,
                    TransparentVariant,
                    colorErrorActive,
                    dangerShadow);
                break;
            case ButtonVariant.Solid:
                SetVariantThemeVariables(
                    dangerColor,
                    dangerColor,
                    dangerColor,
                    colorError,
                    colorErrorHover,
                    colorErrorActive,
                    colorError,
                    colorErrorHover,
                    colorErrorActive,
                    dangerShadow);
                break;
            case ButtonVariant.Outlined:
            case ButtonVariant.Dashed:
                SetVariantThemeVariables(
                    colorError,
                    tokens.Global(SharedTokenKind.ColorErrorBorderHover),
                    colorErrorActive,
                    EffectiveIsGhost ? TransparentVariant : tokens.Own(ButtonTokenKind.DefaultBg),
                    EffectiveIsGhost ? TransparentVariant : tokens.Own(ButtonTokenKind.DefaultHoverBg),
                    EffectiveIsGhost ? TransparentVariant : tokens.Own(ButtonTokenKind.DefaultActiveBg),
                    colorError,
                    tokens.Global(SharedTokenKind.ColorErrorBorderHover),
                    colorErrorActive,
                    dangerShadow);
                break;
            case ButtonVariant.Filled:
                SetVariantThemeVariables(
                    colorError,
                    colorError,
                    colorError,
                    tokens.Global(SharedTokenKind.ColorErrorBg),
                    tokens.Global(SharedTokenKind.ColorErrorBgHover),
                    tokens.Global(SharedTokenKind.ColorErrorBgActive),
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    NoShadowVariant);
                break;
            case ButtonVariant.Text:
                SetVariantThemeVariables(
                    colorError,
                    colorErrorHover,
                    colorErrorActive,
                    TransparentVariant,
                    tokens.Global(SharedTokenKind.ColorErrorBg),
                    tokens.Global(SharedTokenKind.ColorErrorBgActive),
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    NoShadowVariant);
                break;
            case ButtonVariant.Link:
                SetVariantThemeVariables(
                    colorError,
                    colorErrorHover,
                    colorErrorActive,
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    NoShadowVariant);
                break;
        }
    }

    private void ConfigureDefaultVariantThemeVariables(VariantTokens tokens)
    {
        switch (EffectiveVariant)
        {
            case ButtonVariant.Solid:
            {
                var solidText = tokens.Own(ButtonTokenKind.SolidTextColor);
                SetVariantThemeVariables(
                    solidText,
                    solidText,
                    solidText,
                    tokens.Global(SharedTokenKind.ColorBgSolid),
                    tokens.Global(SharedTokenKind.ColorBgSolidHover),
                    tokens.Global(SharedTokenKind.ColorBgSolidActive),
                    tokens.Global(SharedTokenKind.ColorBgSolid),
                    tokens.Global(SharedTokenKind.ColorBgSolidHover),
                    tokens.Global(SharedTokenKind.ColorBgSolidActive),
                    EffectiveIsGhost ? NoShadowVariant : GetDefaultShadow(tokens));
                break;
            }
            case ButtonVariant.Outlined:
            case ButtonVariant.Dashed:
                if (EffectiveIsGhost)
                {
                    SetVariantThemeVariables(
                        tokens.Own(ButtonTokenKind.DefaultGhostColor),
                        tokens.Own(ButtonTokenKind.DefaultHoverColor),
                        tokens.Own(ButtonTokenKind.DefaultActiveColor),
                        tokens.Own(ButtonTokenKind.GhostBg),
                        tokens.Own(ButtonTokenKind.GhostBg),
                        tokens.Own(ButtonTokenKind.GhostBg),
                        tokens.Own(ButtonTokenKind.DefaultGhostBorderColor),
                        tokens.Own(ButtonTokenKind.DefaultHoverBorderColor),
                        tokens.Own(ButtonTokenKind.DefaultActiveBorderColor),
                        NoShadowVariant);
                }
                else
                {
                    SetVariantThemeVariables(
                        tokens.Own(ButtonTokenKind.DefaultColor),
                        tokens.Own(ButtonTokenKind.DefaultHoverColor),
                        tokens.Own(ButtonTokenKind.DefaultActiveColor),
                        tokens.Own(ButtonTokenKind.DefaultBg),
                        tokens.Own(ButtonTokenKind.DefaultHoverBg),
                        tokens.Own(ButtonTokenKind.DefaultActiveBg),
                        tokens.Own(ButtonTokenKind.DefaultBorderColor),
                        tokens.Own(ButtonTokenKind.DefaultHoverBorderColor),
                        tokens.Own(ButtonTokenKind.DefaultActiveBorderColor),
                        GetDefaultShadow(tokens));
                }
                break;
            case ButtonVariant.Filled:
            {
                var text = tokens.Recipe is not { UsesRecipeVariants: true }
                    ? tokens.Own(ButtonTokenKind.DefaultColor)
                    : tokens.OwnOverride(ButtonTokenKind.DefaultColor, SharedTokenKind.ControlItemColorActive);
                SetVariantThemeVariables(
                    text,
                    text,
                    text,
                    tokens.Global(SharedTokenKind.ColorFillTertiary),
                    tokens.Global(SharedTokenKind.ColorFillSecondary),
                    tokens.Global(SharedTokenKind.ColorFill),
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    NoShadowVariant);
                break;
            }
            case ButtonVariant.Text:
                SetVariantThemeVariables(
                    tokens.Own(ButtonTokenKind.TextTextColor),
                    tokens.Own(ButtonTokenKind.TextTextHoverColor),
                    tokens.Own(ButtonTokenKind.TextTextActiveColor),
                    TransparentVariant,
                    tokens.Own(ButtonTokenKind.TextHoverBg),
                    tokens.Global(SharedTokenKind.ColorFill),
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    NoShadowVariant);
                break;
            case ButtonVariant.Link:
                SetVariantThemeVariables(
                    tokens.Global(SharedTokenKind.ColorLink),
                    tokens.Global(SharedTokenKind.ColorLinkHover),
                    tokens.Global(EffectiveColor == ButtonColor.Primary
                        ? SharedTokenKind.ColorPrimaryActive
                        : SharedTokenKind.ColorLinkActive),
                    TransparentVariant,
                    tokens.Own(ButtonTokenKind.LinkHoverBg),
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    NoShadowVariant);
                break;
        }
    }

    private void ConfigureSemanticVariantThemeVariables(
        VariantTokens tokens,
        VariantValue baseColor,
        VariantValue hoverColor,
        VariantValue activeColor,
        VariantValue lightColor,
        VariantValue lightHoverColor,
        VariantValue lightActiveColor,
        VariantValue solidTextColor,
        VariantValue shadow)
    {
        switch (EffectiveVariant)
        {
            case ButtonVariant.Solid:
                SetVariantThemeVariables(
                    solidTextColor,
                    solidTextColor,
                    solidTextColor,
                    baseColor,
                    hoverColor,
                    activeColor,
                    baseColor,
                    hoverColor,
                    activeColor,
                    EffectiveIsGhost ? NoShadowVariant : shadow);
                break;
            case ButtonVariant.Outlined:
            case ButtonVariant.Dashed:
            {
                var background = EffectiveIsGhost ? TransparentVariant : tokens.Own(ButtonTokenKind.DefaultBg);
                SetVariantThemeVariables(
                    baseColor,
                    hoverColor,
                    activeColor,
                    background,
                    background,
                    background,
                    baseColor,
                    hoverColor,
                    activeColor,
                    EffectiveIsGhost ? NoShadowVariant : shadow);
                break;
            }
            case ButtonVariant.Filled:
                SetVariantThemeVariables(
                    baseColor,
                    baseColor,
                    baseColor,
                    lightColor,
                    lightHoverColor,
                    lightActiveColor,
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    NoShadowVariant);
                break;
            case ButtonVariant.Text:
                SetVariantThemeVariables(
                    baseColor,
                    hoverColor,
                    activeColor,
                    TransparentVariant,
                    lightColor,
                    lightActiveColor,
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    NoShadowVariant);
                break;
            case ButtonVariant.Link:
                SetVariantThemeVariables(
                    baseColor,
                    hoverColor,
                    activeColor,
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    NoShadowVariant);
                break;
        }
    }

    private VariantValue GetDefaultShadow(VariantTokens tokens)
    {
        if (EffectiveVariant != ButtonVariant.Outlined && tokens.Recipe is { OutlineOnlyDefaultShadow: true })
        {
            return tokens.OwnOverride(ButtonTokenKind.DefaultShadow);
        }
        return tokens.Own(ButtonTokenKind.DefaultShadow);
    }

    private void SetVariantThemeVariables(
        VariantValue text,
        VariantValue textHover,
        VariantValue textPressed,
        VariantValue background,
        VariantValue backgroundHover,
        VariantValue backgroundPressed,
        VariantValue border,
        VariantValue borderHover,
        VariantValue borderPressed,
        VariantValue shadow)
    {
        // The recipe's press feedback is movement. Keep documented palette states for preset colors
        // and preserve explicit pressed paint on each channel without replacing the other channels.
        if (PressMoves && !TryGetPresetPrimaryColor(EffectiveColor, out _))
        {
            textPressed = ExplicitPressedPaint(textPressed, text, textHover);
            backgroundPressed = ExplicitPressedPaint(backgroundPressed, background, backgroundHover);
            borderPressed = ExplicitPressedPaint(borderPressed, border, borderHover);
        }

        var appearance = VariantAppearance;
        SetVariantSlot(0, text, ref appearance);
        SetVariantSlot(1, textHover, ref appearance);
        SetVariantSlot(2, textPressed, ref appearance);
        SetVariantSlot(3, background, ref appearance);
        SetVariantSlot(4, backgroundHover, ref appearance);
        SetVariantSlot(5, backgroundPressed, ref appearance);
        SetVariantSlot(6, border, ref appearance);
        SetVariantSlot(7, borderHover, ref appearance);
        SetVariantSlot(8, borderPressed, ref appearance);
        SetVariantSlot(VariantShadowSlot, shadow, ref appearance);
        VariantAppearance = appearance;
    }

    private static VariantValue ExplicitPressedPaint(VariantValue pressed, VariantValue normal, VariantValue hover)
    {
        if (!pressed.IsResource || pressed == normal || pressed == hover)
        {
            return NoPressedPaint;
        }

        if (!s_explicitPressedKeys.TryGetValue(pressed.Source, out var key))
        {
            key = new ThemeTokenResourceProvider.ExplicitTokenLookup(pressed.Source);
            s_explicitPressedKeys.Add(pressed.Source, key);
        }

        return new VariantValue(key, true);
    }

    private void SetVariantSlot(int index, VariantValue value, ref ButtonVariantAppearance appearance)
    {
        ref var slot = ref _variantSlots[index];
        if (slot is { } current && current.Equals(value))
        {
            return;
        }

        if (value.IsResource)
        {
            _variantTokens ??= new OwnerTokenSet(this, VariantSlotCount, VariantResourceChanged);
        }

        object? resolved = value.Source;
        if (_variantTokens is { } tokens)
        {
            var previousIndex = _configuringVariantSlot;
            _configuringVariantSlot = index;
            try
            {
                tokens.SetResourceKey(index, value.IsResource ? value.Source : null);
                if (value.IsResource)
                {
                    resolved = tokens.GetValue(index);
                }
            }
            finally
            {
                _configuringVariantSlot = previousIndex;
            }
        }

        var replacesResource = slot is { IsResource: true };
        slot = value;
        ApplyVariantValue(index, replacesResource && ReferenceEquals(resolved, BindingOperations.DoNothing)
            ? AvaloniaProperty.UnsetValue
            : resolved, ref appearance);
    }

    private void VariantResourceChanged(int index, object? previous, object? value)
    {
        if (_configuringVariantSlot != index && _variantSlots[index] is { IsResource: true })
        {
            var appearance = VariantAppearance;
            ApplyVariantValue(index, value, ref appearance);
            VariantAppearance = appearance;
        }
    }

    private void ApplyVariantValue(int index, object? value, ref ButtonVariantAppearance appearance)
    {
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return;
        }
        if (index == VariantShadowSlot)
        {
            appearance = appearance with { Shadow = value is BoxShadows shadows ? shadows : default };
            return;
        }

        var brush = value switch
        {
            IBrush resolved => resolved,
            Color color => ToBrush(color),
            _ => null
        };
        appearance = index switch
        {
            0 => appearance with { Text = brush },
            1 => appearance with { TextHover = brush },
            2 => appearance with { TextPressed = brush },
            3 => appearance with { Background = brush },
            4 => appearance with { BackgroundHover = brush },
            5 => appearance with { BackgroundPressed = brush },
            6 => appearance with { Border = brush },
            7 => appearance with { BorderHover = brush },
            8 => appearance with { BorderPressed = brush },
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }

    private static IBrush ToBrush(Color color)
    {
        return color == Colors.Transparent
            ? TransparentBrush
            : new ImmutableSolidColorBrush(color);
    }

    private bool TryCaptureSnapshotTokens(out ControlTokenAccessor accessor)
    {
        // Off-tree buttons (no themed root yet) get their preset colours once the theme subscription attaches a context.
        if (ThemeScope.ResolveContext(this) is null)
        {
            accessor = default;
            return false;
        }

        accessor = this switch
        {
            DropdownButton => ControlTokenAccessor.Capture<DropdownButton>(this),
            SubmitButton => ControlTokenAccessor.Capture<SubmitButton>(this),
            ResetButton => ControlTokenAccessor.Capture<ResetButton>(this),
            _ => ControlTokenAccessor.Capture<Button>(this)
        };
        return true;
    }

    private static BoxShadows CreatePresetShadow(ControlTokenAccessor tokens, Color lightColor)
    {
        return new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = tokens.GetEffectiveGlobal<double>(SharedTokenKind.ControlOutlineWidth),
            Blur    = 3,
            Spread  = 0,
            Color = ColorUtils.CalculateAlphaColor(
                lightColor,
                tokens.GetEffectiveGlobal<Color>(SharedTokenKind.ColorBgContainer))
        });
    }

    /// <summary>A variant value: a Token resource key to bind, or a fixed value.</summary>
    private readonly record struct VariantValue(object Source, bool IsResource)
    {
        internal static VariantValue Of(Color color) => new(ToBrush(color), false);

        internal static VariantValue Of(BoxShadows shadows) => new(shadows, false);
    }

    /// <summary>Resource keys of the Tokens a variant reads, matching the ControlTheme's ButtonTokenResource keys.</summary>
    private readonly struct VariantTokens
    {
        private readonly ControlTokenIdentity _identity;

        internal VariantTokens(Button owner, ButtonThemeRecipe? recipe)
        {
            // Variant globals and Own values use the same terminal identity.
            _identity = ButtonThemeRecipe.OwnerOf(owner);
            Recipe = recipe;
        }

        internal ButtonThemeRecipe? Recipe { get; }

        internal VariantValue Global(SharedTokenKind kind)
        {
            if (!s_globalVariantKeys.TryGetValue((_identity, kind), out var key))
            {
                key = ControlTokenResourceKey.Global(_identity, kind);
                s_globalVariantKeys.Add((_identity, kind), key);
            }

            return new VariantValue(key, true);
        }

        internal VariantValue OwnOverride(ButtonTokenKind kind, SharedTokenKind? fallback = null)
        {
            var identity = (_identity, kind, fallback);
            if (!s_variantOverrideKeys.TryGetValue(identity, out var key))
            {
                key = new ThemeTokenResourceProvider.ExplicitTokenLookup(
                    Own(kind).Source,
                    fallback is { } global ? Global(global).Source : null);
                s_variantOverrideKeys.Add(identity, key);
            }

            return new VariantValue(key, true);
        }

        internal VariantValue Own(ButtonTokenKind kind)
        {
            if (!s_ownVariantKeys.TryGetValue((_identity, kind), out var key))
            {
                key = _identity == DropdownButtonTokens.Identity ? kind switch
                {
                    ButtonTokenKind.DangerColor => DropdownButtonTokenKind.DangerColor,
                    ButtonTokenKind.DangerShadow => DropdownButtonTokenKind.DangerShadow,
                    ButtonTokenKind.DefaultActiveBg => DropdownButtonTokenKind.DefaultActiveBg,
                    ButtonTokenKind.DefaultActiveBorderColor => DropdownButtonTokenKind.DefaultActiveBorderColor,
                    ButtonTokenKind.DefaultActiveColor => DropdownButtonTokenKind.DefaultActiveColor,
                    ButtonTokenKind.DefaultBg => DropdownButtonTokenKind.DefaultBg,
                    ButtonTokenKind.DefaultBorderColor => DropdownButtonTokenKind.DefaultBorderColor,
                    ButtonTokenKind.DefaultColor => DropdownButtonTokenKind.DefaultColor,
                    ButtonTokenKind.DefaultGhostBorderColor => DropdownButtonTokenKind.DefaultGhostBorderColor,
                    ButtonTokenKind.DefaultGhostColor => DropdownButtonTokenKind.DefaultGhostColor,
                    ButtonTokenKind.DefaultHoverBg => DropdownButtonTokenKind.DefaultHoverBg,
                    ButtonTokenKind.DefaultHoverBorderColor => DropdownButtonTokenKind.DefaultHoverBorderColor,
                    ButtonTokenKind.DefaultHoverColor => DropdownButtonTokenKind.DefaultHoverColor,
                    ButtonTokenKind.DefaultShadow => DropdownButtonTokenKind.DefaultShadow,
                    ButtonTokenKind.GhostBg => DropdownButtonTokenKind.GhostBg,
                    ButtonTokenKind.LinkHoverBg => DropdownButtonTokenKind.LinkHoverBg,
                    ButtonTokenKind.PrimaryColor => DropdownButtonTokenKind.PrimaryColor,
                    ButtonTokenKind.PrimaryShadow => DropdownButtonTokenKind.PrimaryShadow,
                    ButtonTokenKind.SolidTextColor => DropdownButtonTokenKind.SolidTextColor,
                    ButtonTokenKind.TextHoverBg => DropdownButtonTokenKind.TextHoverBg,
                    ButtonTokenKind.TextTextActiveColor => DropdownButtonTokenKind.TextTextActiveColor,
                    ButtonTokenKind.TextTextColor => DropdownButtonTokenKind.TextTextColor,
                    ButtonTokenKind.TextTextHoverColor => DropdownButtonTokenKind.TextTextHoverColor,
                    _ => throw new ArgumentOutOfRangeException(nameof(kind))
                } : _identity == SubmitButtonTokens.Identity ? kind switch
                {
                    ButtonTokenKind.DangerColor => SubmitButtonTokenKind.DangerColor,
                    ButtonTokenKind.DangerShadow => SubmitButtonTokenKind.DangerShadow,
                    ButtonTokenKind.DefaultActiveBg => SubmitButtonTokenKind.DefaultActiveBg,
                    ButtonTokenKind.DefaultActiveBorderColor => SubmitButtonTokenKind.DefaultActiveBorderColor,
                    ButtonTokenKind.DefaultActiveColor => SubmitButtonTokenKind.DefaultActiveColor,
                    ButtonTokenKind.DefaultBg => SubmitButtonTokenKind.DefaultBg,
                    ButtonTokenKind.DefaultBorderColor => SubmitButtonTokenKind.DefaultBorderColor,
                    ButtonTokenKind.DefaultColor => SubmitButtonTokenKind.DefaultColor,
                    ButtonTokenKind.DefaultGhostBorderColor => SubmitButtonTokenKind.DefaultGhostBorderColor,
                    ButtonTokenKind.DefaultGhostColor => SubmitButtonTokenKind.DefaultGhostColor,
                    ButtonTokenKind.DefaultHoverBg => SubmitButtonTokenKind.DefaultHoverBg,
                    ButtonTokenKind.DefaultHoverBorderColor => SubmitButtonTokenKind.DefaultHoverBorderColor,
                    ButtonTokenKind.DefaultHoverColor => SubmitButtonTokenKind.DefaultHoverColor,
                    ButtonTokenKind.DefaultShadow => SubmitButtonTokenKind.DefaultShadow,
                    ButtonTokenKind.GhostBg => SubmitButtonTokenKind.GhostBg,
                    ButtonTokenKind.LinkHoverBg => SubmitButtonTokenKind.LinkHoverBg,
                    ButtonTokenKind.PrimaryColor => SubmitButtonTokenKind.PrimaryColor,
                    ButtonTokenKind.PrimaryShadow => SubmitButtonTokenKind.PrimaryShadow,
                    ButtonTokenKind.SolidTextColor => SubmitButtonTokenKind.SolidTextColor,
                    ButtonTokenKind.TextHoverBg => SubmitButtonTokenKind.TextHoverBg,
                    ButtonTokenKind.TextTextActiveColor => SubmitButtonTokenKind.TextTextActiveColor,
                    ButtonTokenKind.TextTextColor => SubmitButtonTokenKind.TextTextColor,
                    ButtonTokenKind.TextTextHoverColor => SubmitButtonTokenKind.TextTextHoverColor,
                    _ => throw new ArgumentOutOfRangeException(nameof(kind))
                } : _identity == ResetButtonTokens.Identity ? kind switch
                {
                    ButtonTokenKind.DangerColor => ResetButtonTokenKind.DangerColor,
                    ButtonTokenKind.DangerShadow => ResetButtonTokenKind.DangerShadow,
                    ButtonTokenKind.DefaultActiveBg => ResetButtonTokenKind.DefaultActiveBg,
                    ButtonTokenKind.DefaultActiveBorderColor => ResetButtonTokenKind.DefaultActiveBorderColor,
                    ButtonTokenKind.DefaultActiveColor => ResetButtonTokenKind.DefaultActiveColor,
                    ButtonTokenKind.DefaultBg => ResetButtonTokenKind.DefaultBg,
                    ButtonTokenKind.DefaultBorderColor => ResetButtonTokenKind.DefaultBorderColor,
                    ButtonTokenKind.DefaultColor => ResetButtonTokenKind.DefaultColor,
                    ButtonTokenKind.DefaultGhostBorderColor => ResetButtonTokenKind.DefaultGhostBorderColor,
                    ButtonTokenKind.DefaultGhostColor => ResetButtonTokenKind.DefaultGhostColor,
                    ButtonTokenKind.DefaultHoverBg => ResetButtonTokenKind.DefaultHoverBg,
                    ButtonTokenKind.DefaultHoverBorderColor => ResetButtonTokenKind.DefaultHoverBorderColor,
                    ButtonTokenKind.DefaultHoverColor => ResetButtonTokenKind.DefaultHoverColor,
                    ButtonTokenKind.DefaultShadow => ResetButtonTokenKind.DefaultShadow,
                    ButtonTokenKind.GhostBg => ResetButtonTokenKind.GhostBg,
                    ButtonTokenKind.LinkHoverBg => ResetButtonTokenKind.LinkHoverBg,
                    ButtonTokenKind.PrimaryColor => ResetButtonTokenKind.PrimaryColor,
                    ButtonTokenKind.PrimaryShadow => ResetButtonTokenKind.PrimaryShadow,
                    ButtonTokenKind.SolidTextColor => ResetButtonTokenKind.SolidTextColor,
                    ButtonTokenKind.TextHoverBg => ResetButtonTokenKind.TextHoverBg,
                    ButtonTokenKind.TextTextActiveColor => ResetButtonTokenKind.TextTextActiveColor,
                    ButtonTokenKind.TextTextColor => ResetButtonTokenKind.TextTextColor,
                    ButtonTokenKind.TextTextHoverColor => ResetButtonTokenKind.TextTextHoverColor,
                    _ => throw new ArgumentOutOfRangeException(nameof(kind))
                } : kind;
                s_ownVariantKeys.Add((_identity, kind), key);
            }

            return new VariantValue(key, true);
        }
    }
}

internal readonly record struct ButtonVariantAppearance(
    IBrush? Text,
    IBrush? TextHover,
    IBrush? TextPressed,
    IBrush? Background,
    IBrush? BackgroundHover,
    IBrush? BackgroundPressed,
    IBrush? Border,
    IBrush? BorderHover,
    IBrush? BorderPressed,
    BoxShadows Shadow);
