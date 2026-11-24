using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Themes.PrimeNG;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.DesignLanguages.PrimeNG.Tests;

/// <summary>
/// The global tokens the nine PrimeNG controls whose previous control stage also rewrote seeds see in their own view
/// (the token dumps hold control tokens only): every global slot of the control's effective global token equals the
/// scope's global token after the preset's control-global overrides (Map and Alias only, spec 6.1), for every variant.
/// The colours the previous stages wrote as seeds are control tokens now; a few literal values pin them.
/// </summary>
public class PrimeNGControlGlobalTests
{
    private static readonly string[] s_controls =
    [
        "Button", "DropdownButton", "SubmitButton", "ResetButton", "Tag", "Alert", "MessageCard", "NotificationCard",
        "ToolTip"
    ];

    [AvaloniaTheory]
    [InlineData("Aura")]
    [InlineData("Lara")]
    [InlineData("Nora")]
    public async Task Seed_Writing_Controls_See_The_Preset_Overrides(string style)
    {
        foreach (var appearance in new[] { ThemeAppearance.Light, ThemeAppearance.Dark })
        {
            foreach (var density in new[] { DesignDensity.Default, DesignDensity.Compact })
            {
                foreach (var brand in new string?[] { null, "#10B981" })
                {
                    var snapshot = await Apply(style, appearance, density, brand);
                    var variant  = new DesignVariant(new DesignStyle(style), appearance, density, DesignIdiom.Desktop);
                    var case_    = $"{variant} {brand}";
                    foreach (var control in s_controls)
                    {
                        var expected = RootGlobal(snapshot);
                        var language = new PrimeNGToken();
                        new PrimeNGLanguage().DeriveLanguage(in variant, new DesignToken(), language);
                        EvaluateGlobal(control, new TokenEvaluationContext<PrimeNGToken>(variant, language), expected);
                        foreach (var descriptor in GlobalTokenSchema.Tokens)
                        {
                            descriptor.Format(Effective(snapshot, control, descriptor))
                                      .ShouldBe(descriptor.Format(descriptor.GetValue(expected)),
                                          $"{case_} {control} {descriptor.Name}");
                        }
                    }
                }
            }
        }
    }

    [AvaloniaFact]
    public async Task The_Preset_Colours_Reach_The_Control_Tokens()
    {
        var snapshot = await Apply("Aura", ThemeAppearance.Light, DesignDensity.Default, null);
        // The danger button colour is the red 500 stop, where the global error colour is Aura's red 400.
        Global<Color>(snapshot, nameof(DesignToken.ColorError)).ShouldBe(Color.Parse("#F87171"));
        Effective<Color>(snapshot, "Button", nameof(DesignToken.ColorError)).ShouldBe(Color.Parse("#F87171"));
        Own<Color>(snapshot, "Button", nameof(ButtonToken.DangerAccentColor)).ShouldBe(Color.Parse("#EF4444"));
        // Links of a button are drawn in the primary colour.
        Own<Color>(snapshot, "Button", nameof(ButtonToken.LinkColor)).ShouldBe(Global<Color>(snapshot, nameof(DesignToken.ColorPrimary)));
        // Aura's tinted success tag: green 700 text on green 100.
        Own<Color>(snapshot, "Tag", nameof(TagToken.SuccessColor)).ShouldBe(Color.Parse("#15803D"));
        Effective<Color>(snapshot, "Tag", nameof(DesignToken.ColorSuccessBg)).ShouldBe(Color.Parse("#DCFCE7"));
        // PrimeUIX tooltip.font.size 0.75rem and a box of the padded text (12 * 1.5 + 6 + 6).
        Own<double>(snapshot, "ToolTip", nameof(ToolTipToken.ToolTipFontSize)).ShouldBe(12);
        Own<double>(snapshot, "ToolTip", nameof(ToolTipToken.ToolTipMinHeight)).ShouldBe(30);

        var lara = await Apply("Lara", ThemeAppearance.Light, DesignDensity.Default, null);
        // Lara tooltip.font.size 0.625rem, padding 0.5rem: 10 * 1.5 + 8 + 8.
        Own<double>(lara, "ToolTip", nameof(ToolTipToken.ToolTipFontSize)).ShouldBe(10);
        Own<double>(lara, "ToolTip", nameof(ToolTipToken.ToolTipMinHeight)).ShouldBe(31);
    }

