using AtomUI.Theme.Algorithms;
using AtomUI.Theme.ContractTests;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// Runs the reusable algorithm contract (spec 4.6) against the three built-in algorithms. Dark and Compact follow
/// another algorithm, so they are verified after <c>AtomUI:Default</c> and are exempt from the Map completeness check.
/// No built-in is exempted by identity: Compact (Preserve) is also compiled as <c>[Default, Compact, Dark]</c> by the
/// hybrid-chain readability check.
/// </summary>
public class BuiltInAlgorithmContractTests
{
    [Theory]
    [InlineData("Default")]
    [InlineData("Dark")]
    [InlineData("Compact")]
    public void BuiltIn_Algorithm_Satisfies_The_Contract(string id)
    {
        var violations = ThemeAlgorithmContract.Verify(
            ThemeAlgorithmContractInput.Capture(_ => { }),
            new ThemeAlgorithm("AtomUI", id),
            standalone: id == "Default");

        violations.ShouldBeEmpty();
    }
}
