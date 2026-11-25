using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.DesignLanguages.PrimeNG.Tests;

/// <summary>
/// The behaviour switches PrimeNG selects through ordinary control tokens: buttons are disabled by opacity (0.6) and
/// underline links on hover; the split button separator, the input frame and the float button outline keep their
/// defaults.
/// </summary>
public class PrimeNGBehaviorTokenTests
{
    private static readonly string[] s_buttonOwners = ["Button", "DropdownButton", "SubmitButton", "ResetButton"];

    [AvaloniaTheory]
    [InlineData("Aura")]
    [InlineData("Lara")]
    [InlineData("Nora")]
    public async Task PrimeNG_Disables_By_Opacity_And_Underlines_Links_On_Hover(string style)
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        var request = ThemeSelectionResolver.Resolve(manager.Languages, manager.Themes,
            ThemeSelection.Of("PrimeNG", new DesignStyle(style))).Request;
        var result = await manager.ApplyThemeAsync(request, TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
        var snapshot = manager.CurrentSnapshot!;

        foreach (var owner in s_buttonOwners)
        {
            Own<bool>(snapshot, owner, "RecipeVariants").ShouldBeFalse(owner);
            Own<bool>(snapshot, owner, "OutlineOnlyDefaultShadow").ShouldBeFalse(owner);
            Own<bool>(snapshot, owner, "DisabledByOpacity").ShouldBeTrue(owner);
            Own<double>(snapshot, owner, "DisabledOpacity").ShouldBe(0.6, owner);
            Own<bool>(snapshot, owner, "PressMoves").ShouldBeFalse(owner);
            Own<bool>(snapshot, owner, "UnderlineLinkOnHover").ShouldBeTrue(owner);
            Own<bool>(snapshot, owner, "UnderlineLinkAtRest").ShouldBeFalse(owner);
        }

        Own<bool>(snapshot, "SplitButton", "InsetSeparator").ShouldBeFalse();
        foreach (var owner in new[] { "LineEdit", "TextArea", "Select" })
        {
            Own<bool>(snapshot, owner, "TransparentOutlineBorder").ShouldBeFalse(owner);
            Own<bool>(snapshot, owner, "BottomOutlineBorder").ShouldBeFalse(owner);
        }
        Own<bool>(snapshot, "Select", "MutedSelectIndicator").ShouldBeFalse();
        Own<bool>(snapshot, "FloatButton", "HasOutline").ShouldBeFalse();
        Own<bool>(snapshot, "FloatButton", "OutlineOnlyShadow").ShouldBeFalse();
        Own<bool>(snapshot, "FloatButton", "CapsuleDescription").ShouldBeFalse();
        Own<bool>(snapshot, "FloatButton", "UppercaseDescription").ShouldBeFalse();
        Own<FontWeight>(snapshot, "FloatButton", "DescriptionWeight").ShouldBe(FontWeight.Normal);
        Own<double>(snapshot, "FloatButton", "OutlineDescriptionLineHeight").ShouldBe(0);
    }

    private static T Own<T>(ThemeSnapshot snapshot, string controlId, string tokenName)
    {
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", controlId), out var control)
                .ShouldBeTrue(controlId);
        var token = control!.OwnTokens.Single(item => item.Name == tokenName);
        return snapshot.Controls[control.Slot].ControlTokenValues.Get<T>(token.Slot);
    }
}
