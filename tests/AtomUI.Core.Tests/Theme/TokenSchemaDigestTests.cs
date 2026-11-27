using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
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

    /// <summary>
    /// Verifies generated control token rules validate their build-time schema digest against the loaded control assembly.
    /// </summary>
    [Fact]
    public void Generated_Rules_Compare_The_Build_Digest_With_The_Loaded_Token_Digest()
    {
        var matching = ControlTokenRuleDescriptor.Create<ProbeLanguageToken, DigestToken, DigestRule>("loaded");
        var stale = ControlTokenRuleDescriptor.Create<ProbeLanguageToken, DigestToken, DigestRule>("recorded");

        Should.NotThrow(matching.ValidateTokenSchema);
        var failure = Should.Throw<ThemeSchemaException>(stale.ValidateTokenSchema);
        failure.Message.ShouldContain("'recorded'");
        failure.Message.ShouldContain("'loaded'");
    }

    private sealed class DigestToken : AbstractControlDesignToken, IControlToken
    {
        public static ControlTokenIdentity ControlIdentity { get; } = new("Test", "Digest");
        static string IControlToken.TokenSchemaDigest => "loaded";
    }

    private sealed class DigestRule : IControlTokenRule<ProbeLanguageToken, DigestToken>
    {
        public void Apply(in TokenEvaluationContext<ProbeLanguageToken> context, ControlTokenTarget<DigestToken> target) =>
            throw new InvalidOperationException("Schema validation must not run the rule.");
    }
}
