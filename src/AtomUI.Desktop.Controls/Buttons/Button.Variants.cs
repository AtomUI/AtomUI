using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
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
/// Token-backed variables are bound to the same resource keys the ControlTheme's <c>ButtonTokenResource</c> setters use
/// (boxed <see cref="ButtonTokenKind"/> for Own Tokens, the owner's Effective Global key for global Tokens). Application,
/// scoped and <c>ThemeConfigProvider</c> resources therefore reach the variant colours exactly as they reach every other
/// Button Token, and the bound values follow resource changes without a recompute. Global keys use the registered
/// identity of the instance (DropdownButton has its own), Own Tokens stay Button's. Preset colours are not Tokens: they
/// come from the captured snapshot's palettes and are recomputed when the theme publishes.
/// </remarks>
public partial class Button
{
    private const int VariantSlotCount = 10;
    private static readonly IBrush TransparentBrush = new ImmutableSolidColorBrush(Colors.Transparent);
    private static readonly VariantValue TransparentVariant = new(TransparentBrush, false);
    private static readonly VariantValue NoShadowVariant = new(new BoxShadows(), false);
    private static readonly Func<object?, object?> s_toVariantBrush = ToVariantBrush;

    // Theme reads happen on the UI thread only.
    private static readonly Dictionary<ButtonTokenKind, object> s_ownVariantKeys = new();
    private static readonly Dictionary<(ControlTokenIdentity Identity, SharedTokenKind Kind), object> s_globalVariantKeys = new();

    private readonly VariantSlot[] _variantSlots = new VariantSlot[VariantSlotCount];

    private void ConfigureVariantThemeVariables()
    {
        var tokens = new VariantTokens(this);
        if (EffectiveColor == ButtonColor.Default)
        {
            ConfigureDefaultVariantThemeVariables(tokens);
        }
        else if (EffectiveColor == ButtonColor.Primary)
        {
            ConfigureSemanticVariantThemeVariables(
                tokens.Global(SharedTokenKind.ColorPrimary),
                tokens.Global(SharedTokenKind.ColorPrimaryHover),
                tokens.Global(SharedTokenKind.ColorPrimaryActive),
                tokens.Global(SharedTokenKind.ColorPrimaryBg),
                tokens.Global(SharedTokenKind.ColorPrimaryBgHover),
                tokens.Global(SharedTokenKind.ColorPrimaryBorder),
                VariantTokens.Own(ButtonTokenKind.PrimaryColor),
                VariantTokens.Own(ButtonTokenKind.PrimaryShadow));
        }
        else if (EffectiveColor == ButtonColor.Danger)
        {
            ConfigureDangerVariantThemeVariables(tokens);
        }
        else if (TryGetPresetPrimaryColor(EffectiveColor, out var presetColor) &&
                 TryCaptureSnapshotTokens(out var snapshotTokens))
        {
            var colors = snapshotTokens.GetPresetPalette(presetColor).ColorSequence;
            ConfigureSemanticVariantThemeVariables(
                VariantValue.Of(colors[5]),
                VariantValue.Of(colors[4]),
                VariantValue.Of(colors[6]),
                VariantValue.Of(colors[0]),
                VariantValue.Of(colors[1]),
                VariantValue.Of(colors[2]),
                VariantTokens.Own(ButtonTokenKind.SolidTextColor),
                VariantValue.Of(CreatePresetShadow(snapshotTokens, colors[0])));
        }
    }

