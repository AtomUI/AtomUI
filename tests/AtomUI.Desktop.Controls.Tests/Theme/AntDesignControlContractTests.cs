using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Desktop.Controls.Themes.AntDesign;
using AtomUI.Theme.ContractTests;
using Avalonia.Headless.XUnit;
using Shouldly;

namespace AtomUI.Desktop.Controls.Tests.Theme;

/// <summary>
/// Ant Design with the values of every desktop control satisfies the design language contract (spec 14, 15): each
/// control derivation writes every token of its control, is pure and deterministic, under all 8 variants.
/// </summary>
public class AntDesignControlContractTests
{
    [AvaloniaFact]
    public void AntDesign_And_Its_Desktop_Value_Packages_Satisfy_The_Contract()
    {
        var input = DesignLanguageContractInput.Capture(static builder =>
        {
            builder.UseDesktopControls();
            builder.UseDesktopColorPicker();
            builder.UseDesktopDataGrid();
            builder.UseAntDesign(static language => language.AddDesktopColorPicker().AddDesktopDataGrid());
        });

        var violations = DesignLanguageContract.Verify(input, "AntDesign");

        // One line per broken rule and token; the variants it breaks under follow.
        violations.ShouldBeEmpty(string.Join(Environment.NewLine,
            violations.GroupBy(static violation => (violation.Check, violation.TokenName))
                      .Select(static group => $"{group.Key.Check} {group.Key.TokenName} [{string.Join(", ", group.Select(static v => v.Variant))}]: {group.First().Message}")));
    }
}
