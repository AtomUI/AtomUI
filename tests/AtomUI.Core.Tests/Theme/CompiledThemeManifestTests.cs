using AtomUI.Theme.Definitions.Compiled;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// The link manifest of a compiled theme package: every requirement scope round-trips, language tokens included.
/// </summary>
public class CompiledThemeManifestTests
{
    [Fact]
    public void A_Language_Token_Requirement_Round_Trips()
    {
        var manifest = new CompiledThemeManifest
        {
            Requirements =
            [
                new CompiledThemeRequirement
                {
                    ThemeId = "Nova", Catalog = "$language", ControlId = "Shadcn", Scope = "language", TokenName = "Radius",
                    ValueType = "System.Double", ValueContractVersion = 1, Stage = 4
                },
                new CompiledThemeRequirement
                {
                    ThemeId = "Nova", Scope = "global", TokenName = "ColorPrimary", ValueType = "Avalonia.Media.Color",
                    ValueContractVersion = 1, Stage = 0, ResourceKeyType = "AtomUI.Theme.Resources.SharedTokenKind",
                    ResourceKeyMember = "ColorPrimary"
                }
            ]
        };

        var decoded = CompiledThemeManifest.Decode(manifest.Encode());

        decoded.Requirements.Length.ShouldBe(2);
        decoded.Requirements[0].Scope.ShouldBe("language");
        decoded.Requirements[0].ControlId.ShouldBe("Shadcn");
        decoded.Requirements[0].Stage.ShouldBe(4);
    }

    [Fact]
    public void A_Language_Requirement_Without_Its_Language_Is_Rejected()
    {
        var manifest = new CompiledThemeManifest
        {
            Requirements =
            [
                new CompiledThemeRequirement
                {
                    ThemeId = "Nova", Catalog = "", ControlId = "", Scope = "language", TokenName = "Radius",
                    ValueType = "System.Double", ValueContractVersion = 1, Stage = 4
                }
            ]
        };

        Should.Throw<InvalidDataException>(() => CompiledThemeManifest.Decode(manifest.Encode()));
    }
}