    private void ConfigureDangerVariantThemeVariables(VariantTokens tokens)
    {
        var dangerColor      = VariantTokens.Own(ButtonTokenKind.DangerColor);
        var dangerShadow     = EffectiveIsGhost ? NoShadowVariant : VariantTokens.Own(ButtonTokenKind.DangerShadow);
        var colorError       = tokens.Global(SharedTokenKind.ColorError);
        var colorErrorHover  = tokens.Global(SharedTokenKind.ColorErrorHover);
        var colorErrorActive = tokens.Global(SharedTokenKind.ColorErrorActive);
        switch (EffectiveVariant)
        {
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
                    EffectiveIsGhost ? TransparentVariant : VariantTokens.Own(ButtonTokenKind.DefaultBg),
                    EffectiveIsGhost ? TransparentVariant : VariantTokens.Own(ButtonTokenKind.DefaultHoverBg),
                    EffectiveIsGhost ? TransparentVariant : VariantTokens.Own(ButtonTokenKind.DefaultActiveBg),
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
                var solidText = VariantTokens.Own(ButtonTokenKind.SolidTextColor);
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
                    EffectiveIsGhost ? NoShadowVariant : VariantTokens.Own(ButtonTokenKind.DefaultShadow));
                break;
            }
            case ButtonVariant.Outlined:
            case ButtonVariant.Dashed:
                if (EffectiveIsGhost)
                {
                    SetVariantThemeVariables(
                        VariantTokens.Own(ButtonTokenKind.DefaultGhostColor),
                        VariantTokens.Own(ButtonTokenKind.DefaultHoverColor),
                        VariantTokens.Own(ButtonTokenKind.DefaultActiveColor),
                        VariantTokens.Own(ButtonTokenKind.GhostBg),
                        VariantTokens.Own(ButtonTokenKind.GhostBg),
                        VariantTokens.Own(ButtonTokenKind.GhostBg),
                        VariantTokens.Own(ButtonTokenKind.DefaultGhostBorderColor),
                        VariantTokens.Own(ButtonTokenKind.DefaultHoverBorderColor),
                        VariantTokens.Own(ButtonTokenKind.DefaultActiveBorderColor),
                        NoShadowVariant);
                }
                else
                {
                    SetVariantThemeVariables(
                        VariantTokens.Own(ButtonTokenKind.DefaultColor),
                        VariantTokens.Own(ButtonTokenKind.DefaultHoverColor),
                        VariantTokens.Own(ButtonTokenKind.DefaultActiveColor),
                        VariantTokens.Own(ButtonTokenKind.DefaultBg),
                        VariantTokens.Own(ButtonTokenKind.DefaultHoverBg),
                        VariantTokens.Own(ButtonTokenKind.DefaultActiveBg),
                        VariantTokens.Own(ButtonTokenKind.DefaultBorderColor),
                        VariantTokens.Own(ButtonTokenKind.DefaultHoverBorderColor),
                        VariantTokens.Own(ButtonTokenKind.DefaultActiveBorderColor),
                        VariantTokens.Own(ButtonTokenKind.DefaultShadow));
                }
                break;
            case ButtonVariant.Filled:
            {
                var text = VariantTokens.Own(ButtonTokenKind.DefaultColor);
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
                    VariantTokens.Own(ButtonTokenKind.TextTextColor),
                    VariantTokens.Own(ButtonTokenKind.TextTextHoverColor),
                    VariantTokens.Own(ButtonTokenKind.TextTextActiveColor),
                    TransparentVariant,
                    VariantTokens.Own(ButtonTokenKind.TextHoverBg),
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
                    tokens.Global(SharedTokenKind.ColorLinkActive),
                    TransparentVariant,
                    VariantTokens.Own(ButtonTokenKind.LinkHoverBg),
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    TransparentVariant,
                    NoShadowVariant);
                break;
        }
    }

    private void ConfigureSemanticVariantThemeVariables(
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
                var background = EffectiveIsGhost ? TransparentVariant : VariantTokens.Own(ButtonTokenKind.DefaultBg);
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
        SetVariantSlot(0, VariantTextBrushProperty, text, s_toVariantBrush);
        SetVariantSlot(1, VariantTextHoverBrushProperty, textHover, s_toVariantBrush);
        SetVariantSlot(2, VariantTextPressedBrushProperty, textPressed, s_toVariantBrush);
        SetVariantSlot(3, VariantBackgroundBrushProperty, background, s_toVariantBrush);
        SetVariantSlot(4, VariantBackgroundHoverBrushProperty, backgroundHover, s_toVariantBrush);
        SetVariantSlot(5, VariantBackgroundPressedBrushProperty, backgroundPressed, s_toVariantBrush);
        SetVariantSlot(6, VariantBorderBrushProperty, border, s_toVariantBrush);
        SetVariantSlot(7, VariantBorderHoverBrushProperty, borderHover, s_toVariantBrush);
        SetVariantSlot(8, VariantBorderPressedBrushProperty, borderPressed, s_toVariantBrush);
        SetVariantSlot(9, VariantShadowProperty, shadow, null);
    }

    // Re-running the projection (property changes, theme publications) keeps an unchanged slot as it is: its resource
    // binding already follows the Token, so nothing is rebound or reallocated.
    private void SetVariantSlot(int index, AvaloniaProperty property, VariantValue value, Func<object?, object?>? converter)
    {
        ref var slot = ref _variantSlots[index];
        if (slot.Value is { } current && current.Equals(value))
        {
            return;
        }

        slot.Binding?.Dispose();
        slot.Binding = null;
        slot.Value   = value;
        if (value.IsResource)
        {
            slot.Binding = TokenResourceBinder.CreateTokenResourceBinding(
                this, this, property, value.Source, BindingPriority.LocalValue, converter);
        }
        else
        {
            SetValue(property, value.Source);
        }
    }

    private static object? ToVariantBrush(object? value)
    {
        return value is Color color ? ToBrush(color) : value;
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

        accessor = this is DropdownButton
            ? ControlTokenAccessor.Capture<DropdownButton>(this)
            : ControlTokenAccessor.Capture<Button>(this);
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

    private struct VariantSlot
    {
        internal VariantValue? Value;
        internal IDisposable? Binding;
    }

    /// <summary>Resource keys of the Tokens a variant reads, matching the ControlTheme's ButtonTokenResource keys.</summary>
    private readonly struct VariantTokens
    {
        private readonly ControlTokenIdentity _identity;

        internal VariantTokens(Button owner)
        {
            // Registered product families keep separate Effective Global tables; Own Tokens are not replaced by
            // inheritance and stay Button's.
            _identity = owner is DropdownButton ? DropdownButtonTokens.Identity : ButtonTokens.Identity;
        }

        internal VariantValue Global(SharedTokenKind kind)
        {
            if (!s_globalVariantKeys.TryGetValue((_identity, kind), out var key))
            {
                key = ControlTokenResourceKey.Global(_identity, kind);
                s_globalVariantKeys.Add((_identity, kind), key);
            }

            return new VariantValue(key, true);
        }

        internal static VariantValue Own(ButtonTokenKind kind)
        {
            if (!s_ownVariantKeys.TryGetValue(kind, out var key))
            {
                key = kind;
                s_ownVariantKeys.Add(kind, key);
            }

            return new VariantValue(key, true);
        }
    }
}
