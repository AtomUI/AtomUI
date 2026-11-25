using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ThemeConfigImmutabilityTests
{
    [Fact]
    public void Build_Defensively_Copies_All_Nested_Input()
    {
        var identity = new ControlTokenIdentity("AtomUI", "Button");
        var controlBuilder = new ControlThemeConfigBuilder()
                             .WithAppearance(ThemeAppearance.Dark)
                             .WithDensity(DesignDensity.Compact)
                             .WithToken("ColorPrimary", "#1677FF");
        var configBuilder = new ThemeConfigBuilder()
                            .WithLanguage("AntDesign")
                            .WithAppearance(ThemeAppearance.Dark)
                            .WithToken("BorderRadius", "6")
                            .WithLanguageToken("Radius", "4")
                            .WithControl(identity, controlBuilder.Build());

        var config = configBuilder.Build();

        controlBuilder.WithToken("ColorPrimary", "#000000").WithAppearance(ThemeAppearance.Light);
        configBuilder.WithToken("BorderRadius", "99").WithLanguageToken("Radius", "8").WithAppearance(ThemeAppearance.Light);

        config.Language.ShouldBe("AntDesign");
        config.Appearance.ShouldBe(ThemeAppearance.Dark);
        config.Tokens["BorderRadius"].ShouldBe("6");
        config.LanguageTokens["Radius"].ShouldBe("4");
        config.Controls[identity].Appearance.ShouldBe(ThemeAppearance.Dark);
        config.Controls[identity].Density.ShouldBe(DesignDensity.Compact);
        config.Controls[identity].Tokens["ColorPrimary"].ShouldBe("#1677FF");
    }

    [Fact]
    public void Default_Config_Is_An_Immutable_Inheriting_Value()
    {
        var config = new ThemeConfigBuilder().Build();

        config.Inherit.ShouldBeTrue();
        config.Language.ShouldBeNull();
        config.Style.ShouldBeNull();
        config.Appearance.ShouldBeNull();
        config.Density.ShouldBeNull();
        config.Idiom.ShouldBeNull();
        config.Tokens.ShouldBeEmpty();
        config.LanguageTokens.ShouldBeEmpty();
        config.Controls.ShouldBeEmpty();
    }
}
