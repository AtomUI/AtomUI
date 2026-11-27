using AtomUI.Theme.Schema;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// A value package built against another token schema fails at startup.
/// </summary>
public class TokenSchemaDigestTests
{
    [Fact]
    public void A_Mismatch_Names_Both_Packages_And_Both_Digests()
    {
        var message = TokenSchemaDigests.Describe("Acme.Grid.Themes.Shadcn", "Acme.Grid", "aaaa", "bbbb");

        message.ShouldNotBeNull();
        message.ShouldContain("'Acme.Grid.Themes.Shadcn'");
        message.ShouldContain("'Acme.Grid'");
        message.ShouldContain("'aaaa'");
        message.ShouldContain("'bbbb'");
    }

    [Theory]
    [InlineData("aaaa", "aaaa")]
    [InlineData(null, "aaaa")]
    [InlineData("aaaa", null)]
    public void Matching_Or_Unrecorded_Digests_Pass(string? recorded, string? actual)
    {
        TokenSchemaDigests.Describe("Acme.Grid.Themes.Shadcn", "Acme.Grid", recorded, actual).ShouldBeNull();
    }

    [Fact]
    public void A_Package_That_Derives_Its_Own_Tokens_Is_Not_Checked()
    {
        Should.NotThrow(() => TokenSchemaDigests.Validate(typeof(TokenSchemaDigestTests).Assembly,
            typeof(TokenSchemaDigestTests).Assembly));
    }
}