    private static T Own<T>(ThemeSnapshot snapshot, string controlId, string tokenName)
    {
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", controlId), out var control)
                .ShouldBeTrue(controlId);
        var descriptor = control!.OwnTokens.Single(token => token.Name == tokenName);
        return (T)snapshot.Controls[control.Slot].ControlTokenValues.GetValue(descriptor.Slot)!;
    }

    private static void EvaluateGlobal(string control, in TokenEvaluationContext<PrimeNGToken> context, DesignToken global)
    {
        switch (control)
        {
            case "Button": PrimeNGButtonFamilyTokens<ButtonToken>.EvaluateGlobal(in context, global); break;
            case "DropdownButton": PrimeNGButtonFamilyTokens<DropdownButtonToken>.EvaluateGlobal(in context, global); break;
            case "SubmitButton": PrimeNGButtonFamilyTokens<SubmitButtonToken>.EvaluateGlobal(in context, global); break;
            case "ResetButton": PrimeNGButtonFamilyTokens<ResetButtonToken>.EvaluateGlobal(in context, global); break;
            case "Tag": PrimeNGTagTokens.EvaluateGlobal(in context, global); break;
            case "Alert": PrimeNGAlertTokens.EvaluateGlobal(in context, global); break;
            case "MessageCard": PrimeNGMessageCardTokens.EvaluateGlobal(in context, global); break;
            case "NotificationCard": PrimeNGNotificationCardTokens.EvaluateGlobal(in context, global); break;
            case "ToolTip": PrimeNGToolTipTokens.EvaluateGlobal(in context, global); break;
            default: throw new ArgumentOutOfRangeException(nameof(control), control, null);
        }
    }

    private static DesignToken RootGlobal(ThemeSnapshot snapshot)
    {
        var token = new DesignToken();
        foreach (var descriptor in GlobalTokenSchema.Tokens)
        {
            descriptor.SetValue(token, snapshot.GlobalTokenValues.GetValue(descriptor.Slot));
        }
        return token;
    }

    private static object? Effective(ThemeSnapshot snapshot, string controlId, TokenDescriptor descriptor)
    {
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", controlId), out var control)
                .ShouldBeTrue(controlId);
        var delta = snapshot.Controls[control!.Slot].EffectiveGlobalTokenDelta;
        return delta.TryGetValue(descriptor.Slot, out var value) ? value : snapshot.GlobalTokenValues.GetValue(descriptor.Slot);
    }

    private static T Effective<T>(ThemeSnapshot snapshot, string controlId, string tokenName) =>
        (T)Effective(snapshot, controlId, GlobalTokenSchema.Get(tokenName))!;

    private static T Global<T>(ThemeSnapshot snapshot, string tokenName) =>
        (T)snapshot.GlobalTokenValues.GetValue(GlobalTokenSchema.Get(tokenName).Slot)!;

    private static async Task<ThemeSnapshot> Apply(string style, ThemeAppearance appearance, DesignDensity density,
                                                   string? brand)
    {
        var manager   = (ThemeManager)Application.Current!.GetThemeManager()!;
        var selection = ThemeSelection.Of("PrimeNG", new DesignStyle(style)).WithAppearance(appearance).WithDensity(density);
        if (brand is not null)
        {
            selection = selection.WithSeed(nameof(DesignToken.ColorPrimary), brand);
        }
        var request = ThemeSelectionResolver.Resolve(manager.Languages, manager.Themes, selection).Request;
        var result  = await manager.ApplyThemeAsync(request, TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
        return manager.CurrentSnapshot!;
    }
}
