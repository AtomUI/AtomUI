using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Themes.Shadcn;
using AtomUI.Theme.ContractTests;
using Avalonia.Headless.XUnit;
using Shouldly;

namespace AtomUI.DesignLanguages.Shadcn.Tests;

/// <summary>
/// shadcn with the values of every desktop control satisfies the design language contract (spec 14, 15): every
/// derivation writes every token it owns, is pure and deterministic, and the paired roles are readable, under all 32
/// variants (8 styles x light/dark x default/compact).
/// </summary>
public class ShadcnContractTests
{
    [AvaloniaFact]
    public void Shadcn_And_Its_Desktop_Value_Packages_Satisfy_The_Contract()
    {
        var input = DesignLanguageContractInput.Capture(static builder =>
        {
            builder.UseDesktopControls();
            builder.UseDesktopColorPicker();
            builder.UseDesktopDataGrid();
            builder.UseShadcn(static language => language.AddDesktopColorPicker().AddDesktopDataGrid());
        });

        var violations = DesignLanguageContract.Verify(input, "Shadcn");

        violations.ShouldBeEmpty(string.Join(Environment.NewLine,
            violations.GroupBy(static violation => (violation.Check, violation.TokenName))
                      .Select(static group => $"{group.Key.Check} {group.Key.TokenName} [{string.Join(", ", group.Select(static v => v.Variant))}]: {group.First().Message}")));
    }
}
