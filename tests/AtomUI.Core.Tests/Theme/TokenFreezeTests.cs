using AtomUI.Theme.DesignTokens;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// Freeze by name (spec 5.3): an explicit value is frozen before the derivation runs, and the generated setter ignores a
/// later write to a frozen token. A rejected token throws on write; accepted writes can be tracked.
/// </summary>
public class TokenFreezeTests
{
    [Fact]
    public void A_Frozen_Token_Keeps_Its_Value_When_The_Derivation_Writes_It()
    {
        var token = new DesignToken { ColorPrimaryHover = Colors.Red };
        token.Freeze(nameof(DesignToken.ColorPrimaryHover));

        token.ColorPrimaryHover = Colors.Blue;
        token.ColorPrimaryActive = Colors.Green;

        token.ColorPrimaryHover.ShouldBe(Colors.Red);
        token.ColorPrimaryActive.ShouldBe(Colors.Green);
        token.IsFrozenName(nameof(DesignToken.ColorPrimaryHover)).ShouldBeTrue();
        token.IsFrozenName(nameof(DesignToken.ColorPrimaryActive)).ShouldBeFalse();
    }

    [Fact]
    public void ClearFreeze_Lets_Every_Token_Be_Written_Again()
    {
        var token = new DesignToken { ColorPrimaryHover = Colors.Red };
        token.Freeze(nameof(DesignToken.ColorPrimaryHover));
        token.ClearFreeze();

        token.ColorPrimaryHover = Colors.Blue;

        token.ColorPrimaryHover.ShouldBe(Colors.Blue);
        token.IsFrozenName(nameof(DesignToken.ColorPrimaryHover)).ShouldBeFalse();
    }

    [Fact]
    public void Writing_A_Rejected_Token_Throws_With_The_Reason()
    {
        var token = new DesignToken();
        token.RejectWrites(new HashSet<string>([nameof(DesignToken.ColorPrimary)]), "seeds are read-only here.");

        var exception = Should.Throw<InvalidOperationException>(() => token.ColorPrimary = Colors.Red);

        exception.Message.ShouldBe("Token 'ColorPrimary' cannot be written here: seeds are read-only here.");
        token.ColorPrimaryHover = Colors.Red;
        token.ColorPrimaryHover.ShouldBe(Colors.Red);
    }

    [Fact]
    public void Tracking_Reports_Only_Accepted_Writes()
    {
        var token = new DesignToken();
        token.Freeze(nameof(DesignToken.ColorPrimaryHover));
        token.TrackWrites();

        token.ColorPrimaryHover = Colors.Blue;
        token.HasAcceptedWrites.ShouldBeFalse();

        token.ColorPrimaryActive = Colors.Blue;
        token.HasAcceptedWrites.ShouldBeTrue();
    }

    [Fact]
    public void A_Token_Without_A_Freeze_Accepts_Every_Write_And_Tracks_Nothing()
    {
        var token = new DesignToken();

        token.ColorPrimary = Colors.Red;

        token.ColorPrimary.ShouldBe(Colors.Red);
        token.HasAcceptedWrites.ShouldBeFalse();
    }
}
